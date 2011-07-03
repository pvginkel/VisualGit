// VisualGit\Commands\ItemResolveCasing.cs
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
using VisualGit.Scc;
using System.IO;
using Microsoft.VisualStudio.Shell;
using SharpGit;

namespace VisualGit.Commands
{
    [Command(VisualGitCommand.ItemResolveCasing)]
    class ItemResolveCasing : CommandBase
    {
        public override void OnUpdate(CommandUpdateEventArgs e)
        {
            foreach (GitItem item in e.Selection.GetSelectedGitItems(false))
            {
                if (item.IsCasingConflicted)
                {
                    // Ok, something we can fix!
                    return;
                }
            }

            e.Enabled = false;
        }

        public override void OnExecute(CommandEventArgs e)
        {
            List<GitItem> toResolve = new List<GitItem>();

            foreach (GitItem item in e.Selection.GetSelectedGitItems(false))
            {
                if (item.IsCasingConflicted)
                {
                    toResolve.Add(item);
                }
            }
            try
            {
                foreach (GitItem item in toResolve)
                {
                    string svnPath = GetGitCasing(e, item);
                    string actualPath = GitTools.GetTruePath(item.FullPath);

                    if (svnPath == null || actualPath == null)
                        continue; // not found
                    if (!string.Equals(svnPath, actualPath, StringComparison.OrdinalIgnoreCase))
                        continue; // More than casing rename

                    string svnName = Path.GetFileName(svnPath);
                    string actualName = Path.GetFileName(actualPath);

                    if (svnName == actualName)
                        continue; // Can't fix directories!

                    IVisualGitOpenDocumentTracker odt = e.GetService<IVisualGitOpenDocumentTracker>();
                    using (odt.LockDocument(svnPath, DocumentLockType.NoReload))
                    using (odt.LockDocument(actualPath, DocumentLockType.NoReload))
                    {
                        try
                        {
                            // Try the actual rename
                            File.Move(actualPath, svnPath);
                        }
                        catch { }

                        try
                        {
                            // And try to fix the project+document system
                            VsShellUtilities.RenameDocument(e.Context, actualPath, svnPath);
                        }
                        catch
                        { }
                    }
                }
            }
            finally
            {
                e.GetService<IFileStatusMonitor>().ScheduleGitStatus(GitItem.GetPaths(toResolve));
            }
        }

        static string GetGitCasing(CommandEventArgs e, GitItem item)
        {
            string name = null;
            // Find the correct casing
            using (GitClient client = e.GetService<IGitClientPool>().GetNoUIClient())
            {
                GitStatusArgs args = new GitStatusArgs();

                args.Depth = GitDepth.Files;
                args.RetrieveAllEntries = false;
                args.RetrieveIgnoredEntries = false;
                args.ThrowOnCancel = false;
                args.ThrowOnError = false;

                client.Status(item.Directory, args,
                    delegate(object sender, GitStatusEventArgs ea)
                    {
                        if (string.Equals(ea.FullPath, item.FullPath, StringComparison.OrdinalIgnoreCase))
                        {
                            name = ea.FullPath;
                        }
                    });
            }

            return name;
        }

    }
}
