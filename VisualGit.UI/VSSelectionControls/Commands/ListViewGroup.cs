using System;
using System.Collections.Generic;
using System.Text;
using VisualGit.Commands;
using System.Windows.Forms;

namespace VisualGit.UI.VSSelectionControls.Commands
{
    [Command(VisualGitCommand.ListViewGroup0, LastCommand = VisualGitCommand.ListViewGroupMax, AlwaysAvailable = true)]
    [Command((VisualGitCommand)VisualGitCommandMenu.ListViewGroup, AlwaysAvailable=true)]
    class ListViewGroup : ListViewCommandBase
    {
        public override void OnUpdate(CommandUpdateEventArgs e)
        {
            if (!SmartListView.SupportsGrouping)
            {
                e.Visible = e.Enabled = false; // Group by is XP+
                e.DynamicMenuEnd = (e.Command != (VisualGitCommand)VisualGitCommandMenu.ListViewGroup);
                return;
            }

            if (e.Command == (VisualGitCommand)VisualGitCommandMenu.ListViewGroup)
                return;

            base.OnUpdate(e);
        }

        protected override void OnUpdate(SmartListView list, CommandUpdateEventArgs e)
        {
            int n = (int)(e.Command - VisualGitCommand.ListViewGroup0);

            if (n >= list.AllColumns.Count || n < 0)
            {
                e.Text = "";
                e.DynamicMenuEnd = true;
                return;
            }

            SmartColumn column = list.AllColumns[n];

            if (e.TextQueryType == TextQueryType.Name)
            {
                e.Text = column.MenuText;
            }

            if (column == null || !column.Groupable)
                e.Enabled = false;

            e.Checked = list.GroupColumns.Contains(column);
        }

        public override void OnExecute(CommandEventArgs e)
        {
            if (!SmartListView.SupportsGrouping)
                return;

            base.OnExecute(e);
        }

        protected override void OnExecute(SmartListView list, CommandEventArgs e)
        {
            bool extend = ((Control.ModifierKeys & Keys.Shift) != 0);

            int n = (int)(e.Command - VisualGitCommand.ListViewGroup0);
            SmartColumn column = list.AllColumns[n];

            if (list.GroupColumns.Contains(column))
            {
                list.GroupColumns.Remove(column);
            }
            else if (!extend)
            {
                list.GroupColumns.Clear();
                list.GroupColumns.Add(column);
            }
            else
            {
                list.GroupColumns.Add(column);
            }

            list.RefreshGroups();
        }
    }
}
