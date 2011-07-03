// VisualGit.Scc\SettingsMap\SccCategorySettings.cs
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

namespace VisualGit.Scc.SettingMap
{
    class SccCategorySettings
    {
        readonly SortedList<string, string> _props = new SortedList<string, string>(StringComparer.OrdinalIgnoreCase);
        readonly VisualGitSccSettingStorage _store;
        readonly string _id;
        string _name;

        public SccCategorySettings(VisualGitSccSettingStorage store, string id)
        {
            if (store == null)
                throw new ArgumentNullException("store");
            else if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException("id");

            _store = store;
            _id = id;

            store.AddCategory(this);
        }

        public string CategoryId
        {
            get { return _id; }
        }

        public string Name
        {
            get { return _name; }
            internal set { _name = value; }
        }

        public IDictionary<string, string> Properties
        {
            get { return _props; }
        }

        public bool ShouldPersist
        {
            get { return true; }
        }
    }
}
