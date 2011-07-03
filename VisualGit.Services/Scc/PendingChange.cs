// VisualGit.Services\Scc\PendingChange.cs
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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using VisualGit.Scc;
using VisualGit.Selection;
using VisualGit.VS;
using System.IO;
using SharpGit;

namespace VisualGit.Scc
{
    [DebuggerDisplay("File={FullPath}, Change={ChangeText}")]
    public sealed class PendingChange : GitItemData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PendingChange"/> class
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="item">The item.</param>
        public PendingChange(RefreshContext context, GitItem item)
            : base(context, item)
        {
            Refresh(context, item);
        }  

        [DisplayName("Project"), Category("Visual Studio")]
        public new string Project
        {
            get { return _projects; }
        }

        [DisplayName("Change"), Category("Git")]
        public string ChangeText
        {
            get { return _status.Text; }
        }

        protected override string ClassName
        {
            get { return "Pending Change"; }
        }

        protected override string ComponentName
        {
            get { return Name; }
        } 

        /// <summary>
        /// Gets a boolean indicating whether this pending change is clear / is no longer a pending change
        /// </summary>
        [Browsable(false)]
        public bool IsClean
        {
            get { return !PendingChange.IsPending(GitItem); }
        }

        int _iconIndex;
        string _projects;
        string _relativePath;
        PendingChangeStatus _status;
        PendingChangeKind _kind;
        string _fileType;

        [Browsable(false)]
        public int IconIndex
        {
            get { return _iconIndex; }
        }

        [Browsable(false)]
        public PendingChangeKind Kind
        {
            get { return _kind; }
        }

        [Browsable(false)]
        public new PendingChangeStatus Change
        {
            get { return _status; }
        }

        [Browsable(false)]
        public string RelativePath
        {
            get { return _relativePath; }
        }

        [Browsable(false)]
        public string FileType
        {
            get { return _fileType; }
        }

        [Browsable(false)]
        public string LogMessageToolTipText
        {
            get { return String.Format("{0}: {1}", RelativePath, Change.PendingCommitText); }
        }

        /// <summary>
        /// Refreshes the pending change. Returns true if the state was modified, otherwise false
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Refresh(RefreshContext context, GitItem item)
        {
            bool m = false;

            RefreshValue(ref m, ref _iconIndex, GetIcon(context));
            RefreshValue(ref m, ref _projects, GetProjects(context));
            RefreshValue(ref m, ref _status, GetStatus(context, item));
            RefreshValue(ref m, ref _relativePath, GetRelativePath(context));
            RefreshValue(ref m, ref _fileType, GetFileType(context, item));

            return m || (_status == null);
        }

        private string GetFileType(RefreshContext context, GitItem item)
        {
            return context.IconMapper.GetFileType(item.FullPath);
        }        

        private string GetRelativePath(RefreshContext context)
        {
            string projectRoot = context.SolutionSettings.ProjectRootWithSeparator;

            if (!string.IsNullOrEmpty(projectRoot) && FullPath.StartsWith(projectRoot, StringComparison.OrdinalIgnoreCase))
                return FullPath.Substring(projectRoot.Length).Replace('\\', '/');
            else
                return FullPath;
        }

        string GetProjects(RefreshContext context)
        {
            string name = null;
            foreach (GitProject project in context.ProjectFileMapper.GetAllProjectsContaining(FullPath))
            {
                IGitProjectInfo info = context.ProjectFileMapper.GetProjectInfo(project);

                if (info == null)
                {
                    // Handle the case the solution file is in a project (Probably website)
                    if (string.Equals(FullPath, context.SolutionSettings.SolutionFilename))
                        return "<Solution>";
                    continue;
                }

                if (name != null)
                    name += ";" + info.UniqueProjectName;
                else
                    name = info.UniqueProjectName;
            }

            if (!string.IsNullOrEmpty(name))
                return name;
            else if (string.Equals(FullPath, context.SolutionSettings.SolutionFilename))
                return "<Solution>";
            else
                return "<none>";
        }

