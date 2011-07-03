// VisualGit.Services\UI\IVisualGitConfigurationService.cs
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
using VisualGit.Configuration;
using Microsoft.Win32;
using SharpGit;

namespace VisualGit.UI
{
    public enum VisualGitWarningBool
    {
        FatFsFound
    }
    /// <summary>
    /// 
    /// </summary>
    public interface IVisualGitConfigurationService
    {
        VisualGitConfig Instance
        {
            get;
        }

        void LoadConfig();

        void SaveConfig(VisualGitConfig config);

        void LoadDefaultConfig();

        /// <summary>
        /// Opens the user instance key (per hive + per user)
        /// </summary>
        /// <param name="subKey">The sub key.</param>
        /// <returns></returns>
        RegistryKey OpenUserInstanceKey(string subKey);

        /// <summary>
        /// Opens the instance key (per hive)
        /// </summary>
        /// <param name="subKey">The sub key.</param>
        /// <returns></returns>
        RegistryKey OpenInstanceKey(string subKey);

        /// <summary>
        /// Opens the global key (one hklm key)
        /// </summary>
        /// <param name="subKey">The sub key.</param>
        /// <returns></returns>
        RegistryKey OpenGlobalKey(string subKey);


        /// <summary>
        /// Gets the recent log messages.
        /// </summary>
        /// <returns></returns>
        RegistryLifoList GetRecentLogMessages();

        /// <summary>
        /// Gets the recent Repository Urls
        /// </summary>
        /// <returns></returns>
        RegistryLifoList GetRecentReposUrls();

        /// <summary>
        /// Save SmartColumns widths to registry
        /// </summary>
        /// <param name="controlType">The <see cref="Type"/> of control to save columns for</param>
        /// <param name="widths">Dictionary of column names and widths</param>
        void SaveColumnsWidths(Type controlType, IDictionary<string, int> widths);

        /// <summary>
        /// Get SmartColumns widths from registry
        /// </summary>
        /// <param name="controlType">The <see cref="Type"/> of control to get columns for</param>
        /// <returns>Dictionary of column names and widths</returns>
        IDictionary<string, int> GetColumnWidths(Type controlType);

        /// <summary>
        /// Save window size and position in registry
        /// </summary>
        /// <param name="controlType">The <see cref="Type"/> of control to save placement for</param>
        /// <param name="placement">Dictionary of window size and posiotion</param>
        void SaveWindowPlacement(Type controlType, IDictionary<string, int> placement);

        /// <summary>
        /// Get window size and position from registry
        /// </summary>
        /// <param name="controlType">The <see cref="Type"/> of control to get placement for</param>
        /// <returns>Dictionary of window size and position</returns>
        IDictionary<string, int> GetWindowPlacement(Type controlType);

        bool GetWarningBool(VisualGitWarningBool visualGitWarningBool);
        void SetWarningBool(VisualGitWarningBool visualGitWarningBool, bool value);

        CredentialCacheItem GetCredentialCacheItem(string uri, string type, string promptText);
        void StoreCredentialCacheItem(CredentialCacheItem item);
        ICollection<CredentialCacheItem> GetAllCredentialCacheItems();
        void RemoveCredentialCacheItem(string uri, string type, string promptText);

        void StoreCertificate(GitCertificate item);
        ICollection<GitCertificate> GetAllCertificates();
        void RemoveCertificate(string path);
        void RegisterCertificates();
    }
}

