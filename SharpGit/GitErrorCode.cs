using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    [Serializable]
    public sealed class GitErrorCode
    {
        public string Message { get; set; }

        private GitErrorCode(string message)
        {
            if (message == null)
                throw new ArgumentNullException("message");

            Message = message;
        }

        public static readonly GitErrorCode PathNoRepository = new GitErrorCode(Properties.Resources.PathNoRepository);
        public static readonly GitErrorCode CouldNotLock = new GitErrorCode(Properties.Resources.CouldNotLock);
        public static readonly GitErrorCode Unspecified = new GitErrorCode(Properties.Resources.Unspecified);
        public static readonly GitErrorCode OperationCancelled = new GitErrorCode(Properties.Resources.OperationCancelled);
        public static readonly GitErrorCode OutOfDate = new GitErrorCode(Properties.Resources.OutOfDate);
        public static readonly GitErrorCode UnexpectedMultipleRepositories = new GitErrorCode(Properties.Resources.UnexpectedMultipleRepositories);
        public static readonly GitErrorCode RevisionNotFound = new GitErrorCode(Properties.Resources.RevisionNotFound);
        public static readonly GitErrorCode CouldNotFindPathInRevision = new GitErrorCode(Properties.Resources.CouldNotFindPathInRevision);
        public static readonly GitErrorCode UnsupportedUriScheme = new GitErrorCode(Properties.Resources.UnsupportedUriScheme);
        public static readonly GitErrorCode CheckoutFailed = new GitErrorCode(Properties.Resources.CheckoutFailed);
        public static readonly GitErrorCode RepositoryLocked = new GitErrorCode(Properties.Resources.RepositoryLocked);
        public static readonly GitErrorCode PushFailed = new GitErrorCode(Properties.Resources.PushFailed);
        public static readonly GitErrorCode PullFailed = new GitErrorCode(Properties.Resources.PullFailed);
        public static readonly GitErrorCode MoveObstructed = new GitErrorCode(Properties.Resources.MoveObstructed);
        public static readonly GitErrorCode BinaryFile = new GitErrorCode(Properties.Resources.BinaryFile);
        public static readonly GitErrorCode CloneFailed = new GitErrorCode(Properties.Resources.CloneFailed);
        public static readonly GitErrorCode GetRemoteRefsFailed = new GitErrorCode(Properties.Resources.GetRemoteRefsFailed);
        public static readonly GitErrorCode RevertFailed = new GitErrorCode(Properties.Resources.RevertFailed);
    }
}
