using System;
using System.Collections.Generic;
using System.Windows.Forms;
using TraceBackend;

namespace TraceUI
{
    public class DeductionStepDocumentElement : DocumentElement
    {
        VisualisationDisplay PremiseVD, ConseqVD;
        CheckBox ReduceCB;
        protected MDeductionStep DeductionStep;
        public override MObject Object => DeductionStep;
        public override MContext Context => DeductionStep._D._X;

        protected void AddFirstElements()
        {
            AddLabel("Premise:");
            PremiseVD = VisualisationDisplay.FromStatement(DeductionStep.Premise, editable: true, linkonly: true, restriction: ElementType.Statement);
            AddElement(PremiseVD);
            PremiseVD.CompleteChange += PremiseVD_CompleteChange;
            DeductionStep.PremiseChanged += DeductionStep_PremiseChanged;
        }

        private void DeductionStep_PremiseChanged(object sender, EventArgs e)
        {
            PremiseVD.SetStatement(DeductionStep.Premise);
            ConseqVD.SetStatement(DeductionStep.GetConsequence());
            TitleStatement = DeductionStep.GetConsequence();
        }

        private void PremiseVD_CompleteChange(object sender, EventArgs e)
        {
            DeductionStep.SetPremise(PremiseVD.Statement);
            ValidateStep();
        }

        private void ReduceCB_CheckedChanged(object sender, EventArgs e)
        {
            DeductionStep.Reduce = ReduceCB.Checked;
            ValidateStep();
        }

        protected void ValidateStep()
        {
            DeductionStep.Validate();
            ConseqVD.SetStatement(DeductionStep.Consequence);

            TitleStatement = DeductionStep.GetConsequence();
        }

        protected void AddLastElements()
        {
            ConseqVD = VisualisationDisplay.FromStatement(DeductionStep.GetConsequence());
            ReduceCB = new CheckBox() { Text = "Reduce", Font = Properties.Settings.Default.LabelFont, Checked = DeductionStep.Reduce };
            ReduceCB.CheckedChanged += ReduceCB_CheckedChanged;
            AddLabeledElement("Consequence:", ReduceCB);
            AddElement(ConseqVD);

            TitleStatement = DeductionStep.GetConsequence();

            Expanded = false;

            DeductionStep.ValidityChanged += DS_ValidityChanged;
        }

        public static DeductionStepDocumentElement FromDeductionStep(MDeductionStep DS, DocumentStructure DocS = null)
        {
            if (DocS == null) DocS = MainForm.ActiveMainForm.Document.Structure.GetByElement(DS);
            if (DocS == null) throw new Exception();

            DeductionStepDocumentElement DSDE;
            
            if (DS is MTrivialisationDeductionStep TDS) DSDE = TrivialisationDocumentElement.FromDeductionStep(TDS, DocS);
            else if (DS is MPredicateSpecificationDeductionStep PSDS) DSDE = PredicateSpecificationDocumentElement.FromDeductionStep(PSDS, DocS);
            else if (DS is MVariableSubstitutionDeductionStep VSDS) DSDE = VariableSubstitutionDocumentElement.FromDeductionStep(VSDS, DocS);
            else if (DS is MFormulaSubstitutionDeductionStep FSDS) DSDE = FormulaSubstitutionDocumentElement.FromDeductionStep(FSDS, DocS);
            else if (DS is MUniversalGeneralisationDeductionStep UGDS) DSDE = UniversalGeneralisationDocumentElement.FromDeductionStep(UGDS, DocS);
            else if (DS is MUniversalInstantiationDeductionStep UIDS) DSDE = UniversalInstantiationDocumentElement.FromDeductionStep(UIDS, DocS);
            else if (DS is MExistentialGeneralisationDeductionStep EGDS) DSDE = ExistentialGeneralisationDocumentElement.FromDeductionStep(EGDS, DocS);
            else if (DS is MExistentialInstantiationDeductionStep EIDS) DSDE = ExistentialInstantiationDocumentElement.FromDeductionStep(EIDS, DocS);
            else if (DS is MTermSubstitutionDeductionStep TSDS) DSDE = TermSubstitutionDocumentElement.FromDeductionStep(TSDS, DocS);
            else if (DS is MRAADeductionStep RADS) DSDE = RAADocumentElement.FromDeductionStep(RADS, DocS);
            else if (DS is MAssumptionDeductionStep ASDS) DSDE = AssumptionDocumentElement.FromDeductionStep(ASDS, DocS);
            else if (DS is MConditionInstantiationDeductionStep CIDS) DSDE = ConditionInstantiationDocumentElement.FromDeductionStep(CIDS, DocS);
            else throw new Exception("Unknown kind of DeductionStep");
            
            DSDE.labelTitle.Font = Properties.Settings.Default.StepFont;

            return DSDE;
        }

