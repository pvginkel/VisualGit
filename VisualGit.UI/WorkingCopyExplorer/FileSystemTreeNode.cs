// VisualGit.UI\WorkingCopyExplorer\FileSystemTreeNode.cs
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
using System.Windows.Forms;
using VisualGit.Scc;
using VisualGit.UI.WorkingCopyExplorer.Nodes;

namespace VisualGit.UI.WorkingCopyExplorer
{
    sealed class FileSystemTreeNode : TreeNode
    {
        readonly WCTreeNode _wcNode;
        readonly GitItem _item;
        public FileSystemTreeNode(WCTreeNode wcNode, GitItem item)
        {
            if (wcNode == null)
                throw new ArgumentNullException("wcNode");

            _wcNode = wcNode;
            Text = wcNode.Title;
            _item = item;

            wcNode.TreeNode = this;
        }

        public FileSystemTreeNode(WCTreeNode wcNode)
            :this(wcNode, null)
        {
        }

        public FileSystemTreeNode(string text)
            : base(text)
        {
        }

        public new FileSystemTreeView TreeView
        {
            get { return (FileSystemTreeView)base.TreeView; }
        }

        public WCTreeNode WCNode
        {
            get { return _wcNode; }
        }

        public GitItem GitItem
        {
            get
            {
                WCFileSystemNode dirNode = _wcNode as WCFileSystemNode;
                if (_item == null && dirNode != null)
                    return dirNode.GitItem;
                return _item; 
            }
        }

        public void Refresh()
        {
            StateImageIndex = (int) VisualGitGlyph.None;

            if(GitItem == null)
                return;

            if (GitItem.IsDirectory)
            {
                bool canRead;

                foreach (SccFileSystemNode node in SccFileSystemNode.GetDirectoryNodes(GitItem.FullPath, out canRead))
                {
                    canRead = true;
                    break;
                }

                if (!canRead)
                    return;
            }

            StateImageIndex = (int)TreeView.StatusMapper.GetStatusImageForGitItem(GitItem);
        }

        internal void SelectSubNode(string path)
        {
            Expand();

            foreach (FileSystemTreeNode tn in Nodes)
            {
                if (tn.WCNode.ContainsDescendant(path))
                {
                    tn.SelectSubNode(path);
                    return;
                }
            }

            // No subnode to expand; we reached the target, lets select it
            TreeView.SelectedNode = this;
            EnsureVisible();
            if (GitItem != null && GitItem.FullPath == path)
            {
                TreeView.Select();
                TreeView.Focus();
            }
        }
    }
}
