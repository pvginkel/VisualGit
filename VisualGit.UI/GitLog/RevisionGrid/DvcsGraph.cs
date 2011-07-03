// VisualGit.UI\GitLog\RevisionGrid\DvcsGraph.cs
//
// Copyright 2008-2011 The AnkhSVN Project
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//
// Changes and additions made for VisualGit Copyright 2011 Pieter van Ginkel.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.ComponentModel;

namespace VisualGit.UI.GitLog.RevisionGrid
{
    internal partial class DvcsGraph : DataGridView
    {
        #region DataType enum

        [Flags]
        public enum DataType
        {
            Normal = 0,
            Active = 1,
            Special = 2,
            Filtered = 4,
        }

        #endregion

        #region FilterType enum

        public enum FilterType
        {
            None,
            Highlight,
            Hide,
        }

        #endregion

        private const int NodeDimensions = 8;
        private const int LaneWidth = 13;
        private const int LaneLineWidth = 2;
        private const int MaxLanes = 30;
        protected const int RowHeight = 21;

        private readonly AutoResetEvent _backgroundEvent = new AutoResetEvent(false);
        private readonly Graph _graphData;
        private readonly Dictionary<Junction, int> _junctionColors = new Dictionary<Junction, int>();
        private readonly Color _nonRelativeColor = Color.LightGray;

        private static readonly Color[] _possibleColors =
            {
                Color.Red,
                Color.MistyRose,
                Color.Magenta,
                Color.Violet,
                Color.Blue,
                Color.Azure,
                Color.Cyan,
                Color.SpringGreen,
                Color.Green,
                Color.Chartreuse,
                Color.Gold,
                Color.Orange
            };

        private readonly SynchronizationContext _syncContext;
        private readonly List<string> _toBeSelected = new List<string>();
        private readonly object _syncLock = new object();
        private int _backgroundScrollTo;
        private Thread _backgroundThread;
        private int _cacheCount; // Number of elements in the cache.
        private int _cacheCountMax; // Number of elements allowed in the cache. Is based on control height.
        private int _cacheHead = -1; // The 'slot' that is the head of the circular bitmap
        private int _cacheHeadRow; // The node row that is in the head slot
        private FilterType _filterMode = FilterType.None;
        private Bitmap _graphBitmap;
        private int _graphDataCount;
        private Graphics _graphWorkArea;
        private int _rowHeight; // Height of elements in the cache. Is equal to the control's row height.
        private int _visibleBottom;
        private int _visibleTop;
        private bool _isDisposing;
        private Brush _selectionBrush;

        public void SetDimensions(Brush selectionBrush)
        {
            RowTemplate.Height = RowHeight;

            _selectionBrush = selectionBrush;

            _dataGrid_Resize(null, null);
        }

        public DvcsGraph()
        {
            RevisionGraphDrawNonRelativesGray = true;
            MulticolorBranches = true;
            GraphColor = Color.DarkRed;
            BranchBorders = true;
            StripedBranchChange = true;
            TagColor = Color.DarkBlue;
            BranchColor = Color.DarkRed;
            RemoteBranchColor = Color.Green;
            OtherTagColor = Color.Gray;
            ShowRemoteBranches = true;
            RelativeDate = true;

            _syncContext = SynchronizationContext.Current;
            _graphData = new Graph();

            _backgroundThread = new Thread(BackgroundThreadProc)
            {
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal,
                Name = "DvcsGraph.backgroundThread"
            };

            _backgroundThread.Start();

            InitializeComponent();

            ColumnHeadersDefaultCellStyle.Font = SystemFonts.MessageBoxFont;
            Font = SystemFonts.MessageBoxFont;
            DefaultCellStyle.Font = SystemFonts.MessageBoxFont;
            AlternatingRowsDefaultCellStyle.Font = SystemFonts.MessageBoxFont;
            RowsDefaultCellStyle.Font = SystemFonts.MessageBoxFont;
            RowHeadersDefaultCellStyle.Font = SystemFonts.MessageBoxFont;
            RowTemplate.DefaultCellStyle.Font = SystemFonts.MessageBoxFont;
            dataGridColumnGraph.DefaultCellStyle.Font = SystemFonts.MessageBoxFont;

            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            CellPainting += _dataGrid_CellPainting;
            ColumnWidthChanged += _dataGrid_ColumnWidthChanged;
            Scroll += _dataGrid_Scroll;
            _graphData.Updated += _graphData_Updated;

            VirtualMode = true;
            Clear();
        }

