using System;
using System.Collections.Generic;
using System.Text;
using VisualGit.Selection;

namespace VisualGit
{
    public interface IVisualGitProjectLayoutService
    {
        /// <summary>
        /// Gets all update roots of the current open solution
        /// </summary>
        /// <param name="project">The project specified or <c>null</c> to use all projects</param>
        /// <returns></returns>
        IEnumerable<GitItem> GetUpdateRoots(GitProject project);
    }    
}
