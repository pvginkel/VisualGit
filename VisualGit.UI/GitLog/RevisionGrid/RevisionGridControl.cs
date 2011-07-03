// VisualGit.UI\GitLog\RevisionGrid\RevisionGridControl.cs
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
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using SharpGit;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Threading;
using System.Diagnostics;

namespace VisualGit.UI.GitLog.RevisionGrid
{
    internal partial class RevisionGridControl : DvcsGraph
    {
        private static readonly Font NormalFont = SystemFonts.MessageBoxFont;
        private static readonly Font RefsFont = new Font(SystemFonts.MessageBoxFont, FontStyle.Bold);
        private static readonly Font HeadFont = new Font(SystemFonts.MessageBoxFont, FontStyle.Bold);
        private static readonly GitHead[] EmptyHeads = new GitHead[0];

        private int _lastScrollPos;
        private bool _initialLoad;
        private string _initialSelectedRevision;
        private LinearGradientBrush _selectedItemBrush;
        private int _lastRow;
        private GitLogArgs _args;
        private BusyOverlay _busyOverlay;
        private int _revisionsLoaded;
        private Stopwatch _revisionLoadStart;
        private TimeSpan _revisionNextUpdate;

        public ICollection<string> LastSelectedRows { get; private set; }
        public string CurrentCheckout { get; private set; }
        public GitClient Client { get; set; }
        public IList<string> Paths { get; set; }
        public string RepositoryPath { get; set; }

        public GitLogArgs Args
        {
            get { return _args; }
            set
            {
                if (_args != null)
                    _args.Log -= Args_Log;

                _args = value;

                if (_args != null)
                    _args.Log += Args_Log;
            }
        }

        public RevisionGridControl()
        {
            InitializeComponent();

            Client = new GitClient();

            ColumnHeadersVisible = false;

            Disposed += new EventHandler(RevisionGridControl_Disposed);
        }

        void RevisionGridControl_Disposed(object sender, EventArgs e)
        {
            if (_selectedItemBrush != null)
            {
                _selectedItemBrush.Dispose();
                _selectedItemBrush = null;
            }
        }

