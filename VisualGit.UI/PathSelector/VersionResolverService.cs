using System;
using System.Collections.Generic;
using System.Text;
using VisualGit.Scc.UI;
using System.Collections;
using SharpSvn;
using VisualGit.Scc;
using SharpGit;

namespace VisualGit.UI.PathSelector
{
    [GlobalService(typeof(IVisualGitRevisionResolver))]
    sealed class VersionResolverService : VisualGitService, IVisualGitRevisionResolver
    {
        readonly List<IVisualGitRevisionProvider> _providers = new List<IVisualGitRevisionProvider>();

        public VersionResolverService(IVisualGitServiceProvider context)
            : base(context)
        {
            _providers.Add(new StandardVersionResolverService(this));
        }

        #region IVisualGitRevisionResolver Members

        /// <summary>
        /// Registers the extension.
        /// </summary>
        /// <param name="extension">The extension.</param>
        public void RegisterExtension(IVisualGitRevisionProvider extension)
        {
            if (extension == null)
                throw new ArgumentNullException("extension");

            _providers.Add(extension);
        }

        #endregion

        #region IVisualGitRevisionProvider Members

        /// <summary>
        /// Gets a list of VisualGitRevisions for the specified origin
        /// </summary>
        /// <param name="origin">The origin.</param>
        /// <returns></returns>
        public IEnumerable<VisualGitRevisionType> GetRevisionTypes(VisualGit.Scc.GitOrigin origin)
        {
            Hashtable ht = new Hashtable();
            foreach (IVisualGitRevisionProvider p in _providers)
            {
                foreach (VisualGitRevisionType rt in p.GetRevisionTypes(origin))
                {
                    if (!ht.Contains(rt))
                        yield return rt;

                    ht.Add(rt, rt.UniqueName);
                }
            }
        }

        /// <summary>
        /// Resolves the specified revision.
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="revision">The revision.</param>
        /// <returns></returns>
        public VisualGitRevisionType Resolve(GitOrigin origin, GitRevision revision)
        {
            if (revision == null)
                throw new ArgumentNullException("revision");

            foreach (IVisualGitRevisionProvider p in _providers)
            {
                VisualGitRevisionType r = p.Resolve(origin, revision);

                if (r != null)
                    return r;
            }

            switch (revision.RevisionType)
            {
                case GitRevisionType.Hash:
                    ExplicitRevisionType ert = new ExplicitRevisionType(Context, origin);
                    ert.CurrentValue = revision;
                    return ert;
            }

            return null;
        }

        public VisualGitRevisionType Resolve(GitOrigin origin, VisualGitRevisionType revision)
        {
            if (revision == null)
                throw new ArgumentNullException("revision");

            foreach (IVisualGitRevisionProvider p in _providers)
            {
                VisualGitRevisionType r = p.Resolve(origin, revision);

                if (r != null)
                    return r;
            }

            return Resolve(origin, revision.CurrentValue);
        }

        #endregion

        sealed class StandardVersionResolverService : VisualGitService, IVisualGitRevisionProvider
        {
            static readonly SimpleRevisionType _head = new SimpleRevisionType(GitRevision.Head, VersionStrings.HeadVersion);
            static readonly SimpleRevisionType _working = new SimpleRevisionType(GitRevision.Working, VersionStrings.WorkingVersion);
            static readonly SimpleRevisionType _base = new SimpleRevisionType(GitRevision.Base, VersionStrings.BaseVersion);
            static readonly SimpleRevisionType _committed = new SimpleRevisionType(GitRevision.Committed, VersionStrings.CommittedVersion);
            static readonly SimpleRevisionType _previous = new SimpleRevisionType(GitRevision.Previous, VersionStrings.PreviousVersion);

            public StandardVersionResolverService(VersionResolverService context)
                : base(context)
            {

            }
            #region IVisualGitRevisionProvider Members

            public IEnumerable<VisualGitRevisionType> GetRevisionTypes(VisualGit.Scc.GitOrigin origin)
            {
                if (origin == null)
                    throw new ArgumentNullException("origin");

                SvnPathTarget pt = origin.Target as SvnPathTarget;
                bool isPath = (pt != null);

                yield return _head;

                if (isPath)
                {
                    GitItem item = GetService<IFileStatusCache>()[pt.FullPath];

                    if (item.IsVersioned)
                    {
                        yield return _working;
                        yield return _base;
                    }
                    if (item.HasCopyableHistory)
                    {
                        yield return _committed;
                        yield return _previous;
                    }
                    else
                        yield break;
                }

                yield return new DateRevisionType(this, origin);
                yield return new ExplicitRevisionType(this, origin);
            }

            public VisualGitRevisionType Resolve(GitOrigin origin, GitRevision revision)
            {
                if (revision == null)
                    throw new ArgumentNullException("revision");

                switch (revision.RevisionType)
                {
                    case GitRevisionType.Head:
                        return _head;
                    case GitRevisionType.Base:
                        return _base;
                    case GitRevisionType.Committed:
                        return _committed;
                    case GitRevisionType.Previous:
                        return _previous;
                    case GitRevisionType.Working:
                        return _working;
                }

                return null;
            }

