using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VisualGit.UI.GitLog
{
    public sealed class BatchFinishedEventArgs : EventArgs
    {
        readonly int _totalCount;
        internal BatchFinishedEventArgs(int totalCount)
        {
            _totalCount = totalCount;
        }

        public int TotalCount
        {
            get { return _totalCount; }
        }
    }
}
