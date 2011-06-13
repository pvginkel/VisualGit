using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace SharpGit
{
    public class GitBlameEventArgs : CancelEventArgs
    {
        public string Author { get; internal set; }
        public DateTime Time { get; internal set; }
        public string Line { get; internal set; }
        public string Revision { get; set; }
        public int LineNumber { get; set; }
    }
}
