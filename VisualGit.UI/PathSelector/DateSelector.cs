// VisualGit.UI\PathSelector\DateSelector.cs
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
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace VisualGit.UI.PathSelector
{
    partial class DateSelector : UserControl
    {
        public DateSelector()
        {
            InitializeComponent();
        }

        DateTime _date;

        public DateTime Value
        {
            get { return _date = (datePicker.Value.Date + timePicker.Value.TimeOfDay); }
            set
            {
                _date = value;
                datePicker.Value = _date.Date;
                timePicker.Value = DateTime.Today + _date.TimeOfDay;
            }
        }

        public event EventHandler Changed;

        private void datePicker_ValueChanged(object sender, EventArgs e)
        {
            OnChanged(e);
        }

        private void timePicker_ValueChanged(object sender, EventArgs e)
        {
            OnChanged(e);
        }

        protected virtual void OnChanged(EventArgs e)
        {
            if (Changed != null)
                Changed(this, e);
        }
    }
}
