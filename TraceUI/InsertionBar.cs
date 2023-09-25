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
    public partial class InsertionBar : UserControl
    {
        internal static InsertionBar lastCreated;
        int rightedge = 0;
        int offs = 5;
        MObject ParentElement;
        DocumentStructure AfterElement;
        DocumentLayoutPanel Panel;
        internal InsertionRow Row;
        

        public InsertionBar()
        {
            InitializeComponent();
        }

        public Button AddButton(string txt)
        {
            Button B = new Button() { Text = txt, AutoSize = true };
            
            Controls.Add(B);
            B.Location = new Point(rightedge + offs, offs);
            rightedge += B.Width + offs;
            return B;
        }

        internal InsertionMenu AddMenu(string txt)
        {
            InsertionMenu M = new InsertionMenu();

            Button B = AddButton(txt);
            B.Click += (s, e) => M.ShowAtButton(B);

            return M;
        }
        
        public static InsertionBar InDocument(DocumentLayoutPanel panel, DocumentStructure AfterElement)
        {
            InsertionBar IB = new InsertionBar();
            IB.AfterElement = AfterElement;
            lastCreated = IB;
            IB.Panel = panel;

            Button theoremButton = IB.AddButton("Theorem");
            theoremButton.Click += IB.TheoremButton_Click;

            InsertionMenu M = IB.AddMenu("Definition");
            Button binConButton = M.AddButton("Binary Connective");
            binConButton.Click += IB.BinConButton_Click;
            Button funcButton = M.AddButton("Function", true, true, true);
            funcButton.Click += IB.FunctionButton_Click;
            Button quanButton = M.AddButton("Quant./Pred.", true, true, true);
            quanButton.Click += IB.QuantifierButton_Click;

            Button axiomButton = IB.AddButton("Axiom");
            axiomButton.Click += IB.AxiomButton_Click;

            Button textButton = IB.AddButton("Text");
            textButton.Click += IB.TextInDocumentButton_Click;

            return IB;
        }

        public static InsertionBar InDeduction(DocumentLayoutPanel panel, MDeduction deduction, DocumentStructure AfterElement)
        {
            InsertionBar IB = new InsertionBar();
            IB.AfterElement = AfterElement;
            lastCreated = IB;
            IB.Panel = panel;
            IB.ParentElement = deduction;

            InsertionMenu M = IB.AddMenu("Deduction Step");
            Button trivialisationButton = M.AddButton("... by Trivialisation");
            trivialisationButton.Click += IB.TrivialisationButton_Click;
            Button specButton = M.AddButton("... by Specification");
            specButton.Click += IB.SpecButton_Click;
            Button varSubsButton = M.AddButton("... by Variable Substitution");
            varSubsButton.Click += IB.VarSubsButton_Click;
            Button termSubsButton = M.AddButton("... by Term Substitution");
            termSubsButton.Click += IB.TermSubsButton_Click;
            Button formSubsButton = M.AddButton("... by Formula Substitution");
            formSubsButton.Click += IB.FormSubsButton_Click;
            Button uniGenButton = M.AddButton("... by Universal Generalisation");
            uniGenButton.Click += IB.UniGenButton_Click;
            Button uniInstButton = M.AddButton("... by Universal Instantiation");
            uniInstButton.Click += IB.UniInstButton_Click;
            Button exiGenButton = M.AddButton("... by Existential Generalisation");
            exiGenButton.Click += IB.ExiGenButton_Click;
            Button exiInstButton = M.AddButton("... by Existential Instantiation");
            exiInstButton.Click += IB.ExiInstButton_Click;
            Button raaButton = M.AddButton("... by Reductio Ad Absurdum");
            raaButton.Click += IB.RaaButton_Click;
            Button condInstButton = M.AddButton("... by Condition Instantiation");
            condInstButton.Click += IB.CondInstButton_Click;
            Button assButton = M.AddButton("Assumption");
            assButton.Click += IB.AssButton_Click;

            Button textButton = IB.AddButton("Text");
            textButton.Click += IB.TextInDeductionButton_Click;

            return IB;
        }

        public static InsertionBar InAxiomList(DocumentLayoutPanel panel, MDefinition definition, DocumentStructure AfterElement)
        {
            InsertionBar IB = new InsertionBar();
            IB.AfterElement = AfterElement;
            lastCreated = IB;
            IB.Panel = panel;
            IB.ParentElement = definition;

            Button axiomButton = IB.AddButton("Axiom");
            axiomButton.Click += IB.AxiomListButton_Click;

            Button textButton = IB.AddButton("Text");
            textButton.Click += IB.TextInAxiomListButton_Click;

            return IB;
        }

        public static InsertionBar InDefinition(DocumentLayoutPanel panel, MDefinition definition, DocumentStructure AfterElement)
        {
            InsertionBar IB = new InsertionBar();
            IB.AfterElement = AfterElement;
            lastCreated = IB;
            IB.Panel = panel;
            IB.ParentElement = definition;

            Button textButton = IB.AddButton("Text");
            textButton.Click += IB.TextInDefinitionButton_Click;

            return IB;
        }

        public static InsertionBar InTheorem(DocumentLayoutPanel panel, MTheorem theorem, DocumentStructure AfterElement)
        {
            InsertionBar IB = new InsertionBar();
            IB.AfterElement = AfterElement;
            lastCreated = IB;
            IB.Panel = panel;
            IB.ParentElement = theorem;

            Button textButton = IB.AddButton("Text");
            textButton.Click += IB.TextInTheoremButton_Click;

            return IB;
        }

        private void TheoremButton_Click(object sender, EventArgs e)
        {
            MTheorem T = MainForm.ActiveMainForm.Document.CreateTheorem(AfterElement);
            Panel.ReplaceRow(Row, new RowLayout(TheoremDocumentElement.FromTheorem(T)), true);
        }

        private void BinConButton_Click(object sender, EventArgs e)
        {
            MBinaryConnective BC = MainForm.ActiveMainForm.Document.CreateBinaryConnective(AfterElement);
            Panel.ReplaceRow(Row, new RowLayout(DefinitionDocumentElement.FromDefinition(BC)), true);
        }
        private void FunctionButton_Click(object sender, EventArgs e)
        {
            MFunctionSymbol F = MainForm.ActiveMainForm.Document.CreateFunction(AfterElement, InsertionMenuForm.aCount, InsertionMenuForm.vCount, InsertionMenuForm.fCount);
            Panel.ReplaceRow(Row, new RowLayout(DefinitionDocumentElement.FromDefinition(F)), true);
        }
        private void QuantifierButton_Click(object sender, EventArgs e)
        {
            MQuantifier Q = MainForm.ActiveMainForm.Document.CreateQuantifier(AfterElement, InsertionMenuForm.aCount, InsertionMenuForm.vCount, InsertionMenuForm.fCount);
            Panel.ReplaceRow(Row, new RowLayout(DefinitionDocumentElement.FromDefinition(Q)), true);
        }

        private void AxiomButton_Click(object sender, EventArgs e)
        {
            MStatement S = MainForm.ActiveMainForm.Document.CreateAxiom(AfterElement);
            Panel.ReplaceRow(Row, new RowLayout(StatementDocumentElement.FromStatement(S)), true);
        }

        private void AxiomListButton_Click(object sender, EventArgs e)
        {
            MStatement S = (ParentElement as MDefinition).CreateAxiom(MFormula.PlaceholderFormula, (ParentElement as MDefinition).Axioms.IndexOf(AfterElement.Element as MStatement) + 1);
            DocumentStructure Struc;
            if (AfterElement.Element is MStatementList)
                Struc = DocumentStructure.Embed(S, AfterElement);
            else
                Struc = DocumentStructure.Insert(S, AfterElement);
            VisualisationDisplay VD = VisualisationDisplay.FromStructure(Struc, editable: true, restriction: ElementType.Formula);
            Panel.ReplaceRow(Row, new RowLayout(VD), true);
            (Panel.ParentDE as StatementListDocumentElement).AddVDToList(VD);
        }

        private void TrivialisationButton_Click(object sender, EventArgs e)
        {
            MTrivialisationDeductionStep DS = MainForm.ActiveMainForm.Document.CreateTrivialization(AfterElement, (ParentElement as MDeduction).Theorem.GetStatement());
            DS.Validate();
            Panel.ReplaceRow(Row, new RowLayout(TrivialisationDocumentElement.FromDeductionStep(DS)), true);
        }
        private void ExiGenButton_Click(object sender, EventArgs e)
        {
            MExistentialGeneralisationDeductionStep DS = MainForm.ActiveMainForm.Document.CreateExistentialGeneralization(AfterElement, (ParentElement as MDeduction).Theorem.GetStatement());
            DS.Validate();
            Panel.ReplaceRow(Row, new RowLayout(ExistentialGeneralisationDocumentElement.FromDeductionStep(DS)), true);
        }
        private void UniInstButton_Click(object sender, EventArgs e)
        {
            MUniversalInstantiationDeductionStep DS = MainForm.ActiveMainForm.Document.CreateUniversalInstantiation(AfterElement, (ParentElement as MDeduction).Theorem.GetStatement());
            DS.Validate();
            Panel.ReplaceRow(Row, new RowLayout(UniversalInstantiationDocumentElement.FromDeductionStep(DS)), true);
        }
        private void UniGenButton_Click(object sender, EventArgs e)
        {
            MUniversalGeneralisationDeductionStep DS = MainForm.ActiveMainForm.Document.CreateUniversalGeneralization(AfterElement, (ParentElement as MDeduction).Theorem.GetStatement());
            DS.Validate();
            Panel.ReplaceRow(Row, new RowLayout(UniversalGeneralisationDocumentElement.FromDeductionStep(DS)), true);
        }
        private void TermSubsButton_Click(object sender, EventArgs e)
        {
            MTermSubstitutionDeductionStep DS = MainForm.ActiveMainForm.Document.CreateTermSubstitution(AfterElement, (ParentElement as MDeduction).Theorem.GetStatement());
            DS.Validate();
            Panel.ReplaceRow(Row, new RowLayout(TermSubstitutionDocumentElement.FromDeductionStep(DS)), true);
        }
        private void FormSubsButton_Click(object sender, EventArgs e)
        {
            MFormulaSubstitutionDeductionStep DS = MainForm.ActiveMainForm.Document.CreateFormulaSubstitution(AfterElement, (ParentElement as MDeduction).Theorem.GetStatement());
            DS.Validate();
            Panel.ReplaceRow(Row, new RowLayout(FormulaSubstitutionDocumentElement.FromDeductionStep(DS)), true);
        }
        private void VarSubsButton_Click(object sender, EventArgs e)
        {
            MVariableSubstitutionDeductionStep DS = MainForm.ActiveMainForm.Document.CreateVariableSubstitution(AfterElement, (ParentElement as MDeduction).Theorem.GetStatement());
            DS.Validate();
            Panel.ReplaceRow(Row, new RowLayout(VariableSubstitutionDocumentElement.FromDeductionStep(DS)), true);
        }
        private void SpecButton_Click(object sender, EventArgs e)
        {
            MPredicateSpecificationDeductionStep DS = MainForm.ActiveMainForm.Document.CreatePredicateSpecification(AfterElement, (ParentElement as MDeduction).Theorem.GetStatement());
            DS.Validate();
            Panel.ReplaceRow(Row, new RowLayout(PredicateSpecificationDocumentElement.FromDeductionStep(DS)), true);
        }
        private void ExiInstButton_Click(object sender, EventArgs e)
        {
            MExistentialInstantiationDeductionStep DS = MainForm.ActiveMainForm.Document.CreateExistentialInstantiation(AfterElement, (ParentElement as MDeduction).Theorem.GetStatement());
            DS.Validate();
            Panel.ReplaceRow(Row, new RowLayout(ExistentialInstantiationDocumentElement.FromDeductionStep(DS)), true);
        }
        private void RaaButton_Click(object sender, EventArgs e)
        {
            MRAADeductionStep DS = MainForm.ActiveMainForm.Document.CreateReductioAdAbsurdum(AfterElement, (ParentElement as MDeduction).Theorem.GetStatement());
            DS.Validate();
            Panel.ReplaceRow(Row, new RowLayout(RAADocumentElement.FromDeductionStep(DS)), true);
        }
        private void AssButton_Click(object sender, EventArgs e)
        {
            MAssumptionDeductionStep DS = MainForm.ActiveMainForm.Document.CreateAssumption(AfterElement, (ParentElement as MDeduction).Theorem.GetStatement());
            DS.Validate();
            Panel.ReplaceRow(Row, new RowLayout(AssumptionDocumentElement.FromDeductionStep(DS)), true);
        }
        private void CondInstButton_Click(object sender, EventArgs e)
        {
            MConditionInstantiationDeductionStep DS = MainForm.ActiveMainForm.Document.CreateConditionInstantiation(AfterElement, (ParentElement as MDeduction).Theorem.GetStatement());
            DS.Validate();
            Panel.ReplaceRow(Row, new RowLayout(ConditionInstantiationDocumentElement.FromDeductionStep(DS)), true);
        }

        private void TextInDocumentButton_Click(object sender, EventArgs e)
        {
            MDocumentText T = new MDocumentText();
            if (AfterElement == null)
                DocumentStructure.Embed(T, MainForm.ActiveMainForm.Document.Structure);
            else
                DocumentStructure.Insert(T, AfterElement);
            Panel.ReplaceRow(Row, new RowLayout(TextControl.FromDocumentText(T)), true);
        }
        private void TextInAxiomListButton_Click(object sender, EventArgs e)
        {
            MDocumentText T = new MDocumentText();
            if (AfterElement.Element is MStatementList)
                DocumentStructure.Embed(T, AfterElement);
            else
                DocumentStructure.Insert(T, AfterElement);
            Panel.ReplaceRow(Row, new RowLayout(TextControl.FromDocumentText(T)), true);
        }
        private void TextInDefinitionButton_Click(object sender, EventArgs e)
        {
            MDocumentText T = new MDocumentText();
            if (AfterElement.Element is MDefinition)
                DocumentStructure.Embed(T, AfterElement);
            else
                DocumentStructure.Insert(T, AfterElement);
            Panel.ReplaceRow(Row, new RowLayout(TextControl.FromDocumentText(T)), true);
        }
        private void TextInDeductionButton_Click(object sender, EventArgs e)
        {
            MDocumentText T = new MDocumentText();
            if (AfterElement.Element is MDeduction)
                DocumentStructure.Embed(T, AfterElement);
            else
                DocumentStructure.Insert(T, AfterElement);
            Panel.ReplaceRow(Row, new RowLayout(TextControl.FromDocumentText(T)), true);
        }
        private void TextInTheoremButton_Click(object sender, EventArgs e)
        {
            MDocumentText T = new MDocumentText();
            if (AfterElement.Element is MTheorem)
                DocumentStructure.Embed(T, AfterElement);
            else
                DocumentStructure.Insert(T, AfterElement);
            Panel.ReplaceRow(Row, new RowLayout(TextControl.FromDocumentText(T)), true);
        }
    }

    public class InsertionMenu
    {
        static InsertionMenuForm form;

        static InsertionMenu()
        {
            form = new InsertionMenuForm();
        }

        internal List<Button> Buttons;

        internal InsertionMenu() { Buttons = new List<Button>(); }

        internal Button AddButton(string text, bool Arguments = false, bool BoundArguments = false, bool Functions = false)
        {
            Button B = new Button() { Text = text };
            B.AutoSize = true;
            Buttons.Add(B);
            if (!Arguments && !BoundArguments && !Functions)
            {
                B.Click += (e, v) => form.Hide();
                return B;
            }

            Button B2 = new Button() { Text = "Add" };
            B2.Click += (e, v) => form.Hide();
            NumericUpDown Narg = null;
            if(Arguments) Narg = new NumericUpDown();
            NumericUpDown Nbound = null;
            if(BoundArguments) Nbound = new NumericUpDown();
            NumericUpDown Nfun = null;
            if (Functions) Nfun = new NumericUpDown();

            B.Click += (e, v) =>
            {
                B.Hide();
                form.SetupProperties(B, Narg, Nbound, Nfun, B2);
            };

            InsertionMenuForm.aCount = (int)(Narg?.Value ?? 0);
            InsertionMenuForm.vCount = (int)(Nbound?.Value ?? 0);
            InsertionMenuForm.fCount = (int)(Nfun?.Value ?? 0);
            if (Arguments)
                Narg.ValueChanged += (e, v) => InsertionMenuForm.aCount = (int)Narg.Value;
            if (BoundArguments)
                Nbound.ValueChanged += (e, v) => InsertionMenuForm.vCount = (int)Nbound.Value;
            if (Functions)
                Nfun.ValueChanged += (e, v) => InsertionMenuForm.fCount = (int)Nfun.Value;

            return B2;
        }

        internal void ShowAtButton(Button B)
        {
            form.ShowFromInsertionMenu(this, B.PointToScreen(new Point(0,0)));
        }
    }
}
