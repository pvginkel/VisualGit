// VisualGit.VS\Selection\VisualGitCommandService.cs
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
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;

using VisualGit.Commands;
using VisualGit.UI;
using VisualGit.VS;

namespace VisualGit.Services
{
    [GlobalService(typeof(IVisualGitCommandService))]
    sealed class VisualGitCommandService : VisualGitService, IVisualGitCommandService, IVisualGitIdleProcessor
    {
        const int _tickCount = (int)VisualGitCommand.TickLast - (int)VisualGitCommand.TickFirst;
        readonly List<int> _delayedCommands = new List<int>();
        readonly int[] _ticks = new int[_tickCount];
        readonly Queue<VisualGitAction> _idleActions = new Queue<VisualGitAction>();

        public VisualGitCommandService(IVisualGitServiceProvider context)
            : base(context)
        {
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            GetService<IVisualGitPackage>().RegisterIdleProcessor(this);
        }

        IVsUIShell _uiShell;
        IVsUIShell UIShell
        {
            get { return _uiShell ?? (_uiShell = GetService<IVsUIShell>(typeof(SVsUIShell))); }
        }

        IOleCommandTarget _commandDispatcher;
        IOleCommandTarget CommandDispatcher
        {
            [DebuggerStepThrough]
            get { return _commandDispatcher ?? (_commandDispatcher = GetService<IOleCommandTarget>(typeof(SUIHostCommandDispatcher))); }
        }

        VisualGitContext _visualGitContext;
        VisualGitContext VisualGitContext
        {
            [DebuggerStepThrough]
            get { return _visualGitContext ?? (_visualGitContext = GetService<VisualGitContext>()); }
        }

        public CommandResult ExecCommand(VisualGitCommand command)
        {
            // The commandhandler in the package always checks enabled; no need to do it here
            return ExecCommand(command, false);
        }

        public CommandResult ExecCommand(VisualGitCommand command, bool verifyEnabled)
        {
            // The commandhandler in the package always checks enabled; no need to do it here
            return ExecCommand(new CommandID(VisualGitId.CommandSetGuid, (int)command), verifyEnabled);
        }

        public CommandResult ExecCommand(System.ComponentModel.Design.CommandID command)
        {
            return ExecCommand(command, true);
        }

        public CommandResult ExecCommand(System.ComponentModel.Design.CommandID command, bool verifyEnabled)
        {
            return ExecCommand(command, verifyEnabled, null);
        }

        public CommandResult ExecCommand(System.ComponentModel.Design.CommandID command, bool verifyEnabled, object argument)
        {
            if (command == null)
                throw new ArgumentNullException("command");

            // TODO: Assert that we are in the UI thread

            IOleCommandTarget dispatcher = CommandDispatcher;

            if (dispatcher == null)
                return new CommandResult(false);

            Guid g = command.Guid;

            if (verifyEnabled)
            {
                OLECMD[] cmd = new OLECMD[1];
                cmd[0].cmdID = unchecked((uint)command.ID);

                if (VSConstants.S_OK != dispatcher.QueryStatus(ref g, 1, cmd, IntPtr.Zero))
                    return new CommandResult(false);

                OLECMDF flags = (OLECMDF)cmd[0].cmdf;

                if ((flags & OLECMDF.OLECMDF_SUPPORTED) == (OLECMDF)0)
                    return new CommandResult(false); // Not supported

                if ((flags & OLECMDF.OLECMDF_ENABLED) == (OLECMDF)0)
                    return new CommandResult(false); // Not enabled
            }

            IntPtr vIn = IntPtr.Zero;
            IntPtr vOut = IntPtr.Zero;
            try
            {
                vOut = Marshal.AllocCoTaskMem(128);
                NativeMethods.VariantInit(vOut);

                if (argument != null)
                {
                    vIn = Marshal.AllocCoTaskMem(128);
                    Marshal.GetNativeVariantForObject(argument, vIn);
                }

                bool ok = ErrorHandler.Succeeded(dispatcher.Exec(ref g,
                    unchecked((uint)command.ID), (uint)OLECMDEXECOPT.OLECMDEXECOPT_DODEFAULT, IntPtr.Zero, IntPtr.Zero));

                return new CommandResult(ok, Marshal.GetObjectForNativeVariant(vOut));
            }
            finally
            {
                if (vIn != IntPtr.Zero)
                {
                    NativeMethods.VariantClear(vIn);
                    Marshal.FreeCoTaskMem(vIn);
                }
                if (vOut != IntPtr.Zero)
                {
                    NativeMethods.VariantClear(vOut);
                    Marshal.FreeCoTaskMem(vOut);
                }
            }
        }

