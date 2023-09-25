namespace TraceUI
{
    partial class ElementFinder
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.definitionList = new TraceUI.ElementList();
            this.bt_new = new System.Windows.Forms.Button();
            this.linkButton1 = new TraceUI.LinkButton();
            this.bt_eq = new System.Windows.Forms.Button();
            this.bt_neg = new System.Windows.Forms.Button();
            this.ud_argCount = new System.Windows.Forms.NumericUpDown();
            this.bt_true = new System.Windows.Forms.Button();
            this.bt_false = new System.Windows.Forms.Button();
            this.check_bracket = new System.Windows.Forms.CheckBox();
            this.checkLB_freeVars = new System.Windows.Forms.CheckedListBox();
            this.bt_vis = new System.Windows.Forms.Button();
            this.button_refresh = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.ud_argCount)).BeginInit();
            this.SuspendLayout();
            // 
            // definitionList
            // 
            this.definitionList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.definitionList.AutoScroll = true;
            this.definitionList.Location = new System.Drawing.Point(1, 3);
            this.definitionList.Name = "definitionList";
            this.definitionList.Size = new System.Drawing.Size(291, 259);
            this.definitionList.TabIndex = 0;
            // 
            // bt_new
            // 
            this.bt_new.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.bt_new.Location = new System.Drawing.Point(344, 3);
            this.bt_new.Name = "bt_new";
            this.bt_new.Size = new System.Drawing.Size(42, 33);
            this.bt_new.TabIndex = 1;
            this.bt_new.Text = "New";
            this.bt_new.UseVisualStyleBackColor = true;
            this.bt_new.Click += new System.EventHandler(this.bt_new_Click);
            // 
            // linkButton1
            // 
            this.linkButton1.Location = new System.Drawing.Point(298, 42);
            this.linkButton1.Mode = TraceUI.LinkMode.Variable;
            this.linkButton1.Name = "linkButton1";
            this.linkButton1.Size = new System.Drawing.Size(35, 32);
            this.linkButton1.TabIndex = 2;
            this.linkButton1.Linked += new System.EventHandler<TraceUI.LinkedEventArgs>(this.linkButton1_Linked);
            // 
            // bt_eq
            // 
            this.bt_eq.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.bt_eq.Location = new System.Drawing.Point(344, 78);
            this.bt_eq.Name = "bt_eq";
            this.bt_eq.Size = new System.Drawing.Size(42, 33);
            this.bt_eq.TabIndex = 3;
            this.bt_eq.Text = "=";
            this.bt_eq.UseVisualStyleBackColor = true;
            this.bt_eq.Click += new System.EventHandler(this.bt_eq_Click);
            // 
            // bt_neg
            // 
            this.bt_neg.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.bt_neg.Location = new System.Drawing.Point(298, 78);
            this.bt_neg.Name = "bt_neg";
            this.bt_neg.Size = new System.Drawing.Size(46, 33);
            this.bt_neg.TabIndex = 4;
            this.bt_neg.Text = "NOT";
            this.bt_neg.UseVisualStyleBackColor = true;
            this.bt_neg.Click += new System.EventHandler(this.bt_neg_Click);
            // 
            // ud_argCount
            // 
            this.ud_argCount.Location = new System.Drawing.Point(339, 42);
            this.ud_argCount.Name = "ud_argCount";
            this.ud_argCount.Size = new System.Drawing.Size(50, 20);
            this.ud_argCount.TabIndex = 5;
            // 
            // bt_true
            // 
            this.bt_true.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.bt_true.Location = new System.Drawing.Point(298, 117);
            this.bt_true.Name = "bt_true";
            this.bt_true.Size = new System.Drawing.Size(46, 33);
            this.bt_true.TabIndex = 6;
            this.bt_true.Text = "True";
            this.bt_true.UseVisualStyleBackColor = true;
            this.bt_true.Click += new System.EventHandler(this.bt_true_Click);
            // 
            // bt_false
            // 
            this.bt_false.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.bt_false.Location = new System.Drawing.Point(344, 117);
            this.bt_false.Name = "bt_false";
            this.bt_false.Size = new System.Drawing.Size(42, 33);
            this.bt_false.TabIndex = 7;
            this.bt_false.Text = "False";
            this.bt_false.UseVisualStyleBackColor = true;
            this.bt_false.Click += new System.EventHandler(this.bt_false_Click);
            // 
            // check_bracket
            // 
            this.check_bracket.AutoSize = true;
            this.check_bracket.Checked = true;
            this.check_bracket.CheckState = System.Windows.Forms.CheckState.Indeterminate;
            this.check_bracket.Location = new System.Drawing.Point(298, 156);
            this.check_bracket.Name = "check_bracket";
            this.check_bracket.Size = new System.Drawing.Size(63, 17);
            this.check_bracket.TabIndex = 8;
            this.check_bracket.Text = "Bracket";
            this.check_bracket.ThreeState = true;
            this.check_bracket.UseVisualStyleBackColor = true;
            this.check_bracket.CheckedChanged += new System.EventHandler(this.check_bracket_CheckedChanged);
            // 
            // checkLB_freeVars
            // 
            this.checkLB_freeVars.FormattingEnabled = true;
            this.checkLB_freeVars.Location = new System.Drawing.Point(298, 180);
            this.checkLB_freeVars.Name = "checkLB_freeVars";
            this.checkLB_freeVars.Size = new System.Drawing.Size(91, 79);
            this.checkLB_freeVars.TabIndex = 9;
            this.checkLB_freeVars.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.checkLB_freeVars_ItemCheck);
            this.checkLB_freeVars.SelectedIndexChanged += new System.EventHandler(this.checkLB_freeVars_SelectedIndexChanged);
            // 
            // bt_vis
            // 
            this.bt_vis.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.bt_vis.Location = new System.Drawing.Point(358, 156);
            this.bt_vis.Name = "bt_vis";
            this.bt_vis.Size = new System.Drawing.Size(34, 24);
            this.bt_vis.TabIndex = 10;
            this.bt_vis.Text = "vis";
            this.bt_vis.UseVisualStyleBackColor = true;
            this.bt_vis.Click += new System.EventHandler(this.bt_vis_Click);
            // 
            // button_refresh
            // 
            this.button_refresh.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button_refresh.Location = new System.Drawing.Point(298, 3);
            this.button_refresh.Name = "button_refresh";
            this.button_refresh.Size = new System.Drawing.Size(40, 33);
            this.button_refresh.TabIndex = 11;
            this.button_refresh.Text = "Load";
            this.button_refresh.UseVisualStyleBackColor = true;
            this.button_refresh.Click += new System.EventHandler(this.button_refresh_Click);
            // 
            // ElementFinder
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(391, 265);
            this.ControlBox = false;
            this.Controls.Add(this.button_refresh);
            this.Controls.Add(this.bt_vis);
            this.Controls.Add(this.checkLB_freeVars);
            this.Controls.Add(this.check_bracket);
            this.Controls.Add(this.bt_false);
            this.Controls.Add(this.bt_true);
            this.Controls.Add(this.ud_argCount);
            this.Controls.Add(this.bt_neg);
            this.Controls.Add(this.bt_eq);
            this.Controls.Add(this.linkButton1);
            this.Controls.Add(this.bt_new);
            this.Controls.Add(this.definitionList);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.KeyPreview = true;
            this.Name = "ElementFinder";
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.Deactivate += new System.EventHandler(this.ElementFinder_Deactivate);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ElementFinder_FormClosing);
            this.VisibleChanged += new System.EventHandler(this.ElementFinder_VisibleChanged);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ElementFinder_KeyDown);
            this.Leave += new System.EventHandler(this.ElementFinder_Leave);
            ((System.ComponentModel.ISupportInitialize)(this.ud_argCount)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ElementList definitionList;
        private System.Windows.Forms.Button bt_new;
        private LinkButton linkButton1;
        private System.Windows.Forms.Button bt_eq;
        private System.Windows.Forms.Button bt_neg;
        private System.Windows.Forms.NumericUpDown ud_argCount;
        private System.Windows.Forms.Button bt_true;
        private System.Windows.Forms.Button bt_false;
        private System.Windows.Forms.CheckBox check_bracket;
        private System.Windows.Forms.CheckedListBox checkLB_freeVars;
        private System.Windows.Forms.Button bt_vis;
        private System.Windows.Forms.Button button_refresh;
    }
}