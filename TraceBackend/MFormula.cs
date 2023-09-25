using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;

namespace TraceBackend
{
    public partial class MFormula : MExpression
    {
        public static MPlaceholderFormula PlaceholderFormula => new MPlaceholderFormula();

        public MFormula() : base()
        {

        }

        //TODO: change this to GetFreeExpression
        internal MFormula GetFreeFormula(MBoundVariable V)
        {
            return ReplaceVariable(V, V._FreeInstance) as MFormula;
        }

        public virtual bool[] Evaluate(int c = -1)
        {
            return null;
        }

        public virtual MFormula Reduce(bool start = true)
        {
            MFormula F;
            if (start == true)
                F = (MFormula)Copy();
            else F = this;

            for (int i = 0; i < _F.Length; i++)
            {
                F._F[i] = F._F[i].Reduce(false);
            }

            Correctness c = GetCorrectness();
            switch (c)
            {
                case Correctness.Tautological:
                    return MTrivialFormula._True;
                case Correctness.Antitautological:
                    return MTrivialFormula._False;
                default:
                    return F;
            }
        }

        public Correctness GetCorrectness()
        {
            bool[] valueList = Evaluate();
            if (valueList == null || valueList.Length == 0) return Correctness.Empty;

            if (valueList[0] == true)
            {
                foreach (bool v in valueList)
                    if (v == false) return Correctness.Dependent;
                return Correctness.Tautological;
            }
            else
            {
                foreach (bool v in valueList)
                    if (v == true) return Correctness.Dependent;
                return Correctness.Antitautological;
            }
        }

        public bool IsTautology()
        {
            return GetCorrectness() == Correctness.Tautological;
        }

        public enum Correctness
        {
            Tautological = 0,
            Antitautological = 1,
            Dependent = 2,
            Empty = 3
        }
    }

    public partial class MPlaceholderFormula : MFormula
    {
        MVisualisationScheme visualisation;

        internal MPlaceholderFormula(string symbol = "_") : base()
        {
            _F = new MFormula[0];
            _T = new MTerm[0];
            _BV = new MBoundVariable[0];
            _D = null;

            stringSymbol = symbol;

            visualisation = new MVisualisationScheme(null);
            visualisation.AddSymbol(new MTextSymbol(stringSymbol, FontCategory.FormulaUpright));
        }

        public void SetText(string text)
        {
            stringSymbol = text;
            (visualisation.Symbols[0] as MTextSymbol).Text = text;
        }

        public override MObject Copy()
        {
            MFormula F = new MPlaceholderFormula(stringSymbol);
            return F;
        }

        public override MVisualisation GetVisualisation(MVisualisationScheme VS = null)
        {
            MVisualisation V = new MVisualisation(this, visualisation);
            Visualization = V;
            return V;
        }

        public override int CountUndefinedFormulas(List<MFormula> idlist = null)
        {
            bool start = false;
            if (idlist == null) start = true;
            if (start)
            {
                localID = 0;
            }
            else
                idlist.Add(this);
            return 1;
        }

        public override bool[] Evaluate(int c = -1)
        {
            if (c == -1)
            {
                c = CountUndefinedFormulas();
            }

            bool[] ret = new bool[Helper.Pow(2, c)];
            int step = Helper.Pow(2, localID + 1);
            for (int i = 0; i < ret.Length; i += step)
            {
                for (int j = i; j < i + step / 2; j++)
                    ret[j] = true;
                for (int j = i + step / 2; j < i + step; j++)
                    ret[j] = false;
            }

            return ret;
        }
     }

    public partial class MUndefinedPredicate : MObject
    {
        internal MVisualisationScheme visualisation;
        public int argumentCount;

        public MUndefinedPredicate(int argCount, string symbol = "P") : base()
        {
            argumentCount = argCount;
            visualisation = new MVisualisationScheme(null, argCount);
            stringSymbol = symbol;
            int lind = visualisation.AddSymbol(new MTextSymbol(stringSymbol, FontCategory.FormulaCursive));
            if(argCount != 0)
            {
                lind = visualisation.AddSymbol(new MTextSymbol("(", FontCategory.FormulaUpright), lind);
                for(int i = 0; i< argCount; i++)
                {
                    visualisation.Arrange(i, lind);
                    lind = i;
                    if (i != argCount - 1)
                        lind = visualisation.AddSymbol(new MTextSymbol(", ", FontCategory.FormulaUpright), lind);
                }
                visualisation.AddSymbol(new MTextSymbol(")", FontCategory.FormulaUpright), lind);
            }
        }