        private void DS_ValidityChanged(object sender, EventArgs e)
        {
            Valid = DeductionStep.valid;
        }

        protected override void DeleteElement()
        {
            base.DeleteElement();

            DeductionStep.Delete();
        }

        protected override void bt_hide_Click(object sender, EventArgs e)
        {
            Structure.Hidden = true;
            ParentLayoutPanel.HideRow(this);
        }
    }
    
    public class TrivialisationDocumentElement : DeductionStepDocumentElement
    {
        MTrivialisationDeductionStep Step => DeductionStep as MTrivialisationDeductionStep;
        VisualisationDisplay AddVD;
        CheckBox LeftCB;
        int BaseRowIndex;

        public new static TrivialisationDocumentElement FromDeductionStep(MTrivialisationDeductionStep DS, DocumentStructure DocS)
        {
            TrivialisationDocumentElement DE = new TrivialisationDocumentElement()
            {
                Title = "Deduction by Trivialisation.",
                DeductionStep = DS,
                Valid = DS.valid,
                Structure = DocS
            };

            DE.AddFirstElements();

            DE.AddLabel("True Formulas:");
            DE.BaseRowIndex = DE.layoutPanel.Rows.Count;
            for(int i = 0; i< DS.TrueFormulas.Count; i++)
            {
                DE.AddTrivialisation(i);
            }

            DE.AddVD = VisualisationDisplay.FromStatement(null, true, true, ElementType.Statement);
            DE.AddVD.CompleteChange += DE.AddVD_CompleteChange;
            DE.AddElement(DE.AddVD);

            DE.LeftCB = new CheckBox() { Text = "Left", Font = Properties.Settings.Default.LabelFont, Checked = DE.Step.Left };
            DE.LeftCB.CheckedChanged += DE.LeftCB_CheckedChanged;
            DE.AddElement(DE.LeftCB);

            DE.AddLastElements();

            return DE;
        }

        private void LeftCB_CheckedChanged(object sender, EventArgs e)
        {
            Step.Left = LeftCB.Checked;
            ValidateStep();
        }

        private void AddVD_CompleteChange(object sender, EventArgs e)
        {
            if (Step.CreateTrivialisation(AddVD.Statement, false))
            {
                AddTrivialisation();
                ValidateStep();
            }
            AddVD.SetStatement(null);
        }

        public void AddTrivialisation(int index = -1)
        {
            if (index == -1) index = Step.TrueFormulas.Count - 1;
            VisualisationDisplay TrueFormulaVD = VisualisationDisplay.FromStatement(Step.TrueFormulas[index], restriction: ElementType.Statement);
            Button DeleteButton = new Button() { Name = index.ToString(), Text = "X", Size = new System.Drawing.Size(20,20) };
            DeleteButton.Click += DeleteButton_Click;
            RowLayout Row = new RowLayout(TrueFormulaVD, DeleteButton);
            AddRow(Row, BaseRowIndex + index);
        }

        private void DeleteTrivialisation(int index)
        {
            layoutPanel.RemoveRow(BaseRowIndex + index, true);
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            int index = Convert.ToInt32((sender as Button).Name);
            DeleteTrivialisation(index);
            Step.DeleteTrivialisation(index, false);
            ValidateStep();
        }
    }
    public class PredicateSpecificationDocumentElement : DeductionStepDocumentElement
    {
        MPredicateSpecificationDeductionStep Step => DeductionStep as MPredicateSpecificationDeductionStep;
        VisualisationDisplay AddUndefVD, AddDefVD;
        Button AddBT;
        int BaseRowIndex;

