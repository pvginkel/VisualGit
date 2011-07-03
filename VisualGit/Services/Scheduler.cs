// VisualGit\Services\Scheduler.cs
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
using System.Text;
using System.Timers;
using Microsoft.VisualStudio.Shell.Interop;
using VisualGit.Commands;

namespace VisualGit.Services
{
    [GlobalService(typeof(IVisualGitScheduler))]
    sealed class VisualGitScheduler : VisualGitService, IVisualGitScheduler
    {
		struct ActionItem
		{
			public readonly int Id;
			public readonly VisualGitAction Action;

			public ActionItem(int id, VisualGitAction action)
			{
				Id = id;
				Action = action;
			}
		}

        readonly Timer _timer;
        IVisualGitCommandService _commands;
        readonly SortedList<DateTime, ActionItem> _actions = new SortedList<DateTime, ActionItem>();
        Guid _grp = VisualGitId.CommandSetGuid;
		int _nextActionId;

        public VisualGitScheduler(IVisualGitServiceProvider context)
            : base(context)
        {
            _timer = new Timer();
            _timer.Enabled = false;
            _timer.Elapsed += new ElapsedEventHandler(OnTimerElapsed);
        }

        IVisualGitCommandService Commands
        {
            get { return _commands ?? (_commands = GetService<IVisualGitCommandService>()); }
        }

        void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                _timer.Enabled = false;
                DateTime now = DateTime.Now;
                while (true)
                {
                    ActionItem action;
                    lock (_actions)
                    {
                        if (_actions.Count == 0)
                            break;
                        DateTime d = _actions.Keys[0];

                        if (d > now)
                            break;

                        action = _actions.Values[0];
                        _actions.RemoveAt(0);
                    }

                    action.Action();
                }
            }
            catch
            { }
            finally
            {
                Reschedule();
            }
        }

        void Reschedule()
        {
            lock (_actions)
            {
                if (_actions.Count == 0)
                    return;

                double tLeft = (_actions.Keys[0] - DateTime.Now).TotalMilliseconds;
                if (tLeft < 0.0)
                    tLeft = 10.0;

                _timer.Interval = tLeft;
                _timer.Enabled = true;
            }
        }


        #region IVisualGitScheduler Members

        public int ScheduleAt(DateTime time, VisualGitCommand command)
        {
            return ScheduleAt(time, CreateHandler(command));
        }

        public int ScheduleAt(DateTime time, VisualGitAction action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

			if (time.Kind == DateTimeKind.Utc)
				time = time.ToLocalTime();

            lock (_actions)
            {
                while (_actions.ContainsKey(time))
                    time = time.Add(TimeSpan.FromMilliseconds(1));

				ActionItem ai = new ActionItem(unchecked(++_nextActionId), action);

                _actions.Add(time, ai);

                Reschedule();
				return ai.Id;
            }
        }

        VisualGitAction CreateHandler(VisualGitCommand command)
        {
            return delegate
            {
                if (Commands != null)
                    Commands.PostExecCommand(command);
            };
        }

        public int Schedule(TimeSpan timeSpan, VisualGitCommand command)
        {
            return ScheduleAt(DateTime.Now + timeSpan, CreateHandler(command));
        }

        public int Schedule(TimeSpan timeSpan, VisualGitAction action)
        {
            return ScheduleAt(DateTime.Now + timeSpan, action);
        }

        #endregion

		#region IVisualGitScheduler Members


		public bool RemoveTask(int taskId)
		{
			lock (_actions)
			{
				foreach (KeyValuePair<DateTime, ActionItem> i in _actions)
				{
					if (i.Value.Id == taskId)
					{
						_actions.Remove(i.Key);
						return true;
					}
				}
			}
				return false;
		}

		#endregion
	}
}
