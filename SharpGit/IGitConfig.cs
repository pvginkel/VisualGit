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
        long GetLong(string section, string name, long defaultValue);
        long GetLong(string section, string subsection, string name, long defaultValue);
        bool GetBoolean(string section, string name, bool defaultValue);
        bool GetBoolean(string section, string subsection, string name, bool defaultValue);
        T GetEnum<T>(string section, string subsection, string name, T defaultValue);
        T GetEnum<T>(Array all, string section, string subsection, string name, T defaultValue);
        string GetString(string section, string subsection, string name);
        string[] GetStringList(string section, string subsection, string name);
        ICollection<string> GetSubsections(string section);
        ICollection<string> GetSections();
        ICollection<string> GetNames(string section);
        ICollection<string> GetNames(string section, string subsection);
    }
}
