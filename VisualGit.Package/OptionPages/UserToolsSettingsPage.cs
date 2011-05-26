using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.VisualStudio.Shell;
using VisualGit.UI;
using VisualGit.UI.OptionsPages;

namespace VisualGit.VSPackage
{
    [Guid(VisualGitId.UserToolsSettingsPageGuid)]
    class UserToolsSettingsPage : DialogPage
    {
        UserToolSettingsControl _control;
        protected override System.Windows.Forms.IWin32Window Window
        {
            get
            {
                return Control;
            }
        }

        UserToolSettingsControl Control
        {
            get
            {
                return _control ?? (_control = CreateControl());
            }
        }

        UserToolSettingsControl CreateControl()
        {
            UserToolSettingsControl control = new UserToolSettingsControl();
            IVisualGitServiceProvider sp = (IVisualGitServiceProvider)GetService(typeof(IVisualGitServiceProvider));

            if (sp != null)
                control.Context = sp;

            return control;
        }

        public override void LoadSettingsFromStorage()
        {
            base.LoadSettingsFromStorage();

            Control.LoadSettings();
        }

        public override void SaveSettingsToStorage()
        {
            base.SaveSettingsToStorage();

            Control.SaveSettings();

        }

        public override void ResetSettings()
        {
            base.ResetSettings();

            IVisualGitServiceProvider sp = (IVisualGitServiceProvider)GetService(typeof(IVisualGitServiceProvider));
            if (sp != null)
            {
                IVisualGitConfigurationService cfgSvc = sp.GetService<IVisualGitConfigurationService>();
                cfgSvc.LoadDefaultConfig();
            }
        }
    }
}
