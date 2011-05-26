using System;
using System.Collections.Generic;
using System.Text;

namespace VisualGit.VS
{
    /// <summary>
    /// 
    /// </summary>
    public interface IVisualGitTempFileManager
    {
        /// <summary>
        /// Gets a temporary file 
        /// </summary>
        /// <returns></returns>
        /// <remarks>The file is created as a 0 byte unique file before this function returns
        /// and will be removed after VisualGit exits</remarks>
        string GetTempFile();
        /// <summary>
        /// Gets a temporary file with the specified extension
        /// </summary>
        /// <param name="extension">The extension (with or without a leading period). </param>
        /// <returns></returns>
        /// <remarks>The file is created as a 0 byte unique file before this function returns
        /// and will be removed after VisualGit exits</remarks>
        string GetTempFile(string extension);

        /// <summary>
        /// Gets a temp file with the specified name
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <returns></returns>
        string GetTempFileNamed(string filename);
    }
}
