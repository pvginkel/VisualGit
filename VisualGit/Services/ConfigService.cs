using System;
using System.Collections.Generic;
using VisualGit.UI;
using Microsoft.Win32;
using System.ComponentModel;
using VisualGit.VS;
using System.Text;
using System.Security.Cryptography;
using SharpGit;


namespace VisualGit.Configuration
{
    /// <summary>
    /// Contains functions used to load and save configuration data.
    /// </summary>
    [GlobalService(typeof(IVisualGitConfigurationService))]
    sealed class ConfigService : VisualGitService, IVisualGitConfigurationService
    {
        static byte[] _additionalEntropy = { 108, 236, 208, 80, 227 };
        readonly object _lock = new object();
        VisualGitConfig _instance;

        public ConfigService(IVisualGitServiceProvider context)
            : base(context)
        {
        }

        protected override void OnInitialize()
        {
            RegisterCertificates();
        }

        public VisualGitConfig Instance
        {
            get { return _instance ?? (_instance = GetNewConfigInstance()); }
        }

        IVisualGitSolutionSettings _settings;
        IVisualGitSolutionSettings Settings
        {
            get { return _settings ?? (_settings = GetService<IVisualGitSolutionSettings>()); }
        }

        /// <summary>
        /// Loads the VisualGit configuration file from the given path.
        /// </summary>
        /// <returns>A Config object.</returns>
        public VisualGitConfig GetNewConfigInstance()
        {
            VisualGitConfig instance = new VisualGitConfig();

            SetDefaultsFromRegistry(instance);
            SetSettingsFromRegistry(instance);

            return instance;
        }

        void IVisualGitConfigurationService.LoadConfig()
        {
            _instance = GetNewConfigInstance();
        }

        /// <summary>
        /// Loads the default config file. Used as a fallback if the
        /// existing config file cannot be loaded.
        /// </summary>
        /// <returns></returns>
        public void LoadDefaultConfig()
        {
            lock (_lock)
            {
                _instance = new VisualGitConfig();
                SetDefaultsFromRegistry(_instance);
            }
        }

        void SetDefaultsFromRegistry(VisualGitConfig config)
        {
            using (RegistryKey reg = OpenHKLMCommonKey("Configuration"))
            {
                if (reg != null)
                    foreach (PropertyDescriptor pd in TypeDescriptor.GetProperties(config))
                    {
                        string value = reg.GetValue(pd.Name, null) as string;

                        if (value != null)
                            try
                            {
                                pd.SetValue(config, pd.Converter.ConvertFromInvariantString(value));
                            }
                            catch { }
                    }
            }


            using (RegistryKey reg = OpenHKLMKey("Configuration"))
            {
                if (reg != null)
                    foreach (PropertyDescriptor pd in TypeDescriptor.GetProperties(config))
                    {
                        string value = reg.GetValue(pd.Name, null) as string;

                        if (value != null)
                            try
                            {
                                pd.SetValue(config, pd.Converter.ConvertFromInvariantString(value));
                            }
                            catch { }
                    }
            }
        }

        void SetSettingsFromRegistry(VisualGitConfig config)
        {
            using (RegistryKey reg = OpenHKCUKey("Configuration"))
            {
                if (reg == null)
                    return;

                foreach (PropertyDescriptor pd in TypeDescriptor.GetProperties(config))
                {
                    string value = reg.GetValue(pd.Name, null) as string;

                    if (value != null)
                        try
                        {
                            pd.SetValue(config, pd.Converter.ConvertFromInvariantString(value));
                        }
                        catch { }
                }
            }
        }

