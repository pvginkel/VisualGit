using System;
using System.Collections.Generic;
using System.Text;

namespace VisualGit.Scc
{
    /// <summary>
    /// This flag enum specifies the full UI state of a <see cref="GitItem"/> in all live updating windows
    /// </summary>
    /// <remarks>
    /// It is a superset of the <see cref="PendingChangeKind"/> and the <see cref="VisualGitGlyph"/>. 
    /// Those two values can be 100% calculated from just this state
    /// </remarks>
    [Flags]
    public enum GitItemState
    {
        #region State Flags

        /// <summary>
        /// None; not calculated
        /// </summary>
        None                    = 0,

        /// <summary>
        /// File is on disk
        /// </summary>
        Exists                  = 0x00000001,

        /// <summary>
        /// File is in the current open solution
        /// </summary>
        InSolution              = 0x00000002,

        /// <summary>
        /// File is versioned in a working copy
        /// </summary>
        Versioned               = 0x00000004,

        /// <summary>
        /// File is of the wrong kind
        /// </summary>
        Obstructed              = 0x00000010,

        /// <summary>
        /// Item is versioned or below a versioned directory
        /// </summary>
        Versionable             = 0x00000020,

        /// <summary>
        /// Gets a boolean indicating whether the item is a file
        /// </summary>
        /// <remarks>Contains the on disk status.. If obstructed this does not match</remarks>
        IsDiskFile              = 0x00000040,

        /// <summary>
        /// Gets a boolean indicating whether the item is a directory
        /// </summary>
        IsDiskFolder            = 0x00000080,

        /// <summary>
        /// The node is read only on disk
        /// </summary>
        ReadOnly                = 0x00000100,

        /// <summary>
        /// The file is marked as dirty by the editor that has the file open
        /// </summary>
        DocumentDirty           = 0x00000200,

        /// <summary>
        /// Somehow modified in Git
        /// </summary>
        GitDirty                = 0x00000800,

        #endregion

        #region SvnStates (When Versioned is set)

        /// <summary>
        /// The node is modified on disk
        /// </summary>
        Modified                = 0x00001000,
        /// <summary>
        /// The node is scheduled for addition
        /// </summary>
        Added                   = 0x00004000,
        /// <summary>
        /// The node has a copy origin
        /// </summary>
        HasCopyOrigin           = 0x00008000,
        /// <summary>
        /// The node is scheduled for deletion
        /// </summary>
        Deleted                 = 0x00010000,
        /// <summary>
        /// The node is replaced
        /// </summary>
        Replaced                = 0x00020000,

        /// <summary>
        /// The content is marked as conflicted
        /// </summary>
        ContentConflicted       = 0x00100000,

        /// <summary>
        /// The GitItem is part of a tree conflict
        /// </summary>
        TreeConflicted          = 0x00400000,

        /// <summary>
        /// The item is marked as ignored in Git
        /// </summary>
        Ignored                 = 0x00800000,
        #endregion

        /// <summary>
        /// The item is the root of a nested working copy
        /// </summary>
        IsNested                = 0x01000000,

        /// <summary>
        /// The item is a textfile
        /// </summary>
        IsTextFile              = 0x02000000,

        /// <summary>
        /// The item is (part of) the administrative area
        /// </summary>
        IsAdministrativeArea    = unchecked((int)0x80000000)
    }
}
