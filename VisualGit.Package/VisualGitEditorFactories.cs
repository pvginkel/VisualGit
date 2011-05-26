using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

using VisualGit.UI;
using VisualGit.VS.Dialogs;

namespace VisualGit.VSPackage
{
    abstract class VisualGitEditorFactory : VisualGitService, IVsEditorFactory
    {
        IOleServiceProvider _site;

        protected VisualGitEditorFactory(IVisualGitServiceProvider context)
            : base(context)
        {
        }

        #region IVsEditorFactory Members

        public int Close()
        {
            return VSConstants.S_OK;
        }

        public virtual int CreateEditorInstance(uint grfCreateDoc, string pszMkDocument, string pszPhysicalView, IVsHierarchy pvHier, uint itemid, IntPtr punkDocDataExisting, out IntPtr ppunkDocView, out IntPtr ppunkDocData, out string pbstrEditorCaption, out Guid pguidCmdUI, out int pgrfCDW)
        {
            ppunkDocData = IntPtr.Zero;
            ppunkDocView = IntPtr.Zero;
            pbstrEditorCaption = "";
            pguidCmdUI = new Guid("{00000000-0000-0000-e4e7-120000008400}");
            pgrfCDW = 0;
            // Validate inputs
            if ((grfCreateDoc & (VSConstants.CEF_OPENFILE | VSConstants.CEF_SILENT)) == 0)
            {
                return VSConstants.E_INVALIDARG;
            }

            VSEditorControl form = CreateForm();
            object doc;
            object pane;

            GetService<IVisualGitDocumentHostService>().ProvideEditor(form, FactoryId, out doc, out pane);

            ppunkDocView = Marshal.GetIUnknownForObject(pane);
            ppunkDocData = Marshal.GetIUnknownForObject(doc);

            pbstrEditorCaption = form.Text;
            return VSConstants.S_OK;
        }

        protected abstract Guid FactoryId { get; }

        protected abstract VSEditorControl CreateForm();

        public virtual int MapLogicalView(ref Guid rguidLogicalView, out string pbstrPhysicalView)
        {
            pbstrPhysicalView = null;

            if (rguidLogicalView == VSConstants.LOGVIEWID_Primary)
            {
                pbstrPhysicalView = null;
                return VSConstants.S_OK;
            }

            return VSConstants.E_NOTIMPL;
        }

        public int SetSite(IOleServiceProvider psp)
        {
            _site = psp;
            return VSConstants.S_OK;
        }

        #endregion

    }

    [Guid(VisualGitId.DiffEditorId), ComVisible(true)]
    sealed class VisualGitDiffEditorFactory : VisualGitEditorFactory
    {
        public VisualGitDiffEditorFactory(VisualGitGitPackage package)
            : base(package)
        {

        }

        protected override Guid FactoryId
        {
            get { return new Guid(VisualGitId.DiffEditorId); }
        }

        public override int MapLogicalView(ref Guid rguidLogicalView, out string pbstrPhysicalView)
        {
            if (ErrorHandler.Succeeded(base.MapLogicalView(ref rguidLogicalView, out pbstrPhysicalView)))
                return VSConstants.S_OK;

            if (rguidLogicalView == new Guid(VisualGitId.DiffEditorViewId))
            {
                pbstrPhysicalView = null;
                return VSConstants.S_OK;
            }

            return VSConstants.E_NOTIMPL;
        }

        protected override VSEditorControl CreateForm()
        {
            throw new NotImplementedException();
        }
    }

    [Guid(VisualGitId.DynamicEditorId), ComVisible(true)]
    sealed class VisualGitDynamicEditorFactory : VisualGitEditorFactory, IVisualGitDynamicEditorFactory
    {
        readonly Stack<VSEditorControl> _forms = new Stack<VSEditorControl>();
        public VisualGitDynamicEditorFactory(VisualGitGitPackage package)
            : base(package)
        {
        }

        protected override VSEditorControl CreateForm()
        {
            return _forms.Pop();
        }

        #region IVisualGitDynamicEditorFactory Members

        protected override Guid FactoryId
        {
            get { return new Guid(VisualGitId.DynamicEditorId); }
        }

        /// <summary>
        /// Creates the editor.
        /// </summary>
        /// <param name="fullPath">The full path.</param>
        /// <param name="form">The form.</param>
        /// <returns></returns>
        public IVsWindowFrame CreateEditor(string fullPath, VSEditorControl form)
        {
            if (string.IsNullOrEmpty(fullPath))
                throw new ArgumentNullException("fullPath");
            else if (form == null)
                throw new ArgumentNullException("form");
            else if(form.Context == null)
                throw new ArgumentException("Specified form doesn't have a context");

            _forms.Push(form);

            IVsUIHierarchy hier;
            uint id;
            IVsWindowFrame frame;
            VsShellUtilities.OpenDocumentWithSpecificEditor(Context, fullPath, new Guid(VisualGitId.DynamicEditorId), VSConstants.LOGVIEWID_Primary,
                out hier, out id, out frame);

            if (_forms.Contains(form))
            {
                _forms.Pop();
                throw new InvalidOperationException("Can't create dynamic editor (Already open?)");
            }

            GetService<IVisualGitDocumentHostService>().InitializeEditor(form, hier, frame, id);

            return frame;
        }

        public override int CreateEditorInstance(uint grfCreateDoc, string pszMkDocument, string pszPhysicalView, IVsHierarchy pvHier, uint itemid, IntPtr punkDocDataExisting, out IntPtr ppunkDocView, out IntPtr ppunkDocData, out string pbstrEditorCaption, out Guid pguidCmdUI, out int pgrfCDW)
        {
            if (_forms.Count == 0)
            {
                ppunkDocView = IntPtr.Zero;
                ppunkDocData = IntPtr.Zero;
                pbstrEditorCaption = null;
                pguidCmdUI = Guid.Empty;
                pgrfCDW = 0;
                return VSConstants.E_UNEXPECTED;
            }

            return base.CreateEditorInstance(grfCreateDoc, pszMkDocument, pszPhysicalView, pvHier, itemid, punkDocDataExisting, out ppunkDocView, out ppunkDocData, out pbstrEditorCaption, out pguidCmdUI, out pgrfCDW);
        }

        #endregion
    }
}
