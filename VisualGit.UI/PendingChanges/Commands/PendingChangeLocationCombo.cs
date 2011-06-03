using System;
using System.Collections.Generic;
using System.Text;
using VisualGit.Commands;
using VisualGit.VS;
using System.ComponentModel;
using System.ComponentModel.Design;
using SharpGit;

namespace VisualGit.UI.PendingChanges.Commands
{
    [Command(VisualGitCommand.SolutionSwitchComboFill)]
    [Command(VisualGitCommand.SolutionSwitchCombo)]
    class PendingChangeLocationCombo : ICommandHandler, IComponent
    {
        ISite _site;
        IVisualGitSolutionSettings _settings;
        #region IComponent Members

        event EventHandler IComponent.Disposed
        {
            add { }
            remove { }
        }

        public ISite Site
        {
            get { return _site; }
            set
            {
                _site = value;
            }
        }

        public void Dispose()
        {
            
        }

        #endregion


        public void OnUpdate(CommandUpdateEventArgs e)
        {
            if (ProjectRootUri == null)
                e.Enabled = false;
        }

        public void OnExecute(CommandEventArgs e)
        {
            // For combobox processing we have 4 cases
            
            if (e.Command == VisualGitCommand.SolutionSwitchComboFill)
                OnExecuteFill(e); // Fill the list of options
            else if (e.Argument != null)
            {
                string value = e.Argument as string;

                if (value != null)
                    OnExecuteSet(e); // When the user selected a value
                else
                    OnExecuteFilter(e); // Keyboard hook filter (selected in ctc)
            }
            else
                OnExecuteGet(e); // Get the current value
        }

        protected IVisualGitSolutionSettings SolutionSettings
        {
            get
            {
                if (_settings == null && _site != null)
                    _settings = (IVisualGitSolutionSettings)_site.GetService(typeof(IVisualGitSolutionSettings));

                return _settings;
            }
        }


        protected Uri ProjectRootUri
        {
            get
            {
                if (SolutionSettings != null)
                    return SolutionSettings.ProjectRootUri;

                return null;
            }
        }


        void OnExecuteFill(CommandEventArgs e)
        {
            if (ProjectRootUri != null)
            {
                var branches = new List<string>();

                using (var client = e.Context.GetService<IGitClientPool>().GetNoUIClient())
                {
                    string repositoryPath = RepositoryUtil.GetRepositoryRoot(ProjectRootUri);

                    var args = new GitListBranchArgs();
                    GitListBranchResult result;

                    if (client.ListBranch(repositoryPath, args, out result))
                    {
                        var repositoryBranch = RepositoryUtil.GetCurrentBranch(repositoryPath);
                        
                        //branches.Add(repositoryBranch.ShortName);

                        foreach (var branch in result.Branches)
                        {
                            //if (branch != repositoryBranch)
                                branches.Add(branch.ShortName);
                        }
                    }
                }

                e.Result = branches.ToArray();
            }
        }

        void OnExecuteSet(CommandEventArgs e)
        {
            string value = (string)e.Argument;

            IVisualGitCommandService cs = e.GetService<IVisualGitCommandService>();

            if (value == null)
                cs.PostExecCommand(VisualGitCommand.SolutionSwitchDialog);
            else
                cs.PostExecCommand(VisualGitCommand.SolutionSwitchDialog, value);
        }

        void OnExecuteGet(CommandEventArgs e)
        {
            if (ProjectRootUri != null)
            {
                string repositoryPath = RepositoryUtil.GetRepositoryRoot(ProjectRootUri);
                var repositoryBranch = RepositoryUtil.GetCurrentBranch(repositoryPath);
                e.Result = repositoryBranch.ShortName;
            }
        }

        void OnExecuteFilter(CommandEventArgs e)
        {
            // Not called on us; but empty handler would tell: pass through
        }
    }
}