        public new static PredicateSpecificationDocumentElement FromDeductionStep(MPredicateSpecificationDeductionStep DS, DocumentStructure DocS)
        {
            PredicateSpecificationDocumentElement DE = new PredicateSpecificationDocumentElement()
            {
                Title = "Deduction by Predicate Specification.",
                DeductionStep = DS,
                Valid = DS.valid,
                Structure = DocS
            };

            DE.AddFirstElements();

            DE.AddLabel("Specifications:");
            DE.BaseRowIndex = DE.layoutPanel.Rows.Count;
            for (int i = 0; i < DS.PredicateSpecifications.Count; i++)
            {
                DE.AddSpecification(i);
            }

            DE.AddUndefVD = VisualisationDisplay.FromStatement(null, true, true, ElementType.UndefindedPredicate);
            DE.AddDefVD = VisualisationDisplay.FromStatement(null, true, false, ElementType.Formula);
            DE.AddBT = new Button() { Text = "+", Size = new System.Drawing.Size(20, 20) };
            DE.AddBT.Click += DE.AddBT_Click;
            DE.AddRow(new RowLayout(DE.AddUndefVD, DE.AddDefVD, DE.AddBT));

            DE.AddLastElements();

            return DE;
        }

        private void AddSpecification(int index = -1)
        {
            if (index == -1) index = Step.PredicateSpecifications.Count - 1;
            VisualisationDisplay GeneralVD = VisualisationDisplay.FromExpression(Step.PredicateSpecifications.Left[index]);
            VisualisationDisplay SpecificVD = VisualisationDisplay.FromExpression(Step.PredicateSpecifications.Right[index]);
            Button DeleteButton = new Button() { Name = index.ToString(), Text = "X", Size = new System.Drawing.Size(20, 20) };
            DeleteButton.Click += DeleteButton_Click;
            RowLayout Row = new RowLayout(GeneralVD, SpecificVD, DeleteButton);
            AddRow(Row, BaseRowIndex + index);
        }

        private void DeleteSpecification(int index)
        {
            layoutPanel.RemoveRow(BaseRowIndex + index, true);
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            int index = Convert.ToInt32((sender as Button).Name);
            DeleteSpecification(index);
            Step.DeletePredicateSpecification(index, false);
            ValidateStep();
        }

        private void AddBT_Click(object sender, EventArgs e)
        {
            if (Step.CreatePredicateSpecification(AddUndefVD.Expression as MUndefinedPredicateFormula, AddDefVD.Expression as MFormula, false))
            {
                AddSpecification();
                ValidateStep();
            }
            AddUndefVD.SetStatement(null);
            AddDefVD.SetStatement(null);
        }
    }
    public class VariableSubstitutionDocumentElement : DeductionStepDocumentElement
    {
        MVariableSubstitutionDeductionStep Step => DeductionStep as MVariableSubstitutionDeductionStep;
        VisualisationDisplay AddOldVD, AddNewVD;
        Button AddAxiomaticBT, AddEqBT;
        int BaseRowIndex;

        public new static VariableSubstitutionDocumentElement FromDeductionStep(MVariableSubstitutionDeductionStep DS, DocumentStructure DocS)
        {
            VariableSubstitutionDocumentElement DE = new VariableSubstitutionDocumentElement()
            {
                Title = "Deduction by Variable Substitution.",
                DeductionStep = DS,
                Valid = DS.valid,
                Structure = DocS
            };

            DE.AddFirstElements();

            DE.AddLabel("Substitutions:");
            DE.BaseRowIndex = DE.layoutPanel.Rows.Count;
            for (int i = 0; i < DS.VariableSubstitutions.Count; i++)
            {
                DE.AddJustification(i);
            }

            DE.AddOldVD = VisualisationDisplay.FromExpression(MVariable.PlaceholderVariable, true, true, ElementType.Variable);
            DE.AddNewVD = VisualisationDisplay.FromExpression(MTerm.PlaceholderTerm, true, false, ElementType.Term);
            DE.AddAxiomaticBT = new Button() { Text = "+ (Ax)", Size = new System.Drawing.Size(50, 20) };
            DE.AddEqBT = new Button() { Text = "+ (=)", Size = new System.Drawing.Size(50, 20) };
            DE.AddAxiomaticBT.Click += DE.AddAxiomaticBT_Click;
            DE.AddEqBT.Click += DE.AddEqBT_Click;
            DE.AddRow(new RowLayout(DE.AddOldVD, DE.AddNewVD, DE.AddAxiomaticBT, DE.AddEqBT));

            DE.AddLastElements();

            return DE;
        }

