// VisualGit.UI\GitLog\Commands\ShowChanges.cs
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
using System.IO;
using System.CodeDom.Compiler;
using Microsoft.VisualStudio.Shell.Interop;
using VisualGit.Scc.UI;
using VisualGit.Scc;
using SharpGit;

namespace VisualGit.UI.GitLog.Commands
{
    [Command(VisualGitCommand.LogShowChanges, AlwaysAvailable = true)]
    class ShowChanges : ICommandHandler
    {
        public void OnUpdate(CommandUpdateEventArgs e)
        {
            ILogControl logWindow = e.Selection.GetActiveControl<ILogControl>();

            if (logWindow == null || logWindow.Origins == null)
            {
                e.Enabled = false;
                return;
            }

            if (UpdateForChangedFiles(e))
                return;

            UpdateForRevChanges(logWindow, e);
        }

        void UpdateForRevChanges(ILogControl logWindow, CommandUpdateEventArgs e)
        {
            GitOrigin first = EnumTools.GetSingle(logWindow.Origins);

            if (first == null)
            {
                e.Enabled = false;
                return;
            }

            if (e.GetService<IFileStatusCache>()[first.Target.FullPath].IsDirectory)
            {
                // We can't diff directories at this time
                e.Enabled = false;
                return;
            }

            // Note: We can't have a local directory, but we can have a remote one.
        }

        bool UpdateForChangedFiles(CommandUpdateEventArgs e)
        {
            IGitLogChangedPathItem change = EnumTools.GetSingle(e.Selection.GetSelection<IGitLogChangedPathItem>());

            if (change == null)
                return false;

            // Skip all the files we cannot diff
            switch (change.Action)
            {
                case GitChangeAction.Add:
                    if (change.OldRevision != null)
                        break; // We can retrieve this file using OldPath
                    e.Enabled = false;
                    break;
                case GitChangeAction.Delete:
                    e.Enabled = false;
                    break;
            }

            if (change.NodeKind == GitNodeKind.Directory)
                e.Enabled = false;

            return true;
        }

        public void OnExecute(CommandEventArgs e)
        {
            ILogControl logWindow = e.Selection.GetActiveControl<ILogControl>();

            if (PerformRevisionChanges(logWindow, e))
                return;

            PerformFileChanges(e);
        }

        bool PerformRevisionChanges(ILogControl log, CommandEventArgs e)
        {
            GitRevision startRevision = null;
            var startRevisionTime = DateTime.MaxValue;
            GitRevision endRevision = null;
            var endRevisionTime = DateTime.MinValue;

            int n = 0;

            HybridCollection<string> changedPaths = new HybridCollection<string>();
            foreach (IGitLogItem item in e.Selection.GetSelection<IGitLogItem>())
            {
                if (startRevisionTime > item.CommitDate)
                {
                    startRevisionTime = item.CommitDate;
                    startRevision = item.Revision;
                }
                if (endRevisionTime < item.CommitDate)
                {
                    endRevisionTime = item.CommitDate;
                    endRevision = item.Revision;
                }
                n++;
            }

            if (n > 0)
            {
                ExecuteDiff(e, log.Origins, new GitRevisionRange(n == 1 ? startRevision - 1 : startRevision, endRevision));
                return true;
            }

            return false;
        }

        void PerformFileChanges(CommandEventArgs e)
        {
            IGitLogChangedPathItem item = EnumTools.GetSingle(e.Selection.GetSelection<IGitLogChangedPathItem>());

            if (item != null)
            {
                switch (item.Action)
                {
                    case GitChangeAction.Delete:
                        return;
                    case GitChangeAction.Add:
                        if (item.OldRevision == null)
                            return;
                        break;
                }

                ExecuteDiff(e, new GitOrigin[] { item.Origin }, new GitRevisionRange((GitRevision)item.Revision - 1, item.Revision));
            }
        }

        void ExecuteDiff(CommandEventArgs e, ICollection<GitOrigin> targets, GitRevisionRange range)
        {
            if (targets.Count != 1)
                return;

            var target = EnumTools.GetSingle(targets);
            GitTarget diffTarget = target.Target;

            IVisualGitDiffHandler diff = e.GetService<IVisualGitDiffHandler>();
            VisualGitDiffArgs da = new VisualGitDiffArgs();

            string[] files = diff.GetTempFiles(diffTarget, range.StartRevision, range.EndRevision, true);

            if (files == null)
                return;

            da.BaseFile = files[0];
            da.MineFile = files[1];
            da.BaseTitle = diff.GetTitle(diffTarget, range.StartRevision);
            da.MineTitle = diff.GetTitle(diffTarget, range.EndRevision);
            da.ReadOnly = true;
            diff.RunDiff(da);
        }
    }

    public static class LogHelper
    {
        public static IEnumerable<GitItem> IntersectWorkingCopyItemsWithChangedPaths(IEnumerable<GitItem> workingCopyItems, IEnumerable<string> changedPaths)
        {
            foreach (GitItem i in workingCopyItems)
            {
                foreach (string s in changedPaths)
                {
                    if (i.FullPath.ToString().EndsWith(s))
                    {
                        yield return i;
                        break;
                    }
                }
            }
        }
    }
}
