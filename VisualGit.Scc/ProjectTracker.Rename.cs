// VisualGit.Scc\ProjectTracker.Rename.cs
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
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using System.IO;
using System.Diagnostics;
using SharpGit;

namespace VisualGit.Scc
{
    partial class ProjectTracker
    {
        public int OnQueryRenameFiles(IVsProject pProject, int cFiles, string[] rgszMkOldNames, string[] rgszMkNewNames, VSQUERYRENAMEFILEFLAGS[] rgFlags, VSQUERYRENAMEFILERESULTS[] pSummaryResult, VSQUERYRENAMEFILERESULTS[] rgResults)
        {
            if (rgszMkNewNames == null || rgszMkOldNames == null)
                return VSConstants.E_POINTER;

            bool allOk = true;
            if (pProject == null)
            {
                // We are renaming the solution file (or something in the solution itself)

                for (int i = 0; i < cFiles; i++)
                {
                    bool ok = true;

                    if(!GitItem.IsValidPath(rgszMkOldNames[i]))
                        continue;

                    string oldName = GitTools.GetNormalizedFullPath(rgszMkOldNames[i]);
                    string newName = GitTools.GetNormalizedFullPath(rgszMkNewNames[i]);

                    if(oldName == newName)
                        continue;

                    SccProvider.OnBeforeSolutionRenameFile(oldName,newName, rgFlags[i], out ok);

                    if (rgResults != null)
                        rgResults[i] = ok ? VSQUERYRENAMEFILERESULTS.VSQUERYRENAMEFILERESULTS_RenameOK : VSQUERYRENAMEFILERESULTS.VSQUERYRENAMEFILERESULTS_RenameNotOK;

                    if (!ok)
                        allOk = false;
                }
            }
            else
            {
                IVsSccProject2 sccProject = pProject as IVsSccProject2;
                bool track = SccProvider.TrackProjectChanges(sccProject);

                for (int i = 0; i < cFiles; i++)
                {
                    bool ok = true;

                    if (!GitItem.IsValidPath(rgszMkOldNames[i]))
                        continue;

                    string oldName = GitTools.GetNormalizedFullPath(rgszMkOldNames[i]);
                    string newName = GitTools.GetNormalizedFullPath(rgszMkNewNames[i]);

                    if (oldName == newName)
                        continue;

                    if (track)
                        SccProvider.OnBeforeProjectRenameFile(sccProject, oldName,
                            newName, rgFlags[i], out ok);

                    if (rgResults != null)
                        rgResults[i] = ok ? VSQUERYRENAMEFILERESULTS.VSQUERYRENAMEFILERESULTS_RenameOK : VSQUERYRENAMEFILERESULTS.VSQUERYRENAMEFILERESULTS_RenameNotOK;

                    if (!ok)
                        allOk = false;
                }
            }

            if (pSummaryResult != null)
                pSummaryResult[0] = allOk ? VSQUERYRENAMEFILERESULTS.VSQUERYRENAMEFILERESULTS_RenameOK : VSQUERYRENAMEFILERESULTS.VSQUERYRENAMEFILERESULTS_RenameNotOK;

            if (!allOk)
                _batchOk = false;

            return VSConstants.S_OK;
        }

