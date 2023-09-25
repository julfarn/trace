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
    public partial class DocumentElement : UserControl
    {
        public string Title { get { return labelTitle.Text; } set { labelTitle.Text = value; } }
        public MVisualisation TitleVisualisation
        {
            get
            {
                return titleVisDisplay.Visualisation;
            }
            set
            {
                titleVisDisplay.Visualisation = value;

                if (!Expanded)
                {
                    if (value == null)
                    {
                        titleVisDisplay.Hide();
                        labelTitle.Show();
                    }
                    else
                    {
                        titleVisDisplay.Show();
                        labelTitle.Hide();
                    }
                }
            }
        }
        public MStatement TitleStatement
        {
            get
            {
                return titleVisDisplay.Statement;
            }
            set
            {
                titleVisDisplay.SetStatement(value);

                if (!Expanded)
                {
                    if (value == null)
                    {
                        titleVisDisplay.Hide();
                        labelTitle.Show();
                    }
                    else
                    {
                        titleVisDisplay.Show();
                        labelTitle.Hide();
                    }
                }
            }
        }
        public DocumentLayoutPanel ParentLayoutPanel => Parent as DocumentLayoutPanel;
        public DocumentStructure Structure;
        public virtual MObject Object => null;
        public virtual MContext Context => null;
        bool expd = true;
        public bool Expanded { get { return expd; } set {
                expd = value;
                layoutPanel.Visible = value;

                if (value == true)
                {
                    titleVisDisplay.Hide();
                    labelTitle.Show();
                }
                else
                {
                    if (TitleVisualisation == null)
                    {
                        titleVisDisplay.Hide();
                        labelTitle.Show();
                    }
                    else
                    {
                        titleVisDisplay.Show();
                        labelTitle.Hide();
                    }
                }
                    LayoutPanel_HeightChanged(layoutPanel, new EventArgs());
                //redrawExpander();
            } }
        private Validity _valid = Validity.Valid;
        public Validity Valid { get { return _valid; } set { _valid = value; redrawExpander(); } }

        public void AddLabel(string text, int position = -1)
        {
            Label l = new Label() { Text = text, AutoSize = true };
            l.Font = Properties.Settings.Default.LabelFont;
            AddElement(l, position);
        }

        public void AddElement(Control Element, int position = -1)
        {
            layoutPanel.InsertRow(Element, position);
        }

        public void AddLabeledElement(string text, Control Element, int position = -1)
        {
            Label l = new Label() { Text = text, AutoSize = true };
            l.Font = Properties.Settings.Default.LabelFont;
            AddRow(new RowLayout(l, Element), position);
        }

        public void AddRow(RowLayout Row, int position = -1)
        {
            layoutPanel.InsertRow(Row, position);
        }

        public void RemoveRow(int position)
        {
            layoutPanel.RemoveRow(position, true);
        }

        public DocumentElement()
        {
            InitializeComponent();
            layoutPanel.HeightChanged += LayoutPanel_HeightChanged;
            layoutPanel.ParentDE = this;
        }

        public virtual void AddSubStructure(DocumentStructure DS)
        {
            foreach (DocumentStructure SS in DS.Children)
                AddElement(FromStructure(SS));
        }

        public static UserControl FromStructure(DocumentStructure DS)
        {
            if (DS.Element is MTheorem T) return TheoremDocumentElement.FromTheorem(T, DS);
            if (DS.Element is MDefinition D) return DefinitionDocumentElement.FromDefinition(D, DS);
            if (DS.Element is MDeduction Ded) return DeductionDocumentElement.FromDeduction(Ded, DS);
            if (DS.Element is MDeductionStep DedS) return DeductionStepDocumentElement.FromDeductionStep(DedS, DS);
            if (DS.Element is MVisualisationScheme VS) return VisualisationDocumentElement.FromVisualisationScheme(VS, DS);
            if (DS.Element is MStatement S) return StatementDocumentElement.FromStatement(S, DS);
            if (DS.Element is MStatementList SL) return StatementListDocumentElement.FromDefinition(SL.Definition, DS);
            if (DS.Element is MDocumentText Tx) return TextControl.FromDocumentText(Tx);

            return null;
        }

        private void DocumentElement_VisibleChanged(object sender, EventArgs e)
        {
        }

        private void expanderButton_Click_1(object sender, EventArgs e)
        {
            Expanded = !Expanded;
        }

        private void redrawExpander()
        {
            Bitmap b = new Bitmap(expanderButton.ClientSize.Width, expanderButton.ClientSize.Height);
            Graphics g = Graphics.FromImage(b);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            g.Clear(Color.White);

            int top = 10;
            int top1 = top + 5;
            int top2 = top + 10;
            int bot = b.Height - 3;
            int left = 2;
            int right = b.Width - 2;
            int mid = b.Width / 2;

            Pen col = Valid.GetPen();

            if (Expanded)
            {
                g.DrawLine(col, left, top, right, top);
                g.DrawLine(col, left, top, mid, top2);
                g.DrawLine(col, right, top, mid, top2);
                g.DrawLine(col, mid, top2, mid, bot);
                g.DrawLine(col, left, bot, right, bot);
            }
            else
            {
                g.DrawLine(col, left, top, right, top1);
                g.DrawLine(col, left, top, left, top2);
                g.DrawLine(col, left, top2, right, top1);
            }

            expanderButton.Image = b;
        }

        internal void ContainerWidthChanged(object sender, EventArgs e)
        {
            Size = new Size((sender as Control).Width, Height);

            layoutPanel.SetWidth(Width - layoutPanel.Location.X);
        }

        private void LayoutPanel_HeightChanged(object sender, EventArgs e)
        {
            if (Expanded)
                Size = new Size(Width, layoutPanel.Height + layoutPanel.Location.Y);
            else
            {
                Size = new Size(Width, 30);
            }

            redrawExpander();
        }

        private void expanderButton_MouseDown(object sender, MouseEventArgs e)
        {
            Focus();
        }

        protected virtual void DeleteElement()
        {
            Structure.Delete();
            ParentLayoutPanel.RemoveRow(this, true);
        }

        protected virtual void bt_delete_Click(object sender, EventArgs e)
        {
            DeleteElement();
        }

        protected virtual void bt_hide_Click(object sender, EventArgs e)
        {

        }
    }

    public static class ValidityExtensions
    {
        public static Color GetColor(this Validity V)
        {
            if (!V.IsValid) return Color.Red;
            if (V.IsDependent) return Color.Blue;
            return Color.Black;
        }

        public static Brush GetBrush(this Validity V)
        {
            if (!V.IsValid) return Brushes.Red;
            if (V.IsDependent) return Brushes.Blue;
            return Brushes.Black;
        }

        public static Pen GetPen(this Validity V)
        {
            if (!V.IsValid) return Pens.Red;
            if (V.IsDependent) return Pens.Blue;
            return Pens.Gray;
        }
    }

    public class TheoremDocumentElement : DocumentElement
    {
        VisualisationDisplay StatementVD;

        protected MTheorem Theorem;
        public override MObject Object => Theorem;
        public override MContext Context => Theorem.Context;

        public static TheoremDocumentElement FromTheorem(MTheorem T, DocumentStructure DS = null)
        {
            if (DS == null) DS = MainForm.ActiveMainForm.Document.Structure.GetByElement(T);

            TheoremDocumentElement DE = new TheoremDocumentElement()
            {
                Title = "Theorem.",
                Theorem = T,
                Valid = T.valid,
                Structure = DS
            };

            DE.StatementVD = VisualisationDisplay.FromStatement(T.GetStatement(), editable: true, restriction: ElementType.Formula);
            //DE.AddElement(DE.StatementVD);
            DE.StatementVD.ExpressionChanged += DE.StatementVD_ExpressionChanged;
            DE.StatementVD.CompleteChange += DE.StatementVD_CompleteChange;

            if (T.Deduction == null) //TODO: change to GetDeduction()
                DE.AddLabel("Proven by evaluation.");

            // DE.AddElement(DeductionDocumentElement.FromDeduction(T.Deduction)); // see above
            DE.AddSubStructure(DS);

            T.ValidityChanged += DE.T_ValidityChanged;
            T.StatementChanged += DE.T_StatementChanged;
            DE.labelTitle.Font = TraceBackend.Properties.Settings.Default.Headline;

            return DE;
        }

        public override void AddSubStructure(DocumentStructure DS)
        {
            foreach (DocumentStructure SS in DS.Children)
            {
                if (SS.Element is MStatement S)
                {
                    AddElement(StatementVD);
                    StatementVD.Structure = SS;
                }
                else
                    AddElement(FromStructure(SS));
            }
        }

        private void T_StatementChanged(object sender, EventArgs e)
        {
            StatementVD.SetStatement(Theorem.GetStatement());
        }

        private void StatementVD_CompleteChange(object sender, EventArgs e)
        {
            Theorem.SetStatement(StatementVD.Expression as MFormula, true);
        }

        private void StatementVD_ExpressionChanged(object sender, EventArgs e)
        {
            Theorem.Validate(false);
        }

        private void T_ValidityChanged(object sender, EventArgs e)
        {
            Valid = Theorem.valid;
        }

        protected override void bt_delete_Click(object sender, EventArgs e)
        {
            base.bt_delete_Click(sender, e);

            Theorem.Context.RemoveTheorem(Theorem);
        }
    }

    public class DeductionDocumentElement : DocumentElement
    {
        protected MDeduction Deduction;
        public override MObject Object => Deduction;
        public override MContext Context => Deduction._X;

        public static DeductionDocumentElement FromDeduction(MDeduction D, DocumentStructure DS = null)
        {
            if (DS == null) DS = MainForm.ActiveMainForm.Document.Structure.GetByElement(D);

            DeductionDocumentElement DE = new DeductionDocumentElement()
            {
                Title = "Proof.",
                Deduction = D,
                Valid = D.valid,
                Structure = DS
            };

            DE.AddSubStructure(DS);

            D.ValidityChanged += DE.D_ValidityChanged;

            DE.labelTitle.Font = TraceBackend.Properties.Settings.Default.Subheadline;

            return DE;
        }

        public override void AddSubStructure(DocumentStructure DS)
        {
            foreach (DocumentStructure SS in DS.Children)
            {
                AddElement(FromStructure(SS));
                if (SS.Hidden) layoutPanel.HideRow(layoutPanel.Rows.Count - 1);
            }
        }

        private void D_ValidityChanged(object sender, EventArgs e)
        {
            Valid = Deduction.valid;
        }

        protected override void bt_delete_Click(object sender, EventArgs e)
        {
            //only relevant when we allow multiple definitions, until then:
            return;

            base.bt_delete_Click(sender, e);

            Deduction.Theorem.RemoveDeduction(Deduction);
        }
    }
    
    public class StatementDocumentElement : DocumentElement
    {
        VisualisationDisplay StatementVD;

        protected MStatement Statement;
        public override MObject Object => Statement;
        public override MContext Context => Statement._X;

        public static StatementDocumentElement FromStatement(MStatement S, DocumentStructure DS = null)
        {
            if (DS == null) DS = MainForm.ActiveMainForm.Document.Structure.GetByElement(S);

            StatementDocumentElement DE = new StatementDocumentElement()
            {
                Title = (S.valid.IsAxiom) ? "Axiom." : "Statement.",
                Statement = S,
                Valid = S.valid,
                Structure = DS
            };

            DE.StatementVD = VisualisationDisplay.FromStatement(S, editable: true, restriction: ElementType.Formula);
            DE.AddElement(DE.StatementVD);

            DE.AddSubStructure(DS);

            S.ValidityChanged += DE.S_ValidityChanged;
            DE.StatementVD.CompleteChange += DE.StatementVD_CompleteChange;
            DE.StatementVD.ExpressionChanged += DE.StatementVD_ExpressionChanged;

            DE.labelTitle.Font = TraceBackend.Properties.Settings.Default.Headline;

            return DE;
        }

        public override void AddSubStructure(DocumentStructure DS)
        {
            foreach (DocumentStructure SS in DS.Children)
            {
                if (SS.Element is MStatement S)
                {
                    AddElement(StatementVD);
                }
                else
                    AddElement(FromStructure(SS));
            }
        }

        private void StatementVD_ExpressionChanged(object sender, EventArgs e)
        {
            Statement.SetFormula(StatementVD.Expression as MFormula);
        }

        private void StatementVD_CompleteChange(object sender, EventArgs e)
        {
            Statement.SetFormula(StatementVD.Expression as MFormula);
        }

        private void S_ValidityChanged(object sender, EventArgs e)
        {
            Valid = Statement.valid;
        }

        protected override void bt_delete_Click(object sender, EventArgs e)
        {
            base.bt_delete_Click(sender, e);

            Context.RemoveAxiom(Statement);
        }
    }

    public class StatementListDocumentElement : DocumentElement
    {
        List<VisualisationDisplay> VDlist;

        protected MDefinition Definition;
        public override MObject Object => Definition;
        public override MContext Context => Definition._X;

        public static StatementListDocumentElement FromDefinition(MDefinition D, DocumentStructure DS)
        {

            StatementListDocumentElement DE = new StatementListDocumentElement()
            {
                Title = "Axioms.",
                Definition = D,
                Structure = DS
            };

            DE.VDlist = new List<VisualisationDisplay>();

            DE.AddSubStructure(DS);

            DE.labelTitle.Font = TraceBackend.Properties.Settings.Default.Subheadline;
            
            return DE;
        }

        public override void AddSubStructure(DocumentStructure DS)
        {
            foreach (DocumentStructure SS in DS.Children)
            {
                if (SS.Element is MStatement S)
                {
                    VisualisationDisplay newVD = VisualisationDisplay.FromStructure(SS, editable: true, restriction: ElementType.Formula);
                    AddElement(newVD);
                    AddVDToList(newVD);
                }
                else
                    AddElement(FromStructure(SS));
            }
        }

        internal void AddVDToList(VisualisationDisplay newVD)
        {
            VDlist.Add(newVD);
            newVD.ExpressionChanged += VD_ExpressionChanged;
            newVD.CompleteChange += VD_CompleteChange;
        }

        private void VD_CompleteChange(object sender, EventArgs e)
        {
            foreach (VisualisationDisplay VD in VDlist)
                VD.Statement.SetFormula(VD.Expression as MFormula);
        }

        private void VD_ExpressionChanged(object sender, EventArgs e)
        {
            foreach (VisualisationDisplay VD in VDlist)
                VD.Statement.SetFormula(VD.Expression as MFormula);
        }

        protected override void bt_delete_Click(object sender, EventArgs e)
        {
            // Do nothing
        }
    }

    public class DefinitionDocumentElement : DocumentElement
    {
        protected MDefinition Definition;
        public override MObject Object => Definition;
        public override MContext Context => Definition._X;

        public static DefinitionDocumentElement FromDefinition(MDefinition D, DocumentStructure DS = null)
        {
            if (DS == null) DS = MainForm.ActiveMainForm.Document.Structure.GetByElement(D);

            DefinitionDocumentElement DE = new DefinitionDocumentElement()
            {
                Title = "Definition.",
                Definition = D,
                Structure = DS
            };

            DE.AddSubStructure(DS);

            if (D is MBinaryConnective b) DE.AddElement(DefinitionPropertyDocumentElement.PropertiesFromBinaryConnective(b));
            if (D is MQuantifier q) DE.AddElement(DefinitionPropertyDocumentElement.PropertiesFromQuantifier(q));

            DE.labelTitle.Font = TraceBackend.Properties.Settings.Default.Headline;

            return DE;
        }

        protected override void bt_delete_Click(object sender, EventArgs e)
        {
            base.bt_delete_Click(sender, e);

            Context.RemoveDefinition(Definition);
        }
    }

    public class VisualisationDocumentElement : DocumentElement
    {
        protected MVisualisationScheme Scheme;
        //public override MObject Object => Scheme;

        public static VisualisationDocumentElement FromVisualisationScheme(MVisualisationScheme VS, DocumentStructure DS = null)
        {
            if (DS == null) DS = MainForm.ActiveMainForm.Document.Structure.GetByElement(VS);

            VisualisationDocumentElement DE = new VisualisationDocumentElement()
            {
                Title = "Visualisation.",
                Scheme = VS,
                Structure = DS
            };

            DE.AddElement(VisualisationDesigner.FromDefinition(VS.Definition));

            DE.labelTitle.Font = TraceBackend.Properties.Settings.Default.Subheadline;

            DE.Expanded = false;

            return DE;
        }

        protected override void bt_delete_Click(object sender, EventArgs e)
        {
            //do nothing

            /*base.bt_delete_Click(sender, e);

            if (Scheme.Definition != null)
                Scheme.Definition.RemoveVisualization(Scheme);*/
        }
    }

    public class DefinitionPropertyDocumentElement : DocumentElement
    {
        protected MDefinition Definition;

        public static DefinitionPropertyDocumentElement PropertiesFromBinaryConnective(MBinaryConnective BC)
        {
            DefinitionPropertyDocumentElement DE = new DefinitionPropertyDocumentElement()
            {
                Title = "Properties.",
                Definition = BC
            };

            DE.AddElement(BinaryConnectiveProperties.FromBinaryConnective(BC));

            DE.labelTitle.Font = TraceBackend.Properties.Settings.Default.Subheadline;

            return DE;
        }

        public static DefinitionPropertyDocumentElement PropertiesFromQuantifier(MQuantifier Q)
        {
            DefinitionPropertyDocumentElement DE = new DefinitionPropertyDocumentElement()
            {
                Title = "Properties.",
                Definition = Q
            };

            DE.AddElement(QuantifierProperties.FromQuantifier(Q));

            DE.labelTitle.Font = TraceBackend.Properties.Settings.Default.Subheadline;

            return DE;
        }

        protected override void bt_delete_Click(object sender, EventArgs e)
        {
            // Do nothing.
        }
    }
}
