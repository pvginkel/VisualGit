// VisualGit.UI\WorkingCopyExplorer\Nodes\WCDirectoryNode.cs
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
using System.IO;
using VisualGit.Scc;
using VisualGit.VS;

namespace VisualGit.UI.WorkingCopyExplorer.Nodes
{
    abstract class WCFileSystemNode : WCTreeNode
    {
        readonly GitItem _item;
        public GitItem GitItem { get { return _item; } }
        protected WCFileSystemNode(IVisualGitServiceProvider context, WCTreeNode parent, GitItem item)
            :base(context, parent)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            _item = item;
        }

        IFileStatusCache _statusCache;
        protected IFileStatusCache StatusCache
        {
            get { return _statusCache ?? (_statusCache = Context.GetService<IFileStatusCache>()); }
        }
 
        public override string Title
        {
            get { return string.IsNullOrEmpty(_item.Name) ? _item.FullPath : _item.Name; }
        }
    }

    class WCFileNode : WCFileSystemNode
    {
        public WCFileNode(IVisualGitServiceProvider context, WCTreeNode parent, GitItem item)
            : base(context, parent, item)
        {
        }

        public override bool IsContainer
        {
            get
            {
                return false;
            }
        }

        public override int ImageIndex
        {
            get
            {
                IFileIconMapper iconMap = Context.GetService<IFileIconMapper>();
                return iconMap.GetIcon(GitItem.FullPath);
            }
        }

        public override void GetResources(System.Collections.ObjectModel.Collection<GitItem> list, bool getChildItems, Predicate<GitItem> filter)
        {
        }

        protected override void RefreshCore(bool rescan)
        {
            if(GitItem == null)
                return;
             
            if(rescan)
                StatusCache.MarkDirtyRecursive(GitItem.FullPath);

            if (TreeNode != null)
                TreeNode.Refresh();
        }

        public override IEnumerable<WCTreeNode> GetChildren()
        {
            yield break;
        }

        internal override bool ContainsDescendant(string path)
        {
            return false;
        }
        
    }

    class WCDirectoryNode : WCFileSystemNode
    {
        public WCDirectoryNode(IVisualGitServiceProvider context, WCTreeNode parent, GitItem item)
            : base(context, parent, item)
        {
        }

        public override int ImageIndex
        {
            get 
            {
                IFileIconMapper iconMap = Context.GetService<IFileIconMapper>();
                return iconMap.GetIcon(GitItem.FullPath);
            }
        }

        public override void GetResources(System.Collections.ObjectModel.Collection<GitItem> list, bool getChildItems, Predicate<GitItem> filter)
        {
        }

        protected override void RefreshCore(bool rescan)
        {
        }

        public override IEnumerable<WCTreeNode> GetChildren()
        {
            IFileStatusCache cache = Context.GetService<IFileStatusCache>();
            foreach (SccFileSystemNode node in SccFileSystemNode.GetDirectoryNodes(GitItem.FullPath))
            {
                if ((node.Attributes & (FileAttributes.Hidden | FileAttributes.System
                    | FileAttributes.Offline)) != 0)
                    continue;

                if ((node.Attributes & FileAttributes.Directory) > 0)
                    yield return new WCDirectoryNode(Context, this, cache[node.FullPath]);
                else
                    yield return new WCFileNode(Context, this, cache[node.FullPath]);
            }
        }

        internal override bool ContainsDescendant(string path)
        {
            return StatusCache[path].IsBelowPath(GitItem);
        }
    }
}
