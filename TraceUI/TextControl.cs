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
    public partial class TextControl : UserControl
    {
        bool _updating;
        public new MDocumentText Text;
        public DocumentStructure Structure => MainForm.ActiveMainForm.Document.Structure.GetByElement(Text); //TODO: improve

        public TextControl()
        {
            InitializeComponent();
        }

        public static TextControl FromDocumentText(MDocumentText Text)
        {
            TextControl TC = new TextControl() { Text = Text };
            Text.TextChanged += TC.Text_TextChanged;
            TC.Text_TextChanged(TC, EventArgs.Empty);
            return TC;
        }

        private void Text_TextChanged(object sender, EventArgs e)
        {
            if (!_updating)
            {
                richTextBox.Text = Text.Text;
            }
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            _updating = true;
            Text.Text = richTextBox.Text;               
            _updating = false;
        }

        private void richTextBox_ContentsResized(object sender, ContentsResizedEventArgs e)
        {
            Size = new Size(Width, e.NewRectangle.Height + 5);
        }
    }
}
