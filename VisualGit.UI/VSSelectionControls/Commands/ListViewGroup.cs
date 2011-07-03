// VisualGit.UI\VSSelectionControls\Commands\ListViewGroup.cs
//
// Copyright 2008-2011 The AnkhSVN Project
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//
// Changes and additions made for VisualGit Copyright 2011 Pieter van Ginkel.

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
