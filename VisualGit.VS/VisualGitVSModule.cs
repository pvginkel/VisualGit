using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

using VisualGit.Scc;

namespace VisualGit.VS
{
    /// <summary>
    /// 
    /// </summary>
    public class VisualGitVSModule : Module
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VisualGitVSModule"/> class.
        /// </summary>
        /// <param name="runtime">The runtime.</param>
        public VisualGitVSModule(VisualGitRuntime runtime)
            : base(runtime)
        {
        }

        /// <summary>
        /// Called when added to the <see cref="VisualGitRuntime"/>
        /// </summary>
        public override void OnPreInitialize()
        {
            Assembly thisAssembly = typeof(VisualGitVSModule).Assembly;

            Runtime.CommandMapper.LoadFrom(thisAssembly);

            Runtime.LoadServices(Container, thisAssembly, Context);

        }

        /// <summary>
        /// Called when <see cref="VisualGitRuntime.Start"/> is called
        /// </summary>
        public override void OnInitialize()
        {
            EnsureService<IFileStatusCache>();
            EnsureService<IStatusImageMapper>();
        }
    }
}
