// VisualGit.VS\LanguageServices\VisualGitEditorResolver.cs
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
using System.Text;
using Microsoft.VisualStudio.Package;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace VisualGit.VS.LanguageServices
{
    [GlobalService(typeof(IVisualGitEditorResolver))]
    class VisualGitEditorResolver : VisualGitService, IVisualGitEditorResolver
    {
        readonly EditorFactory _factory;
        public VisualGitEditorResolver(IVisualGitServiceProvider context)
            : base(context)
        {
            _factory = new EditorFactory();
            _factory.SetSite(this);
        }

        public bool TryGetLanguageService(string extension, out Guid languageService)
        {
            if (!string.IsNullOrEmpty(extension))
            {
                string value = _factory.GetLanguageService(extension);

                if (!string.IsNullOrEmpty(value))
                {
                    languageService = new Guid(value);
                    return true;
                }
            }
            languageService = Guid.Empty;
            return false;
        }
    }
}
