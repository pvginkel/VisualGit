using VisualGit.Commands;
using VisualGit.ExtensionPoints.IssueTracker;
using VisualGit.VS;
using VisualGit.Scc;
using VisualGit.IssueTracker;
using System.Collections.Generic;

namespace VisualGit.UI.IssueTracker
{
    [Command(VisualGitCommand.SolutionIssueTrackerSetup)]
    class IssueTrackerSetupCommand : ICommandHandler
    {
        #region ICommandHandler Members

        public void OnUpdate(CommandUpdateEventArgs e)
        {
            IVisualGitIssueService service = null;
            GitItem item = null;
            e.Enabled = true
                && (item = GetRoot(e)) != null
                && item.IsVersioned // ensure solution (project root) is versioned
                && (service = e.GetService<IVisualGitIssueService>())!= null
                && service.Connectors != null
                && service.Connectors.Count > 0;
        }

        public void OnExecute(CommandEventArgs e)
        {
            GitItem firstVersioned = null;
            IFileStatusCache cache = e.GetService<IFileStatusCache>();
            IVisualGitSolutionSettings solutionSettings = e.GetService<IVisualGitSolutionSettings>();
            if (solutionSettings != null)
            {
                firstVersioned = cache[solutionSettings.ProjectRoot];
            }

            if (firstVersioned == null)
                return; // exceptional case

            using (IssueTrackerConfigDialog dialog = new IssueTrackerConfigDialog(e.Context))
            {
                if (dialog.ShowDialog(e.Context) == System.Windows.Forms.DialogResult.OK)
                {
                    IIssueTrackerSettings currentSettings = e.GetService<IIssueTrackerSettings>();

                    IssueRepository newRepository = dialog.NewIssueRepository;
                    if (newRepository == null
                        || string.IsNullOrEmpty(newRepository.ConnectorName)
                        || newRepository.RepositoryUri == null)
                    {
                        DeleteIssueRepositoryProperties(e.Context, firstVersioned);
                    }
                    else if (currentSettings == null
                        || currentSettings.ShouldPersist(newRepository))
                    {
                        SetIssueRepositoryProperties(e.Context, firstVersioned, newRepository);
                    }
                }
            }
        }

        #endregion

        private bool DeleteIssueRepositoryProperties(VisualGitContext context, GitItem item)
        {
            return context.GetService<IProgressRunner>().RunModal("Removing Issue Repository settings",
                delegate(object sender, ProgressWorkerArgs wa)
                {
                    wa.SvnClient.DeleteProperty(item.FullPath, VisualGitSccPropertyNames.IssueRepositoryConnector);
                    wa.SvnClient.DeleteProperty(item.FullPath, VisualGitSccPropertyNames.IssueRepositoryUri);
                    wa.SvnClient.DeleteProperty(item.FullPath, VisualGitSccPropertyNames.IssueRepositoryId);
                    wa.SvnClient.DeleteProperty(item.FullPath, VisualGitSccPropertyNames.IssueRepositoryPropertyNames);
                    wa.SvnClient.DeleteProperty(item.FullPath, VisualGitSccPropertyNames.IssueRepositoryPropertyValues);
                }).Succeeded;
        }

        private bool SetIssueRepositoryProperties(VisualGitContext context, GitItem item, IssueRepositorySettings settings)
        {
            return context.GetService<IProgressRunner>().RunModal("Applying Issue Repository settings",
                delegate(object sender, ProgressWorkerArgs wa)
                {
                    wa.SvnClient.SetProperty(item.FullPath, VisualGitSccPropertyNames.IssueRepositoryConnector, settings.ConnectorName);
                    wa.SvnClient.SetProperty(item.FullPath, VisualGitSccPropertyNames.IssueRepositoryUri, settings.RepositoryUri.ToString());
                    string repositoryId = settings.RepositoryId;
                    if (string.IsNullOrEmpty(repositoryId))
                    {
                        wa.SvnClient.DeleteProperty(item.FullPath, VisualGitSccPropertyNames.IssueRepositoryId);
                    }
                    else
                    {
                        wa.SvnClient.SetProperty(item.FullPath, VisualGitSccPropertyNames.IssueRepositoryId, settings.RepositoryId);
                    }
                    IDictionary<string, object> customProperties = settings.CustomProperties;
                    if (customProperties == null
                        || customProperties.Count == 0
                        )
                    {
                        wa.SvnClient.DeleteProperty(item.FullPath, VisualGitSccPropertyNames.IssueRepositoryPropertyNames);
                        wa.SvnClient.DeleteProperty(item.FullPath, VisualGitSccPropertyNames.IssueRepositoryPropertyValues);
                    }
                    else
                    {
                        string[] propNameArray = new string[customProperties.Keys.Count];
                        customProperties.Keys.CopyTo(propNameArray, 0);
                        string propNames = string.Join(",", propNameArray);

                        List<string> propValueList = new List<string>();
                        foreach (string propName in propNameArray)
                        {
                            object propValue;
                            if (!customProperties.TryGetValue(propName, out propValue))
                            {
                                propValue = string.Empty;
                            }
                            propValueList.Add(propValue == null ? string.Empty : propValue.ToString());
                        }
                        string propValues = string.Join(",", propValueList.ToArray());
                        wa.SvnClient.SetProperty(item.FullPath, VisualGitSccPropertyNames.IssueRepositoryPropertyNames, propNames);
                        wa.SvnClient.SetProperty(item.FullPath, VisualGitSccPropertyNames.IssueRepositoryPropertyValues, propValues);
                    }

                }).Succeeded;
        }

        /// <summary>
        /// Gets the "project root"
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private static GitItem GetRoot(BaseCommandEventArgs e)
        {
            GitItem item = null;
            switch (e.Command)
            {
                case VisualGitCommand.SolutionIssueTrackerSetup:
                    IVisualGitSolutionSettings ss = e.GetService<IVisualGitSolutionSettings>();
                    if (ss == null)
                        return null;

                    string root = ss.ProjectRoot;

                    if (string.IsNullOrEmpty(root))
                        return null;

                    item = e.GetService<IFileStatusCache>()[root];
                    break;
            }

            return item;
        }
    }
}
