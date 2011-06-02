using System;
using System.Collections.Generic;
using System.Text;
using SharpSvn;
using System.Diagnostics;
using SharpGit;

namespace VisualGit.UI
{
    public class PathSelectorResult
    {
        readonly bool _succeeded;
        readonly List<GitItem> _selection;
        GitDepth _depth = GitDepth.Unknown;
        GitRevision _start;
        GitRevision _end;

        public PathSelectorResult(bool succeeded, IEnumerable<GitItem> items)
        {
            if (items == null)
                throw new ArgumentNullException("items");

            _succeeded = succeeded;
            _selection = new List<GitItem>(items);
        }

        public GitDepth Depth
        {
            [DebuggerStepThrough]
            get { return _depth; }
            set { _depth = value; }
        }

        public GitRevision RevisionStart
        {
            [DebuggerStepThrough]
            get { return _start; }
            set { _start = value; }
        }

        public GitRevision RevisionEnd
        {
            [DebuggerStepThrough]
            get { return _end; }
            set { _end = value; }
        }


        public IList<GitItem> Selection
        {
            [DebuggerStepThrough]
            get { return _selection; }
        }

        public bool Succeeded
        {
            [DebuggerStepThrough]
            get { return _succeeded; }
        }
    }
}
