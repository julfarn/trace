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
    public partial class QuantifierProperties : UserControl
    {
        public MQuantifier Q;
        public event EventHandler PropertiesChanged;

        private bool nochange = false;

        public QuantifierProperties()
        {
            InitializeComponent();
            PropertiesChanged += OnPropertiesChanged;
        }

        public static QuantifierProperties FromQuantifier(MQuantifier q)
        {
            QuantifierProperties QP = new QuantifierProperties() { Q = q };
            QP.UpdateFromDefinition();
            return QP;
        }

        private void OnPropertiesChanged(object sender, EventArgs e)
        {
            UpdateDefinition();
        }

        public void UpdateFromDefinition()
        {
            nochange = true;
            switch (Q.type)
            {
                case MQuantifier.QuantifierType.Existential:
                    rb_exis.Checked = true;
                    break;
                case MQuantifier.QuantifierType.Universal:
                    rb_uni.Checked = true;
                    break;
                default:
                    rb_other.Checked = true;
                    break;
            }
            nochange = false;
        }

        public void UpdateDefinition()
        {
            Q.type = rb_other.Checked ? MQuantifier.QuantifierType.Other : rb_exis.Checked ? MQuantifier.QuantifierType.Existential : MQuantifier.QuantifierType.Universal;
        }

        private void rb_uni_CheckedChanged(object sender, EventArgs e)
        { if (!nochange && rb_uni.Checked) PropertiesChanged?.Invoke(this, new EventArgs()); }
        private void rb_exis_CheckedChanged(object sender, EventArgs e)
        { if (!nochange && rb_exis.Checked) PropertiesChanged?.Invoke(this, new EventArgs()); }
        private void rb_other_CheckedChanged(object sender, EventArgs e)
        { if (!nochange && rb_other.Checked) PropertiesChanged?.Invoke(this, new EventArgs()); }
        
    }
}
