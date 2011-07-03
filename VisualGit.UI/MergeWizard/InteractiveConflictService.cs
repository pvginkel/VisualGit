// VisualGit.UI\MergeWizard\InteractiveConflictService.cs
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
using VisualGit.Scc;
using System.ComponentModel;
using VisualGit.Configuration;
using SharpGit;

namespace VisualGit.UI.MergeWizard
{
    [GlobalService(typeof(IConflictHandler))]
    class InteractiveConflictService : VisualGitService, IConflictHandler
    {
        public InteractiveConflictService(IVisualGitServiceProvider context)
            : base(context)
        {
        }
        #region IConflictHandler Members

        public void RegisterConflictHandler(IGitConflictsClientArgs args, System.ComponentModel.ISynchronizeInvoke synch)
        {
            args.Conflict += new EventHandler<GitConflictEventArgs>(new Handler(this, synch).OnConflict);
        }

        #endregion

        class Handler : VisualGitService
        {
            ISynchronizeInvoke _synchronizer;
            MergeConflictHandler _currentMergeConflictHandler;

            public Handler(IVisualGitServiceProvider context, ISynchronizeInvoke synchronizer)
                : base(context)
            {
                _synchronizer = synchronizer;
            }

            public void OnConflict(object sender, GitConflictEventArgs e)
            {
                if (_synchronizer != null && _synchronizer.InvokeRequired)
                {
                    // If needed marshall the call to the UI thread

                    _synchronizer.Invoke(new EventHandler<GitConflictEventArgs>(OnConflict), new object[] { sender, e });
                    return;
                }

                VisualGitConfig config = GetService<IVisualGitConfigurationService>().Instance;

                if (config.InteractiveMergeOnConflict)
                {
                    // Only call interactive merge if the user opted in on it
                    if (_currentMergeConflictHandler == null)
                        _currentMergeConflictHandler = CreateMergeConflictHandler();

                    _currentMergeConflictHandler.OnConflict(e);
                }
            }

            private MergeConflictHandler CreateMergeConflictHandler()
            {
                MergeConflictHandler mergeConflictHandler = new MergeConflictHandler(Context);
                mergeConflictHandler.PromptOnBinaryConflict = true;
                mergeConflictHandler.PromptOnTextConflict = true;
                return mergeConflictHandler;
            }
        }
    }
}