        public int OnAfterRenameFiles(int cProjects, int cFiles, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgszMkOldNames, string[] rgszMkNewNames, VSRENAMEFILEFLAGS[] rgFlags)
        {
            if (rgszMkNewNames == null || rgpProjects == null || rgszMkOldNames == null || rgszMkOldNames.Length != rgszMkNewNames.Length)
                return VSConstants.E_POINTER;

            // TODO: C++ projects do not send directory renames; but do send OnAfterRenameFile() events
            //       for all files (one at a time). We should detect that case here and fix up this dirt!

            int iFile = 0;

            for (int i = 0; i < cFiles; i++)
            {
                string s = rgszMkOldNames[i];
                if (!string.IsNullOrEmpty(s) && GitItem.IsValidPath(s))
                    StatusCache.MarkDirty(s);

                s = rgszMkNewNames[i];
                if (!string.IsNullOrEmpty(s) && GitItem.IsValidPath(s))
                    StatusCache.MarkDirty(s);
            }

            if (SccProvider.IsActive)
            {
                FixWorkingCopyAfterRenames(rgszMkOldNames, rgszMkNewNames);

                for (int iProject = 0; (iProject < cProjects) && (iFile < cFiles); iProject++)
                {
                    int iLastFileThisProject = (iProject < cProjects - 1) ? rgFirstIndices[iProject + 1] : cFiles;

                    if (rgpProjects[iProject] != null)
                    {
                        IVsSccProject2 sccProject = rgpProjects[iProject] as IVsSccProject2;

                        bool track = SccProvider.TrackProjectChanges(sccProject);

                        for (; iFile < iLastFileThisProject; iFile++)
                        {
                            if (sccProject == null || !track)
                                continue; // Not handled by our provider

                            if (string.IsNullOrEmpty(rgszMkOldNames[iFile]) || !GitItem.IsValidPath(rgszMkOldNames[iFile]))
                                continue;

                            string oldName = GitTools.GetNormalizedFullPath(rgszMkOldNames[iFile]);
                            string newName = GitTools.GetNormalizedFullPath(rgszMkNewNames[iFile]);

                            if (oldName == newName)
                                continue;

                            SccProvider.OnProjectRenamedFile(sccProject, oldName, newName,rgFlags[iFile]);
                        }
                    }
                    else
                    {
                        // Renaming something in the solution (= solution file itself)

                        for (; iFile < iLastFileThisProject; iFile++)
                        {
                            if (string.IsNullOrEmpty(rgszMkOldNames[iFile]) || !GitItem.IsValidPath(rgszMkOldNames[iFile]))
                                continue;

                            string oldName = GitTools.GetNormalizedFullPath(rgszMkOldNames[iFile]);
                            string newName = GitTools.GetNormalizedFullPath(rgszMkNewNames[iFile]);

                            if (oldName == newName)
                                continue;

                            SccProvider.OnSolutionRenamedFile(oldName, newName, rgFlags[iFile]);
                        }
                    }
                }
            }

            return VSConstants.S_OK;
        }

        /// <summary>
        /// Fixes working copies which are invalidated by a rename operation
        /// </summary>
        /// <param name="rgszMkOldNames"></param>
        /// <param name="rgszMkNewNames"></param>
        private void FixWorkingCopyAfterRenames(string[] rgszMkOldNames, string[] rgszMkNewNames)
        {
            if (rgszMkNewNames == null || rgszMkOldNames == null || rgszMkOldNames.Length != rgszMkNewNames.Length)
                return;

            for (int i = 0; i < rgszMkOldNames.Length; i++)
            {
                string oldName = rgszMkOldNames[i];
                string newName = rgszMkNewNames[i];

                if (string.IsNullOrEmpty(oldName) || !GitItem.IsValidPath(oldName))
                    continue;

                oldName = GitTools.GetNormalizedFullPath(oldName);
                newName = GitTools.GetNormalizedFullPath(newName);
                
                string oldDir;
                string newDir;
                bool safeRename =false;

                if (!Directory.Exists(newName))
                {
                    // Try to fix the parent of the new item
                     oldDir = GitTools.GetNormalizedDirectoryName(oldName);
                     newDir = GitTools.GetNormalizedDirectoryName(newName);
                }
                else
                {
                    // The item itself is the directory to fix
                    oldDir = oldName;
                    newDir = newName;
                    safeRename = true;
                }                                

                if (Directory.Exists(oldDir))
                    continue; // Nothing to fix up

                string parent = GitTools.GetNormalizedDirectoryName(oldDir);
                if (!Directory.Exists(parent))
                {
                    continue; // We can't fix up more than one level at this time
                    // We probably fix it with one of the following renames; as paths are included too
                }

                GitItem item = StatusCache[oldDir];

                if (!item.IsVersioned && item.Status.State != GitStatus.Missing)
                    continue; // Item was not cached as versioned or now-missing (Missing implicits Versioned)

                StatusCache.MarkDirty(oldDir);
                StatusCache.MarkDirty(newDir);

                item = StatusCache[oldDir];

                if (item.Status.State != GitStatus.Missing)
                    continue;

                GitItem newItem = StatusCache[newDir];

                using (GitSccContext git = new GitSccContext(Context))
                {
                    GitStatusEventArgs wa = git.SafeGetEntry(newDir);
                    string newParent = GitTools.GetNormalizedDirectoryName(newDir);

                    if (wa != null)
                        continue; // Not an unexpected WC root
                    else if (!GitTools.IsBelowManagedPath(newDir))
                        continue; // Not a wc root at all

                    git.SafeWcDirectoryCopyFixUp(oldDir, newDir, safeRename); // Recreate the old WC directory

                    _delayedDeletes.Add(oldDir); // Delete everything in the old wc when done
                    // TODO: Once Git understands true renames, fixup the renames in the delayed hook
                    RegisterForSccCleanup();

                    // We have all files of the old wc directory unversioned in the new location now

                    StatusCache.MarkDirtyRecursive(oldDir);
                    StatusCache.MarkDirtyRecursive(newDir);
                }
            }
        }

