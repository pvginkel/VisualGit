using System;
using System.Collections.Generic;
using System.Text;

namespace VisualGit.Commands
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public sealed class CommandAttribute : Attribute
    {
        readonly VisualGitCommand _command;
        readonly VisualGitCommandContext _context;
        VisualGitCommand _lastCommand;
        CommandTarget _target;

        /// <summary>
        /// Defines the class or function as a handler of the specified <see cref="VisualGitCommand"/>
        /// </summary>
        /// <param name="command">The command.</param>
        public CommandAttribute(VisualGitCommand command)
        {
            _command = command;
        }

        public CommandAttribute(VisualGitCommand command, VisualGitCommandContext context)
            : this(command)
        {
            _context = context;
        }


        /// <summary>
        /// Gets the command.
        /// </summary>
        /// <value>The command.</value>
        public VisualGitCommand Command
        {
            get { return _command; }
        }

        public VisualGitCommandContext Context
        {
            get { return _context; }
        }

        /// <summary>
        /// Gets or sets the last command.
        /// </summary>
        /// <value>The last command.</value>
        public VisualGitCommand LastCommand
        {
            get { return _lastCommand; }
            set { _lastCommand = value; }
        }

        /// <summary>
        /// Gets or sets the command target.
        /// </summary>
        /// <value>The command target.</value>
        public CommandTarget CommandTarget
        {
            get { return _target; }
            set { _target = value; }
        }

        bool _showWhenDisabled;
        /// <summary>
        /// Gets or sets a value indicating whether [hide when disabled].
        /// </summary>
        /// <value><c>true</c> if [hide when disabled]; otherwise, <c>false</c>.</value>
        public bool HideWhenDisabled
        {
            get { return !_showWhenDisabled; }
            set { _showWhenDisabled = !value; }
        }

        bool _alwaysAvailable;

        /// <summary>
        /// Gets or sets a boolean indicating whether this command might be enabled if VisualGit is not the current SCC provider
        /// </summary>
        /// <remarks>If set to false the command is disabled (and when <see cref="HideWhenDisabled"/> also hidden)</remarks>
        public bool AlwaysAvailable
        {
            get { return _alwaysAvailable; }
            set { _alwaysAvailable = value; }
        }

        string _argumentDefinition;
        /// <summary>
        /// Gets or sets the argument definition string
        /// </summary>
        /// <remarks>
        ///     ‘~’ - No autocompletion for this parameter.
        ///     ‘$’ - This parameter is the rest of the input line (no autocompletion).
        ///     ‘a’ – An alias.
        ///     ‘c’ – The canonical name of a command.
        ///     ‘d’ – A filename from the file system.
        ///     ‘p’ – The filename from a project in the current solution.
        ///     ‘u’ – A URL.
        ///     ‘|’ – Combines two parameter types for the same parameter.
        ///     ‘*’ – Indicates zero or more occurrences of the previous parameter.
        ///     
        /// Some examples:
        ///     "d|p *" filenames or projects
        ///     
        /// “p p” – Command accepts two filenames 
        /// “u d” – Command accepts one URL and one filename argument.
        /// “u *” – Command accepts zero or more URL arguments.
        /// </remarks>
        public string ArgumentDefinition
        {
            get { return _argumentDefinition; }
            set { _argumentDefinition = value; }
        }

        internal IEnumerable<VisualGitCommand> GetAllCommands()
        {
            if (LastCommand == VisualGitCommand.None)
                yield return Command;
            else if (LastCommand < Command || ((int)LastCommand - (int)Command) > 256)
                throw new InvalidOperationException("Command range larger then 256 on range starting with" + Command.ToString());
            else
                for (VisualGitCommand c = Command; c <= LastCommand; c++)
                {
                    yield return c;
                }
        }
    }
}
