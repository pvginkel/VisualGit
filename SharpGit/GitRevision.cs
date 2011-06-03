using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NGit;

namespace SharpGit
{
    public sealed class GitRevision : IEquatable<GitRevision>
    {
        private GitRevision(GitRevision other)
        {
            IsExplicit = other.IsExplicit;
            Offset = other.Offset;
            RequiresWorkingCopy = other.RequiresWorkingCopy;
            Revision = other.Revision;
            RevisionType = other.RevisionType;
            Time = other.Time;
        }

        public GitRevision(string revision)
            : this(GitRevisionType.Hash)
        {
            if (revision == null)
                throw new ArgumentNullException("revision");

            Revision = revision;
        }

        public GitRevision(DateTime date)
            : this(GitRevisionType.Time)
        {
            Time = date;
        }

        private GitRevision(GitRevisionType type)
        {
            RevisionType = type;

            switch (type)
            {
                case GitRevisionType.Base:
                case GitRevisionType.Committed:
                case GitRevisionType.Hash:
                case GitRevisionType.Head:
                case GitRevisionType.Previous:
                case GitRevisionType.Working:
                case GitRevisionType.Zero:
                case GitRevisionType.One:
                    IsExplicit = true;
                    break;
            }
        }

        //public static GitRevision One { get; private set; }
        //public static GitRevision Zero { get; private set; }

        public static readonly GitRevision Committed = new GitRevision(GitRevisionType.Committed);
        public static readonly GitRevision Previous = new GitRevision(GitRevisionType.Previous);
        public static readonly GitRevision Base = new GitRevision(GitRevisionType.Base);
        public static readonly GitRevision Working = new GitRevision(GitRevisionType.Working);
        public static readonly GitRevision Head = new GitRevision(GitRevisionType.Head);
        public static readonly GitRevision None = new GitRevision(GitRevisionType.None);
        public static readonly GitRevision Zero = new GitRevision(GitRevisionType.Zero);
        public static readonly GitRevision One = new GitRevision(GitRevisionType.One);

        public bool IsExplicit { get; private set; }
        public bool RequiresWorkingCopy { get; private set; }
        public DateTime Time { get; private set; }
        public string Revision { get; private set; }
        public GitRevisionType RevisionType { get; private set; }
        public int Offset { get; private set; }

        internal ObjectId GetObjectId(Repository repository)
        {
            if (repository == null)
                throw new ArgumentNullException("repository");

            switch (RevisionType)
            {
                case GitRevisionType.Hash:
                    return repository.Resolve(ToString());

                case GitRevisionType.Head:
                    return repository.Resolve(Constants.HEAD);

                default:
                    throw new NotSupportedException();
            }
        }

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
                    return
                        String.Equals(Revision, other.Revision, StringComparison.OrdinalIgnoreCase) &&
                        Offset == other.Offset;

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

        public static GitRevision operator +(GitRevision rev, int offset)
        {
            return CreateOffset(rev, offset);
        }

        public static GitRevision operator +(int offset, GitRevision rev)
        {
            return CreateOffset(rev, offset);
        }

        public static GitRevision operator -(GitRevision rev, int offset)
        {
            return CreateOffset(rev, -offset);
        }

        public static GitRevision operator -(int offset, GitRevision rev)
        {
            return CreateOffset(rev, -offset);
        }

        public static GitRevision CreateOffset(GitRevision rev, int offset)
        {
            if (rev == null)
                return null;

            if (rev.RevisionType != GitRevisionType.Hash)
                throw new ArgumentException("Offsets are only supported on hashes", "rev");

            var result = new GitRevision(rev);

            result.Offset += offset;

            return result;
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
                    string revision = Revision;

                    if (Offset > 0)
                        throw new NotImplementedException();
                    if (Offset < 0)
                        revision += new String('^', -Offset);

                    return revision;

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
                Revision != null ? Revision.ToUpperInvariant().GetHashCode() : 0,
                Offset.GetHashCode()
            );
        }
    }
}
