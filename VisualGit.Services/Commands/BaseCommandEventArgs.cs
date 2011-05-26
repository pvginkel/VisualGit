using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell.Interop;

using VisualGit.Selection;

namespace VisualGit.Commands
{
    public class BaseCommandEventArgs : EventArgs
    {
        readonly VisualGitCommand _command;
        readonly VisualGitContext _context;
        CommandMapItem _mapItem;

        public BaseCommandEventArgs(VisualGitCommand command, VisualGitContext context)
        {
            _command = command;
            _context = context;
        }

        public VisualGitCommand Command
        {
            [DebuggerStepThrough]
            get { return _command; }
        }

        public VisualGitContext Context
        {
            [DebuggerStepThrough]
            get { return _context; }
        }

        ISelectionContext _selection;
        IVisualGitCommandStates _state;
        /// <summary>
        /// Gets the Visual Studio selection
        /// </summary>
        /// <value>The selection.</value>        
        public ISelectionContext Selection
        {
            [DebuggerStepThrough]
            get { return _selection ?? (_selection = GetService<ISelectionContext>()); }
        }

        /// <summary>
        /// Gets the command states.
        /// </summary>
        /// <value>The state.</value>
        public IVisualGitCommandStates State
        {
            [DebuggerStepThrough]
            get { return _state ?? (_state = GetService<IVisualGitCommandStates>()); }
        }

        [GuidAttribute("3C536122-57B1-46DE-AB34-ACC524140093")]
        sealed class SVsExtensibility
        {
        }
        /// <summary>
        /// Gets a value indicating whether this instance is in automation.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is in automation; otherwise, <c>false</c>.
        /// </value>
        public bool IsInAutomation
        {
            get
            {
                IVsExtensibility3 extensibility = GetService<IVsExtensibility3>(typeof(SVsExtensibility));

                if (extensibility != null)
                {
                    int inAutomation;

                    if (extensibility.IsInAutomationFunction(out inAutomation) == 0)
                        return inAutomation != 0;
                }

                return false;
            }
        }

        /// <summary>
        /// Gets the service.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [DebuggerStepThrough]
        public T GetService<T>()
            where T : class
        {
            return Context.GetService<T>();
        }

        /// <summary>
        /// Gets the service.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serviceType">Type of the service.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public T GetService<T>(Type serviceType)
            where T : class
        {
            return Context.GetService<T>(serviceType);
        }

        internal void Prepare(CommandMapItem item)
        {
            _mapItem = item;
        }
    }
}
