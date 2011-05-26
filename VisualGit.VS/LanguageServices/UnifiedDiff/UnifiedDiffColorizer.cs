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
