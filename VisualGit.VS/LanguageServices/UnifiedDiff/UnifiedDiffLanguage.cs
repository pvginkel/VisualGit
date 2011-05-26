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
