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
    }
}
