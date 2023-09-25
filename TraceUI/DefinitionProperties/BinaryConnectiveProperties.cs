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

namespace TraceUI
{
    public partial class BinaryConnectiveProperties : UserControl
    {
        public MBinaryConnective BC;
        public event EventHandler PropertiesChanged;
        public bool _tt => checkBox_tt.Checked;
        public bool _tf => checkBox_tf.Checked;
        public bool _ft => checkBox_ft.Checked;
        public bool _ff => checkBox_ff.Checked;

        private bool nochange = false;

        public BinaryConnectiveProperties()
        {
            InitializeComponent();
            PropertiesChanged += OnPropertiesChanged;
        }

        public static BinaryConnectiveProperties FromBinaryConnective(MBinaryConnective bc)
        {
            BinaryConnectiveProperties BCP = new BinaryConnectiveProperties() { BC = bc };
            BCP.UpdateFromDefinition();
            return BCP;
        }

        private void OnPropertiesChanged(object sender, EventArgs e)
        {
            UpdateDefinition();
        }

        public void UpdateFromDefinition()
        {
            nochange = true; 
            checkBox_tt.Checked = BC._tt;
            checkBox_tf.Checked = BC._tf;
            checkBox_ft.Checked = BC._ft;
            checkBox_ff.Checked = BC._ff;
            nochange = false;
        }

        public void UpdateDefinition()
        {
            BC._tt = _tt;
            BC._tf = _tf;
            BC._ft = _ft;
            BC._ff = _ff;
        }

        private void checkBox_tt_CheckedChanged(object sender, EventArgs e)
        { if(!nochange) PropertiesChanged?.Invoke(this, new EventArgs()); }
        private void checkBox_tf_CheckedChanged(object sender, EventArgs e)
        { if (!nochange) PropertiesChanged?.Invoke(this, new EventArgs()); }
        private void checkBox_ft_CheckedChanged(object sender, EventArgs e)
        { if (!nochange) PropertiesChanged?.Invoke(this, new EventArgs()); }
        private void checkBox_ff_CheckedChanged(object sender, EventArgs e)
        { if (!nochange) PropertiesChanged?.Invoke(this, new EventArgs()); }
    }
}
