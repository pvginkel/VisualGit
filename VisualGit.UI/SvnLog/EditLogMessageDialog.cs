namespace VisualGit.UI.SvnLog
{
    public partial class EditLogMessageDialog : VSContainerForm
    {
        public EditLogMessageDialog()
        {
            InitializeComponent();
        }

        public string LogMessage
        {
            get { return logMessageEditor.Text; }
            set { logMessageEditor.Text = value; }
        }
    }
}
