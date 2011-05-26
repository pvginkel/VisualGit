using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SharpSvn;
using VisualGit.Scc;

namespace VisualGit.UI.RepositoryExplorer.Dialogs
{
    public partial class ConfirmDeleteDialog : VSContainerForm
    {
        public ConfirmDeleteDialog()
        {
            InitializeComponent();
        }

        sealed class UriComparer : IComparer<Uri>
        {
            #region IComparer<Uri> Members

            public int Compare(Uri x, Uri y)
            {
                return StringComparer.OrdinalIgnoreCase.Compare(x.ToString(), y.ToString());
            }

            #endregion

            public static readonly UriComparer Default = new UriComparer();
        }

        Uri[] _uris;
        public void SetUris(IEnumerable<SvnOrigin> uris)
        {
            deleteList.ClearSelected();

            SortedDictionary<Uri,SvnOrigin> d = new SortedDictionary<Uri, SvnOrigin>(UriComparer.Default);
            foreach (SvnOrigin o in uris)
            {
                SvnUriTarget ut = o.Target as SvnUriTarget;
                if (ut != null)
                    d[ut.Uri] = o;
                else
                    d[o.Uri] = o;
            }

            _uris = new Uri[d.Count];
            List<Uri> newUris = new List<Uri>();
            foreach (SvnOrigin o in d.Values)
            {
                deleteList.Items.Add(o.Uri);
                newUris.Add(SvnTools.GetNormalizedUri(o.Uri));
            }
            _uris = newUris.ToArray();
        }

        public string LogMessage
        {
            get { return logMessage.Text; }
        }

        public Uri[] Uris
        {
            get { return _uris; }
        }
    }
}