        [DefaultValue(true)]
        public bool RevisionGraphDrawNonRelativesGray { get; set; }

        [DefaultValue(true)]
        public bool MulticolorBranches { get; set; }

        [DefaultValue(typeof(Color), "DarkRed")]
        public Color GraphColor { get; set; }

        [DefaultValue(true)]
        public bool BranchBorders { get; set; }

        [DefaultValue(true)]
        public bool StripedBranchChange { get; set; }

        [DefaultValue(false)]
        public bool ShowAuthorDate { get; set; }

        [DefaultValue(false)]
        public bool RevisionGraphDrawNonRelativesTextGray { get; set; }

        [DefaultValue(typeof(Color), "DarkBlue")]
        public Color TagColor { get; set; }

        [DefaultValue(typeof(Color), "DarkRed")]
        public Color BranchColor { get; set; }

        [DefaultValue(typeof(Color), "Green")]
        public Color RemoteBranchColor { get; set; }

        [DefaultValue(typeof(Color), "Gray")]
        public Color OtherTagColor { get; set; }

        [DefaultValue(true)]
        public bool ShowRemoteBranches { get; set; }

        [DefaultValue(true)]
        public bool RelativeDate { get; set; }

        public FilterType FilterMode
        {
            get { return _filterMode; }
            set
            {
                // TODO: We only need to rebuild the graph if switching to or from hide
                if (_filterMode == value)
                {
                    return;
                }

                _syncContext.Send(o =>
                {
                    lock (_backgroundEvent) // Make sure the background thread isn't running
                    {
                        lock (_syncLock)
                        {
                            _backgroundScrollTo = 0;
                            _graphDataCount = 0;
                        }
                        lock (_graphData)
                        {
                            _filterMode = value;
                            _graphData.IsFilter = (_filterMode & FilterType.Hide) == FilterType.Hide;
                            RebuildGraph();
                        }
                    }
                }, this);
            }
        }

        public object[] SelectedData
        {
            get
            {
                if (SelectedRows.Count == 0)
                {
                    return null;
                }
                var data = new object[SelectedRows.Count];
                for (int i = 0; i < SelectedRows.Count; i++)
                {
                    data[i] = _graphData[SelectedRows[i].Index].Node.Data;
                }
                return data;
            }
        }

