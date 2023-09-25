using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;

namespace TraceBackend
{
    public partial class MTheorem : MObject, IElementWithFileID
    {
        public event EventHandler StatementChanged;
        public event EventHandler ValidityChanged;

        public bool loaded = false;

        public TheoremFileID fileID;
        public FileID baseFileID => fileID;
        public MContext Context;
        public MDeduction Deduction;
        public MStatement Statement;

        public Validity valid => Statement?.valid ?? Validity.Valid;

        public string LaTeXStart => @"\begin{theorem}";
        public string LaTeXEnd => @"\end{theorem}";

        public void Invalidate(bool sendEvents = true)
        {
            if (loaded)
            {
                valid.Invalidate(sendEvents);
            }
        }

        private MTheorem(MContext X) : base()
        {
            Context = X;
        }

        internal MTheorem(TheoremFileID ID, MContext X) : this(X)
        {
            fileID = ID;
        }

        internal MTheorem(FileIDManager IDM, MContext X) : this(X)
        {
            fileID = IDM.RequestTheoremFileID(this);
        }

        public void SetStatement(MStatement S, bool validate = false)
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");

            Statement = S;

            if (validate)
                Validate(false);

            StatementChanged?.Invoke(this, EventArgs.Empty);
        }

        public void SetStatement(MFormula F, bool validate = false)
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");

            Statement.SetFormula(F);

            if (validate)
                Validate(false);

