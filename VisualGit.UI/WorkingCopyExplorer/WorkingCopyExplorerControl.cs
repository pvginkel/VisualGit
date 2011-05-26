using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Windows.Forms;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

using VisualGit.Commands;
using VisualGit.Scc;
using VisualGit.UI.WorkingCopyExplorer.Nodes;
using VisualGit.VS;
using System.Windows.Forms.Design;

namespace VisualGit.UI.WorkingCopyExplorer
{
    public partial class WorkingCopyExplorerControl : VisualGitToolWindowControl
    {
        IVisualGitConfigurationService _configurationService;
        protected virtual IVisualGitConfigurationService ConfigurationService
        {
            get { return _configurationService ?? (_configurationService = Context.GetService<IVisualGitConfigurationService>()); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkingCopyExplorerControl"/> class.
        /// </summary>
        public WorkingCopyExplorerControl()
        {
            this.InitializeComponent();
        }

        protected override void OnContextChanged(EventArgs e)
        {
            folderTree.Context = Context;
            fileList.Context = Context;
        }

        /// <summary>
        /// Called when the frame is created
        /// </summary>
        /// <param name="e"></param>
        protected override void OnFrameCreated(EventArgs e)
        {
            base.OnFrameCreated(e);

            ToolWindowHost.CommandContext = VisualGitId.SccExplorerContextGuid;
            ToolWindowHost.KeyboardContext = VisualGitId.SccExplorerContextGuid;

            folderTree.Context = Context;
            fileList.Context = Context;

            VSCommandHandler.Install(Context, this, VisualGitCommand.ExplorerOpen, OnOpen, OnUpdateOpen);
            VSCommandHandler.Install(Context, this, VisualGitCommand.ExplorerUp, OnUp, OnUpdateUp);
            VSCommandHandler.Install(Context, this, VisualGitCommand.Refresh, OnRefresh, OnUpdateRefresh);
            VSCommandHandler.Install(Context, this, new CommandID(VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.Delete), OnDelete);

            VisualGitServiceEvents environment = Context.GetService<VisualGitServiceEvents>();

            // The Workingcopy explorer is a singleton toolwindow (Will never be destroyed unless VS closes)
            environment.SolutionClosed += OnSolutionClosed;
            environment.SolutionOpened += OnSolutionOpened;

            IUIService ui = Context.GetService<IUIService>();

            if (ui != null)
            {
                ToolStripRenderer renderer = ui.Styles["VsToolWindowRenderer"] as ToolStripRenderer;

                if (renderer != null)
                    foldersStrip.Renderer = renderer;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

			fileList.ColumnWidthChanged += new ColumnWidthChangedEventHandler(fileList_ColumnWidthChanged);
            IDictionary<string, int> widths = ConfigurationService.GetColumnWidths(GetType());
            fileList.SetColumnWidths(widths);
        }

		protected void fileList_ColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
        {
            IDictionary<string, int> widths = fileList.GetColumnWidths();
            ConfigurationService.SaveColumnsWidths(GetType(), widths);
        }

        void OnSolutionOpened(object sender, EventArgs e)
        {
            AddRoots(false);
            AddRoots(true);
            RefreshRoots();
        }

        void OnSolutionClosed(object sender, EventArgs e)
        {
            AddRoots(false);
            AddRoots(true);
            RefreshRoots();
        }

        void OnDelete(object sender, CommandEventArgs e)
        {
            e.GetService<IVisualGitCommandService>().PostExecCommand(VisualGitCommand.ItemDelete);
        }

        bool _rootsPresent;
        void AddRoots(bool add)
        {
            if (add == _rootsPresent)
                return;

            if (!_rootsPresent)
            {
                IVisualGitSolutionSettings slnSettings = Context.GetService<IVisualGitSolutionSettings>();
                if (!string.IsNullOrEmpty(slnSettings.SolutionFilename))
                {
                    SvnItem slnItem = FileStatusCache[slnSettings.SolutionFilename];
                    folderTree.AddRoot(new WCSolutionNode(Context, slnItem));
                }
                folderTree.AddRoot(new WCMyComputerNode(Context));

                _rootsPresent = true;
            }
            else
            {
                folderTree.ClearRoots();
                _rootsPresent = false;
            }
        }

        IFileStatusCache FileStatusCache
        {
            get { return Context.GetService<IFileStatusCache>(); }
        }

        protected override void OnFrameShow(VisualGit.Scc.UI.FrameEventArgs e)
        {
            base.OnFrameShow(e);
            switch (e.Show)
            {
                case __FRAMESHOW.FRAMESHOW_WinShown:
                    AddRoots(true);
                    break;
            }
        }

        protected override void OnFrameClose(EventArgs e)
        {
            base.OnFrameClose(e);

            AddRoots(false);
        }

        private void RefreshRoots()
        {
            AddRoots(false);
            AddRoots(true);
        }

        internal void AddRoot(WCTreeNode root)
        {
            this.folderTree.AddRoot(root);
        }

        internal void RefreshItem(WCTreeNode item)
        {
            item.Refresh();
        }

        private void folderTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            WCTreeNode item = this.folderTree.SelectedItem;
            if (item == null)
                return;

            this.fileList.SetDirectory(item);
        }

        public bool IsWcRootSelected()
        {
            return false;
        }

        public void RemoveRoot()
        {
            //
        }

        IFileStatusCache _cache;
        protected internal IFileStatusCache StatusCache
        {
            get { return _cache ?? (_cache = Context.GetService<IFileStatusCache>()); }
        }

        public void AddRoot(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");

            SvnItem item = StatusCache[path];

            if (item == null)
                return;

            SvnWorkingCopy wc = item.WorkingCopy;

            string root;
            if (wc != null)
                root = wc.FullPath;
            else
                root = item.FullPath;



            AddRoot(CreateRoot(root));

            folderTree.SelectSubNode(item);
        }

        public void BrowsePath(IVisualGitServiceProvider context, string path)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            else if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");

            folderTree.BrowsePath(path);

            if (context.GetService<IFileStatusCache>()[path].IsFile)
            {
                fileList.SelectPath(path);
            }
        }

        private WCTreeNode CreateRoot(string directory)
        {
            StatusCache.MarkDirtyRecursive(directory);
            SvnItem item = StatusCache[directory];

            return new WCDirectoryNode(Context, null, item);
        }

        internal void OpenItem(IVisualGitServiceProvider context, string p)
        {
            VisualGit.Commands.IVisualGitCommandService cmd = context.GetService<VisualGit.Commands.IVisualGitCommandService>();

            if (cmd != null)
                cmd.ExecCommand(VisualGitCommand.ItemOpenVisualStudio, true);
        }

        void OnUpdateRefresh(object sender, CommandUpdateEventArgs e)
        {
        }

        void OnRefresh(object sender, CommandEventArgs e)
        {
            e.GetService<IVisualGitCommandService>().DirectlyExecCommand(VisualGitCommand.Refresh);

            RefreshSelection();
        }

        void OnUpdateUp(object sender, CommandUpdateEventArgs e)
        {
            FileSystemTreeNode tn = folderTree.SelectedNode as FileSystemTreeNode;

            if (tn == null || !(tn.Parent is FileSystemTreeNode))
                e.Enabled = false;
        }

        void OnUp(object sender, CommandEventArgs e)
        {
            FileSystemTreeNode t = folderTree.SelectedNode as FileSystemTreeNode;
            if (t != null && t.Parent != null)
            {
                folderTree.SelectedNode = t.Parent;
            }
        }

        void OnUpdateOpen(object sender, CommandUpdateEventArgs e)
        {
            SvnItem item = EnumTools.GetSingle(e.Selection.GetSelectedSvnItems(false));

            if (item == null)
                e.Enabled = false;
        }

        void OnOpen(object sender, CommandEventArgs e)
        {
            SvnItem item = EnumTools.GetSingle(e.Selection.GetSelectedSvnItems(false));

            if (item.IsDirectory)
            {
                folderTree.SelectSubNode(item);
                return;
            }

            AutoOpenCommand(e, item);
        }

        private static void AutoOpenCommand(CommandEventArgs e, SvnItem item)
        {
            IProjectFileMapper pfm = e.GetService<IProjectFileMapper>();
            IVisualGitCommandService svc = e.GetService<IVisualGitCommandService>();
            IVisualGitSolutionSettings solutionSettings = e.GetService<IVisualGitSolutionSettings>();

            if (pfm == null || svc == null || solutionSettings == null)
                return;

            // We can assume we have a file

            if (pfm.IsProjectFileOrSolution(item.FullPath))
            {
                // Ok, the user selected the current solution file or an open project
                // Let's jump to it in the solution explorer

                svc.ExecCommand(VisualGitCommand.ItemOpenSolutionExplorer);
                return;
            }

            if (item.InSolution)
            {
                // The file is part of the solution, we can assume VS knows how to open it
                svc.ExecCommand(VisualGitCommand.ItemOpenVisualStudio);
                return;
            }

            string filename = item.Name;
            string ext = item.Extension;

            if (string.IsNullOrEmpty(ext))
            {
                // No extension -> Open as text
                svc.PostExecCommand(VisualGitCommand.ItemOpenTextEditor);
                return;
            }

            foreach (string projectExt in solutionSettings.AllProjectExtensionsFilter.Split(';'))
            {
                if (projectExt.TrimStart('*').Trim().Equals(ext, StringComparison.OrdinalIgnoreCase))
                {
                    // We found a project or solution: Ask VS to open it

                    e.GetService<IVisualGitSolutionSettings>().OpenProjectFile(item.FullPath);
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
                        VsShellUtilities.OpenDocument(e.Context, item.FullPath);
                        return;
                    }
                }
            }

            // Ultimate fallback: Just open the file as windows would
            svc.PostExecCommand(VisualGitCommand.ItemOpenWindows);
        }

        public void RefreshSelection()
        {
            FileSystemTreeNode tn = (FileSystemTreeNode)folderTree.SelectedNode;
            tn.WCNode.Refresh(true);
            folderTree.FillNode(tn);

            WCTreeNode item = this.folderTree.SelectedItem;
            if (item == null)
                return;

            this.fileList.SetDirectory(item);
        }
    }
}
