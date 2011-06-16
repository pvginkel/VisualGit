using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Package;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio;
using VisualGit.Scc;
using System.Diagnostics;
using VisualGit.UI;
using VisualGit.VS.LanguageServices.Core;

namespace VisualGit.VS.LanguageServices.LogMessages
{
    class LogMessageColorizer : VisualGitColorizer
    {
        public LogMessageColorizer(LogMessageLanguage language, IVsTextLines lines)
            : base(language, lines)
        {
        }

        IVisualGitConfigurationService _svc;
        IVisualGitConfigurationService Configuration
        {
            get { return _svc ?? (_svc = GetService<IVisualGitConfigurationService>()); }
        }

        protected override void ColorizeLine(string line, int lineNr, int startState, uint[] attrs, out int endState)
        {
            if (!Configuration.Instance.DisableDashInLogComment)
            {
                bool isComment = false;
                for (int i = 0; i < line.Length; i++)
                {
                    if (!char.IsWhiteSpace(line, i))
                    {
                        if (line[i] == '#')
                            isComment = true;
                        break;
                    }
                }

                if (isComment)
                {
                    for (int i = 0; i < attrs.Length; i++)
                        attrs[i] = (uint)TokenColor.Comment | (uint)COLORIZER_ATTRIBUTE.HUMAN_TEXT_ATTR;

                    endState = 1; 
                    return;
                }
            }
            for (int i = 0; i < attrs.Length; i++)
                attrs[i] = (uint)TokenColor.Text | (uint)COLORIZER_ATTRIBUTE.HUMAN_TEXT_ATTR;

            string combined = null;
            int start = 0, end;
            endState = 1;

            if (startState == 0 && lineNr > 0)
            {
                combined = GetLine(lineNr - 1) + "\n";
                start = combined.Length;
            }

            combined += line;

            end = combined.Length;

            string followed = GetLine(lineNr + 1);
            if (!string.IsNullOrEmpty(followed))
                combined += "\n" + followed;

            if (!string.IsNullOrEmpty(line))
                endState = 0;
        }
    }
}