        public ICollection<string> SelectedIds
        {
            get
            {
                if (SelectedRows.Count == 0)
                    return null;

                return (
                    from selectedRow in SelectedRows.Cast<DataGridViewRow>()
                    where _graphData[selectedRow.Index] != null
                    select _graphData[selectedRow.Index].Node.Id
                ).ToArray();
            }
            set
            {
                lock (_graphData)
                {
                    ClearSelection();

                    CurrentCell = null;
                    
                    _toBeSelected.Clear();
                    
                    if (value == null)
                        return;

                    foreach (string rowItem in value)
                    {
                        int row = FindRow(rowItem);

                        if (row >= 0 && Rows.Count > row)
                        {
                            Rows[row].Selected = true;

                            if (CurrentCell == null)
                            {
                                // Set the current cell to the first item. We use cell
                                // 1 because cell 0 could be hidden if they've chosen to
                                // not see the graph

                                CurrentCell = Rows[row].Cells[1];
                            }
                        }
                        else
                        {
                            // Remember this node, and if we see it again, select it.

                            _toBeSelected.Add(rowItem);
                        }
                    }
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            _isDisposing = true;

            if (_backgroundThread != null)
            {
                _backgroundEvent.Set();
                _backgroundThread.Join();
                _backgroundThread = null;
            }

            if (disposing)
            {
                if (_graphBitmap != null)
                {
                    _graphBitmap.Dispose();
                    _graphBitmap = null;
                }
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Loading Handler. NOTE: This will often happen on a background thread
        /// so UI operations may not be safe!
        /// </summary>
        public event EventHandler<LoadingEventArgs> Loading;

        protected virtual void OnLoading(LoadingEventArgs e)
        {
            var ev = Loading;

            if (ev != null)
                ev(this, e);
        }

        public void Add(string aId, ICollection<string> aParentIds, DataType aType, object aData)
        {
            int lastItem = -1;
            lock (_graphData)
            {
                lastItem = _graphData.Count;
                _graphData.Add(aId, aParentIds, aType, aData);
            }

            UpdateData();
        }

        public void Clear()
        {
            lock (_syncLock)
            {
                _backgroundScrollTo = 0;
            }
            lock (_graphData)
            {
                SetRowCount(0);
                _junctionColors.Clear();
                _graphData.Clear();
                _graphDataCount = 0;
                RebuildGraph();
            }
            _filterMode = FilterType.None;
        }

        public void FilterClear()
        {
            lock (_graphData)
            {
                foreach (Node n in _graphData.Nodes.Values)
                {
                    n.IsFiltered = false;
                }
                _graphData.IsFilter = false;
            }
        }

        public void Filter(string aId)
        {
            lock (_graphData)
            {
                _graphData.Filter(aId);
            }
        }

        public bool RowIsRelative(int aRow)
        {
            lock (_graphData)
            {
                Graph.LaneRow row = _graphData[aRow];
                if (row == null)
                {
                    return false;
                }

                if (row.Node.Ancestors.Count > 0)
                    return row.Node.Ancestors[0].IsRelative;

                return true;
            }
        }

        public object GetRowData(int aRow)
        {
            lock (_graphData)
            {
                Graph.LaneRow row = _graphData[aRow];
                if (row == null)
                {
                    return null;
                }
                return row.Node.Data;
            }
        }

        public string GetRowId(int aRow)
        {
            lock (_graphData)
            {
                Graph.LaneRow row = _graphData[aRow];
                if (row == null)
                {
                    return null;
                }
                return row.Node.Id;
            }
        }

        public int FindRow(string aId)
        {
            lock (_graphData)
            {
                int i;
                for (i = 0; i < _graphData.CachedCount; i++)
                {
                    if (_graphData[i] != null && _graphData[i].Node.Id.CompareTo(aId) == 0)
                    {
                        break;
                    }
                }

                return i == _graphData.Count ? -1 : i;
            }
        }

        public bool Prune()
        {
            bool status;
            int count;
            lock (_graphData)
            {
                status = _graphData.Prune();
                count = _graphData.Count;
            }
            SetRowCount(count);
            return status;
        }

        private void RebuildGraph()
        {
            // Redraw
            _cacheHead = -1;
            _cacheHeadRow = 0;
            ClearDrawCache();
            UpdateData();
            Invalidate(true);
        }

        private void SetRowCount(int count)
        {
            if (InvokeRequired)
            {
                // DO NOT INVOKE! The RowCount is fixed at other strategic points in time.
                // -Doing this in synch can lock up the application
                // -Doing this asynch causes the scrollbar to flicker and eats performance
                // -At first I was concerned that returning might lead to some cases where 
                //  we have more items in the list than we're showing, but I'm pretty sure 
                //  when we're done processing we'll update with the final count, so the 
                //  problem will only be temporary, and not able to distinguish it from 
                //  just git giving us data slowly.
                //Invoke(new MethodInvoker(delegate { SetRowCount(count); }));
                return;
            }

            lock (_syncLock)
            {
                if (CurrentCell == null)
                {
                    RowCount = count;
                    CurrentCell = null;
                }
                else
                {
                    RowCount = count;
                }
            }
        }

        private void _graphData_Updated(object sender, EventArgs e)
        {
            // We have to post this since the thread owns a lock on GraphData that we'll
            // need in order to re-draw the graph.
            _syncContext.Post(o =>
            {
                ClearDrawCache();
                Invalidate();
            }, this);
        }

        private void _dataGrid_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (Rows[e.RowIndex].Height != RowTemplate.Height)
            {
                Rows[e.RowIndex].Height = RowTemplate.Height;
                _dataGrid_Scroll(null, null);
            }

            lock (_graphData)
            {
                Graph.LaneRow row = _graphData[e.RowIndex];
                if (row != null && (e.State & DataGridViewElementStates.Visible) != 0)
                {
                    if (e.ColumnIndex == 0)
                    {
                        if ((e.State & DataGridViewElementStates.Selected) == DataGridViewElementStates.Selected)
                            e.Graphics.FillRectangle(
                                _selectionBrush, e.CellBounds);
                        else
                            e.Graphics.FillRectangle(new SolidBrush(Color.White), e.CellBounds);

                        Rectangle srcRect = DrawGraph(e.RowIndex);
                        if (!srcRect.IsEmpty)
                        {
                            e.Graphics.DrawImage
                                (
                                    _graphBitmap,
                                    e.CellBounds,
                                    srcRect,
                                    GraphicsUnit.Pixel
                                );
                        }

                        e.Handled = true;
                    }
                }
            }
        }

        private void _dataGrid_ColumnWidthChanged(object sender, DataGridViewColumnEventArgs e)
        {
            ClearDrawCache();
        }

        private void _dataGrid_Scroll(object sender, ScrollEventArgs e)
        {
            UpdateData();
            UpdateColumnWidth();
        }

        private void BackgroundThreadProc()
        {
            while (_backgroundEvent.WaitOne())
            {
                if (_isDisposing)
                    return;

                lock (_backgroundEvent)
                {
                    int scrollTo;
                    lock (_syncLock)
                    {
                        scrollTo = _backgroundScrollTo;
                    }

                    int curCount;
                    lock (_graphData)
                    {
                        curCount = _graphDataCount;
                        _graphDataCount = _graphData.CachedCount;
                    }

                    while (curCount < scrollTo)
                    {
                        if (_isDisposing)
                            return;

                        lock (_graphData)
                        {
                            // Cache the next item
                            if (!_graphData.CacheTo(curCount))
                            {
                                Console.WriteLine("Cached item FAILED {0}", curCount);
                                lock (_syncLock)
                                {
                                    _backgroundScrollTo = curCount;
                                }
                                break;
                            }

                            // Update the row (if needed)
                            if (curCount < _visibleBottom || _toBeSelected.Count > 0)
                            {
                                _syncContext.Post(o => UpdateRow((int)o), curCount);
                            }

                            if (curCount ==
                                (FirstDisplayedCell == null ? 0 : FirstDisplayedCell.RowIndex + DisplayedRowCount(true)))
                            {
                                _syncContext.Post(state1 => UpdateColumnWidth(), null);
                            }

                            curCount = _graphData.CachedCount;
                            _graphDataCount = curCount;
                        }
                    }

                    lock (_syncLock)
                    {
                        int rowCount = RowCount;
                    }
                }
            }
        }

        private void UpdateData()
        {
            if (_isDisposing)
                return;

            lock (_syncLock)
            {
                _visibleTop = FirstDisplayedCell == null ? 0 : FirstDisplayedCell.RowIndex;
                _visibleBottom = _rowHeight > 0 ? _visibleTop + (Height / _rowHeight) : _visibleTop;

                //Subtract 2 for safe marge (1 for rounding and 1 for whitspace)....
                if (_visibleBottom - 2 > _graphData.Count)
                {
                    //Currently we are doing some important work; we are recieving
                    //rows that the user is viewing
                    SetBackgroundThreadToNormalPriority();
                    if (_graphData.Count > RowCount)// && _graphData.Count != RowCount)
                    {
                        OnLoading(new LoadingEventArgs(true));
                    }
                }
                else
                {
                    //All rows that the user is viewing are loaded. We now can hide the loading
                    //animation that is shown. (the event Loading(bool) triggers this!)
                    //Since the graph is not drawn for the visible graph yet, keep the
                    //priority on Normal. Lower it when the graph is visible.                            
                    OnLoading(new LoadingEventArgs(false));
                }

                if (_visibleBottom > _graphData.Count)
                {
                    _visibleBottom = _graphData.Count;
                }

                int targetBottom = _visibleBottom + 2000;
                targetBottom = Math.Min(targetBottom, _graphData.Count);
                if (_backgroundScrollTo < targetBottom)
                {
                    _backgroundScrollTo = targetBottom;
                    _backgroundEvent.Set();
                }
            }
        }

        private void SetBackgroundThreadToNormalPriority()
        {
            if (_backgroundThread != null)
                _backgroundThread.Priority = ThreadPriority.Normal;
        }

        private void SetBackgroundThreadToLowPriority()
        {
            if (_backgroundThread != null)
                _backgroundThread.Priority = ThreadPriority.BelowNormal;
        }

        private void UpdateRow(int row)
        {
            lock (_graphData)
            {
                if (RowCount < _graphData.Count)
                {
                    SetRowCount(_graphData.Count);
                }

                // Check to see if the newly added item should be selected
                if (_graphData.Count > row)
                {
                    string id = _graphData[row].Node.Id;
                    if (_toBeSelected.Contains(id))
                    {
                        _toBeSelected.Remove(id);
                        Rows[row].Selected = true;
                        if (CurrentCell == null)
                        {
                            // Set the current cell to the first item. We use cell
                            // 1 because cell 0 could be hidden if they've chosen to
                            // not see the graph
                            CurrentCell = Rows[row].Cells[1];
                        }
                    }
                }


                if (_visibleBottom < _graphDataCount)
                {
                    //All data for the current view is loaded! Lower the thread priority.
                    SetBackgroundThreadToLowPriority();
                }
                else
                {
                    //We need to draw the graph for the visible part of the grid. Higher the priority.
                    SetBackgroundThreadToNormalPriority();
                }

                try
                {
                    InvalidateRow(row);
                }
                catch (ArgumentOutOfRangeException)
                {
                    // Ignore. It is possible that RowCount gets changed before
                    // this is processed and the row is larger than RowCount.
                }
            }
        }

        private void UpdateColumnWidth()
        {
            lock (_graphData)
            {
                // Auto scale width on scroll
                if (dataGridColumnGraph.Visible)
                {
                    int laneCount = 2;
                    if (_graphData != null)
                    {
                        int width = 1;
                        int start = VerticalScrollBar.Value / _rowHeight;
                        int stop = start + DisplayedRowCount(true);
                        for (int i = start; i < stop && _graphData[i] != null; i++)
                        {
                            width = Math.Max(_graphData[i].Count, width);
                        }

                        laneCount = Math.Min(Math.Max(laneCount, width), MaxLanes);
                    }
                    if (dataGridColumnGraph.Width != LaneWidth * laneCount && LaneWidth * laneCount > dataGridColumnGraph.MinimumWidth)
                        dataGridColumnGraph.Width = LaneWidth * laneCount;
                }
            }
        }

        //Color of non-relative branches.

        private List<Color> GetJunctionColors(IEnumerable<Junction> aJunction)
        {
            List<Color> colors = new List<Color>();
            foreach (Junction j in aJunction)
            {
                colors.Add(GetJunctionColor(j));
            }

            if (colors.Count == 0)
            {
                colors.Add(Color.Black);
            }

            return colors;
        }

        // http://en.wikipedia.org/wiki/File:RBG_color_wheel.svg

        private Color GetJunctionColor(Junction aJunction)
        {
            //Draw non-relative branches gray
            if (!aJunction.IsRelative && RevisionGraphDrawNonRelativesGray)
                return _nonRelativeColor;

            if (!MulticolorBranches)
                return GraphColor;

            // This is the order to grab the colors in.
            int[] preferedColors = { 4, 8, 6, 10, 2, 5, 7, 3, 9, 1, 11 };

            int colorIndex;
            if (_junctionColors.TryGetValue(aJunction, out colorIndex))
            {
                return _possibleColors[colorIndex];
            }


            // Get adjacent junctions
            var adjacentJunctions = new List<Junction>();
            var adjacentColors = new List<int>();
            adjacentJunctions.AddRange(aJunction.Child.Ancestors);
            adjacentJunctions.AddRange(aJunction.Child.Descendants);
            adjacentJunctions.AddRange(aJunction.Parent.Ancestors);
            adjacentJunctions.AddRange(aJunction.Parent.Descendants);
            foreach (Junction peer in adjacentJunctions)
            {
                if (_junctionColors.TryGetValue(peer, out colorIndex))
                {
                    adjacentColors.Add(colorIndex);
                }
                else
                {
                    colorIndex = -1;
                }
            }

            if (adjacentColors.Count == 0) //This is an end-point. We need to 'pick' a new color
            {
                colorIndex = 0;
            }
            else //This is a parent branch, calculate new color based on parent branch
            {
                int start = adjacentColors[0];
                int i;
                for (i = 0; i < preferedColors.Length; i++)
                {
                    colorIndex = (start + preferedColors[i]) % _possibleColors.Length;
                    if (!adjacentColors.Contains(colorIndex))
                    {
                        break;
                    }
                }
                if (i == preferedColors.Length)
                {
                    var r = new Random();
                    colorIndex = r.Next(preferedColors.Length);
                }
            }

            _junctionColors[aJunction] = colorIndex;
            return _possibleColors[colorIndex];
        }

        public override void Refresh()
        {
            ClearDrawCache();
            base.Refresh();
        }

        private void ClearDrawCache()
        {
            _cacheHead = 0;
            _cacheCount = 0;
        }

        private Rectangle DrawGraph(int aNeededRow)
        {
            lock (_graphData)
            {
                if (aNeededRow < 0 || _graphData.Count == 0 || _graphData.Count <= aNeededRow)
                {
                    return Rectangle.Empty;
                }

                #region Make sure the graph cache bitmap is setup

                int height = _cacheCountMax * _rowHeight;
                int width = dataGridColumnGraph.Width;
                if (_graphBitmap == null ||
                    //Resize the bitmap when the with or height is changed. The height won't change very often.
                    //The with changes more often, when branches become visible/invisible.
                    //Try to be 'smart' and not resize the bitmap for each little change. Enlarge when needed
                    //but never shrink the bitmap since the huge performance hit is worse than the little extra memory.
                    _graphBitmap.Width < width || _graphBitmap.Height != height)
                {
                    if (_graphBitmap != null)
                    {
                        _graphBitmap.Dispose();
                        _graphBitmap = null;
                    }
                    if (width > 0 && height > 0)
                    {
                        _graphBitmap = new Bitmap(Math.Max(width, LaneWidth * 3), height, PixelFormat.Format32bppPArgb);
                        _graphWorkArea = Graphics.FromImage(_graphBitmap);
                        _graphWorkArea.SmoothingMode = SmoothingMode.AntiAlias;
                        _cacheHead = 0;
                        _cacheCount = 0;
                    }
                    else
                    {
                        return Rectangle.Empty;
                    }
                }

                #endregion

                // Compute how much the head needs to move to show the requested item. 
                int neededHeadAdjustment = aNeededRow - _cacheHead;
                if (neededHeadAdjustment > 0)
                {
                    neededHeadAdjustment -= _cacheCountMax - 1;
                    if (neededHeadAdjustment < 0)
                    {
                        neededHeadAdjustment = 0;
                    }
                }
                int newRows = 0;
                if (_cacheCount < _cacheCountMax)
                {
                    newRows = (aNeededRow - _cacheCount) + 1;
                }

                // Adjust the head of the cache
                _cacheHead = _cacheHead + neededHeadAdjustment;
                _cacheHeadRow = (_cacheHeadRow + neededHeadAdjustment) % _cacheCountMax;
                if (_cacheHeadRow < 0)
                {
                    _cacheHeadRow = _cacheCountMax + _cacheHeadRow;
                }

                int start;
                int end;
                if (newRows > 0)
                {
                    start = _cacheHead + _cacheCount;
                    _cacheCount = Math.Min(_cacheCount + newRows, _cacheCountMax);
                    end = _cacheHead + _cacheCount;
                }
                else if (neededHeadAdjustment > 0)
                {
                    end = _cacheHead + _cacheCount;
                    start = Math.Max(_cacheHead, end - neededHeadAdjustment);
                }
                else if (neededHeadAdjustment < 0)
                {
                    start = _cacheHead;
                    end = start + Math.Min(_cacheCountMax, -neededHeadAdjustment);
                }
                else
                {
                    // Item already in the cache
                    return CreateRectangle(aNeededRow, width);
                }


                for (int rowIndex = start; rowIndex < end; rowIndex++)
                {
                    Graph.LaneRow row = _graphData[rowIndex];
                    if (row == null)
                    {
                        // This shouldn't be happening...If it does, clear the cache so we
                        // eventually pick it up.
                        Console.WriteLine("Draw lane {0} {1}", rowIndex, "NO DATA");
                        ClearDrawCache();
                        return Rectangle.Empty;
                    }

                    Region oldClip = _graphWorkArea.Clip;

                    // Get the x,y value of the current item's upper left in the cache
                    int curCacheRow = (_cacheHeadRow + rowIndex - _cacheHead) % _cacheCountMax;
                    int x = 0;
                    int y = curCacheRow * _rowHeight;

                    var laneRect = new Rectangle(0, y, Width, _rowHeight);
                    if (rowIndex == start || curCacheRow == 0)
                    {
                        // Draw previous row first. Clip top to row. We also need to clear the area
                        // before we draw since nothing else would clear the top 1/2 of the item to draw.
                        _graphWorkArea.RenderingOrigin = new Point(x, y - _rowHeight);
                        var newClip = new Region(laneRect);
                        _graphWorkArea.Clip = newClip;
                        _graphWorkArea.Clear(Color.Transparent);
                        DrawItem(_graphWorkArea, _graphData[rowIndex - 1]);
                        _graphWorkArea.Clip = oldClip;
                    }

                    bool isLast = (rowIndex == end - 1);
                    if (isLast)
                    {
                        var newClip = new Region(laneRect);
                        _graphWorkArea.Clip = newClip;
                    }

                    _graphWorkArea.RenderingOrigin = new Point(x, y);
                    bool success = DrawItem(_graphWorkArea, row);

                    _graphWorkArea.Clip = oldClip;

                    if (!success)
                    {
                        ClearDrawCache();
                        return Rectangle.Empty;
                    }
                }

                return CreateRectangle(aNeededRow, width);
            } // end lock
        }

        private Rectangle CreateRectangle(int aNeededRow, int width)
        {
            return new Rectangle
                (
                0,
                (_cacheHeadRow + aNeededRow - _cacheHead) % _cacheCountMax * RowTemplate.Height,
                width,
                _rowHeight
                );
        }

        // end DrawGraph

        private bool DrawItem(Graphics wa, Graph.LaneRow row)
        {
            if (row == null || row.NodeLane == -1)
            {
                return false;
            }

            // Clip to the area we're drawing in, but draw 1 pixel past so
            // that the top/bottom of the line segment's anti-aliasing isn't
            // visible in the final rendering.
            int top = wa.RenderingOrigin.Y + _rowHeight / 2;
            var laneRect = new Rectangle(0, top, Width, _rowHeight);
            Region oldClip = wa.Clip;
            var newClip = new Region(laneRect);
            newClip.Intersect(oldClip);
            wa.Clip = newClip;
            wa.Clear(Color.Transparent);

            for (int r = 0; r < 2; r++)
                for (int lane = 0; lane < row.Count; lane++)
                {
                    int mid = wa.RenderingOrigin.X + (int)((lane + 0.5) * LaneWidth);

                    for (int item = 0; item < row.LaneInfoCount(lane); item++)
                    {
                        Graph.LaneInfo laneInfo = row[lane, item];

                        //Draw all non-relative items first, them draw
                        //all relative items on top
                        if (laneInfo.Junctions.FirstOrDefault() != null)
                            if (laneInfo.Junctions.First().IsRelative == (r == 0))
                                continue;

                        List<Color> curColors = GetJunctionColors(laneInfo.Junctions);

                        // Create the brush for drawing the line
                        Brush brushLineColor;
                        bool drawBorder = BranchBorders; //hide border for "non-relatives"
                        if (curColors.Count == 1 || !StripedBranchChange)
                        {
                            if (curColors[0] != _nonRelativeColor)
                            {
                                brushLineColor = new SolidBrush(curColors[0]);
                            }
                            else if (curColors.Count > 1 && curColors[1] != _nonRelativeColor)
                            {
                                brushLineColor = new SolidBrush(curColors[1]);
                            }
                            else
                            {
                                drawBorder = false;
                                brushLineColor = new SolidBrush(_nonRelativeColor);
                            }
                        }
                        else
                        {
                            brushLineColor = new HatchBrush(HatchStyle.DarkDownwardDiagonal, curColors[0], curColors[1]);
                            if (curColors[0] == _nonRelativeColor && curColors[1] == _nonRelativeColor) drawBorder = false;
                        }

                        for (int i = drawBorder ? 0 : 2; i < 3; i++)
                        {
                            Pen penLine;
                            if (i == 0)
                            {
                                penLine = new Pen(new SolidBrush(Color.White), LaneLineWidth + 2);
                            }
                            else if (i == 1)
                            {
                                penLine = new Pen(new SolidBrush(Color.Black), LaneLineWidth + 1);
                            }
                            else
                            {
                                penLine = new Pen(brushLineColor, LaneLineWidth);
                            }

                            if (laneInfo.ConnectLane == lane)
                            {
                                wa.DrawLine
                                    (
                                        penLine,
                                        new Point(mid, top - 1),
                                        new Point(mid, top + _rowHeight + 2)
                                    );
                            }
                            else
                            {
                                wa.DrawBezier
                                    (
                                        penLine,
                                        new Point(mid, top - 1),
                                        new Point(mid, top + _rowHeight + 2),
                                        new Point(mid + (laneInfo.ConnectLane - lane) * LaneWidth, top - 1),
                                        new Point(mid + (laneInfo.ConnectLane - lane) * LaneWidth, top + _rowHeight + 2)
                                    );
                            }
                        }
                    }
                }

            // Reset the clip region
            wa.Clip = oldClip;
            {
                // Draw node
                var nodeRect = new Rectangle
                    (
                    wa.RenderingOrigin.X + (LaneWidth - NodeDimensions) / 2 + row.NodeLane * LaneWidth,
                    wa.RenderingOrigin.Y + (_rowHeight - NodeDimensions) / 2,
                    NodeDimensions,
                    NodeDimensions
                    );

                Brush nodeBrush;
                bool drawBorder = BranchBorders;
                List<Color> nodeColors = GetJunctionColors(row.Node.Ancestors);
                if (nodeColors.Count == 1)
                {
                    nodeBrush = new SolidBrush(nodeColors[0]);
                    if (nodeColors[0] == _nonRelativeColor) drawBorder = false;
                }
                else
                {
                    nodeBrush = new LinearGradientBrush(nodeRect, nodeColors[0], nodeColors[1],
                                                        LinearGradientMode.Horizontal);
                    if (nodeColors[0] == _nonRelativeColor && nodeColors[1] == Color.LightGray) drawBorder = false;
                }

                if (_filterMode == FilterType.Highlight && row.Node.IsFiltered)
                {
                    Rectangle highlightRect = nodeRect;
                    highlightRect.Inflate(2, 3);
                    wa.FillRectangle(Brushes.Yellow, highlightRect);
                    wa.DrawRectangle(new Pen(Brushes.Black), highlightRect);
                }

                if (row.Node.Data == null)
                {
                    wa.FillEllipse(Brushes.White, nodeRect);
                    wa.DrawEllipse(new Pen(Color.Red, 2), nodeRect);
                }
                else if (row.Node.IsActive)
                {
                    wa.FillRectangle(nodeBrush, nodeRect);
                    nodeRect.Inflate(1, 1);
                    wa.DrawRectangle(new Pen(Color.Black, 3), nodeRect);
                }
                else if (row.Node.IsSpecial)
                {
                    wa.FillRectangle(nodeBrush, nodeRect);
                    if (drawBorder)
                    {
                        wa.DrawRectangle(new Pen(Color.Black, 1), nodeRect);
                    }
                }
                else
                {
                    wa.FillEllipse(nodeBrush, nodeRect);
                    if (drawBorder)
                    {
                        wa.DrawEllipse(new Pen(Color.Black, 1), nodeRect);
                    }
                }
            }
            return true;
        }

        private void _dataGrid_Resize(object sender, EventArgs e)
        {
            _rowHeight = RowTemplate.Height;
            // Keep an extra page in the cache
            _cacheCountMax = Height * 2 / _rowHeight + 1;
            ClearDrawCache();
            _dataGrid_Scroll(null, null);
        }
    }

    // end of class DvcsGraph
}
