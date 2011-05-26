using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace VisualGit.UI
{
    public partial class RecentMessageDialog : VSContainerForm
    {
        ColumnHeader column;
        public RecentMessageDialog()
        {
            InitializeComponent();

            logMessageList.Columns.Clear();
            column = new ColumnHeader();
            logMessageList.Columns.Add(column);
            logMessageList.SizeChanged += new EventHandler(logMessageList_SizeChanged);
            SizeColumn();
        }

        void logMessageList_SizeChanged(object sender, EventArgs e)
        {
            SizeColumn();
        }

        private void SizeColumn()
        {
            column.Width = logMessageList.Width - SystemInformation.VerticalScrollBarWidth - 4;
        }

        public string SelectedText
        {
            get { return _selectedText; }
        }

        RegistryLifoList _items;
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (Context != null)
            {
                IVisualGitConfigurationService config = Context.GetService<IVisualGitConfigurationService>();

                _items = config.GetRecentLogMessages();

                logMessageList.Items.Clear();

                foreach (string i in _items)
                {
                    if (string.IsNullOrEmpty(i))
                        continue;

                    ListViewItem item = new ListViewItem();

                    item.Text = i.Trim().Replace("\r", "").Replace("\n", "\x23CE");
                    item.Tag = i;
                    logMessageList.Items.Add(i);
                }                
            }
        }

        string _selectedText;
        private void logMessageList_SelectedIndexChanged(object sender, EventArgs e)
        {
            string text = "";

            if(logMessageList.SelectedItems.Count >= 1)
            {
                StringBuilder msg = new StringBuilder();
                bool next = false;
                foreach (ListViewItem item in logMessageList.SelectedItems)
                {
                    if(next)
                        msg.AppendLine();
                    else
                        next = true;
                    msg.Append(item.Text);
                }
                text = msg.ToString();
            }

            previewBox.Text = _selectedText = text;
        }
    }
}
