namespace VisualGit.UI.PendingChanges
{
    partial class PendingCommitsPage
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PendingCommitsPage));
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.topLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.logMessageEditor = new VisualGit.UI.PendingChanges.LogMessageEditor(this.components);
            this.pendingCommits = new VisualGit.UI.PendingChanges.Commits.PendingCommitsView(this.components);
            this.label1 = new System.Windows.Forms.Label();
            this.lastRevLabel = new System.Windows.Forms.Label();
            this.lastRevBox = new System.Windows.Forms.TextBox();
            this.amendBox = new System.Windows.Forms.CheckBox();
            this.pathColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.projectColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.changeColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.fullPathColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.topLayoutPanel.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer
            // 
            resources.ApplyResources(this.splitContainer, "splitContainer");
            this.splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.topLayoutPanel);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.pendingCommits);
            // 
            // topLayoutPanel
            // 
            resources.ApplyResources(this.topLayoutPanel, "topLayoutPanel");
            this.topLayoutPanel.Controls.Add(this.panel1, 0, 1);
            this.topLayoutPanel.Controls.Add(this.label1, 0, 0);
            this.topLayoutPanel.Controls.Add(this.lastRevLabel, 1, 0);
            this.topLayoutPanel.Controls.Add(this.lastRevBox, 2, 0);
            this.topLayoutPanel.Controls.Add(this.amendBox, 3, 0);
            this.topLayoutPanel.Name = "topLayoutPanel";
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.topLayoutPanel.SetColumnSpan(this.panel1, 4);
            this.panel1.Controls.Add(this.logMessageEditor);
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Name = "panel1";
            // 
            // logMessageEditor
            // 
            resources.ApplyResources(this.logMessageEditor, "logMessageEditor");
            this.logMessageEditor.HideHorizontalScrollBar = true;
            this.logMessageEditor.Name = "logMessageEditor";
            this.logMessageEditor.PasteSource = this.pendingCommits;
            // 
            // pendingCommits
            // 
            this.pendingCommits.AllowColumnReorder = true;
            this.pendingCommits.CheckBoxes = true;
            resources.ApplyResources(this.pendingCommits, "pendingCommits");
            this.pendingCommits.HideSelection = false;
            this.pendingCommits.Name = "pendingCommits";
            this.pendingCommits.ShowItemToolTips = true;
            this.pendingCommits.ShowSelectAllCheckBox = true;
            this.pendingCommits.ResolveItem += new System.EventHandler<VisualGit.UI.VSSelectionControls.ListViewWithSelection<VisualGit.UI.PendingChanges.Commits.PendingCommitItem>.ResolveItemEventArgs>(this.pendingCommits_ResolveItem);
            this.pendingCommits.KeyUp += new System.Windows.Forms.KeyEventHandler(this.pendingCommits_KeyUp);
            this.pendingCommits.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.pendingCommits_MouseDoubleClick);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // lastRevLabel
            // 
            resources.ApplyResources(this.lastRevLabel, "lastRevLabel");
            this.lastRevLabel.Name = "lastRevLabel";
            this.lastRevLabel.SizeChanged += new System.EventHandler(this.lastRevLabel_SizeChanged);
            // 
            // lastRevBox
            // 
            this.lastRevBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.lastRevBox, "lastRevBox");
            this.lastRevBox.MaximumSize = new System.Drawing.Size(320, 32767);
            this.lastRevBox.Name = "lastRevBox";
            this.lastRevBox.ReadOnly = true;
            // 
            // amendBox
            // 
            resources.ApplyResources(this.amendBox, "amendBox");
            this.amendBox.Name = "amendBox";
            this.amendBox.UseVisualStyleBackColor = true;
            // 
            // PendingCommitsPage
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer);
            this.Name = "PendingCommitsPage";
            this.Load += new System.EventHandler(this.PendingCommitsPage_Load);
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.topLayoutPanel.ResumeLayout(false);
            this.topLayoutPanel.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ColumnHeader pathColumn;
        private System.Windows.Forms.ColumnHeader projectColumn;
        private System.Windows.Forms.ColumnHeader changeColumn;
        private System.Windows.Forms.ColumnHeader fullPathColumn;
        private System.Windows.Forms.TableLayoutPanel topLayoutPanel;
        private System.Windows.Forms.Label label1;
        private VisualGit.UI.PendingChanges.LogMessageEditor logMessageEditor;
        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.Panel panel1;
        private VisualGit.UI.PendingChanges.Commits.PendingCommitsView pendingCommits;
        private System.Windows.Forms.Label lastRevLabel;
        private System.Windows.Forms.TextBox lastRevBox;
        private System.Windows.Forms.CheckBox amendBox;
    }
}
