using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TraceBackend;

namespace TraceUI
{
    public partial class SymbolPicker : Form
    {
        static SymbolPicker Instance;
        public static event EventHandler<ObjectChosenEventArgs> SymbolPicked;
        

        public SymbolPicker()
        {
            InitializeComponent();
            foreach (MShapeSymbol S in MShapeSymbol.SymbolList)
                elementList1.AddSymbol(S);

            elementList1.ObjectChosen += ElementList_ObjectChosen;
        }

        private void ElementList_ObjectChosen(object sender, ObjectChosenEventArgs e)
        {
            SymbolPicked?.Invoke(sender, e);
        }

        public static void ShowPicker(Point Position)
        {
            if (Instance == null) LoadPicker();
                Instance.ShowAtPosition(Position);
        }

        public static void HidePicker()
        {
            SymbolPicked = null;
            Instance.Hide();
        }
            
        
        static void LoadPicker()
        {
            Instance = new SymbolPicker();
        }

        private void ShowAtPosition(Point Pos)
        {
            Show();
            DesktopLocation = Pos;
            textBox1.Focus();
            elementList1.Filter(F:"");
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            elementList1.Filter(F:textBox1.Text);
        }
    }
}
