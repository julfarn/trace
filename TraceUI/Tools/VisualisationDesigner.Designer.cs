namespace TraceUI
{
    partial class VisualisationDesigner
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
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.input_y = new System.Windows.Forms.NumericUpDown();
            this.input_x = new System.Windows.Forms.NumericUpDown();
            this.panel1 = new System.Windows.Forms.Panel();
            this.visualisationDisplay1 = new TraceUI.VisualisationDisplay();
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.button7 = new System.Windows.Forms.Button();
            this.button8 = new System.Windows.Forms.Button();
            this.button9 = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.button10 = new System.Windows.Forms.Button();
            this.button11 = new System.Windows.Forms.Button();
            this.check_Cursive = new System.Windows.Forms.CheckBox();
            this.check_subscript = new System.Windows.Forms.CheckBox();
            this.bracketSet_glob = new TraceUI.Tools.BracketSettingPicker();
            this.bracketSet_sel = new TraceUI.Tools.BracketSettingPicker();
            this.label1 = new System.Windows.Forms.Label();
            this.check_growX = new System.Windows.Forms.CheckBox();
            this.check_growY = new System.Windows.Forms.CheckBox();
            this.cb_ChooseVis = new System.Windows.Forms.ComboBox();
            this.bt_addvis = new System.Windows.Forms.Button();
            this.bt_delvis = new System.Windows.Forms.Button();
            this.bt_default = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.tb_latex = new System.Windows.Forms.TextBox();
            this.bt_hidesub = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.input_y)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.input_x)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(360, 3);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(72, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "Add symbol";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button2.Location = new System.Drawing.Point(363, 73);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(107, 23);
            this.button2.TabIndex = 2;
            this.button2.Text = "delete symbol";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button3.Location = new System.Drawing.Point(412, 154);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(26, 23);
            this.button3.TabIndex = 3;
            this.button3.Text = "0";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // input_y
            // 
            this.input_y.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.input_y.DecimalPlaces = 3;
            this.input_y.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.input_y.Location = new System.Drawing.Point(417, 212);
            this.input_y.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.input_y.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            -2147483648});
            this.input_y.Name = "input_y";
            this.input_y.Size = new System.Drawing.Size(56, 20);
            this.input_y.TabIndex = 4;
            this.input_y.ValueChanged += new System.EventHandler(this.input_y_ValueChanged);
            // 
            // input_x
            // 
            this.input_x.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.input_x.DecimalPlaces = 3;
            this.input_x.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.input_x.Location = new System.Drawing.Point(360, 212);
            this.input_x.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.input_x.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            -2147483648});
            this.input_x.Name = "input_x";
            this.input_x.Size = new System.Drawing.Size(57, 20);
            this.input_x.TabIndex = 5;
            this.input_x.ValueChanged += new System.EventHandler(this.input_x_ValueChanged);
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BackColor = System.Drawing.SystemColors.ControlDark;
            this.panel1.Controls.Add(this.visualisationDisplay1);
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(296, 153);
            this.panel1.TabIndex = 6;
            // 
            // visualisationDisplay1
            // 
            this.visualisationDisplay1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.visualisationDisplay1.BackColor = System.Drawing.Color.White;
            this.visualisationDisplay1.Editable = false;
            this.visualisationDisplay1.interactionMode = TraceUI.VisualisationDisplay.InteractionMode.symbols;
            this.visualisationDisplay1.LinkOnly = false;
            this.visualisationDisplay1.Location = new System.Drawing.Point(3, 3);
            this.visualisationDisplay1.minRestTime = 20;
            this.visualisationDisplay1.Name = "visualisationDisplay1";
            this.visualisationDisplay1.Restriction = TraceUI.ElementType.Any;
            this.visualisationDisplay1.Size = new System.Drawing.Size(291, 147);
            this.visualisationDisplay1.TabIndex = 0;
            this.visualisationDisplay1.Visualisation = null;
            this.visualisationDisplay1.SizeChanged += new System.EventHandler(this.visualisationDisplay1_SizeChanged);
            // 
            // button4
            // 
            this.button4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button4.Location = new System.Drawing.Point(430, 3);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(40, 23);
            this.button4.TabIndex = 7;
            this.button4.Text = "abc";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button5
            // 
            this.button5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button5.Location = new System.Drawing.Point(380, 154);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(26, 23);
            this.button5.TabIndex = 8;
            this.button5.Text = "l";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // button6
            // 
            this.button6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button6.Location = new System.Drawing.Point(412, 130);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(26, 23);
            this.button6.TabIndex = 9;
            this.button6.Text = "t";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // button7
            // 
            this.button7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button7.Location = new System.Drawing.Point(444, 154);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(26, 23);
            this.button7.TabIndex = 10;
            this.button7.Text = "r";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler(this.button7_Click);
            // 
            // button8
            // 
            this.button8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button8.Location = new System.Drawing.Point(412, 183);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(26, 23);
            this.button8.TabIndex = 11;
            this.button8.Text = "b";
            this.button8.UseVisualStyleBackColor = true;
            this.button8.Click += new System.EventHandler(this.button8_Click);
            // 
            // button9
            // 
            this.button9.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button9.Location = new System.Drawing.Point(363, 101);
            this.button9.Name = "button9";
            this.button9.Size = new System.Drawing.Size(107, 23);
            this.button9.TabIndex = 12;
            this.button9.Text = "set parent";
            this.button9.UseVisualStyleBackColor = true;
            this.button9.Click += new System.EventHandler(this.button9_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(368, 28);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(100, 20);
            this.textBox1.TabIndex = 13;
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // button10
            // 
            this.button10.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button10.Location = new System.Drawing.Point(363, 49);
            this.button10.Name = "button10";
            this.button10.Size = new System.Drawing.Size(107, 20);
            this.button10.TabIndex = 14;
            this.button10.Text = "Symbol Editor";
            this.button10.UseVisualStyleBackColor = true;
            this.button10.Click += new System.EventHandler(this.button10_Click);
            // 
            // button11
            // 
            this.button11.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button11.Location = new System.Drawing.Point(363, 125);
            this.button11.Name = "button11";
            this.button11.Size = new System.Drawing.Size(46, 23);
            this.button11.TabIndex = 15;
            this.button11.Text = "root";
            this.button11.UseVisualStyleBackColor = true;
            this.button11.Click += new System.EventHandler(this.button11_Click);
            // 
            // check_Cursive
            // 
            this.check_Cursive.AutoSize = true;
            this.check_Cursive.Checked = true;
            this.check_Cursive.CheckState = System.Windows.Forms.CheckState.Checked;
            this.check_Cursive.Location = new System.Drawing.Point(4, 159);
            this.check_Cursive.Name = "check_Cursive";
            this.check_Cursive.Size = new System.Drawing.Size(61, 17);
            this.check_Cursive.TabIndex = 16;
            this.check_Cursive.Text = "Cursive";
            this.check_Cursive.UseVisualStyleBackColor = true;
            this.check_Cursive.CheckedChanged += new System.EventHandler(this.check_Cursive_CheckedChanged);
            // 
            // check_subscript
            // 
            this.check_subscript.AutoSize = true;
            this.check_subscript.Checked = true;
            this.check_subscript.CheckState = System.Windows.Forms.CheckState.Checked;
            this.check_subscript.Location = new System.Drawing.Point(4, 182);
            this.check_subscript.Name = "check_subscript";
            this.check_subscript.Size = new System.Drawing.Size(70, 17);
            this.check_subscript.TabIndex = 17;
            this.check_subscript.Text = "Subscript";
            this.check_subscript.UseVisualStyleBackColor = true;
            this.check_subscript.CheckedChanged += new System.EventHandler(this.check_subscript_CheckedChanged);
            // 
            // bracketSet_glob
            // 
            this.bracketSet_glob.Caption = "Backet (global)";
            this.bracketSet_glob.Location = new System.Drawing.Point(80, 159);
            this.bracketSet_glob.Name = "bracketSet_glob";
            this.bracketSet_glob.Setting = TraceBackend.BracketSetting.No;
            this.bracketSet_glob.Size = new System.Drawing.Size(224, 24);
            this.bracketSet_glob.TabIndex = 18;
            this.bracketSet_glob.SettingChanged += new System.EventHandler(this.bracketSet_glob_SettingChanged);
            // 
            // bracketSet_sel
            // 
            this.bracketSet_sel.Caption = "Bracket (selected)";
            this.bracketSet_sel.Location = new System.Drawing.Point(80, 183);
            this.bracketSet_sel.Name = "bracketSet_sel";
            this.bracketSet_sel.Setting = TraceBackend.BracketSetting.No;
            this.bracketSet_sel.Size = new System.Drawing.Size(224, 24);
            this.bracketSet_sel.TabIndex = 19;
            this.bracketSet_sel.SettingChanged += new System.EventHandler(this.bracketSet_sel_SettingChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 212);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 13);
            this.label1.TabIndex = 20;
            this.label1.Text = "Grow Along:";
            // 
            // check_growX
            // 
            this.check_growX.AutoSize = true;
            this.check_growX.Checked = true;
            this.check_growX.CheckState = System.Windows.Forms.CheckState.Checked;
            this.check_growX.Location = new System.Drawing.Point(74, 213);
            this.check_growX.Name = "check_growX";
            this.check_growX.Size = new System.Drawing.Size(33, 17);
            this.check_growX.TabIndex = 21;
            this.check_growX.Text = "X";
            this.check_growX.UseVisualStyleBackColor = true;
            this.check_growX.CheckedChanged += new System.EventHandler(this.check_growX_CheckedChanged);
            // 
            // check_growY
            // 
            this.check_growY.AutoSize = true;
            this.check_growY.Checked = true;
            this.check_growY.CheckState = System.Windows.Forms.CheckState.Checked;
            this.check_growY.Location = new System.Drawing.Point(113, 213);
            this.check_growY.Name = "check_growY";
            this.check_growY.Size = new System.Drawing.Size(33, 17);
            this.check_growY.TabIndex = 22;
            this.check_growY.Text = "Y";
            this.check_growY.UseVisualStyleBackColor = true;
            this.check_growY.CheckedChanged += new System.EventHandler(this.check_growY_CheckedChanged);
            // 
            // cb_ChooseVis
            // 
            this.cb_ChooseVis.FormattingEnabled = true;
            this.cb_ChooseVis.Location = new System.Drawing.Point(173, 209);
            this.cb_ChooseVis.Name = "cb_ChooseVis";
            this.cb_ChooseVis.Size = new System.Drawing.Size(42, 21);
            this.cb_ChooseVis.TabIndex = 23;
            this.cb_ChooseVis.SelectedIndexChanged += new System.EventHandler(this.cb_ChooseVis_SelectedIndexChanged);
            // 
            // bt_addvis
            // 
            this.bt_addvis.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bt_addvis.Location = new System.Drawing.Point(221, 209);
            this.bt_addvis.Name = "bt_addvis";
            this.bt_addvis.Size = new System.Drawing.Size(26, 23);
            this.bt_addvis.TabIndex = 24;
            this.bt_addvis.Text = "+";
            this.bt_addvis.UseVisualStyleBackColor = true;
            this.bt_addvis.Click += new System.EventHandler(this.bt_addvis_Click);
            // 
            // bt_delvis
            // 
            this.bt_delvis.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bt_delvis.Location = new System.Drawing.Point(253, 209);
            this.bt_delvis.Name = "bt_delvis";
            this.bt_delvis.Size = new System.Drawing.Size(26, 23);
            this.bt_delvis.TabIndex = 25;
            this.bt_delvis.Text = "-";
            this.bt_delvis.UseVisualStyleBackColor = true;
            this.bt_delvis.Click += new System.EventHandler(this.bt_delvis_Click);
            // 
            // bt_default
            // 
            this.bt_default.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bt_default.Location = new System.Drawing.Point(285, 209);
            this.bt_default.Name = "bt_default";
            this.bt_default.Size = new System.Drawing.Size(26, 23);
            this.bt_default.TabIndex = 26;
            this.bt_default.Text = "Def.";
            this.bt_default.UseVisualStyleBackColor = true;
            this.bt_default.Click += new System.EventHandler(this.bt_default_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 235);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(42, 13);
            this.label2.TabIndex = 27;
            this.label2.Text = "LaTeX:";
            // 
            // tb_latex
            // 
            this.tb_latex.Location = new System.Drawing.Point(51, 234);
            this.tb_latex.Name = "tb_latex";
            this.tb_latex.Size = new System.Drawing.Size(417, 20);
            this.tb_latex.TabIndex = 28;
            this.tb_latex.TextChanged += new System.EventHandler(this.tb_latex_TextChanged);
            // 
            // bt_hidesub
            // 
            this.bt_hidesub.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bt_hidesub.Location = new System.Drawing.Point(300, 73);
            this.bt_hidesub.Name = "bt_hidesub";
            this.bt_hidesub.Size = new System.Drawing.Size(57, 23);
            this.bt_hidesub.TabIndex = 29;
            this.bt_hidesub.Text = "hide sub";
            this.bt_hidesub.UseVisualStyleBackColor = true;
            this.bt_hidesub.Click += new System.EventHandler(this.bt_hidesub_Click);
            // 
            // VisualisationDesigner
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.bt_hidesub);
            this.Controls.Add(this.tb_latex);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.bt_default);
            this.Controls.Add(this.bt_delvis);
            this.Controls.Add(this.bt_addvis);
            this.Controls.Add(this.cb_ChooseVis);
            this.Controls.Add(this.check_growY);
            this.Controls.Add(this.check_growX);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.bracketSet_sel);
            this.Controls.Add(this.bracketSet_glob);
            this.Controls.Add(this.check_subscript);
            this.Controls.Add(this.check_Cursive);
            this.Controls.Add(this.button11);
            this.Controls.Add(this.button10);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.button9);
            this.Controls.Add(this.button8);
            this.Controls.Add(this.button7);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.input_x);
            this.Controls.Add(this.input_y);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Name = "VisualisationDesigner";
            this.Size = new System.Drawing.Size(473, 257);
            ((System.ComponentModel.ISupportInitialize)(this.input_y)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.input_x)).EndInit();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.NumericUpDown input_y;
        private System.Windows.Forms.NumericUpDown input_x;
        private System.Windows.Forms.Panel panel1;
        private VisualisationDisplay visualisationDisplay1;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.Button button8;
        private System.Windows.Forms.Button button9;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button button10;
        private System.Windows.Forms.Button button11;
        private System.Windows.Forms.CheckBox check_Cursive;
        private System.Windows.Forms.CheckBox check_subscript;
        private Tools.BracketSettingPicker bracketSet_glob;
        private Tools.BracketSettingPicker bracketSet_sel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox check_growX;
        private System.Windows.Forms.CheckBox check_growY;
        private System.Windows.Forms.ComboBox cb_ChooseVis;
        private System.Windows.Forms.Button bt_addvis;
        private System.Windows.Forms.Button bt_delvis;
        private System.Windows.Forms.Button bt_default;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tb_latex;
        private System.Windows.Forms.Button bt_hidesub;
    }
}
