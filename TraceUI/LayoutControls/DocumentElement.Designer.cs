namespace TraceUI
{
    partial class DocumentElement
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
            this.labelTitle = new System.Windows.Forms.Label();
            this.expanderButton = new System.Windows.Forms.PictureBox();
            this.titleVisDisplay = new TraceUI.VisualisationDisplay();
            this.layoutPanel = new TraceUI.DocumentLayoutPanel();
            this.bt_delete = new System.Windows.Forms.Button();
            this.bt_hide = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.expanderButton)).BeginInit();
            this.SuspendLayout();
            // 
            // labelTitle
            // 
            this.labelTitle.AutoSize = true;
            this.labelTitle.Font = new System.Drawing.Font("Garamond", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTitle.Location = new System.Drawing.Point(18, 6);
            this.labelTitle.Name = "labelTitle";
            this.labelTitle.Size = new System.Drawing.Size(40, 18);
            this.labelTitle.TabIndex = 1;
            this.labelTitle.Text = "Title";
            // 
            // expanderButton
            // 
            this.expanderButton.Dock = System.Windows.Forms.DockStyle.Left;
            this.expanderButton.Location = new System.Drawing.Point(0, 0);
            this.expanderButton.Name = "expanderButton";
            this.expanderButton.Size = new System.Drawing.Size(15, 40);
            this.expanderButton.TabIndex = 3;
            this.expanderButton.TabStop = false;
            this.expanderButton.Click += new System.EventHandler(this.expanderButton_Click_1);
            this.expanderButton.DoubleClick += new System.EventHandler(this.expanderButton_Click_1);
            this.expanderButton.MouseDown += new System.Windows.Forms.MouseEventHandler(this.expanderButton_MouseDown);
            // 
            // titleVisDisplay
            // 
            this.titleVisDisplay.BackColor = System.Drawing.Color.White;
            this.titleVisDisplay.Editable = false;
            this.titleVisDisplay.interactionMode = TraceUI.VisualisationDisplay.InteractionMode.subVis;
            this.titleVisDisplay.LinkOnly = false;
            this.titleVisDisplay.Location = new System.Drawing.Point(21, 6);
            this.titleVisDisplay.minRestTime = 50;
            this.titleVisDisplay.Name = "titleVisDisplay";
            this.titleVisDisplay.Restriction = TraceUI.ElementType.Any;
            this.titleVisDisplay.Size = new System.Drawing.Size(71, 18);
            this.titleVisDisplay.TabIndex = 4;
            this.titleVisDisplay.Visible = false;
            this.titleVisDisplay.Visualisation = null;
            // 
            // layoutPanel
            // 
            this.layoutPanel.BackColor = System.Drawing.Color.White;
            this.layoutPanel.ListenToVisibility = true;
            this.layoutPanel.Location = new System.Drawing.Point(21, 27);
            this.layoutPanel.Name = "layoutPanel";
            this.layoutPanel.Size = new System.Drawing.Size(69, 10);
            this.layoutPanel.TabIndex = 5;
            this.layoutPanel.VerticalSpace = 5;
            // 
            // bt_delete
            // 
            this.bt_delete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bt_delete.Location = new System.Drawing.Point(84, 1);
            this.bt_delete.Name = "bt_delete";
            this.bt_delete.Size = new System.Drawing.Size(20, 20);
            this.bt_delete.TabIndex = 6;
            this.bt_delete.Text = "x";
            this.bt_delete.UseVisualStyleBackColor = true;
            this.bt_delete.Click += new System.EventHandler(this.bt_delete_Click);
            // 
            // bt_hide
            // 
            this.bt_hide.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bt_hide.Location = new System.Drawing.Point(64, 1);
            this.bt_hide.Name = "bt_hide";
            this.bt_hide.Size = new System.Drawing.Size(20, 20);
            this.bt_hide.TabIndex = 7;
            this.bt_hide.Text = "-";
            this.bt_hide.UseVisualStyleBackColor = true;
            this.bt_hide.Click += new System.EventHandler(this.bt_hide_Click);
            // 
            // DocumentElement
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.bt_hide);
            this.Controls.Add(this.bt_delete);
            this.Controls.Add(this.layoutPanel);
            this.Controls.Add(this.expanderButton);
            this.Controls.Add(this.labelTitle);
            this.Controls.Add(this.titleVisDisplay);
            this.Name = "DocumentElement";
            this.Size = new System.Drawing.Size(107, 40);
            this.VisibleChanged += new System.EventHandler(this.DocumentElement_VisibleChanged);
            ((System.ComponentModel.ISupportInitialize)(this.expanderButton)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.PictureBox expanderButton;
        private VisualisationDisplay titleVisDisplay;
        protected System.Windows.Forms.Label labelTitle;
        internal DocumentLayoutPanel layoutPanel;
        private System.Windows.Forms.Button bt_delete;
        private System.Windows.Forms.Button bt_hide;
    }
}
