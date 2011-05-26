using System;
using System.Collections.Generic;
using System.Text;
using VisualGit;
using VisualGit.VS;
using Microsoft.Win32;
using Microsoft.VisualStudio.Shell;
using VisualGit.Commands;
using VisualGit.UI;

namespace VisualGit.Services
{
	[GlobalService(typeof(IVisualGitMigrationService))]
	class MigrationService : VisualGitService, IVisualGitMigrationService
	{
		public MigrationService(IVisualGitServiceProvider context)
			: base(context)
		{
		}

		const string MigrateId = "MigrateId";
		public void MaybeMigrate()
		{
			IVisualGitPackage pkg = GetService<IVisualGitPackage>();
			IVisualGitCommandService cs = GetService<IVisualGitCommandService>();

			if (pkg == null || cs == null)
				return;

			using (RegistryKey rkRoot = pkg.UserRegistryRoot)
			using (RegistryKey visualGitMigration = rkRoot.CreateSubKey("VisualGit-Trigger"))
			{
				int migrateFrom = 0;
				object value = visualGitMigration.GetValue(MigrateId, migrateFrom);

				if (value is int)
					migrateFrom = (int)value;
				else
					visualGitMigration.DeleteValue(MigrateId, false);

				if (migrateFrom < 0)
					migrateFrom = 0;

				if (migrateFrom >= VisualGitId.MigrateVersion)
					return; // Nothing to do

				try
				{
					if (cs.DirectlyExecCommand(VisualGitCommand.MigrateSettings).Success)
					{
						visualGitMigration.SetValue(MigrateId, VisualGitId.MigrateVersion);
					}
				}
				catch
				{ /* NOP: Don't fail here... ever! */}
			}
		}
	}
}
