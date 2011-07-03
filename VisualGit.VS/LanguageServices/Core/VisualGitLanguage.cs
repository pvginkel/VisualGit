// VisualGit.VS\LanguageServices\Core\VisualGitLanguage.cs
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

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using LanguagePreferences = Microsoft.VisualStudio.Package.LanguagePreferences;

using VisualGit.Selection;


namespace VisualGit.VS.LanguageServices.Core
{
    public abstract partial class VisualGitLanguage : VisualGitService, IVsLanguageInfo
    {
        protected VisualGitLanguage(IVisualGitServiceProvider context)
            : base(context)
        {
        }

        VisualGitCommandMenu _defaultContextMenu;
        public VisualGitCommandMenu DefaultContextMenu
        {
            get { return _defaultContextMenu; }
            set { _defaultContextMenu = value; }
        }

        #region IVsLanguageInfo Members

        int IVsLanguageInfo.GetCodeWindowManager(IVsCodeWindow codeWindow, out IVsCodeWindowManager codeWindowManager)
        {
            codeWindowManager = CreateCodeWindowManager(codeWindow);

            if (codeWindowManager != null)
                return VSConstants.S_OK;
            else
                return VSConstants.E_NOTIMPL;
        }

        [CLSCompliant(false)]
        protected virtual VisualGitCodeWindowManager CreateCodeWindowManager(IVsCodeWindow codeWindow)
        {
            return new VisualGitCodeWindowManager(this, codeWindow);
        }

        int IVsLanguageInfo.GetColorizer(IVsTextLines lines, out IVsColorizer colorizer)
        {
            colorizer = CreateColorizer(lines);

            if (colorizer != null)
                return VSConstants.S_OK;
            else
                return VSConstants.E_NOTIMPL;
        }

        [CLSCompliant(false)]
        protected virtual VisualGitColorizer CreateColorizer(IVsTextLines lines)
        {
            return new VisualGitColorizer(this, lines);
        }

        int IVsLanguageInfo.GetFileExtensions(out string pbstrExtensions)
        {
            pbstrExtensions = FileExtensions ?? "";
            return VSConstants.S_OK;
        }

        protected virtual string FileExtensions
        {
            get { return ""; }
        }

        public int GetLanguageName(out string name)
        {
            name = Name;
            return VSConstants.S_OK;
        }

        public abstract string Name
        {
            get;
        }

        [CLSCompliant(false)]
        protected virtual LanguagePreferences CreatePreferences()
        {
            LanguagePreferences preferences;

            preferences = new LanguagePreferences(this, GetType().GUID, Name);
            preferences.Init();

            return preferences;
        }

        LanguagePreferences _preferences;
        [CLSCompliant(false)]
        public LanguagePreferences LanguagePreferences
        {
            get { return _preferences ?? (_preferences = CreatePreferences()); }
        }

        #endregion

        #region IObjectWithAutomation Members

        public object AutomationObject
        {
            get { return LanguagePreferences; }
        }

        #endregion

        [CLSCompliant(false)]
        protected internal virtual void OnNewView(VisualGitCodeWindowManager codeWindowManager, IVsTextView view)
        {
            VisualGitViewFilter filter = CreateFilter(codeWindowManager, view);

            if (filter != null)
            {
                IOleCommandTarget chained;
                view.AddCommandFilter(filter, out chained);

                filter.AddChained(chained);
            }
        }

        internal void OnCloseView(VisualGitCodeWindowManager visualGitCodeWindowManager, IVsTextView view)
        {

        }

        [CLSCompliant(false)]
        protected virtual VisualGitViewFilter CreateFilter(VisualGitCodeWindowManager manager, IVsTextView view)
        {
            return new VisualGitViewFilter(manager, view);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual bool NeedsPerLineState
        {
            get { return false; }
        }

        public virtual VisualGitLanguageDropDownBar CreateDropDownBar(VisualGitCodeWindowManager manager)
        {
            return null;
        }
    }
}
