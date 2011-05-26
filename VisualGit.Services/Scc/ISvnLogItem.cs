using System;
using System.Collections.Generic;
using System.Text;
using SharpSvn;
using SharpSvn.Implementation;

namespace VisualGit.Scc
{
    public interface ISvnLogItem
    {
        DateTime CommitDate { get; }
        string Author { get; }
        string LogMessage { get; }
        IEnumerable<VisualGit.VS.IssueMarker> Issues { get; }
        long Revision { get; }
        int Index { get; }
        SvnChangeItemCollection ChangedPaths { get; }

        Uri RepositoryRoot { get; }
    }
}
