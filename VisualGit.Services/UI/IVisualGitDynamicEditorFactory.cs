using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.Shell.Interop;

namespace VisualGit.UI
{
    /// <summary>
    /// 
    /// </summary>
    [CLSCompliant(false)]
    public interface IVisualGitDynamicEditorFactory
    {
        /// <summary>
        /// Creates the editor.
        /// </summary>
        /// <param name="fullPath">The full path.</param>
        /// <param name="form">The form.</param>
        /// <returns></returns>
        IVsWindowFrame CreateEditor(string fullPath, VSEditorControl form);
    }

    [CLSCompliant(false)]
    public interface IVisualGitDocumentHostService
    {
        /// <summary>
        /// Prepares the editor for hosting in a document window
        /// </summary>
        /// <param name="form">The form.</param>
        /// <param name="factoryId">The factory id.</param>
        /// <param name="doc">The doc.</param>
        /// <param name="pane">The pane.</param>
        void ProvideEditor(VSEditorControl form, Guid factoryId, out object doc, out object pane);

        /// <summary>
        /// Initializes the editor.
        /// </summary>
        /// <param name="frame">The frame.</param>
        void InitializeEditor(VSEditorControl form, IVsUIHierarchy hier, IVsWindowFrame frame, uint docid);
    }
}
