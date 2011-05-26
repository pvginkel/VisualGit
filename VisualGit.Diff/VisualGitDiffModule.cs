using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace VisualGit.Diff
{
    public class VisualGitDiffModule : Module
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VisualGitDiffModule"/> class.
        /// </summary>
        /// <param name="runtime">The runtime.</param>
        public VisualGitDiffModule(VisualGitRuntime runtime)
            : base(runtime)
        {
        }

        /// <summary>
        /// Called when added to the <see cref="VisualGitRuntime"/>
        /// </summary>
        public override void OnPreInitialize()
        {
            Assembly thisAssembly = typeof(VisualGitDiffModule).Assembly;

            Runtime.CommandMapper.LoadFrom(thisAssembly);

            Runtime.LoadServices(Container, thisAssembly, Context);
        }

        /// <summary>
        /// Called when <see cref="VisualGitRuntime.Start"/> is called
        /// </summary>
        public override void OnInitialize()
        {
            
        }
    }
}
