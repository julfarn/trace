using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TraceUI
{
    public partial class InsertionMenuForm : Form
    {
        bool maintainfocus = false;
        public static int aCount = 0;
        public static int vCount = 0;
        public static int fCount = 0;
        public InsertionMenuForm()
        {
            InitializeComponent();
        }

        public void ShowFromInsertionMenu(InsertionMenu IM, Point position)
        {
            Controls.Clear();

            int vpos = 10;
            for (int i = 0; i < IM.Buttons.Count; i++)
            {
                Controls.Add(IM.Buttons[i]);
                IM.Buttons[i].Location = new Point(10, vpos);

                vpos += IM.Buttons[i].Height + 10;
            }

            Size = new Size(200, vpos);

            maintainfocus = true;
            Show();
            DesktopLocation = new Point(position.X, position.Y - Height - 5);
            Focus();
            maintainfocus = false;
        }

        public void SetupProperties(Button Bold, NumericUpDown N1, NumericUpDown N2, NumericUpDown N3, Button B2)
        {
            Point P = Bold.Location;
            if (N1 != null)
            {
                Controls.Add(N1);
                N1.Location = P;
                N1.Width = 40;
                N1.Show();
                P = new Point(N1.Location.X + N1.Width + 5, P.Y);
            }
            if (N2 != null)
            {
                Controls.Add(N2);
                N2.Location = P;
                N2.Width = 40;
                N2.Show();
                P = new Point(N2.Location.X + N2.Width + 5, P.Y);
            }
            if (N3 != null)
            {
                Controls.Add(N3);
                N3.Location = P;
                N3.Width = 40;
                N3.Show();
                P = new Point(N3.Location.X + N3.Width + 5, P.Y);
            }
            Controls.Add(B2);
            B2.Location = P;
            B2.Show();
        }

        private void InsertionMenuForm_Deactivate(object sender, EventArgs e)
        {
            if (!maintainfocus)
                Hide();
        }
    }
}
