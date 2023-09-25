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
    public partial class VisualisationDesigner : UserControl
    {
        bool settingparent;
        bool loading;
        int SelectedElement;
        public MVisualisationScheme Scheme;
        public MDefinition Definition;


        public static VisualisationDesigner FromDefinition(MDefinition D)
        {
            VisualisationDesigner VD = new VisualisationDesigner();

            VD.Definition = D;
            VD.LoadScheme(D.DefaultVisualization);
            for(int i = 0; i< D.visualisations.Count;i++)
                VD.cb_ChooseVis.Items.Add(i);
            return VD;
        }

        public static VisualisationDesigner FromVisualisationScheme(MVisualisationScheme VS)
        {
            VisualisationDesigner VD = new VisualisationDesigner() { Scheme = VS };

            VD.LoadScheme(VS);
            return VD;
        }

        private void LoadScheme(MVisualisationScheme VS)
        {
            Scheme = VS;
            visualisationDisplay1.Visualisation = VS.Definition.GetPlaceholderVisualisation(VS);

            loading = true;

            bracketSet_glob.Setting = Scheme.myBracket;

            loading = false;
        }

        public VisualisationDesigner()
        {
            InitializeComponent();
            visualisationDisplay1.SymbolSelected += VisualisationDisplay_SymbolSelected;
        }

        private void UpdateView(bool total = false)
        {
            if (total)
            {
                visualisationDisplay1.Visualisation = Scheme.Definition.GetPlaceholderVisualisation(Scheme, true);
            }
            else
                visualisationDisplay1.Redraw(true);
        }

        private void visualisationDisplay1_SizeChanged(object sender, EventArgs e)
        {
            visualisationDisplay1.Location = new Point(panel1.Width / 2 - visualisationDisplay1.Width / 2, panel1.Height / 2 - visualisationDisplay1.Height / 2);
        }
        
        private void VisualisationDisplay_SymbolSelected(object sender, MClickEventArgs e)
        {
            int oldSelected = SelectedElement;

            if(e.Visualisation == visualisationDisplay1.Visualisation)
            {
                //Symbol
                SelectedElement = e.symbolIndex + Scheme.Children;
            }
            else
            {
                //Subvis
               SelectedElement = visualisationDisplay1.Visualisation.SubVisualisations.ToList().IndexOf(e.Visualisation);
            }

            if (settingparent)
            {
                Scheme.SetParent(oldSelected, SelectedElement);
                UpdateView(true);
                settingparent = false;
            }

            LoadFromSelected();
        }

        private void LoadFromSelected()
        {
            loading = true;
            MArrangementTree sel = Scheme.arrangement.FindByIndex(SelectedElement);
            input_x.Value = (decimal)(sel?.xOff ?? 0);
            input_y.Value = (decimal)(sel?.yOff ?? 0);
            textBox1.Text = Scheme.GetText(SelectedElement);
            tb_latex.Text = Scheme.Latex;
            MSymbol sym = Scheme.GetSymbol(SelectedElement);
            if(sym is MTextSymbol ts)
            {
                check_Cursive.Checked = ts.font == FontCategory.FormulaCursive;
            }
            check_subscript.Checked = sel.MakeSmall;
            if (SelectedElement >= 0 && SelectedElement < Scheme.Brackets.Length)
                bracketSet_sel.Setting = Scheme.Brackets[SelectedElement];
            check_growX.Checked = sel.GrowAlongChildren.x;
            check_growY.Checked = sel.GrowAlongChildren.y;
            loading = false;
        }

        private void button9_Click(object sender, EventArgs e)
        {
            settingparent = true;
        }

        private void button3_Click(object sender, EventArgs e)
        {

        }

        private void button6_Click(object sender, EventArgs e)
        {
            Scheme.SetAnchor(SelectedElement, MAnchor.top);
            UpdateView();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Scheme.SetAnchor(SelectedElement, MAnchor.left);
            UpdateView();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Scheme.SetAnchor(SelectedElement, MAnchor.right);
            UpdateView();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            Scheme.SetAnchor(SelectedElement, MAnchor.bottom);
            UpdateView();
        }

        private void input_x_ValueChanged(object sender, EventArgs e)
        {
            if (loading) return;
            Scheme.SetXOffset(SelectedElement, (float)input_x.Value);
            UpdateView();
        }

        private void input_y_ValueChanged(object sender, EventArgs e)
        {
            if (loading) return;
            Scheme.SetYOffset(SelectedElement, (float)input_y.Value);
            UpdateView();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Scheme.AddSymbol(new MTextSymbol("abc"), SelectedElement);
            LoadFromSelected();
            UpdateView(true);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (loading) return;
                Scheme.SetText(SelectedElement, textBox1.Text);
            UpdateView();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SymbolPicker.ShowPicker(PointToScreen(button1.Location));
            SymbolPicker.SymbolPicked += SymbolPicker_SymbolPicked;
        }

        private void SymbolPicker_SymbolPicked(object sender, ObjectChosenEventArgs e)
        {
            Scheme.AddSymbol(e.Object as MShapeSymbol, SelectedElement);
            LoadFromSelected();
            UpdateView(true);
            SymbolPicker.HidePicker();

        }

        private void button10_Click(object sender, EventArgs e)
        {
            (Parent as DocumentLayoutPanel).InsertRow(new SymbolEditor());
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Scheme.RemoveSymbol(SelectedElement);
            UpdateView(true);
            SelectedElement = -1;
        }

        private void button11_Click(object sender, EventArgs e)
        {
            int oldSelected = SelectedElement;

            SelectedElement = -1;

            if (settingparent)
            {
                Scheme.SetParent(oldSelected, SelectedElement);
                UpdateView();
                settingparent = false;
            }

            UpdateView(true);
        }

        private void check_Cursive_CheckedChanged(object sender, EventArgs e)
        {
            if (loading) return;
            MSymbol sym = Scheme.GetSymbol(SelectedElement);
            if (sym is MTextSymbol ts)
            {
                if (check_Cursive.Checked)
                    ts.font = FontCategory.FormulaCursive;
                else
                    ts.font = FontCategory.FormulaUpright;
                UpdateView(true);
            }
        }

        private void check_subscript_CheckedChanged(object sender, EventArgs e)
        {
            if (loading) return;
            Scheme.SetSmall(SelectedElement, check_subscript.Checked);
            UpdateView(true);
        }

        private void bracketSet_glob_SettingChanged(object sender, EventArgs e)
        {
            if (loading) return;
            Scheme.myBracket = bracketSet_glob.Setting;
            UpdateView(true);
        }

        private void bracketSet_sel_SettingChanged(object sender, EventArgs e)
        {
            if (loading) return;
            if(SelectedElement >= 0 && SelectedElement < Scheme.Brackets.Length)
            Scheme.Brackets[SelectedElement] = bracketSet_sel.Setting;
            UpdateView(true);
        }

        private void check_growX_CheckedChanged(object sender, EventArgs e)
        {
            if (loading) return;
            Scheme.SetGrowX(SelectedElement, check_growX.Checked);
            UpdateView(false);
        }

        private void check_growY_CheckedChanged(object sender, EventArgs e)
        {
            if (loading) return;
            Scheme.SetGrowY(SelectedElement, check_growY.Checked);
            UpdateView(false);
        }

        private void bt_addvis_Click(object sender, EventArgs e)
        {
            MVisualisationScheme V = Definition.CreateVisualisation();
            cb_ChooseVis.Items.Add(cb_ChooseVis.Items.Count);
            cb_ChooseVis.SelectedIndex = cb_ChooseVis.Items.Count-1;
        }

        private void cb_ChooseVis_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadScheme(Definition.visualisations[cb_ChooseVis.SelectedIndex]);
            SelectedElement = -1;
            LoadFromSelected();
            UpdateView(true);
        }

        private void bt_delvis_Click(object sender, EventArgs e)
        {
            // TODO: implement deleting visualizations
        }

        private void tb_latex_TextChanged(object sender, EventArgs e)
        {
            if (loading) return;
            Scheme.Latex = tb_latex.Text;
        }

        private void bt_default_Click(object sender, EventArgs e)
        {
            Definition.defaultVisIndex = cb_ChooseVis.SelectedIndex;
        }

        private void bt_hidesub_Click(object sender, EventArgs e)
        {
            Scheme.HideChild(SelectedElement);
            UpdateView(true);
            SelectedElement = -1;
        }
    }
}
