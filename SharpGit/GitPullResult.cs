using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    public sealed class GitPullResult : GitCommandResult
    {
        public GitPullResult()
        {
            FailedMergePaths = new Dictionary<string, GitMergeFailureReason>(FileSystemUtil.StringComparer);
        }

        public string Commit { get; internal set; }

        public GitMergeResult MergeResult { get; internal set; }

        public IDictionary<string, GitMergeFailureReason> FailedMergePaths { get; private set; }

        public IDictionary<string, int[][]> Conflicts { get; set; }
    }
}
