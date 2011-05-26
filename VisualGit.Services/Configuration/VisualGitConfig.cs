using System;
using System.ComponentModel;
using System.Windows.Forms.Design;
using System.Drawing.Design;
using Microsoft.Win32;
using System.Security.AccessControl;
namespace VisualGit.Configuration
{
    /// <summary>
    /// VisualGit configuration container. Read and written via <see cref="IVisualGitConfigurationService"/>
    /// </summary>
    public class VisualGitConfig
    {
        string _mergeExePath;
        string _diffExePath;
        string _patchExePath;
        bool _interactiveMergeOnConflict;
        bool _flashWindow;
        bool _autoAddEnabled;
        bool _autoLockEnabled;
        bool _noDashComment;
        bool _pcDoubleClickShowsChanges;
        int _recentChangesRefreshInterval;
        bool _disableUpdateCheck;

        /// <summary>
        /// Gets or sets the merge exe path.
        /// </summary>
        /// <value>The merge exe path.</value>
        [DefaultValue(null)]
        public string MergeExePath
        {
            get { return _mergeExePath; }
            set { _mergeExePath = value; }
        }

        /// <summary>
        /// Gets or sets the diff exe path.
        /// </summary>
        /// <value>The diff exe path.</value>
        [DefaultValue(null)]
        public string DiffExePath
        {
            get { return _diffExePath; }
            set { _diffExePath = value; }
        }

        /// <summary>
        /// Gets or sets the patch exe path.
        /// </summary>
        /// <value>The patch exe path.</value>
        [DefaultValue(null)]
        public string PatchExePath
        {
            get { return _patchExePath; }
            set { _patchExePath = value; }
        }

        [DefaultValue(false)]
        public bool InteractiveMergeOnConflict
        {
            get { return _interactiveMergeOnConflict; }
            set { _interactiveMergeOnConflict = value; }
        }

        [DefaultValue(false)]
        public bool FlashWindowWhenOperationCompletes
        {
            get { return _flashWindow; }
            set { _flashWindow = value; }
        }

        [DefaultValue(false)]
        public bool AutoAddEnabled
        {
            get { return _autoAddEnabled; }
            set { _autoAddEnabled = value; }
        }

        [DefaultValue(false)]
        public bool SuppressLockingUI
        {
            get { return _autoLockEnabled; }
            set { _autoLockEnabled = value; }
        }

        [DefaultValue(false)]
        public bool DisableDashInLogComment
        {
            get { return _noDashComment; }
            set { _noDashComment = value; }
        }

        [DefaultValue(false)]
        public bool PCDoubleClickShowsChanges
        {
            get { return _pcDoubleClickShowsChanges; }
            set { _pcDoubleClickShowsChanges = value; }
        }

        /// <summary>
        /// Gets or sets the Recent Changes auto-refresh interval in seconds
        /// </summary>
        [DefaultValue(0)]
        public int RecentChangesRefreshInterval
        {
            get { return _recentChangesRefreshInterval; }
            set { _recentChangesRefreshInterval = value; }
        }

        [DefaultValue(false)]
        public bool DisableUpdateCheck
        {
            get { return _disableUpdateCheck; }
            set { _disableUpdateCheck = value; }
        }
    }
}
