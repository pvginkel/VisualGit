using System;
using System.Collections.Generic;
using VisualGit.Configuration;
using Microsoft.Win32;

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
    }
}

