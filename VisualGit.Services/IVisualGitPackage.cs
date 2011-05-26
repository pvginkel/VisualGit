using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using VisualGit.VS;
using Microsoft.Win32;

namespace VisualGit.UI
{
    /// <summary>
    /// Public api of the VisualGit package as used by other components
    /// </summary>
    [CLSCompliant(false)]
    public interface IVisualGitPackage : IVisualGitServiceProvider, System.ComponentModel.Design.IServiceContainer, IVisualGitQueryService
    {
        /// <summary>
        /// Gets the UI version. Retrieved from the registry after being installed by our MSI
        /// </summary>
        /// <value>The UI version.</value>
        Version UIVersion { get; }

        /// <summary>
        /// Gets the package version. The assembly version of VisualGit.Package.dll
        /// </summary>
        /// <value>The package version.</value>
        Version PackageVersion { get; }

        void ShowToolWindow(VisualGitToolWindow window);
        void ShowToolWindow(VisualGitToolWindow window, int id, bool create);

        void CloseToolWindow(VisualGitToolWindow toolWindow, int id, Microsoft.VisualStudio.Shell.Interop.__FRAMECLOSE frameClose);

        void RegisterIdleProcessor(IVisualGitIdleProcessor processor);
        void UnregisterIdleProcessor(IVisualGitIdleProcessor processor);

        AmbientProperties AmbientProperties { get; }

        bool LoadUserProperties(string streamName);

        // Summary:
        //     Gets the root registry key of the current Visual Studio registry hive.
        //
        // Returns:
        //     The root Microsoft.Win32.RegistryKey of the Visual Studio registry hive.
        RegistryKey ApplicationRegistryRoot { get; }

		/// <summary>
		/// Gets a registry key that can be used to store user data. 
		/// </summary>
		RegistryKey UserRegistryRoot { get; }
    }
}
