using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using TraceBackend.AI;

namespace TraceBackend
{
    public partial class MTerm : MExpression
    {
        public static MPlaceholderTerm PlaceholderTerm => new MPlaceholderTerm();

        public virtual void MakeVariableList(List<MVariable> list)
        {

        }
    }

    public partial class MPlaceholderTerm : MTerm
    {
        internal MVisualisationScheme visualisation;

        internal MPlaceholderTerm(string symbol = "_") : base()
        {
            stringSymbol = symbol;
            visualisation = new MVisualisationScheme(null);
            visualisation.AddSymbol(new MTextSymbol(stringSymbol, FontCategory.FormulaUpright));

            _F = new MFormula[0];
            _T = new MTerm[0];
            _D = null;
        }

        public void SetText(string text)
        {
            stringSymbol = text;
            (visualisation.Symbols[0] as MTextSymbol).Text = text;
        }

        public override MVisualisation GetVisualisation(MVisualisationScheme VS = null)
        {
            MVisualisation V = new MVisualisation(this, visualisation);
            Visualization = V;
            return V;
        }

        public override MObject Copy()
        {
            return new MPlaceholderTerm(stringSymbol);
        }
    }

    public partial class MVariable : MTerm
    {
        public VariableFileID fileID;
        internal MVisualisationScheme visualisation;
        public static MVariable PlaceholderVariable;
        internal List<MStatement> Axioms;
        internal List<MStatement> Assumptions;
        public MContext _X;

        public bool HasAxioms => Axioms != null ? Axioms.Count != 0 : false;

        public event EventHandler<AxiomChangedEventArgs> AxiomAdded;
        public event EventHandler<AxiomChangedEventArgs> AxiomChanged;
        public event EventHandler<AxiomChangedEventArgs> AxiomRemoved;

        private NeuralComplex _mind;
        public string MindPath => Properties.Settings.Default.AIDirectory + "\\" + _X.Document.Name + fileID.ToXML(_X.Document) + ".mind";
        public NeuralComplex Mind => _mind == null ? DefinitionReader.FromFile(MindPath) : _mind;
        public void SaveMind()
        {
            (Mind.Operation(DefinitionReader.lmind) as NeuralNetwork).ToFile();
        }

        public override MObject Copy()
        {
            return this;
        }

        internal void AddAxiom(MStatement A, bool events)
        {
            if (A._X != _X) throw new Exception();

            List<MStatement> list;
            if (A.valid.IsAssumption) list = Assumptions;
            else if (A.valid.IsAxiom) list = Axioms;
            else return;

            if (list.Contains(A)) return;

            list.Add(A);
            if (events) AxiomAdded?.Invoke(this, new AxiomChangedEventArgs(A, list.Count - 1));
            A.ValidityChanged += Axiom_Changed;
            A.ExpressionChanged += Axiom_Changed;
        }

        internal void RemoveAxiom(MStatement A, bool events)
        {
            List<MStatement> list;
            if (A.valid.IsAssumption) list = Assumptions;
            else if (A.valid.IsAxiom) list = Axioms;
            else return;

            if (!list.Contains(A)) return;

            int index = list.IndexOf(A);
            list.Remove(A);
            if(events) AxiomRemoved?.Invoke(this, new AxiomChangedEventArgs(A, index));
            A.ValidityChanged -= Axiom_Changed;
            A.ExpressionChanged -= Axiom_Changed;
        }

        void Axiom_Changed(object sender, EventArgs e)
        {
            if (Axioms.Contains(sender as MStatement))
                AxiomChanged?.Invoke(this, new AxiomChangedEventArgs(sender as MStatement, Axioms.IndexOf(sender as MStatement)));
        }

        static MVariable()
        {
            PlaceholderVariable = new MVariable("_");
        }

        internal override void MakeAxiom(MStatement Axiom, bool unmake, List<MVariable> restrictions, bool events)
        {
            if (Axiom._X != _X) return;

            if (unmake)
                RemoveAxiom(Axiom, events);
            else
            {
                if (restrictions.Contains(this))
                    AddAxiom(Axiom, events);
                else
                    RemoveAxiom(Axiom, events);
            }
        }

        protected MVariable(string symbol = " ") : base()
        {
            stringSymbol = symbol;
            visualisation = new MVisualisationScheme(null);
            visualisation.AddSymbol(new MTextSymbol(stringSymbol, FontCategory.FormulaCursive));
            Axioms = new List<MStatement>();
            Assumptions = new List<MStatement>();

            _F = new MFormula[0];
            _T = new MTerm[0];
            _BV = new MBoundVariable[0];
            _D = null;
        }

        public override bool Identical(MObject E, IdTable<MObject, MObject> idTable = null)
        {
            return E == this || E.ID == ID;
        }

        public MVariable(VariableFileID ID, MContext X, string symbol = " ") : this(symbol)
        {
            fileID = ID;
            _X = X;
        }

        public MVariable(FileIDManager IDM, MContext X, string symbol = " ") : this(symbol)
        {
            fileID = IDM.RequestVariableFileID(this, X);
            _X = X;
        }

