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
