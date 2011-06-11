using System;
using System.Collections.Generic;

namespace VisualGit.UI.GitLog.RevisionGrid
{
    internal class GitRevision : IGitItem, VisualGit.Scc.IGitLogItem
    {
        private static readonly VS.IssueMarker[] EmptyIssueMarker = new VS.IssueMarker[0];

        public static string UncommittedWorkingDirGuid = "0000000000000000000000000000000000000000";
        public static string IndexGuid = "1111111111111111111111111111111111111111";

        public IList<string> ParentRevisions { get; set; }

        private List<IGitItem> _subItems;

        public GitRevision()
        {
            Heads = new List<GitHead>();
        }

        public ICollection<GitHead> Heads { get; set; }

        public string TreeHash { get; set; }

        public string Author { get; set; }
        public string AuthorEmail { get; set; }
        public DateTime AuthorDate { get; set; }
        public string Committer { get; set; }
        public DateTime CommitDate { get; set; }

        public string LogMessage { get; set; }

        #region IGitItem Members

        public string Revision { get; set; }
        public string Name { get; set; }

        public List<IGitItem> SubItems
        {
            get { return _subItems ?? (_subItems = GitCommandHelpers.GetTree(TreeHash)); }
        }

        #endregion

        #region IGitLogItem Members

        public int Index { get; set; }
        public Uri RepositoryRoot { get; set; }

        IEnumerable<VS.IssueMarker> VisualGit.Scc.IGitLogItem.Issues
        {
            get { return EmptyIssueMarker; }
        }

        SharpGit.GitChangeItemCollection VisualGit.Scc.IGitLogItem.ChangedPaths
        {
            get { return null; }
        }

        #endregion

        public override string ToString()
        {
            var sha = Revision;
            if (sha.Length > 8)
            {
                sha = sha.Substring(0, 4) + ".." + sha.Substring(sha.Length - 4, 4);
            }
            return String.Format("{0}:{1}", sha, LogMessage);
        }

        public bool MatchesSearchString(string searchString)
        {
            foreach (var gitHead in Heads)
            {
                if (gitHead.Name.ToLower().Contains(searchString))
                    return true;
            }

            if ((searchString.Length > 2) && Revision.StartsWith(searchString, StringComparison.OrdinalIgnoreCase))
                return true;


            return
                Author.StartsWith(searchString, StringComparison.CurrentCultureIgnoreCase) ||
                LogMessage.ToLower().Contains(searchString);
        }
    }
}