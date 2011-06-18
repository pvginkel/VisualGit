using System;
using System.Collections.Generic;
using System.Text;

namespace VisualGit.Scc
{
    public interface IPendingChangeHandler
    {
        bool Commit(IEnumerable<PendingChange> changes, PendingChangeCommitArgs args);
        bool CreatePatch(IEnumerable<PendingChange> changes, PendingChangeCreatePatchArgs args);

        bool ApplyChanges(IEnumerable<PendingChange> changes, PendingChangeApplyArgs args);
    }

    public class PendingChangeCommitArgs
    {
        string _logMessage;
        bool _storeMessageOnError;
        bool _amendLastCommit;

        /// <summary>
        /// Gets or sets the log message.
        /// </summary>
        /// <value>The log message.</value>
        public string LogMessage
        {
            get { return _logMessage; }
            set { _logMessage = value; }
        }

        public bool AmendLastCommit
        {
            get { return _amendLastCommit; }
            set { _amendLastCommit = value; }
        }

        public bool StoreMessageOnError
        {
            get { return _storeMessageOnError; }
            set { _storeMessageOnError = value; }
        }
    }

    public class PendingChangeCreatePatchArgs
    {
        string _fileName;
        string _relativeToPath;
        bool _addUnversionedFiles;

        public string FileName
        {
            get { return _fileName; }
            set { _fileName = value; }
        }

        public string RelativeToPath
        {
            get { return _relativeToPath; }
            set { _relativeToPath = value; }
        }

        public bool AddUnversionedFiles
        {
            get { return _addUnversionedFiles; }
            set { _addUnversionedFiles = value; }
        }
    }

    public class PendingChangeApplyArgs
    {
    }
}
