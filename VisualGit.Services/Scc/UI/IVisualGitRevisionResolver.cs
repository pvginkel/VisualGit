// VisualGit.Services\Scc\UI\IVisualGitRevisionResolver.cs
//
// Copyright 2008-2011 The AnkhSVN Project
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//
// Changes and additions made for VisualGit Copyright 2011 Pieter van Ginkel.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using SharpGit;

namespace VisualGit.Scc.UI
{
    public interface IVisualGitRevisionProvider
    {
        /// <summary>
        /// Gets a list of VisualGitRevisions for the specified origin
        /// </summary>
        /// <param name="origin">The origin.</param>
        /// <returns></returns>
        IEnumerable<VisualGitRevisionType> GetRevisionTypes(GitOrigin origin);

        /// <summary>
        /// Resolves the specified revision.
        /// </summary>
        /// <param name="revision">The revision.</param>
        /// <returns></returns>
        VisualGitRevisionType Resolve(GitOrigin origin, GitRevision revision);

        /// <summary>
        /// Resolves the specified origin.
        /// </summary>
        /// <param name="origin">The origin.</param>
        /// <param name="from">From.</param>
        /// <returns></returns>
        VisualGitRevisionType Resolve(GitOrigin origin, VisualGitRevisionType from);
    }

    public interface IVisualGitRevisionResolver : IVisualGitRevisionProvider
    {
        /// <summary>
        /// Registers the extension.
        /// </summary>
        /// <param name="extension">The extension.</param>
        void RegisterExtension(IVisualGitRevisionProvider extension);
    }

    public abstract class VisualGitRevisionType : IEquatable<VisualGitRevisionType>
    {
        /// <summary>
        /// Gets the current value.
        /// </summary>
        /// <value>The current value.</value>
        public abstract GitRevision CurrentValue
        {
            get;
            set;
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public abstract override string ToString();


        string _uniqueName;
        /// <summary>
        /// Gets a unique name for this revision type
        /// </summary>
        /// <value>The unique typename</value>
        public virtual string UniqueName
        {
            get { return _uniqueName ?? (_uniqueName = GetType().FullName); }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has UI.
        /// </summary>
        /// <value><c>true</c> if this instance has UI; otherwise, <c>false</c>.</value>
        public virtual bool HasUI
        {
            get { return false; }
        }

        /// <summary>
        /// Instantiates the UI in the specified panel
        /// </summary>
        /// <param name="parentPanel">The parent panel.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        public virtual Control InstantiateUIIn(Panel parentPanel, EventArgs e)
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Gets the current control.
        /// </summary>
        /// <value>The current control.</value>
        public virtual Control CurrentControl
        {
            get { return null; }
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
            return Equals(obj as VisualGitRevisionType);
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            return UniqueName.GetHashCode();
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        public bool Equals(VisualGitRevisionType other)
        {
            if (other == null)
                return false;

            return UniqueName == other.UniqueName;
        }

        /// <summary>
        /// Determines whether [is valid on] [the specified SVN origin].
        /// </summary>
        /// <param name="SvnOrigin">The SVN origin.</param>
        /// <returns>
        /// 	<c>true</c> if [is valid on] [the specified SVN origin]; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool IsValidOn(GitOrigin origin)
        {
            return true;
        }
    }
}
