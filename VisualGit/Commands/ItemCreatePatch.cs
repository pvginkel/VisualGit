using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using VisualGit.Scc;
using VisualGit.VS;
using VisualGit.UI.PathSelector;

namespace VisualGit.Commands
{
    [Command(VisualGitCommand.CreatePatch)]
    class ItemCreatePatch : CommandBase
    {
        public override void OnUpdate(CommandUpdateEventArgs e)
        {
            foreach (GitItem i in e.Selection.GetSelectedGitItems(true))
            {
                if (i.IsVersioned)
                {
                    if (i.IsModified || i.IsDocumentDirty)
                        return; // There might be a new version of this file
                }
                else if (i.IsIgnored)
                    continue;
                else if (i.InSolution && i.IsVersionable)
                    return; // The file is 'to be added'
            }
            e.Enabled = false;
        }

        public override void OnExecute(CommandEventArgs e)
        {
            IPendingChangesManager pcm = e.GetService<IPendingChangesManager>();
            Dictionary<string, PendingChange> changes = new Dictionary<string, PendingChange>(StringComparer.OrdinalIgnoreCase);

            foreach (PendingChange pc in pcm.GetAll())
            {
                if (!changes.ContainsKey(pc.FullPath))
                    changes.Add(pc.FullPath, pc);
            }

            Dictionary<string, GitItem> selectedChanges = new Dictionary<string, GitItem>(StringComparer.OrdinalIgnoreCase);
            foreach (GitItem item in e.Selection.GetSelectedGitItems(true))
            {
                if (changes.ContainsKey(item.FullPath) &&
                    !selectedChanges.ContainsKey(item.FullPath))
                {
                    selectedChanges.Add(item.FullPath, item);
                }
            }

            Collection<GitItem> resources = new Collection<GitItem>();
            List<GitItem> selectedItems = new List<GitItem>(selectedChanges.Values);

            // TODO: Give the whole list to a refreshable dialog!
            foreach (GitItem item in selectedItems)
            {
                PendingChange pc = changes[item.FullPath];

                if (pc.IsChangeForPatching())
                    continue;

                resources.Add(item);
            }
            if (resources.Count == 0)
                return;

            using (PendingChangeSelector pcs = new PendingChangeSelector())
            {
                pcs.Context = e.Context;
                pcs.Text = CommandStrings.CreatePatchTitle;

                pcs.PreserveWindowPlacement = true;

                pcs.LoadItems(e.Selection.GetSelectedGitItems(true));

                DialogResult dr = pcs.ShowDialog(e.Context);

                if (dr != DialogResult.OK)
                    return;

                string fileName = GetFileName(e.Context.DialogOwner);
                if (string.IsNullOrEmpty(fileName))
                {
                    return;
                }

                PendingChangeCreatePatchArgs pca = new PendingChangeCreatePatchArgs();
                pca.FileName = fileName;
                IVisualGitSolutionSettings ss = e.GetService<IVisualGitSolutionSettings>();
                pca.RelativeToPath = ss.ProjectRoot;
                pca.AddUnversionedFiles = true;

                List<PendingChange> patchChanges = new List<PendingChange>(pcs.GetSelection());
                e.GetService<IPendingChangeHandler>().CreatePatch(patchChanges, pca);
            }
        }

        private static string GetFileName(IWin32Window dialogOwner)
        {
            string fileName = null;
            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.Filter = "Patch files( *.patch)|*.patch|Diff files (*.diff)|*.diff|" +
                    "Text files (*.txt)|*.txt|All files (*.*)|*";
                dlg.AddExtension = true;

                if (dlg.ShowDialog(dialogOwner) == DialogResult.OK)
                {
                    fileName = dlg.FileName;
                }
            }
            return fileName;
        }
    }
}
