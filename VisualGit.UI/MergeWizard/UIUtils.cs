using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using SharpSvn;
using VisualGit.UI.RepositoryExplorer;
using VisualGit.UI.RepositoryOpen;
using VisualGit.UI.WizardFramework;
using System.IO;

namespace VisualGit.UI.MergeWizard
{
    /// <summary>
    /// This class contains utility methods for the UI.
    /// </summary>
    static class UIUtils
    {
        /// <summary>
        /// Resizes a <code>System.Windows.Forms.ComboBox</code>'s DropDownWidth
        /// based on the longest string in the list.
        /// </summary>
        public static void ResizeDropDownForLongestEntry(ComboBox comboBox)
        {
            int width = comboBox.DropDownWidth;
            using (Graphics g = comboBox.CreateGraphics())
            {
                Font font = comboBox.Font;
                int vertScrollBarWidth = (comboBox.Items.Count > comboBox.MaxDropDownItems)
                    ? SystemInformation.VerticalScrollBarWidth : 0;
                int newWidth;

                foreach (object o in comboBox.Items)
                {
                    string s = "";

                    if (o is string)
                        s = (string)o;
                    else if (o is KeyValuePair<SvnDepth, string>)
                    {
                        s = ((KeyValuePair<SvnDepth, string>)o).Value;
                    }
                    else
                    {
                        return;
                    }

                    newWidth = (int)g.MeasureString(s, font).Width
                        + vertScrollBarWidth;
                    if (width < newWidth)
                    {
                        width = newWidth;
                    }
                }
                if (comboBox.DropDownWidth < width)
                    comboBox.DropDownWidth = width;
            }
        }
        public static Uri DisplayBrowseDialogAndGetResult(WizardPage page, GitItem target, string baseUri)
        {
            Uri u;
            if(Uri.TryCreate(baseUri, UriKind.Absolute, out u))
                return DisplayBrowseDialogAndGetResult(page, target, u);
            
            page.Message = MergeUtils.INVALID_FROM_URL;

            return null;
        }
        public static Uri DisplayBrowseDialogAndGetResult(WizardPage page, GitItem target, Uri baseUri)
        {
            IVisualGitServiceProvider context = ((MergeWizard)page.Wizard).Context;

            if (((MergeWizard)page.Wizard).MergeTarget.IsDirectory)
            {
                using (RepositoryFolderBrowserDialog dlg = new RepositoryFolderBrowserDialog())
                {
                    dlg.SelectedUri = baseUri;

                    if (dlg.ShowDialog(context) == DialogResult.OK)
                    {
                        return dlg.SelectedUri;
                    }
                }
            }
            else
            {
                using (RepositoryOpenDialog dlg = new RepositoryOpenDialog())
                {
                    string fileName = Path.GetFileName(target.FullPath);

                    dlg.Context = context;
                    dlg.Filter = fileName + "|" + fileName + "|All Files (*.*)|*";


                    dlg.SelectedUri = baseUri;

                    if (dlg.ShowDialog(context) == DialogResult.OK)
                        return dlg.SelectedUri;
                }
            }

            return null;
        }
    }
}
