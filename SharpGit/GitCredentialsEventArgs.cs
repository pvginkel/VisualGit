// SharpGit\GitCredentialsEventArgs.cs
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
using NGit.Transport;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace SharpGit
{
    public class GitCredentialsEventArgs : CancelEventArgs
    {
        internal GitCredentialsEventArgs(string uri, CredentialItem[] credentialItems)
        {
            if (credentialItems == null)
                throw new ArgumentNullException("credentialItems");

            var items = new List<GitCredentialItem>();

            foreach (var item in credentialItems)
            {
                items.Add(new GitCredentialItem(item));
            }

            Items = new ReadOnlyCollection<GitCredentialItem>(items);

            Uri = uri;
        }

        public string Uri { get; private set; }

        public ICollection<GitCredentialItem> Items { get; private set; }
    }
}
