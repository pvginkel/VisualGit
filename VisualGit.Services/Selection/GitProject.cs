using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.Shell.Interop;

namespace VisualGit.Selection
{
    /// <summary>
    /// An <see cref="GitProject"/> instance is a black box reference to a project
    /// </summary>
    /// <remarks>
    /// <para>Normally you only use <see cref="GitProject"/> instances to pass between the <see cref="ISelectionContext"/>, 
    /// <see cref="ISccHierarchyWalker"/> and <see cref="IProjectFileMapper"/> services</para>
    /// <para>The GitProject contains a <see cref="path"/> and a <see cref="rawhandle"/> which can both be null but not both at the same time</para>
    /// <para>FullPath = null in case of a solution only-project (E.g. website project)</para>
    /// <para>RawHandle = null when retrieved from the selectionprovider when the file is not in a project (E.g. solution folder)</para>
    /// </remarks>
    public class GitProject : IEquatable<GitProject>
    {
        readonly string _fullPath;
        readonly IVsSccProject2 _rawHandle;

        static readonly GitProject _solution = new GitProject();
        public static GitProject Solution
        {
            get { return _solution; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SvnProjectItem"/> class.
        /// </summary>
        /// <param name="fullPath">The full path.</param>
        /// <param name="rawHandle">The raw handle.</param>
        [CLSCompliant(false)]
        public GitProject(string fullPath, IVsSccProject2 rawHandle)
        {
            if (string.IsNullOrEmpty(fullPath) && rawHandle == null)
                throw new ArgumentNullException("fullPath");

            // Current implementation details (which might change)

            // fullpath or rawHandle must be non-null

            // rawHandle = a IVsSccProject2 instance
            // fullPath = a file in the

            _fullPath = fullPath;
            _rawHandle = rawHandle;
        }

        private GitProject()
        { }

        /// <summary>
        /// Gets the full path.
        /// </summary>
        /// <value>The full path.</value>
        public string FullPath
        {
            get { return _fullPath; }
        }

        /// <summary>
        /// Gets the raw handle.
        /// </summary>
        /// <value>The raw handle.</value>
        [CLSCompliant(false)]
        public IVsSccProject2 RawHandle
        {
            get { return _rawHandle; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is solution.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is solution; otherwise, <c>false</c>.
        /// </value>
        public bool IsSolution
        {
            get { return _rawHandle == null && _fullPath == null; }
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
            return Equals(obj as GitProject);
        }

        /// <summary>
        /// Checks whether the specified object specifies the same object
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns></returns>
        public bool Equals(GitProject obj)
        {
            if (obj == null)
                return false;

            if(RawHandle != null || obj.RawHandle != null)
                return (RawHandle == obj.RawHandle);

            return FullPath == obj.FullPath;
        }

        /// <summary>
        /// Gets the hash code.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            if (RawHandle != null)
                return RawHandle.GetHashCode();

            if (this == Solution)
                return 0;

            return FullPath.GetHashCode();
        }
    }
}
