namespace TraceUI
{
    partial class VisualisationDisplay
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
            this.pB = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pB)).BeginInit();
            this.SuspendLayout();
            // 
            // pB
            // 
            this.pB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pB.Location = new System.Drawing.Point(0, 0);
            this.pB.Name = "pB";
            this.pB.Size = new System.Drawing.Size(192, 87);
            this.pB.TabIndex = 0;
            this.pB.TabStop = false;
            this.pB.MouseEnter += new System.EventHandler(this.pB_MouseEnter);
            this.pB.MouseLeave += new System.EventHandler(this.pB_MouseLeave);
            this.pB.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pB_MouseMove);
            // 
            // VisualisationDisplay
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.pB);
            this.Name = "VisualisationDisplay";
            this.Size = new System.Drawing.Size(192, 87);
            this.Load += new System.EventHandler(this.VisualisationDisplay_Load);
            this.SizeChanged += new System.EventHandler(this.VisualisationDisplay_SizeChanged);
            this.Click += new System.EventHandler(this.VisualisationDisplay_Click);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.VisualisationDisplay_Paint);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.VisualisationDisplay_KeyDown);
            this.MouseLeave += new System.EventHandler(this.VisualisationDisplay_MouseLeave);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.VisualisationDisplay_MouseMove);
            ((System.ComponentModel.ISupportInitialize)(this.pB)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pB;
    }
}
