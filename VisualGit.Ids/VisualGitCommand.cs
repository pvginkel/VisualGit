using System;
using System.Runtime.InteropServices;

namespace VisualGit
{
    // Disable Missing XML comment warning for this file
#pragma warning disable 1591

    /// <summary>
    /// List of VisualGit commands
    /// </summary>
    /// <remarks>
    /// <para>New items should be added at the end. Items should only be obsoleted between releases.</para>
    /// <para>The values of this enum are part of our interaction with other packages within Visual Studio</para>
    /// </remarks>
    [Guid(VisualGitId.CommandSet)]
    public enum VisualGitCommand // : int
    {
        None = 0,

        // Special task commands not used in menu's; only for use by VisualGit internally

        // Tick commands; one shot delayed task handlers
        TickFirst,
        MarkProjectDirty,
        FileCacheFinishTasks,
        SccFinishTasks,
        TickRefreshPendingTasks,
        TickRefreshGitItems,
        // /Tick Commands
        TickLast,

        // Delayed implementation commands
        OldActivateSccProvider,
        NotifyWcToNew,
        ForceUIShow,

        // /Private commands

        // These values live in the same numberspace as the other values within 
        // the command set. So we start countin at this number to make sure we
        // do not reuse values
        CommandFirst = 0x1FFFFFF,

        // Start of public commands; values shouldn't change between versions to
        // allow interop with other packages (like Collabnet Desktop and others)

        FileSccOpenFromGit,

        FileSccAddSolutionToGit,

        // Raw dump of old commands; to be sorted out
        AddItem,
        CheckForUpdates,
        WorkingCopyBrowse,
        ItemAnnotate,
        /// <summary>
        /// Execute blame command from blame window
        /// </summary>
        GitNodeAnnotate,
        ItemResolveCasing,
        CommitItem,
        CreatePatch,
        ItemShowChanges,
        DocumentShowChanges,
        DiffLocalItem,
        Export,
        Log,
        LogItem,
        Refresh,
        RemoveWorkingCopyExplorerRoot,
        DocumentAnnotate,
        DocumentHistory,

        AnnotateShowLog,
        RevertItem,
        DocumentConflictEdit,
        ShowPendingChanges,
        ShowWorkingCopyExplorer,
        SolutionBranch,
        SolutionTag,
        
        ItemSelectInWorkingCopyExplorer,
        ItemSelectInSolutionExplorer,

        CommitPendingChanges,

        SolutionCommit,
        SolutionHistory,
        SolutionMerge,

        ProjectCommit,
        ProjectHistory,
        ProjectMerge,

        PendingChangesViewFlat,
        PendingChangesViewProject,
        PendingChangesViewFolder,

        PendingChangesViewAll,
        PendingChangesViewLogMessage,

        ItemConflictEdit,
        ItemConflictEditVisualStudio,
        ItemConflictResolvedMerged,
        ItemConflictResolvedMineFull,
        ItemConflictResolvedTheirsFull,

        ItemCompareBase,
        ItemCompareLatest,
        ItemCompareCommitted,
        ItemComparePrevious,
        ItemCompareSpecific,

        ItemOpenVisualStudio,
        ItemOpenWindows,
        ItemOpenTextEditor,
        ItemOpenFolder,
        ItemOpenSolutionExplorer,

        ItemRevertBase,

        ItemIgnoreFile,
        ItemIgnoreFileType,
        ItemIgnoreFilesInFolder,

        ItemResolveMerge,

        ItemResolveMineFull,
        ItemResolveTheirsFull,
        ItemResolveMineConflict,
        ItemResolveTheirsConflict,
        ItemResolveBase,
        ItemResolveWorking,
        ItemResolveMergeTool,

        ItemMerge,

        RefreshPendingChanges,

        SolutionSwitchCombo,
        SolutionSwitchComboFill,
        SolutionSwitchDialog,

        PcLogEditorPasteFileList,
        PcLogEditorPasteRecentLog,

        LogCompareWithWorkingCopy,
        LogRevertThisRevision,
        LogMergeThisRevision,
        LogRevertTo,
        LogSwitchToRevision,

        ItemIgnoreFolder,

        PendingChangesApplyWorkingCopy,
        PendingChangesCreatePatch,
        SolutionApplyPatch,

        ExplorerOpen,

        RepositoryShowChanges,
        RepositoryCompareWithWc,

        ItemRename,
        ItemDelete,

        ItemAddToPending,
        ItemRemoveFromPending,

        LogFetchAll,
        LogShowChangedPaths,
        LogShowLogMessage,

        LogShowChanges,

        MigrateSettings,
        LogAnnotateRevision,


        UnifiedDiff,
        WorkingCopyAdd,

        SwitchProject,
        ProjectBranch,
        ProjectTag,

        ListViewSortAscending,
        ListViewSortDescending,
        ListViewSort0,
        ListViewSortMax = ListViewSort0 + 64,
        ListViewGroup0,
        ListViewGroupMax = ListViewGroup0 + 64,
        ListViewShow0,
        ListViewShowMax = ListViewShow0 + 64,

        DocumentAddToPending,
        DocumentRemoveFromPending,

        MakeNonSccFileWriteable,

        PendingChangesPush,
        PendingChangesPushSpecificBranch,
        PendingChangesPushSpecificTag,
        PendingChangesPull,
        PendingChangesPullEx,
    }
}
