namespace TraceUI
{
    partial class BinaryConnectiveProperties
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
            this.label_header = new System.Windows.Forms.Label();
            this.label_tt = new System.Windows.Forms.Label();
            this.label_tf = new System.Windows.Forms.Label();
            this.label_ft = new System.Windows.Forms.Label();
            this.label_ff = new System.Windows.Forms.Label();
            this.checkBox_tt = new System.Windows.Forms.CheckBox();
            this.checkBox_tf = new System.Windows.Forms.CheckBox();
            this.checkBox_ft = new System.Windows.Forms.CheckBox();
            this.checkBox_ff = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label_header
            // 
            this.label_header.AutoSize = true;
            this.label_header.Location = new System.Drawing.Point(0, 0);
            this.label_header.Name = "label_header";
            this.label_header.Size = new System.Drawing.Size(68, 13);
            this.label_header.TabIndex = 0;
            this.label_header.Text = "A  B  |  A ~ B";
            // 
            // label_tt
            // 
            this.label_tt.AutoSize = true;
            this.label_tt.Location = new System.Drawing.Point(3, 16);
            this.label_tt.Name = "label_tt";
            this.label_tt.Size = new System.Drawing.Size(33, 13);
            this.label_tt.TabIndex = 1;
            this.label_tt.Text = "t   t   |";
            // 
            // label_tf
            // 
            this.label_tf.AutoSize = true;
            this.label_tf.Location = new System.Drawing.Point(3, 35);
            this.label_tf.Name = "label_tf";
            this.label_tf.Size = new System.Drawing.Size(33, 13);
            this.label_tf.TabIndex = 2;
            this.label_tf.Text = "t   f   |";
            // 
            // label_ft
            // 
            this.label_ft.AutoSize = true;
            this.label_ft.Location = new System.Drawing.Point(3, 55);
            this.label_ft.Name = "label_ft";
            this.label_ft.Size = new System.Drawing.Size(33, 13);
            this.label_ft.TabIndex = 3;
            this.label_ft.Text = "f   t   |";
            // 
            // label_ff
            // 
            this.label_ff.AutoSize = true;
            this.label_ff.Location = new System.Drawing.Point(3, 75);
            this.label_ff.Name = "label_ff";
            this.label_ff.Size = new System.Drawing.Size(33, 13);
            this.label_ff.TabIndex = 4;
            this.label_ff.Text = "f   f   |";
            // 
            // checkBox_tt
            // 
            this.checkBox_tt.AutoSize = true;
            this.checkBox_tt.Location = new System.Drawing.Point(42, 16);
            this.checkBox_tt.Name = "checkBox_tt";
            this.checkBox_tt.Size = new System.Drawing.Size(15, 14);
            this.checkBox_tt.TabIndex = 5;
            this.checkBox_tt.UseVisualStyleBackColor = true;
            this.checkBox_tt.CheckedChanged += new System.EventHandler(this.checkBox_tt_CheckedChanged);
            // 
            // checkBox_tf
            // 
            this.checkBox_tf.AutoSize = true;
            this.checkBox_tf.Location = new System.Drawing.Point(42, 35);
            this.checkBox_tf.Name = "checkBox_tf";
            this.checkBox_tf.Size = new System.Drawing.Size(15, 14);
            this.checkBox_tf.TabIndex = 6;
            this.checkBox_tf.UseVisualStyleBackColor = true;
            this.checkBox_tf.CheckedChanged += new System.EventHandler(this.checkBox_tf_CheckedChanged);
            // 
            // checkBox_ft
            // 
            this.checkBox_ft.AutoSize = true;
            this.checkBox_ft.Location = new System.Drawing.Point(42, 55);
            this.checkBox_ft.Name = "checkBox_ft";
            this.checkBox_ft.Size = new System.Drawing.Size(15, 14);
            this.checkBox_ft.TabIndex = 7;
            this.checkBox_ft.UseVisualStyleBackColor = true;
            this.checkBox_ft.CheckedChanged += new System.EventHandler(this.checkBox_ft_CheckedChanged);
            // 
            // checkBox_ff
            // 
            this.checkBox_ff.AutoSize = true;
            this.checkBox_ff.Location = new System.Drawing.Point(42, 75);
            this.checkBox_ff.Name = "checkBox_ff";
            this.checkBox_ff.Size = new System.Drawing.Size(15, 14);
            this.checkBox_ff.TabIndex = 8;
            this.checkBox_ff.UseVisualStyleBackColor = true;
            this.checkBox_ff.CheckedChanged += new System.EventHandler(this.checkBox_ff_CheckedChanged);
            // 
            // BinaryConnectiveProperties
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.checkBox_ff);
            this.Controls.Add(this.checkBox_ft);
            this.Controls.Add(this.checkBox_tf);
            this.Controls.Add(this.checkBox_tt);
            this.Controls.Add(this.label_ff);
            this.Controls.Add(this.label_ft);
            this.Controls.Add(this.label_tf);
            this.Controls.Add(this.label_tt);
            this.Controls.Add(this.label_header);
            this.Name = "BinaryConnectiveProperties";
            this.Size = new System.Drawing.Size(71, 95);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label_header;
        private System.Windows.Forms.Label label_tt;
        private System.Windows.Forms.Label label_tf;
        private System.Windows.Forms.Label label_ft;
        private System.Windows.Forms.Label label_ff;
        private System.Windows.Forms.CheckBox checkBox_tt;
        private System.Windows.Forms.CheckBox checkBox_tf;
        private System.Windows.Forms.CheckBox checkBox_ft;
        private System.Windows.Forms.CheckBox checkBox_ff;
    }
}