            public VisualGitRevisionType Resolve(GitOrigin origin, VisualGitRevisionType revision)
            {
                return Resolve(origin, revision.CurrentValue);
            }

            #endregion

            sealed class SimpleRevisionType : VisualGitRevisionType
            {
                readonly GitRevision _rev;
                readonly string _title;
                public SimpleRevisionType(GitRevision rev, string title)
                {
                    if (rev == null)
                        throw new ArgumentNullException("rev");
                    else if (string.IsNullOrEmpty(title))
                        throw new ArgumentNullException("title");

                    _rev = rev;
                    _title = title;
                }

                /// <summary>
                /// Gets the current value.
                /// </summary>
                /// <value>The current value.</value>
                public override GitRevision CurrentValue
                {
                    get { return _rev; }
                    set { throw new InvalidOperationException(); }
                }

                /// <summary>
                /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
                /// </summary>
                /// <returns>
                /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
                /// </returns>
                public override string ToString()
                {
                    return _title;
                }

                /// <summary>
                /// Gets a unique name for this revision type
                /// </summary>
                /// <value>The unique typename</value>
                public override string UniqueName
                {
                    get
                    {
                        return _rev.RevisionType.ToString();
                    }
                }

                public override bool IsValidOn(GitOrigin origin)
                {
                    switch (_rev.RevisionType)
                    {
                        case GitRevisionType.Base:
                        case GitRevisionType.Committed:
                        case GitRevisionType.Previous:
                        case GitRevisionType.Working:
                            return origin.Target is SvnPathTarget;
                        default:
                            return base.IsValidOn(origin);
                    }
                }
            }
        }
        sealed class DateRevisionType : VisualGitRevisionType
        {
            readonly GitOrigin _origin;
            readonly IVisualGitServiceProvider _context;
            DateTime _date;

            public DateRevisionType(IVisualGitServiceProvider context, GitOrigin origin)
            {
                if (context == null)
                    throw new ArgumentNullException("context");
                else if (origin == null)
                    throw new ArgumentNullException("origin");

                _context = context;
                _origin = origin;
                _date = DateTime.Today;
            }

            public override GitRevision CurrentValue
            {
                get { return _date != DateTime.MinValue ? new GitRevision(_date) : null; }
                set
                {
                    if (value == null || value.RevisionType != GitRevisionType.Time)
                        _date = DateTime.MinValue;
                    else
                        _date = value.Time;
                }
            }

            public override string ToString()
            {
                return VersionStrings.DateVersion;
            }

            public override bool HasUI
            {
                get
                {
                    return true;
                }
            }

            DateSelector _sel;

            public override System.Windows.Forms.Control InstantiateUIIn(System.Windows.Forms.Panel parentPanel, EventArgs e)
            {
                if (_sel != null)
                    throw new InvalidOperationException();

                _sel = new DateSelector();
                parentPanel.Controls.Add(_sel);
                _sel.Dock = System.Windows.Forms.DockStyle.Fill;
                _sel.Changed += new EventHandler(OnSelChanged);
                _sel.Value = _date;

                return _sel;
            }

            void OnSelChanged(object sender, EventArgs e)
            {
                if (_sel != null)
                    _date = _sel.Value;
            }

            public override System.Windows.Forms.Control CurrentControl
            {
                get
                {
                    return _sel;
                }
            }
        }

        sealed class ExplicitRevisionType : VisualGitRevisionType
        {
            readonly GitOrigin _origin;
            readonly IVisualGitServiceProvider _context;
            string _rev;

            public ExplicitRevisionType(IVisualGitServiceProvider context, GitOrigin origin)
            {
                if (context == null)
                    throw new ArgumentNullException("context");
                else if (origin == null)
                    throw new ArgumentNullException("origin");

                _context = context;
                _origin = origin;
            }

            public override GitRevision CurrentValue
            {
                get { return !String.IsNullOrEmpty(_rev) ? new GitRevision(_rev) : null; }
                set
                {
                    if (value == null || value.RevisionType != GitRevisionType.Hash)
                        _rev = null;
                    else
                        _rev = value.Revision;
                }
            }

            public override string ToString()
            {
                return VersionStrings.ExplicitVersion;
            }

            public override bool HasUI
            {
                get
                {
                    return true;
                }
            }

            RevisionSelector _sel;

            public override System.Windows.Forms.Control InstantiateUIIn(System.Windows.Forms.Panel parentPanel, EventArgs e)
            {
                if (_sel != null)
                    throw new InvalidOperationException();

                _sel = new RevisionSelector();
                _sel.Context = _context;
                _sel.GitOrigin = _origin;
                parentPanel.Controls.Add(_sel);
                _sel.Dock = System.Windows.Forms.DockStyle.Fill;
                _sel.Changed += new EventHandler(OnVersionChanged);
                _sel.Revision = _rev;

                return _sel;
            }

            void OnVersionChanged(object sender, EventArgs e)
            {
                if (_sel != null)
                {
                    string value = _sel.Revision;

                    if (!String.IsNullOrEmpty(value))
                        _rev = value;
                }
            }

            public override System.Windows.Forms.Control CurrentControl
            {
                get
                {
                    return _sel;
                }
            }
        }

    }
}
