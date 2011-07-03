// VisualGit.UI\GitLog\RevisionGrid\DvcsGraph_Lanes.cs
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
using System.Text;
using System.Collections;
using System.Diagnostics;

namespace VisualGit.UI.GitLog.RevisionGrid
{
    partial class DvcsGraph
    {
        #region Nested type: Lanes

        private class Lanes : IEnumerable<Graph.LaneRow>
        {
            private readonly ActiveLaneRow _currentRow = new ActiveLaneRow();
            private readonly List<LaneJunctionDetail> _laneNodes = new List<LaneJunctionDetail>();
            private readonly List<Graph.LaneRow> _laneRows;
            private readonly Graph _sourceGraph;

            public Lanes(Graph aGraph)
            {
                _sourceGraph = aGraph;
                // Rebuild lanes
                _laneRows = new List<Graph.LaneRow>();
            }

            public Graph.LaneRow this[int row]
            {
                get
                {
                    if (row < 0)
                    {
                        return null;
                    }

                    if (row < _laneRows.Count)
                    {
                        return _laneRows[row];
                    }

                    if (row < _sourceGraph.AddedNodes.Count)
                    {
                        return new SavedLaneRow(_sourceGraph.AddedNodes[row]);
                    }

                    return null;
                }
            }

            public int Count
            {
                get { return _sourceGraph.Count; }
            }

            public int CachedCount
            {
                get { return _laneRows.Count; }
            }

            #region IEnumerable<LaneRow> Members

