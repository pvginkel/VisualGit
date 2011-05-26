using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using VisualGit.UI.PendingChanges.Commits;

namespace VisualGit.UI.PendingChanges
{
    /// <summary>
    /// This class is used to implement CodeEditorUserControl
    /// </summary>
    /// <seealso cref="UserControl"/>
    public class LogMessageEditor : VSTextEditor
    {
        
        public LogMessageEditor()
        {
            BackColor = SystemColors.Window;
            base.ForceLanguageService = new Guid(VisualGitId.LogMessageLanguageServiceId);
        }

        public LogMessageEditor(IContainer container)
            : base(container)
        {
            base.ForceLanguageService = new Guid(VisualGitId.LogMessageLanguageServiceId);
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Guid? ForceLanguageService
        {
            get { return base.ForceLanguageService; }
            set { throw new InvalidOperationException(); }
        }

        IPendingChangeSource _pasteSrc;
        /// <summary>
        /// Gets or sets the paste source.
        /// </summary>
        /// <value>The paste source.</value>
        [DefaultValue(null)]
        public IPendingChangeSource PasteSource
        {
            get { return _pasteSrc; }
            set { _pasteSrc = value; }
        }
    }
}

