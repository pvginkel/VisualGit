using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using VisualGit.UI.VSSelectionControls;

namespace VisualGit.UI.OptionsPages
{
    public partial class GitAuthenticationCacheEditor : VSDialogForm
    {
        public GitAuthenticationCacheEditor()
        {
            InitializeComponent();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            ResizeList();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!DesignMode)
            {
                ResizeList();
                Refreshlist();
            }
        }

        private void Refreshlist()
        {
            credentialList.Items.Clear();

            ICollection<CredentialCacheItem> items = ConfigurationService.GetAllCredentialCacheItems();

            foreach (CredentialCacheItem item in items)
            {
                AuthenticationListItem lvi = new AuthenticationListItem(credentialList);
                lvi.CacheItem = item;
                lvi.Refresh();
                credentialList.Items.Add(lvi);
            }
        }

        void ResizeList()
        {
            if (!DesignMode && credentialList != null)
                credentialList.ResizeColumnsToFit(uriHeader, promptTextHeader, typeHeader);
        }

        class AuthenticationListItem : SmartListViewItem
        {
            CredentialCacheItem _item;

            public AuthenticationListItem(SmartListView listview)
                : base(listview)
            {
            }

            public CredentialCacheItem CacheItem
            {
                get { return _item; }
                set { _item = value; }
            }

            public void Refresh()
            {
                SetValues(
                    _item.Uri,
                    _item.PromptText,
                    _item.Type
                );
            }
        }

        private void credentialList_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (AuthenticationListItem li in credentialList.SelectedItems)
            {
                if (li != null)
                {
                    removeButton.Enabled = true;
                    return;
                }
            }

            removeButton.Enabled = false;
        }

        private void removeButton_Click(object sender, EventArgs e)
        {
            VisualGitMessageBox mb = new VisualGitMessageBox(Context);

            if (DialogResult.OK != mb.Show(OptionsResources.TheSelectedCredentialsWillBeRemoved, "", MessageBoxButtons.OKCancel))
                return;

            bool changed = false;
            try
            {
                foreach (AuthenticationListItem li in credentialList.SelectedItems)
                {
                    ConfigurationService.RemoveCredentialCacheItem(
                        li.CacheItem.Uri,
                        li.CacheItem.Type,
                        li.CacheItem.PromptText
                    );

                    changed = true;
                }
            }
            finally
            {
                if (changed)
                    Refreshlist();
            }
        }
    }
}