            StatementChanged?.Invoke(this, EventArgs.Empty);
        }

        public MStatement CreateStatement(MFormula F)
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");

            Statement = new MStatement(fileID.IDManager, this, Context, F, Validity.Invalid)
            {
                loaded = true
            };
            return Statement;
        }

        internal void SetDeduction(MDeduction deduction, bool invalidate = true)
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");

            if(invalidate) Invalidate();
            Deduction = deduction;
            Deduction.ValidityChanged += Deduction_ValidityChanged;
            Deduction.ConsequenceChanged += Deduction_ConsequenceChanged;
        }

        public void RemoveDeduction(MDeduction deduction)
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");

            //TODO: this is relevant when we allow multiple deductions
        }

        private void Deduction_ConsequenceChanged(object sender, EventArgs e)
        {
            Validate(false);
        }

        private void Deduction_ValidityChanged(object sender, EventArgs e)
        {
            if (!Deduction.valid.IsValid) Invalidate();
            else Validate(false);
        }

        internal void TransferToNewContext(MContext X)
        {
            Context = X;
            fileID.ApplyContext(X);
            Statement.TransferToNewContext(X);
            Deduction.TransferToNewContext(X);
        }

        public MDeduction CreateDeduction()
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");

            SetDeduction(new MDeduction(fileID.IDManager, Context, this));
            Deduction.loaded = true;
            return Deduction;
        }

        public MStatement GetStatement()
        {
            if (Statement == null) LoadStatement();
            return Statement;
        }
        public MDeduction GetDeduction()
        {
            if (Deduction == null) LoadDeduction(); //TODO: variable: hasdeduction
            return Deduction;
        }

        public Validity Validate(bool checkDeduction = true)
        {
            if (loaded)
            {
                Validity prevValid = valid;

                if (Deduction == null || Deduction.Steps.Count == 0)
                {
                    if (Statement.IsTautology()) return returnVal(Validity.Valid, prevValid); 
                    else return returnVal(Validity.Invalid, prevValid); 
                }

                if (!(Statement._X.ContainsSuperContext(Deduction._X)))
                    return returnVal(Validity.Invalid, prevValid);
                if (checkDeduction) Deduction.Validate();
                if (!Deduction.valid.IsValid)
                    return returnVal(Validity.Invalid, prevValid);
                if (!Statement.Identical(Deduction._S))
                    return returnVal(Validity.Invalid, prevValid);
                
                return returnVal(Deduction.valid.Copy(), prevValid);
            }
            return valid;

            Validity returnVal(Validity b, Validity prevb)
            {
                Statement.valid = b;

                if (!prevb.Equivalent(b)) ValidityChanged?.Invoke(this, EventArgs.Empty);
                return b;
            }
        }

        public override string ToString()
        {
            if (Deduction == null)
                return "Theorem.\n" + Helper.Indent(Statement.ToString()) + "\nProven through evaluation.\n";
            return "Theorem.\n" + Helper.Indent(Statement.ToString()) + "\nProof.\n" + Helper.Indent(Deduction.ToString()) + "\n";
        }
    }

    public partial class MDeduction : MObject, IElementWithFileID
    {
        public event EventHandler ConsequenceChanged;
        public event EventHandler ValidityChanged;

        public bool loaded = false;

        public DeductionFileID fileID;
        public FileID baseFileID => fileID;
        public MContext _X;
        public MTheorem Theorem;
        public List<MDeductionStep> Steps;
        public MStatement _S => Steps?.LastOrDefault()?.Consequence;

        public Validity valid => _S?.valid ?? Validity.Valid;

        public string LaTeXStart => @"\begin{proof}";
        public string LaTeXEnd => @"\end{proof}";

        private MDeduction(MContext X, MTheorem T) : base()
        {
            //Invalidate();
            _X = X;
            Theorem = T;
            Steps = new List<MDeductionStep>();
        }

        internal MDeduction(DeductionFileID ID, MContext X, MTheorem T) : this(X, T)
        {
            fileID = ID;
        }

        internal MDeduction(FileIDManager IDM, MContext X, MTheorem T) : this(X, T)
        {
            fileID = IDM.RequestDeductionFileID(this);
        }

        internal void AddStep(MDeductionStep S, int index = -1)
        {
            if (index != -1)
            {
                Steps.Insert(index, S);
                /*for (int i = index + 1; i < Steps.Count; i++)
                    Steps[i].Invalidate();*/
            }
            else
            {
                Steps.Add(S);
                //Invalidate();
            }
            S.ValidityChanged += Step_ValidityChanged;
        }

        public void RemoveStep(MDeductionStep DS)
        {
            DS.Invalidate();
            DS.ValidityChanged -= Step_ValidityChanged;
            Steps.Remove(DS);
        }

        internal void TransferToNewContext(MContext X)
        {
            _X = X;
            fileID.ApplyContext(X);
            foreach(MDeductionStep DS in Steps)
                DS.TransferToNewContext(X);
        }

        private void Step_ValidityChanged(object sender, EventArgs e)
        {
            if (Steps[Steps.Count - 1] == (sender as MDeductionStep))
            {
                ValidityChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        
        public MPredicateSpecificationDeductionStep CreatePredicateSpecificatingStep(MStatement premise, int index = -1)
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");
            MPredicateSpecificationDeductionStep DS = new MPredicateSpecificationDeductionStep(fileID.IDManager, this, premise);
            AddStep(DS, index);
            DS.loaded = true;
            DS.Consequence = new MStatement(fileID.IDManager, DS, _X, MTrivialFormula._True, Validity.Valid);
            return DS;
        }
        public MVariableSubstitutionDeductionStep CreateSubstitutingStep(MStatement premise, int index = -1)
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");
            MVariableSubstitutionDeductionStep DS = new MVariableSubstitutionDeductionStep(fileID.IDManager, this, premise);
            AddStep(DS, index);
            DS.loaded = true;
            DS.Consequence = new MStatement(fileID.IDManager, DS, _X, MTrivialFormula._True, Validity.Valid);
            return DS;
        }
        public MTrivialisationDeductionStep CreateTrivialisingStep(MStatement premise, int index = -1)
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");
            MTrivialisationDeductionStep DS = new MTrivialisationDeductionStep(fileID.IDManager, this, premise);
            AddStep(DS, index);
            DS.loaded = true;
            DS.Consequence = new MStatement(fileID.IDManager, DS, _X, MTrivialFormula._True, Validity.Valid);
            return DS;
        }
        public MUniversalGeneralisationDeductionStep CreateUniversallyGeneralisingStep(MStatement premise, int index = -1)
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");
            MUniversalGeneralisationDeductionStep DS = new MUniversalGeneralisationDeductionStep(fileID.IDManager, this, premise);
            AddStep(DS, index);
            DS.loaded = true;
            DS.Consequence = new MStatement(fileID.IDManager, DS, _X, MTrivialFormula._True, Validity.Valid);
            DS.Quantifier = MQuantifier.PlaceholderQuantifier;
            DS.Old = MVariable.PlaceholderVariable;
            DS.New = MVariable.PlaceholderVariable;
            return DS;
        }
        public MUniversalInstantiationDeductionStep CreateUniversallyInstantiatingStep(MStatement premise, int index = -1)
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");
            MUniversalInstantiationDeductionStep DS = new MUniversalInstantiationDeductionStep(fileID.IDManager, this, premise);
            AddStep(DS, index);
            DS.loaded = true;
            DS.Consequence = new MStatement(fileID.IDManager, DS, _X, MTrivialFormula._True, Validity.Valid);
            DS.Quantifier = MQuantifier.PlaceholderQuantifier;
            DS.T = MTerm.PlaceholderTerm;
            return DS;
        }
        public MExistentialGeneralisationDeductionStep CreateExistentiallyGeneralisingStep(MStatement premise, int index = -1)
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");
            MExistentialGeneralisationDeductionStep DS = new MExistentialGeneralisationDeductionStep(fileID.IDManager, this, premise);
            AddStep(DS, index);
            DS.loaded = true;
            DS.Consequence = new MStatement(fileID.IDManager, DS, _X, MTrivialFormula._True, Validity.Valid);
            DS.Quantifier = MQuantifier.PlaceholderQuantifier;
            DS.Old = MTerm.PlaceholderTerm;
            DS.New = MVariable.PlaceholderVariable;
            return DS;
        }
        public MExistentialInstantiationDeductionStep CreateExistentiallyInstantiatingStep(MStatement premise, int index = -1)
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");
            MExistentialInstantiationDeductionStep DS = new MExistentialInstantiationDeductionStep(fileID.IDManager, this, premise);
            AddStep(DS, index);
            DS.loaded = true;
            DS.Consequence = new MStatement(fileID.IDManager, DS, _X, MTrivialFormula._True, Validity.Valid);
            DS.Quantifier = MQuantifier.PlaceholderQuantifier;
            DS.V = MVariable.PlaceholderVariable;
            return DS;
        }
        public MFormulaSubstitutionDeductionStep CreateFormulaSubstitutingStep(MStatement premise, int index = -1)
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");
            MFormulaSubstitutionDeductionStep DS = new MFormulaSubstitutionDeductionStep(fileID.IDManager, this, premise);
            AddStep(DS, index);
            DS.loaded = true;
            DS.Consequence = new MStatement(fileID.IDManager, DS, _X, MTrivialFormula._True, Validity.Valid);
            DS.Old = MFormula.PlaceholderFormula;
            DS.New = MFormula.PlaceholderFormula;
            DS.Justification = premise;
            return DS;
        }
        public MTermSubstitutionDeductionStep CreateTermSubstitutingStep(MStatement premise, int index = -1)
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");
            MTermSubstitutionDeductionStep DS = new MTermSubstitutionDeductionStep(fileID.IDManager, this, premise);
            AddStep(DS, index);
            DS.loaded = true;
            DS.Consequence = new MStatement(fileID.IDManager, DS, _X, MTrivialFormula._True, Validity.Valid);
            DS.Old = MTerm.PlaceholderTerm;
            DS.New = MTerm.PlaceholderTerm;
            DS.Justification = premise;
            return DS;
        }
        public MRAADeductionStep CreateRAAStep(MStatement premise, int index = -1)
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");
            MRAADeductionStep DS = new MRAADeductionStep(fileID.IDManager, this, premise);
            AddStep(DS, index);
            DS.loaded = true;
            DS.Consequence = new MStatement(fileID.IDManager, DS, _X, MTrivialFormula._True, Validity.Valid);
            DS.Condition = premise;
            return DS;
        }
        public MAssumptionDeductionStep CreateAssumptionStep(MStatement premise, int index = -1)
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");
            MAssumptionDeductionStep DS = new MAssumptionDeductionStep(fileID.IDManager, this, premise);
            AddStep(DS, index);
            DS.loaded = true;
            DS.Consequence = new MStatement(fileID.IDManager, DS, _X, MTrivialFormula._True, Validity.Valid);
            DS.Assumption = MFormula.PlaceholderFormula;
            return DS;
        }
        public MConditionInstantiationDeductionStep CreateConditionInstantiatingStep(MStatement premise, int index = -1)
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");
            MConditionInstantiationDeductionStep DS = new MConditionInstantiationDeductionStep(fileID.IDManager, this, premise);
            AddStep(DS, index);
            DS.loaded = true;
            DS.Consequence = new MStatement(fileID.IDManager, DS, _X, MTrivialFormula._True, Validity.Valid);
            DS.Implication = null; // what else?
            DS.Condition = premise;
            return DS;
        }

        public MDeductionStep GetStep(int index)
        {
            if (Steps[index] == null) LoadStep(index);

            return Steps[index];
        }
        public MDeductionStep GetStep(DeductionStepFileID ID)
        {
            //Look for loaded Steps
            foreach (MDeductionStep DS in Steps)
                if (DS != null && DS.fileID.IsSubFileID(ID)) return DS;

            if (loaded) return null;
            //Look for unloaded Steps
            return LoadStep(ID);
        }

        public Validity Validate(int startstep = 0)
        {
            if (!loaded) return valid;

            //Invalidate(false);
            for (int i = startstep; i < Steps.Count; i++)
                Steps[i].Validate();

            return valid;
        }

        internal void RaiseConsequenceChanged()
        {
            ConsequenceChanged?.Invoke(this, EventArgs.Empty);
            ValidityChanged?.Invoke(this, EventArgs.Empty);
        }

        public override string ToString()
        {
            string ret = "";
            for (int i = 0; i < Steps.Count; i++)
            {
                ret = ret + "Step " + (i + 1).ToString() + ":\n" + Helper.Indent(Steps[i].ToString()) + (i == Steps.Count - 1 ? "\nQ.E.D." : "\n");
            }
            return ret;
        }
    }

    public partial class MDeductionStep : MObject, IElementWithFileID
    {
        public event EventHandler PremiseChanged; // This is only invoked when the premise gets changed automatically.
        public event EventHandler ValidityChanged;

        public bool loaded = false;

        public DeductionStepFileID fileID;
        public FileID baseFileID => fileID;
        public MDeduction _D;
        public MStatement Premise;
        public MStatement Consequence;
        public bool Reduce;
        protected bool IgnorePremiseValidity;

        public Validity valid => Consequence?.valid ?? Validity.Invalid;

        public int Index => _D.Steps.IndexOf(this);
        public bool Final => _D.Steps.Count == Index + 1;

        public string LaTeXStart => 
@"\begin{equation}
" + Consequence._F.Visualization.GetLaTeX() + @"
\end{equation}";
        public string LaTeXEnd => @"";

        bool IsProcessed = false;

        public void Invalidate()
        {
            if (loaded && Consequence != null)
            {
                Consequence.valid = Validity.Invalid; // always sends events.
            }
        }

        protected MDeductionStep(MDeduction D, MStatement premise)
        {
            Invalidate();
            _D = D;

            SetPremise(premise);
        }

        public void SetPremise (MStatement premise)
        {
            if (premise != null)
                if (!_D._X.AddSuperContext(premise._X)) return;
            
            if (Premise != null) Premise.ValidityChanged -= Premise_ValidityChanged;

            Premise = premise;

            if (Premise != null) Premise.ValidityChanged += Premise_ValidityChanged;
        }

        private void Premise_ValidityChanged(object sender, EventArgs e)
        {
            MStatement newPremise = Premise.fileID.FindStatement();
            if (Premise == newPremise || Premise.Identical(newPremise))
            {
                // Change is just in validity
                Premise.ValidityChanged -= Premise_ValidityChanged;
                newPremise.ValidityChanged += Premise_ValidityChanged;
                Premise = newPremise;
                UpdateValidity();
            }
            else
            {
                Premise.ValidityChanged -= Premise_ValidityChanged;
                newPremise.ValidityChanged += Premise_ValidityChanged;
                Premise = newPremise;

                Validate();

                PremiseChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        internal virtual void SecondaryPremise_ValidityChanged(object sender, EventArgs e) 
        {
            // Do nothing
        }

        internal void TransferToNewContext(MContext X)
        {
            fileID.ApplyContext(X);
            Consequence.TransferToNewContext(X);
        }

        public MStatement GetConsequence()
        {
            if (Consequence == null) LoadConsequence();
            return Consequence;
        }

        public Validity Validate()
        {
            if (!loaded) return valid;

            //Invalidate(false);
            IsProcessed = false;

            MStatement OldConsequence = Consequence;

            // Check if Argument is in Context
            if (!_D._X.ContainsSuperContext(Premise._X))
                return ret(Validity.Invalid, OldConsequence.valid);
            // Check if Argument is true
            if (!Premise.valid.IsValid)
                return ret(Validity.Invalid, OldConsequence.valid);

            Validity valChildren = ValidateChildren(out MFormula ProcessedArgument);
            if (!valChildren.IsValid) return ret(Validity.Invalid, OldConsequence.valid);

            if (Reduce) ProcessedArgument = ProcessedArgument.Reduce();
            ProcessedArgument.UpdateBinding();
            
            if(!IgnorePremiseValidity)
                valChildren = Premise.valid & valChildren;
            valChildren.IsAxiom = false;
            Consequence = new MStatement(Consequence.fileID, _D._X, ProcessedArgument, valChildren) { loaded = true };
            if (this is MExistentialInstantiationDeductionStep EI)
            {
                EI.GetVariable().AxiomAdded -= SecondaryPremise_ValidityChanged;  // We want to send events, but do not want this particular step to react to the event.
                Consequence.valid.IsAxiom = true;   // This is a choice we impose on a variable, i. e. let x=3. after this, we cannot make another choice (let x=2).
                Consequence.AddRestrictedVariable(EI.GetVariable());
                EI.GetVariable().AxiomAdded += SecondaryPremise_ValidityChanged;
            }
            if (this is MAssumptionDeductionStep A)   // An assumption is a self-dependent Axiom.
            {
                Consequence.valid.IsAxiom = true;
                Consequence.valid.AddDependence(Consequence);
                List<MVariable> RestrList = A.Assumption.MakeFreeVariableList();
                for (int i = 0; i < RestrList.Count; i++)
                    if (A.RestrictedVars.Contains(i))
                        Consequence.AddRestrictedVariable(RestrList[i]);
            }

            if (!OldConsequence.valid.Equivalent(Consequence.valid))
                ValidityChanged?.Invoke(this, EventArgs.Empty);
            OldConsequence.valid.ValidityChanged -= Consequence_ValidityChanged;
            if (OldConsequence == Consequence) throw new Exception();
            if (OldConsequence.valid == Consequence.valid) throw new Exception();
            OldConsequence.valid = Validity.Invalid; //this way we always get a validityChanged event
            Consequence.valid.ValidityChanged += Consequence_ValidityChanged;

            IsProcessed = true;

            return ret(Consequence.valid, OldConsequence.valid);

            Validity ret(Validity v, Validity oldV)
            {
                bool change = !v.Equivalent(oldV);
                if (!v.IsValid) Invalidate();
                if (change)
                {
                    ValidityChanged?.Invoke(this, EventArgs.Empty);
                    if (Final) _D.RaiseConsequenceChanged();
                }

                return valid;
            }
        }

        public Validity UpdateValidity()
        {
            //TODO change
            return Validate();

            if (!IsProcessed) return Validate();

            Validity ret(Validity v, Validity oldV)
            {
                bool change = !v.Equivalent(oldV);
                if (!v.IsValid) Invalidate();
                if (change)
                {
                    ValidityChanged?.Invoke(this, EventArgs.Empty);
                    if (Final) _D.RaiseConsequenceChanged();
                }

                return valid;
            }
        }

        private void Consequence_ValidityChanged(object sender, EventArgs e)
        {
            ValidityChanged?.Invoke(this, e);
        }

        protected virtual Validity ValidateChildren(out MFormula ProcessedArgument)
        {
            throw new Exception("Call only on children");
        }

        public void Delete()
        {
            if (Consequence != null)
                Consequence.valid = Validity.Invalid;

            _D.RemoveStep(this);
            SetPremise(null); //TODO: Inherit this Method and remove secondary Premises
        }
    }
    
    #region DeductionSteps
    public partial class MPredicateSpecificationDeductionStep : MDeductionStep
    {
        public IdTable<MUndefinedPredicateFormula, MFormula> PredicateSpecifications;

        private MPredicateSpecificationDeductionStep(MDeduction D, MStatement premise) : base(D, premise)
        {
            PredicateSpecifications = new IdTable<MUndefinedPredicateFormula, MFormula>(IdRuleset.LeftUnique);
        }

        internal MPredicateSpecificationDeductionStep(DeductionStepFileID ID, MDeduction D, MStatement premise) : this(D, premise)
        { fileID = ID; }
        internal MPredicateSpecificationDeductionStep(FileIDManager IDM, MDeduction D, MStatement premise) : this(D, premise)
        { fileID = IDM.RequestDeductionStepFileID(this); }

        public bool CreatePredicateSpecification(MUndefinedPredicateFormula up, MFormula F, bool invalidate = true)
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");
            
            //The PseudoDefinition must be unique, unique UndefinedPredicateFormulas are not sufficient.
            foreach (MUndefinedPredicateFormula l in PredicateSpecifications.Left)
                if (l.PseudoDefinition == up.PseudoDefinition) return false;

            if(invalidate) Invalidate();
            return PredicateSpecifications.Identify(up, F);
        }

        public void DeletePredicateSpecification(int index, bool invalidate = true)
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");

            PredicateSpecifications.removePair(index);

            if (invalidate) Invalidate();
        }

        public IdTable<MUndefinedPredicateFormula, MFormula> GetPredicateSpecifications()
        {
            if (PredicateSpecifications == null) Load();
            return PredicateSpecifications;
        }

        protected override Validity ValidateChildren(out MFormula ProcessedArgument)
        {
            ProcessedArgument = Premise._F.ReplacePredicates(PredicateSpecifications) as MFormula;
            return Validity.Valid;
        }

        public override string ToString()
        {
            string ret = "Deduction by Predicate Specification.\n";
            ret = ret + "Premise: " + Premise.ToString() +
                "\nSpecifications:\n" + Helper.Indent(PredicateSpecifications.ToString());
            ret = ret + "\n" + (Reduce ? "Reduced " : "") + "Consequence: " + Consequence.ToString() + "\n";
            return ret;
        }
    }

    public partial class MVariableSubstitutionDeductionStep : MDeductionStep
    {
        public MVariableSubstitutions VariableSubstitutions;

        private MVariableSubstitutionDeductionStep(MDeduction D, MStatement premise) : base(D, premise)
        {
            VariableSubstitutions = new MVariableSubstitutions(this);
        }

        internal MVariableSubstitutionDeductionStep(DeductionStepFileID ID, MDeduction D, MStatement premise) : this(D, premise)
        { fileID = ID; }
        internal MVariableSubstitutionDeductionStep(FileIDManager IDM, MDeduction D, MStatement premise) : this(D, premise)
        { fileID = IDM.RequestDeductionStepFileID(this); }

        public MAxiomaticSubstitutionJustification CreateAxiomaticSubstitution(MVariable Old, MTerm New)
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");
            Invalidate();
            return VariableSubstitutions.SubstituteAxiomatically(Old, New);
        }

        public MEqualitySubstitutionJustification CreateEqualiticalSubstitution(MVariable Old, MTerm New)
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");
            Invalidate();
            return VariableSubstitutions.SubstituteEqualitically(Old, New);
        }

        public MVariableSubstitutions GetVariableSubstitutions()
        {
            if (VariableSubstitutions == null) Load();
            return VariableSubstitutions;
        }

        protected override Validity ValidateChildren(out MFormula ProcessedArgument)
        {
            Validity v = VariableSubstitutions.AreValid();
            if(!v.IsValid) { ProcessedArgument = Premise.GetFormula(); return Validity.Invalid; }

            ProcessedArgument = Premise._F.ReplaceVariables(VariableSubstitutions.Substitutions) as MFormula;
            return v;
        }

        internal override void SecondaryPremise_ValidityChanged(object sender, EventArgs e)
        {
            bool realchange = false;
            for (int i = 0; i < VariableSubstitutions.Justifications.Count; i++)
            {
                if (VariableSubstitutions.Justifications[i] is MEqualitySubstitutionJustification EqJ)
                {
                    MStatement newEqJ = EqJ.Equality.fileID.FindStatement();

                    if (EqJ.Equality == newEqJ || EqJ.Equality.Identical(newEqJ))
                    {
                    }
                    else
                    {
                        realchange = true;
                    }
                    EqJ.Equality.ValidityChanged -= SecondaryPremise_ValidityChanged;
                    EqJ.Equality.ExpressionChanged -= SecondaryPremise_ValidityChanged;
                    newEqJ.ValidityChanged += SecondaryPremise_ValidityChanged;
                    newEqJ.ExpressionChanged += SecondaryPremise_ValidityChanged;
                    EqJ.Equality = newEqJ;
                }
            }

            if (!realchange)
            {
                // Change is just in validity
                UpdateValidity();
            }
            else
            {
                Validate();
            }
        }

        public override string ToString()
        {
            string ret = "Deduction by Substitution.\n";
            ret = ret + "Premise: " + Premise.ToString() +
                "\nSubstitutions:\n" + Helper.Indent(VariableSubstitutions.ToString());
            ret = ret + "\n" + (Reduce ? "Reduced " : "") + "Consequence: " + Consequence.ToString() + "\n";
            return ret;
        }
    }

    public partial class MTrivialisationDeductionStep : MDeductionStep
    {
        public List<MStatement> TrueFormulas;
        public bool Left;

        private MTrivialisationDeductionStep(MDeduction D, MStatement premise) : base(D, premise)
        {
            TrueFormulas = new List<MStatement>();
        }

        internal MTrivialisationDeductionStep(DeductionStepFileID ID, MDeduction D, MStatement premise) : this(D, premise)
        { fileID = ID; }
        internal MTrivialisationDeductionStep(FileIDManager IDM, MDeduction D, MStatement premise) : this(D, premise)
        { fileID = IDM.RequestDeductionStepFileID(this); }

        public bool CreateTrivialisation(MStatement trueStatement, bool invalidate = true)
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");
            if(invalidate) Invalidate();

            if (!_D._X.AddSuperContext(trueStatement._X))
                return false;
            if (!TrueFormulas.Contains(trueStatement))
            {
                TrueFormulas.Add(trueStatement);
                trueStatement.ValidityChanged += SecondaryPremise_ValidityChanged;
                trueStatement.ExpressionChanged += SecondaryPremise_ValidityChanged;
            }
            return true;
        }

        public void DeleteTrivialisation(int index, bool invalidate = true)
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");
            TrueFormulas[index].ValidityChanged -= SecondaryPremise_ValidityChanged;
            TrueFormulas[index].ExpressionChanged -= SecondaryPremise_ValidityChanged;
            TrueFormulas.RemoveAt(index);
            if (invalidate) Invalidate();
        }

        public List<MStatement> GetTrueFormulas()
        {
            if (TrueFormulas == null) Load();
            return TrueFormulas;
        }

        protected override Validity ValidateChildren(out MFormula ProcessedArgument)
        {
            // Check if Trivialisations are in Context and True
            Validity v = Validity.Valid;
            for (int i = 0; i < TrueFormulas.Count; i++)
            {
                MStatement castA = TrueFormulas[i];
                if (!_D._X.ContainsSuperContext(castA._X)) { ProcessedArgument = Premise.GetFormula(); return Validity.Invalid; }
                 v = v & castA.valid;
            }

            if (Left && Premise._F is MBinaryConnectiveFormula BC && 
                ((BC._D as MBinaryConnective).IsImplication || (BC._D as MBinaryConnective).IsEquivalence))
            {
                MBinaryConnectiveFormula F = Premise._F.Copy() as MBinaryConnectiveFormula;
                F._F[0] = F._F[0].SetTrueFormulas(TrueFormulas) as MFormula;
                ProcessedArgument = F;
            }
            else
                ProcessedArgument = Premise._F.SetTrueFormulas(TrueFormulas) as MFormula;
            return v;
            
        }

        internal override void SecondaryPremise_ValidityChanged(object sender, EventArgs e)
        {
            bool realchange = false;
            for (int i = 0; i < TrueFormulas.Count; i++)
            {
                MStatement newTF = TrueFormulas[i].fileID.FindStatement();
                
                if (TrueFormulas[i] == newTF || TrueFormulas[i].Identical(newTF))
                {
                    
                }
                else
                {
                    realchange = true;
                }
                TrueFormulas[i].ValidityChanged -= SecondaryPremise_ValidityChanged;
                newTF.ValidityChanged += SecondaryPremise_ValidityChanged;
                TrueFormulas[i] = newTF;
            }

            if(!realchange){
                // Change is just in validity
                UpdateValidity();
            }
            else
            {
                Validate();
            }
        }

        public override string ToString()
        {
            string ret = "Deduction by Trivialisation.\n";
            string tf = "";
            for (int i = 0; i < TrueFormulas.Count; i++)
                tf = tf + TrueFormulas[i].ToString() + (i == TrueFormulas.Count - 1 ? "" : "\n");
            ret = ret + "Premise: " + Premise.ToString() +
                "\nUsing True Statements:\n" + Helper.Indent(tf);
            ret = ret + "\n" + (Reduce ? "Reduced " : "") + "Consequence: " + Consequence.ToString() + "\n";
            return ret;
        }
    }

    public partial class MUniversalGeneralisationDeductionStep : MDeductionStep
    {
        public MVariable Old, New;
        public MQuantifier Quantifier;

        private MUniversalGeneralisationDeductionStep(MDeduction D, MStatement premise) : base(D, premise)
        {
        }

        internal MUniversalGeneralisationDeductionStep(DeductionStepFileID ID, MDeduction D, MStatement premise) : this(D, premise)
        { fileID = ID; }
        internal MUniversalGeneralisationDeductionStep(FileIDManager IDM, MDeduction D, MStatement premise) : this(D, premise)
        { fileID = IDM.RequestDeductionStepFileID(this); }

        public MVariable Generalise(MQuantifier Q, MVariable vToQuantify, MVariable boundVariable)
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");

            if (Old != null) Old.AxiomAdded -= SecondaryPremise_ValidityChanged;
            Old = vToQuantify;
            Old.AxiomAdded += SecondaryPremise_ValidityChanged;

            New = boundVariable ?? _D._X.CreateVariable(vToQuantify.stringSymbol + "*");
            Quantifier = Q;
            return New;
        }

        public MVariable GetOld()
        {
            if (Old == null) Load();
            return Old;
        }
        public MVariable GetNew()
        {
            if (New == null) Load();
            return New;
        }
        public MQuantifier GetQuantifier()
        {
            if (Quantifier == null) Load();
            return Quantifier;
        }

        public override string ToString()
        {
            string ret = "Deduction by Universal Generalisation.\n";
            ret = ret + "Premise: " + Premise.ToString();
            ret = ret + "\n" + (Reduce ? "Reduced " : "") + "Consequence: " + Consequence.ToString() + "\n";
            return ret;
        }

        protected override Validity ValidateChildren(out MFormula ProcessedArgument)
        {
            if (Old == null || New == null) { ProcessedArgument = Premise.GetFormula(); return Validity.Invalid; }

            if (Old.HasDependencies(Premise.valid))
            {
                ProcessedArgument = Premise.GetFormula();
                return Validity.Invalid;
            }

            if (Quantifier == null || Quantifier.type != MQuantifier.QuantifierType.Universal || Quantifier.boundVarCount != 1) { ProcessedArgument = Premise.GetFormula(); return Validity.Invalid; }

            ProcessedArgument = new MQuantifierFormula(Quantifier, new MTerm[0], new MVariable[] { New }, new MFormula[1] { Premise._F.ReplaceVariable(Old, New) as MFormula });

            return Validity.Valid;
        }
    }

    public partial class MUniversalInstantiationDeductionStep : MDeductionStep
    {
        public MTerm T;
        public MQuantifier Quantifier;

        private MUniversalInstantiationDeductionStep(MDeduction D, MStatement premise) : base(D, premise)
        {
        }

        internal MUniversalInstantiationDeductionStep(DeductionStepFileID ID, MDeduction D, MStatement premise) : this(D, premise)
        { fileID = ID; }
        internal MUniversalInstantiationDeductionStep(FileIDManager IDM, MDeduction D, MStatement premise) : this(D, premise)
        { fileID = IDM.RequestDeductionStepFileID(this); }

        public bool Instantiate(MTerm tToInstantiate)
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");
            T = tToInstantiate;
            if (Premise._F is MQuantifierFormula castF)
            {
                Quantifier = castF._D as MQuantifier;
                return true;
            }
            return false;
        }

        public MTerm GetTerm()
        {
            if (T == null) Load();
            return T;
        }
        public MQuantifier GetQuantifier()
        {
            if (Quantifier == null) Load();
            return Quantifier;
        }

        protected override Validity ValidateChildren(out MFormula ProcessedArgument)
        {
            if (T == null) { ProcessedArgument = Premise.GetFormula(); return Validity.Invalid; }

            if (Quantifier == null || Quantifier.type != MQuantifier.QuantifierType.Universal) { ProcessedArgument = Premise.GetFormula(); return Validity.Invalid; }

            if (Premise._F is MQuantifierFormula castF)
            {
                ProcessedArgument = castF._F[0].ReplaceVariable(castF._BV[0], T) as MFormula;
            }
            else { ProcessedArgument = Premise.GetFormula(); return Validity.Invalid; }

            return Validity.Valid;
        }

        public override string ToString()
        {
            string ret = "Deduction by Universal Instantiation.\n";
            ret = ret + "Premise: " + Premise.ToString();
            ret = ret + "\n" + (Reduce ? "Reduced " : "") + "Consequence: " + Consequence.ToString() + "\n";
            return ret;
        }
    }
    
    public partial class MExistentialGeneralisationDeductionStep : MDeductionStep
    {
        public MTerm Old;
        public MVariable New;
        public MQuantifier Quantifier;

        private MExistentialGeneralisationDeductionStep(MDeduction D, MStatement premise) : base(D, premise)
        {
        }

        internal MExistentialGeneralisationDeductionStep(DeductionStepFileID ID, MDeduction D, MStatement premise) : this(D, premise)
        { fileID = ID; }
        internal MExistentialGeneralisationDeductionStep(FileIDManager IDM, MDeduction D, MStatement premise) : this(D, premise)
        { fileID = IDM.RequestDeductionStepFileID(this); }

        public MVariable Generalise(MQuantifier Q, MTerm tToQuantify, MVariable boundVariable = null)
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");
            Old = tToQuantify;
            New = boundVariable ?? _D._X.CreateVariable(tToQuantify.stringSymbol + "*");
            Quantifier = Q;
            return New;
        }

        public MTerm GetOld()
        {
            if (Old == null) Load();
            return Old;
        }
        public MVariable GetNew()
        {
            if (New == null) Load();
            return New;
        }
        public MQuantifier GetQuantifier()
        {
            if (Quantifier == null) Load();
            return Quantifier;
        }

        protected override Validity ValidateChildren(out MFormula ProcessedArgument)
        {
            if (Old == null || New == null) { ProcessedArgument = Premise.GetFormula(); return Validity.Invalid; }

            if (Quantifier == null || Quantifier.type != MQuantifier.QuantifierType.Existential || Quantifier.boundVarCount != 1) { ProcessedArgument = Premise.GetFormula(); return Validity.Invalid; }

            ProcessedArgument = new MQuantifierFormula(Quantifier, new MTerm[0], new MVariable[] { New }, new MFormula[] { Premise._F.ReplaceTerm(Old, New) as MFormula });
            
            return Validity.Valid;
        }

        public override string ToString()
        {
            string ret = "Deduction by Existential Generalisation.\n";
            ret = ret + "Premise: " + Premise.ToString();
            ret = ret + "\n" + (Reduce ? "Reduced " : "") + "Consequence: " + Consequence.ToString() + "\n";
            return ret;
        }
    }

    public partial class MExistentialInstantiationDeductionStep : MDeductionStep
    {
        public MVariable V;
        public MQuantifier Quantifier;

        private MExistentialInstantiationDeductionStep(MDeduction D, MStatement premise) : base(D, premise)
        {
        }

        internal MExistentialInstantiationDeductionStep(DeductionStepFileID ID, MDeduction D, MStatement premise) : this(D, premise)
        { fileID = ID; }
        internal MExistentialInstantiationDeductionStep(FileIDManager IDM, MDeduction D, MStatement premise) : this(D, premise)
        { fileID = IDM.RequestDeductionStepFileID(this); }

        public bool Instantiate(MVariable vToInstantiate)
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");
            if (V == null || V._X != _D._X) return false;

            V.AxiomAdded -= SecondaryPremise_ValidityChanged;
            V = vToInstantiate;
            V.AxiomAdded += SecondaryPremise_ValidityChanged;

            if (Premise._F is MQuantifierFormula castF)
            {
                Quantifier = castF._D as MQuantifier;
                return true;
            }
            return false;
        }

        public MVariable GetVariable()
        {
            if (V == null) Load();
            return V;
        }
        public MQuantifier GetQuantifier()
        {
            if (Quantifier == null) Load();
            return Quantifier;
        }
        

        protected override Validity ValidateChildren(out MFormula ProcessedArgument)
        {
            if (V == null) { ProcessedArgument = Premise.GetFormula(); return Validity.Invalid; }

            if (Consequence != null)
                Consequence._F.MakeAxiom(Consequence, true, null, false);
            if (V.HasDependencies(Premise.valid) 
                || Premise.ContainsUnrestrictedVariable(otherthan:V) 
                || Premise._F.ContainsUndifinedPredicate())
            {
                ProcessedArgument = Premise.GetFormula();
                return Validity.Invalid;
            }

            if (Quantifier == null || Quantifier.type != MQuantifier.QuantifierType.Existential) { ProcessedArgument = Premise.GetFormula(); return Validity.Invalid; }

            if (Premise._F is MQuantifierFormula castF)
            {
                ProcessedArgument = castF._F[0].ReplaceVariable(castF._BV[0], V) as MFormula;
            }
            else { ProcessedArgument = Premise.GetFormula(); return Validity.Invalid; }

            return Validity.Valid;
        }

        public override string ToString()
        {
            string ret = "Deduction by Existential Instantiation.\n";
            ret = ret + "Premise: " + Premise.ToString();
            ret = ret + "\n" + (Reduce ? "Reduced " : "") + "Consequence: " + Consequence.ToString() + "\n";
            return ret;
        }
    }

    public partial class MFormulaSubstitutionDeductionStep : MDeductionStep
    {
        internal MFormula Old;
        internal MFormula New;
        internal MStatement Justification;

        private MFormulaSubstitutionDeductionStep(MDeduction D, MStatement premise) : base(D, premise)
        {

        }

        internal MFormulaSubstitutionDeductionStep(DeductionStepFileID ID, MDeduction D, MStatement premise) : this(D, premise)
        { fileID = ID; }
        internal MFormulaSubstitutionDeductionStep(FileIDManager IDM, MDeduction D, MStatement premise) : this(D, premise)
        { fileID = IDM.RequestDeductionStepFileID(this); }

        public void Substitute(MFormula o, MFormula n, MStatement Just, bool invalidate = true)
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");
            Old = o;
            New = n;
            if (Justification != null)
            {
                Justification.ValidityChanged -= SecondaryPremise_ValidityChanged;
                Justification.ExpressionChanged -= SecondaryPremise_ValidityChanged;
            }
            Justification = Just;
            _D._X.AddSuperContext(Just._X);
            Justification.ValidityChanged += SecondaryPremise_ValidityChanged;
            Justification.ExpressionChanged += SecondaryPremise_ValidityChanged;
            if (invalidate)Invalidate();
        }

        public MFormula GetOld()
        {
            if (Old == null) Load();
            return Old;
        }
        public MFormula GetNew()
        {
            if (New == null) Load();
            return New;
        }
        public MStatement GetJustification()
        {
            if (Justification == null) Load();
            return Justification;
        }

        protected override Validity ValidateChildren(out MFormula ProcessedArgument)
        {
            // Check if Substitutions are Justified
            if (Old == null || New == null || Justification == null) { ProcessedArgument = Premise.GetFormula(); return Validity.Invalid; }
            if (!(Justification._F is MBinaryConnectiveFormula J)) { ProcessedArgument = Premise.GetFormula(); return Validity.Invalid; }
            else if (!_D._X.ContainsSuperContext(Justification._X)) { ProcessedArgument = Premise.GetFormula(); return Validity.Invalid; }
            else if (!((J._D as MBinaryConnective).IsEquivalence)) { ProcessedArgument = Premise.GetFormula(); return Validity.Invalid; }
            else if (!((J._F[0].Identical(New) && J._F[1].Identical(Old)) ||
                (J._F[1].Identical(New) && J._F[0].Identical(Old)))) { ProcessedArgument = Premise.GetFormula(); return Validity.Invalid; }

            ProcessedArgument = Premise._F.ReplaceFormula(Old, New) as MFormula;
            
            return Justification.valid.Copy();
        }

        internal override void SecondaryPremise_ValidityChanged(object sender, EventArgs e)
        {
            MStatement newJustification = Justification.fileID.FindStatement();
            if (Justification == newJustification || Justification.Identical(newJustification))
            {
                // Change is just in validity
                Justification.ValidityChanged -= SecondaryPremise_ValidityChanged;
                newJustification.ValidityChanged += SecondaryPremise_ValidityChanged;
                Justification = newJustification;
                UpdateValidity();
            }
            else
            {
                Justification.ValidityChanged -= SecondaryPremise_ValidityChanged;
                newJustification.ValidityChanged += SecondaryPremise_ValidityChanged;
                Justification = newJustification;
                Validate();
            }
        }

        public override string ToString()
        {
            string ret = "Deduction by Formula Substitution.\n";
            ret = ret + "Premise: " + Premise.ToString() +
                "\nSubstitution:\n" + Helper.Indent(Old.ToString() + " << " + New.ToString()) +
                "\nJustification:\n" + Helper.Indent(Justification.ToString());
            ret = ret + "\n" + (Reduce ? "Reduced " : "") + "Consequence: " + Consequence.ToString() + "\n";
            return ret;
        }
    }

    public partial class MTermSubstitutionDeductionStep : MDeductionStep
    {
        internal MTerm Old;
        internal MTerm New;
        internal MStatement Justification;

        private MTermSubstitutionDeductionStep(MDeduction D, MStatement premise) : base(D, premise)
        {

        }

        internal MTermSubstitutionDeductionStep(DeductionStepFileID ID, MDeduction D, MStatement premise) : this(D, premise)
        { fileID = ID; }
        internal MTermSubstitutionDeductionStep(FileIDManager IDM, MDeduction D, MStatement premise) : this(D, premise)
        { fileID = IDM.RequestDeductionStepFileID(this); }

        public void Substitute(MTerm o, MTerm n, MStatement Just, bool invalidate = true)
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");
            Old = o;
            New = n;
            if (Justification != null)
            {
                Justification.ValidityChanged -= SecondaryPremise_ValidityChanged;
                Justification.ExpressionChanged -= SecondaryPremise_ValidityChanged;
            }
            Justification = Just;
            _D._X.AddSuperContext(Just._X);
            Justification.ValidityChanged += SecondaryPremise_ValidityChanged;
            Justification.ExpressionChanged += SecondaryPremise_ValidityChanged;
            if (invalidate) Invalidate();
        }

        public MTerm GetOld()
        {
            if (Old == null) Load();
            return Old;
        }
        public MTerm GetNew()
        {
            if (New == null) Load();
            return New;
        }
        public MStatement GetJustification()
        {
            if (Justification == null) Load();
            return Justification;
        }

        protected override Validity ValidateChildren(out MFormula ProcessedArgument)
        {
            // Check if Substitutions are Justified
            if (Old == null || New == null || Justification == null) { ProcessedArgument = Premise.GetFormula(); return Validity.Invalid; }
            if (!(Justification._F is MEqualityFormula J)) { ProcessedArgument = Premise.GetFormula(); return Validity.Invalid; }
            else if (!_D._X.ContainsSuperContext(Justification._X)) { ProcessedArgument = Premise.GetFormula(); return Validity.Invalid; }
            else if (!((J._T[0].Identical(New) && J._T[1].Identical(Old)) ||
                (J._T[1].Identical(New) && J._T[0].Identical(Old)))) { ProcessedArgument = Premise.GetFormula(); return Validity.Invalid; }

            ProcessedArgument = Premise._F.ReplaceTerm(Old, New) as MFormula;

            return Justification.valid.Copy();
        }

        internal override void SecondaryPremise_ValidityChanged(object sender, EventArgs e)
        {
            MStatement newJustification = Justification.fileID.FindStatement();
            if (Justification == newJustification || Justification.Identical(newJustification))
            {
                // Change is just in validity
                Justification.ValidityChanged -= SecondaryPremise_ValidityChanged;
                newJustification.ValidityChanged += SecondaryPremise_ValidityChanged;
                Justification = newJustification;
                UpdateValidity();
            }
            else
            {
                Justification.ValidityChanged -= SecondaryPremise_ValidityChanged;
                newJustification.ValidityChanged += SecondaryPremise_ValidityChanged;
                Justification = newJustification;
                Validate();
            }
        }

        public override string ToString()
        {
            string ret = "Deduction by Term Substitution.\n";
            ret = ret + "Premise: " + Premise.ToString() +
                "\nSubstitution:\n" + Helper.Indent(Old.ToString() + " << " + New.ToString()) +
                "\nJustification:\n" + Helper.Indent(Justification.ToString());
            ret = ret + "\n" + (Reduce ? "Reduced " : "") + "Consequence: " + Consequence.ToString() + "\n";
            return ret;
        }
    }

    public partial class MRAADeductionStep : MDeductionStep
    {
        internal MStatement Condition; // TODO: What if the condition is changed?

        private MRAADeductionStep(MDeduction D, MStatement premise) : base(D, premise)
        {
            IgnorePremiseValidity = true;
        }

        internal MRAADeductionStep(DeductionStepFileID ID, MDeduction D, MStatement premise) : this(D, premise)
        { fileID = ID; }
        internal MRAADeductionStep(FileIDManager IDM, MDeduction D, MStatement premise) : this(D, premise)
        { fileID = IDM.RequestDeductionStepFileID(this); }

        public void RAA(MStatement Cond, bool invalidate = true)
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");

            if (Condition != null)
            {
                Condition.ValidityChanged -= SecondaryPremise_ValidityChanged;
                Condition.ExpressionChanged -= SecondaryPremise_ValidityChanged;
            }
            Condition = Cond;
            _D._X.AddSuperContext(Cond._X);
            Condition.ValidityChanged += SecondaryPremise_ValidityChanged;
            Condition.ExpressionChanged += SecondaryPremise_ValidityChanged;
            if (invalidate) Invalidate();
        }

        public MStatement GetCondition()
        {
            if (Condition == null) Load();
            return Condition;
        }

        protected override Validity ValidateChildren(out MFormula ProcessedArgument)
        {
            // Check if Substitutions are Justified
            if (Condition == null) { ProcessedArgument = Premise.GetFormula(); return Validity.Invalid; }
            if (!(Premise._F.Identical(MTrivialFormula._False))) { ProcessedArgument = Premise.GetFormula(); return Validity.Invalid; }
            else if (!Condition.valid.IsValid) { ProcessedArgument = Premise.GetFormula(); return Validity.Invalid; }
            else if (!_D._X.ContainsSuperContext(Condition._X)) { ProcessedArgument = Premise.GetFormula(); return Validity.Invalid; }

            ProcessedArgument = new MNegationFormula(Condition._F.Copy() as  MFormula);

            Validity V = Premise.valid.Copy();
            V.RemoveDependence(Condition);
            return V;
        }

        public override string ToString()
        {
            string ret = "Reductio ad Absurdum.\n";
            ret = ret + "Premise: " + Premise.ToString() +
                "\nCondition:\n" + Helper.Indent(Condition.ToString());
            ret = ret + "\n" + (Reduce ? "Reduced " : "") + "Consequence: " + Consequence.ToString() + "\n";
            return ret;
        }
    }

    public partial class MAssumptionDeductionStep : MDeductionStep
    {
        internal MFormula Assumption;
        internal List<int> RestrictedVars;

        private MAssumptionDeductionStep(MDeduction D, MStatement premise) : base(D, premise)
        {

        }

        internal MAssumptionDeductionStep(DeductionStepFileID ID, MDeduction D, MStatement premise) : this(D, premise)
        { fileID = ID; }
        internal MAssumptionDeductionStep(FileIDManager IDM, MDeduction D, MStatement premise) : this(D, premise)
        { fileID = IDM.RequestDeductionStepFileID(this); }

        public void Assume(MFormula Ass, List<int> restrict, bool invalidate = true)
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");
            if (Consequence != null)
                Consequence._F.MakeAxiom(Consequence, true, null, false);
            if (Ass != null)
            {
                //Ass.ExpressionChanged -= SecondaryPremise_ValidityChanged;
            }
            Assumption = Ass;
            RestrictedVars = restrict;
            //Assumption.ExpressionChanged += SecondaryPremise_ValidityChanged;
            if (invalidate) Invalidate();
        }

        public MFormula GetAssumption()
        {
            if (Assumption == null) Load();
            return Assumption;
        }
        public List<int> GetRestrictedVars()
        {
            if (RestrictedVars == null) Load();
            return RestrictedVars;
        }

        protected override Validity ValidateChildren(out MFormula ProcessedArgument)
        {
            if (Assumption == null) { ProcessedArgument = Premise.GetFormula(); return Validity.Invalid; }

            ProcessedArgument = Assumption;

            Validity V = Validity.Valid;
            return V;
        }

        public override string ToString() 
        {
            string ret = "Assumption.\n" + (Reduce ? "Reduced " : "") + "Consequence: " + Consequence.ToString() + "\n";
            return ret;
        }
    }

    public partial class MConditionInstantiationDeductionStep : MDeductionStep
    {
        internal MStatement Condition; //TODO: what if the condition is changed?
        internal MBinaryConnective Implication;
        internal bool ImplicationNull = true;

        private MConditionInstantiationDeductionStep(MDeduction D, MStatement premise) : base(D, premise)
        {
            IgnorePremiseValidity = true;
        }

        internal MConditionInstantiationDeductionStep(DeductionStepFileID ID, MDeduction D, MStatement premise) : this(D, premise)
        { fileID = ID; }
        internal MConditionInstantiationDeductionStep(FileIDManager IDM, MDeduction D, MStatement premise) : this(D, premise)
        { fileID = IDM.RequestDeductionStepFileID(this); }

        public void Instantiate(MStatement Cond, MFormula Imp = null, bool invalidate = true)
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");

            if (Condition != null)
            {
                Condition.ValidityChanged -= SecondaryPremise_ValidityChanged;
                Condition.ExpressionChanged -= SecondaryPremise_ValidityChanged;
            }
            Condition = Cond;
            _D._X.AddSuperContext(Cond._X);
            Condition.ValidityChanged += SecondaryPremise_ValidityChanged;
            Condition.ExpressionChanged += SecondaryPremise_ValidityChanged;
            if (Imp != null && Imp is MBinaryConnectiveFormula BCimp)
            {
                Implication = Imp._D as MBinaryConnective;
                ImplicationNull = false;
            }
            if (invalidate) Invalidate();
        }

        public MStatement GetCondition()
        {
            if (Condition == null) Load();
            return Condition;
        }

        public MFormula GetImplication()
        {
            if (ImplicationNull) return MFormula.PlaceholderFormula;
            if (Implication == null) Load();
            return Implication.GetPlaceholderVisualisation(null).VisualisedObject as MFormula;
        }

        protected override Validity ValidateChildren(out MFormula ProcessedArgument)
        {
            if (Condition == null) { ProcessedArgument = Premise.GetFormula(); return Validity.Invalid; }
            if (Implication == null || !(Implication.IsImplication)) { ProcessedArgument = Premise.GetFormula(); return Validity.Invalid; }
            else if (!Premise.valid.IsValid) { ProcessedArgument = Premise.GetFormula(); return Validity.Invalid; }
            else if (!Condition.valid.IsValid) { ProcessedArgument = Premise.GetFormula(); return Validity.Invalid; }
            else if (!_D._X.ContainsSuperContext(Condition._X)) { ProcessedArgument = Premise.GetFormula(); return Validity.Invalid; }

            ProcessedArgument = new MBinaryConnectiveFormula(Implication, Condition._F.Copy() as MFormula, Premise._F.Copy() as MFormula);

            Validity V = Premise.valid.Copy();
            V.RemoveDependence(Condition);
            return V;
        }

        public override string ToString()
        {
            string ret = "Condition Instantiation.\n";
            ret = ret + "Premise: " + Premise.ToString() +
                "\nCondition:\n" + Helper.Indent(Condition.ToString());
            ret = ret + "\n" + (Reduce ? "Reduced " : "") + "Consequence: " + Consequence.ToString() + "\n";
            return ret;
        }
    }
    #endregion

    #region SubstitutionClasses
    public partial class MVariableSubstitutions
    {
        public MVariableSubstitutionDeductionStep DS;
        internal IdTable<MVariable, MTerm> Substitutions;
        internal List<MSubstitutionJustification> Justifications;

        public int Count => Substitutions.Count;

        public MSubstitutionJustification GetSubstitution(int index)
        {
            return Justifications[index]; //TODO: check if loaded? not suuuure
        }

        internal MVariableSubstitutions(MVariableSubstitutionDeductionStep Step)
        {
            DS = Step;
            Substitutions = new IdTable<MVariable, MTerm>(IdRuleset.LeftUnique);
            Justifications = new List<MSubstitutionJustification>();
        }

        internal MAxiomaticSubstitutionJustification SubstituteAxiomatically(MVariable V, MTerm T)
        {
            Substitutions.Identify(V, T);
            MAxiomaticSubstitutionJustification J = new MAxiomaticSubstitutionJustification(this, V, T);
            Justifications.Add(J);
            return J;
        }

        internal MEqualitySubstitutionJustification SubstituteEqualitically(MVariable V, MTerm T)
        {
            Substitutions.Identify(V, T);
            MEqualitySubstitutionJustification J = new MEqualitySubstitutionJustification(this, V, T);
            Justifications.Add(J);
            return J;
        }

        internal void Substitute(MVariable V, MTerm T, MSubstitutionJustification J)
        {
            Substitutions.Identify(V, T);
            Justifications.Add(J);
        }

        public Validity AreValid()
        {
            Validity V = Validity.Valid;
            foreach (MSubstitutionJustification J in Justifications)
                V = V & J.IsValid();
            return V;
        }
    }

    public partial class MSubstitutionJustification
    {
        internal MVariableSubstitutions Substitutions;
        public MVariable Old;
        public MTerm New;

        internal MSubstitutionJustification(MVariableSubstitutions S, MVariable O, MTerm N)
        {
            Substitutions = S;
            Old = O;
            New = N;
        }

        internal virtual Validity IsValid() { return Validity.Invalid; }
    }

    public partial class MAxiomaticSubstitutionJustification : MSubstitutionJustification
    {
        internal MAxiomaticSubstitutionJustification(MVariableSubstitutions S, MVariable O, MTerm N) : base(S, O, N)
        {
            Old.AxiomAdded += Old_AxiomChanged;
            Old.AxiomChanged += Old_AxiomChanged;
            Old.AxiomRemoved += Old_AxiomChanged;
        }

        private void Old_AxiomChanged(object sender, AxiomChangedEventArgs e) // TODO: Check if Axiomchanged gets called for assumptions
        {
            Substitutions.DS.Invalidate();
        }
        
        internal override Validity IsValid()
        {
            return Old.HasDependencies(Substitutions.DS.Premise.valid) ? Validity.Invalid : Validity.Valid;
        }
    }

    public partial class MEqualitySubstitutionJustification : MSubstitutionJustification
    {
        public MStatement Equality;

        public MEqualitySubstitutionJustification(MVariableSubstitutions S, MVariable O, MTerm N) : base(S, O, N) { }

        public bool Justify(MStatement E)
        {
            if (!E.valid.IsValid) return false;
            if (!Substitutions.DS._D._X.AddSuperContext(E._X)) return false;

            if (E._F is MEqualityFormula)
                if ((E._F._T[0].Identical(Old) && E._F._T[1].Identical(New)) ||
                    (E._F._T[1].Identical(Old) && E._F._T[0].Identical(New)))
                {
                    E.ValidityChanged += Substitutions.DS.SecondaryPremise_ValidityChanged;
                    E.ExpressionChanged += Substitutions.DS.SecondaryPremise_ValidityChanged;
                    if (Equality != null)
                    {
                        Equality.ValidityChanged -= Substitutions.DS.SecondaryPremise_ValidityChanged;
                        Equality.ExpressionChanged -= Substitutions.DS.SecondaryPremise_ValidityChanged;
                    }
                    Equality = E;
                    return true;
                }
            return false;
        }

        internal override Validity IsValid()
        {
            if (Equality == null) return Validity.Invalid;
            if (!Substitutions.DS._D._X.ContainsSuperContext(Equality._X)) return Validity.Invalid;

            if (Equality._F is MEqualityFormula)
                if ((Equality._F._T[0].Identical(Old) && Equality._F._T[1].Identical(New)) ||
                    (Equality._F._T[1].Identical(Old) && Equality._F._T[0].Identical(New)))
                    return Equality.valid.Copy();

            return Validity.Invalid;
        }
    }
#endregion

   
}