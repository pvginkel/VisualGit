using System;
using System.Collections.Generic;
using VisualGit.ExtensionPoints.IssueTracker;
using VisualGit.IssueTracker;

namespace VisualGit.Services.IssueTracker
{
    public class IssueRepositorySettingsProxy : IssueRepositorySettings
    {
        readonly IVisualGitServiceProvider _context;

        public IssueRepositorySettingsProxy(IVisualGitServiceProvider context, string connectorName)
            : base(connectorName)
        {
            _context = context;
        }

        #region IssueRepositorySettingsBase Members

        public override Uri RepositoryUri
        {
            get
            {
                IIssueTrackerSettings settings = Settings;
                return settings == null ? null : settings.IssueRepositoryUri;
            }
        }

        public override string RepositoryId
        {
            get
            {
                IIssueTrackerSettings settings = Settings;
                return settings == null ? null : settings.IssueRepositoryId;
            }
        }

        public override IDictionary<string, object> CustomProperties
        {
            get
            {
                IIssueTrackerSettings settings = Settings;
                return settings == null ? null : settings.CustomProperties;
            }
        }

        #endregion

        private IIssueTrackerSettings Settings
        {
            get
            {
                if (_context == null) { return null; }
                return _context.GetService<IIssueTrackerSettings>();
            }
        }
    }
}
