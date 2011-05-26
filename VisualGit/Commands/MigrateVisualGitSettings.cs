namespace VisualGit.Commands
{
    [Command(VisualGitCommand.MigrateSettings, AlwaysAvailable = true)]
    class MigrateVisualGitSettings : CommandBase
    {
        public override void OnUpdate(CommandUpdateEventArgs e)
        {
            // Always available
        }

        public override void OnExecute(CommandEventArgs e)
        {
            //int upgradeFrom = 0;
            bool incremental = false;
            if (e.Argument is int)
            {
                incremental = true;
                //upgradeFrom = (int)e.Argument;
            }

            Migrate.Cleanup.RemoveOldUI(e.Context, !incremental);
        }
    }
}
