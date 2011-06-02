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
    }
}
