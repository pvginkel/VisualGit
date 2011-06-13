using System;
using System.Runtime.InteropServices;

namespace VisualGit
{
    // Disable Missing XML comment warning for this file
#pragma warning disable 1591 

    /// <summary>
    /// List of VisualGit command groups
    /// </summary>
    /// <remarks>
    /// <para>New items should be added at the end. Items should only be obsoleted between releases.</para>
    /// <para>The values of this enum are part of our interaction with other packages within Visual Studio</para>
    /// </remarks>
    [Guid(VisualGitId.CommandSet)]
    public enum VisualGitCommandGroup
    {
        None = 0,

        // These values live in the same numberspace as the other values within 
        // the command set. So we start countin at this number to make sure we
        // do not reuse values
        GroupFirst = 0x3FFFFFF,

        FileSourceControl,
        FileMenuScc,
        FileSccMenuTasks,
        FileSccItem,
        FileSccOpen,
        FileSccAdd,
        FileSccManagement,

        NoUI,

        SccSlnAdvanced,
        SccProjectAdvanced,
        SccItemAdvanced,

        RepositoryExplorerTbBrowse,
        RepositoryExplorerTbEdit,
        RepositoryExplorerContext,
        WorkingCopyExplorerToolBar,
        WorkingCopyExplorerContext,

        PendingChangesToolBar,
        PendingChangesLogMessageCommands,
        PendingChangesLogEditor,
        PendingChangesLogEditor2,
        PendingChangesContextMenu,

        PendingChangesSolutionCommands,
        PendingChangesView,
        PendingChangesItems,
        PendingChangesAdmin,
        PendingChangesSwitch,

        PendingChangesOpenEx,
        PendingCommitsMenu,
        PendingCommitsSortActions,
        PendingCommitsGroupActions,
        PendingCommitsActions,

        LogMessageCommands,

        ItemResolveCommand,
        ItemResolveFile,
        ItemResolveAutoFull,
        ItemResolveAutoConflict,

        SccTbManage,
        SccTbItem,
        SccTbExtra,

        LogViewerToolbar,
        LogViewerContextMenu,

        SccExplore,

        LogChangedPathsContextMenu,

        ListViewState,

        ListViewSort,
        ListViewSortOrder,
        ListViewGroup,
        ListViewGroupOrder,
        ListViewShow,

        ItemIgnore,
        AnnotateContextMenu,

        CodeEditorScc,

        PendingChangesCommit1,
        PendingChangesCommit2,
        PendingChangesCommit3,

        PendingChangesUpdate1,
        PendingChangesUpdate2,

        PendingChangesPush1,
        PendingChangesPull1,

        RepositoryExplorerOpen,
        RepositoryExplorerReview,
        RepositoryExplorerEdit,
        RepositoryExplorerRefer,

        RepositoryExplorerTbOpen,
        RepositoryExplorerTbAction,

        OpenWith,
        OpenWith2,

        /// <summary>
        /// Special parent group for VS Menu Designer support
        /// </summary>
        VisualGitContextMenus,
        SccLock,
        SccAddIgnore,

        SccCleanupRefresh,

        SccItemUpdate,
        SccItemAction,
        SccItemChanges,
        //SccItemAdvanced,
        SccItemBranch,
        SccItemPatch,

        SccProjectUpdate,
        SccProjectAction,
        SccProjectChanges,        
        //SccProjectAdvanced,
        SccProjectBranch,
        SccProjectPatch,

        SccSlnAction,
        SccSlnChanges,        
        //SccSlnAdvanced,
        SccSlnBranch,
        SccSlnPatch,

        SccPrjFileUpdate,
        SccPrjFileAction,
        SccPrjFileChanges,
        SccPrjFileAdvanced,
        SccPrjFileBranch,

        SccSlnFileUpdate,
        SccSlnFileAction,
        SccSlnFileChanges,
        SccSlnFileAdvanced,
        SccSlnFileBranch,

        WceUpdate,
        WceAction,
        WceChanges,
        WceAdvanced,
        WceOpen,
        WceBranch,
        WceEdit,

        PendingChangesLogEditor3,
        MdiDocumentScc,
        DocumentPending,

        GitInfoTBOperations,
        GitInfoTBCommands,
        GitInfoTB3,
        GitInfoTB4,
        GitInfoTB5,
    }
}
