using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace SharpGit.Implementation
{
    public class GitLibrary
    {
        internal GitLibrary(Assembly assembly)
        {
            Name = assembly.GetName().Name;
            VersionString = assembly.GetName().Version.ToString();
        }

        public string Name { get; private set; }

        public string VersionString { get; private set; }
    }
}
