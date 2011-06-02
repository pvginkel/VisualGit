using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    public sealed class GitRevision : IEquatable<GitRevision>
    {
        public GitRevision(string revision)
            : this(GitRevisionType.Hash)
        {
            if (revision == null)
                throw new ArgumentNullException("revision");

            Revision = revision;
            IsExplicit = true;
        }

        public GitRevision(DateTime date)
            : this(GitRevisionType.Time)
        {
            Time = date;
            IsExplicit = true;
        }

        private GitRevision(GitRevisionType type)
        {
            RevisionType = type;
        }

        //public static GitRevision One { get; private set; }
        //public static GitRevision Zero { get; private set; }

        public static readonly GitRevision Committed = new GitRevision(GitRevisionType.Committed);
        public static readonly GitRevision Previous = new GitRevision(GitRevisionType.Previous);
        public static readonly GitRevision Base = new GitRevision(GitRevisionType.Base);
        public static readonly GitRevision Working = new GitRevision(GitRevisionType.Working);
        public static readonly GitRevision Head = new GitRevision(GitRevisionType.Head);
        public static readonly GitRevision None = new GitRevision(GitRevisionType.None);

        public bool IsExplicit { get; private set; }
        public bool RequiresWorkingCopy { get; private set; }
        public DateTime Time { get; private set; }
        public string Revision { get; private set; }
        public GitRevisionType RevisionType { get; private set; }

        public bool Equals(GitRevision other)
        {
            if (ReferenceEquals(this, other))
                return true;

            if (other == null || RevisionType != other.RevisionType)
                return false;

            switch (RevisionType)
            {
                case GitRevisionType.Time:
                    return Time == other.Time;

                case GitRevisionType.Hash:
                    return String.Equals(Revision, other.Revision, StringComparison.OrdinalIgnoreCase);

                default:
                    return true;
            }
        }

        public static bool operator ==(GitRevision rev1, GitRevision rev2)
        {
            if (ReferenceEquals(rev1, rev2))
                return true;

            return rev1.Equals(rev2);
        }

        public static bool operator !=(GitRevision rev1, GitRevision rev2)
        {
            return !(rev1 == rev2);
        }

#if false
        public static implicit operator GitRevision(GitRevisionType value)
        {
            return new GitRevision(value);
        }
#endif

        public static implicit operator GitRevision(DateTime value)
        {
            return new GitRevision(value);
        }

        public static implicit operator GitRevision(string value)
        {
            return new GitRevision(value);
        }

        public override sealed string ToString()
        {
            switch (RevisionType)
            {
                case GitRevisionType.Hash:
                    return Revision;

                case GitRevisionType.Time:
                    return Time.ToString("g");

                default:
                    return RevisionType.ToString().ToUpper();
            }
        }

        public override sealed bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            return Equals(obj as GitRevision);
        }

        public override sealed int GetHashCode()
        {
            return ObjectUtil.CombineHashCodes(
                RevisionType.GetHashCode(),
                Time.GetHashCode(),
                Revision != null ? Revision.ToUpperInvariant().GetHashCode() : 0
            );
        }
    }
}
