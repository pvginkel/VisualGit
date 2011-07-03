// SharpGit\GitDeleteCommand.cs
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
using System.Linq;
using System.Text;
using NGit.Api;
using System.IO;

namespace SharpGit
{
    internal sealed class GitDeleteCommand : GitCommand<GitDeleteArgs>
    {
        public GitDeleteCommand(GitClient client, GitClientArgs args)
            : base(client, args)
        {
        }

        public void Execute(string path)
        {
            var repositoryEntry = Client.GetRepository(path);

            using (repositoryEntry.Lock())
            {
                var repository = repositoryEntry.Repository;

                if (!Args.KeepLocal && File.Exists(path))
                    File.Delete(path);

                var command = new Git(repository).Rm();

                command.AddFilepattern(repository.GetRepositoryPath(path));

                // DeleteCommand does not have an option to keep the local file
                // to be deleted. Temporarily move it away so the DeleteCommand
                // won't delete it.

                IDisposable moveAwayHandle = null;

                if (Args.KeepLocal && File.Exists(path))
                    moveAwayHandle = MoveAway(path);

                try
                {
                    command.Call();
                }
                finally
                {
                    if (moveAwayHandle != null)
                        moveAwayHandle.Dispose();
                }

                RaiseNotify(new GitNotifyEventArgs
                {
                    Action = GitNotifyAction.Delete,
                    CommandType = Args.CommandType,
                    FullPath = path,
                    NodeKind = GitNodeKind.File,
                    ContentState= GitNotifyState.None
                });
            }
        }
    }
}
