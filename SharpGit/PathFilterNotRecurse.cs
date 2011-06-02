using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NGit.Treewalk.Filter;
using NGit.Treewalk;

namespace SharpGit
{
    internal class PathFilterNotRecurse : TreeFilter
    {
        private readonly string _path;
        private readonly GitDepth _depth;

        public PathFilterNotRecurse(string path, GitDepth depth)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            _path = path;
            _depth = depth;
        }

        public override TreeFilter Clone()
        {
            return this;
        }

        public override bool Include(TreeWalk walker)
        {
            return RepositoryUtil.PathMatches(_path, walker.PathString, walker.IsSubtree, _depth);
        }

        public override bool ShouldBeRecursive()
        {
            return _path.Contains('/') || _depth > GitDepth.Files;
        }
    }
}
