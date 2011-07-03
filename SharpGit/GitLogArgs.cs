// SharpGit\GitLogArgs.cs
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
using System.Collections.ObjectModel;

namespace SharpGit
{
    public sealed class GitLogArgs : GitClientArgs
    {
        public GitLogArgs()
            : base(GitCommandType.Log)
        {
        }

        public bool RetrieveMergedRevisions { get; set; }
        public bool StrictNodeHistory { get; set; }
        public int Limit { get; set; }
        public GitRevision End { get; set; }
        public GitRevision Start { get; set; }
        public GitRevision OperationalRevision { get; set; }
        public bool RetrieveChangedPaths { get; set; }

        internal void OnLog(GitLogEventArgs e)
        {
            var ev = Log;

            if (ev != null)
                ev(this, e);
        }

        public event EventHandler<GitLogEventArgs> Log;
    }
}
