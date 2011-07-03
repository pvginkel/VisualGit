// SharpGit\IGitConfig.cs
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
