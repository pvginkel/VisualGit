using System;
using System.Collections.Generic;
using System.Text;
using VisualGit.Selection;

namespace VisualGit.Scc
{
    public interface IProjectFileMapper
    {
        /// <summary>
        /// Gets an IEnumerable over all projects containing <paramref name="path"/>
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        IEnumerable<GitProject> GetAllProjectsContaining(string path);
        /// <summary>
        /// Gets an IEnumerable over all projects containing one or more of the specified <paramref name="paths"/>
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        IEnumerable<GitProject> GetAllProjectsContaining(IEnumerable<string> paths);

        /// <summary>
        /// Gets all projects.
        /// </summary>
        /// <returns></returns>
        IEnumerable<GitProject> GetAllProjects();

        /// <summary>
        /// Gets a list of all files contained within <paramref name="project"/>
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        IEnumerable<string> GetAllFilesOf(GitProject project);
        /// <summary>
        /// Gets a list of all files contained within the list of <paramref name="projects"/>
        /// </summary>
        /// <param name="projects"></param>
        /// <returns></returns>
        IEnumerable<string> GetAllFilesOf(ICollection<GitProject> projects);

        /// <summary>
        /// Gets all files of all projects.
        /// </summary>
        /// <returns></returns>
        ICollection<string> GetAllFilesOfAllProjects();

        /// <summary>
        /// Gets a boolean indicating whether one or more projects (or the solution) contains path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        bool ContainsPath(string path);

        /// <summary>
        /// Gets the solution path.
        /// </summary>
        /// <value>The solution path.</value>
        string SolutionFilename { get; }

        /// <summary>
        /// Gets a boolean indicating whether the specified path is of a project or the solution
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        bool IsProjectFileOrSolution(string path);

        /// <summary>
        /// Gets the project info.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <returns></returns>
        IGitProjectInfo GetProjectInfo(GitProject project);

        /// <summary>
        /// Gets the icon of the file in the first project containing the file
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        ProjectIconReference GetPathIconHandle(string path);
    }
}