        int GetIcon(RefreshContext context)
        {
            if (GitItem.Exists)
            {
                if (GitItem.IsDirectory || GitItem.NodeKind == GitNodeKind.Directory)
                    return context.IconMapper.DirectoryIcon; // Is or was a directory
                else
                    return context.IconMapper.GetIcon(FullPath);
            }
            else if (GitItem.Status != null && GitItem.Status.NodeKind == GitNodeKind.Directory)
                return context.IconMapper.DirectoryIcon;
            else
                return context.IconMapper.GetIconForExtension(GitItem.Extension);
        }

        PendingChangeStatus GetStatus(RefreshContext context, GitItem item)
        {
            VisualGitStatus status = item.Status;
            _kind = CombineStatus(status.State, item.IsTreeConflicted, item);

            if (_kind != PendingChangeKind.None)
                return new PendingChangeStatus(_kind);
            else
                return null;
        }

        static void RefreshValue<T>(ref bool changed, ref T field, T newValue)
            where T : class, IEquatable<T>
        {
            if (field == null || !field.Equals(newValue))
            {
                changed = true;
                field = newValue;
            }
        }

        static void RefreshValue(ref bool changed, ref int field, int newValue)
        {
            if (field != newValue)
            {
                changed = true;
                field = newValue;
            }
        }

        public static bool IsPending(GitItem item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            bool create = false;
            if (item.IsConflicted)
                create = true; // Tree conflict (unversioned) or other conflict
            else if (item.IsModified)
                create = true; // Must commit
            else if (item.InSolution && !item.IsVersioned && !item.IsIgnored && item.IsVersionable)
                create = true; // To be added
            else if (item.IsVersioned && item.IsDocumentDirty)
                create = true;

            return create;
        }

        /// <summary>
        /// Creates if pending.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="isDirty">if set to <c>true</c> [is dirty].</param>
        /// <param name="pc">The pc.</param>
        /// <returns></returns>
        public static bool CreateIfPending(RefreshContext context, GitItem item, out PendingChange pc)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            else if (item == null)
                throw new ArgumentNullException("item");

            if (IsPending(item))
            {
                pc = new PendingChange(context, item);
                return true;
            }