        /// <summary>
        /// Saves the supplied Config object
        /// </summary>
        /// <param name="config"></param>
        public void SaveConfig(VisualGitConfig config)
        {
            if (config == null)
                throw new ArgumentNullException("config");

            lock (_lock)
            {
                VisualGitConfig defaultConfig = new VisualGitConfig();
                SetDefaultsFromRegistry(defaultConfig);

                using (RegistryKey reg = OpenHKCUKey("Configuration"))
                {
                    PropertyDescriptorCollection pdc = TypeDescriptor.GetProperties(defaultConfig);
                    foreach (PropertyDescriptor pd in pdc)
                    {
                        object value = pd.GetValue(config);

                        // Set the value only if it is already set previously, or if it's different from the default
                        if (!pd.ShouldSerializeValue(config) && !pd.ShouldSerializeValue(defaultConfig))
                        {
                            reg.DeleteValue(pd.Name, false);
                        }
                        else
                            reg.SetValue(pd.Name, pd.Converter.ConvertToInvariantString(value));
                    }
                }
            }
        }

        RegistryKey OpenHKLMCommonKey(string subKey)
        {
            if (string.IsNullOrEmpty(subKey))
                throw new ArgumentNullException("subKey");

            // Opens the specified key or returns null
            return Registry.LocalMachine.OpenSubKey("SOFTWARE\\VisualGit\\Global\\" + subKey, RegistryKeyPermissionCheck.ReadSubTree);
        }

        RegistryKey OpenHKLMKey(string subKey)
        {
            if (string.IsNullOrEmpty(subKey))
                throw new ArgumentNullException("subKey");

            // Opens the specified key or returns null
            return Registry.LocalMachine.OpenSubKey("SOFTWARE\\VisualGit\\" + Settings.RegistryHiveSuffix + "\\" + subKey, RegistryKeyPermissionCheck.ReadSubTree);
        }

        /// <summary>
        /// Opens or creates the HKCU key with the specified name
        /// </summary>
        /// <param name="subKey"></param>
        /// <returns></returns>
        RegistryKey OpenHKCUKey(string subKey)
        {
            if (string.IsNullOrEmpty(subKey))
                throw new ArgumentNullException("subKey");

            // Opens or creates the specified key
            return Registry.CurrentUser.CreateSubKey("SOFTWARE\\VisualGit\\" + Settings.RegistryHiveSuffix + "\\" + subKey);
        }

        #region IVisualGitConfigurationService Members

        RegistryKey IVisualGitConfigurationService.OpenUserInstanceKey(string subKey)
        {
            return OpenHKCUKey(subKey);
        }

        RegistryKey IVisualGitConfigurationService.OpenInstanceKey(string subKey)
        {
            return OpenHKLMKey(subKey);
        }

        RegistryKey IVisualGitConfigurationService.OpenGlobalKey(string subKey)
        {
            return OpenHKLMCommonKey(subKey);
        }


        /// <summary>
        /// Gets the recent log messages.
        /// </summary>
        /// <returns></returns>
        public RegistryLifoList GetRecentLogMessages()
        {
            return new RegistryLifoList(Context, "RecentLogMessages", 32);
        }

        /// <summary>
        /// Gets the recent Repository Urls
        /// </summary>
        /// <returns></returns>
        public RegistryLifoList GetRecentReposUrls()
        {
            return new RegistryLifoList(Context, "RecentRepositoryUrls", 32);
        }

        public void SaveColumnsWidths(Type controlType, IDictionary<string, int> widths)
        {
            if(controlType == null)
                throw new ArgumentNullException("controlType");

            if (widths == null || widths.Count == 0)
                return;

            SaveNumberValues("ColumnWidths", controlType.FullName, widths);
        }

        public IDictionary<string, int> GetColumnWidths(Type controlType)
        {
            if (controlType == null)
                throw new ArgumentNullException("controlType");

            return GetNumberValues("ColumnWidths", controlType.FullName);
        }

        public void SaveWindowPlacement(Type controlType, IDictionary<string, int> placement)
        {
            if (controlType == null)
                throw new ArgumentNullException("controlType");

            SaveNumberValues("WindowPlacements", controlType.FullName, placement);
        }

        public IDictionary<string, int> GetWindowPlacement(Type controlType)
        {
            if (controlType == null)
                throw new ArgumentNullException("controlType");

            return GetNumberValues("WindowPlacements", controlType.FullName);
        }

