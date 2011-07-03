// VisualGit.VS\LanguageServices\UnifiedDiff\UnifiedDiffColorizer.cs
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

using Microsoft.VisualStudio.TextManager.Interop;

using VisualGit.VS.LanguageServices.Core;

namespace VisualGit.VS.LanguageServices.UnifiedDiff
{
    class UnifiedDiffColorizer : VisualGitColorizer
    {
        public UnifiedDiffColorizer(UnifiedDiffLanguage language, IVsTextLines lines)
            : base(language, lines)
        {
        }

        protected override void ColorizeLine(string line, int lineNr, int startState, uint[] attrs, out int endState)
        {
            char c = '\0';

            if (line.Length >= 1)
                c = line[0];

            uint attr;

            switch (c)
            {
                case '+':
                    attr = (uint)TokenColor.String | (uint)COLORIZER_ATTRIBUTE.HUMAN_TEXT_ATTR;
                    break;
                case '-':
                    attr = (uint)TokenColor.Keyword | (uint)COLORIZER_ATTRIBUTE.HUMAN_TEXT_ATTR;
                    break;
                case '@':
                    attr = (uint)TokenColor.Comment | (uint)COLORIZER_ATTRIBUTE.HUMAN_TEXT_ATTR;
                    break;
                default:
                    attr = (uint)TokenColor.Text | (uint)COLORIZER_ATTRIBUTE.HUMAN_TEXT_ATTR; ;
                    break;
            }

            for(int i = 0; i < attrs.Length; i++)
                attrs[i] = attr;

            endState = 0;
        }
    }
}
