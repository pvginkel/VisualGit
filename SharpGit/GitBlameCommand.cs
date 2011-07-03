// SharpGit\GitBlameCommand.cs
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
using NGit.Diff;
using NGit.Revwalk;

namespace SharpGit
{
    internal class GitBlameCommand : GitCommand<GitBlameArgs>
    {
        public GitBlameCommand(GitClient client, GitClientArgs args)
            : base(client, args)
        {
        }

        public GitBlameResult Execute(GitTarget target)
        {
            if (FileSystemUtil.FileIsBinary(target.FullPath))
                throw new GitClientBinaryFileException();

            var repositoryEntry = Client.GetRepository(target.FullPath);

            using (repositoryEntry.Lock())
            {
                var repository = repositoryEntry.Repository;

                var command = new Git(repository).Blame();

                command.SetFilePath(repository.GetRepositoryPath(target.FullPath));
                command.SetFollowFileRenames(true);

                if (Args.End != null)
                    command.SetStartCommit(Args.End.GetObjectId(repository));

                switch (Args.IgnoreSpacing)
                {
                    case GitIgnoreSpacing.IgnoreAll:
                        command.SetTextComparator(RawTextComparator.WS_IGNORE_ALL);
                        break;

                    case GitIgnoreSpacing.IgnoreSpace:
                        command.SetTextComparator(IgnoreSpaceTextComparator.Instance);
                        break;
                }

                var result = command.Call();

                var commandResult = new GitBlameResult();
                var resultContents = result.GetResultContents();

                for (int i = 0, count = resultContents.Size(); i < count; i++)
                {
                    var commit = result.GetSourceCommit(i);

                    commandResult.Items.Add(new GitBlameEventArgs
                    {
                        Author = result.GetSourceAuthor(i).GetName(),
                        LineNumber = i,
                        Line = resultContents.GetString(i),
                        Revision = commit.Name,
                        Time = GitTools.CreateDateFromGitTime(commit.CommitTime)
                    });
                }

                return commandResult;
            }
        }
    }
}
