// SharpGit\GitPushCommand.cs
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
using NGit.Api.Errors;
using NGit;

namespace SharpGit
{
    internal class GitPushCommand : GitTransportCommand<GitPushArgs>
    {
        public GitPushCommand(GitClient client, GitClientArgs args)
            : base(client, args)
        {
        }

        internal void Execute(string repositoryPath)
        {
            var repositoryEntry = Client.GetRepository(repositoryPath);

            using (repositoryEntry.Lock())
            {
                Repository repository = null;

                try
                {
                    repository = repositoryEntry.Repository;

                    var pushCommand = new Git(repository).Push();

                    if (Args.AllBranches || Args.AllTags)
                    {
                        if (Args.AllBranches)
                            pushCommand.SetPushAll();
                        if (Args.AllTags)
                            pushCommand.SetPushTags();
                    }
                    else if (Args.LocalBranch == null && Args.Tag == null)
                    {
                        pushCommand.Add(repository.GetFullBranch());
                    }
                    else
                    {
                        if (Args.LocalBranch != null)
                            pushCommand.Add(Args.LocalBranch.ShortName);
                        if (Args.Tag != null)
                            pushCommand.Add(Args.Tag.ShortName);
                    }

                    pushCommand.SetForce(Args.Force);

                    if (Args.Remote != null)
                        pushCommand.SetRemote(Args.Remote);
                    else if (Args.RemoteUri != null)
                        pushCommand.SetRemote(Args.RemoteUri);

                    pushCommand.SetProgressMonitor(new ProgressMonitor(this));

                    using (new CredentialsProviderScope(new CredentialsProvider(this)))
                    {
                        pushCommand.Call();
                    }
                }
                finally
                {
                    if (repository != null)
                        repository.Close();
                }
            }
        }
    }
}
