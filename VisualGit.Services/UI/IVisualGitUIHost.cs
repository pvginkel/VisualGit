using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using Microsoft.VisualStudio.OLE.Interop;

namespace VisualGit.UI.Services
{
    /// <summary>
    /// Site as set on package hosted components
    /// </summary>
    [CLSCompliant(false)]
    public interface IVisualGitUISite : IVisualGitServiceProvider
    {
        IVisualGitPackage Package { get; }
        string Title { get; set; }
        string OriginalTitle { get; }

		void AddCommandTarget(IOleCommandTarget target);
    }
}