        public MVisualisation GetPlaceholderVisualisation()
        {
            MVisualisation V = new MVisualisation(this, visualisation);
            return V;
        }
    }

    public partial class MUndefinedPredicateFormula : MFormula
    {
        public MUndefinedPredicate PseudoDefinition;

        public MUndefinedPredicateFormula(MUndefinedPredicate type, MTerm[] T) : base()
        {
            _F = new MFormula[0];
            _T = T;
            _BV = new MBoundVariable[0];
            _D = null;
            PseudoDefinition = type;
            stringSymbol = PseudoDefinition.stringSymbol;
            if (_T.Length != PseudoDefinition.argumentCount)
                throw new ArgumentException("The number of Terms must be equal to the number of arguments of the PseudoDefinition.");
        }

        public override MObject Copy()
        {
            MFormula F = new MUndefinedPredicateFormula(PseudoDefinition, _T.Select(T => T.Copy() as MTerm).ToArray());
            return F;
        }

        public override int CountUndefinedFormulas(List<MFormula> idlist = null)
        {
            bool start = false;
            if (idlist == null) start = true;
            if (start)
            {
                localID = 0;
            }
            else
                idlist.Add(this);
            return 1;
        }

        public override bool Identical(MObject F, IdTable<MObject, MObject> idTable = null)
        {
            if (!(F is MUndefinedPredicateFormula castF)) return false;
            else if (castF.PseudoDefinition != PseudoDefinition) return false;

            return base.Identical(F, idTable);
        }

        public override bool[] Evaluate(int c = -1)
        {
            if (c == -1)
            {
                c = CountUndefinedFormulas();
            }

            bool[] ret = new bool[Helper.Pow(2, c)];
            int step = Helper.Pow(2, localID + 1);
            for (int i = 0; i < ret.Length; i += step)
            {
                for (int j = i; j < i + step / 2; j++)
                    ret[j] = true;
                for (int j = i + step / 2; j < i + step; j++)
                    ret[j] = false;
            }

            return ret;
        }

        public override string ToString()
        {
            string s;
            if (stringSymbol == " ")
                s= "P_" + ID.ToString();
            else s= stringSymbol;

            if (_T.Length > 0)
            {
                s = s + " ( ";
                for (int i = 0; i < _T.Length; i++)
                    s = s + _T[i].ToString() + (i == _T.Length - 1 ? " ) " : ", ");
            }
            return s;
        }

        public override void MakeUndefinedFormulaList(List<MUndefinedPredicate> pdlist)
        {
            if (!pdlist.Contains(PseudoDefinition)) pdlist.Add(PseudoDefinition);
        }

        public override MVisualisation GetVisualisation(MVisualisationScheme VS = null)
        {
            return base.GetVisualisation(PseudoDefinition.visualisation);
        }

        public override bool ContainsUndifinedPredicate()
        {
            return true;
        }

        internal MFormula Define(MQuantifier Predicate) //TODO: check that Predicate fits
        {
            return new MQuantifierFormula(Predicate, _T.Select(T => T.Copy() as MTerm).ToArray(), new MVariable[0], new MFormula[0]);
        }

        internal override MFormula FindOccurenceOfPredicate(MUndefinedPredicate P)
        {
            if (PseudoDefinition.Identical(P)) return this;
            return null;
        }
    }

    public partial class MTrivialFormula : MFormula
    {
        public bool _V;

        public static MTrivialFormula _True;
        static MVisualisationScheme VisTrue;
        public static MTrivialFormula _False;
        static MVisualisationScheme VisFalse;

        static MTrivialFormula()
        {
            _True = new MTrivialFormula(true);
            _False = new MTrivialFormula(false);

            VisTrue = new MVisualisationScheme(null);
            VisTrue.AddSymbol(new MTextSymbol("True", FontCategory.FormulaUpright));
            VisTrue.Latex = @"\mathrm{True}";
            VisFalse = new MVisualisationScheme(null);
            VisFalse.AddSymbol(new MTextSymbol("False", FontCategory.FormulaUpright));
            VisFalse.Latex = @"\mathrm{False}";
        }

