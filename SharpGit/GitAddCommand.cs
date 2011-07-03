// SharpGit\GitAddCommand.cs
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

namespace SharpGit
{
    internal sealed class GitAddCommand : GitCommand<GitAddArgs>
    {
        public GitAddCommand(GitClient client, GitClientArgs args)
            : base(client, args)
        {
        }

        public void Execute(string path)
        {
            var repositoryEntry = Client.GetRepository(path);

            using (repositoryEntry.Lock())
            {
                var addCommand = new Git(repositoryEntry.Repository).Add();

                addCommand.AddFilepattern(repositoryEntry.Repository.GetRepositoryPath(path));
                addCommand.SetUpdate(Args.Update);

                addCommand.Call();

                RaiseNotify(new GitNotifyEventArgs
                {
                    Action = GitNotifyAction.Add,
                    CommandType = Args.CommandType,
                    ContentState = GitNotifyState.Unknown,
                    FullPath = path,
                    NodeKind = GitNodeKind.File
                });
            }
        }
    }
}
