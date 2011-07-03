// VisualGit\Commands\RepositoryExplorer\ShowRepositoryItemChanges.cs
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
using VisualGit.Scc.UI;
using SharpGit;

namespace VisualGit.Commands.RepositoryExplorer
{
    [Command(VisualGitCommand.RepositoryShowChanges, AlwaysAvailable = true)]
    [Command(VisualGitCommand.RepositoryCompareWithWc, AlwaysAvailable = true)]
    class ShowRepositoryItemChanges : CommandBase
    {
        public override void OnUpdate(CommandUpdateEventArgs e)
        {
            IGitRepositoryItem reposItem = EnumTools.GetSingle(e.Selection.GetSelection<IGitRepositoryItem>());

            if (reposItem != null && reposItem.Origin != null && reposItem.NodeKind != GitNodeKind.Directory
                && reposItem.Revision.RevisionType == GitRevisionType.Hash)
            {
                return;
            }

            e.Enabled = false;
        }

        public override void OnExecute(CommandEventArgs e)
        {
            IVisualGitDiffHandler diff = e.GetService<IVisualGitDiffHandler>();
            IGitRepositoryItem reposItem = EnumTools.GetSingle(e.Selection.GetSelection<IGitRepositoryItem>());

            if (reposItem == null)
                return;

            GitRevision from;
            GitRevision to;
            if (e.Command == VisualGitCommand.RepositoryCompareWithWc)
            {
                from = reposItem.Revision;
                to = GitRevision.Working;
            }
            else
            {
                from = reposItem.Revision - 1;
                to = reposItem.Revision;
            }
            VisualGitDiffArgs da = new VisualGitDiffArgs();

            if (to == GitRevision.Working)
            {
                da.BaseFile = diff.GetTempFile(reposItem.Origin.Target, from, true);

                if (da.BaseFile == null)
                    return; // User canceled

                da.MineFile = reposItem.Origin.Target.FullPath;
            }
            else
            {
                string[] files = diff.GetTempFiles(reposItem.Origin.Target, from, to, true);

                if (files == null)
                    return; // User canceled
                da.BaseFile = files[0];
                da.MineFile = files[1];
                System.IO.File.SetAttributes(da.MineFile, System.IO.FileAttributes.ReadOnly | System.IO.FileAttributes.Normal);
            }

            da.BaseTitle = diff.GetTitle(reposItem.Origin.Target, from);
            da.MineTitle = diff.GetTitle(reposItem.Origin.Target, to);
            diff.RunDiff(da);
        }
    }
}