        void SaveNumberValues(string regKey, string subKey, IDictionary<string, int> values)
        {
            if (string.IsNullOrEmpty(regKey))
                throw new ArgumentNullException("regKey");
            if (string.IsNullOrEmpty(subKey))
                throw new ArgumentNullException("subKey");
            if (values == null)
                throw new ArgumentNullException("values");

            lock (_lock)
            {
                subKey = regKey + "\\" + subKey;
                using (RegistryKey reg = OpenHKCUKey(subKey))
                {
                    if (reg == null)
                        return;
                    foreach (KeyValuePair<string, int> item in values)
                    {
                        if (item.Value <= 0)
                        {
                            reg.DeleteValue(item.Key, false);
                        }
                        else
                        {
                            reg.SetValue(item.Key, item.Value, RegistryValueKind.DWord);
                        }
                    }
                }
            }
        }

        IDictionary<string, int> GetNumberValues(string regKey, string subKey)
        {
            if (string.IsNullOrEmpty(regKey))
                throw new ArgumentNullException("regKey");
            if (string.IsNullOrEmpty(subKey))
                throw new ArgumentNullException("subKey");
            IDictionary<string, int> values;
            lock (_lock)
            {
                subKey = regKey + "\\" + subKey;
                using (RegistryKey reg = OpenHKCUKey(subKey))
                {
                    if (reg == null)
                        return null;
                    HybridCollection<string> hs = new HybridCollection<string>();
                    hs.AddRange(reg.GetValueNames());

                    values = new Dictionary<string, int>(hs.Count);

                    foreach (string item in hs)
                    {
                        int width;
                        if (RegistryUtils.TryGetIntValue(reg, item, out width) && width > 0)
                            values.Add(item, width);
                    }
                }
            }
            return values;
        }

		public bool GetWarningBool(VisualGitWarningBool visualGitWarningBool)
        {
            using (RegistryKey rk = OpenHKCUKey("Warnings\\Bools"))
            {
                if (rk == null)
                    return false;

                object v = rk.GetValue(visualGitWarningBool.ToString());

                if (!(v is int))
                    return false;

                return ((int)v) != 0;
            }
        }

        public void SetWarningBool(VisualGitWarningBool visualGitWarningBool, bool value)
        {
            using (RegistryKey rk = OpenHKCUKey("Warnings\\Bools"))
            {
                if (rk == null)
                    return;

                if (value)
                    rk.SetValue(visualGitWarningBool.ToString(), 1);
                else
                    rk.DeleteValue(visualGitWarningBool.ToString());
            }
        }

        public CredentialCacheItem GetCredentialCacheItem(string uri, string type, string promptText)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            string hash = GetCacheKey(type, uri, promptText);

            using (RegistryKey rk = OpenHKCUKey("Credentials"))
            {
                using (RegistryKey rksub = rk.OpenSubKey(hash))
                {
                    if (rksub == null)
                        return null;

                    return CreateCredentialCacheItemFromRegistryKey(rksub);
                }
            }
        }

        private CredentialCacheItem CreateCredentialCacheItemFromRegistryKey(RegistryKey rksub)
        {
            try
            {
                return new CredentialCacheItem(
                    (string)rksub.GetValue("Uri"),
                    (string)rksub.GetValue("Type"),
                    (string)rksub.GetValue("PromptText"),
                    Unprotect((string)rksub.GetValue("Response"))
                    );
            }
            catch
            {
                // Don't fail on incorrect credentials configuration
                // in the registry.
                return null;
            }
        }

        private string Protect(string value)
        {
            var result = ProtectedData.Protect(
                Encoding.UTF8.GetBytes(value ?? ""),
                _additionalEntropy,
                DataProtectionScope.CurrentUser
            );

            return Convert.ToBase64String(result);
        }

        private string Unprotect(string value)
        {
            var result = ProtectedData.Unprotect(
                Convert.FromBase64String(value),
                _additionalEntropy,
                DataProtectionScope.CurrentUser
            );

            return Encoding.UTF8.GetString(result);
        }

        private string GetCacheKey(string type, string uri, string promptText)
        {
            return ComputeHash((uri ?? "") + ":" + type + ":" + (promptText ?? ":"));
        }

