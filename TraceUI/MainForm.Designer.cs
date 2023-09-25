using System;
using System.Windows.Forms;

namespace TraceUI
{
    partial class MainForm
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.NewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveXMLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.latexToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.DocumentPanel = new TraceUI.DocumentLayoutPanel();
            this.PagePanel = new NoStupidScrollPanel();
            this.OpenDialog = new System.Windows.Forms.OpenFileDialog();
            this.SaveDialog = new System.Windows.Forms.SaveFileDialog();
            this.SaveXMLDialog = new System.Windows.Forms.SaveFileDialog();
            this.menuStrip1.SuspendLayout();
            this.PagePanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(721, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.NewToolStripMenuItem,
            this.openToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.saveXMLToolStripMenuItem,
            this.latexToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // NewToolStripMenuItem
            // 
            this.NewToolStripMenuItem.Name = "NewToolStripMenuItem";
            this.NewToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.NewToolStripMenuItem.Text = "New";
            this.NewToolStripMenuItem.Click += new System.EventHandler(this.NewToolStripMenuItem_Click);
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.saveToolStripMenuItem.Text = "Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // saveXMLToolStripMenuItem
            // 
            this.saveXMLToolStripMenuItem.Name = "saveXMLToolStripMenuItem";
            this.saveXMLToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.saveXMLToolStripMenuItem.Text = "Save XML";
            this.saveXMLToolStripMenuItem.Click += new System.EventHandler(this.saveXMLToolStripMenuItem_Click);
            // 
            // latexToolStripMenuItem
            // 
            this.latexToolStripMenuItem.Name = "latexToolStripMenuItem";
            this.latexToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.latexToolStripMenuItem.Text = "Export LaTeX";
            this.latexToolStripMenuItem.Click += new System.EventHandler(this.latexToolStripMenuItem_Click);
            // 
            // DocumentPanel
            // 
            this.DocumentPanel.BackColor = System.Drawing.Color.White;
            this.DocumentPanel.VerticalSpace = 5;
            this.DocumentPanel.ListenToVisibility = true;
            this.DocumentPanel.Location = new System.Drawing.Point(0, 3);
            this.DocumentPanel.Name = "DocumentPanel";
            this.DocumentPanel.Size = new System.Drawing.Size(689, 536);
            this.DocumentPanel.TabIndex = 1;
            this.DocumentPanel.HeightChanged += new System.EventHandler(this.DocumentPanel_HeightChanged);
            // 
            // PagePanel
            // 
            this.PagePanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PagePanel.AutoScroll = true;
            this.PagePanel.AutoScrollMinSize = new System.Drawing.Size(0, 1000);
            this.PagePanel.Controls.Add(this.DocumentPanel);
            this.PagePanel.Location = new System.Drawing.Point(0, 27);
            this.PagePanel.Name = "PagePanel";
            this.PagePanel.Size = new System.Drawing.Size(721, 717);
            this.PagePanel.TabIndex = 2;
            this.PagePanel.SizeChanged += new System.EventHandler(this.PagePanel_SizeChanged);
            // 
            // OpenDialog
            // 
            this.OpenDialog.DefaultExt = "tr";
            this.OpenDialog.Filter = "Trace Documents|*.tr;*.xtr|All Files|*.*";
            this.OpenDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.OpenDialog_FileOk);
            // 
            // SaveDialog
            // 
            this.SaveDialog.DefaultExt = "tr";
            this.SaveDialog.Filter = "Trace Documents|*.tr|All Files|*.*";
            // 
            // SaveXMLDialog
            // 
            this.SaveXMLDialog.DefaultExt = "xtr";
            this.SaveXMLDialog.Filter = "XML Trace Documents|*.xtr|All Files|*.*";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(721, 745);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.PagePanel);
            this.MainMenuStrip = this.menuStrip1;
            this.KeyPreview = true;
            this.Name = "MainForm";
            this.Text = "Trace";
            this.Icon = TraceUI.Properties.Resources.logo_04;
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.SizeChanged += new System.EventHandler(this.MainForm_SizeChanged);
            this.Click += new System.EventHandler(this.MainForm_Click);
            this.Activated += new System.EventHandler(this.MainForm_GotFocus);
            this.Deactivate += new System.EventHandler(this.MainForm_LostFocus);
            this.FormClosing += new FormClosingEventHandler(this.MainForm_Closing);
            this.KeyDown += new KeyEventHandler(this.MainForm_KeyDown);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.PagePanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
            
        }

        private void pictureBox1_MouseWheel(object sender, MouseEventArgs e)
        {
            throw new NotImplementedException();
        }

        #endregion

        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem openToolStripMenuItem;
        private ToolStripMenuItem saveToolStripMenuItem;
        private ToolStripMenuItem saveXMLToolStripMenuItem;
        private ToolStripMenuItem latexToolStripMenuItem;
        private ToolStripMenuItem NewToolStripMenuItem;
        private DocumentLayoutPanel DocumentPanel;
        private NoStupidScrollPanel PagePanel;
        private OpenFileDialog OpenDialog;
        private SaveFileDialog SaveDialog;
        private SaveFileDialog SaveXMLDialog;
    }
}

