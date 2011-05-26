using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio;

namespace VisualGit.Scc
{
#if DEBUG
    // Currently unused in production code
    [GlobalService(typeof(IVisualGitTextEditingTracker))]
#endif
    class TextEditingTracker : VisualGitService, IVisualGitTextEditingTracker, IVsTextManagerEvents2
    {
        IVsTextManager _manager;
        IVsTextManager2 _manager2;
        uint _cookie;

        public TextEditingTracker(IVisualGitServiceProvider context)
            : base(context)
        {
        }

        IVsTextManager Manager
        {
            get { return _manager ?? (_manager = GetService<IVsTextManager>(typeof(SVsTextManager))); }
        }

        IVsTextManager2 Manager2
        {
            get { return _manager2 ?? (_manager2 = Manager as IVsTextManager2); }
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            TryHookConnectionPoint<IVsTextManagerEvents2>(Manager, this, out _cookie);
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (_cookie != 0)
                {
                    uint ck = _cookie;
                    _cookie = 0;
                    ReleaseHook<IVsTextManagerEvents2>(Manager, _cookie);
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        public int OnRegisterMarkerType(int iMarkerType)
        {
            return VSConstants.S_OK;
        }

        public int OnRegisterView(IVsTextView pView)
        {
            return VSConstants.S_OK;
        }

        public int OnReplaceAllInFilesBegin()
        {
            return VSConstants.S_OK;
        }

        public int OnReplaceAllInFilesEnd()
        {
            return VSConstants.S_OK;
        }

        public int OnUnregisterView(IVsTextView pView)
        {
            return VSConstants.S_OK;
        }

        public int OnUserPreferencesChanged2(VIEWPREFERENCES2[] pViewPrefs, FRAMEPREFERENCES2[] pFramePrefs, LANGPREFERENCES2[] pLangPrefs, FONTCOLORPREFERENCES2[] pColorPrefs)
        {
            return VSConstants.S_OK;
        }
    }
}
