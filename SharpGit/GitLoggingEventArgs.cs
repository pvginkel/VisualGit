using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace SharpGit
{
    public abstract class GitLoggingEventArgs : CancelEventArgs
    {
        protected GitLoggingEventArgs()
        {
        }

        public string LogMessage { get; internal set; }
        public DateTime Time { get; internal set; }
        public string Author { get; internal set; }
        public string AuthorName { get; internal set; }
        public string AuthorEmail { get; internal set; }
        public DateTime AuthorTime { get; internal set; }
        public string Revision { get; internal set; }
        public IList<string> ParentRevisions { get; internal set; }
        public IList<GitRef> Refs { get; internal set; }

        public GitChangeItemCollection ChangedPaths { get; internal set; }
    }
}
