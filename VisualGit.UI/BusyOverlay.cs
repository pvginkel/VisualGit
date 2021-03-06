// VisualGit.UI\BusyOverlay.cs
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
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing;
using VisualGit.UI.Properties;
using VisualGit.Scc.UI;

namespace VisualGit.UI
{
    public class BusyOverlay : Component
    {
        Control _parent;
        PictureBox _pb;
        AnchorStyles _anchor;
        ScrollBars _scrollBarsVisible = ScrollBars.None;
        int _show;

        public BusyOverlay()
        {
            _anchor = AnchorStyles.None;
        }

        public BusyOverlay(Control parent, AnchorStyles anchor)
            : this()
        {
            Parent = parent;
            Anchor = anchor;
        }

        [DefaultValue(DockStyle.None)]
        public AnchorStyles Anchor
        {
            get { return _anchor; }
            set { _anchor = value; UpdatePosition(); }
        }

        [DefaultValue(ScrollBars.None)]
        public ScrollBars ScrollBarsVisible
        {
            get { return _scrollBarsVisible; }
            set { _scrollBarsVisible = value; UpdatePosition(); }
        }

        Control _top;

        public Control Parent
        {
            get { return _parent; }
            set 
            {
                if (_parent == value)
                    return;

                if (_parent != null)
                {
                    _parent.SizeChanged -= new EventHandler(OnParentChanged);
                    _parent.VisibleChanged -= new EventHandler(OnParentChanged);
                    _parent.HandleDestroyed -= new EventHandler(OnParentHandleDestroyed);
                    _parent = value;
                }

                if (_top != null)
                {
                    IVisualGitToolWindowControl tw = _top as IVisualGitToolWindowControl;
                    if (tw != null)
                        tw.ToolWindowVisibileChanged -= new EventHandler(OnTopVisibleChanged);
                    else
                        _top.VisibleChanged -= new EventHandler(OnToolWindowVisibleChanged);
                }

                _parent = value;
                _top = null;
                if (_pb != null)
                {
                    _pb.Parent = value;
                }

                if(_parent != null)
                {
                    Control p = _parent;

                    while (p.Parent != null)
                        p = p.Parent;

                    if (p != value)
                        _top = p;
                }
                    
                UpdatePosition();

                if (_parent != null)
                {
                    _parent.HandleDestroyed += new EventHandler(OnParentHandleDestroyed);
                    _parent.VisibleChanged += new EventHandler(OnParentChanged);
                    _parent.SizeChanged += new EventHandler(OnParentChanged);
                }

                if (_top != null)
                {
                    IVisualGitToolWindowControl tw = _top as IVisualGitToolWindowControl;
                    if (tw != null)
                        tw.ToolWindowVisibileChanged += new EventHandler(OnToolWindowVisibleChanged);
                    else
                        _top.VisibleChanged += new EventHandler(OnTopVisibleChanged);
                }
            }
        }

        void OnTopVisibleChanged(object sender, EventArgs e)
        {
            if (_top != null && !_top.Visible)
            {
                if (_pb != null)
                {
                    _pb.Dispose();
                    _pb = null;
                }
            }
        }

        void OnToolWindowVisibleChanged(object sender, EventArgs e)
        {
            IVisualGitToolWindowControl tw = _top as IVisualGitToolWindowControl;
            if (tw == null)
                return;

            if (_pb != null && !tw.ToolWindowVisible)
            {
                _pb.Dispose();
                _pb = null;
            }
        }

        void OnParentHandleDestroyed(object sender, EventArgs e)
        {
            if (_pb != null)
            {
                _pb.Dispose();
                _pb = null;
            }
        }

        void OnParentChanged(object sender, EventArgs e)
        {
            UpdatePosition();
        }

        private void UpdatePosition()
        {
            if (Parent == null || _pb == null)
                return;

            Size parentSize = Parent.Size;
            Point p = new Point();

            if ((parentSize.Width < _pb.Width) || (parentSize.Height < _pb.Height))
                p = new Point(1, 1);
            else
            {
                Size ps = Parent.ClientSize;

                switch (Anchor & (AnchorStyles.Left | AnchorStyles.Right))
                {
                    case AnchorStyles.Left:
                        p.X = 1;
                        break;
                    case AnchorStyles.Right:
                        p.X = ps.Width - _pb.Width - 2;

                        if (_scrollBarsVisible.HasFlag(ScrollBars.Vertical))
                            p.X -= SystemInformation.VerticalScrollBarWidth;
                        break;
                    default:
                        p.X = (ps.Width - _pb.Width) / 2;
                        break;
                }

                switch (Anchor & (AnchorStyles.Top | AnchorStyles.Bottom))
                {
                    case AnchorStyles.Top:
                        p.Y = 1;
                        break;
                    case AnchorStyles.Bottom:
                        p.Y = ps.Height - _pb.Height - 2;

                        if (_scrollBarsVisible.HasFlag(ScrollBars.Horizontal))
                            p.Y -= SystemInformation.HorizontalScrollBarHeight;
                        break;
                    default:
                        p.X = (ps.Height - _pb.Height) / 2;
                        break;
                }
            }

            _pb.Location = p;
        }

        public void Show()
        {
            if (_show >= 0 && _pb == null && Parent != null && !Parent.IsDisposed && Parent.Visible)
            {
                Bitmap img = (Bitmap)Resources.Busy;

                Size sz = img.Size;

                _pb = new PictureBox();
                _pb.Size = img.Size;
                _pb.BackColor = Parent.BackColor;
                _pb.Image = img;
                _parent.Controls.Add(_pb);

                UpdatePosition();
                _pb.Show();
            }
            _show++;
        }

        public void Hide()
        {
            if(_show > 0)
            {
                _show--;

                if(_show == 0 && _pb != null)
                {
                    _pb.Dispose();
                    _pb = null;
                }                    
            }
        }
    }
}