        private void AddJustification(int index = -1)
        {
            if (index == -1) index = Step.VariableSubstitutions.Count - 1;

            MSubstitutionJustification J = Step.VariableSubstitutions.GetSubstitution(index);
            DocumentElement DE;
            if (J is MEqualitySubstitutionJustification EJ)
                DE = EqualitySubstitutionJustificationDocumentElement.FromJustification(EJ);
            else
                DE = AxiomaticSubstitutionJustificationDocumentElement.FromJustification(J as MAxiomaticSubstitutionJustification);

            AddElement(DE, BaseRowIndex + index);
        }

        private void AddEqBT_Click(object sender, EventArgs e)
        {
            MEqualitySubstitutionJustification J = Step.CreateEqualiticalSubstitution(AddOldVD.Expression as MVariable, AddNewVD.Expression as MTerm);

            AddJustification();
        }

        private void AddAxiomaticBT_Click(object sender, EventArgs e)
        {
            MAxiomaticSubstitutionJustification J = Step.CreateAxiomaticSubstitution(AddOldVD.Expression as MVariable, AddNewVD.Expression as MTerm);

            AddJustification();
        }
    }
    public class EqualitySubstitutionJustificationDocumentElement : DocumentElement
    {
        VisualisationDisplay OldVD, NewVD, ToShowVD;
        MEqualitySubstitutionJustification Justification;

        public static EqualitySubstitutionJustificationDocumentElement FromJustification(MEqualitySubstitutionJustification J)
        {
            EqualitySubstitutionJustificationDocumentElement DE = new EqualitySubstitutionJustificationDocumentElement()
            {
                Title = "Substitution by Equality",
                Justification = J
            };

            DE.AddLabel("Old Variable");
            DE.OldVD = VisualisationDisplay.FromExpression(J.Old, editable: false, linkonly: false, restriction: ElementType.Variable);
            DE.AddElement(DE.OldVD);

            DE.AddLabel("New Variable");
            DE.NewVD = VisualisationDisplay.FromExpression(J.New, editable: false, linkonly: false, restriction: ElementType.Variable);
            DE.AddElement(DE.NewVD);

            DE.AddLabel("Equality");
            DE.ToShowVD = VisualisationDisplay.FromStatement(J.Equality, editable: true, linkonly: true, restriction: ElementType.Statement);
            DE.ToShowVD.CompleteChange += DE.ToShowVD_Change;
            DE.ToShowVD.ExpressionChanged += DE.ToShowVD_Change;
            DE.AddElement(DE.ToShowVD);

            return DE;
        }

        private void ToShowVD_Change(object sender, EventArgs e)
        {
            if (!Justification.Justify(ToShowVD.Statement))
                ToShowVD.SetStatement(Justification.Equality);
        }
    }
    public class AxiomaticSubstitutionJustificationDocumentElement : DocumentElement
    {
        VisualisationDisplay OldVD, NewVD;
        MAxiomaticSubstitutionJustification Justification;

        public static AxiomaticSubstitutionJustificationDocumentElement FromJustification(MAxiomaticSubstitutionJustification J)
        {
            AxiomaticSubstitutionJustificationDocumentElement DE = new AxiomaticSubstitutionJustificationDocumentElement()
            {
                Title = "Axiomatic Substitution",
                Justification = J
            };

            DE.AddLabel("Old Variable");
            DE.OldVD = VisualisationDisplay.FromExpression(J.Old, editable: false, linkonly: false, restriction: ElementType.Variable);
            DE.AddElement(DE.OldVD);

            DE.AddLabel("New Variable");
            DE.NewVD = VisualisationDisplay.FromExpression(J.New, editable: false, linkonly: false, restriction: ElementType.Variable);
            DE.AddElement(DE.NewVD);

            return DE;
        }
    }

    public class FormulaSubstitutionDocumentElement : DeductionStepDocumentElement
    {
        MFormulaSubstitutionDeductionStep Step => DeductionStep as MFormulaSubstitutionDeductionStep;
        VisualisationDisplay OldVD, NewVD, ToShowVD;

