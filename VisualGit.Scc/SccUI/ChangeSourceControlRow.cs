using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using VisualGit.Selection;
using VisualGit.VS;
using System.IO;
using Microsoft.VisualStudio.Shell;
using SharpSvn;

namespace VisualGit.Scc.SccUI
{
    sealed class ChangeSourceControlRow : DataGridViewRow
    {
        readonly IVisualGitServiceProvider _context;
        readonly GitProject _project;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeSourceControlRow"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="project">The project.</param>
        public ChangeSourceControlRow(IVisualGitServiceProvider context, GitProject project)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            else if (project == null)
                throw new ArgumentNullException("project");

            _context = context;
            _project = project;
        }

        static Stack<ChangeSourceControlRow> _cloning;
        /// <summary>
        /// [Implementation detail of Clone()]
        /// </summary>
        public ChangeSourceControlRow()
        {
            ChangeSourceControlRow from = _cloning.Pop();
            _context = from._context;
            _project = from._project;
        }

        /// <summary>
        /// Creates an exact copy of this row.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Object"/> that represents the cloned <see cref="T:System.Windows.Forms.DataGridViewRow"/>.
        /// </returns>
        /// <PermissionSet>
        /// 	<IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/>
        /// 	<IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/>
        /// 	<IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence"/>
        /// 	<IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/>
        /// </PermissionSet>
        public override object Clone()
        {
            if (_cloning == null)
                _cloning = new Stack<ChangeSourceControlRow>();

            _cloning.Push(this);

            return base.Clone();
        }

        /// <summary>
        /// Gets the project.
        /// </summary>
        /// <value>The project.</value>
        public GitProject Project
        {
            get { return _project; }
        }

        IVisualGitSolutionSettings _solutionSettings;
        IVisualGitSolutionSettings SolutionSettings
        {
            get { return _solutionSettings ?? (_solutionSettings = _context.GetService<IVisualGitSolutionSettings>()); }
        }

        IProjectFileMapper _projectMap;
        IProjectFileMapper ProjectMap
        {
            get { return _projectMap ?? (_projectMap = _context.GetService<IProjectFileMapper>()); }
        }

        IVisualGitSccService _scc;
        IVisualGitSccService Scc
        {
            get { return _scc ?? (_scc = _context.GetService<IVisualGitSccService>()); }
        }

        IFileStatusCache _cache;
        IFileStatusCache Cache
        {
            get { return _cache ?? (_cache = _context.GetService<IFileStatusCache>()); }
        }

        string SafeToString(object value)
        {
            return value == null ? "" : value.ToString();
        }

        /// <summary>
        /// Refreshes this instance.
        /// </summary>
        public void Refresh()
        {
            IGitProjectInfo projectInfo;
            if (_project.IsSolution)
            {
                GitItem rootItem = SolutionSettings.ProjectRootGitItem;

                SetValues(
                    Scc.IsSolutionManaged,
                    "Solution: " + Path.GetFileNameWithoutExtension(SolutionSettings.SolutionFilename),
                    SafeRepositoryRoot(rootItem),
                    SafeRepositoryPath(rootItem),
                    GetStatus(rootItem, null, SolutionSettings.SolutionFilename),
                    EmptyToDot(PackageUtilities.MakeRelative(rootItem.FullPath, SvnTools.GetNormalizedDirectoryName(SolutionSettings.SolutionFilename))),
                    rootItem.FullPath
                    );
            }
            else if (null != (projectInfo = ProjectMap.GetProjectInfo(_project)) && null != (projectInfo.ProjectDirectory))
            {
                GitItem dirItem = Cache[projectInfo.SccBaseDirectory];

                SetValues(
                    Scc.IsProjectManaged(_project),
                    projectInfo.UniqueProjectName,
                    SafeRepositoryRoot(dirItem),
                    SafeRepositoryPath(dirItem),
                    GetStatus(dirItem, projectInfo, projectInfo.ProjectFile),
                    EmptyToDot(PackageUtilities.MakeRelative(projectInfo.SccBaseDirectory, projectInfo.ProjectDirectory)),
                    projectInfo.SccBaseDirectory
                    );
            }
            else
            {
                // Should have been filtered before; probably a buggy project that changed while the dialog was open
                SetValues(
                    false,
                    "-?-",
                    "-?-",
                    "-?-",
                    "-?-",
                    "-?-",
                    "-?-"
                    );
            }
        }

        private string GetStatus(GitItem dirItem, IGitProjectInfo projectInfo, string file)
        {
            if (dirItem == null || !dirItem.Exists || !dirItem.IsVersioned)
                return "<not found>";

            if (projectInfo == null)
            {
                if (Scc.IsSolutionManaged)
                    return "Connected"; // Solution itself + Connected
                else
                    return "Not Connected";
            }

            if (dirItem.IsBelowPath(SolutionSettings.ProjectRootGitItem)
                    && dirItem.WorkingCopy == SolutionSettings.ProjectRootGitItem.WorkingCopy)
            {
                // In master working copy
                if (Scc.IsSolutionManaged && Scc.IsProjectManaged(_project))
                    return "Connected";
                else
                    return "Valid"; // In master working copy
            }
            else if (Scc.IsSolutionManaged && Scc.IsProjectManaged(_project))
                return "Connected"; // Required information in solution
            else
                return "Detached"; // Separate working copy
        }

        string SafeRepositoryPath(GitItem item)
        {
            if (item == null || item.Uri == null)
                return "";

            GitWorkingCopy wc = item.WorkingCopy;
            if (wc != null)
            {
                Uri root = wc.RepositoryRoot;

                if (root != null)
                {
                    Uri relative = root.MakeRelativeUri(item.Uri);

                    if (!relative.IsAbsoluteUri)
                    {
                        string v = SvnTools.UriPartToPath(relative.ToString()).Replace(Path.DirectorySeparatorChar, '/');

                        if (!string.IsNullOrEmpty(v) && !v.StartsWith("/") && !v.StartsWith("../") && v != ".")
                            return "^/" + v;
                    }
                }
            }

            return item.Uri.ToString();
        }

        string SafeRepositoryRoot(GitItem item)
        {
            if (item == null || item.WorkingCopy == null)
                return "";

            Uri root = item.WorkingCopy.RepositoryRoot;

            if (root != null)
                return root.ToString();

            return "";
        }

        private string EmptyToDot(string value)
        {
            if (string.IsNullOrEmpty(value))
                return ".";
            else
                return value;
        }
    }
}
