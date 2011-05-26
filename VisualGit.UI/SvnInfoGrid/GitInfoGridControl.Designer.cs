namespace VisualGit.UI.SvnInfoGrid
{
	partial class GitInfoGridControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GitInfoGridControl));
			System.Windows.Forms.ToolStripSystemRenderer toolStripSystemRenderer1 = new System.Windows.Forms.ToolStripSystemRenderer();
            this.grid = new VisualGit.UI.SvnInfoGrid.InfoPropertyGrid();
			this.SuspendLayout();
			// 
			// grid
			// 
			resources.ApplyResources(this.grid, "grid");
			this.grid.Name = "grid";
			this.grid.ToolStripRenderer = toolStripSystemRenderer1;
			// 
			// GitInfoGridControl
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.grid);
			this.Name = "GitInfoGridControl";
			this.ResumeLayout(false);

		}

		#endregion

        private VisualGit.UI.SvnInfoGrid.InfoPropertyGrid grid;
	}
}
