using System;
using System.Collections.Generic;
using System.Text;
using SharpSvn;
using SharpSvn.Implementation;
using SharpGit;

namespace VisualGit.Scc
{
    public interface IGitLogItem
    {
        DateTime CommitDate { get; }
        string Author { get; }
        string LogMessage { get; }
        IEnumerable<VisualGit.VS.IssueMarker> Issues { get; }
        IList<string> ParentRevisions { get; }
        string Revision { get; }
        int Index { get; }
        GitChangeItemCollection ChangedPaths { get; }

        Uri RepositoryRoot { get; }
    }
}
