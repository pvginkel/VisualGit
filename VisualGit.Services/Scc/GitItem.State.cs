// VisualGit.Services\Scc\GitItem.State.cs
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
using System.Diagnostics;
using VisualGit.Scc;
using SharpGit;

namespace VisualGit
{
    public interface IGitItemStateUpdate
    {
        IList<GitItem> GetUpdateQueueAndClearScheduled();

        void SetDocumentDirty(bool value);
        void SetSolutionContained(bool value);
    }

    partial class GitItem : IGitItemStateUpdate
    {
        GitItemState _currentState;
        GitItemState _validState;
        GitItemState _onceValid;

        const GitItemState MaskRefreshTo =
            GitItemState.Versioned | GitItemState.Obstructed | GitItemState.Modified
            | GitItemState.Added | GitItemState.HasCopyOrigin | GitItemState.Deleted
            | GitItemState.ContentConflicted | GitItemState.GitDirty | GitItemState.Ignored;

        public GitItemState GetState(GitItemState flagsToGet)
        {
            GitItemState unavailable = flagsToGet & ~_validState;

            if (unavailable == 0)
                return _currentState & flagsToGet; // We have everything we need

            if (0 != (unavailable & MaskRefreshTo))
            {
                Debug.Assert(_statusDirty != XBool.False);
                RefreshStatus();

                unavailable = flagsToGet & ~_validState;

                Debug.Assert((~_validState & MaskRefreshTo) == 0, "RefreshMe() set all attributes it should");
            }

            if (0 != (unavailable & MaskGetAttributes))
            {
                UpdateAttributeInfo();

                unavailable = flagsToGet & ~_validState;

                Debug.Assert((~_validState & MaskGetAttributes) == 0, "UpdateAttributeInfo() set all attributes it should");
            }

            if (0 != (unavailable & MaskUpdateSolution))
            {
                UpdateSolutionInfo();

                unavailable = flagsToGet & ~_validState;

                Debug.Assert((~_validState & MaskUpdateSolution) == 0, "UpdateSolution() set all attributes it should");
            }

            if (0 != (unavailable & MaskDocumentInfo))
            {
                UpdateDocumentInfo();

                unavailable = flagsToGet & ~_validState;

                Debug.Assert((~_validState & MaskDocumentInfo) == 0, "UpdateDocumentInfo() set all attributes it should");
            }

            if (0 != (unavailable & MaskVersionable))
            {
                UpdateVersionable();

                unavailable = flagsToGet & ~_validState;

                Debug.Assert((~_validState & MaskVersionable) == 0, "UpdateVersionable() set all attributes it should");
            }

            if (0 != (unavailable & MaskTextFile))
            {
                UpdateTextFile();

                unavailable = flagsToGet & ~_validState;

                Debug.Assert((~_validState & MaskTextFile) == 0, "UpdateTextFile() set all attributes it should");
            }

            if (0 != (unavailable & MaskIsAdministrativeArea))
            {
                UpdateAdministrativeArea();

                unavailable = flagsToGet &~_validState;

                Debug.Assert((~_validState & MaskIsAdministrativeArea) == 0, "UpdateIsAdministrativeArea() set all attributes it should");
            }

            if (unavailable != 0)
            {
                Trace.WriteLine(string.Format("Don't know how to retrieve {0:X} state; clearing dirty flag", (int)unavailable));

                _validState |= unavailable;
            }

            return _currentState & flagsToGet;
        }

        IList<GitItem> IGitItemStateUpdate.GetUpdateQueueAndClearScheduled()
        {
            lock (_stateChanged)
            {
                _scheduled = false;

                if (_stateChanged.Count == 0)
                    return null;

                List<GitItem> modified = new List<GitItem>(_stateChanged.Count);
                modified.AddRange(_stateChanged);
                _stateChanged.Clear();

                foreach (GitItem i in modified)
                    i._enqueued = false;


                return modified;
            }
        }

        private void SetDirty(GitItemState dirty)
        {
            // NOTE: This method is /not/ thread safe, but its callers have race conditions anyway
            // Setting an integer could worst case completely destroy the integer; nothing a refresh can't fix

            _validState &= ~dirty;
        }

        // Mask of states not to broadcast for
        const GitItemState NoBroadcastFor = ~(GitItemState.DocumentDirty | GitItemState.InSolution);

        void SetState(GitItemState set, GitItemState unset)
        {
            // NOTE: This method is /not/ thread safe, but its callers have race conditions anyway
            // Setting an integer could worst case completely destroy the integer; nothing a refresh can't fix

            GitItemState st = (_currentState & ~unset) | set;

            if (st != _currentState)
            {
                // Calculate whether we have a change or just new information
                bool changed = (st & _onceValid & NoBroadcastFor) != (_currentState & _onceValid & NoBroadcastFor);

                if (changed && !_enqueued)
                {
                    _enqueued = true;

                    // Schedule a stat changed broadcast
                    lock (_stateChanged)
                    {
                        _stateChanged.Enqueue(this);

                        ScheduleUpdateNotify();
                    }
                }

                _currentState = st;

            }
            _validState |= (set | unset);
            _onceValid |= _validState;
        }

