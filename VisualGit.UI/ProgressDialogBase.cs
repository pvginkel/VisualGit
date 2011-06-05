using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpGit;

namespace VisualGit.UI
{
    public class ProgressDialogBase : VSDialogForm
    {
        string _caption;
        string _title;

        public event EventHandler Cancel;

        public string Caption
        {
            get
            {
                return _caption;
            }
            set
            {
                if (_title == null)
                    _title = Text;

                _caption = value;
                Text = string.Format(_title, _caption).TrimStart().TrimStart('-', ' ');
            }
        }

        public virtual IDisposable Bind(GitClient client)
        {
            return null;
        }

        protected virtual void OnCancel(EventArgs e)
        {
            var ev = Cancel;

            if (ev != null)
                ev(this, e);
        }
    }
}
