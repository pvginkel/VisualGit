// VisualGit.UI\GitLog\RevisionGrid\RevisionItem.cs
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
using VisualGit.Scc;
using System.ComponentModel;
using SharpGit;

namespace VisualGit.UI.GitLog.RevisionGrid
{
    class RevisionItem : VisualGitPropertyGridItem, IGitLogItem
    {
        private GitRevision _revision;

        public RevisionItem(GitRevision revision)
        {
            if (revision == null)
                throw new ArgumentNullException("revision");

            _revision = revision;
        }

        [DisplayName("Author")]
        [Description("Author of the revision")]
        public string Author
        {
            get { return _revision.Author + " <" + _revision.AuthorEmail + ">"; }
        }

        [DisplayName("Date")]
        [Description("Date the revision was committed")]
        public DateTime AuthorDate
        {
            get { return _revision.AuthorDate; }
        }

        [DisplayName("Log Message")]
        [Description("Log message associated with the revision")]
        public string LogMessage
        {
            get { return _revision.LogMessage; }
        }

        [DisplayName("Revision")]
        [Description("Unique hash of the revision")]
        public string Revision
        {
            get { return _revision.Revision; }
        }

        [DisplayName("Repository Root")]
        [Description("Root directory of the repository of the revision")]
        public string RepositoryRoot
        {
            get { return _revision.RepositoryRoot; }
        }

        protected override string ClassName
        {
            get { return "Revision"; }
        }

        protected override string ComponentName
        {
            get { return _revision.Revision; }
        }

        DateTime IGitLogItem.CommitDate
        {
            get { return _revision.CommitDate; }
        }

        IList<string> IGitLogItem.ParentRevisions
        {
            get { return _revision.ParentRevisions; }
        }

        int IGitLogItem.Index
        {
            get { return _revision.Index; }
        }

        GitChangeItemCollection IGitLogItem.ChangedPaths
        {
            get { return null; }
        }
    }
}
