using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using VisualGit.Scc;
using System.Globalization;
using VisualGit.UI.GitLog;

namespace VisualGit.UI.PathSelector
{
    partial class RevisionSelector : UserControl
    {
        public RevisionSelector()
        {
            InitializeComponent();
        }

        IVisualGitServiceProvider _context;
        GitOrigin _origin;

        /// <summary>
        /// Gets or sets the context.
        /// </summary>
        /// <value>The context.</value>
        public IVisualGitServiceProvider Context
        {
            get { return _context; }
            set { _context = value; EnableBrowse(); }
        }

        /// <summary>
        /// Gets or sets the SVN origin.
        /// </summary>
        /// <value>The SVN origin.</value>
        public GitOrigin GitOrigin
        {
            get { return _origin; }
            set { _origin = value; EnableBrowse(); }
        }

        void EnableBrowse()
        {
            browseButton.Enabled = (GitOrigin != null) && (Context != null);
        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            using (LogViewerDialog lvd = new LogViewerDialog(GitOrigin))
            {
                if (DialogResult.OK != lvd.ShowDialog(Context))
                    return;

                IGitLogItem li = EnumTools.GetSingle(lvd.SelectedItems);

                if (li == null)
                    return;

                throw new NotImplementedException("IGitLogItem revisions must become a string");

                Revision = li.Revision.ToString();
            }
        }

        public event EventHandler Changed;

        public string Revision
        {
            get
            {
                string text = revisionBox.Text;

                if (String.IsNullOrEmpty(text))
                    return null;
                else
                    return text.Trim();
            }
            set
            {
                revisionBox.Text = value;
            }
        }

        private void revisionBox_TextChanged(object sender, EventArgs e)
        {
            OnChanged(e);
        }

        protected virtual void OnChanged(EventArgs e)
        {
            if (Changed != null)
                Changed(this, e);
        }
    }
}
