// SharpGit\GitMergeCommand.cs
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
using NGit.Merge;

namespace SharpGit
{
    internal class GitMergeCommand : GitCommand<GitMergeArgs>
    {
        public GitMergeCommand(GitClient client, GitClientArgs args)
            : base(client, args)
        {
        }

        public void Execute(string repositoryPath, GitRef branch)
        {
            if (branch == null)
                throw new ArgumentNullException("branch");

            MergeCommandResult mergeResult;

            var repositoryEntry = Client.GetRepository(repositoryPath);

            using (repositoryEntry.Lock())
            {
                var repository = repositoryEntry.Repository;

                var command = new Git(repository).Merge();

                switch (Args.Strategy)
                {
                    case GitMergeStrategy.Ours:
                        command.SetStrategy(MergeStrategy.OURS);
                        break;

                    case GitMergeStrategy.Theirs:
                        command.SetStrategy(MergeStrategy.THEIRS);
                        break;

                    case GitMergeStrategy.SimpleTwoWayInCore:
                        command.SetStrategy(MergeStrategy.SIMPLE_TWO_WAY_IN_CORE);
                        break;

                    case GitMergeStrategy.Resolve:
                        command.SetStrategy(MergeStrategy.RESOLVE);
                        break;
                }

                command.Include(repository.Resolve(branch.Name));

                mergeResult = command.Call();

                RaiseNotifyFromDiff(repositoryEntry.Repository);
            }

            if (mergeResult != null)
                RaiseMergeResults(repositoryEntry, mergeResult);
        }
    }
}