        void IGitItemUpdate.SetState(GitItemState set, GitItemState unset)
        {
            SetState(set, unset);
        }
        void IGitItemUpdate.SetDirty(GitItemState dirty)
        {
            SetDirty(dirty);
        }
        bool IGitItemUpdate.TryGetState(GitItemState get, out GitItemState value)
        {
            return TryGetState(get, out value);
        }

        #region Versionable

        const GitItemState MaskVersionable = GitItemState.Versionable;

        void UpdateVersionable()
        {
            bool versionable;

            GitItemState state;

            if (TryGetState(GitItemState.Versioned, out state) && state != 0)
                versionable = true;
            else if (Exists && GitTools.IsBelowManagedPath(FullPath)) // Will call GetState again!
                versionable = true;
            else
                versionable = false;

            if (versionable)
                SetState(GitItemState.Versionable, GitItemState.None);
            else
                SetState(GitItemState.None, GitItemState.Versionable);
        }

        #endregion

        #region DocumentInfo

        const GitItemState MaskDocumentInfo = GitItemState.DocumentDirty;

        void UpdateDocumentInfo()
        {
            IVisualGitOpenDocumentTracker dt = _context.GetService<IVisualGitOpenDocumentTracker>();

            if (dt == null)
            {
                // We /must/ make the state not dirty
                SetState(GitItemState.None, GitItemState.DocumentDirty);
                return;
            }

            if (dt.IsDocumentDirty(FullPath, true))
                SetState(GitItemState.DocumentDirty, GitItemState.None);
            else
                SetState(GitItemState.None, GitItemState.DocumentDirty);
        }

        #endregion

        #region Solution Info
        const GitItemState MaskUpdateSolution = GitItemState.InSolution;
        void UpdateSolutionInfo()
        {
            IProjectFileMapper pfm = _context.GetService<IProjectFileMapper>();
            bool inSolution = false;

            if (pfm != null)
                inSolution = pfm.ContainsPath(FullPath);

            if (inSolution)
                SetState(GitItemState.InSolution, GitItemState.None);
            else
                SetState(GitItemState.None, GitItemState.InSolution);
        }

        void IGitItemStateUpdate.SetSolutionContained(bool inSolution)
        {
            if (inSolution)
                SetState(GitItemState.InSolution, GitItemState.None);
            else
                SetState(GitItemState.None, GitItemState.InSolution);
        }

        #endregion

        #region TextFile File
        const GitItemState MaskTextFile = GitItemState.IsTextFile;
        void UpdateTextFile()
        {
            GitItemState value = GitItemState.IsDiskFile | GitItemState.Versioned;
            GitItemState v;

            bool isTextFile;

            if (TryGetState(GitItemState.Versioned, out v) && (v == 0))
                isTextFile = false;
            else if (GetState(value) != value)
                isTextFile = false;
            else
                isTextFile = true;

            if (isTextFile)
                SetState(GitItemState.IsTextFile, GitItemState.None);
            else
                SetState(GitItemState.None, GitItemState.IsTextFile);
        }
        #endregion

        #region Attribute Info
        const GitItemState MaskGetAttributes = GitItemState.Exists | GitItemState.ReadOnly | GitItemState.IsDiskFile | GitItemState.IsDiskFolder;

        void UpdateAttributeInfo()
        {
            // One call of the kernel's GetFileAttributesW() gives us most info we need

            uint value = NativeMethods.GetFileAttributes(FullPath);

            if (value == NativeMethods.INVALID_FILE_ATTRIBUTES)
            {
                // File does not exist / no rights, etc.

                SetState(GitItemState.None,
                    GitItemState.Exists | GitItemState.ReadOnly | GitItemState.Versionable | GitItemState.IsDiskFolder | GitItemState.IsDiskFile);

                return;
            }

            GitItemState set = GitItemState.Exists;
            GitItemState unset = GitItemState.None;

            if ((value & NativeMethods.FILE_ATTRIBUTE_READONLY) != 0)
                set |= GitItemState.ReadOnly;
            else
                unset = GitItemState.ReadOnly;

            if ((value & NativeMethods.FILE_ATTRIBUTE_DIRECTORY) != 0)
            {
                unset |= GitItemState.IsDiskFile | GitItemState.ReadOnly;
                set = GitItemState.IsDiskFolder | (set & ~GitItemState.ReadOnly); // Don't set readonly
            }
            else
            {
                set |= GitItemState.IsDiskFile;
                unset |= GitItemState.IsDiskFolder;
            }

            SetState(set, unset);
        }
        #endregion

        #region Administrative Area
        const GitItemState MaskIsAdministrativeArea = GitItemState.IsAdministrativeArea;
        void UpdateAdministrativeArea()
        {
            if(string.Equals(Name, GitConstants.AdministrativeDirectoryName, StringComparison.OrdinalIgnoreCase))
                SetState(GitItemState.IsAdministrativeArea, GitItemState.None);
            else
                SetState(GitItemState.None, GitItemState.IsAdministrativeArea);
        }
        #endregion

        #region IGitItemStateUpdate Members

        void IGitItemStateUpdate.SetDocumentDirty(bool value)
        {
            if (value)
                SetState(GitItemState.DocumentDirty, GitItemState.None);
            else
                SetState(GitItemState.None, GitItemState.DocumentDirty);
        }

        #endregion
    }
}
