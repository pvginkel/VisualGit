using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using SharpSvn;
using VisualGit.VS;
using VisualGit.UI.VSSelectionControls;
using VisualGit.Commands;
using Microsoft.VisualStudio;
using System.ComponentModel.Design;
using VisualGit.Scc;
using System.IO;
using System.Windows.Forms.Design;
using SharpSvn.Remote;

namespace VisualGit.UI.RepositoryExplorer
{
    /// <summary>
    /// Gives a tree view of the repository based on revision.
    /// </summary>
    public partial class RepositoryExplorerControl : VisualGitToolWindowControl
    {
        IVisualGitConfigurationService _configurationService;
        protected IVisualGitConfigurationService ConfigurationService
        {
            get { return _configurationService ?? (_configurationService = Context.GetService<IVisualGitConfigurationService>()); }
        }

        public RepositoryExplorerControl()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            this.components = new Container();
            treeView.RetrieveItems = SvnDirEntryItems.Kind | SvnDirEntryItems.LastAuthor | SvnDirEntryItems.Revision | SvnDirEntryItems.Size | SvnDirEntryItems.Time;
        }

        protected override void OnFrameCreated(EventArgs e)
        {
            base.OnFrameCreated(e);

            ToolWindowHost.CommandContext = VisualGitId.SccExplorerContextGuid;
            ToolWindowHost.KeyboardContext = VisualGitId.SccExplorerContextGuid;

            VSCommandHandler.Install(Context, this, new CommandID(VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.Delete), OnDelete);
            VSCommandHandler.Install(Context, this, VisualGitCommand.ExplorerOpen, OnOpen, OnUpdateOpen);
            VSCommandHandler.Install(Context, this, VisualGitCommand.ExplorerUp, OnUp, OnUpdateUp);

            IUIService ui = Context.GetService<IUIService>();

            if (ui != null)
            {
                ToolStripRenderer renderer = ui.Styles["VsToolWindowRenderer"] as ToolStripRenderer;

                if (renderer != null)
                    toolFolders.Renderer = renderer;
            }
        }

        void OnDelete(object sender, CommandEventArgs e)
        {
            e.GetService<IVisualGitCommandService>().PostExecCommand(VisualGitCommand.GitNodeDelete);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            treeView.Context = Context;
            fileView.Context = Context;
            treeView.SelectionPublishServiceProvider = Context;
            fileView.SelectionPublishServiceProvider = Context;

            fileView.ColumnWidthChanged += new ColumnWidthChangedEventHandler(fileView_ColumnWidthChanged);
            IDictionary<string, int> widths = ConfigurationService.GetColumnWidths(GetType());
            fileView.SetColumnWidths(widths);
        }

        protected void fileView_ColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
        {
            IDictionary<string, int> widths = fileView.GetColumnWidths();
            ConfigurationService.SaveColumnsWidths(GetType(), widths);
        }

        /// <summary>
        /// Add a new URL root to the tree.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="node"></param>
        public void AddRoot(Uri uri)
        {
            this.treeView.AddRoot(uri);
        }

        /// <summary>
        /// Remove the root(server) of the node identified by the <paramref name="uri"/>
        /// </summary>
        /// <param name="uri">URI of the tree node</param>
        public void RemoveRootOf(Uri uri)
        {
            this.treeView.RemoveRootOf(uri);
        }

        /// <summary>
        /// Reloads the node identified by the <paramref name="uri"/><br/>
        /// </summary>
        /// <param name="uri">URI of the tree node</param>
        public void Reload(Uri uri)
        {
            this.treeView.Reload(uri);
        }

        /// <summary>
        /// Get the URI of the selected node
        /// </summary>
        public Uri SelectedUri
        {
            get
            {
                RepositoryTreeNode selected = this.treeView.SelectedNode;
                return selected == null ? null : selected.RawUri;
            }
        }

        private void OnTreeViewShowContextMenu(object sender, MouseEventArgs e)
        {
            if (Context == null)
                return;

            Point screen;
            if (e.X == -1 && e.Y == -1)
            {
                screen = treeView.GetSelectionPoint();
                if (screen.IsEmpty)
                    return;
            }
            else
            {
                screen = e.Location;
            }

            IVisualGitCommandService cs = Context.GetService<IVisualGitCommandService>();

            cs.ShowContextMenu(VisualGitCommandMenu.RepositoryExplorerContextMenu, screen.X, screen.Y);
        }

