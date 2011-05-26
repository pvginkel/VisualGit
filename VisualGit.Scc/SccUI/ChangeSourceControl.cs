using System;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Windows.Forms.Design;

using VisualGit.Selection;
using VisualGit.Scc;
using VisualGit.UI;
using VisualGit.VS;
using System.Diagnostics;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;

namespace VisualGit.Scc.SccUI
{
    public partial class ChangeSourceControl : VSDialogForm
    {
        public ChangeSourceControl()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (DesignMode)
                return;

            if (Context != null)
            {
                IUIService ds = Context.GetService<IUIService>();

                if (ds != null)
                {
                    ToolStripRenderer renderer = ds.Styles["VsToolWindowRenderer"] as ToolStripRenderer;

                    if (renderer != null)
                        toolStrip1.Renderer = renderer;
                }

                IVisualGitSolutionSettings settings = Context.GetService<IVisualGitSolutionSettings>();

                if (settings.SolutionFilename != null)
                    Text += " - " + Path.GetFileName(settings.SolutionFilename);
            }

            if (bindingGrid != null)
                InitializeGrid();
        }

        private void InitializeGrid()
        {
            bindingGrid.Rows.Clear();

            if (Context == null)
                return;

            if (string.IsNullOrEmpty(SolutionSettings.SolutionFilename))
            {
                return;
            }

            // TODO: Optimize to one time init and then just refresh
            if (SvnItem.IsValidPath(SolutionSettings.SolutionFilename))
            {
                bindingGrid.Rows.Add(new ChangeSourceControlRow(Context, SvnProject.Solution));
            }
            foreach (SvnProject project in ProjectMapper.GetAllProjects())
            {
                if (project.IsSolution)
                    continue;

                ISvnProjectInfo projectInfo = ProjectMapper.GetProjectInfo(project);

                if (projectInfo == null || string.IsNullOrEmpty(projectInfo.ProjectDirectory))
                    continue;

                if (!projectInfo.IsSccBindable && !Scc.IsProjectManaged(project))
                    continue;

                bindingGrid.Rows.Add(new ChangeSourceControlRow(Context, project));
            }
            // /TODO

            RefreshGrid();
        }

        VisualGitSccProvider _scc;
        VisualGitSccProvider Scc
        {
            get { return _scc ?? (_scc = Context.GetService<VisualGitSccProvider>(typeof(IVisualGitSccService))); }
        }

        void RefreshGrid()
        {
            foreach (ChangeSourceControlRow row in bindingGrid.Rows)
            {
                row.Refresh();
            }

            bool enableConnect = false;
            bool enableDisconnect = false;

            IVisualGitSccService scc = Context.GetService<IVisualGitSccService>();

            bool isSolution = false;
            foreach (SvnProject project in SelectedProjects)
            {
                if (project.IsSolution)
                    isSolution = true;

                if (scc.IsProjectManaged(project))
                    enableDisconnect = true;
                else if (!enableConnect)
                {
                    SvnItem item = null;
                    if (!project.IsSolution)
                    {
                        ISvnProjectInfo projectInfo = ProjectMapper.GetProjectInfo(project);

                        if (projectInfo == null || string.IsNullOrEmpty(projectInfo.ProjectDirectory))
                            continue;

                        item = StatusCache[projectInfo.SccBaseDirectory];
                    }
                    else
                        item = SolutionSettings.ProjectRootSvnItem;

                    if (item != null && item.Uri != null)
                        enableConnect = true;
                }

                if (enableConnect && enableDisconnect && isSolution)
                    break;
            }

            EnableTab(solutionSettingsTab, isSolution);
            EnableTab(sharedSettingsTab, !isSolution);
            EnableTab(userSettingsTab, false);

            UpdateSettingTabs();

            connectButton.Enabled = enableConnect;
            disconnectButton.Enabled = enableDisconnect;

            UpdateSettingTabs();            
        }        

        private void refreshButton_Click(object sender, EventArgs e)
        {
            RefreshGrid();
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            if (Context == null)
                return;

            IVisualGitSccService scc = Context.GetService<IVisualGitSccService>();

            foreach (SvnProject project in SelectedProjects)
            {
                scc.SetProjectManaged(project, true);
            }

            RefreshGrid();
        }
        private void disconnectButton_Click(object sender, EventArgs e)
        {
            if (Context == null)
                return;

            IVisualGitSccService scc = Context.GetService<IVisualGitSccService>();

            foreach (SvnProject project in SelectedProjects)
            {
                scc.SetProjectManaged(project, false);
            }

            RefreshGrid();
        }

        IProjectFileMapper _projectMapper;
        IProjectFileMapper ProjectMapper
        {
            get { return _projectMapper ?? (_projectMapper = Context.GetService<IProjectFileMapper>()); }
        }

        IFileStatusCache _fileCache;
        IFileStatusCache StatusCache
        {
            get { return _fileCache ?? (_fileCache = Context.GetService<IFileStatusCache>()); }
        }

        IVisualGitSolutionSettings _solutionSettings;
        IVisualGitSolutionSettings SolutionSettings
        {
            get { return _solutionSettings ?? (_solutionSettings = Context.GetService<IVisualGitSolutionSettings>()); }
        }

        IEnumerable<SvnProject> SelectedProjects
        {
            get
            {
                List<SvnProject> projects = new List<SvnProject>();
                foreach (ChangeSourceControlRow row in bindingGrid.SelectedRows)
                {
                    SvnProject project = row.Project;

                    if (projects.Contains(project))
                        continue;

                    projects.Add(project);
                    yield return project;
                }
            }
        }

