using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

using SharpSvn;

using VisualGit.Configuration;
using VisualGit.UI;
using VisualGit.UI.SccManagement;

namespace VisualGit.Commands
{
    /// <summary>
    /// Command to lock the selected item.
    /// </summary>
    [Command(VisualGitCommand.Lock)]
    [Command(VisualGitCommand.LockMustLock)]
    [Command(VisualGitCommand.SccLock)]
    class LockCommand : CommandBase
    {
        public override void OnUpdate(CommandUpdateEventArgs e)
        {
            if (e.Command == VisualGitCommand.SccLock)
                return; // Always enabled

            bool mustOnly = (e.Command == VisualGitCommand.LockMustLock);
            foreach (GitItem item in e.Selection.GetSelectedGitItems(e.Command != VisualGitCommand.LockMustLock))
            {
                if (item.IsFile && item.IsVersioned && !item.IsLocked)
                {
                    if (!mustOnly || item.IsReadOnlyMustLock)
                        return;
                }
            }
            e.Enabled = false;
        }

        public override void OnExecute(CommandEventArgs e)
        {
            IEnumerable<GitItem> items = e.Argument as IEnumerable<GitItem>;

            if (e.Command == VisualGitCommand.SccLock && items == null)
                return;

            PathSelectorInfo psi = new PathSelectorInfo("Select Files to Lock",
                                                        items ?? e.Selection.GetSelectedGitItems(true));
            psi.VisibleFilter += delegate(GitItem item)
                                     {
                                         return item.IsFile && item.IsVersioned && !item.IsLocked;
                                     };

            psi.CheckedFilter += delegate(GitItem item)
                                     {
                                         return item.IsFile && item.IsVersioned && !item.IsLocked;
                                     };

            PathSelectorResult psr;
            bool stealLocks = false;
            string comment = "";

            IVisualGitConfigurationService cs = e.GetService<IVisualGitConfigurationService>();
            VisualGitConfig config = cs.Instance;

            IEnumerable<GitItem> selectedItems = null;

            if (!config.SuppressLockingUI || e.PromptUser)
            {
                if (e.PromptUser || !(Shift || e.DontPrompt))
                {
                    using (LockDialog dlg = new LockDialog(psi))
                    {
                        bool succeeded = (dlg.ShowDialog(e.Context) == DialogResult.OK);
                        psr = new PathSelectorResult(succeeded, dlg.CheckedItems);
                        stealLocks = dlg.StealLocks;
                        comment = dlg.Message;
                    }

                }
                else
                {
                    psr = psi.DefaultResult;
                }
                if (!psr.Succeeded)
                {
                    return;
                }
                selectedItems = psr.Selection;
            }


            if (selectedItems == null)
                selectedItems = psi.DefaultResult.Selection;

            List<string> files = new List<string>();
            foreach (GitItem item in selectedItems)
            {
                if (item.IsFile) // svn lock is only for files
                {
                    files.Add(item.FullPath);
                }
            }

            if (files.Count == 0)
                return;


            SortedList<string, string> alreadyLockedFiles = new SortedList<string, string>(StringComparer.OrdinalIgnoreCase);
            e.GetService<IProgressRunner>().RunModal(
                "Locking",
                 delegate(object sender, ProgressWorkerArgs ee)
                 {
                     SvnLockArgs la = new SvnLockArgs();
                     la.StealLock = stealLocks;
                     la.Comment = comment;
                     la.AddExpectedError(SvnErrorCode.SVN_ERR_FS_PATH_ALREADY_LOCKED);
                     la.Notify += delegate(object nSender, SvnNotifyEventArgs notifyArgs)
                                      {
                                          if (notifyArgs.Action == SvnNotifyAction.LockFailedLock)
                                          {
                                              alreadyLockedFiles.Add(notifyArgs.FullPath, GuessUserFromError(notifyArgs.Error.Message));
                                          }
                                      };
                     ee.Client.Lock(files, la);
                 });

            if (alreadyLockedFiles.Count == 0)
                return;

            StringBuilder msg = new StringBuilder();
            msg.AppendLine(CommandStrings.ItemsAlreadyLocked);
            msg.AppendLine();

            foreach (KeyValuePair<string, string> kv in alreadyLockedFiles)
            {
                if (!string.IsNullOrEmpty(kv.Value))
                    msg.AppendFormat(CommandStrings.ItemFileLocked, kv.Key, kv.Value);
                else
                    msg.Append(kv.Key);
                msg.AppendLine();
            }

            // TODO: Create a dialog where the user can select what locks to steal, and also what files are already locked.
            VisualGitMessageBox box = new VisualGitMessageBox(e.Context);
            DialogResult rslt = box.Show(
                msg.ToString().TrimEnd(),
                "",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (rslt == DialogResult.Yes)
            {
                e.GetService<IProgressRunner>().RunModal(
                    CommandStrings.LockingTitle,
                     delegate(object sender, ProgressWorkerArgs ee)
                     {
                         SvnLockArgs la = new SvnLockArgs();
                         la.StealLock = true;
                         la.Comment = comment;
                         ee.Client.Lock(files, la);
                     });
            }
        }

        Regex _guessRx;
        private string GuessUserFromError(string message)
        {
            // Parses errors in the formats:
            // "Path '%s' is already locked by user '%s' in filesystem '%s'" (Used by most languages)
            // "Pfad �%s� ist bereits vom Benutzer �%s� im Dateisystem �%s� gesperrt" (German)
            //
            // Ordering is used in both FS backends and unlikely to change over versions
            // but additional fields might be added later
            if (_guessRx == null)
                _guessRx = new Regex("^[^']+ ['�](?<path>.*?)['�][^']+ ['�](?<user>.*?)['�][^']+( ['�].*?['�][^']*)*$", RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.Singleline);

            Match m = _guessRx.Match(message);

            string user = null;
            if (m.Success)
                user = m.Groups["user"].Value;

            return user ?? "";
        }
    }
}
