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

        FileFileOpenFromGit,
        FileFileAddFromGit,

        FileSccOpenFromGit,
        FileSccAddFromGit,

        FileSccAddSolutionToGit,
        FileSccAddProjectToGit,

        FileSccChangeSourceControl,

        FileSccMenuUpdateLatest,
        FileSccMenuUpdateSpecific,

        // Raw dump of old commands; to be sorted out
        AddItem,
        RepositoryBrowse,
        CheckForUpdates,
        WorkingCopyBrowse,
        ItemAnnotate,
        /// <summary>
        /// Execute blame command from blame window
        /// </summary>
        GitNodeAnnotate,
        Checkout,
        CopyToWorkingCopy,
        ItemResolveCasing,
        Cleanup,
        CommitItem,
        CopyReposExplorerUrl,
        CreatePatch,
        ItemShowChanges,
        DocumentShowChanges,
        DiffLocalItem,
        Export,
        Log,
        LogItem,
        NewDirectory,
        Refresh,
        RefreshRepositoryItem,
        RemoveRepositoryRoot,
        RemoveWorkingCopyExplorerRoot,
        DocumentAnnotate,
        DocumentHistory,

        AnnotateShowLog,
        RevertItem,
        SaveToFile,
        DocumentConflictEdit,
        ShowPendingChanges,
        ShowRepositoryExplorer,
        ShowWorkingCopyExplorer,
        RenameRepositoryItem,
        UpdateItemSpecific,
        UpdateItemLatest,
        ViewInVsNet,
        ViewInWindows,
        SolutionBranch,
        
        ItemSelectInRepositoryExplorer,
        ItemSelectInWorkingCopyExplorer,
        ItemSelectInSolutionExplorer,

        CommitPendingChanges,

        PendingChangesUpdateLatest,
        SolutionUpdateSpecific,
        SolutionCommit,
        SolutionHistory,
        SolutionMerge,

        ProjectUpdateLatest,
        ProjectUpdateSpecific,
        ProjectCommit,
        ProjectHistory,
        ProjectMerge,

        PendingChangesViewFlat,
        PendingChangesViewProject,
        PendingChangesViewChangeList,
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
        ItemOpenInRepositoryExplorer, // Unused

        ItemRevertBase,
        PendingChangesSpacer, // Whitespace command to move all buttons a bit

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
        LogCompareWithPrevious,
        LogCompareRevisions,
        LogRevertThisRevisions,
        LogOpen,
        LogOpenInVs,
        LogOpenWith,
        LogUpdateTo,
        LogRevertTo,
        LogMergeTo,

        ItemIgnoreFolder,

        GitNodeDelete,
        SolutionUpdateLatest,

        PendingChangesApplyWorkingCopy,
        PendingChangesCreatePatch,
        PendingChangesApplyPatch,
        SolutionApplyPatch,

        ExplorerOpen,
        ExplorerUp,
        ReposExplorerOpenWith,
        ReposExplorerShowPrevChanges,
        ViewInVsText,
        ViewInWindowsWith,

        ReposCopyTo,
        ReposMoveTo,

        RepositoryShowChanges,
        RepositoryCompareWithWc,
        UpdateItemLatestRecursive,

        ItemRename,
        ItemDelete,

        ItemAddToPending,
        ItemRemoveFromPending,

        LogStrictNodeHistory,
        LogIncludeMergedRevisions,
        LogFetchAll,
        LogShowChangedPaths,
        LogShowLogMessage,

        LogChangeLogMessage,
        LogShowChanges,

        MigrateSettings,
        ReposExplorerLog,
        LogAnnotateRevision,


        UnifiedDiff,
        WorkingCopyAdd,

        SwitchProject,
        ProjectBranch,

        ListViewSortAscending,
        ListViewSortDescending,
        ListViewSort0,
        ListViewSortMax = ListViewSort0 + 64,
        ListViewGroup0,
        ListViewGroupMax = ListViewGroup0 + 64,
        ListViewShow0,
        ListViewShowMax = ListViewShow0 + 64,

        MoveToNewChangeList,
        MoveToExistingChangeList0,
        MoveToExistingChangeListMax = MoveToExistingChangeList0 + 20,
        MoveToIgnoreChangeList,
        RemoveFromChangeList,

        SolutionIssueTrackerSetup,
        PcLogEditorOpenIssue,
        LogOpenIssue,
        DocumentAddToPending,
        DocumentRemoveFromPending,

        FolderUpdateSpecific,
        FolderUpdateLatest,
        MakeNonSccFileWriteable,

        PendingChangesConfigureRecentChangesPage,
        ShowGitInfo,

        PendingChangesPush,
        PendingChangesPushSpecificBranch,
        PendingChangesPushSpecificTag,
        PendingChangesPull,
        PendingChangesFetch,
    }
}
