using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TraceBackend;

namespace TraceUI.Tools
{
    public partial class BracketSettingPicker : UserControl
    {
        public string Caption
        {
            get { return label1.Text; }
            set { label1.Text = value; }
        }

        private BracketSetting setting;

        public BracketSetting Setting { get { return setting; } set { setting = value; comboBox1.SelectedIndex = (int)setting; } }

        public event EventHandler SettingChanged;

        public BracketSettingPicker()
        {
            InitializeComponent();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Setting = (BracketSetting)comboBox1.SelectedIndex;
            SettingChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
