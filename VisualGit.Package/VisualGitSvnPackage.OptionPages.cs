using System;
using System.Collections.Generic;
using System.Text;
using VisualGit.VSPackage.Attributes;
using Microsoft.VisualStudio.Shell;

namespace VisualGit.VSPackage
{
    [ProvideOptionPage(typeof(UserToolsSettingsPage), "Source Control", "Subversion User Tools", 106, 108, false)]
    [ProvideToolsOptionsPageVisibility("Source Control", "Subversion User Tools", VisualGitId.SccProviderId)]
    [ProvideOptionPage(typeof(EnvironmentSettingsPage), "Source Control", "Subversion", 106, 107, false)]
    [ProvideToolsOptionsPageVisibility("Source Control", "Subversion", VisualGitId.SccProviderId)]
    partial class VisualGitSvnPackage
    {
    }
}
