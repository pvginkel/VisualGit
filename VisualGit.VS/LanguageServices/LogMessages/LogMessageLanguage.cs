using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TextManager.Interop;
using VisualGit.VS.LanguageServices.Core;

namespace VisualGit.VS.LanguageServices.LogMessages
{
    /// <summary>
    /// Implements a simple VS Languageservice to implement syntaxcoloring on our LogMessages
    /// </summary>
    [Guid(VisualGitId.LogMessageLanguageServiceId), ComVisible(true), CLSCompliant(false)]
    [GlobalService(typeof(LogMessageLanguage), true)]
    public partial class LogMessageLanguage : VisualGitLanguage
    {
        public const string ServiceName = VisualGitId.LogMessageServiceName;

        public LogMessageLanguage(IVisualGitServiceProvider context)
            : base(context)
        {
            DefaultContextMenu = VisualGitCommandMenu.LogMessageEditorContextMenu;
        }

        protected override VisualGitColorizer CreateColorizer(IVsTextLines textLines)
        {
            return new LogMessageColorizer(this, textLines);
        }

        public override bool NeedsPerLineState
        {
            get { return true; }
        }

        public override string Name
        {
            get { return ServiceName; }
        }
    }
}
