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
