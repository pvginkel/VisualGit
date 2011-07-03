// VisualGit.VS\LanguageServices\Core\VisualGitViewFilter.cs
//
// Copyright 2008-2011 The AnkhSVN Project
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//
// Changes and additions made for VisualGit Copyright 2011 Pieter van Ginkel.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using Cursor = System.Windows.Forms.Cursor;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using OleConstants = Microsoft.VisualStudio.OLE.Interop.Constants;



namespace VisualGit.VS.LanguageServices.Core
{
    public class VisualGitViewFilter : VisualGitService, IVsTextViewFilter, IOleCommandTarget
    {
        IOleCommandTarget _fallThrough;
        [CLSCompliant(false)]
        public VisualGitViewFilter(VisualGitCodeWindowManager codeWindowManager, IVsTextView textView)
            : base(codeWindowManager)
        {
        }

        protected VisualGitCodeWindowManager CodeWindowManager
        {
            get { return (VisualGitCodeWindowManager)Context; }
        }

        protected VisualGitLanguage Language
        {
            get { return CodeWindowManager.Language; }
        }

        #region IVsTextViewFilter Members

        [CLSCompliant(false)]
        public int GetDataTipText(TextSpan[] pSpan, out string pbstrText)
        {
            pbstrText = null;
            return VSConstants.E_NOTIMPL;
        }

        [CLSCompliant(false)]
        public int GetPairExtents(int iLine, int iIndex, TextSpan[] pSpan)
        {
            return VSConstants.E_NOTIMPL;
        }

        [CLSCompliant(false)]
        public int GetWordExtent(int iLine, int iIndex, uint dwFlags, TextSpan[] pSpan)
        {
            return VSConstants.E_NOTIMPL;
        }

        #endregion

        #region IOleCommandTarget Members

        int IOleCommandTarget.QueryStatus(ref Guid cmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            /* TODO: Handle things that *must* be handled on every language service; bubble everything else
             *       up the chain */
            Debug.Assert(cCmds == 1);

            int hr = QueryStatus(ref cmdGroup, ref prgCmds[0], pCmdText);

            if (hr != VSConstants.E_FAIL)
                return hr;

            if (_fallThrough != null)
                return _fallThrough.QueryStatus(ref cmdGroup, cCmds, prgCmds, pCmdText);
            else
                return VSConstants.E_FAIL; // delegate to next command target.
        }

        [CLSCompliant(false)]
        protected virtual int QueryStatus(ref Guid cmdGroup, ref OLECMD oleCmd, IntPtr pCmdText)
        {
            if (cmdGroup == VSConstants.VSStd2K)
            {
                switch ((VSConstants.VSStd2KCmdID)oleCmd.cmdID)
                {
                    case VSConstants.VSStd2KCmdID.OUTLN_START_AUTOHIDING:
                    case VSConstants.VSStd2KCmdID.OUTLN_STOP_HIDING_ALL:
                        return (int)OleConstants.OLECMDERR_E_NOTSUPPORTED;

                    default:
                        return VSConstants.E_FAIL; ; /* Unhandled */
                }
            }

            return VSConstants.E_FAIL;
        }

        [CLSCompliant(false)]
        public virtual int Exec(ref Guid cmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (cmdGroup == VSConstants.VSStd2K)
            {
                switch ((VSConstants.VSStd2KCmdID)nCmdID)
                {
                    case VSConstants.VSStd2KCmdID.SHOWCONTEXTMENU:
                        VisualGitCommandMenu menu = Language.DefaultContextMenu;

                        if (menu != 0)
                            ShowContextMenu(VisualGitId.CommandSetGuid, (int)menu, this);
                        else
                            ShowContextMenu(VsMenus.guidSHLMainMenu, VsMenus.IDM_VS_CTXT_CODEWIN, this);
                        return VSConstants.S_OK;
                }
            }
            if (_fallThrough != null)
                return _fallThrough.Exec(ref cmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
            else
                return VSConstants.E_FAIL; // delegate to next command target.
        }

        [CLSCompliant(false)]
        public virtual void ShowContextMenu(Guid cmdGroup, int menuId, IOleCommandTarget target)
        {
            IVsUIShell uiShell = GetService<IVsUIShell>(typeof(SVsUIShell));

            if (uiShell != null)
            {
                Point pt = Cursor.Position;
                POINTS[] where = new POINTS[1];
                where[0].x = (short)pt.X;
                where[0].y = (short)pt.Y;

                uiShell.ShowContextMenu(0, ref cmdGroup, menuId, where, target);
            }
        }

        #endregion

        internal void AddChained(IOleCommandTarget chained)
        {
            _fallThrough = chained;
        }
    }
}
