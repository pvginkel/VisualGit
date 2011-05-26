using System;

using VisualGit.ExtensionPoints.IssueTracker;

namespace VisualGit.Services.IssueTracker
{
    /// <summary>
    /// Acts as a proxy to the the actual Issue Tracker Repository Connector.
    /// </summary>
    /// <remarks>
    /// This proxy serves "descriptive" properties w/o initializing the actual connector.
    /// The actual connector package initialization is delayed until a non-descriptive property is needed.
    /// Currently, "connector name" is the only descriptive property.
    /// </remarks>
    class IssueRepositoryConnectorProxy : IssueRepositoryConnector
    {
        private IssueRepositoryConnector _delegate;
        private readonly IVisualGitServiceProvider _context;
        private readonly string _name;
        private readonly string _delegateId;

        public IssueRepositoryConnectorProxy(IVisualGitServiceProvider context, string name, string delegateServiceId)
        {
            _context = context;
            _name = name;
            _delegateId = delegateServiceId;
        }

        private IssueRepositoryConnector Delegate
        {
            get
            {
                if (_delegate == null
                    && !string.IsNullOrEmpty(_delegateId))
                {
                    _delegate = _context.GetService<IVisualGitQueryService>().QueryService<IssueRepositoryConnector>(new Guid(_delegateId));
                }
                return _delegate;
            }
        }

        #region IIssueRepositoryConnector Members

        public override IssueRepository Create(IssueRepositorySettings settings)
        {
            IssueRepositoryConnector dlg = Delegate;
            if (dlg != null)
            {
                return dlg.Create(settings);
            }
            return null;
        }

        public override IssueRepositoryConfigurationPage ConfigurationPage
        {
            get
            {
                IssueRepositoryConnector dlg = Delegate;
                if (dlg != null)
                {
                    return dlg.ConfigurationPage;
                }
                return null;
            }
        }

        public override string Name
        {
            get { return _name; }
        }

        #endregion

    }
}
