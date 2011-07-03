// VisualGit.VS\LanguageServices\UnifiedDiff\UnifiedDiffLanguage.cs
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
using System.Runtime.InteropServices;

using Microsoft.VisualStudio.Package;

using VisualGit.VS.LanguageServices.Core;
using Microsoft.VisualStudio.TextManager.Interop;

namespace VisualGit.VS.LanguageServices.UnifiedDiff
{
    [Guid(VisualGitId.UnifiedDiffLanguageServiceId), ComVisible(true), CLSCompliant(false)]
    [GlobalService(typeof(UnifiedDiffLanguage), true)]
    public class UnifiedDiffLanguage : VisualGitLanguage
    {
        public const string ServiceName = VisualGitId.UnifiedDiffServiceName;

        public UnifiedDiffLanguage(IVisualGitServiceProvider context)
            : base(context)
        {
        }

        public override string Name
        {
            get { return ServiceName; }
        }

        protected override VisualGitColorizer CreateColorizer(IVsTextLines lines)
        {
            return new UnifiedDiffColorizer(this, lines);
        }

        public override VisualGitLanguageDropDownBar CreateDropDownBar(VisualGitCodeWindowManager manager)
        {
            return new UnifiedDiffDropDownBar(this, manager);
        }

    }
}
