namespace TraceUI
{
    partial class ElementDisplay
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
            this.visualisationDisplay = new TraceUI.VisualisationDisplay();
            this.label = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // visualisationDisplay
            // 
            this.visualisationDisplay.BackColor = System.Drawing.Color.White;
            this.visualisationDisplay.Editable = false;
            this.visualisationDisplay.interactionMode = TraceUI.VisualisationDisplay.InteractionMode.passive;
            this.visualisationDisplay.Location = new System.Drawing.Point(0, 0);
            this.visualisationDisplay.minRestTime = 1000;
            this.visualisationDisplay.Name = "visualisationDisplay";
            this.visualisationDisplay.Size = new System.Drawing.Size(104, 56);
            this.visualisationDisplay.TabIndex = 0;
            this.visualisationDisplay.Visualisation = null;
            this.visualisationDisplay.SizeChanged += new System.EventHandler(this.visualisationDisplay_SizeChanged);
            // 
            // label
            // 
            this.label.AutoSize = true;
            this.label.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label.Location = new System.Drawing.Point(134, 22);
            this.label.Name = "label";
            this.label.Size = new System.Drawing.Size(29, 13);
            this.label.TabIndex = 1;
            this.label.Text = "label";
            this.label.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label.TextChanged += new System.EventHandler(this.label_TextChanged);
            this.label.MouseHover += new System.EventHandler(this.label_MouseHover);
            // 
            // ElementDisplay
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.label);
            this.Controls.Add(this.visualisationDisplay);
            this.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Name = "ElementDisplay";
            this.Size = new System.Drawing.Size(166, 59);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private VisualisationDisplay visualisationDisplay;
        private System.Windows.Forms.Label label;
    }
}
