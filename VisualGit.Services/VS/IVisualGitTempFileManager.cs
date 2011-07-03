// VisualGit.Services\VS\IVisualGitTempFileManager.cs
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

namespace VisualGit.VS
{
    /// <summary>
    /// 
    /// </summary>
    public interface IVisualGitTempFileManager
    {
        /// <summary>
        /// Gets a temporary file 
        /// </summary>
        /// <returns></returns>
        /// <remarks>The file is created as a 0 byte unique file before this function returns
        /// and will be removed after VisualGit exits</remarks>
        string GetTempFile();
        /// <summary>
        /// Gets a temporary file with the specified extension
        /// </summary>
        /// <param name="extension">The extension (with or without a leading period). </param>
        /// <returns></returns>
        /// <remarks>The file is created as a 0 byte unique file before this function returns
        /// and will be removed after VisualGit exits</remarks>
        string GetTempFile(string extension);

        /// <summary>
        /// Gets a temp file with the specified name
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <returns></returns>
        string GetTempFileNamed(string filename);
    }
}
