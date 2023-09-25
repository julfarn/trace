namespace TraceUI
{
    partial class ElementList
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

        #region Vom Komponenten-Designer generierter Code

        /// <summary> 
        /// Erforderliche Methode für die Designerunterstützung. 
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.listPanel = new TraceUI.DocumentLayoutPanel();
            this.SuspendLayout();
            // 
            // listPanel
            // 
            this.listPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listPanel.BackColor = System.Drawing.Color.White;
            this.listPanel.VerticalSpace = 5;
            this.listPanel.Location = new System.Drawing.Point(3, 3);
            this.listPanel.Name = "listPanel";
            this.listPanel.Size = new System.Drawing.Size(165, 32);
            this.listPanel.TabIndex = 0;
            // 
            // DefinitionList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.Controls.Add(this.listPanel);
            this.Name = "DefinitionList";
            this.Size = new System.Drawing.Size(202, 38);
            this.ResumeLayout(false);

        }

        #endregion

        private DocumentLayoutPanel listPanel;
    }
}
