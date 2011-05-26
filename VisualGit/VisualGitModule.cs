using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using VisualGit.Commands;

namespace VisualGit
{
    public class VisualGitModule : Module
    {
        public VisualGitModule(VisualGitRuntime runtime)
            : base(runtime)
        {
        }

        public override void OnPreInitialize()
        {
            Assembly thisAssembly = typeof(VisualGitModule).Assembly;

            Runtime.CommandMapper.LoadFrom(thisAssembly);

            Runtime.LoadServices(Container, thisAssembly, Context);
        }

        public override void OnInitialize()
        {
            EnsureService<IVisualGitErrorHandler>();
            EnsureService<IVisualGitCommandService>();

            CheckForUpdates.MaybePerformUpdateCheck(Context);
        }
    }
}