            public IEnumerator<Graph.LaneRow> GetEnumerator()
            {
                return new LaneEnumerator(this);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            #endregion

            public void Clear()
            {
                _laneRows.Clear();
                _laneNodes.Clear();
                _currentRow.Clear();

                foreach (Node aNode in _sourceGraph.GetHeads())
                {
                    if (aNode.Descendants.Count == 0)
                    {
                        // This node is a head, create a new lane for it
                        Node h = aNode;
                        if (h.Ancestors.Count == 0)
                        {
                            // This is a single entry with no parents or children.
                            var detail = new LaneJunctionDetail(h);
                            _laneNodes.Add(detail);
                        }
                        else
                        {
                            foreach (Junction j in h.Ancestors)
                            {
                                var detail = new LaneJunctionDetail(j);
                                _laneNodes.Add(detail);
                            }
                        }
                    }
                }
            }

            public bool CacheTo(int row)
            {
                bool isValid = true;
                while (isValid && row >= CachedCount)
                {
                    isValid = MoveNext();
                }

                return isValid;
            }

            public void Update(Node aNode)
            {
                if (aNode.Descendants.Count == 0)
                {
                    // This node is a head, create a new lane for it
                    Node h = aNode;
                    if (h.Ancestors.Count == 0)
                    {
                        // This is a single entry with no parents or children.
                        var detail = new LaneJunctionDetail(h);
                        _laneNodes.Add(detail);
                    }
                    else
                    {
                        foreach (Junction j in h.Ancestors)
                        {
                            var detail = new LaneJunctionDetail(j);
                            _laneNodes.Add(detail);
                        }
                    }
                }
            }

            private bool MoveNext()
            {
                // If there are no lanes, there is nothing more to draw
                if (_laneNodes.Count == 0 || _sourceGraph.Count <= _laneRows.Count)
                {
                    return false;
                }

                // Find the new current row's node (newest item in the row)

                #region Find current node & index

                _currentRow.Node = null;
                for (int curLane = 0; curLane < _laneNodes.Count; curLane++)
                {
                    LaneJunctionDetail lane = _laneNodes[curLane];
                    if (lane.Count == 0)
                    {
                        continue;
                    }

                    // NOTE: We could also compare with _sourceGraph _sourceGraph.AddedNodes[_sourceGraph.processedNodes],
                    // since it should always be the same value
                    if (_currentRow.Node == null ||
                        _currentRow.Node.Data == null ||
                        (lane.Current.Data != null && lane.Current.Index < _currentRow.Node.Index))
                    {
                        _currentRow.Node = lane.Current;
                        _currentRow.NodeLane = curLane;
                    }
                }
                if (_currentRow.Node == null)
                {
                    // DEBUG: The check above didn't find anything, but should have
                    if (Debugger.IsAttached) Debugger.Break();
                    //Node[] topo = _sourceGraph.TopoSortedNodes();
                    return false;
                }

                // If this row doesn't contain data, we're to the end of the valid entries.
                if (_currentRow.Node.Data == null)
                {
                    return false;
                }

                _sourceGraph.ProcessNode(_currentRow.Node);

                #endregion

                // Check for multiple junctions with this node at the top. Remove the 
                // node from that junction as well. This will happen when there is a branch 

                #region Check for branches

                _currentRow.Clear(_currentRow.NodeLane);
                for (int curLane = 0; curLane < _laneNodes.Count; curLane++)
                {
                    LaneJunctionDetail lane = _laneNodes[curLane];
                    if (lane.Count == 0)
                    {
                        continue;
                    }

                    if (_currentRow.Node != lane.Current)
                    {
                        // We're only interested in columns that have the same node
                        // at the top of the junction as the current row's node
                        continue;
                    }

                    // Remove the item from the lane, since it is being drawn now.
                    // We need to draw the graph line for this lane. If there are no items 
                    // left in the lane we don't draw it.
                    int intoLane = AdvanceLane(curLane);
                    if (intoLane < curLane)
                    {
                        // AdvanceLane could have removed lanes so we need to start from
                        // the merged into lane (otherwise we could skip a lane, causing
                        // us to try to insert a node into the graph twice)
                        curLane = intoLane;
                    }

                    // Re-process the lane to make sure there are no actions left.
                    curLane--;
                }

                #endregion

                // Look for lanes that cross and reorder to straighten them out if possible,
                // and keep the lanes that merge next to each other.

                #region Straighten out lanes

                // Look for crossing lanes
                for (int lane = 0; lane < _currentRow.Count; lane++)
                {
                    for (int item = 0; item < _currentRow.LaneInfoCount(lane); item++)
                    {
                        Graph.LaneInfo laneInfo = _currentRow[lane, item];
                        if (laneInfo.ConnectLane <= lane)
                        {
                            continue;
                        }
                        // Lane is moving to the right, check to see if it intersects
                        // with any lanes moving to the left.
                        for (int otherLane = lane + 1; otherLane <= laneInfo.ConnectLane; otherLane++)
                        {
                            if (_currentRow.LaneInfoCount(otherLane) != 1)
                            {
                                continue;
                            }
                            Graph.LaneInfo otherLaneInfo = _currentRow[otherLane, 0];
                            if (otherLaneInfo.ConnectLane < otherLane)
                            {
                                _currentRow.Swap(otherLaneInfo.ConnectLane, otherLane);
                                LaneJunctionDetail temp = _laneNodes[otherLane];
                                _laneNodes[otherLane] = _laneNodes[otherLaneInfo.ConnectLane];
                                _laneNodes[otherLaneInfo.ConnectLane] = temp;
                            }
                        }
                    }
                }

                //// Keep the merge lanes next to each other
                //int mergeFromCount = _currentRow.LaneInfoCount(_currentRow.NodeLane);
                //if (mergeFromCount > 1)
                //{
                //    for (int i = 0; i < mergeFromCount; i++)
                //    {
                //        Graph.LaneInfo laneInfo = _currentRow[_currentRow.NodeLane, i];
                //        // Check to see if the lane is currently next to us
                //        if (laneInfo.ConnectLane - _currentRow.NodeLane > mergeFromCount)
                //        {
                //            // Only move the lane if it isn't already being drawn.
                //            if (_currentRow.LaneInfoCount(laneInfo.ConnectLane) == 0)
                //            {
                //                // Remove the row laneInfo.ConnectLane and insert
                //                // it at _currentRow.NodeLane+1. 
                //                // Then start over searching for others if i != mergeFromCount-1?
                //                int adjacentLane = _currentRow.NodeLane + 1;
                //                if (adjacentLane >= _laneNodes.Count) Debugger.Break();
                //                _currentRow.Expand(adjacentLane);
                //                _currentRow.Replace(laneInfo.ConnectLane + 1, adjacentLane);

                //                LaneJunctionDetail temp = _laneNodes[laneInfo.ConnectLane];
                //                _laneNodes.RemoveAt(laneInfo.ConnectLane);
                //                _laneNodes.Insert(adjacentLane, temp);
                //            }
                //        }
                //    }
                //}

                #endregion

                if (_currentRow.Node != null)
                {
                    Graph.LaneRow row = _currentRow.Advance();

                    // This means there is a node that got put in the graph twice...
                    if (row.Node.InLane != int.MaxValue)
                    {
                        if (Debugger.IsAttached) Debugger.Break();
                    }

                    row.Node.InLane = _laneRows.Count;
                    _laneRows.Add(row);
                    return true;
                }

                // Return that there are more items left
                return false;
            }

            /// <summary>
            /// Advance the lane to the next element
            /// </summary>
            /// <param name="curLane">Index of the lane to advance</param>
            /// <returns>True if there will still be nodes in this lane</returns>
            private int AdvanceLane(int curLane)
            {
                LaneJunctionDetail lane = _laneNodes[curLane];
                int minLane = curLane;

                // Advance the lane
                lane.Next();

                // See if we can pull up ancestors
                if (lane.Count == 0 && lane.Junction == null)
                {
                    // Handle a single node branch.
                    _currentRow.Collapse(curLane);
                    _laneNodes.RemoveAt(curLane);
                }
                else if (lane.Count == 0)
                {
                    Node node = lane.Junction.Parent;
                    foreach (Junction parent in node.Ancestors)
                    {
                        if (parent.CurrentState != Junction.State.Unprocessed)
                        {
                            // This item is already in the lane list, no action needed
                            continue;
                        }

                        var addedLane = new LaneJunctionDetail(parent);
                        addedLane.Next();
                        int addedLaneLane = int.MaxValue;

                        // Check to see if this junction already points to one of the
                        // existing lanes. If so, we'll just add the lane line and not
                        // add it to the _laneNodes.
                        if (addedLane.Count == 1)
                        {
                            for (int i = 0; i < _laneNodes.Count; i++)
                            {
                                if (_laneNodes[i].Current == addedLane.Current)
                                {
                                    // We still advance the lane so it gets
                                    // marked as processed.
                                    addedLane.Next();

                                    addedLaneLane = i;
                                    break;
                                }
                            }
                        }

                        // Add to the lane nodes
                        if (addedLaneLane == int.MaxValue)
                        {
                            if (lane.Count == 0)
                            {
                                lane = addedLane;
                                _laneNodes[curLane] = lane;
                                addedLaneLane = curLane;
                            }
                            else
                            {
                                addedLaneLane = curLane + 1;
                                _laneNodes.Insert(addedLaneLane, addedLane);
                                _currentRow.Expand(addedLaneLane);
                            }
                        }

                        _currentRow.Add(curLane, new Graph.LaneInfo(addedLaneLane, parent));
                    }

                    // If the lane count after processing is still 0
                    // this is a root node of the graph
                    if (lane.Count == 0)
                    {
                        _currentRow.Collapse(curLane);
                        _laneNodes.RemoveAt(curLane);
                    }
                }
                else if (lane.Count == 1)
                {
                    // If any other lanes have this node on top, merge them together
                    for (int i = 0; i < _laneNodes.Count; i++)
                    {
                        if (i == curLane || curLane >= _laneNodes.Count) continue;
                        if (_laneNodes[i].Current == _laneNodes[curLane].Current)
                        {
                            int left;
                            int right;
                            Junction junction = _laneNodes[curLane].Junction;
                            if (i > curLane)
                            {
                                left = curLane;
                                right = i;
                            }
                            else
                            {
                                left = i;
                                right = curLane;
                                curLane = i;
                            }
                            _currentRow.Replace(right, left);
                            _currentRow.Collapse(right);
                            _laneNodes[right].Clear();
                            _laneNodes.RemoveAt(right);

                            _currentRow.Add(_currentRow.NodeLane, new Graph.LaneInfo(left, junction));
                            minLane = Math.Min(minLane, left);
                        }
                    }

                    // If the current lane is still active, add it. It might not be active
                    // if it got merged above.
                    if (!lane.IsClear)
                    {
                        _currentRow.Add(_currentRow.NodeLane, new Graph.LaneInfo(curLane, lane.Junction));
                    }
                }
                else // lane.Count > 1
                {
                    _currentRow.Add(_currentRow.NodeLane, new Graph.LaneInfo(curLane, lane.Junction));
                }

                return curLane;
            }

            #region Nested type: ActiveLaneRow

            private class ActiveLaneRow : Graph.LaneRow
            {
                private Edges _edges;
                private Node _node;
                private int _nodeLane = -1;

                public Edge[] EdgeList
                {
                    get { return _edges.EdgeList.ToArray(); }
                }

                #region LaneRow Members

                public int NodeLane
                {
                    get { return _nodeLane; }
                    set { _nodeLane = value; }
                }

                public Node Node
                {
                    get { return _node; }
                    set { _node = value; }
                }

                public int Count
                {
                    get { return _edges.CountCurrent(); }
                }

                public int LaneInfoCount(int lane)
                {
                    return _edges.CountCurrent(lane);
                }

                public Graph.LaneInfo this[int col, int row]
                {
                    get { return _edges.Current(col, row); }
                }

                #endregion

                public void Add(int lane, Graph.LaneInfo data)
                {
                    _edges.Add(lane, data);
                }

                public void Clear()
                {
                    _edges = new Edges();
                }

                public void Clear(int lane)
                {
                    _edges.Clear(lane);
                }

                public void Collapse(int col)
                {
                    int edgeCount = Math.Max(_edges.CountCurrent(), _edges.CountNext());
                    for (int i = col; i < edgeCount; i++)
                    {
                        while (_edges.CountNext(i) > 0)
                        {
                            int start, end;
                            Graph.LaneInfo info = _edges.RemoveNext(i, 0, out start, out end);
                            info.ConnectLane--;
                            _edges.Add(start, info);
                        }
                    }
                }

                public void Expand(int col)
                {
                    int edgeCount = Math.Max(_edges.CountCurrent(), _edges.CountNext());
                    for (int i = edgeCount - 1; i >= col; --i)
                    {
                        while (_edges.CountNext(i) > 0)
                        {
                            int start, end;
                            Graph.LaneInfo info = _edges.RemoveNext(i, 0, out start, out end);
                            info.ConnectLane++;
                            _edges.Add(start, info);
                        }
                    }
                }

                public void Replace(int aOld, int aNew)
                {
                    for (int j = _edges.CountNext(aOld) - 1; j >= 0; --j)
                    {
                        int start, end;
                        Graph.LaneInfo info = _edges.RemoveNext(aOld, j, out start, out end);
                        info.ConnectLane = aNew;
                        _edges.Add(start, info);
                    }
                }

                public void Swap(int aOld, int aNew)
                {
                    // TODO: There is a more efficient way to do this
                    int temp = _edges.CountNext();
                    Replace(aOld, temp);
                    Replace(aNew, aOld);
                    Replace(temp, aNew);
                }

                public Graph.LaneRow Advance()
                {
                    var newLaneRow = new SavedLaneRow(this);

                    var newEdges = new Edges();
                    for (int i = 0; i < _edges.CountNext(); i++)
                    {
                        int edgeCount = _edges.CountNext(i);
                        if (edgeCount > 0)
                        {
                            Graph.LaneInfo info = _edges.Next(i, 0).Clone();
                            for (int j = 1; j < edgeCount; j++)
                            {
                                Graph.LaneInfo edgeInfo = _edges.Next(i, j);
                                info.UnionWith(edgeInfo);
                            }
                            newEdges.Add(i, info);
                        }
                    }
                    _edges = newEdges;

                    return newLaneRow;
                }

                public override string ToString()
                {
                    string s = _nodeLane + "/" + Count + ": ";
                    for (int i = 0; i < Count; i++)
                    {
                        if (i == _nodeLane)
                            s += "*";
                        s += "{";
                        for (int j = 0; j < LaneInfoCount(i); j++)
                            s += " " + this[i, j];
                        s += " }, ";
                    }
                    s += _node;
                    return s;
                }

                #region Nested type: Edges

                private class Edges
                {
                    private readonly List<int> _countEnd = new List<int>();
                    private readonly List<int> _countStart = new List<int>();
                    private readonly List<Edge> _edges = new List<Edge>();

                    private static readonly Graph.LaneInfo _emptyItem = new Graph.LaneInfo();

                    public List<Edge> EdgeList
                    {
                        get { return _edges; }
                    }

                    public Graph.LaneInfo Current(int lane, int item)
                    {
                        int found = 0;
                        foreach (Edge e in _edges)
                        {
                            if (e.Start == lane)
                            {
                                if (item == found)
                                {
                                    return e.Data;
                                }
                                found++;
                            }
                        }
                        return _emptyItem;
                    }

                    public Graph.LaneInfo Next(int lane, int item)
                    {
                        int found = 0;
                        foreach (Edge e in _edges)
                        {
                            if (e.End == lane)
                            {
                                if (item == found)
                                {
                                    return e.Data;
                                }
                                found++;
                            }
                        }
                        return _emptyItem;
                    }

                    public Graph.LaneInfo RemoveNext(int lane, int item, out int start, out int end)
                    {
                        int found = 0;
                        for (int i = 0; i < _edges.Count; i++)
                        {
                            if (_edges[i].End == lane)
                            {
                                if (item == found)
                                {
                                    Graph.LaneInfo data = _edges[i].Data;
                                    start = _edges[i].Start;
                                    end = _edges[i].End;
                                    _countStart[start]--;
                                    _countEnd[end]--;
                                    _edges.RemoveAt(i);
                                    return data;
                                }
                                found++;
                            }
                        }

                        start = -1;
                        end = -1;
                        return _emptyItem;
                    }

                    public void Add(int from, Graph.LaneInfo data)
                    {
                        var e = new Edge(data, from);
                        _edges.Add(e);

                        while (_countStart.Count <= e.Start)
                        {
                            _countStart.Add(0);
                        }
                        _countStart[e.Start]++;
                        while (_countEnd.Count <= e.End)
                        {
                            _countEnd.Add(0);
                        }
                        _countEnd[e.End]++;
                    }

                    public void Clear(int lane)
                    {
                        for (int i = _edges.Count - 1; i >= 0; --i)
                        {
                            int start = _edges[i].Start;
                            if (start == lane)
                            {
                                int end = _edges[i].End;
                                _countStart[start]--;
                                _countEnd[end]--;
                                _edges.RemoveAt(i);
                            }
                        }
                    }

                    public int CountCurrent()
                    {
                        int count = _countStart.Count;
                        while (count > 0 && _countStart[count - 1] == 0)
                        {
                            count--;
                            _countStart.RemoveAt(count);
                        }

                        return count;
                    }

                    public int CountCurrent(int lane)
                    {
                        int found = 0;
                        foreach (Edge e in _edges)
                        {
                            if (e.Start == lane)
                            {
                                found++;
                            }
                        }
                        return found;
                    }

                    public int CountNext()
                    {
                        int count = _countEnd.Count;
                        while (count > 0 && _countEnd[count - 1] == 0)
                        {
                            count--;
                            _countEnd.RemoveAt(count);
                        }

                        return count;
                    }

                    public int CountNext(int lane)
                    {
                        int found = 0;
                        foreach (Edge e in _edges)
                        {
                            if (e.End == lane)
                            {
                                found++;
                            }
                        }
                        return found;
                    }

                    public bool IsActive(int lane)
                    {
                        if (lane >= CountNext())
                        {
                            return false;
                        }
                        return (_countEnd[lane] > 0);
                    }

                    private void Remove(int start, int end)
                    {
                    }
                }

                #endregion
            }

            #endregion

            #region Nested type: Edge

            private struct Edge
            {
                public readonly int Start;
                public Graph.LaneInfo Data;

                public Edge(Graph.LaneInfo data, int start)
                {
                    Data = data;
                    Start = start;
                }

                public int End
                {
                    get { return Data.ConnectLane; }
                }

                public override string ToString()
                {
                    return string.Format("{0}->{1}: {2}", Start, End, Data);
                }
            }

            #endregion

            #region Nested type: LaneEnumerator

            private class LaneEnumerator : IEnumerator<Graph.LaneRow>
            {
                private readonly Lanes _lanes;
                private int _index;

                public LaneEnumerator(Lanes aLanes)
                {
                    _lanes = aLanes;
                    Reset();
                }

                #region IEnumerator<LaneRow> Members

                public void Reset()
                {
                    _index = 0;
                }

                void IDisposable.Dispose()
                {
                }

                object IEnumerator.Current
                {
                    get { return Current; }
                }

                public Graph.LaneRow Current
                {
                    get { return _lanes[_index]; }
                }

                public bool MoveNext()
                {
                    _index++;
                    return _index < _lanes._laneRows.Count;
                }

                #endregion
            }

            #endregion

            #region Nested type: LaneJunctionDetail

            private class LaneJunctionDetail
            {
                private int _index;
                private Junction _junction;
                private Node _node;

                public LaneJunctionDetail()
                {
                }

                public LaneJunctionDetail(Node n)
                {
                    _node = n;
                }

                public LaneJunctionDetail(Junction j)
                {
                    _junction = j;
                    _junction.CurrentState = Junction.State.Processing;
                    _index = 0;
                }

                public int Count
                {
                    get
                    {
                        if (_node != null)
                        {
                            return 1 - _index;
                        }
                        else if (_junction == null)
                        {
                            return 0;
                        }
                        else
                        {
                            return _junction.Bunch.Count - _index;
                        }
                    }
                }

                public Junction Junction
                {
                    get { return _junction; }
                }

                public Node Current
                {
                    get
                    {
                        if (_node != null)
                        {
                            return _node;
                        }
                        else if (_index < _junction.Bunch.Count)
                        {
                            return _junction.Bunch[_index];
                        }
                        else
                        {
                            return null;
                        }
                    }
                }

                public bool IsClear
                {
                    get { return (_junction == null && _node == null); }
                }

                public void Clear()
                {
                    _node = null;
                    _junction = null;
                    _index = 0;
                }

                public Node Next()
                {
                    Node n;
                    if (_node != null)
                    {
                        n = _node;
                    }
                    else
                    {
                        n = _junction.Bunch[_index];
                    }
                    _index++;

                    if (_junction != null && _index >= _junction.Bunch.Count)
                    {
                        _junction.CurrentState = Junction.State.Processed;
                    }
                    return n;
                }

                public override string ToString()
                {
                    if (_junction != null)
                    {
                        string nodeName = "(null)";
                        if (_index < _junction.Bunch.Count)
                        {
                            nodeName = _junction.Bunch[_index].ToString();
                        }
                        return _index + "/" + _junction.Bunch.Count + "~" + nodeName + "~" + _junction;
                    }
                    else if (_node != null)
                    {
                        return _index + "/n~" + _node + "~(null)";
                    }
                    else
                    {
                        return "X/X~(null)~(null)";
                    }
                }
            }

            #endregion

            #region Nested type: SavedLaneRow

            private class SavedLaneRow : Graph.LaneRow
            {
                private readonly Edge[] _edges;
                private Node _node;
                private int _nodeLane = -1;

                public SavedLaneRow(Node aNode)
                {
                    _node = aNode;
                    _nodeLane = -1;
                    _edges = null;
                }

                public SavedLaneRow(ActiveLaneRow activeRow)
                {
                    _nodeLane = activeRow.NodeLane;
                    _node = activeRow.Node;
                    _edges = activeRow.EdgeList;
                }

                #region LaneRow Members

                public int NodeLane
                {
                    get { return _nodeLane; }
                    set { _nodeLane = value; }
                }

                public Node Node
                {
                    get { return _node; }
                    set { _node = value; }
                }

                public Graph.LaneInfo this[int col, int row]
                {
                    get
                    {
                        int count = 0;
                        foreach (Edge edge in _edges)
                        {
                            if (edge.Start == col)
                            {
                                if (count == row)
                                {
                                    return edge.Data;
                                }
                                count++;
                            }
                        }
                        throw new Exception("Bad lane");
                    }
                }

                public int Count
                {
                    get
                    {
                        if (_edges == null)
                        {
                            return 0;
                        }

                        int count = -1;
                        foreach (Edge edge in _edges)
                        {
                            if (edge.Start > count)
                            {
                                count = edge.Start;
                            }
                        }
                        return count + 1;
                    }
                }

                public int LaneInfoCount(int lane)
                {
                    int count = 0;
                    foreach (Edge edge in _edges)
                    {
                        if (edge.Start == lane)
                        {
                            count++;
                        }
                    }
                    return count;
                }

                #endregion

                public override string ToString()
                {
                    string s = _nodeLane + "/" + Count + ": ";
                    for (int i = 0; i < Count; i++)
                    {
                        if (i == _nodeLane)
                            s += "*";
                        s += "{";
                        for (int j = 0; j < LaneInfoCount(i); j++)
                            s += " " + this[i, j];
                        s += " }, ";
                    }
                    s += _node;
                    return s;
                }

                // Node information
            }

            #endregion
        }

        #endregion
    }
}
