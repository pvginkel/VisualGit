using VisualGit.UI.PendingChanges;

namespace VisualGit.UI.Annotate
{
    partial class AnnotateEditorControl
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.blameMarginControl1 = new VisualGit.UI.Annotate.AnnotateMarginControl();
            this.editor = new VisualGit.UI.VSTextEditor(this.components);
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.blameMarginControl1);
            this.splitContainer1.Panel1MinSize = 100;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.editor);
            this.splitContainer1.Size = new System.Drawing.Size(500, 500);
            this.splitContainer1.SplitterDistance = 335;
            this.splitContainer1.SplitterWidth = 2;
            this.splitContainer1.TabIndex = 3;
            // 
            // blameMarginControl1
            // 
            this.blameMarginControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.blameMarginControl1.Location = new System.Drawing.Point(0, 0);
            this.blameMarginControl1.Name = "blameMarginControl1";
            this.blameMarginControl1.Size = new System.Drawing.Size(335, 300);
            this.blameMarginControl1.TabIndex = 1;
            this.blameMarginControl1.Text = "blameMarginControl1";
            this.blameMarginControl1.TipOwner = this;
            // 
            // editor
            // 
            this.editor.BackColor = System.Drawing.SystemColors.Window;
            this.editor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.editor.Location = new System.Drawing.Point(0, 0);
            this.editor.Name = "editor";
            this.editor.Size = new System.Drawing.Size(183, 300);
            this.editor.TabIndex = 2;
            this.editor.DisableWordWrap = true;
            this.editor.VerticalTextScroll += new System.EventHandler<VisualGit.UI.VSTextEditorScrollEventArgs>(this.logMessageEditor1_VerticalScroll);
            // 
            // AnnotateEditorControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Name = "AnnotateEditorControl";
            this.Size = new System.Drawing.Size(300, 300);
            this.Text = " (Annotated)";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private AnnotateMarginControl blameMarginControl1;
        private VSTextEditor editor;
        private System.Windows.Forms.SplitContainer splitContainer1;
    }
}
