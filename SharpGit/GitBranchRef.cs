using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    public sealed class GitBranchRef : IEquatable<GitBranchRef>
    {
        private static readonly string[] ShortenPrefixes = new string[] { "refs/tags/", "refs/heads/", "refs/remotes/", "refs/" }; // Shortest match **MUST** be at the end
        private static readonly string[] ShortenPostfixes = new string[] { "/HEAD" };

        public GitBranchRef(string name)
            : this(name, null)
        {
        }

        public GitBranchRef(string name, GitRevision revision)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            Name = name;
            Revision = revision ?? GitRevision.Head;

            ShortName = name;

            foreach (string prefix in ShortenPrefixes)
            {
                if (ShortName.StartsWith(prefix, StringComparison.Ordinal))
                {
                    ShortName = ShortName.Substring(prefix.Length);
                    break;
                }
            }

            foreach (string postfix in ShortenPostfixes)
            {
                if (ShortName.EndsWith(postfix, StringComparison.Ordinal))
                {
                    ShortName = ShortName.Substring(0, ShortName.Length - postfix.Length);
                    break;
                }
            }
        }

        public string Name { get; private set; }
        public string ShortName { get; private set; }
        public GitRevision Revision { get; private set; }

        public override string ToString()
        {
            return ShortName;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            return Equals(obj as GitBranchRef);
        }

        public bool Equals(GitBranchRef other)
        {
            if (ReferenceEquals(this, other))
                return true;

            return
                other != null &&
                Name == other.Name &&
                Revision == other.Revision;
        }

        public override int GetHashCode()
        {
            return ObjectUtil.CombineHashCodes(
                Name.GetHashCode(),
                Revision != null ? Revision.GetHashCode() : 0
            );
        }

        public static bool operator ==(GitBranchRef rev1, GitBranchRef rev2)
        {
            if (ReferenceEquals(rev1, rev2))
                return true;

            return rev1.Equals(rev2);
        }

        public static bool operator !=(GitBranchRef rev1, GitBranchRef rev2)
        {
            return !(rev1 == rev2);
        }
    }
}
