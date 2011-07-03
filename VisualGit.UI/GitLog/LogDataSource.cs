// VisualGit.UI\GitLog\LogDataSource.cs
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
using System.ComponentModel;
using VisualGit.Scc;
using SharpGit;

namespace VisualGit.UI.GitLog
{
    internal partial class LogDataSource : Component
    {
        public LogDataSource()
        {
            InitializeComponent();
        }

        public LogDataSource(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        ISynchronizeInvoke _synchronizer;
        public ISynchronizeInvoke Synchronizer
        {
            get { return _synchronizer; }
            set { _synchronizer = value; }
        }

        ICollection<GitOrigin> _targets;
        GitOrigin _mergeTarget;
        public ICollection<GitOrigin> Targets
        {
            get { return _targets; }
            set { _targets = value; }
        }

        public GitOrigin MergeTarget
        {
            get { return _mergeTarget; }
            set { _mergeTarget = value; }
        }

        public string RepositoryRoot
        {
            get
            {
                GitOrigin o = EnumTools.GetFirst(Targets);
                if (o != null)
                    return o.RepositoryRoot;
                
                return null;
            }
        }

        GitRevision _start, _end;
        public GitRevision Start
        {
            get { return _start; }
            set { _start = value; }
        }

        public GitRevision End
        {
            get { return _end; }
            set { _end = value; }
        }

        int _limit = -1;
        public int Limit
        {
            get { return _limit; }
            set { _limit = value; }
        }

        bool _strictNodeHistory, _includeMerged;
        public bool StrictNodeHistory
        {
            get { return _strictNodeHistory; }
            set { _strictNodeHistory = value; }
        }

        public bool IncludeMergedRevisions
        {
            get { return _includeMerged; }
            set { _includeMerged = value; }
        }
    }
}
