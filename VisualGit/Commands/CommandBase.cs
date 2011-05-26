using System;
using VisualGit.Selection;
using VisualGit.Scc;

namespace VisualGit.Commands
{
    /// <summary>
    /// Base class for ICommand instances
    /// </summary>
    public abstract class CommandBase : ICommandHandler
    {
        public virtual void OnUpdate(CommandUpdateEventArgs e)
        {
            // Just leave the defaults: Enabled          
        }

        public abstract void OnExecute(CommandEventArgs e);

        /// <summary>
        /// Gets whether the Shift key was down when the current window message was send
        /// </summary>
        protected static bool Shift
        {
            get
            {
                return (0 != (System.Windows.Forms.Control.ModifierKeys & System.Windows.Forms.Keys.Shift));
            }
        }

        /// <summary>
        /// Saves all dirty documents within the provided selection
        /// </summary>
        /// <param name="selection">The selection.</param>
        /// <param name="context">The context.</param>
        protected static void SaveAllDirtyDocuments(ISelectionContext selection, IVisualGitServiceProvider context)
        {
            if (selection == null)
                throw new ArgumentNullException("selection");
            if (context == null)
                throw new ArgumentNullException("context");

            IVisualGitOpenDocumentTracker tracker = context.GetService<IVisualGitOpenDocumentTracker>();
            if (tracker != null)
                tracker.SaveDocuments(selection.GetSelectedFiles(true));
        }
    }
}
