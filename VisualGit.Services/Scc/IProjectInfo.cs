using System;
using System.Collections.Generic;
using System.Text;

namespace VisualGit.Scc
{
    public enum SccEnlistMode
    {
        /// <summary>
        /// This project is not enlist capable and is not handled specially
        /// </summary>
        None = 0,

        /// <summary>
        /// This project does not use enlist support but is not in the
        /// same tree as the solution; VisualGit stores extra information to track
        /// the reference.
        /// </summary>
        SvnStateOnly,

        /// <summary>
        /// The project requires Scc enlistment
        /// </summary>
        SccEnlistCompulsory,

        /// <summary>
        /// The project allows Scc enlistment
        /// </summary>
        SccEnlistOptional
    }

    /// <summary>
    /// 
    /// </summary>
    public interface ISvnProjectInfo
    {
        /// <summary>
        /// Gets the name of the project.
        /// </summary>
        /// <value>The name of the project.</value>
        string ProjectName { get; }

        /// <summary>
        /// Gets the project file.
        /// </summary>
        /// <value>The project file.</value>
        string ProjectFile { get; }

        /// <summary>
        /// Gets the full name of the project (the project prefixed by the folder it is under)
        /// </summary>
        /// <value>The full name of the project.</value>
        string UniqueProjectName { get; }

        /// <summary>
        /// Gets the project directory.
        /// </summary>
        /// <value>The project directory.</value>
        string ProjectDirectory { get; }

        /// <summary>
        /// Gets the SCC base directory.
        /// </summary>
        /// <value>The SCC base directory.</value>
        /// <remarks>The SCC base directory is the project directory or one of its parents</remarks>
        string SccBaseDirectory { get; set;  }

        /// <summary>
        /// Gets or sets the SCC base URI.
        /// </summary>
        /// <value>The SCC base URI.</value>
        Uri SccBaseUri { get; set; }

        /// <summary>
        /// Gets the enlist mode.
        /// </summary>
        /// <value>The enlist mode.</value>
        SccEnlistMode SccEnlistMode { get; }

        /// <summary>
        /// Gets a boolean indicating whether the project support binding to SCC
        /// </summary>
        bool IsSccBindable { get; }
    }
}
