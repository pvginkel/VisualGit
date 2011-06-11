using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.IO;

namespace SharpGit
{
    public sealed class GitStatusEventArgs : CancelEventArgs
    {
        private string _name;

        public Uri Uri { get; internal set; }

        public string Name
        {
            get
            {
                if (_name == null && FullPath != null)
                    _name = Path.GetFileName(FullPath);

                return _name;
            }
        }

        public string FullPath { get; internal set; }
        public GitNodeKind NodeKind { get; internal set; }
        public GitStatus LocalContentStatus { get; internal set; }
        public bool LocalCopied { get; internal set; }
        public GitWorkingCopyInfo WorkingCopyInfo { get; internal set; }
        public GitConflictData TreeConflict { get; internal set; }
        public GitInternalStatus InternalContentStatus { get; set; }
    }
}
