// VisualGit.UI\PendingChanges\Commands\PendingChangeLocationCombo.cs
//
// Copyright 2008-2011 The AnkhSVN Project
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//
// Changes and additions made for VisualGit Copyright 2011 Pieter van Ginkel.

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
            if (ProjectRoot == null)
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


        protected string ProjectRoot
        {
            get
            {
                if (SolutionSettings != null)
                    return SolutionSettings.ProjectRoot;

                return null;
            }
        }


        void OnExecuteFill(CommandEventArgs e)
        {
            if (ProjectRoot != null)
            {
                var branches = new List<string>();

                using (var client = e.Context.GetService<IGitClientPool>().GetNoUIClient())
                {
                    try
                    {
                        string repositoryPath = GitTools.GetRepositoryRoot(ProjectRoot);

                        foreach (var @ref in client.GetRefs(repositoryPath))
                        {
                            switch (@ref.Type)
                            {
                                case GitRefType.Branch:
                                    branches.Add(@ref.ShortName);
                                    break;
                            }
                        }
                    }
                    catch (GitNoRepositoryException)
                    {
                        // Ignore no repository exceptions.
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
            if (ProjectRoot != null)
            {
                try
                {
                    string repositoryPath = GitTools.GetRepositoryRoot(ProjectRoot);

                    using (var client = e.GetService<IGitClientPool>().GetNoUIClient())
                    {
                        var repositoryBranch = client.GetCurrentBranch(repositoryPath);

                        if (repositoryBranch.Type == GitRefType.Unknown)
                            e.Result = Properties.Resources.NoBranch;
                        else
                            e.Result = repositoryBranch.ShortName;
                    }
                }
                catch (GitNoRepositoryException)
                {
                    // Ignore no repository exceptions.
                }
            }
        }

        void OnExecuteFilter(CommandEventArgs e)
        {
            // Not called on us; but empty handler would tell: pass through
        }
    }
}
