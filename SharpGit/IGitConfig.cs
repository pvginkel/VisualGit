using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    public interface IGitConfig
    {
        int GetInt(string section, string name, int defaultValue);
        int GetInt(string section, string subsection, string name, int defaultValue);
        void SetInt(string section, string subsection, string name, int value);
        long GetLong(string section, string name, long defaultValue);
        long GetLong(string section, string subsection, string name, long defaultValue);
        void SetLong(string section, string subsection, string name, long value);
        bool GetBoolean(string section, string name, bool defaultValue);
        bool GetBoolean(string section, string subsection, string name, bool defaultValue);
        void SetBoolean(string section, string subsection, string name, bool value);
        T GetEnum<T>(string section, string subsection, string name, T defaultValue);
        T GetEnum<T>(Array all, string section, string subsection, string name, T defaultValue);
        void SetEnum<T>(string section, string subsection, string name, T value);
        string GetString(string section, string subsection, string name);
        void SetString(string section, string subsection, string name, string value);
        string[] GetStringList(string section, string subsection, string name);
        void SetStringList(string section, string subsection, string name, IList<string> values);
        ICollection<string> GetSubsections(string section);
        ICollection<string> GetSections();
        ICollection<string> GetNames(string section);
        ICollection<string> GetNames(string section, string subsection);
        void Unset(string section, string subsection, string name);
        void UnsetSection(string section, string subsection);
        bool CanSave { get; }
        void Save();
    }
}
