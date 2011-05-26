using System;
using System.Collections.Generic;
using System.Text;
using MsVsShell = Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell;

namespace VisualGit.VSPackage.Attributes
{
	internal sealed class ProvideUIVersionAttribute : MsVsShell.RegistrationAttribute
    {
        public ProvideUIVersionAttribute()
        {
        }

        internal const string RemapName = "VisualGit-UI-Version";
        string GetPath(RegistrationAttribute.RegistrationContext context)
        {
            return "Packages\\" + context.ComponentType.GUID.ToString("B").ToUpperInvariant();
        }

        public override void Register(RegistrationAttribute.RegistrationContext context)
        {
            // Create the visibility key.
            using (Key childKey = context.CreateKey(GetPath(context)))
            {
                // Set the value for the command UI guid.
                if (context.GetType().Name.ToUpperInvariant().Contains("PKGDEF"))
                    childKey.SetValue(RemapName, new System.Reflection.AssemblyName(context.ComponentType.Assembly.FullName).Version.ToString());
                else
                    childKey.SetValue(RemapName, "[ProductVersion]");
            }
        }

        public override void Unregister(Microsoft.VisualStudio.Shell.RegistrationAttribute.RegistrationContext context)
        {
            context.RemoveKey(GetPath(context));
        }
    }
}
