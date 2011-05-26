using System;

namespace VisualGit.UI.OptionsPages
{
    public partial class EnvironmentSettingsControl : VisualGitOptionsPageControl
    {
        public EnvironmentSettingsControl()
        {
            InitializeComponent();
        }

        private void authenticationEdit_Click(object sender, EventArgs e)
        {
            using (GitAuthenticationCacheEditor editor = new GitAuthenticationCacheEditor())
            {
                editor.ShowDialog(Context);
            }
        }

        protected override void SaveSettingsCore()
        {
            Config.InteractiveMergeOnConflict = interactiveMergeOnConflict.Checked;
            Config.AutoAddEnabled = autoAddFiles.Checked;
            Config.SuppressLockingUI = autoLockFiles.Checked;
            Config.FlashWindowWhenOperationCompletes = flashWindowAfterOperation.Checked;
            Config.PCDoubleClickShowsChanges = pcDefaultDoubleClick.SelectedIndex == 1;
        }

        protected override void LoadSettingsCore()
        {
            interactiveMergeOnConflict.Checked = Config.InteractiveMergeOnConflict;
            autoAddFiles.Checked = Config.AutoAddEnabled;
            autoLockFiles.Checked = Config.SuppressLockingUI;
            flashWindowAfterOperation.Checked = Config.FlashWindowWhenOperationCompletes;
            pcDefaultDoubleClick.SelectedIndex = Config.PCDoubleClickShowsChanges ? 1 : 0;
        }

        private void proxyEdit_Click(object sender, EventArgs e)
        {
            using (GitProxyEditor editor = new GitProxyEditor())
            {
                editor.ShowDialog(Context);
            }
        }
    }
}
