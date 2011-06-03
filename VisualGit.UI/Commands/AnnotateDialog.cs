using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using VisualGit.Scc;
using SharpSvn;
using VisualGit.UI.RepositoryExplorer;
using SharpGit;

namespace VisualGit.UI.Commands
{
    public partial class AnnotateDialog : VSDialogForm
    {
        public AnnotateDialog()
        {
            InitializeComponent();
        }

        private void AnnotateDialog_Load(object sender, EventArgs e)
        {
            whitespaceBox.Items.Add(CommandStrings.WhitespaceCompare);
            whitespaceBox.Items.Add(CommandStrings.WhitespaceIgnoreChanges);
            whitespaceBox.Items.Add(CommandStrings.WhitespaceIgnoreAllWhitespace);
            whitespaceBox.SelectedIndex = 1;
        }

        public void SetTargets(IEnumerable<GitItem> targets)
        {
            List<GitOrigin> origins = new List<GitOrigin>();

            foreach (GitItem i in targets)
                origins.Add(new GitOrigin(i));

            SetTargets(origins);
        }

        public void SetTargets(List<GitOrigin> origins)
        {
            foreach (GitOrigin i in origins)
                targetBox.Items.Add(i);

            if (targetBox.Items.Count > 0)
                targetBox.SelectedIndex = 0;
        }

        public GitOrigin SelectedTarget
        {
            get { return targetBox.SelectedItem as GitOrigin; }
        }

        public GitRevision StartRevision
        {
            get { return startRevision.Revision; }
            set { startRevision.Revision = value; }
        }

        public GitRevision EndRevision
        {
            get { return toRevision.Revision; }
            set { toRevision.Revision = value; }
        }

        private void targetBox_SelectedValueChanged(object sender, EventArgs e)
        {
            startRevision.GitOrigin = SelectedTarget;
            toRevision.GitOrigin = SelectedTarget;
        }

        public bool IgnoreEols
        {
            get { return ignoreEols.Checked; }
            set { ignoreEols.Checked = value; }
        }

        public SvnIgnoreSpacing IgnoreSpacing
        {
            get
            {
                switch (whitespaceBox.SelectedIndex)
                {
                    default:
                    case 0:
                        return SvnIgnoreSpacing.None;
                    case 1:
                        return SvnIgnoreSpacing.IgnoreSpace;
                    case 2:
                        return SvnIgnoreSpacing.IgnoreAll;
                }
            }
        }

        public bool RetrieveMergeInfo
        {
            get { return includeMergeInfo.Checked; }
            set { includeMergeInfo.Checked = value; }
        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            using (RepositoryFolderBrowserDialog dlg = new RepositoryFolderBrowserDialog())
            {
                GitOrigin from = SelectedTarget;

                if (from == null)
                    return;

                dlg.ShowFiles = true;

                GitUriTarget ut = from.Target as GitUriTarget;
                if (ut != null)
                    dlg.SelectedUri = ut.Uri;
                else
                {
                    GitItem file = GetService<IFileStatusCache>()[((GitPathTarget)from.Target).FullPath];

                    if (file.Uri == null)
                        dlg.SelectedUri = from.RepositoryRoot;
                    else
                        dlg.SelectedUri = file.Uri;
                }

                if (dlg.ShowDialog(Context) == DialogResult.OK)
                {
                    Uri selectedUri = dlg.SelectedUri;

                    GitOrigin o = new GitOrigin(Context, selectedUri, null);

                    targetBox.Items.Add(o);
                    targetBox.SelectedItem = o;
                }
            }
        }
    }
}
