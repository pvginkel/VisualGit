using System;
using System.Collections.Generic;
using System.Text;
using VisualGit.UI.PendingChanges;
using VisualGit.UI.Services;
using VisualGit.Scc;
using System.Reflection;

namespace VisualGit.UI
{
    public class VisualGitUIModule : Module
    {
        public VisualGitUIModule(VisualGitRuntime runtime)
            : base(runtime)
        {

        }

        public override void OnPreInitialize()
        {
            Assembly thisAssembly = typeof(VisualGitUIModule).Assembly;

            Runtime.CommandMapper.LoadFrom(thisAssembly);
            Runtime.LoadServices(Container, thisAssembly);
        }

        public override void OnInitialize()
        {
            
        }
    }
}