            pc = null;
            return false;
        }

        public sealed class RefreshContext : IVisualGitServiceProvider
        {
            readonly IVisualGitServiceProvider _context;
            public RefreshContext(IVisualGitServiceProvider context)
            {
                if (context == null)
                    throw new ArgumentNullException("context");
                _context = context;
            }

            IProjectFileMapper _fileMapper;
            IFileIconMapper _iconMapper;
            IVisualGitSolutionSettings _solutionSettings;

            public IProjectFileMapper ProjectFileMapper
            {
                [DebuggerStepThrough]
                get { return _fileMapper ?? (_fileMapper = GetService<IProjectFileMapper>()); }
            }

            public IFileIconMapper IconMapper
            {
                [DebuggerStepThrough]
                get { return _iconMapper ?? (_iconMapper = GetService<IFileIconMapper>()); }
            }

            public IVisualGitSolutionSettings SolutionSettings
            {
                [DebuggerStepThrough]
                get { return _solutionSettings ?? (_solutionSettings = GetService<IVisualGitSolutionSettings>()); }
            }

            #region IVisualGitServiceProvider Members
            [DebuggerStepThrough]
            public T GetService<T>() where T : class
            {
                return _context.GetService<T>();
            }

            [DebuggerStepThrough]
            public T GetService<T>(Type serviceType) where T : class
            {
                return _context.GetService<T>(serviceType);
            }

            [DebuggerStepThrough]
            public object GetService(Type serviceType)
            {
                return _context.GetService(serviceType);
            }
            #endregion
        }

        /// <summary>
        /// Determines whether the bool is a local change only (e.g. locked) and does not need user selection
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if [is cleanup change]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsChangeForPatching()
        {
            switch (Kind)
            {
                case PendingChangeKind.LockedOnly:
                    return true;
                case PendingChangeKind.TreeConflict:
                    return !GitItem.IsVersioned;
            }

            return false;
        }

        /// <summary>
        /// Determines whether the item is (below) the specified path
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>
        /// 	<c>true</c> if the item is (below) the specified path; otherwise, <c>false</c>.
        /// </returns>
        public bool IsBelowPath(string path)
        {
            return GitItem.IsBelowPath(path);
        }

        /// <summary>
        /// Combines the statuses to a single PendingChangeKind status for UI purposes
        /// </summary>
        /// <param name="contentStatus">The content status.</param>
        /// <param name="propertyStatus">The property status.</param>
        /// <param name="treeConflict">if set to <c>true</c> [tree conflict].</param>
        /// <param name="item">The item or null if no on disk representation is availavke</param>
        /// <returns></returns>
        public static PendingChangeKind CombineStatus(GitStatus contentStatus, bool treeConflict, GitItem item)
        {
            // item can be null!
            if (treeConflict || (item != null && item.IsTreeConflicted))
                return PendingChangeKind.TreeConflict;
            else if (contentStatus == GitStatus.Conflicted)
                return PendingChangeKind.Conflicted;

            switch (contentStatus)
            {
                case GitStatus.NotVersioned:
                    if (item != null)
                    {
                        if (item.IsIgnored)
                            return PendingChangeKind.Ignored;
                        else if (item.InSolution)
                            return PendingChangeKind.New;
                    }
                    return PendingChangeKind.None;
                case GitStatus.Modified:
                    return PendingChangeKind.Modified;
                case GitStatus.Added:
                    if (item != null && item.HasCopyableHistory)
                        return PendingChangeKind.Copied;

                    return PendingChangeKind.Added;
                case GitStatus.Deleted:
                    return PendingChangeKind.Deleted;
                case GitStatus.Missing:
                    if (item != null && item.IsCasingConflicted)
                        return PendingChangeKind.WrongCasing;
                    else
                        return PendingChangeKind.Missing;
                case GitStatus.Obstructed:
                    return PendingChangeKind.Obstructed;
                case GitStatus.Incomplete:
                    return PendingChangeKind.Incomplete;
                case GitStatus.None:
                case GitStatus.Normal:
                case GitStatus.Ignored:
                    // No usefull status / No change
                    break;

                case GitStatus.Zero:
                case GitStatus.Conflicted:
                case GitStatus.Merged:
                default: // Give error on missed values
                    throw new ArgumentOutOfRangeException("contentStatus", contentStatus, "Invalid content status");
            }

            if (item != null && item.IsDocumentDirty)
                return PendingChangeKind.EditorDirty;

            return PendingChangeKind.None;
        }

        /// <summary>
        /// Gets a value indicating whether this instance can be applied to a working copy
        /// </summary>
        /// <value><c>true</c> if this instance can apply; otherwise, <c>false</c>.</value>
        [Browsable(false)]
        public bool CanApply
        {
            get 
            {
                switch (Kind)
                {
                    case PendingChangeKind.New:
                        return true;
                    case PendingChangeKind.WrongCasing:
                        return true;
                    default:
                        return false;
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class PendingChangeCollection : KeyedCollection<string, PendingChange>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PendingChangeCollection"/> class.
        /// </summary>
        public PendingChangeCollection()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        /// <summary>
        /// Extracts the FullPath from the specified element.
        /// </summary>
        /// <param name="item">The element from which to extract the key.</param>
        /// <returns>The key for the specified element.</returns>
        protected override string GetKeyForItem(PendingChange item)
        {
            return item.FullPath;
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public bool TryGetValue(string key, out PendingChange value)
        {
            if (Dictionary != null)
                return Dictionary.TryGetValue(key, out value);

            foreach (PendingChange p in this)
            {
                if (String.Equals(p.FullPath, key, StringComparison.OrdinalIgnoreCase))
                {
                    value = p;
                    return true;
                }
            }

            value = null;
            return false;
        }
    }
}
