using System;
using System.Windows.Forms;
using VisualGit.Scc;
using VisualGit.UI.WorkingCopyExplorer.Nodes;

namespace VisualGit.UI.WorkingCopyExplorer
{
    sealed class FileSystemTreeNode : TreeNode
    {
        readonly WCTreeNode _wcNode;
        readonly SvnItem _item;
        public FileSystemTreeNode(WCTreeNode wcNode, SvnItem item)
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

        public SvnItem SvnItem
        {
            get
            {
                WCFileSystemNode dirNode = _wcNode as WCFileSystemNode;
                if (_item == null && dirNode != null)
                    return dirNode.SvnItem;
                return _item; 
            }
        }

        public void Refresh()
        {
            StateImageIndex = (int) VisualGitGlyph.None;

            if(SvnItem == null)
                return;

            if (SvnItem.IsDirectory)
            {
                bool canRead;

                foreach (SccFileSystemNode node in SccFileSystemNode.GetDirectoryNodes(SvnItem.FullPath, out canRead))
                {
                    canRead = true;
                    break;
                }

                if (!canRead)
                    return;
            }

            StateImageIndex = (int)TreeView.StatusMapper.GetStatusImageForSvnItem(SvnItem);
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
            if (SvnItem != null && SvnItem.FullPath == path)
            {
                TreeView.Select();
                TreeView.Focus();
            }
        }
    }
}
