using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace VisualGit.UI.GitLog.RevisionGrid
{
    partial class DvcsGraph
    {
        #region Nested type: Graph

        private class Graph
        {
            public List<Node> AddedNodes { get; private set; }
            public List<Junction> Junctions { get; private set; }
            public Dictionary<string, Node> Nodes { get; private set; }

            private readonly Lanes _lanes;
            private int _filterNodeCount;

            private bool _isFilter;
            private int _nodeCount;
            private int _processedNodes;

            public Graph()
            {
                AddedNodes = new List<Node>();
                Junctions = new List<Junction>();
                Nodes = new Dictionary<string, Node>(StringComparer.OrdinalIgnoreCase);

                _lanes = new Lanes(this);
            }

            public bool IsFilter
            {
                get { return _isFilter; }
                set
                {
                    _isFilter = value;
                    _lanes.Clear();
                    foreach (Node n in Nodes.Values)
                    {
                        n.InLane = int.MaxValue;
                    }
                    foreach (Junction j in Junctions)
                    {
                        j.CurrentState = Junction.State.Unprocessed;
                    }

                    // We need to signal the DvcsGraph object that it needs to 
                    // redraw everything.
                    OnUpdated(EventArgs.Empty);
                }
            }

            public int Count
            {
                get
                {
                    if (IsFilter)
                    {
                        return _filterNodeCount;
                    }

                    return _nodeCount;
                }
            }

            public LaneRow this[int col]
            {
                get { return _lanes[col]; }
            }

            public int CachedCount
            {
                get { return _lanes.CachedCount; }
            }

            public void Filter(string aId)
            {
                Node node = Nodes[aId];

                if (!node.IsFiltered)
                {
                    _filterNodeCount++;
                    node.IsFiltered = true;
                }

                // Clear the filtered lane data. 
                // TODO: We could be smart and only clear items after Node[aId]. The check
                // below isn't valid, since it could be either the filtered or unfiltered
                // lane...
                //if (node.InLane != int.MaxValue)
                //{
                //    filteredLanes.Clear();
                //}
            }

            public event EventHandler Updated;

            protected virtual void OnUpdated(EventArgs e)
            {
                var ev = Updated;

                if (ev != null)
                    ev(this, e);
            }

            public void Add(string aId, ICollection<string> aParentIds, DataType aType, object aData)
            {
                // If we haven't seen this node yet, create a new junction.
                Node node = null;
                if (!GetNode(aId, out node) && (aParentIds == null || aParentIds.Count == 0))
                {
                    var newJunction = new Junction(node, node);
                    Junctions.Add(newJunction);
                }
                _nodeCount++;
                node.Data = aData;
                node.DataType = aType;
                node.Index = AddedNodes.Count;
                AddedNodes.Add(node);

                foreach (string parentId in aParentIds)
                {
                    Node parent;
                    GetNode(parentId, out parent);
                    if (parent.Index < node.Index)
                    {
                        // TODO: We might be able to recover from this with some work, but
                        // since we build the graph async it might be tough to figure out.
                        //throw new ArgumentException("The nodes must be added such that all children are added before their parents", "aParentIds");
                        continue;
                    }

                    if (node.Descendants.Count == 1 && node.Ancestors.Count <= 1
                        && node.Descendants[0].Parent == node
                        && parent.Ancestors.Count == 0
                        //If this is true, the current revision is in the middle of a branch 
                        //and is about to start a new branch. This will also mean that the last
                        //revisions are non-relative. Make sure a new junction is added and this
                        //is the start of a new branch (and color!)
                        && (aType & DataType.Active) != DataType.Active
                        )
                    {
                        // The node isn't a junction point. Just the parent to the node's
                        // (only) ancestor junction.
                        node.Descendants[0].Add(parent);
                    }
                    else if (node.Ancestors.Count == 1 && node.Ancestors[0].Child != node)
                    {
                        // The node is in the middle of a junction. We need to split it.                   
                        Junction splitNode = node.Ancestors[0].Split(node);
                        Junctions.Add(splitNode);

                        // The node is a junction point. We are a new junction
                        var junction = new Junction(node, parent);
                        Junctions.Add(junction);
                    }
                    else if (parent.Descendants.Count == 1 && parent.Descendants[0].Parent != parent)
                    {
                        // The parent is in the middle of a junction. We need to split it.     
                        Junction splitNode = parent.Descendants[0].Split(parent);
                        Junctions.Add(splitNode);

                        // The node is a junction point. We are a new junction
                        var junction = new Junction(node, parent);
                        Junctions.Add(junction);
                    }
                    else
                    {
                        // The node is a junction point. We are a new junction
                        var junction = new Junction(node, parent);
                        Junctions.Add(junction);
                    }
                }

                bool isRelative = (aType & DataType.Active) == DataType.Active;
                if (!isRelative && node.Descendants.Any(d => d.IsRelative))
                {
                    isRelative = true;
                }

                bool isRebuild = false;
                foreach (Junction d in node.Ancestors)
                {
                    d.IsRelative = isRelative || d.IsRelative;

                    // Uh, oh, we've already processed this lane. We'll have to update some rows.
                    int idx = d.Bunch.IndexOf(node);
                    if (idx < d.Bunch.Count && d.Bunch[idx + 1].InLane != int.MaxValue)
                    {
                        int resetTo = d.Parent.Descendants.Aggregate(d.Parent.InLane, (current, dd) => Math.Min(current, dd.Child.InLane));

                        Console.WriteLine("We have to start over at lane {0} because of {1}", resetTo, node);
                        isRebuild = true;
                        break;
                    }
                }

                if (isRebuild)
                {
                    // TODO: It would be nice if we didn't have to start completely over...but it wouldn't
                    // be easy since we don't keep around all of the necessary lane state for each step.
                    int lastLane = _lanes.Count - 1;
                    _lanes.Clear();
                    _lanes.CacheTo(lastLane);

                    // We need to signal the DvcsGraph object that it needs to redraw everything.
                    OnUpdated(EventArgs.Empty);
                }
                else
                {
                    _lanes.Update(node);
                }
            }

            public void Clear()
            {
                AddedNodes.Clear();
                Junctions.Clear();
                Nodes.Clear();
                _lanes.Clear();
                _nodeCount = 0;
                _filterNodeCount = 0;
            }

            public void ProcessNode(Node aNode)
            {
                if (_isFilter)
                {
                    return;
                }
                for (int i = _processedNodes; i < AddedNodes.Count; i++)
                {
                    if (AddedNodes[i] == aNode)
                    {
                        bool isChanged = false;
                        while (i > _processedNodes)
                        {
                            // This only happens if we weren't in topo order
                            if (Debugger.IsAttached) Debugger.Break();

                            Node temp = AddedNodes[i];
                            AddedNodes[i] = AddedNodes[i - 1];
                            AddedNodes[i - 1] = temp;
                            i--;
                            isChanged = true;
                        }

                        // Signal that these rows have changed
                        if (isChanged)
                        {
                            OnUpdated(EventArgs.Empty);
                        }

                        _processedNodes++;
                        break;
                    }
                }
            }

            public bool Prune()
            {
                bool isPruned = false;
            // Remove all nodes that don't have a value associated with them.
            start_over:
                foreach (Node n in Nodes.Values)
                {
                    if (n.Data == null)
                    {
                        Nodes.Remove(n.Id);
                        // This guy should have been at the end of some junctions
                        foreach (Junction j in n.Descendants)
                        {
                            j.Bunch.Remove(n);
                            j.Parent.Ancestors.Remove(j);
                        }
                        isPruned = true;
                        goto start_over;
                    }
                }

                return isPruned;
            }

            public IEnumerable<Node> GetHeads()
            {
                var nodes = new List<Node>();
                foreach (Junction j in Junctions)
                {
                    if (j.Child.Descendants.Count == 0 && !nodes.Contains(j.Child))
                    {
                        nodes.Add(j.Child);
                    }
                }
                return nodes;
            }

            public bool CacheTo(int idx)
            {
                return _lanes.CacheTo(idx);
            }

            // TopoSorting is an easy way to detect if something has gone wrong with the graph

            public Node[] TopoSortedNodes()
            {
                //http://en.wikipedia.org/wiki/Topological_ordering
                //L ← Empty list that will contain the sorted nodes
                //S ← Set of all nodes with no incoming edges

                //function visit(node n)
                //    if n has not been visited yet then
                //        mark n as visited
                //        for each node m with an edge from n to m do
                //            visit(m)
                //        add n to L

                //for each node n in S do
                //    visit(n)

                var L = new Queue<Node>();
                var S = new Queue<Node>();
                var P = new Queue<Node>();
                foreach (Node h in GetHeads())
                {
                    foreach (Junction j in h.Ancestors)
                    {
                        if (!S.Contains(j.Parent)) S.Enqueue(j.Parent);
                        if (!S.Contains(j.Child)) S.Enqueue(j.Child);
                    }
                }

                Func<Node, bool> visit = null;
                Func<Node, bool> localVisit = visit;
                visit = (Node n) =>
                {
                    if (!P.Contains(n))
                    {
                        P.Enqueue(n);
                        foreach (Junction e in n.Ancestors)
                        {
                            if (localVisit != null) localVisit(e.Parent);
                        }
                        L.Enqueue(n);
                        return true;
                    }
                    return false;
                };
                foreach (Node n in S)
                {
                    visit(n);
                }

                // Sanity check
                var J = new Queue<Junction>();
                var X = new Queue<Node>();
                foreach (Node n in L)
                {
                    foreach (Junction e in n.Descendants)
                    {
                        if (X.Contains(e.Child))
                        {
                            Debugger.Break();
                        }
                        if (!J.Contains(e))
                        {
                            J.Enqueue(e);
                        }
                    }
                    X.Enqueue(n);
                }

                if (J.Count != Junctions.Count)
                {
                    foreach (Junction j in Junctions)
                    {
                        if (!J.Contains(j))
                        {
                            if (j.Parent != j.Child)
                            {
                                Console.WriteLine("*** {0} *** {1} {2}", j, Nodes.Count, Junctions.Count);
                            }
                        }
                    }
                }

                return L.ToArray();
            }

            private bool GetNode(string aId, out Node aNode)
            {
                if (!Nodes.TryGetValue(aId, out aNode))
                {
                    aNode = new Node(aId);
                    Nodes.Add(aId, aNode);
                    return false;
                }
                return true;
            }

            #region Nested type: LaneInfo

            public struct LaneInfo
            {
                private int connectLane;
                private List<Junction> junctions;

                public LaneInfo(int aConnectLane, Junction aJunction)
                {
                    connectLane = aConnectLane;
                    junctions = new List<Junction>(1);
                    junctions.Add(aJunction);
                }

                public int ConnectLane
                {
                    get { return connectLane; }
                    set { connectLane = value; }
                }

                public IEnumerable<Junction> Junctions
                {
                    get { return junctions; }
                }

                public LaneInfo Clone()
                {
                    var other = new LaneInfo { connectLane = connectLane, junctions = new List<Junction>(junctions) };
                    return other;
                }

                public void UnionWith(LaneInfo aOther)
                {
                    foreach (Junction other in aOther.junctions)
                    {
                        if (!junctions.Contains(other))
                        {
                            junctions.Add(other);
                        }
                    }
                    junctions.TrimExcess();
                }

                public static implicit operator int(LaneInfo a)
                {
                    return a.ConnectLane;
                }

                public override string ToString()
                {
                    return ConnectLane.ToString();
                }
            }

            #endregion

            #region Nested type: LaneRow

            public interface LaneRow
            {
                // Node information
                int NodeLane { get; }
                Node Node { get; }

                // Lane information
                int Count { get; }
                LaneInfo this[int lane, int item] { get; }
                int LaneInfoCount(int lane);
            }

            #endregion
        }

        #endregion

        #region Nested type: Junction

        private class Junction
        {
            #region State enum

            public enum State
            {
                Unprocessed,
                Processing,
                Processed,
            }

            #endregion

            private static uint _debugIdNext;
            private readonly uint _debugId;

            public List<Node> Bunch { get; private set; }
            public State CurrentState { get; set; }
            public bool IsRelative { get; set; }

            public Junction(Node aNode, Node aParent)
                : this()
            {
                _debugId = _debugIdNext++;

                Bunch.Add(aNode);
                if (aNode != aParent)
                {
                    aNode.Ancestors.Add(this);
                    aParent.Descendants.Add(this);
                    Bunch.Add(aParent);
                }
            }

            private Junction(Junction aDescendant, Node aNode)
                : this()
            {
                // Private constructor used by split. This junction will be a
                // ancestor of an existing junction.
                _debugId = _debugIdNext++;
                aNode.Ancestors.Remove(aDescendant);
                Bunch.Add(aNode);
            }

            private Junction()
            {
                Bunch = new List<Node>();
                CurrentState = State.Unprocessed;
            }

            public Node Child
            {
                get { return Bunch[0]; }
            }

            public Node Parent
            {
                get { return Bunch[Bunch.Count - 1]; }
            }

            public void Add(Node aParent)
            {
                aParent.Descendants.Add(this);
                Parent.Ancestors.Add(this);
                Bunch.Add(aParent);
            }

            public Junction Split(Node aNode)
            {
                // The 'top' (Child->node) of the junction is retained by 
                // The 'bottom' (node->Parent) of the junction is returned.
                int index = Bunch.IndexOf(aNode);
                if (index == -1)
                {
                    return null;
                }
                var bottom = new Junction(this, aNode);
                // Add 1, since aNode was at the index
                index += 1;
                while (Bunch.Count > index)
                {
                    Node node = Bunch[index];
                    Bunch.RemoveAt(index);
                    node.Ancestors.Remove(this);
                    node.Descendants.Remove(this);
                    bottom.Add(node);
                }

                return bottom;
            }

            public override string ToString()
            {
                return string.Format("{3}: {0}--({2})--{1}", Child, Parent, Bunch.Count, _debugId);
            }
        }

        #endregion

    }
}