        public void StoreCredentialCacheItem(CredentialCacheItem item)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            if (item.Type == null)
                throw new ArgumentNullException("item.Type");

            string hash = GetCacheKey(item.Type, item.Uri, item.PromptText);

            using (RegistryKey rk = OpenHKCUKey("Credentials\\" + hash))
            {
                rk.SetValue("Uri", item.Uri);
                rk.SetValue("Type", item.Type);
                rk.SetValue("PromptText", item.PromptText);
                rk.SetValue("Response", Protect(item.Response));
            }
        }

        string ComputeHash(string input)
        {
            using(SHA256 hasher = new SHA256Managed())
            {
                return ToHex(
                    hasher.ComputeHash(
                        Encoding.UTF8.GetBytes(input)
                    )
                );
            }
        }

        string ToHex(byte[] value)
        {
            var sb = new StringBuilder(value.Length * 2);

            foreach (byte b in value)
            {
                sb.Append(b.ToString("x2"));
            }

            return sb.ToString();
        }

        public ICollection<CredentialCacheItem> GetAllCredentialCacheItems()
        {
            List<CredentialCacheItem> items = new List<CredentialCacheItem>();

            using (RegistryKey rk = OpenHKCUKey("Credentials"))
            {
                foreach (string subKeyName in rk.GetSubKeyNames())
                {
                    using (RegistryKey rksub = rk.OpenSubKey(subKeyName))
                    {
                        CredentialCacheItem item = CreateCredentialCacheItemFromRegistryKey(rksub);

                        if (item != null)
                            items.Add(item);
                    }
                }
            }

            return items;
        }

        public void RemoveCredentialCacheItem(string uri, string type, string promptText)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            using (RegistryKey rk = OpenHKCUKey("Credentials"))
            {
                string hash = GetCacheKey(type, uri, promptText);

                rk.DeleteSubKey(hash, false);
            }
        }

        public void StoreCertificate(GitCertificate item)
        {
            // Ensure the certificate doesn't exist yet.

            RemoveCertificate(item.Path);

            // Add the new one.

            int largestIndex = 0;

            using (RegistryKey rk = OpenHKCUKey("Certificates"))
            {
                foreach (string name in rk.GetValueNames())
                {
                    int index;

                    if (int.TryParse(name, out index))
                        largestIndex = Math.Max(largestIndex, index);
                }

                rk.SetValue((largestIndex + 1).ToString(), item.Path);
            }

            // Update the SharpGit registrations.

            RegisterCertificates();
        }

        public ICollection<GitCertificate> GetAllCertificates()
        {
            List<GitCertificate> items = new List<GitCertificate>();

            using (RegistryKey rk = OpenHKCUKey("Certificates"))
            {
                foreach (string name in rk.GetValueNames())
                {
                    string value = (string)rk.GetValue(name);

                    if (!String.IsNullOrEmpty(value))
                        items.Add(new GitCertificate(value));
                }
            }

            return items;
        }

        public void RemoveCertificate(string path)
        {
            using (RegistryKey rk = OpenHKCUKey("Certificates"))
            {
                List<string> toRemove = new List<string>();

                foreach (string name in rk.GetValueNames())
                {
                    string value = (string)rk.GetValue(name);

                    if (
                        !String.IsNullOrEmpty(value) &&
                        String.Equals(value, path, FileSystemUtil.StringComparison)
                    )
                        toRemove.Add(name);
                }

                foreach (string name in toRemove)
                {
                    rk.DeleteValue(name);
                }
            }

            // Update the SharpGit registrations.

            RegisterCertificates();
        }

        public void RegisterCertificates()
        {
            // Remove all current certificates.

            foreach (GitCertificate item in GitCertificates.GetAllCertificates())
            {
                GitCertificates.RemoveCertificate(item);
            }

            // All all certificates we know of.

            foreach (GitCertificate item in GetAllCertificates())
            {
                GitCertificates.AddCertificate(item);
            }
        }

        #endregion
    }
}