        static class NativeMethods
        {
            [DllImport("oleaut32.dll")]
            public static extern int VariantClear(IntPtr v);

            [DllImport("oleaut32.dll")]
            public static extern int VariantInit(IntPtr v);
        }

        bool IsTickCommand(VisualGitCommand c)
        {
            if (c < VisualGitCommand.TickFirst || c >= VisualGitCommand.TickLast)
                return false;

            return true;
        }
        /// <summary>
        /// Posts the tick command.
        /// </summary>
        /// <param name="tick">if set to <c>true</c> [tick].</param>
        /// <param name="command">The command.</param>
        /// <returns></returns>
        public bool PostTickCommand(ref bool tick, VisualGitCommand command)
        {
            if (IsTickCommand(command))
                _ticks[command - VisualGitCommand.TickFirst] = 1;

            if (tick)
                return false;

            tick = true;
            bool ok = false;
            try
            {
                return ok = PostExecCommand(command);
            }
            finally
            {
                if (!ok)
                    tick = false;
            }
        }

        public void SafePostTickCommand(ref bool tick, VisualGitCommand command)
        {
            if (tick)
                return;

            tick = true;
            lock (_delayTasks)
            {
                _delayedCommands.Add((int)command);
            }
        }

        public bool PostExecCommand(VisualGitCommand command)
        {
            return PostExecCommand(command, null, CommandPrompt.DoDefault);
        }

        public bool PostExecCommand(VisualGitCommand command, object args)
        {
            return PostExecCommand(command, args, CommandPrompt.DoDefault);
        }

        public bool PostExecCommand(VisualGitCommand command, object args, CommandPrompt prompt)
        {
            return PostExecCommand(new CommandID(VisualGitId.CommandSetGuid, (int)command), args, prompt);
        }

        public bool PostExecCommand(System.ComponentModel.Design.CommandID command)
        {
            if (command == null)
                throw new ArgumentNullException("command");

            return PostExecCommand(command, null);
        }

        public bool PostExecCommand(System.ComponentModel.Design.CommandID command, object args)
        {
            return PostExecCommand(command, args, CommandPrompt.DoDefault);
        }

        bool _delayed;
        readonly List<VisualGitAction> _delayTasks = new List<VisualGitAction>();

        public bool PostExecCommand(CommandID command, object args, CommandPrompt prompt)
        {
            if (command == null)
                throw new ArgumentNullException("command");

            lock (_delayTasks)
            {
                if (_delayed)
                {
                    _delayTasks.Add(
                        delegate
                        {
                            PerformPost(command, prompt, args);
                        });

                    return true;
                }
                else if (PerformPost(command, prompt, args))
                {
                    for (int i = 0; i < _delayedCommands.Count; i++)
                    {
                        if (!PerformPost(new CommandID(VisualGitId.CommandSetGuid, _delayedCommands[i]), CommandPrompt.DoDefault, null))
                        {
                            _delayedCommands.RemoveRange(0, i);
                            return true;
                        }
                    }
                    _delayedCommands.Clear();
                    return true;
                }
                else
                    return false;
            }
        }

        bool PerformPost(CommandID command, CommandPrompt prompt, object args)
        {
            IVsUIShell shell = UIShell;

            if (shell != null)
            {
                uint flags;
                switch (prompt)
                {
                    case CommandPrompt.Always:
                        flags = (uint)OLECMDEXECOPT.OLECMDEXECOPT_PROMPTUSER;
                        break;
                    case CommandPrompt.Never:
                        flags = (uint)OLECMDEXECOPT.OLECMDEXECOPT_DONTPROMPTUSER;
                        break;
                    default:
                        flags = (uint)OLECMDEXECOPT.OLECMDEXECOPT_DODEFAULT;
                        break;
                }

                Guid set = command.Guid;
                object a = args;

                return VSConstants.S_OK == shell.PostExecCommand(ref set,
                        unchecked((uint)command.ID), flags, ref a);
            }

            return false;
        }

        SynchronizationContext _syncContext;
        SynchronizationContext SyncContext
        {
            [DebuggerStepThrough]
            get { return _syncContext ?? (_syncContext = GetService<SynchronizationContext>()); }
        }

        readonly List<DelayDelegateCheck> _checks = new List<DelayDelegateCheck>();
        public void DelayPostCommands(DelayDelegateCheck check)
        {
            if (check == null)
                throw new ArgumentNullException("check");

            _checks.Add(check);
            if (!_delayed)
            {
                _delayed = true;
                SyncContext.Post(TryRelease, null);
            }
        }

