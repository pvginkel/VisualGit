using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Microsoft.VisualStudio.Shell;
using System.IO;
using SharpGit;

namespace VisualGit.Scc.ProjectMap
{
    class SccTranslateData
    {
        readonly VisualGitSccProvider _provider;
        readonly Guid _projectId;
        string _slnProjectLocation;
        SccProjectData _project;
        bool _disposed;
        string _sccPath;

        public SccTranslateData(VisualGitSccProvider provider, Guid projectId, string slnProjectLocation)
        {
            if (provider == null)
                throw new ArgumentNullException("provider");
            else if (string.IsNullOrEmpty(slnProjectLocation))
                throw new ArgumentNullException("slnProjectLocation");

            _provider = provider;
            _projectId = projectId;

            SlnProjectLocation = slnProjectLocation;
        }

        internal virtual void Dispose()
        {
            _disposed = true;
        }

        public Guid ProjectId
        {
            get { return _projectId; }
        }

        [DebuggerStepThrough]
        T GetService<T>()
            where T : class
        {
            return ((IVisualGitServiceProvider)_provider).GetService<T>();
        }

        [DebuggerStepThrough]
        T GetService<T>(Type serviceType)
            where T : class
        {
            return ((IVisualGitServiceProvider)_provider).GetService<T>(serviceType);
        }

        public SccProjectData Project
        {
            get
            {
                if (_project != null && !_project.IsDisposed)
                    return _project;

                return _project = _provider.GetSccProject(_projectId);
            }
        }

        string _sccBasePath;
        /// <summary>
        /// Gets the current Scc Base Directory (per user setting)
        /// </summary>
        public string SccBaseDirectory
        {
            get
            {
                if (_sccBasePath != null)
                    return string.IsNullOrEmpty(_sccBasePath) ? null : _sccBasePath;
                else if (Project != null)
                    return Project.SccBaseDirectory;
                else
                    return null;
            }
            set { _sccBasePath = value ?? ""; }
        }

        public string SccRelativePath
        {
            get { return PackageUtilities.MakeRelative(SccBaseDirectory, SccPathName); }
        }
        
        public string SlnProjectLocation
        {
            get { return _slnProjectLocation; }
            set 
            {
                string oldValue = _slnProjectLocation;

                _slnProjectLocation = value ?? "";

                _provider.Translate_SetStoredPath(this, oldValue, value);
            }
        }

        public string GetSlnProjectLocation(string solutionDirectory)
        {
            if(string.IsNullOrEmpty(solutionDirectory))
                throw new ArgumentNullException("solutionDirectory");

            string location = SlnProjectLocation;

            if (GitItem.IsValidPath(location))
                return PackageUtilities.MakeRelative(solutionDirectory, location);
            else
                return location;
        }

        /// <summary>
        /// Gets or sets the path used for storing the project in the solution
        /// </summary>
        /// <value>The name of the stored path.</value>
        public string StoredPathName
        {
            get { return SlnProjectLocation; }            
        }

        /// <summary>
        /// Gets or sets the name of the SCC path; the path as used to access the project on disk
        /// </summary>
        /// <value>The name of the SCC path.</value>
        public string SccPathName
        {
            get 
            {
                if (_sccPath != null)
                    return string.IsNullOrEmpty(_sccPath) ? null : _sccPath;
                else if (Project != null)
                    return Project.ProjectDirectory;

                return null;
            }
            set
            {
                if (_sccPath == value)
                    return;

                Debug.Assert(!_disposed, "Not disposed");

                string path = GitTools.GetNormalizedFullPath(value);

                if (_provider.Translate_SetSccPath(this, path))
                    _sccPath = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the enlist path.
        /// </summary>
        /// <value>The name of the enlist path.</value>
        public virtual string EnlistPathName
        {
            get { return null; }
            set { throw new InvalidOperationException(); }
        }

        SccEnlistMode? _enlistMode;
        public SccEnlistMode SccEnlistMode
        {
            get
            {
                if (_enlistMode.HasValue)
                    return _enlistMode.Value;
                else if (Project != null)
                    return Project.EnlistMode;
                else
                    return SccEnlistMode.None;
            }
            set { _enlistMode = value; }
        }

        /// <summary>
        /// Gets a boolean indicating whether translation data should be stored in the solution
        /// </summary>
        /// <returns></returns>
        public virtual bool ShouldSerializeInSolution()
        {
            if (SccEnlistMode == SccEnlistMode.None)
                return false;
            if (true)
            {
                if (SccBaseDirectory == null)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Gets a boolean indicating whether translation data should be stored in the user settings
        /// </summary>
        /// <returns></returns>
        public virtual bool ShouldSerializeInUserSettings()
        {
            return true;
        }
    }
}
