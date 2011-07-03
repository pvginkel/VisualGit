// VisualGit.UI\Commands\MergeStrategy.cs
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
using System.Linq;
using System.Text;
using SharpGit;
using System.Collections.ObjectModel;

namespace VisualGit.UI.Commands
{
    public class MergeStrategy
    {
        public GitMergeStrategy Strategy { get; private set; }
        public string Label { get; private set; }

        private MergeStrategy(GitMergeStrategy strategy, string label)
        {
            if (label == null)
                throw new ArgumentNullException("label");

            Strategy = strategy;
            Label = label;
        }

        public override string ToString()
        {
            return Label;
        }

        public static readonly MergeStrategy Resolve = new MergeStrategy(GitMergeStrategy.Resolve, Properties.Resources.StrategyResolve);
        public static readonly MergeStrategy SimpleTwoWayInCore = new MergeStrategy(GitMergeStrategy.SimpleTwoWayInCore, Properties.Resources.StrategySimpleTwoWayInCore);
        public static readonly MergeStrategy Ours = new MergeStrategy(GitMergeStrategy.Ours, Properties.Resources.StrategyOurs);
        public static readonly MergeStrategy Theirs = new MergeStrategy(GitMergeStrategy.Theirs, Properties.Resources.StrategyTheirs);

        public static readonly ICollection<MergeStrategy> All = new ReadOnlyCollection<MergeStrategy>(new MergeStrategy[]
        {
            Resolve,
            Ours,
            Theirs,
            SimpleTwoWayInCore
        });
    }
}
