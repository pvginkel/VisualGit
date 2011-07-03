using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NGit;

namespace SharpGit
{
    internal class GitConfigWrapper : IGitConfig
    {
        private Config _config;

        public GitConfigWrapper(Config config)
        {
            if (config == null)
                throw new ArgumentNullException("config");

            _config = config;
        }

        public int GetInt(string section, string name, int defaultValue)
        {
            return _config.GetInt(section, name, defaultValue);
        }

        public int GetInt(string section, string subsection, string name, int defaultValue)
        {
            return _config.GetInt(section, subsection, name, defaultValue);
        }

        public long GetLong(string section, string name, long defaultValue)
        {
            return _config.GetLong(section, name, defaultValue);
        }

        public long GetLong(string section, string subsection, string name, long defaultValue)
        {
            return _config.GetLong(section, subsection, name, defaultValue);
        }

        public bool GetBoolean(string section, string name, bool defaultValue)
        {
            return _config.GetBoolean(section, name, defaultValue);
        }

        public bool GetBoolean(string section, string subsection, string name, bool defaultValue)
        {
            return _config.GetBoolean(section, subsection, name, defaultValue);
        }

        public T GetEnum<T>(string section, string subsection, string name, T defaultValue)
        {
            return _config.GetEnum<T>(section, subsection, name, defaultValue);
        }

        public T GetEnum<T>(Array all, string section, string subsection, string name, T defaultValue)
        {
            return _config.GetEnum<T>(section, subsection, name, defaultValue);
        }

        public string GetString(string section, string subsection, string name)
        {
            return _config.GetString(section, subsection, name);
        }

        public string[] GetStringList(string section, string subsection, string name)
        {
            return _config.GetStringList(section, subsection, name);
        }

        public ICollection<string> GetSubsections(string section)
        {
            return _config.GetSubsections(section);
        }

        public ICollection<string> GetSections()
        {
            return _config.GetSections();
        }

        public ICollection<string> GetNames(string section)
        {
            return _config.GetNames(section);
        }

        public ICollection<string> GetNames(string section, string subsection)
        {
            return _config.GetNames(section, subsection);
        }


        public void SetInt(string section, string subsection, string name, int value)
        {
            _config.SetInt(section, subsection, name, value);
        }

        public void SetLong(string section, string subsection, string name, long value)
        {
            _config.SetLong(section, subsection, name, value);
        }

        public void SetBoolean(string section, string subsection, string name, bool value)
        {
            _config.SetBoolean(section, subsection, name, value);
        }

        public void SetEnum<T>(string section, string subsection, string name, T value)
        {
            _config.SetEnum<T>(section, subsection, name, value);
        }

        public void SetString(string section, string subsection, string name, string value)
        {
            _config.SetString(section, subsection, name, value);
        }

        public void SetStringList(string section, string subsection, string name, IList<string> values)
        {
            _config.SetStringList(section, subsection, name, values);
        }

        public void Unset(string section, string subsection, string name)
        {
            _config.Unset(section, subsection, name);
        }

        public void UnsetSection(string section, string subsection)
        {
            _config.UnsetSection(section, subsection);
        }

        public bool CanSave
        {
            get { return _config is StoredConfig; }
        }

        public void Save()
        {
            if (!CanSave)
                throw new InvalidOperationException("Config cannot be saved");

            ((StoredConfig)_config).Save();
        }
    }
}