        public new static FormulaSubstitutionDocumentElement FromDeductionStep(MFormulaSubstitutionDeductionStep DS, DocumentStructure DocS)
        {
            FormulaSubstitutionDocumentElement DE = new FormulaSubstitutionDocumentElement()
            {
                Title = "Deduction by Formula Substitution.",
                DeductionStep = DS,
                Valid = DS.valid,
                Structure = DocS
            };

            DE.AddFirstElements();

            DE.AddLabel("Old Formula");
            DE.OldVD = VisualisationDisplay.FromExpression(DS.GetOld(), editable: true, linkonly: false, restriction: ElementType.Formula);
            DE.OldVD.CompleteChange += DE.OldVD_Change;
            DE.OldVD.ExpressionChanged += DE.OldVD_Change;
            DE.AddElement(DE.OldVD);

            DE.AddLabel("New Formula");
            DE.NewVD = VisualisationDisplay.FromExpression(DS.GetNew(), editable: true, linkonly: false, restriction: ElementType.Formula);
            DE.NewVD.CompleteChange += DE.NewVD_Change;
            DE.NewVD.ExpressionChanged += DE.NewVD_Change;
            DE.AddElement(DE.NewVD);

            DE.AddLabel("Equivalence");
            DE.ToShowVD = VisualisationDisplay.FromStatement(DS.GetJustification(), editable: true, linkonly: true, restriction: ElementType.Statement);
            DE.ToShowVD.CompleteChange += DE.ToShowVD_Change;
            DE.ToShowVD.ExpressionChanged += DE.ToShowVD_Change;
            DE.AddElement(DE.ToShowVD);

            DE.AddLastElements();

            return DE;
        }

        private void OldVD_Change(object sender, EventArgs e)
        {
            Step.Substitute(OldVD.Expression as MFormula, Step.GetNew(), Step.GetJustification(), false);
            ValidateStep();
        }
        private void NewVD_Change(object sender, EventArgs e)
        {
            Step.Substitute(Step.GetOld(), NewVD.Expression as MFormula, Step.GetJustification(), false);
            ValidateStep();
        }
        private void ToShowVD_Change(object sender, EventArgs e)
        {
            Step.Substitute(Step.GetOld(), Step.GetNew(), ToShowVD.Statement, false);
            ValidateStep();
        }
    }

    public class TermSubstitutionDocumentElement : DeductionStepDocumentElement
    {
        MTermSubstitutionDeductionStep Step => DeductionStep as MTermSubstitutionDeductionStep;
        VisualisationDisplay OldVD, NewVD, ToShowVD;

        public new static TermSubstitutionDocumentElement FromDeductionStep(MTermSubstitutionDeductionStep DS, DocumentStructure DocS)
        {
            TermSubstitutionDocumentElement DE = new TermSubstitutionDocumentElement()
            {
                Title = "Deduction by Term Substitution.",
                DeductionStep = DS,
                Valid = DS.valid,
                Structure = DocS
            };

            DE.AddFirstElements();

            DE.AddLabel("Old Term");
            DE.OldVD = VisualisationDisplay.FromExpression(DS.GetOld(), editable: true, linkonly: false, restriction: ElementType.Term);
            DE.OldVD.CompleteChange += DE.OldVD_Change;
            DE.OldVD.ExpressionChanged += DE.OldVD_Change;
            DE.AddElement(DE.OldVD);

            DE.AddLabel("New Term");
            DE.NewVD = VisualisationDisplay.FromExpression(DS.GetNew(), editable: true, linkonly: false, restriction: ElementType.Term);
            DE.NewVD.CompleteChange += DE.NewVD_Change;
            DE.NewVD.ExpressionChanged += DE.NewVD_Change;
            DE.AddElement(DE.NewVD);

            DE.AddLabel("Equality");
            DE.ToShowVD = VisualisationDisplay.FromStatement(DS.GetJustification(), editable: true, linkonly: true, restriction: ElementType.Statement);
            DE.ToShowVD.CompleteChange += DE.ToShowVD_Change;
            DE.ToShowVD.ExpressionChanged += DE.ToShowVD_Change;
            DE.AddElement(DE.ToShowVD);

            DE.AddLastElements();

            return DE;
        }

        private void OldVD_Change(object sender, EventArgs e)
        {
            Step.Substitute(OldVD.Expression as MTerm, Step.GetNew(), Step.GetJustification(), false);
            ValidateStep();
        }
        private void NewVD_Change(object sender, EventArgs e)
        {
            Step.Substitute(Step.GetOld(), NewVD.Expression as MTerm, Step.GetJustification(), false);
            ValidateStep();
        }
        private void ToShowVD_Change(object sender, EventArgs e)
        {
            Step.Substitute(Step.GetOld(), Step.GetNew(), ToShowVD.Statement, false);
            ValidateStep();
        }
    }

    public class UniversalGeneralisationDocumentElement : DeductionStepDocumentElement
    {
        MUniversalGeneralisationDeductionStep Step => DeductionStep as MUniversalGeneralisationDeductionStep;
        VisualisationDisplay QuantifierVD, VtoquantifyVD, BoundVD;

