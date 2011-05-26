using System;
using System.Collections.Generic;
using System.Text;
using VisualGit.Commands;

namespace VisualGit.UI.VSSelectionControls.Commands
{
    [Command(VisualGitCommand.ListViewSortAscending, AlwaysAvailable = true, HideWhenDisabled = false)]
    [Command(VisualGitCommand.ListViewSortDescending, AlwaysAvailable = true, HideWhenDisabled = false)]
    class ListViewSortOrder : ListViewCommandBase
    {
        protected override void OnUpdate(SmartListView list, VisualGit.Commands.CommandUpdateEventArgs e)
        {
            bool foundOne = false;

            e.Checked = true;

            foreach (SmartColumn sc in list.SortColumns)
            {
                foundOne = true;

                switch (e.Command)
                {
                    case VisualGitCommand.ListViewSortAscending:
                        if (sc.ReverseSort)
                        {
                            e.Checked = false;
                            return;
                        }
                        break;
                    case VisualGitCommand.ListViewSortDescending:
                        if (!sc.ReverseSort)
                        {
                            e.Checked = false;
                            return;
                        }
                        break;
                }
            }
            if (!foundOne)
            {
                e.Checked = e.Enabled = false;
            }
        }

        protected override void OnExecute(SmartListView list, CommandEventArgs e)
        {
            bool value = (e.Command == VisualGitCommand.ListViewSortDescending);

            foreach (SmartColumn sc in list.SortColumns)
            {
                sc.ReverseSort = value;
            }
            list.UpdateSortGlyphs();
            list.Sort();
        }
    }
}
