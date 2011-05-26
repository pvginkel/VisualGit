using System;
using System.Collections.Generic;
using System.Text;
using VisualGit.Commands;
using System.Windows.Forms;

namespace VisualGit.UI.VSSelectionControls.Commands
{
    abstract class ListViewCommandBase : ICommandHandler
    {
        public virtual void OnUpdate(CommandUpdateEventArgs e)
        {
            SmartListView list = GetListView(e);

            if (list == null)
            {
                e.Enabled = false;
                return;
            }

            OnUpdate(list, e);
        }

        public virtual void OnExecute(CommandEventArgs e)
        {
            SmartListView list = GetListView(e);

            if (list == null)
                return;

            OnExecute(list, e);
        }

        private static SmartListView GetListView(BaseCommandEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException("e");

            return e.Selection.GetActiveControl<SmartListView>();
        }

        protected abstract void OnUpdate(SmartListView list, CommandUpdateEventArgs e);
        protected abstract void OnExecute(SmartListView list, CommandEventArgs e);
    }
}
