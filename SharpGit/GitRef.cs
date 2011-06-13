using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NGit.Transport;

namespace SharpGit
{
    public sealed class GitRef : IEquatable<GitRef>
    {
        private static readonly Dictionary<GitRefType, string> _shortenPrefixes = new Dictionary<GitRefType, string>
        {
            { GitRefType.Tag, "refs/tags/" },
            { GitRefType.Branch, "refs/heads/" },
            { GitRefType.RemoteBranch, "refs/remotes/" },
            { GitRefType.Unknown, "refs/" }
        };

        private static readonly string[] ShortenPostfixes = new string[] { "/HEAD" };

        public static GitRefType TypeOf(string name)
        {
            if (name.IndexOf(':') >= 0)
            {
                return GitRefType.RefSpec;
            }
            else
            {
                var longestMatch = new KeyValuePair<GitRefType, string>();

                foreach (var prefix in _shortenPrefixes)
                {
                    if (
                        name.StartsWith(prefix.Value, StringComparison.Ordinal) &&
                        (longestMatch.Value == null || longestMatch.Value.Length < prefix.Value.Length)
                    )
                        longestMatch = prefix;
                }

                if (longestMatch.Value != null)
                {
                    return longestMatch.Key;
                }
                else if (String.Equals(name, "HEAD", StringComparison.Ordinal))
                {
                    return GitRefType.Head;
                }
                else
                {
                    return GitRefType.Unknown;
                }
            }
        }

        public GitRef(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            name = name.Trim();

            if (String.Empty.Equals(name))
                throw new ArgumentException("Name cannot be empty", "name");

            Name = name;

            ShortName = name;

            if (Name.IndexOf(':') >= 0)
            {
                Type = GitRefType.RefSpec;
            }
            else
            {
                var longestMatch = new KeyValuePair<GitRefType, string>();

                foreach (var prefix in _shortenPrefixes)
                {
                    if (
                        ShortName.StartsWith(prefix.Value, StringComparison.Ordinal) &&
                        (longestMatch.Value == null || longestMatch.Value.Length < prefix.Value.Length)
                    )
                        longestMatch = prefix;
                }

                if (longestMatch.Value != null)
                {
                    ShortName = ShortName.Substring(longestMatch.Value.Length);
                    Type = longestMatch.Key;
                }
                else if (String.Equals(Name, "HEAD", StringComparison.Ordinal))
                {
                    Type = GitRefType.Head;
                }
                else
                {
                    Type = GitRefType.Unknown;
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
        }

        internal GitRef(NGit.Ref @ref)
            : this(@ref.GetName())
        {
            Revision = @ref.GetObjectId().Name;
        }

        public static GitRef Create(string name, GitRefType type)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            name = name.Trim();

            if (String.Empty.Equals(name))
                throw new ArgumentException("Name cannot be empty", "name");

            if (type == GitRefType.Head && !String.Equals(name, "HEAD", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Reference of type Head must be named 'HEAD'", "name"); 

            string prefix;
            
            if (
                _shortenPrefixes.TryGetValue(type, out prefix) &&
                !name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            )
                name = prefix + name.TrimStart('/');

            return new GitRef(name);
        }

        public string Name { get; private set; }
        public string ShortName { get; private set; }
        public GitRefType Type { get; private set; }
        public string Revision { get; private set; }

        public override string ToString()
        {
            return ShortName;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            return Equals(obj as GitRef);
        }

        public bool Equals(GitRef other)
        {
            if (ReferenceEquals(this, other))
                return true;

            return
                other != null &&
                Name == other.Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public static bool operator ==(GitRef rev1, GitRef rev2)
        {
            if (ReferenceEquals(rev1, rev2))
                return true;

            return rev1.Equals(rev2);
        }

        public static bool operator !=(GitRef rev1, GitRef rev2)
        {
            return !(rev1 == rev2);
        }

        public static implicit operator GitRevision(GitRef @ref)
        {
            if (@ref == null)
                return null;
            else
                return new GitRevision(@ref.Name);
        }
    }
}