        private MTrivialFormula(bool V) : base()
        {
            _F = new MFormula[0];
            _T = new MTerm[0];
            _BV = new MBoundVariable[0];
            _D = null;

            _V = V;
        }

        public override MObject Copy()
        {
            return this;
        }

        public override bool Identical(MObject F, IdTable<MObject, MObject> idTable = null)
        {
            if (!(F is MTrivialFormula castF)) return false;
            return castF._V == _V;
        }

        public override bool[] Evaluate(int c = -1)
        {
            if (c == -1)
            {
                c = CountUndefinedFormulas();
            }

            bool[] ret = new bool[Helper.Pow(2, c)];
            for (int i = 0; i < ret.Length; i++)
                ret[i] = _V;

            return ret;
        }

        public override string ToString()
        {
            return _V.ToString();
        }

        public override MVisualisation GetVisualisation(MVisualisationScheme VS = null)
        {
            MVisualisation V = new MVisualisation(this, _V == true ? VisTrue : VisFalse);
            Visualization = V;
            return V;
        }
    }

    public partial class MEqualityFormula : MFormula
    {
        public MEqualityFormula(MTerm A, MTerm B) : base()
        {
            _F = new MFormula[0];
            _D = null;
            _T = new MTerm[2];
            _BV = new MBoundVariable[0];
            _T[0] = A;
            _T[1] = B;
        }

        public override int CountUndefinedFormulas(List<MFormula> idlist = null)
        {
            bool start = false;
            if (idlist == null) start = true;
            if (start)
            {
                localID = 0;
            }
            else
                idlist.Add(this);
            return 1;
        }

        public override bool[] Evaluate(int c = -1)
        {
            if (c == -1)
            {
                c = CountUndefinedFormulas();
            }

            bool[] ret = new bool[Helper.Pow(2, c)];
            int step = Helper.Pow(2, localID + 1);
            for (int i = 0; i < ret.Length; i += step)
            {
                for (int j = i; j < i + step / 2; j++)
                    ret[j] = true;
                for (int j = i + step / 2; j < i + step; j++)
                    ret[j] = false;
            }

            return ret;
        }

        public override MObject Copy()
        {
            MFormula F = new MEqualityFormula(_T[0].Copy() as MTerm, _T[1].Copy() as MTerm);
            return F;
        }

        public override string ToString()
        {
            return _T[0].ToString() + " = " + _T[1].ToString();
        }

        public override MVisualisation GetVisualisation(MVisualisationScheme VS = null)
        {
            MVisualisation V = new MVisualisation(this, MEqualityDefinition.DefaultDefinition.DefaultVisualization);
            V.SetSub(_T[0].GetVisualisation(), 0);
            V.SetSub(_T[1].GetVisualisation(), 1);
            Visualization = V;
            return V;
        }
    }

    public partial class MNegationFormula : MFormula
    {
        public MNegationFormula(MFormula F) : base()
        {
            _F = new MFormula[1];
            _D = null;
            _T = new MTerm[0];
            _BV = new MBoundVariable[0];
            _F[0] = F;
        }

        public override MObject Copy()
        {
            MFormula F = new MNegationFormula((MFormula)_F[0].Copy());
            return F;
        }

        public override bool[] Evaluate(int c = -1)
        {
            if (c == -1)
            {
                c = CountUndefinedFormulas();
            }

            bool[] ret = _F[0].Evaluate(c);

            for (int i = 0; i < ret.Length; i++)
                ret[i] = !ret[i];

            return ret;
        }

        public override string ToString()
        {
            return "-.( " + _F[0].ToString() + " )";
        }

        public override MVisualisation GetVisualisation(MVisualisationScheme VS = null)
        {
            MVisualisation V = new MVisualisation(this, MNegationDefinition.DefaultDefinition.DefaultVisualization);
            V.SetSub(_F[0].GetVisualisation());
            Visualization = V;
            return V;
        }
    }

