using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VisualGit.Scc;
using System.ComponentModel;
using SharpGit;

namespace VisualGit.UI.GitLog.RevisionGrid
{
    class RevisionItem : VisualGitPropertyGridItem, IGitLogItem
    {
        private GitRevision _revision;

        public RevisionItem(GitRevision revision)
        {
            if (revision == null)
                throw new ArgumentNullException("revision");

            _revision = revision;
        }

        [DisplayName("Author")]
        [Description("Author of the revision")]
        public string Author
        {
            get { return _revision.Author + " <" + _revision.AuthorEmail + ">"; }
        }

        [DisplayName("Date")]
        [Description("Date the revision was committed")]
        public DateTime AuthorDate
        {
            get { return _revision.AuthorDate; }
        }

        [DisplayName("Log Message")]
        [Description("Log message associated with the revision")]
        public string LogMessage
        {
            get { return _revision.LogMessage; }
        }

        [DisplayName("Revision")]
        [Description("Unique hash of the revision")]
        public string Revision
        {
            get { return _revision.Revision; }
        }

        [DisplayName("Repository Root")]
        [Description("Root directory of the repository of the revision")]
        public string RepositoryRoot
        {
            get { return _revision.RepositoryRoot; }
        }

        protected override string ClassName
        {
            get { return "Revision"; }
        }

        protected override string ComponentName
        {
            get { return _revision.Revision; }
        }

        DateTime IGitLogItem.CommitDate
        {
            get { return _revision.CommitDate; }
        }

        IList<string> IGitLogItem.ParentRevisions
        {
            get { return _revision.ParentRevisions; }
        }

        int IGitLogItem.Index
        {
            get { return _revision.Index; }
        }

        GitChangeItemCollection IGitLogItem.ChangedPaths
        {
            get { return null; }
        }
    }
}