        public override bool ContainsVariable(MVariable V)
        {
            return Identical(V); // TODO: shouldnt this be == ?
        }

        public override bool ContainsUnrestrictedVariable(Validity V, MVariable otherthan = null)
        {
            if (otherthan != null && this == otherthan) return true; 
            return !HasDependencies(V);
        }

        public override string ToString()
        {
            if (stringSymbol == " ")
                return "x_" + ID.ToString();
            return stringSymbol;
        }

        public bool HasDependencies(Validity v) 
        {
            if (Axioms.Count > 0) return true; // Axioms

            foreach (MStatement ass in Assumptions)
                if (v.IsDependent && v.Conditions.Contains(ass))        // Assumptions
                    return true;

            return false;
        }

        public virtual MBoundVariable CreateBoundInstance(MExpression BindingExpression)
        {
            return new MBoundVariable(this, BindingExpression);
        }

        public override MVisualisation GetVisualisation(MVisualisationScheme VS = null)
        {
            MVisualisation V = new MVisualisation(this, visualisation);
            Visualization = V;
            return V;
        }

        public override void MakeVariableList(List<MVariable> list)
        {
            if (!(list.Contains(this))) list.Add(this);
        }

        public override List<MVariable> MakeFreeVariableList()
        {
            return new List<MVariable> { this };
        }

        public override void ReferenceInDocument(MDocument D)
        {
            D.ReferenceDocument(fileID.Document);
        }
    }
    public class AxiomChangedEventArgs:EventArgs
    {
        public MStatement Axiom;
        public int index;

        public AxiomChangedEventArgs(MStatement A, int i)
        {
            Axiom = A;
            index = i;
        }
    }

    public partial class MBoundVariable : MVariable
    {
        public MVariable _FreeInstance;
        private MExpression BoundBy;

        public MBoundVariable(MVariable FreeInstance, MExpression BindingExpression) : base(FreeInstance.stringSymbol)
        {
            _FreeInstance = FreeInstance;
            fileID = FreeInstance.fileID;
            BoundBy = BindingExpression;
        }


        internal override void MakeAxiom(MStatement Axiom, bool unmake, List<MVariable> exceptions, bool events)
        {
            //Do nothing
        }

        public override bool Identical(MObject E, IdTable<MObject, MObject> idTable = null)
        {
            if(E is MBoundVariable V)
            {
                return V._FreeInstance.Identical(_FreeInstance) && V.BoundBy == BoundBy;
            }
            return false;
        }

        public override MBoundVariable CreateBoundInstance(MExpression BindingExpression)
        {
            return new MBoundVariable(_FreeInstance, BindingExpression);
        }

        public override void MakeVariableList(List<MVariable> list)
        {
            // do nothing
        }

        public override List<MVariable> MakeFreeVariableList()
        {
            return new List<MVariable> ();
        }
    }

    public partial class MFunction : MTerm
    {
        public MFunction(MFunctionSymbol function, MTerm[] args, MVariable[] bvars, MFormula[] formulas) : base()
        {
            _T = args;
            _F = formulas;
            _BV = new MBoundVariable[function.boundVarCount];
            for (int i = 0; i < function.boundVarCount; i++)
                _BV[i] = bvars[i].CreateBoundInstance(this);
            _D = function;

            UpdateBinding();
        }

        public override MObject Copy()
        {
            return new MFunction(_D as MFunctionSymbol, _T.Select(T => T.Copy() as MTerm).ToArray(), _BV, _F.Select(F => F.Copy() as MFormula).ToArray());
        }

        public override string ToString()
        {
            string ret = _D.stringSymbol;
            if ((_D as MFunctionSymbol).boundVarCount > 0)
            {
                ret = ret + " [ ";
                for (int i = 0; i < (_D as MFunctionSymbol).boundVarCount; i++)
                {
                    ret = ret + _BV[i].ToString();
                    if (i < (_D as MFunctionSymbol).boundVarCount - 1)
                        ret = ret + ", ";
                }
                ret = ret + " ] ";
            }
            if ((_D as MFunctionSymbol).argumentCount > 0)
            {
                ret = ret + " ( ";
                for (int i = 0; i < (_D as MFunctionSymbol).argumentCount; i++)
                {
                    ret = ret + _T[i].ToString();
                    if (i < (_D as MFunctionSymbol).argumentCount - 1)
                        ret = ret + ", ";
                }
                ret = ret + " ) ";
            }
            if ((_D as MFunctionSymbol).formulaCount > 0)
            {
                ret = ret + " { ";
                for (int i = 0; i < (_D as MFunctionSymbol).formulaCount; i++)
                {
                    ret = ret + _F[i].ToString();
                    if (i < (_D as MFunctionSymbol).formulaCount - 1)
                        ret = ret + ", ";
                }
                ret = ret + " }";
            }
            return ret;
        }

        public override void UpdateBinding()
        {
            foreach (MBoundVariable v in _BV)
                BindVariable(v);

            base.UpdateBinding();
        }

        public override void MakeVariableList(List<MVariable> list)
        {
            foreach (MTerm arg in _T)
                arg.MakeVariableList(list); //TODO: maybe account for bound variables, but maybe this method is obsolete
        }
    }
}