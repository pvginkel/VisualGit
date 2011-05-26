using System;
using System.Collections.Generic;
using VisualGit.ExtensionPoints.IssueTracker;
using VisualGit.Scc;
using VisualGit.VS;

namespace VisualGit
{
    public interface IVisualGitIssueService
    {
        /// <summary>
        /// Gets all the registered Issue repository connectors.
        /// </summary>
        /// <remarks>This call DOES NOT trigger connector package initialization.</remarks>
        ICollection<IssueRepositoryConnector> Connectors { get; }

        /// <summary>
        /// Tries to find a registered connector with the given name.
        /// </summary>
        /// <remarks>This call DOES NOT trigger connector package initialization.</remarks>
        bool TryGetConnector(string name, out IssueRepositoryConnector connector);

        /// <summary>
        /// Gets the issue repository settings associated with the current solution.
        /// </summary>
        IssueRepositorySettings CurrentIssueRepositorySettings { get; }

        /// <summary>
        /// Gets or Sets the issue repository associated with the current solution.
        /// </summary>
        IssueRepository CurrentIssueRepository { get; set; }

        /// <summary>
        /// Occurs when current solution's Issue Tracker Repository association settings are changed
        /// </summary>
        event EventHandler IssueRepositoryChanged;

        /// <summary>
        /// Marks Issue Service as dirty which signals Issues page to refresh itself.
        /// </summary>
        void MarkDirty();

        /// <summary>
        /// Gets the issue references from the specified text
        /// </summary>
        /// <param name="logmessage">text.</param>
        /// <returns></returns>
        /// <remarks>Precondition: Current solution is associated with a repository.</remarks>
        bool TryGetIssues(string text, out IEnumerable<IssueMarker> issues);

        /// <summary>
        /// Passes the open request to the current issue repository
        /// </summary>
        /// <param name="issueId"></param>
        void OpenIssue(string issueId);

		void ShowConnectHelp();
	}
}
