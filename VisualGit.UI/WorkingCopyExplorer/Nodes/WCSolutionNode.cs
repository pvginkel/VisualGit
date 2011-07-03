// VisualGit.UI\WorkingCopyExplorer\Nodes\WCSolutionNode.cs
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
using VisualGit.VS;
using System.IO;

namespace VisualGit.UI.WorkingCopyExplorer.Nodes
{
    class WCSolutionNode : WCFileSystemNode
    {
        readonly int _imageIndex;
        public WCSolutionNode(IVisualGitServiceProvider context, GitItem item)
            : base(context, null, item)
        {
            string file = Context.GetService<IVisualGitSolutionSettings>().SolutionFilename;

            IFileIconMapper iconMapper = context.GetService<IFileIconMapper>();

            if (string.IsNullOrEmpty(file))
                _imageIndex = iconMapper.GetIconForExtension(".sln");
            else
                _imageIndex = iconMapper.GetIcon(file);
        }

        public override string Title
        {
            get 
            { 
                string file = Context.GetService<IVisualGitSolutionSettings>().SolutionFilename;

                if (file != null)
                    file = Path.GetFileNameWithoutExtension(file);

                return string.Format(WCStrings.SolutionX, file); 
            }
        }

        IEnumerable<GitItem> UpdateRoots
        {
            get
            {
                IVisualGitProjectLayoutService pls = Context.GetService<IVisualGitProjectLayoutService>();
                foreach (GitItem item in pls.GetUpdateRoots(null))
                    yield return item;
            }
        }

        public override IEnumerable<WCTreeNode> GetChildren()
        {
            foreach(GitItem item in UpdateRoots)
            {
                yield return new WCDirectoryNode(Context, this, item);
            }
        }

        public override bool IsContainer
        {
            get
            {
                return true;
            }
        }

        public override void GetResources(System.Collections.ObjectModel.Collection<GitItem> list, bool getChildItems, Predicate<GitItem> filter)
        {
//            throw new NotSupportedException();
        }

        protected override void RefreshCore(bool rescan)
        {
//            throw new NotSupportedException();
        }

        public override int ImageIndex
        {
            get { return _imageIndex; }
        }

        internal override bool ContainsDescendant(string path)
        {
            GitItem needle = StatusCache[path];

            foreach (GitItem item in UpdateRoots)
            {
                if (needle.IsBelowPath(item))
                    return true;
            }
            return false;
        }
    }
}
