// VisualGit.VS\LanguageServices\Core\VisualGitCodeWindowManager.cs
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
using System.Runtime.InteropServices;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.TextManager.Interop;

namespace VisualGit.VS.LanguageServices.Core
{
    public class VisualGitCodeWindowManager : VisualGitService, IVsCodeWindowManager, IVsCodeWindowEvents
    {
        IVsCodeWindow _window;
        VisualGitLanguageDropDownBar _bar;
        readonly List<IVsTextView> _views;
        uint _cookie;

        [CLSCompliant(false)]
        public VisualGitCodeWindowManager(VisualGitLanguage language, IVsCodeWindow window)
            : base(language)
        {
            if (window == null)
                throw new ArgumentNullException("window");

            _window = window;
            _views = new List<IVsTextView>();

            if (!TryHookConnectionPoint<IVsCodeWindowEvents>(_window, this, out _cookie))
                _cookie = 0;
        }

        public VisualGitLanguage Language
        {
            get { return (VisualGitLanguage)Context; }
        }

        [CLSCompliant(false)]
        public IVsCodeWindow CodeWindow
        {
            get { return _window; }
        }

        public void Close()
        {
            object window = _window;
            _window = null;
            if (window != null)
            {
                if (_cookie != 0)
                {
                    ReleaseHook<IVsCodeWindowEvents>(window, _cookie);
                    _cookie = 0;
                }

                if (Marshal.IsComObject(window))
                    try
                    {
                        Marshal.ReleaseComObject(window);
                    }
                    catch { }
            }
        }

        public int AddAdornments()
        {
            IVsTextView primaryView, secondaryView;
            if (ErrorHandler.Succeeded(_window.GetPrimaryView(out primaryView)) && primaryView != null)
                OnNewView(primaryView);

            if (ErrorHandler.Succeeded(_window.GetSecondaryView(out secondaryView)) && secondaryView != null)
                OnNewView(secondaryView);

            if (primaryView != null || secondaryView != null)
            {
                VisualGitLanguageDropDownBar bar = Language.CreateDropDownBar(this);

                if (bar != null)
                {
                    _bar = bar;

                    bar.Initialize();
                }
            }

            return VSConstants.S_OK;
        }

        public int RemoveAdornments()
        {
            VisualGitLanguageDropDownBar bar = _bar;
            _bar = null;

            if (bar != null)
                bar.Close();
            return VSConstants.S_OK;
        }

        [CLSCompliant(false)]
        public virtual int OnNewView(IVsTextView view)
        {
            if (!_views.Contains(view))
            {
                _views.Add(view);
                Language.OnNewView(this, view);

                if (_bar != null)
                    _bar.OnNewView(view);
            }

            return VSConstants.S_OK;
        }

        #region IVsCodeWindowEvents Members

        int IVsCodeWindowEvents.OnCloseView(IVsTextView view)
        {
            if (_views.Contains(view))
            {
                _views.Remove(view);

                if (_bar != null)
                    _bar.OnCloseView(view);


                Language.OnCloseView(this, view);

                OnCloseView(view);

                if (_views.Count == 0)
                    Close();
            }

            return VSConstants.S_OK;
        }

        [CLSCompliant(false)]
        protected virtual void OnCloseView(IVsTextView view)
        {
            
        }

        #endregion

        [CLSCompliant(false)]
        protected internal IEnumerable<IVsTextView> GetViews()
        {
            return _views;
        }
    }
}