        void TryRelease(object v)
        {
            if (_delayed)
                TryReleaseDelayed();

            if (_delayed)
                PostCheck();
        }

        void PostCheck()
        {
            VisualGitAction pt = delegate()
            {
                Thread.Sleep(50);
                SyncContext.Post(TryRelease, null);
            };

            pt.BeginInvoke(null, null);
        }

        void TryReleaseDelayed()
        {
            for (int i = 0; i < _checks.Count; i++)
            {
                if (!_checks[i]())
                    _checks.RemoveAt(i--);
            }

            lock (_delayTasks)
            {
                if (_checks.Count == 0)
                {
                    try
                    {
                        foreach (VisualGitAction dpc in _delayTasks)
                            dpc();
                    }
                    finally
                    {
                        _delayTasks.Clear();
                        _delayed = false;
                    }
                }
            }
        }

        public CommandResult DirectlyExecCommand(VisualGitCommand command)
        {
            return DirectlyExecCommand(command, null, CommandPrompt.DoDefault);
        }

        public CommandResult DirectlyExecCommand(VisualGitCommand command, object args)
        {
            return DirectlyExecCommand(command, args, CommandPrompt.DoDefault);
        }

        public CommandResult DirectlyExecCommand(VisualGitCommand command, object args, CommandPrompt prompt)
        {
            // TODO: Assert that we are in the UI thread

            CommandMapper mapper = GetService<CommandMapper>();

            if (mapper == null)
                return new CommandResult(false, null);

            CommandEventArgs e = new CommandEventArgs(command, VisualGitContext, args, prompt == CommandPrompt.Always, prompt == CommandPrompt.Never);
            bool ok = mapper.Execute(command, e);

            return new CommandResult(ok, e.Result);
        }

        /// <summary>
        /// Updates the command UI.
        /// </summary>
        /// <param name="performImmediately">if set to <c>true</c> [perform immediately].</param>
        public void UpdateCommandUI(bool performImmediately)
        {
            IVsUIShell shell = UIShell;

            if (shell != null)
                shell.UpdateCommandUI(performImmediately ? 1 : 0);
        }


        public void ShowContextMenu(VisualGitCommandMenu menu, int x, int y)
        {
            IMenuCommandService mcs = GetService<IMenuCommandService>();

            IVsUIShell shell = GetService<IVsUIShell>();
            if (mcs != null)
            {
                try
                {
                    mcs.ShowContextMenu(new CommandID(VisualGitId.CommandSetGuid, (int)menu), x, y);
                }
                catch (COMException)
                {
                    /* Menu is not declared correctly (no items) */
                }
            }
        }

        public void ShowContextMenu(VisualGitCommandMenu menu, System.Drawing.Point location)
        {
            ShowContextMenu(menu, location.X, location.Y);
        }

        public void TockCommand(VisualGitCommand visualGitCommand)
        {
            if (IsTickCommand(visualGitCommand))
                _ticks[visualGitCommand - VisualGitCommand.TickFirst] = 0;
        }

        public void OnIdle(VisualGitIdleArgs e)
        {
            if (_delayed)
                TryReleaseDelayed();
            else
                if (e.Periodic)
                {
                    for (int i = 0; i < _tickCount; i++)
                    {
                        if (_ticks[i] != 0)
                        {
                            Debug.WriteLine(string.Format("VisualGit: Tocking {0}", VisualGitCommand.TickFirst + i));
                            PostExecCommand(VisualGitCommand.TickFirst + i);
                        }
                    }
                }

            if (e.NonPeriodic)
            {                
                VisualGitAction action;

                while (null != (action = GetIdleAction()))
                {
                    try
                    {
                        if (action != null)
                            action();
                    }
                    catch (Exception ex)
                    {
                        IVisualGitErrorHandler handler = GetService<IVisualGitErrorHandler>();
                        if (handler != null && handler.IsEnabled(ex))
                            handler.OnError(ex);
                        else
                            throw;
                    }

                    if (!e.ContinueIdle())
                        break;
                }
            }
        }

        VisualGitAction GetIdleAction()
        {
            lock (_idleActions)
            {
                if (_idleActions.Count > 0)
                    return _idleActions.Dequeue();

                return null;
            }
        }                

        public void PostIdleCommand(VisualGitCommand command)
        {
            PostIdleCommand(command, null);
        }

        public void PostIdleCommand(VisualGitCommand command, object args)
        {
            PostIdleAction(
                delegate()
                {
                    DirectlyExecCommand(command, args);
                });
        }

        public void PostIdleAction(VisualGitAction action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            lock (_idleActions)
            {
                _idleActions.Enqueue(action);
            }
        }
    }
}
