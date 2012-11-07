using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    public class GitFileStatus
    {
        public string FullPath { get; private set; }
        public GitStatus LocalContentStatus { get; private set; }
        public GitSchedule Schedule { get; private set; }
        public GitInternalStatus InternalContentStatus { get; private set; }
        public GitNodeKind NodeKind { get; private set; }

        public GitFileStatus(string fullPath, GitStatus localContentStatus, GitSchedule schedule, GitInternalStatus internalContentStatus, GitNodeKind nodeKind)
        {
            FullPath = fullPath;
            LocalContentStatus = localContentStatus;
            Schedule = schedule;
            InternalContentStatus = internalContentStatus;
            NodeKind = nodeKind;
        }
    }
}
