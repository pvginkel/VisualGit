// SharpGit\GitCloneCommand.cs
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

namespace SharpGit
{
    internal class GitCloneCommand : GitTransportCommand<GitCloneArgs>
    {
        public GitCloneCommand(GitClient client, GitClientArgs args)
            : base(client, args)
        {
        }

        internal void Execute(string remote, GitRef @ref, string destination)
        {
            if (remote == null)
                throw new ArgumentNullException("remote");
            if (@ref == null)
                throw new ArgumentNullException("ref");
            if (destination == null)
                throw new ArgumentNullException("destination");

            var command = new CloneCommand();

            command.SetBranch(@ref.Name);
            command.SetProgressMonitor(new ProgressMonitor(this));
            command.SetDirectory(destination);
            command.SetURI(remote);

            using (new CredentialsProviderScope(new CredentialsProvider(this)))
            {
                command.Call();
            }
        }
    }
}
