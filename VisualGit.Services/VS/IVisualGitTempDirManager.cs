using System;
using System.Collections.Generic;
using System.Text;

namespace VisualGit.VS
{
    public interface IVisualGitTempDirManager
    {
        /// <summary>
        /// Gets a temporary directory
        /// </summary>
        /// <returns></returns>
        /// <remarks>The directory is created.</remarks>
        string GetTempDir();
    }
}