        private void bindingGrid_SelectionChanged(object sender, EventArgs e)
        {
            if (DesignMode)
                return;

            RefreshGrid();
        }

        private void EnableTab(TabPage tab, bool enable)
        {
            if (enable == settingsTabControl.Controls.Contains(tab))
                return;

            Control activeControl = ActiveControl;

            if (enable)
                settingsTabControl.Controls.Add(tab);
            else
                settingsTabControl.Controls.Remove(tab);

            if (activeControl != ActiveControl)
                ActiveControl = activeControl;
        }

        private void UpdateSettingTabs()
        {
            string projectBase = null;
            string relativePath = null;
            string projectLocation = null;
            bool first = true;
            Uri projectUri = null;
            SccEnlistMode enlistMode = (SccEnlistMode) (-1);

            foreach (SvnProject p in SelectedProjects)
            {
                ISvnProjectInfo info;
                if (p.IsSolution ||
                    null == (info = ProjectMapper.GetProjectInfo(p)) ||
                    string.IsNullOrEmpty(info.ProjectDirectory))
                {
                    projectBase = null;
                    break;
                }

                string pBase = info.SccBaseDirectory;
                string relPath = info.ProjectDirectory;
                string pLoc = null;
                Uri pUri;

                if (relPath.StartsWith(info.SccBaseDirectory))
                {
                    int pdLen = info.ProjectDirectory.Length;
                    int sccLen = info.SccBaseDirectory.Length;

                    if (pdLen == sccLen)
                        relPath = ".";
                    else if (sccLen < pdLen && info.ProjectDirectory[sccLen] == Path.DirectorySeparatorChar)
                        relPath = info.ProjectDirectory.Substring(sccLen + 1);
                    else
                        relPath = info.ProjectDirectory;
                }

                if (p.RawHandle != null)
                {
                    IVsProject2 ps = p.RawHandle as IVsProject2;

                    if (ps != null)
                    {
                        string doc;
                        if (ErrorHandler.Succeeded(ps.GetMkDocument(VSConstants.VSITEMID_ROOT, out doc)))
                        {
                            if (!string.IsNullOrEmpty(doc) && SvnItem.IsValidPath(doc))
                                pLoc = PackageUtilities.MakeRelative(SolutionSettings.SolutionFilename, doc);
                            else
                                pLoc = doc;
                        }
                    }
                }

                pUri = info.SccBaseUri;

                if (pUri == null && pBase != null)
                {
                    SvnItem pi = StatusCache[pBase];

                    if (pi != null)
                        pUri = pi.Uri;
                }

                KeepOneIgnoreCase(ref projectBase, pBase, first);
                KeepOneIgnoreCase(ref relativePath, relPath, first);
                KeepOneIgnoreCase(ref projectLocation, pLoc, first);

                KeepOne(ref projectUri, pUri, first);

                if (first)
                    enlistMode = info.SccEnlistMode;
                else
                    enlistMode = SccEnlistMode.None;

                first = false;
            }

            if (projectBase == null)
            {
                relativePath = null;
                projectLocation = null;
            }

            shProjectLocation.Text = projectLocation ?? "";
            shBindPath.Text = projectBase ?? "";
            shRelativePath.Text = string.IsNullOrEmpty(relativePath) ? "." : relativePath;
            shProjectUrl.Text = (projectUri != null) ? projectUri.ToString() : "";

            usProjectLocation.Text = projectLocation ?? "";
            usBindPath.Text = projectBase ?? "";
            usRelativePath.Text = string.IsNullOrEmpty(relativePath) ? "." : relativePath;
            usProjectUrl.Text = (projectUri != null) ? projectUri.ToString() : "";

            slnProjectLocation.Text = SolutionSettings.SolutionFilename;
            slnBindPath.Text = SolutionSettings.ProjectRoot;

            string slRelativePath = Path.GetDirectoryName(PackageUtilities.MakeRelative(SolutionSettings.ProjectRoot, SolutionSettings.SolutionFilename));

            slnRelativePath.Text = string.IsNullOrEmpty(slRelativePath) ? "." : slRelativePath;

            slnBindUrl.Text = (SolutionSettings.ProjectRootUri != null) ? SolutionSettings.ProjectRootUri.ToString() : "";

            usProjectLocationBrowse.Visible = enlistMode > SccEnlistMode.None;
            usProjectLocationBrowse.Enabled = enlistMode > SccEnlistMode.SvnStateOnly;

            sharedProjectUrlBrowse.Enabled = sharedBasePathBrowse.Visible 
                = (enlistMode > SccEnlistMode.None) 
                && (projectBase != null) 
                && (enlistMode > SccEnlistMode.SvnStateOnly || projectBase != SolutionSettings.ProjectRoot);

            slnBindBrowse.Enabled = (SolutionSettings.ProjectRootSvnItem != null) && SolutionSettings.ProjectRootSvnItem.WorkingCopy != null;
        }

        private void KeepOneIgnoreCase(ref string result, string newValue, bool first)
        {
            if (first)
                result = newValue;
            else if (result == null || string.Equals(result, newValue, StringComparison.OrdinalIgnoreCase))
                return;
            else
                result = null;
        }

        void KeepOne<T>(ref T result, T value, bool first)
            where T : class
        {
            if (first)
                result = value;
            else if (result == null || result.Equals(value))
                return;
            else
                result = null;
        }

        private void slnBindBrowse_Click(object sender, EventArgs e)
        {
            using (ChangeSolutionRoot sr = new ChangeSolutionRoot())
            {
                sr.ShowDialog(Context);
                RefreshGrid();
            }
        }
    }
}