        IFileIconMapper _iconMapper;
        IFileIconMapper IconMapper
        {
            get
            {
                if (_iconMapper == null && Context != null)
                    _iconMapper = Context.GetService<IFileIconMapper>();

                return _iconMapper;
            }
        }

        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            RefreshFileList();
        }

        void RefreshFileList()
        {
            fileView.Items.Clear();

            RepositoryTreeNode tn = treeView.SelectedNode as RepositoryTreeNode;

            if (tn != null && tn.Origin != null)
            {
                foreach (RepositoryTreeNode sn in tn.Nodes)
                {
                    if (sn.FolderItems.Contains(sn.RawUri))
                    {
                        ISvnRepositoryListItem ea = sn.FolderItems[sn.RawUri];
                        RepositoryListItem item = new RepositoryListItem(fileView, ea, tn.Origin, IconMapper);

                        fileView.Items.Add(item);
                    }
                }
                foreach (ISvnRepositoryListItem ee in tn.FolderItems)
                {
                    if (ee.Uri != tn.RawUri)
                    {
                        RepositoryListItem item = new RepositoryListItem(fileView, ee, tn.Origin, IconMapper);

                        fileView.Items.Add(item);
                    }
                }

                if (fileView.Items.Count > 0)
                {
                    SmartColumn fileColumn = fileView.AllColumns[0];

                    if (fileColumn.DisplayIndex >= 0)
                        fileColumn.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
                }
            }
        }

        BusyOverlay _bo;
        private void treeView_RetrievingChanged(object sender, EventArgs e)
        {
            if (treeView.Retrieving)
            {
                if (_bo == null)
                    _bo = new BusyOverlay(treeView, AnchorStyles.Top | AnchorStyles.Right);

                _bo.Show();
            }
            else
            {
                if (_bo != null)
                {
                    _bo.Hide();
                    _bo.Dispose();
                    _bo = null;
                }
            }
        }

        private void treeView_SelectedNodeRefresh(object sender, EventArgs e)
        {
            RefreshFileList();
        }

        private void fileView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ListViewHitTestInfo ht = fileView.HitTest(e.X, e.Y);

            RepositoryListItem li = ht.Item as RepositoryListItem;

            if (ht.Location == ListViewHitTestLocations.None || li == null)
                return;

            if (!li.Selected)
            {
                fileView.SelectedIndices.Clear();
                li.Selected = true;
            }

            Context.GetService<IVisualGitCommandService>().PostExecCommand(VisualGitCommand.ExplorerOpen);
        }


        void OnUpdateOpen(object sender, CommandUpdateEventArgs e)
        {
            RepositoryExplorerItem item = EnumTools.GetSingle(e.Selection.GetSelection<RepositoryExplorerItem>());

            if (item == null || item.Uri == null)
                e.Enabled = false;
        }

        void OnOpen(object sender, CommandEventArgs e)
        {
            RepositoryExplorerItem item = EnumTools.GetSingle(e.Selection.GetSelection<RepositoryExplorerItem>());

            if (item.Entry == null || item.Entry.NodeKind == SvnNodeKind.Directory || item.Origin == null)
            {
                treeView.BrowseTo(item.Uri);
                return;
            }

            AutoOpenCommand(e, item.Origin);
        }

        private static void AutoOpenCommand(CommandEventArgs e, GitOrigin origin)
        {
            IVisualGitCommandService svc = e.GetService<IVisualGitCommandService>();
            IVisualGitSolutionSettings solutionSettings = e.GetService<IVisualGitSolutionSettings>();

            if (svc == null || solutionSettings == null)
                return;

            // Ok, we can assume we have a file
            string filename = origin.Target.FileName;
            string ext = Path.GetExtension(filename);

            if (string.IsNullOrEmpty(ext))
            {
                // No extension -> Open as text
                svc.PostExecCommand(VisualGitCommand.ViewInVsText);
                return;
            }

            foreach (string projectExt in solutionSettings.AllProjectExtensionsFilter.Split(';'))
            {
                if (projectExt.TrimStart('*').Trim().Equals(ext, StringComparison.OrdinalIgnoreCase))
                {
                    // We found a project or solution, use Open from Git to create a checkout

                    svc.PostExecCommand(VisualGitCommand.FileFileOpenFromGit, origin);
                    return;
                }
            }

            bool odd = false;
            foreach (string block in solutionSettings.OpenFileFilter.Split('|'))
            {
                odd = !odd;
                if (odd)
                    continue;

                foreach (string itemExt in block.Split(';'))
                {
                    if (itemExt.TrimStart('*').Trim().Equals(ext, StringComparison.OrdinalIgnoreCase))
                    {
                        svc.PostExecCommand(VisualGitCommand.ViewInVsNet);
                        return;
                    }
                }
            }

            // Ultimate fallback: Just ask the user what to do (don't trust the repository!)
            svc.PostExecCommand(VisualGitCommand.ViewInWindowsWith);
        }

        void OnUpdateUp(object sender, CommandUpdateEventArgs e)
        {
            if (treeView.SelectedNode == null
                || treeView.Nodes.Count == 0
                || treeView.SelectedNode == treeView.Nodes[0])
            {
                e.Enabled = false;
            }
        }

        void OnUp(object sender, CommandEventArgs e)
        {
            treeView.SelectedNode = treeView.SelectedNode.Parent as RepositoryTreeNode;
        }
    }
}
