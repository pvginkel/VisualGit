// VisualGit\Services\MigrationService.cs
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