        public void BeginRefresh()
        {
            if (Client == null || Paths == null || RepositoryPath == null || _args == null)
                throw new InvalidOperationException("Both Client, Paths, RepositoryPath as Args must be set in order to retrieve revisions");

            try
            {
                _initialLoad = true;

                _lastScrollPos = FirstDisplayedScrollingRowIndex;

                var newCurrentCheckout = GetCurrentCheckout();

                // If the current checkout changed, don't get the currently selected rows, select the
                // new current checkout instead.

                if (String.Equals(newCurrentCheckout, CurrentCheckout, StringComparison.OrdinalIgnoreCase))
                    LastSelectedRows = SelectedIds;

                ClearSelection();

                CurrentCheckout = newCurrentCheckout;

                Clear();

                base.Refresh();

                UpdateContents(false);

                RetrieveRevisions();

                LoadRevisions();

                SetRevisionsLayout();

                ShowBusyIndicator();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RetrieveRevisions()
        {
            _revisionsLoaded = 0;
            _revisionNextUpdate = TimeSpan.FromSeconds(3);

            ThreadPool.QueueUserWorkItem(p =>
            {
                _args.ThrowOnCancel = false;

                Client.Log(Paths, _args);

                BeginInvoke(new Action<bool>(UpdateContents), true);

                HideBusyIndicator();
            });
        }

        private void UpdateContents(bool last)
        {
            Prune();

            if (_busyOverlay != null)
            {
                _busyOverlay.ScrollBarsVisible =
                    (HorizontalScrollBar.Visible ? ScrollBars.Horizontal : 0) |
                    (VerticalScrollBar.Visible ? ScrollBars.Vertical : 0);
            }

            if (last)
                OnBatchDone(new BatchFinishedEventArgs(RowCount));
        }

        public event EventHandler<BatchFinishedEventArgs> BatchDone;

        protected virtual void OnBatchDone(BatchFinishedEventArgs e)
        {
            var ev = BatchDone;

            if (ev != null)
                ev(this, e);
        }

        void Args_Log(object sender, GitLogEventArgs e)
        {
            if (IsDisposed)
            {
                e.Cancel = true;
                return;
            }

            GitHead[] heads;

            if (e.Refs == null)
                heads = EmptyHeads;
            else
                heads = e.Refs.Where(p => p.Name != GitConstants.Head).Select(p => new GitHead(p.Revision, p.Name)).ToArray();

            UpdateGraph(new GitRevision
            {
                Author = e.AuthorName,
                AuthorDate = e.AuthorTime,
                AuthorEmail = e.AuthorEmail,
                CommitDate = e.Time,
                Revision = e.Revision,
                LogMessage = e.LogMessage,
                ParentRevisions = e.ParentRevisions,
                Heads = heads,
                Index = _revisionsLoaded++,
                RepositoryRoot = RepositoryPath
            });

            // Force an early refresh to get some content into the grid.

            if (_revisionsLoaded == 100)
            {
                BeginInvoke(new Action<bool>(UpdateContents), false);

                _revisionLoadStart = new Stopwatch();
                _revisionLoadStart.Start();
            }

            // Force a refresh every three seconds, incremented by one second
            // on every refresh.

            if (_revisionLoadStart != null)
            {
                if (_revisionLoadStart.Elapsed > _revisionNextUpdate)
                {
                    _revisionLoadStart.Restart();

                    _revisionNextUpdate += TimeSpan.FromSeconds(1);

                    BeginInvoke(new Action<bool>(UpdateContents), false);
                }
            }
        }

        private void LoadRevisions()
        {
            SuspendLayout();

            Columns[1].HeaderText = Properties.Resources.MessageCaption;
            Columns[2].HeaderText = Properties.Resources.AuthorCaption;
            Columns[3].HeaderText =
                ShowAuthorDate
                ? Properties.Resources.AuthorDateCaption
                : Properties.Resources.CommitDateCaption;

            if (LastSelectedRows != null)
            {
                SelectedIds = LastSelectedRows;
                LastSelectedRows = null;
            }
            else if (_initialSelectedRevision == null)
            {
                SelectedIds = new[] { CurrentCheckout };
            }

            if (_lastScrollPos > 0 && RowCount > _lastScrollPos)
            {
                FirstDisplayedScrollingRowIndex = _lastScrollPos;
                _lastScrollPos = -1;
            }

            ResumeLayout();

            if (!_initialLoad)
                return;

            _initialLoad = false;
        }

        private void SetRevisionsLayout()
        {
            _selectedItemBrush = new LinearGradientBrush(
                new Rectangle(0, 0, RowHeight, RowHeight),
                RowTemplate.DefaultCellStyle.SelectionBackColor,
                Color.LightBlue, 90, false
            );

            SetDimensions(_selectedItemBrush);
        }

        private string GetCurrentCheckout()
        {
            return Client.ResolveReference(RepositoryPath, new SharpGit.GitRevision("HEAD"));
        }

        public void SetInitialRevision(GitRevision initialSelectedRevision)
        {
            _initialSelectedRevision = initialSelectedRevision != null ? initialSelectedRevision.Revision : null;
        }

        private void RevisionsMouseClick(object sender, MouseEventArgs e)
        {
            var pt = PointToClient(Cursor.Position);
            var hti = HitTest(pt.X, pt.Y);

            _lastRow = hti.RowIndex;
        }

        private void RevisionsCellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            var pt = PointToClient(Cursor.Position);
            var hti = HitTest(pt.X, pt.Y);

            _lastRow = hti.RowIndex;

            ClearSelection();

            if (_lastRow >= 0 && Rows.Count > _lastRow)
                Rows[_lastRow].Selected = true;
        }

        private void UpdateGraph(GitRevision rev)
        {
            if (rev == null)
                throw new ArgumentNullException("rev");

            DvcsGraph.DataType dataType;

            if (String.Equals(rev.Revision, CurrentCheckout, StringComparison.OrdinalIgnoreCase))
                dataType = DvcsGraph.DataType.Active;
            else if (rev.Heads.Count > 0)
                dataType = DvcsGraph.DataType.Special;
            else
                dataType = DvcsGraph.DataType.Normal;

            Add(rev.Revision, rev.ParentRevisions, dataType, rev);
        }

        protected override void OnCellPainting(DataGridViewCellPaintingEventArgs e)
        {
            base.OnCellPainting(e);

            // The graph column is handled by the DvcsGraph

            if (e.ColumnIndex == 0)
                return;

            var column = e.ColumnIndex;

            if (e.RowIndex < 0 || !e.State.HasFlag(DataGridViewElementStates.Visible))
                return;

            if (RowCount <= e.RowIndex)
                return;

            var revision = GetRevision(e.RowIndex);

            if (revision == null)
                return;

            e.Handled = true;

            if (e.State.HasFlag(DataGridViewElementStates.Selected))
                e.Graphics.FillRectangle(_selectedItemBrush, e.CellBounds);
            else
                e.Graphics.FillRectangle(new SolidBrush(Color.White), e.CellBounds);

            Color foreColor;

            if (
                !RevisionGraphDrawNonRelativesGray ||
                !RevisionGraphDrawNonRelativesTextGray ||
                RowIsRelative(e.RowIndex)
            )
                foreColor = e.CellStyle.ForeColor;
            else
                foreColor = Color.LightGray;

            var rowFont =
                String.Equals(revision.Revision, CurrentCheckout, StringComparison.OrdinalIgnoreCase)
                ? HeadFont
                : NormalFont;

            var lineRun = new LineRun();

            switch (column)
            {
                case 1: //Description!!
                    if (revision.Heads.Count > 0)
                    {
                        foreach (var head in revision.Heads.OrderBy(p => p))
                        {
                            if (head.IsRemote && !ShowRemoteBranches)
                                continue;

                            var item = new LineRunItem();

                            if (head.IsTag)
                                item.Color = TagColor;
                            else if (head.IsHead)
                                item.Color = BranchColor;
                            else if (head.IsRemote)
                                item.Color = RemoteBranchColor;
                            else
                                item.Color = OtherTagColor;

                            item.Text = "[" + head.Name + "]";
                            item.Font = RefsFont;

                            lineRun.Items.Add(item);
                        }
                    }

                    lineRun.Items.Add(new LineRunItem
                    {
                        Text = revision.LogMessage,
                        Font = rowFont,
                        Color = foreColor
                    });
                    break;

                case 2:
                    lineRun.Items.Add(new LineRunItem
                    {
                        Text = revision.AuthorEmail,
                        Font = rowFont,
                        Color = foreColor
                    });
                    break;

                case 3:
                    lineRun.Items.Add(new LineRunItem
                    {
                        Text = TimeToString(ShowAuthorDate ? revision.AuthorDate : revision.CommitDate),
                        Font = rowFont,
                        Color = foreColor
                    });
                    break;
            }

            lineRun.Draw(
                e.Graphics,
                new Rectangle(e.CellBounds.Left, e.CellBounds.Top + 4, e.CellBounds.Width, e.CellBounds.Height - 4)
            );
        }

        public GitRevision GetRevision(int aRow)
        {
            return GetRowData(aRow) as GitRevision;
        }

        private string TimeToString(DateTime time)
        {
            if (time == DateTime.MinValue || time == DateTime.MaxValue)
                return "";

            if (!RelativeDate)
                return String.Format("{0} {1}", time.ToShortDateString(), time.ToLongTimeString());

            var span = DateTime.Now - time;

            if (span.Minutes < 0)
                return String.Format(Properties.Resources.XSecondsAgo, span.Seconds);

            if (span.TotalHours < 1)
                return String.Format(Properties.Resources.XMinutesAgo, span.Minutes + Math.Round(span.Seconds / 60.0, 0));

            if (span.TotalHours + Math.Round(span.Minutes / 60.0, 0) < 2)
                return String.Format(Properties.Resources.XHourAgo, (int)span.TotalHours + Math.Round(span.Minutes / 60.0, 0));

            if (span.TotalHours < 24)
                return String.Format(Properties.Resources.XHoursAgo, (int)span.TotalHours + Math.Round(span.Minutes / 60.0, 0));

            if (span.TotalDays + Math.Round(span.Hours / 24.0, 0) < 2)
                return String.Format(Properties.Resources.XDayAgo, (int)span.TotalDays + Math.Round(span.Hours / 24.0, 0));

            if (span.TotalDays < 30)
                return String.Format(Properties.Resources.XDaysAgo, (int)span.TotalDays + Math.Round(span.Hours / 24.0, 0));

            if (span.TotalDays < 45)
                return String.Format(Properties.Resources.XMonthAgo, "1");

            if (span.TotalDays < 365)
                return String.Format(Properties.Resources.XMonthsAgo, (int)Math.Round(span.TotalDays / 30, 0));

            return String.Format(Properties.Resources.XYearsAgo, String.Format("{0:#.#} ", Math.Round(span.TotalDays / 365)));
        }

        private void ShowBusyIndicator()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(ShowBusyIndicator));
                return;
            }

            if (_busyOverlay == null)
            {
                _busyOverlay = new BusyOverlay(this, AnchorStyles.Bottom | AnchorStyles.Right);
                _busyOverlay.ScrollBarsVisible =
                    (HorizontalScrollBar.Visible ? ScrollBars.Horizontal : 0) |
                    (VerticalScrollBar.Visible ? ScrollBars.Vertical : 0);
            }

            _busyOverlay.Show();
        }

        private void HideBusyIndicator()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(HideBusyIndicator));
                return;
            }

            if (_busyOverlay != null)
                _busyOverlay.Hide();
        }
    }
}