    public partial class MBinaryConnectiveFormula : MFormula
    {
        public MBinaryConnectiveFormula(MBinaryConnective C, MFormula F, MFormula G) : base()
        {
            _F = new MFormula[2];
            _D = C;
            _T = new MTerm[0];
            _BV = new MBoundVariable[0];
            _F[0] = F;
            _F[1] = G;
        }

        public override MObject Copy()
        {
            MFormula F = new MBinaryConnectiveFormula((MBinaryConnective)_D, (MFormula)_F[0].Copy(), (MFormula)_F[1].Copy());
            return F;
        }

        public override bool[] Evaluate(int c = -1)
        {
            if (c == -1)
            {
                c = CountUndefinedFormulas();
            }

            bool[] fEV = _F[0].Evaluate(c);
            bool[] gEV = _F[1].Evaluate(c);
            bool[] ret = new bool[fEV.Length];
            MBinaryConnective castC = (MBinaryConnective)_D;

            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = false;
                if (fEV[i] && gEV[i]) ret[i] = castC._tt;
                if (fEV[i] && !gEV[i]) ret[i] = castC._tf;
                if (!fEV[i] && gEV[i]) ret[i] = castC._ft;
                if (!fEV[i] && !gEV[i]) ret[i] = castC._ff;
            }

            return ret;
        }

        public override MFormula Reduce(bool start = true)
        {
            MFormula F = base.Reduce(start);

            if (F is MBinaryConnectiveFormula)
            {
                // TRUE -> F => F
                if (F._F[0] is MTrivialFormula)
                {
                    MTrivialFormula castF = (MTrivialFormula)F._F[0];
                    MBinaryConnective castC = (MBinaryConnective)F._D;
                    if (castF._V == true && castC._tt == true && castC._tf == false)
                        return F._F[1];
                }
                // F <= F <- TRUE
                if (F._F[1] is MTrivialFormula)
                {
                    MTrivialFormula castF = (MTrivialFormula)F._F[1];
                    MBinaryConnective castC = (MBinaryConnective)F._D;
                    if (castF._V == true && castC._tt == true && castC._ft == false)
                        return F._F[0];
                }
            }
            return F;
        }

        public override string ToString()
        {
            return "( " + _F[0].ToString() + " ) " + _D.stringSymbol + " ( " + _F[1].ToString() + " )";
        }
    }

    public partial class MQuantifierFormula : MFormula
    {
        public bool IsPredicate => (_D as MQuantifier).IsPredicate;

        public MQuantifierFormula(MQuantifier Q,MTerm[] T, MVariable[] V, MFormula[] F) : base()
        {
            _F = F;
            _D = Q;
            _T = T;
            _BV = new MBoundVariable[Q.boundVarCount];

            for(int i=0;i<Q.boundVarCount;i++)
            _BV[i] = V[i].CreateBoundInstance(this);
            UpdateBinding();
        }

        public MFormula GetFreeFormula()
        {
            return _F[0].GetFreeFormula(_BV[0] as MBoundVariable);
        }

        public override int CountUndefinedFormulas(List<MFormula> idlist = null)
        {
            bool start = false;
            if (idlist == null) start = true;
            if (start)
            {
                localID = 0;
            }
            else
                idlist.Add(this);
            return 1;
        }

        public override void UpdateBinding()
        {
            foreach (MBoundVariable v in _BV)
                BindVariable(v);

            base.UpdateBinding();
        }

        public override bool[] Evaluate(int c = -1)
        {
            if (c == -1)
            {
                c = CountUndefinedFormulas();
            }

            bool[] ret = new bool[Helper.Pow(2, c)];
            int step = Helper.Pow(2, localID + 1);
            for (int i = 0; i < ret.Length; i += step)
            {
                for (int j = i; j < i + step / 2; j++)
                    ret[j] = true;
                for (int j = i + step / 2; j < i + step; j++)
                    ret[j] = false;
            }

            return ret;
        }

        public override MObject Copy()
        {
            MFormula F = new MQuantifierFormula((MQuantifier)_D, _T.Select(T => T.Copy() as MTerm).ToArray(), _BV, _F.Select(f => f.Copy() as MFormula).ToArray());
            return F;
        }

        public override string ToString()
        {
            return _D.stringSymbol + " " + _BV[0].ToString() + ": ( " + _F[0].ToString() + " )";
        }
    }  
}