        public int OnQueryRenameDirectories(IVsProject pProject, int cDirs, string[] rgszMkOldNames, string[] rgszMkNewNames, VSQUERYRENAMEDIRECTORYFLAGS[] rgFlags, VSQUERYRENAMEDIRECTORYRESULTS[] pSummaryResult, VSQUERYRENAMEDIRECTORYRESULTS[] rgResults)
        {
            if (rgszMkNewNames == null || pProject == null || rgszMkOldNames == null)
                return VSConstants.E_POINTER;

            IVsSccProject2 sccProject = pProject as IVsSccProject2;
            bool track = SccProvider.TrackProjectChanges(sccProject);

            if (track)
                for (int i = 0; i < cDirs; i++)
                {
                    string s = rgszMkNewNames[i];
                    if (!string.IsNullOrEmpty(s))
                        StatusCache.MarkDirty(s);
                }

            bool allOk = true;
            for (int i = 0; i < cDirs; i++)
            {
                bool ok = true;

                if (track)
                    SccProvider.OnBeforeProjectDirectoryRename(sccProject,
                        GitTools.GetNormalizedFullPath(rgszMkOldNames[i]),
                        GitTools.GetNormalizedFullPath(rgszMkNewNames[i]), rgFlags[i], out ok);

                if (rgResults != null)
                    rgResults[i] = ok ? VSQUERYRENAMEDIRECTORYRESULTS.VSQUERYRENAMEDIRECTORYRESULTS_RenameOK : VSQUERYRENAMEDIRECTORYRESULTS.VSQUERYRENAMEDIRECTORYRESULTS_RenameNotOK;

                if (!ok)
                    allOk = false;
            }

            if (pSummaryResult != null)
                pSummaryResult[0] = allOk ? VSQUERYRENAMEDIRECTORYRESULTS.VSQUERYRENAMEDIRECTORYRESULTS_RenameOK : VSQUERYRENAMEDIRECTORYRESULTS.VSQUERYRENAMEDIRECTORYRESULTS_RenameNotOK;

            if (!allOk)
                _batchOk = false;

            return VSConstants.S_OK;
        }

        public int OnAfterRenameDirectories(int cProjects, int cDirs, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgszMkOldNames, string[] rgszMkNewNames, VSRENAMEDIRECTORYFLAGS[] rgFlags)
        {
            if (rgszMkNewNames == null || rgpProjects == null || rgszMkOldNames == null)
                return VSConstants.E_POINTER;

            int iDirectory = 0;

            for (int i = 0; i < cDirs; i++)
            {
                string s = rgszMkOldNames[i];
                if (!string.IsNullOrEmpty(s))
                    StatusCache.MarkDirty(s);

                s = rgszMkNewNames[i];
                if (!string.IsNullOrEmpty(s))
                    StatusCache.MarkDirty(s);
            }

            for (int iProject = 0; (iProject < cProjects) && (iDirectory < cDirs); iProject++)
            {
                int iLastDirectoryThisProject = (iProject < cProjects - 1) ? rgFirstIndices[iProject + 1] : cDirs;

                IVsSccProject2 sccProject = rgpProjects[iProject] as IVsSccProject2;

                bool track = SccProvider.TrackProjectChanges(sccProject);

                for (; iDirectory < iLastDirectoryThisProject; iDirectory++)
                {
                    if (sccProject == null || !track)
                        continue; // Not handled by our provider

                    SccProvider.OnProjectDirectoryRenamed(sccProject,
                        GitTools.GetNormalizedFullPath(rgszMkOldNames[iDirectory]),
                        GitTools.GetNormalizedFullPath(rgszMkNewNames[iDirectory]), rgFlags[iDirectory]);
                }
            }

            return VSConstants.S_OK;
        }
    }
}
