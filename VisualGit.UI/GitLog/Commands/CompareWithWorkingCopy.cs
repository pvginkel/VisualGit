// VisualGit.UI\GitLog\Commands\CompareWithWorkingCopy.cs
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
using VisualGit.Scc;
using VisualGit.Scc.UI;
using SharpGit;

namespace VisualGit.UI.GitLog.Commands
{
    [Command(VisualGitCommand.LogCompareWithWorkingCopy, AlwaysAvailable = true)]
    public class CompareWithWorkingCopy : ICommandHandler
    {
        public void OnUpdate(CommandUpdateEventArgs e)
        {
            IGitLogItem item = EnumTools.GetSingle(e.Selection.GetSelection<IGitLogItem>());

            if (item != null)
            {
                ILogControl logWindow = e.Selection.GetActiveControl<ILogControl>();

                if (logWindow != null)
                {
                    GitOrigin origin = EnumTools.GetSingle(logWindow.Origins);

                    if (origin != null)
                    {
                        GitItem gitItem = e.GetService<IFileStatusCache>()[origin.Target.FullPath];

                        if (gitItem != null && !gitItem.IsDirectory)
                        {
                            if (null == e.Selection.GetActiveControl<ILogControl>())
                                e.Enabled = false;

                            return;
                        }
                    }
                }
            }

            e.Enabled = false;
        }

        public void OnExecute(CommandEventArgs e)
        {
            // All checked in OnUpdate            
            ILogControl logWindow = e.Selection.GetActiveControl<ILogControl>();
            GitOrigin origin = EnumTools.GetSingle(logWindow.Origins);
            IGitLogItem item = EnumTools.GetSingle(e.Selection.GetSelection<IGitLogItem>());

            IVisualGitDiffHandler diff = e.GetService<IVisualGitDiffHandler>();

            VisualGitDiffArgs da = new VisualGitDiffArgs();

            da.BaseFile = diff.GetTempFile(origin.Target, item.Revision, true);
            if (da.BaseFile == null)
                return; // User cancel
            da.MineFile = origin.Target.FullPath;
            da.BaseTitle = string.Format("Base (r{0})", item.Revision);
            da.MineTitle = "Working";

            diff.RunDiff(da);
        }
    }
}
