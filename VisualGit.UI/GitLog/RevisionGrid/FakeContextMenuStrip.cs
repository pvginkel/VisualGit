using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace VisualGit.UI.GitLog.RevisionGrid
{
    internal class FakeContextMenuStrip : ContextMenuStrip
    {
        private readonly EventHandler<EventArgs> _action;

        public FakeContextMenuStrip(EventHandler<EventArgs> action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            _action = action;
        }

        protected override void SetVisibleCore(bool visible)
        {
            if (visible)
                _action(this, EventArgs.Empty);
        }
    }
}
