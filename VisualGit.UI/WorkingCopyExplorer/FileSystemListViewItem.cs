// VisualGit.UI\WorkingCopyExplorer\FileSystemListViewItem.cs
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
using System.Text;
using System.Windows.Forms;
using VisualGit.UI.VSSelectionControls;
using System.IO;
using System.Diagnostics;
using VisualGit.VS;
using VisualGit.Scc;

namespace VisualGit.UI.WorkingCopyExplorer
{
    class FileSystemListViewItem : SmartListViewItem
    {
        readonly GitItem _gitItem;

        public FileSystemListViewItem(SmartListView view, GitItem item)
            : base(view)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            
            _gitItem = item;

            ImageIndex = View.IconMapper.GetIcon(item.FullPath);

            RefreshValues();
        }

        FileSystemDetailsView View
        {
            get { return base.ListView as FileSystemDetailsView; }
        }

        public GitItem GitItem
        {
            [DebuggerStepThrough]
            get { return _gitItem; }
        }

        PendingChangeStatus _chg;
    
        private void RefreshValues()
        {
            bool exists = GitItem.Exists;
            string name = string.IsNullOrEmpty(GitItem.Name) ? GitItem.FullPath : GitItem.Name;

            VisualGitStatus status = GitItem.Status;
            PendingChangeKind kind = PendingChange.CombineStatus(status.State, GitItem.IsTreeConflicted, GitItem);

            if (_chg == null || _chg.State != kind)
                _chg = new PendingChangeStatus(kind);

            SetValues(
                name,
                Modified.ToString("g"),
                View.Context.GetService<IFileIconMapper>().GetFileType(GitItem),
                _chg.ExplorerText,
                "",
                GitItem.Status.Revision.ToString(),
                GitItem.Status.LastChangeTime.ToLocalTime().ToString(),
                GitItem.Status.LastChangeRevision.ToString(),
                GitItem.Status.LastChangeAuthor,
                GitItem.Status.State.ToString(),
                "",
                GitItem.Status.IsCopied.ToString(),
                GitItem.IsConflicted.ToString(),
                GitItem.FullPath
                );

            StateImageIndex = (int)View.StatusMapper.GetStatusImageForGitItem(GitItem);
        }

        internal bool IsDirectory
        {
            get { return GitItem.IsDirectory; }
        }

        internal DateTime Modified
        {
            get { return GitItem.Modified; }

        }
    }
}
