// VisualGit.UI\GitLog\RevisionGrid\GitRevisions.cs
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

namespace VisualGit.UI.GitLog.RevisionGrid
{
    internal class GitRevision : IGitItem, VisualGit.Scc.IGitLogItem
    {
        public static string UncommittedWorkingDirGuid = "0000000000000000000000000000000000000000";
        public static string IndexGuid = "1111111111111111111111111111111111111111";

        public IList<string> ParentRevisions { get; set; }

        public GitRevision()
        {
            Heads = new List<GitHead>();
        }

        public ICollection<GitHead> Heads { get; set; }

        public string Author { get; set; }
        public string AuthorEmail { get; set; }
        public DateTime AuthorDate { get; set; }
        public string Committer { get; set; }
        public DateTime CommitDate { get; set; }

        public string LogMessage { get; set; }

        #region IGitItem Members

        public string Revision { get; set; }
        public string Name { get; set; }

        #endregion

        #region IGitLogItem Members

        public int Index { get; set; }
        public string RepositoryRoot { get; set; }

        SharpGit.GitChangeItemCollection VisualGit.Scc.IGitLogItem.ChangedPaths
        {
            get { return null; }
        }

        #endregion

        public override string ToString()
        {
            var sha = Revision;
            if (sha.Length > 8)
            {
                sha = sha.Substring(0, 4) + ".." + sha.Substring(sha.Length - 4, 4);
            }
            return String.Format("{0}:{1}", sha, LogMessage);
        }

        public bool MatchesSearchString(string searchString)
        {
            foreach (var gitHead in Heads)
            {
                if (gitHead.Name.ToLower().Contains(searchString))
                    return true;
            }

            if ((searchString.Length > 2) && Revision.StartsWith(searchString, StringComparison.OrdinalIgnoreCase))
                return true;


            return
                Author.StartsWith(searchString, StringComparison.CurrentCultureIgnoreCase) ||
                LogMessage.ToLower().Contains(searchString);
        }
    }
}