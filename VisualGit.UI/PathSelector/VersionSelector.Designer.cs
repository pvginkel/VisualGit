using System;
using System.Collections.Generic;
using System.Text;

namespace VisualGit.UI.PathSelector
{
    partial class VersionSelector
    {

        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.typeCombo = new System.Windows.Forms.ComboBox();
            this.typeLabel = new System.Windows.Forms.Label();
            this.versionTypePanel = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // typeCombo
            // 
            this.typeCombo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.typeCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.typeCombo.FormattingEnabled = true;
            this.typeCombo.Location = new System.Drawing.Point(40, 3);
            this.typeCombo.Name = "typeCombo";
            this.typeCombo.Size = new System.Drawing.Size(42, 21);
            this.typeCombo.TabIndex = 1;
            this.typeCombo.SelectedValueChanged += new System.EventHandler(this.typeCombo_SelectedValueChanged);
            // 
            // typeLabel
            // 
            this.typeLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.typeLabel.Location = new System.Drawing.Point(0, 0);
            this.typeLabel.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.typeLabel.Name = "typeLabel";
            this.typeLabel.Size = new System.Drawing.Size(34, 27);
            this.typeLabel.TabIndex = 0;
            this.typeLabel.Text = "&Type:";
            this.typeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // versionTypePanel
            // 
            this.versionTypePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.versionTypePanel.Location = new System.Drawing.Point(85, 0);
            this.versionTypePanel.Margin = new System.Windows.Forms.Padding(0);
            this.versionTypePanel.Name = "versionTypePanel";
            this.versionTypePanel.Size = new System.Drawing.Size(115, 27);
            this.versionTypePanel.TabIndex = 2;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.tableLayoutPanel1.Controls.Add(this.typeLabel, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.versionTypePanel, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.typeCombo, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(200, 27);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // VersionSelector
            // 
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "VersionSelector";
            this.Size = new System.Drawing.Size(200, 27);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;
        private System.Windows.Forms.ComboBox typeCombo;
        private System.Windows.Forms.Label typeLabel;
        private System.Windows.Forms.Panel versionTypePanel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}
