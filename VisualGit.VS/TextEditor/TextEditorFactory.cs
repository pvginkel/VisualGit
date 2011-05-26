using System;
using System.Collections.Generic;
using System.Text;
using VisualGit.UI;
using VisualGit.UI.VS.TextEditor;

namespace VisualGit.VS.TextEditor
{
    [GlobalService(typeof(IVSTextEditorFactory))]
    class TextEditorFactory : VisualGitService, IVSTextEditorFactory
    {
        public TextEditorFactory(IVisualGitServiceProvider context)
            : base(context)
        {
        }

        #region IVSTextEditorFactory Members

        public bool TryInstantiateIn(VSTextEditor editor, out IVSTextEditorImplementation implementation)
        {
            TheVSTextEditor edit = new TheVSTextEditor();
            edit.Dock = System.Windows.Forms.DockStyle.Fill;

            implementation = edit;

            implementation.ForceLanguageService = editor.ForceLanguageService;
            implementation.DisableWordWrap = editor.DisableWordWrap;
            implementation.HideHorizontalScrollBar = editor.HideHorizontalScrollBar;
            implementation.EnableSplitter = editor.EnableSplitter;
            implementation.EnableNavigationBar = editor.EnableNavigationBar;
            implementation.InteractiveEditor = editor.InteractiveEditor;
            implementation.ReadOnly = editor.ReadOnly;
            implementation.Text = editor.Text;

            editor.Controls.Add(edit);

            return true;
        }

        #endregion
    }
}
