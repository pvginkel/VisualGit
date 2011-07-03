// VisualGit.Services\UI\VisualGitToolWindowControl.cs
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
using System.Windows.Forms;
using VisualGit.Scc.UI;
using System.ComponentModel;

namespace VisualGit.UI
{
    public class VisualGitToolWindowControl : UserControl, IVisualGitToolWindowControl, IVisualGitCommandHookAccessor
    {
        IVisualGitToolWindowHost _host;
        protected VisualGitToolWindowControl()
        {
        }

        public override string Text
        {
            get
            {
                if (_host != null)
                    return _host.Title;
                else
                    return base.Text;
            }
            set
            {
                if (_host != null)
                    _host.Title = value;

                base.Text = value;
            }
        }

        /// <summary>
        /// Gets the UI site.
        /// </summary>
        /// <value>The UI site.</value>
        [CLSCompliant(false), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IVisualGitToolWindowHost ToolWindowHost
        {
            get { return _host; }
            set
            {
                if (_host != value)
                {
                    _host = value;
                    OnContextChanged(EventArgs.Empty);
                }
            }
        }

        protected virtual void OnContextChanged(EventArgs e)
        {
        }

        /// <summary>
        /// Gets the context.
        /// </summary>
        /// <value>The context.</value>
        [Browsable(false)]
        public IVisualGitServiceProvider Context
        {
            get { return _host; }
        }

        /// <summary>
        /// Returns an object that represents a service provided by the <see cref="T:System.ComponentModel.Component"/> or by its <see cref="T:System.ComponentModel.Container"/>.
        /// </summary>
        /// <param name="service">A service provided by the <see cref="T:System.ComponentModel.Component"/>.</param>
        /// <returns>
        /// An <see cref="T:System.Object"/> that represents a service provided by the <see cref="T:System.ComponentModel.Component"/>, or null if the <see cref="T:System.ComponentModel.Component"/> does not provide the specified service.
        /// </returns>
        protected override object GetService(Type service)
        {
            object r;
            if (Context != null)
            {
                r = Context.GetService(service);

                if (r != null)
                    return r;
            }

            return base.GetService(service);
        }

        #region IVisualGitToolWindowControl Members

        /// <summary>
        /// Called when the frame is created
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnFrameCreated(EventArgs e)
        {
        }

        void IVisualGitToolWindowControl.OnFrameCreated(EventArgs e)
        {
            OnFrameCreated(e);
        }

        /// <summary>
        /// Called when the frame is closed
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void OnFrameClose(EventArgs e)
        {
        }

        void IVisualGitToolWindowControl.OnFrameClose(EventArgs e)
        {
            OnFrameClose(e);
        }

        /// <summary>
        /// Called when the dockstate is changing
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnFrameDockableChanged(FrameEventArgs e)
        {
        }

        void IVisualGitToolWindowControl.OnFrameDockableChanged(FrameEventArgs e)
        {
            OnFrameDockableChanged(e);
        }

        void IVisualGitToolWindowControl.OnFrameMove(FrameEventArgs e)
        {
            OnFrameMove(e);
        }

        protected virtual void OnFrameMove(FrameEventArgs e)
        {
        }

        /// <summary>
        /// Occurs when the frame show state changed
        /// </summary>
        public event EventHandler<FrameEventArgs> FrameShow;

        protected virtual void OnFrameShow(FrameEventArgs e)
        {
            if (FrameShow != null)
                FrameShow(this, e);
        }

        void IVisualGitToolWindowControl.OnFrameShow(FrameEventArgs e)
        {
            OnFrameShow(e);

            if (ToolWindowVisibileChanged != null)
                ToolWindowVisibileChanged(this, e);
        }

        /// <summary>
        /// Occurs when the frame show size changed
        /// </summary>
        public event EventHandler<FrameEventArgs> FrameSize;

        protected virtual void OnFrameSize(FrameEventArgs e)
        {
            if (FrameSize != null)
                FrameSize(this, e);
        }


        void IVisualGitToolWindowControl.OnFrameSize(FrameEventArgs e)
        {
            OnFrameSize(e);
        }

        #endregion

        #region IVisualGitCommandHookAccessor Members

        VisualGitCommandHook _hook;
        VisualGitCommandHook IVisualGitCommandHookAccessor.CommandHook
        {
            get { return _hook; }
            set { _hook = value; }
        }

        #endregion

        public event EventHandler ToolWindowVisibileChanged;

        [Browsable(false)]
        public bool ToolWindowVisible
        {
            get
            {
                if (!IsHandleCreated || ToolWindowHost == null)
                    return false;

                return ToolWindowHost.IsOnScreen;
            }
        }
    }
}
