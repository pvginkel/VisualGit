using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.Shell;
using System.ComponentModel;
using Microsoft.VisualStudio.Shell.Interop;
using VisualGit.UI.Services;

namespace VisualGit.UI
{
    [CLSCompliant(false)]
    public interface IVisualGitToolWindowHost : IVisualGitUISite
    {
        IVsWindowFrame Frame { get; }
        IVsWindowPane Pane { get; }

        /// <summary>
        /// Gets or sets the keyboard context of the frame window
        /// </summary>
        Guid KeyboardContext { get; set; }
        /// <summary>
        /// Gets or sets the command context of the frame window
        /// </summary>
        Guid CommandContext { get; set; }

		/// <summary>
		/// Gets a boolean indicating whether the toolwindow is on screen
		/// </summary>
		bool IsOnScreen { get; }
	}
}
