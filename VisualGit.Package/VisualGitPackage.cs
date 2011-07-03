// VisualGit.Package\VisualGitPackage.cs
//
// Copyright 2008-2011 The AnkhSVN Project
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//
// Changes and additions made for VisualGit Copyright 2011 Pieter van Ginkel.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;

using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

using VisualGit.Scc;
using VisualGit.VS;
using VisualGit.UI;
using VisualGit.VSPackage.Attributes;
using VisualGit.Diff;

namespace VisualGit.VSPackage
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the registration utility (regpkg.exe) that this class needs
    // to be registered as package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [Description(VisualGitId.PackageDescription)]
    // A Visual Studio component can be registered under different regitry roots; for instance
    // when you debug your package you want to register it in the experimental hive. This
    // attribute specifies the registry root to use if no one is provided to regpkg.exe with
    // the /root switch.
    [DefaultRegistryRoot("Software\\Microsoft\\VisualStudio\\9.0")]

    // In order be loaded inside Visual Studio in a machine that has not the VS SDK installed, 
    // package needs to have a valid load key (it can be requested at 
    // http://msdn.microsoft.com/vstudio/extend/). This attributes tells the shell that this 
    // package has a load key embedded in its resources.
    [ProvideLoadKey("Standard", VisualGitId.PlkVersion, VisualGitId.PlkProduct, VisualGitId.PlkCompany, 1)]
    [Guid(VisualGitId.PackageId)]
    [ProvideAutoLoad(VisualGitId.SccProviderId)] // Load on 'Scc active'

    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResourceEx("1000.ctmenu", 1)] // The number must match the number in the .csproj file for the ctc task

    [ProvideKeyBindingTable(VisualGitId.LogViewContext, 501)]
    [ProvideKeyBindingTable(VisualGitId.DiffMergeViewContext, 502)]
    //[ProvideKeyBindingTable(VisualGitId.PendingChangeViewContext, 503)] // Won't work at this time
    [ProvideKeyBindingTable(VisualGitId.SccExplorerViewContext, 504)]

    [CLSCompliant(false)]
    [ProvideSourceControlProvider(VisualGitId.SccProviderTitle, "#100")]
    [ProvideService(typeof(ITheVisualGitSccProvider), ServiceName = VisualGitId.GitSccName)]
    [ProvideOutputWindow(VisualGitId.VisualGitOutputPaneId, "#111", InitiallyInvisible = false, Name = VisualGitId.PlkProduct, ClearWithSolution = false)]
    sealed partial class VisualGitPackage : Package, IVisualGitPackage, IVisualGitQueryService
    {
        readonly VisualGitRuntime _runtime;

        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public VisualGitPackage()
        {
            _runtime = new VisualGitRuntime(this);
        }

        /////////////////////////////////////////////////////////////////////////////
        // Overriden Package Implementation        

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            if (InCommandLineMode)
                return; // Do nothing; speed up devenv /setup by not loading all our modules!

            try
            {
                Trace.WriteLine("VisualGit: Loading package");
            }
            catch
            {
                new VisualGitMessageBox(this).Show(Resources.DotNetTracingFails);
            }

            InitializeRuntime(); // Moved to function of their own to speed up devenv /setup
            RegisterAsOleComponent();
        }

        void InitializeRuntime()
        {
            IServiceContainer container = GetService<IServiceContainer>();
            container.AddService(typeof(IVisualGitPackage), this, true);
            container.AddService(typeof(IVisualGitQueryService), this, true);

            _runtime.AddModule(new VisualGitModule(_runtime));
            _runtime.AddModule(new VisualGitSccModule(_runtime));
            _runtime.AddModule(new VisualGitVSModule(_runtime));
            _runtime.AddModule(new VisualGitUIModule(_runtime));
            _runtime.AddModule(new VisualGitDiffModule(_runtime));

            RegisterEditors();

            NotifyLoaded(false);

            _runtime.Start();

            NotifyLoaded(true);
        }

        private void NotifyLoaded(bool started)
        {
            // We set the user context VisualGitLoadCompleted active when we are loaded
            // This event can be used to trigger loading other packages that depend on VisualGit
            // 
            // When the use:
            // [ProvideAutoLoad(VisualGitId.VisualGitLoadCompleted)]
            // On their package, they load automatically when we are completely loaded
            //

            IVsMonitorSelection ms = GetService<IVsMonitorSelection>();
            if (ms != null)
            {
                Guid gVisualGitLoaded = new Guid(started ? VisualGitId.VisualGitRuntimeStarted : VisualGitId.VisualGitServicesAvailable);

                uint cky;
                if (ErrorHandler.Succeeded(ms.GetCmdUIContextCookie(ref gVisualGitLoaded, out cky)))
                {
                    ms.SetCmdUIContext(cky, 1);
                }
            }
        }

        /// <summary>
        /// Gets the context.
        /// </summary>
        /// <value>The context.</value>
        public VisualGitContext Context
        {
            get { return _runtime.Context; }
        }

        bool? _inCommandLineMode;
        /// <summary>
        /// Get a boolean indicating whether we are running in commandline mode
        /// </summary>
        public bool InCommandLineMode
        {
            get
            {
                if (!_inCommandLineMode.HasValue)
                {
                    IVsShell shell = (IVsShell)GetService(typeof(SVsShell));

                    if (shell == null)
                        _inCommandLineMode = false; // Probably running in a testcase; the shell loads us!
                    else
                    {
                        object value;
                        if (ErrorHandler.Succeeded(shell.GetProperty((int)__VSSPROPID.VSSPROPID_IsInCommandLineMode, out value)))
                        {
                            _inCommandLineMode = Convert.ToBoolean(value);
                        }
                    }
                }

                return _inCommandLineMode.Value;
            }
        }

        #region IVisualGitQueryService Members

        static Guid IID_IUnknown = VSConstants.IID_IUnknown;

        public T QueryService<T>(Guid serviceGuid) where T : class
        {
            IOleServiceProvider sp = GetService<IOleServiceProvider>();
            IntPtr handle;

            if (sp == null)
                return null;

            if (!ErrorHandler.Succeeded(sp.QueryService(ref serviceGuid, ref IID_IUnknown, out handle))
                || handle == IntPtr.Zero)
                return null;

            try
            {
                object ob = Marshal.GetObjectForIUnknown(handle);

                return ob as T;
            }
            finally
            {
                Marshal.Release(handle);
            }
        }

        #endregion
    }
}
