using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;
using System.ComponentModel.Design;
using VisualGit.UI;

namespace VisualGit.VSPackage
{
    [ProvideEditorFactoryAttribute(typeof(VisualGitDiffEditorFactory), 302)]
    [ProvideEditorFactoryAttribute(typeof(VisualGitDynamicEditorFactory), 303)]
    [ProvideEditorLogicalView(typeof(VisualGitDiffEditorFactory), VisualGitId.DiffEditorViewId)]
    partial class VisualGitPackage
    {
        void RegisterEditors()
        {
            RegisterEditorFactory(new VisualGitDiffEditorFactory(this));

            VisualGitDynamicEditorFactory def = new VisualGitDynamicEditorFactory(this);

            RegisterEditorFactory(def);
            _runtime.GetService<IServiceContainer>().AddService(typeof(IVisualGitDynamicEditorFactory), def);
        }
    }
}
