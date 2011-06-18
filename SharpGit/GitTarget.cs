using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    public abstract class GitTarget : IEquatable<GitTarget>
    {
        readonly GitRevision _revision;

        internal GitTarget(GitRevision revision)
        {
            if (revision == null)
                _revision = GitRevision.None;
            else
                _revision = revision;
        }

        /// <summary>Gets the operational revision</summary>
        public GitRevision Revision
        {
            get { return _revision; }
        }

        /// <summary>Gets the target name in normalized format</summary>
        public abstract string TargetName { get; }

        public abstract string FileName { get; }

        internal virtual string GitTargetName { get { return TargetName; } }

        /// <summary>Gets the GitTarget as string</summary>
        public override string ToString()
        {
            if (Revision.RevisionType == GitRevisionType.None)
                return TargetName;
            else
                return TargetName + "@" + Revision.ToString();
        }

        public static GitTarget FromUri(Uri value)
        {
            return new GitUriTarget(value);
        }

        public static GitTarget FromString(string value)
        {
            return new GitPathTarget(value);
        }

        public static implicit operator GitTarget(Uri value)
        {
            return value != null ? FromUri(value) : null;
        }

        public static implicit operator GitTarget(string value)
        {
            return value != null ? FromString(value) : null;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            return Equals(obj as GitTarget);
        }

        public virtual bool Equals(GitTarget other)
        {
            if (ReferenceEquals(this, other))
                return false;

            if (!String.Equals(other.GitTargetName, GitTargetName))
                return false;

            return Revision.Equals(other.Revision);
        }

        /// <summary>Serves as a hashcode for the specified type</summary>
        public override int GetHashCode()
        {
            return TargetName.GetHashCode();
        }

        internal abstract GitRevision GetGitRevision(GitRevision fileNoneValue, GitRevision uriNoneValue);
    }
}