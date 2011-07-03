// VisualGit.Services\UI\PathSelectorInfo.cs
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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using VisualGit.UI;
using VisualGit.Scc;
using SharpGit;

namespace VisualGit
{
    /// <summary>
    /// Represents the parameters passed to UIShell.ShowPathSelector
    /// </summary>
    public class PathSelectorInfo
    {
        readonly string _caption;
        bool _singleSelection;
        bool _enableRecursive;
        GitDepth _depth = GitDepth.Empty;
        readonly ICollection<GitItem> _items;
        readonly Dictionary<string, GitItem> _checkedItems = new Dictionary<string, GitItem>(StringComparer.OrdinalIgnoreCase);
        readonly Dictionary<string, GitItem> _visibleItems = new Dictionary<string, GitItem>(StringComparer.OrdinalIgnoreCase);
        GitRevision _revisionStart;
        GitRevision _revisionEnd;
        Predicate<GitItem> _checkedFilter;
        Predicate<GitItem> _visibleFilter;
        SelectableFilter _checkableFilter;

        bool _evaluated;

        public PathSelectorInfo(string caption, IEnumerable<GitItem> items)
        {
            if (string.IsNullOrEmpty(caption))
                throw new ArgumentNullException("caption");
            if (items == null)
                throw new ArgumentNullException("items");

            _caption = caption;
            _items = new List<GitItem>(items);
        }

        public event Predicate<GitItem> CheckedFilter
        {
            add
            {
                _evaluated = false;
                _checkedFilter += value;
            }
            remove
            {
                _evaluated = false;
                _checkedFilter -= value;
            }
        }

        public event Predicate<GitItem> VisibleFilter
        {
            add
            {
                _evaluated = false;
                _visibleFilter += value;
            }
            remove
            {
                _evaluated = false;
                _visibleFilter -= value;
            }
        }

        public event SelectableFilter CheckableFilter
        {
            add
            {
                _evaluated = false;
                _checkableFilter += value;
            }
            remove
            {
                _evaluated = false;
                _checkableFilter -= value;
            }
        }

        public bool EvaluateChecked(GitItem item)
        {
            return EvaluateFilter(item, _checkedFilter);
        }

        public bool EvaluateCheckable(GitItem item, GitRevision from, GitRevision to)
        {
            if (_checkableFilter == null)
                return true;

            foreach (SelectableFilter i in _checkableFilter.GetInvocationList())
            {
                if (!i(item, from, to))
                    return false;
            }
            return true;
        }

        void EnsureFiltered()
        {
            if (!_evaluated)
            {
                _checkedItems.Clear();
                _visibleItems.Clear();

                foreach (GitItem i in _items)
                {
                    if (EvaluateFilter(i, _visibleFilter))
                    {
                        if (!_visibleItems.ContainsKey(i.FullPath))
                            _visibleItems.Add(i.FullPath, i);

                        if (EvaluateFilter(i, _checkedFilter)
                            // make sure all the checked items are suitable for the revisions
                            && EvaluateCheckable(i, RevisionStart, RevisionEnd))
                        {
                            if (!_checkedItems.ContainsKey(i.FullPath))
                                _checkedItems.Add(i.FullPath, i);
                        }
                    }
                }
                _evaluated = true;
            }
        }

        public ICollection<GitItem> VisibleItems
        {
            get
            {
                EnsureFiltered();
                return _visibleItems.Values;
            }
        }

        ICollection<GitItem> CheckedItems
        {
            get
            {
                EnsureFiltered();
                return _checkedItems.Values;
            }
        }

        public bool EnableRecursive
        {
            get { return _enableRecursive; }
            set { _enableRecursive = value; }
        }

        public GitDepth Depth
        {
            get { return _depth; }
            set { _depth = value; }
        }

        public GitRevision RevisionStart
        {
            get { return _revisionStart; }
            set { _revisionStart = value; }
        }

        public GitRevision RevisionEnd
        {
            get { return _revisionEnd; }
            set { _revisionEnd = value; }
        }

        public bool SingleSelection
        {
            get { return _singleSelection; }
            set { _singleSelection = value; }
        }

        public string Caption
        {
            get { return this._caption; }
            //set{ this.caption = value; }
        }

        public PathSelectorResult DefaultResult
        {
            get
            {
                PathSelectorResult result = new PathSelectorResult(true, CheckedItems);
                result.RevisionStart = RevisionStart;
                result.RevisionEnd = RevisionEnd;
                result.Depth = Depth;
                return result;
            }
        }

        public delegate bool SelectableFilter(GitItem item, GitRevision from, GitRevision to);

        public static bool EvaluateFilter(GitItem item, Predicate<GitItem> filter)
        {
            if (item == null)
                return false;
            if (filter == null)
                return true;

            foreach (Predicate<GitItem> i in filter.GetInvocationList())
            {
                if (!i(item))
                    return false;
            }
            return true;
        }
    }
}
