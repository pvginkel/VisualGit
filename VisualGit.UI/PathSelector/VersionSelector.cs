// VisualGit.UI\PathSelector\VersionSelector.cs
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
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

using System.Text.RegularExpressions;
using VisualGit.Scc.UI;
using VisualGit.Scc;
using System.Collections.Generic;
using SharpGit;

namespace VisualGit.UI.PathSelector
{
    /// <summary>
    /// A control that allows the user to pick a revision.
    /// </summary>
    public partial class VersionSelector : System.Windows.Forms.UserControl
    {
        public event EventHandler Changed;

        public VersionSelector()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();
        }

        /// <summary>
        /// Whether the control has a valid revision.
        /// </summary>
        public bool Valid
        {
            get
            {
                /*if ( this.revisionTypeBox.SelectedItem == null )
                    return NUMBER.IsMatch( this.revisionTypeBox.Text ); 
                else*/
                return true;
            }
        }

        IVisualGitServiceProvider _context;
        /// <summary>
        /// Gets or sets the context.
        /// </summary>
        /// <value>The context.</value>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IVisualGitServiceProvider Context
        {
            get { return _context ?? ((Context = this.ParentForm as IVisualGitServiceProvider)); }
            set
            {
                bool set = (_context == null);
                _context = value;

                if (value != null && set && _newValue != null)
                {
                    EnsureList();
                    Revision = _newValue;
                }
            }
        }

        IVisualGitRevisionResolver _resolver;
        /// <summary>
        /// Gets the revision resolver.
        /// </summary>
        /// <value>The revision resolver.</value>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IVisualGitRevisionResolver RevisionResolver
        {
            get
            {
                if (_resolver == null && Context != null)
                    return _resolver = Context.GetService<IVisualGitRevisionResolver>();

                return _resolver;
            }
        }

        VisualGitRevisionType _currentRevType;
        List<VisualGitRevisionType> _revTypes;
        GitRevision _newValue;
        /// <summary>
        /// The revision selected by the user.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public GitRevision Revision
        {
            get
            {
                if (_currentRevType != null)
                    return _currentRevType.CurrentValue;
                else
                    return null;
            }
            set
            {
                _newValue = null;
                if (value == null || RevisionResolver == null)
                {
                    _newValue = value;
                    SetRevision(null);
                }
                else
                    SetRevision(RevisionResolver.Resolve(GitOrigin, value));
            }
        }

        GitOrigin _origin;
        public GitOrigin GitOrigin
        {
            get { return _origin; }
            set { _origin = value; EnsureList(); }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            EnsureList();
        }

        private void EnsureList()
        {
            if (RevisionResolver == null || GitOrigin == null)
                return;

            foreach (VisualGitRevisionType ri in new ArrayList(typeCombo.Items))
            {
                if (!ri.IsValidOn(GitOrigin))
                {
                    if (ri == _currentRevType)
                    {
                        _newValue = ri.CurrentValue;
                        _currentRevType = null;
                    }
                    typeCombo.Items.Remove(ri);
                }
            }

            if (_revTypes != null)
                foreach (VisualGitRevisionType rt in _revTypes)
                {
                    if (rt.IsValidOn(GitOrigin) && !typeCombo.Items.Contains(rt))
                        typeCombo.Items.Add(rt);
                }
            else
                _revTypes = new List<VisualGitRevisionType>();

            foreach (VisualGitRevisionType rt in RevisionResolver.GetRevisionTypes(GitOrigin))
            {
                if (_revTypes.Contains(rt))
                    continue;

                _revTypes.Add(rt);
                typeCombo.Items.Add(rt);
            }

            if (_currentRevType == null && _newValue != null && _newValue != GitRevision.None)
            {
                VisualGitRevisionType rt = RevisionResolver.Resolve(GitOrigin, _newValue);

                if (rt != null && !rt.IsValidOn(GitOrigin))
                {
                    _newValue = GitOrigin.Target.Revision;
                    if (_newValue == null || _newValue == GitRevision.None)
                        _newValue = GitRevision.Base;

                    rt = RevisionResolver.Resolve(GitOrigin, _newValue);
                }

                SetRevision(rt);
            }

            if (_currentRevType != typeCombo.SelectedItem)
                typeCombo.SelectedItem = _currentRevType;
        }

        void SetRevision(VisualGitRevisionType rev)
        {
            if (rev == null && _currentRevType == null)
                return;
            else if (rev != null && _currentRevType != null && rev.Equals(_currentRevType))
                return;

            if (_revTypes == null)
                _revTypes = new List<VisualGitRevisionType>();

            if (_currentRevType != null && !_revTypes.Contains(_currentRevType))
                _revTypes.Add(_currentRevType);

            if (rev != null && !_revTypes.Contains(rev))
                _revTypes.Add(rev);

            _currentRevType = rev;

            EnsureList();

            foreach (Control c in versionTypePanel.Controls)
            {
                c.Enabled = c.Visible = false;
            }

            if (rev.HasUI)
            {
                if (rev.CurrentControl == null)
                    rev.InstantiateUIIn(versionTypePanel, EventArgs.Empty);

                rev.CurrentControl.Visible = rev.CurrentControl.Enabled = true;
            }

            typeCombo.SelectedItem = _currentRevType;
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        private void typeCombo_SelectedValueChanged(object sender, EventArgs e)
        {
            VisualGitRevisionType rev = typeCombo.SelectedItem as VisualGitRevisionType;

            if (rev != null)
                SetRevision(rev);

            OnChanged(EventArgs.Empty);
        }

        protected virtual void OnChanged(EventArgs e)
        {
            if (Changed != null)
                Changed(this, e);
        }
    }
}
