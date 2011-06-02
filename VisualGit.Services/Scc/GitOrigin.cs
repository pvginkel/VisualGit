using System;
using System.Collections.Generic;
using System.Text;
using SharpSvn;
using System.Diagnostics;
using System.Globalization;

namespace VisualGit.Scc
{
    /// <summary>
    /// Container of a <see cref="SvnTarget"/>, its repository Uri and its repository root.
    /// </summary>
    public class GitOrigin : IEquatable<GitOrigin>, IFormattable
    {
        readonly SvnTarget _target;
        readonly Uri _uri;
        readonly Uri _reposRoot;

        /// <summary>
        /// Initializes a new instance of the <see cref="GitOrigin"/> class using a GitItem
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="gitItem">The SVN item.</param>
        public GitOrigin(GitItem gitItem)
        {
            if (gitItem == null)
                throw new ArgumentNullException("gitItem");

            if (!gitItem.IsVersioned)
                throw new InvalidOperationException("Can only create a GitOrigin from versioned items");

            _target = new SvnPathTarget(gitItem.FullPath);
            _uri = gitItem.Status.Uri;
            _reposRoot = gitItem.WorkingCopy.RepositoryRoot;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GitOrigin"/> class.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <param name="reposRoot">The repos root.</param>
        public GitOrigin(Uri uri, Uri reposRoot)
            : this(new SvnUriTarget(uri, SvnRevision.Head), reposRoot)
        {
            _uri = uri; // Keep Uri unnormalized for UI purposes
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GitOrigin"/> class.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <param name="origin">The original origin to calculate from</param>
        public GitOrigin(Uri uri, GitOrigin origin)
        {
            if (uri == null)
                throw new ArgumentNullException("uri");
            else if (origin == null)
                throw new ArgumentNullException("origin");

            SvnUriTarget target = new SvnUriTarget(uri, origin.Target.Revision);
            _target = target;
            _uri = uri;
            _reposRoot = origin.RepositoryRoot;

#if DEBUG
            Debug.Assert(!_reposRoot.MakeRelativeUri(_uri).IsAbsoluteUri);
#endif

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GitOrigin"/> class.
        /// </summary>
        /// <param name="uriTarget">The URI target.</param>
        /// <param name="reposRoot">The repos root.</param>
        public GitOrigin(SvnUriTarget uriTarget, Uri reposRoot)
        {
            if (uriTarget == null)
                throw new ArgumentNullException("uriTarget");
            else if (reposRoot == null)
                throw new ArgumentNullException("reposRoot");

            _target = uriTarget;
            _uri = uriTarget.Uri;
            _reposRoot = reposRoot;
#if DEBUG
            Debug.Assert(!_reposRoot.MakeRelativeUri(_uri).IsAbsoluteUri);
#endif
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GitOrigin"/> class from a SvnTarget
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="target">The target.</param>
        /// <param name="reposRoot">The repos root or <c>null</c> to retrieve the repository root from target</param>
        public GitOrigin(IVisualGitServiceProvider context, SvnTarget target, Uri reposRoot)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            else if (target == null)
                throw new ArgumentNullException("target");

            SvnPathTarget pt = target as SvnPathTarget;

            if (pt != null)
            {
                GitItem item = context.GetService<IFileStatusCache>()[pt.FullPath];

                if (item == null || !item.IsVersioned)
                    throw new InvalidOperationException("Can only create a GitOrigin from versioned items");

                _target = target;
                _uri = item.Status.Uri;
                _reposRoot = item.WorkingCopy.RepositoryRoot; // BH: Prefer the actual root over the provided
                return;
            }

            SvnUriTarget ut = target as SvnUriTarget;

            if (ut != null)
            {
                _target = ut;
                _uri = ut.Uri;
                if (reposRoot != null)
                    _reposRoot = reposRoot;
                else
                {
                    using (SvnClient client = context.GetService<ISvnClientPool>().GetClient())
                    {
                        _reposRoot = client.GetRepositoryRoot(ut.Uri);

                        if (_reposRoot == null)
                            throw new InvalidOperationException("Can't retrieve the repository root of the UriTarget");

#if DEBUG
                        Debug.Assert(!_reposRoot.MakeRelativeUri(_uri).IsAbsoluteUri);
#endif
                    }
                }

                return;
            }

            throw new InvalidOperationException("Invalid target type");
        }

        /// <summary>
        /// Gets the repository root.
        /// </summary>
        /// <value>The repository root.</value>
        public Uri RepositoryRoot
        {
            get { return _reposRoot; }
        }

        /// <summary>
        /// Gets the repository URI of the item
        /// </summary>
        /// <value>The URI.</value>
        public Uri Uri
        {
            get { return _uri; }
        }

        /// <summary>
        /// Gets the target of the item
        /// </summary>
        /// <value>The target.</value>
        public SvnTarget Target
        {
            get { return _target; }
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>.</param>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.NullReferenceException">
        /// The <paramref name="obj"/> parameter is null.
        /// </exception>
        public override bool Equals(object obj)
        {
            return Equals(obj as GitOrigin);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        public bool Equals(GitOrigin other)
        {
            if (other == null)
                return false;

            return other.Target == Target && other.Uri == Uri;
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            return _target.GetHashCode();
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="o1">The o1.</param>
        /// <param name="o2">The o2.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(GitOrigin o1, GitOrigin o2)
        {
            bool n1 = (object)o1 == null;
            bool n2 = (object)o2 == null;

            if (n1 ^ n2)
                return false;
            else if (n1 && n2)
                return true;

            return o1.Target == o2.Target && o1.RepositoryRoot == o2.RepositoryRoot && o1.Uri == o2.Uri;
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="o1">The o1.</param>
        /// <param name="o2">The o2.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(GitOrigin o1, GitOrigin o2)
        {
            return !(o1 == o2);
        }

        /// <summary>
        /// Determines whether the origin specified the repositoryroot
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if [is repository root]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsRepositoryRoot
        {
            get
            {
                return SvnTools.GetNormalizedUri(RepositoryRoot) == SvnTools.GetNormalizedUri(Uri);
            }
        }

        #region IFormattable Members

        public override string ToString()
        {
            return ToString("", CultureInfo.CurrentCulture);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (string.IsNullOrEmpty(format))
                return Target.ToString();

            switch (format)
            {
                case "t":
                    return Target.ToString();
                case "f":
                    return Target.FileName;
                case "s":
                    return Target.FileName + " (" + Target.ToString() + ")";
                default:
                    return Target.ToString();
            }
        }

        #endregion
    }
}
