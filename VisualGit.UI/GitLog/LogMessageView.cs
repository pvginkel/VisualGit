using System.ComponentModel;
using System.Windows.Forms;
using VisualGit.Scc;

namespace VisualGit.UI.GitLog
{
    public partial class LogMessageView : UserControl
    {
        ICurrentItemSource<IGitLogItem> logItemSource;

        public LogMessageView()
        {
            InitializeComponent();
        }

        public LogMessageView(IContainer container)
            : this()
        {
            container.Add(this);
        }

        public ICurrentItemSource<IGitLogItem> ItemSource
        {
            get { return logItemSource; }
            set 
            { 
                if(logItemSource != null)
                    logItemSource.FocusChanged -= LogFocusChanged;

                logItemSource = value; 
                
                if(logItemSource != null)
                    logItemSource.FocusChanged += LogFocusChanged;
            }
        }

        void LogFocusChanged(object sender, CurrentItemEventArgs<IGitLogItem> e)
        {
            if (ItemSource != null && ItemSource.FocusedItem != null)
                logMessageEditor.Text = logItemSource.FocusedItem.LogMessage;
            else
                logMessageEditor.Text = "";
        }

        internal void Reset()
        {
            logMessageEditor.Text = "";
        }
    }
}
