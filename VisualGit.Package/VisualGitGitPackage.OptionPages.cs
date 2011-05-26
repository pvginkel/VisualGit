using System;
using System.Collections.Generic;
using System.Text;
using VisualGit.VSPackage.Attributes;
using Microsoft.VisualStudio.Shell;

namespace VisualGit.VSPackage
{
    [ProvideOptionPage(typeof(UserToolsSettingsPage), "Source Control", "Git User Tools", 106, 108, false)]
    [ProvideToolsOptionsPageVisibility("Source Control", "Git User Tools", VisualGitId.SccProviderId)]
    [ProvideOptionPage(typeof(EnvironmentSettingsPage), "Source Control", "Git", 106, 107, false)]
    [ProvideToolsOptionsPageVisibility("Source Control", "Git", VisualGitId.SccProviderId)]
    partial class VisualGitGitPackage
    {
    }
}
