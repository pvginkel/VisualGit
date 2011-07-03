// SharpGit\GitRemoteRefsCommand.cs
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
using NGit.Transport;
using NGit;
using NGit.Errors;

namespace SharpGit
{
    internal class GitRemoteRefsCommand : GitTransportCommand<GitRemoteRefsArgs>
    {
        public GitRemoteRefsCommand(GitClient client, GitClientArgs args)
            : base(client, args)
        {
        }

        public GitRemoteRefsResult Execute(string remote, GitRemoteRefType types)
        {
            if (remote == null)
                throw new ArgumentNullException("remote");
            if (types == 0)
                throw new ArgumentOutOfRangeException("types", "Select at least one type");

            var repository = CreateDummyRepository();

            try
            {
                var command = new Git(repository).LsRemote();

                command.SetRemote(remote);

                var result = new GitRemoteRefsResult();

                ICollection<Ref> refs;

                using (new CredentialsProviderScope(new CredentialsProvider(this)))
                {
                    refs = command.Call();
                }

                foreach (var item in refs)
                {
                    result.Items.Add(new GitRef(item));
                }

                return result;
            }
            finally
            {
                repository.Close();
            }
        }
    }
}