        public new static UniversalGeneralisationDocumentElement FromDeductionStep(MUniversalGeneralisationDeductionStep DS, DocumentStructure DocS)
        {
            UniversalGeneralisationDocumentElement DE = new UniversalGeneralisationDocumentElement()
            {
                Title = "Deduction by Universal Generalisation.",
                DeductionStep = DS,
                Valid = DS.valid,
                Structure = DocS
            };

            DE.AddFirstElements();


            DE.QuantifierVD = new VisualisationDisplay()
            {
                Visualisation = DS.GetQuantifier().GetPlaceholderVisualisation(null),
                Editable = true,
                LinkOnly = true,
                Restriction = ElementType.QuantifierDefinition
            };
            DE.QuantifierVD.CompleteChange += DE.QuantifierVD_CompleteChange;
            DE.AddLabeledElement("Quantifier", DE.QuantifierVD);
            
            DE.VtoquantifyVD = VisualisationDisplay.FromExpression(DS.GetOld(), editable: true, linkonly: true, restriction: ElementType.Variable);
            DE.VtoquantifyVD.CompleteChange += DE.VtoquantifyVD_CompleteChange;
            DE.AddLabeledElement("Variable To Quantify:", DE.VtoquantifyVD);


            DE.BoundVD = VisualisationDisplay.FromExpression(DS.GetNew(), editable: true, linkonly: true, restriction: ElementType.Variable);
            DE.BoundVD.CompleteChange += DE.BoundVD_CompleteChange;
            DE.AddLabeledElement("Variable To Bind:", DE.BoundVD);

            DE.AddLastElements();

            return DE;
        }

        private void QuantifierVD_CompleteChange(object sender, EventArgs e)
        {
            Step.Generalise((QuantifierVD.Expression as MQuantifierFormula)._D as MQuantifier, Step.GetOld(), Step.GetNew());
            ValidateStep();
        }
        private void VtoquantifyVD_CompleteChange(object sender, EventArgs e)
        {
            Step.Generalise(Step.GetQuantifier(), VtoquantifyVD.Expression as MVariable, Step.GetNew());
            ValidateStep();
        }
        private void BoundVD_CompleteChange(object sender, EventArgs e)
        {
            Step.Generalise(Step.Quantifier, Step.GetOld(), BoundVD.Expression as MVariable);
            ValidateStep();
        }
    }
    public class UniversalInstantiationDocumentElement : DeductionStepDocumentElement
    {
        MUniversalInstantiationDeductionStep Step => DeductionStep as MUniversalInstantiationDeductionStep;
        VisualisationDisplay TermVD;

        public new static UniversalInstantiationDocumentElement FromDeductionStep(MUniversalInstantiationDeductionStep DS, DocumentStructure DocS)
        {
            UniversalInstantiationDocumentElement DE = new UniversalInstantiationDocumentElement()
            {
                Title = "Deduction by Universal Instantiation.",
                DeductionStep = DS,
                Valid = DS.valid,
                Structure = DocS
            };

            DE.AddFirstElements();


            DE.TermVD = VisualisationDisplay.FromExpression(DS.GetTerm(), editable: true, linkonly: true, restriction: ElementType.Term);
            DE.TermVD.CompleteChange += DE.TermVD_CompleteChange;
            DE.AddLabeledElement("Term To Instantiate:", DE.TermVD);

            DE.AddLastElements();

            return DE;
        }

        private void TermVD_CompleteChange(object sender, EventArgs e)
        {
            Step.Instantiate(TermVD.Expression as MTerm);
            ValidateStep();
        }
    }
    public class ExistentialGeneralisationDocumentElement : DeductionStepDocumentElement
    {
        MExistentialGeneralisationDeductionStep Step => DeductionStep as MExistentialGeneralisationDeductionStep;
        VisualisationDisplay QuantifierVD, TtoquantifyVD, BoundVD;

