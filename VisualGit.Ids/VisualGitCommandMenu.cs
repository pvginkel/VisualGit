// VisualGit.Ids\VisualGitCommandMenu.cs
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
using System.Runtime.InteropServices;

namespace VisualGit
{
    // Disable Missing XML comment warning for this file
#pragma warning disable 1591

    /// <summary>
    /// List of VisualGit menus
    /// </summary>
    /// <remarks>
    /// <para>New items should be added at the end. Items should only be obsoleted between releases.</para>
    /// <para>The values of this enum are part of our interaction with other packages within Visual Studio</para>
    /// </remarks>
    [Guid(VisualGitId.CommandSet)]
    public enum VisualGitCommandMenu
    {
        None = 0,

        // These values live in the same numberspace as the other values within 
        // the command set. So we start countin at this number to make sure we
        // do not reuse values
        MenuFirst = 0x5FFFFFF,

        FileScc,

        WorkingCopyExplorerToolBar,
        WorkingCopyExplorerContextMenu,

        SolutionExplorerSccForSolution,
        SolutionExplorerSccForProject,
        SolutionExplorerSccForItem,

        PendingCommitsContextMenu,
        PendingCommitsHeaderContextMenu,
        LogMessageEditorContextMenu,

        PendingChangesCommit,
        PendingChangesView,

        PendingChangesPush,
        PendingChangesPull,

        PendingCommitsView,
        PendingCommitsSort,

        PendingCommitsGroup,

        ItemConflict,
        ItemCompare,
        ItemOpen,
        ItemRevert,
        ItemResolve,

        ItemIgnore,

        LogViewerContextMenu,
        LogChangedPathsContextMenu,

        ListViewSort,
        ListViewGroup,
        ListViewShow,
        ListViewHeader,

        SolutionFileScc,
        ProjectFileScc,
        
        AnnotateContextMenu,

        EditorScc,

        MdiSccMenu
    }
}
