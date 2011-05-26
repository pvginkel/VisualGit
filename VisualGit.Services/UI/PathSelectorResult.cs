using System;
using System.Collections.Generic;
using System.Text;
using SharpSvn;
using System.Diagnostics;

namespace VisualGit.UI
{
    public class PathSelectorResult
    {
        readonly bool _succeeded;
        readonly List<GitItem> _selection;
        SvnDepth _depth = SvnDepth.Unknown;
        SvnRevision _start;
        SvnRevision _end;

        public PathSelectorResult(bool succeeded, IEnumerable<GitItem> items)
        {
            if (items == null)
                throw new ArgumentNullException("items");

            _succeeded = succeeded;
            _selection = new List<GitItem>(items);
        }

        public SvnDepth Depth
        {
            [DebuggerStepThrough]
            get { return _depth; }
            set { _depth = value; }
        }

        public SvnRevision RevisionStart
        {
            [DebuggerStepThrough]
            get { return _start; }
            set { _start = value; }
        }

        public SvnRevision RevisionEnd
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
