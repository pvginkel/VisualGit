using System;
using System.Collections.Generic;
using System.Text;
using VisualGit.UI;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace VisualGit.VS.Dialogs
{
    [GlobalService(typeof(IVisualGitDocumentHostService))]
    class VSDocumentHostService : VisualGitService, IVisualGitDocumentHostService
    {
        public VSDocumentHostService(IVisualGitServiceProvider context)
            : base(context)
        {
        }
        public void ProvideEditor(VSEditorControl form, Guid factoryId, out object doc, out object pane)
        {
            VSDocumentInstance dc = new VSDocumentInstance(Context, factoryId);
            pane = new VSDocumentFormPane(Context, dc, form);

            doc = dc;
        }

        #region IVisualGitDocumentHostService Members

        public void InitializeEditor(VSEditorControl form, IVsUIHierarchy hier, IVsWindowFrame frame, uint docid)
        {
            VSDocumentFormPane pane = null;
            object value;
            if (ErrorHandler.Succeeded(frame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out value)))
            {
                pane = value as VSDocumentFormPane;
            }


            if (pane != null)
                ((IVSEditorControlInit)form).InitializedForm(hier, docid, frame, pane.Host);
        }

        #endregion
    }
}
