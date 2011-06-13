using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    [Serializable]
    public sealed class GitErrorCode
    {
        public GitErrorCategory Category { get; set; }

        public string Message { get; set; }

        private GitErrorCode(GitErrorCategory category, string message)
        {
            if (message == null)
                throw new ArgumentNullException("message");

            Category = category;
            Message = message;
        }

        public static readonly GitErrorCode PathNoRepository = new GitErrorCode(GitErrorCategory.None, Properties.Resources.PathNoRepository);
        public static readonly GitErrorCode CouldNotLock = new GitErrorCode(GitErrorCategory.None, Properties.Resources.CouldNotLock);
        public static readonly GitErrorCode Unspecified = new GitErrorCode(GitErrorCategory.None, Properties.Resources.Unspecified);
        public static readonly GitErrorCode OperationCancelled = new GitErrorCode(GitErrorCategory.None, Properties.Resources.OperationCancelled);
        public static readonly GitErrorCode OutOfDate = new GitErrorCode(GitErrorCategory.None, Properties.Resources.OutOfDate);
        public static readonly GitErrorCode UnexpectedMultipleRepositories = new GitErrorCode(GitErrorCategory.None, Properties.Resources.UnexpectedMultipleRepositories);
        public static readonly GitErrorCode RevisionNotFound = new GitErrorCode(GitErrorCategory.None, Properties.Resources.RevisionNotFound);
        public static readonly GitErrorCode CouldNotFindPathInRevision = new GitErrorCode(GitErrorCategory.None, Properties.Resources.CouldNotFindPathInRevision);
        public static readonly GitErrorCode UnsupportedUriScheme = new GitErrorCode(GitErrorCategory.None, Properties.Resources.UnsupportedUriScheme);
        public static readonly GitErrorCode CheckoutFailed = new GitErrorCode(GitErrorCategory.None, Properties.Resources.CheckoutFailed);
        public static readonly GitErrorCode RepositoryLocked = new GitErrorCode(GitErrorCategory.None, Properties.Resources.RepositoryLocked);
        public static readonly GitErrorCode PushFailed = new GitErrorCode(GitErrorCategory.None, Properties.Resources.PushFailed);
        public static readonly GitErrorCode PullFailed = new GitErrorCode(GitErrorCategory.None, Properties.Resources.PullFailed);
        public static readonly GitErrorCode MoveObstructed = new GitErrorCode(GitErrorCategory.None, Properties.Resources.MoveObstructed);
        public static readonly GitErrorCode BinaryFile = new GitErrorCode(GitErrorCategory.None, Properties.Resources.BinaryFile);
        public static readonly GitErrorCode CloneFailed = new GitErrorCode(GitErrorCategory.None, Properties.Resources.CloneFailed);
        public static readonly GitErrorCode GetRemoteRefsFailed = new GitErrorCode(GitErrorCategory.None, Properties.Resources.GetRemoteRefsFailed);
    }
}