        public new static ExistentialGeneralisationDocumentElement FromDeductionStep(MExistentialGeneralisationDeductionStep DS, DocumentStructure DocS)
        {
            ExistentialGeneralisationDocumentElement DE = new ExistentialGeneralisationDocumentElement()
            {
                Title = "Deduction by Existential Generalisation.",
                DeductionStep = DS,
                Valid = DS.valid,
                Structure = DocS
            };

            DE.AddFirstElements();


            DE.QuantifierVD = new VisualisationDisplay()
            {
                Visualisation = DS.GetQuantifier().GetPlaceholderVisualisation(null),
                Editable = true,
                LinkOnly = true,
                Restriction = ElementType.QuantifierDefinition
            };
            DE.QuantifierVD.CompleteChange += DE.QuantifierVD_CompleteChange;
            DE.AddLabeledElement("Quantifier:", DE.QuantifierVD);


            DE.TtoquantifyVD = VisualisationDisplay.FromExpression(DS.GetOld(), editable: true, linkonly: true, restriction: ElementType.Term);
            DE.TtoquantifyVD.CompleteChange += DE.TtoquantifyVD_CompleteChange;
            DE.AddLabeledElement("Term To Quantify:", DE.TtoquantifyVD);


            DE.BoundVD = VisualisationDisplay.FromExpression(DS.GetNew(), editable: true, linkonly: true, restriction: ElementType.Variable);
            DE.BoundVD.CompleteChange += DE.BoundVD_CompleteChange;
            DE.AddLabeledElement("Variable To Bind:", DE.BoundVD);

            DE.AddLastElements();

            return DE;
        }

        private void QuantifierVD_CompleteChange(object sender, EventArgs e)
        {
            Step.Generalise((QuantifierVD.Expression as MQuantifierFormula)._D as MQuantifier, Step.GetOld(), Step.GetNew());
            ValidateStep();
        }
        private void TtoquantifyVD_CompleteChange(object sender, EventArgs e)
        {
            Step.Generalise(Step.GetQuantifier(), TtoquantifyVD.Expression as MTerm, Step.GetNew());
            ValidateStep();
        }
        private void BoundVD_CompleteChange(object sender, EventArgs e)
        {
            Step.Generalise(Step.Quantifier, Step.GetOld(), BoundVD.Expression as MVariable);
            ValidateStep();
        }
    }
    public class ExistentialInstantiationDocumentElement : DeductionStepDocumentElement
    {
        MExistentialInstantiationDeductionStep Step => DeductionStep as MExistentialInstantiationDeductionStep;
        VisualisationDisplay VarVD;

        public new static ExistentialInstantiationDocumentElement FromDeductionStep(MExistentialInstantiationDeductionStep DS, DocumentStructure DocS)
        {
            ExistentialInstantiationDocumentElement DE = new ExistentialInstantiationDocumentElement()
            {
                Title = "Deduction by Existential Instantiation.",
                DeductionStep = DS,
                Valid = DS.valid,
                Structure = DocS
            };

            DE.AddFirstElements();


            DE.VarVD = VisualisationDisplay.FromExpression(DS.GetVariable(), editable: true, linkonly: true, restriction: ElementType.Variable);
            DE.VarVD.CompleteChange += DE.VarVD_CompleteChange;
            DE.AddLabeledElement("Variable To Instantiate:", DE.VarVD);

            DE.AddLastElements();

            return DE;
        }

        private void VarVD_CompleteChange(object sender, EventArgs e)
        {
            Step.Instantiate(VarVD.Expression as MVariable);
            ValidateStep();
        }
    }

    public class RAADocumentElement : DeductionStepDocumentElement
    {
        MRAADeductionStep Step => DeductionStep as MRAADeductionStep;
        VisualisationDisplay CondVD;

        public new static RAADocumentElement FromDeductionStep(MRAADeductionStep DS, DocumentStructure DocS)
        {
            RAADocumentElement DE = new RAADocumentElement()
            {
                Title = "Reductio Ad Absurdum.",
                DeductionStep = DS,
                Valid = DS.valid,
                Structure = DocS
            };

            DE.AddFirstElements();

            DE.AddLabel("Condition");
            DE.CondVD = VisualisationDisplay.FromStatement(DS.GetCondition(), editable: true, linkonly: true, restriction: ElementType.Statement);
            DE.CondVD.CompleteChange += DE.CondVD_Change;
            DE.CondVD.ExpressionChanged += DE.CondVD_Change;
            DE.AddElement(DE.CondVD);

            DE.AddLastElements();

            return DE;
        }
        
        private void CondVD_Change(object sender, EventArgs e)
        {
            Step.RAA(CondVD.Statement, false);
            ValidateStep();
        }
    }
    public class AssumptionDocumentElement : DeductionStepDocumentElement
    {
        MAssumptionDeductionStep Step => DeductionStep as MAssumptionDeductionStep;
        VisualisationDisplay AssVD;
        CheckedListBox RestrictCLB;
        bool updating = false;

