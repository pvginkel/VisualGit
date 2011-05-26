using System.Collections.Generic;
using VisualGit.Scc;
using SharpSvn;
using VisualGit.UI.Commands;
using System.Windows.Forms;

namespace VisualGit.Commands
{
    [Command(VisualGitCommand.MoveToNewChangeList)]
    [Command(VisualGitCommand.MoveToIgnoreChangeList)]
    [Command(VisualGitCommand.RemoveFromChangeList)]
    [Command(VisualGitCommand.MoveToExistingChangeList0, LastCommand=VisualGitCommand.MoveToExistingChangeListMax)]
    class ItemMoveToChangeList : CommandBase
    {
        const string IgnoreOnCommit = "ignore-on-commit";
        public override void OnUpdate(CommandUpdateEventArgs e)
        {
            if (e.Command == VisualGitCommand.RemoveFromChangeList)
            {
                OnUpdateRemove(e);
                return;
            }
            List<string> names = (List<string>)e.Selection.Cache[typeof(ItemMoveToChangeList)];

            bool found = false;

            if(names != null)
                found = true; // We have cached names -> We have a selection
            else
                foreach (SvnItem item in e.Selection.GetSelectedSvnItems(false))
                {
                    if (item.IsFile && item.IsVersioned && (item.IsModified || item.IsDocumentDirty))
                    {
                        found = true;
                        break;
                    }
                }

            bool inRange = (e.Command >= VisualGitCommand.MoveToExistingChangeList0 && e.Command < VisualGitCommand.MoveToExistingChangeListMax);

            if (!found)
            {
                e.Enabled = false;
                if(inRange)
                    e.DynamicMenuEnd = true;

                return;
            }

            if (!inRange)
                return;

            if (names == null)
                names = GetRecentNames(e);

            int n = e.Command - VisualGitCommand.MoveToExistingChangeList0;

            if(n >= names.Count)
            {
                e.Enabled = e.Visible = false;
                e.DynamicMenuEnd = true;
            }
            else
            {
                e.Text = names[n].Replace("&", "&&");
            }
        }

        private static List<string> GetRecentNames(BaseCommandEventArgs e)
        {
            List<string> names = (List<string>)e.Selection.Cache[typeof(ItemMoveToChangeList)];

            if (names != null)
                return names;

            names = new List<string>();

            foreach (string cl in e.GetService<IPendingChangesManager>().GetSuggestedChangeLists())
            {
                names.Add(cl);

                if (names.Count >= 10)
                    break;
            }

            e.Selection.Cache[typeof(ItemMoveToChangeList)] = names;

            return names;
        }

        private static void OnUpdateRemove(CommandUpdateEventArgs e)
        {
            foreach (SvnItem item in e.Selection.GetSelectedSvnItems(false))
            {
                if (item.IsVersioned && !string.IsNullOrEmpty(item.Status.ChangeList))
                {
                    return;
                }
            }
            e.Enabled = false;
        }

        public override void OnExecute(CommandEventArgs e)
        {
            string name;

            if (e.Command == VisualGitCommand.MoveToIgnoreChangeList)
                name = IgnoreOnCommit;
            else if (e.Command == VisualGitCommand.RemoveFromChangeList)
                name = null;
            else if (e.Command >= VisualGitCommand.MoveToExistingChangeList0 && e.Command < VisualGitCommand.MoveToExistingChangeListMax)
            {
                List<string> names = GetRecentNames(e);

                int n = e.Command - VisualGitCommand.MoveToExistingChangeList0;

                if (n >= names.Count)
                    return;

                name = names[n];
            }
            else using(CreateChangeListDialog dlg = new CreateChangeListDialog())
            {
                if(DialogResult.OK != dlg.ShowDialog(e.Context))
                    return;

                name = dlg.ChangeListName;
            }

            if(string.IsNullOrEmpty(name))
                name = null;

            List<string> paths = new List<string>();
            
            foreach (SvnItem item in e.Selection.GetSelectedSvnItems(false))
            {
                if (item.IsVersioned && (name != null) ? (item.IsFile || item.IsModified || item.IsDocumentDirty) : !string.IsNullOrEmpty(item.Status.ChangeList))
                {
                    paths.Add(item.FullPath);
                }
            }

            if (paths.Count == 0)
                return;

            e.Selection.Cache.Remove(typeof(ItemMoveToChangeList)); // Remove cached list of items

            using (SvnClient cl = e.GetService<ISvnClientPool>().GetNoUIClient())
            {
                if (name == null)
                {
                    SvnRemoveFromChangeListArgs ra = new SvnRemoveFromChangeListArgs();
                    ra.ThrowOnError = false;
                    cl.RemoveFromChangeList(paths, ra);
                }
                else
                {
                    SvnAddToChangeListArgs ca = new SvnAddToChangeListArgs();
                    ca.ThrowOnError = false;
                    cl.AddToChangeList(paths, name, ca);
                }

                // The svn client broadcasts glyph updates to fix our UI
            }
        }
    }
}
