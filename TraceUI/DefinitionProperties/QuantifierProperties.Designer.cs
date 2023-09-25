namespace TraceUI
{
    partial class QuantifierProperties
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
            this.rb_uni = new System.Windows.Forms.RadioButton();
            this.rb_exis = new System.Windows.Forms.RadioButton();
            this.rb_other = new System.Windows.Forms.RadioButton();
            this.SuspendLayout();
            // 
            // rb_uni
            // 
            this.rb_uni.AutoSize = true;
            this.rb_uni.Location = new System.Drawing.Point(3, 3);
            this.rb_uni.Name = "rb_uni";
            this.rb_uni.Size = new System.Drawing.Size(69, 17);
            this.rb_uni.TabIndex = 0;
            this.rb_uni.Text = "Universal";
            this.rb_uni.UseVisualStyleBackColor = true;
            this.rb_uni.CheckedChanged += new System.EventHandler(this.rb_uni_CheckedChanged);
            // 
            // rb_exis
            // 
            this.rb_exis.AutoSize = true;
            this.rb_exis.Location = new System.Drawing.Point(3, 26);
            this.rb_exis.Name = "rb_exis";
            this.rb_exis.Size = new System.Drawing.Size(72, 17);
            this.rb_exis.TabIndex = 1;
            this.rb_exis.Text = "Existential";
            this.rb_exis.UseVisualStyleBackColor = true;
            this.rb_exis.CheckedChanged += new System.EventHandler(this.rb_exis_CheckedChanged);
            // 
            // rb_other
            // 
            this.rb_other.AutoSize = true;
            this.rb_other.Checked = true;
            this.rb_other.Location = new System.Drawing.Point(3, 49);
            this.rb_other.Name = "rb_other";
            this.rb_other.Size = new System.Drawing.Size(51, 17);
            this.rb_other.TabIndex = 2;
            this.rb_other.TabStop = true;
            this.rb_other.Text = "Other";
            this.rb_other.UseVisualStyleBackColor = true;
            this.rb_other.CheckedChanged += new System.EventHandler(this.rb_other_CheckedChanged);
            // 
            // QuantifierProperties
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.rb_other);
            this.Controls.Add(this.rb_exis);
            this.Controls.Add(this.rb_uni);
            this.Name = "QuantifierProperties";
            this.Size = new System.Drawing.Size(90, 69);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton rb_uni;
        private System.Windows.Forms.RadioButton rb_exis;
        private System.Windows.Forms.RadioButton rb_other;
    }
}