        public new static AssumptionDocumentElement FromDeductionStep(MAssumptionDeductionStep DS, DocumentStructure DocS)
        {
            AssumptionDocumentElement DE = new AssumptionDocumentElement()
            {
                Title = "Assumption.",
                DeductionStep = DS,
                Valid = DS.valid,
                Structure = DocS
            };

            DE.AddFirstElements();

            DE.AddLabel("Assumption");
            DE.AssVD = VisualisationDisplay.FromExpression(DS.GetAssumption(), editable: true, linkonly: false, restriction: ElementType.Formula);
            DE.AssVD.CompleteChange += DE.AssVD_Change;
            DE.AssVD.ExpressionChanged += DE.AssVD_Change;
            DE.AddElement(DE.AssVD);
            
            DE.RestrictCLB = new CheckedListBox();
            DE.RestrictCLB.ItemCheck += DE.RestrictCLB_Check;
            DE.UpdateRestrictCLB();
            DE.AddLabeledElement("Restrict:", DE.RestrictCLB);

            DE.AddLastElements();

            return DE;
        }
        
        private void AssVD_Change(object sender, EventArgs e)
        {
            UpdateRestrictCLB();
            Step.Assume(AssVD.Expression as MFormula, GetRList(), false);
            ValidateStep();
        }

        private void UpdateRestrictCLB()
        {
            updating = true;
            RestrictCLB.Items.Clear();
            List<MVariable> VList = Step.GetAssumption().MakeFreeVariableList();
            for( int i = 0; i< VList.Count; i++)
            {
                RestrictCLB.Items.Add(VList[i].stringSymbol);
                RestrictCLB.SetItemChecked(i, Step.GetRestrictedVars().Contains(i));
            }
            updating = false;
        }

        private List<int> GetRList(int neg = -1)
        {
            List<int> rList = new List<int>();
            for (int i = 0; i < RestrictCLB.Items.Count; i++)
            {
                if (i == neg)
                {
                    if (!RestrictCLB.GetItemChecked(i))
                        rList.Add(i);
                }
                else
                {
                    if (RestrictCLB.GetItemChecked(i))
                        rList.Add(i);
                }
            }
            return rList;
        }

        private void RestrictCLB_Check(object sender, ItemCheckEventArgs e)
        {
            if (updating) return;
            
            Step.Assume(AssVD.Expression as MFormula, GetRList(e.Index), false);
            ValidateStep();
        }
    }
    public class ConditionInstantiationDocumentElement : DeductionStepDocumentElement
    {
        MConditionInstantiationDeductionStep Step => DeductionStep as MConditionInstantiationDeductionStep;
        VisualisationDisplay CondVD, ImpVD;

        public new static ConditionInstantiationDocumentElement FromDeductionStep(MConditionInstantiationDeductionStep DS, DocumentStructure DocS)
        {
            ConditionInstantiationDocumentElement DE = new ConditionInstantiationDocumentElement()
            {
                Title = "Condition Instantiation.",
                DeductionStep = DS,
                Valid = DS.valid,
                Structure = DocS
            };

            DE.AddFirstElements();

            DE.AddLabel("Condition");
            DE.CondVD = VisualisationDisplay.FromStatement(DS.GetCondition(), editable: true, linkonly: true, restriction: ElementType.Statement);
            DE.CondVD.CompleteChange += DE.CondVD_Change;
            DE.CondVD.ExpressionChanged += DE.CondVD_Change;
            DE.AddElement(DE.CondVD);

            DE.AddLabel("Implication.");
            DE.ImpVD = VisualisationDisplay.FromExpression(DS.GetImplication(), editable: true, linkonly: false, restriction: ElementType.Formula);
            DE.ImpVD.CompleteChange += DE.ImpVD_Change;
            DE.ImpVD.ExpressionChanged += DE.ImpVD_Change;
            DE.AddElement(DE.ImpVD);

            DE.AddLastElements();

            return DE;
        }

        private void ImpVD_Change(object sender, EventArgs e)
        {
            Step.Instantiate(Step.GetCondition(), ImpVD.Expression as MFormula, false);
            ValidateStep();
        }

        private void CondVD_Change(object sender, EventArgs e)
        {
            Step.Instantiate(CondVD.Statement, invalidate:false);
            ValidateStep();
        }
    }
}
