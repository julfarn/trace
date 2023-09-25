using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraceBackend
{
    public partial class MExpression : MObject
    {
        public MFormula[] _F;
        public MTerm[] _T;
        public MBoundVariable[] _BV; // These are variables being bound by this Expression. Inherited Bound Variables will be found in _T.
        public MDefinition _D;
        protected int visID = 0;
        public int VisualizationID { get { return visID; } set { visID = value; GetVisualisation(); } }
        MVisualisation vis;
        public MVisualisation Visualization { get { return vis; } set { vis = value; BracketMode = brMode; } }
        protected BracketMode brMode = BracketMode.Auto;
        public BracketMode BracketMode
        {
            get { return brMode; }
            set
            {
                brMode = value;
                if (Visualization == null)
                    return;
                Visualization.BracketMode = value;
            }
        }

        public int localID;

        public int SubCount => _T.Length + _F.Length + _BV.Length;
        public MExpression this[int i]
        {
            get
            {
                return (i < _T.Length) ? _T[i] :
                    (i < _F.Length + _T.Length) ? (MExpression)_F[i - _T.Length] :
                    (i < SubCount) ? _BV[i - _F.Length - _T.Length] :
                    null;
            }

            set //TODO: make protected / internal?
            {
                if (i < 0 || i >= SubCount) throw new IndexOutOfRangeException();
                if (i < _T.Length) if (value is MTerm T) _T[i] = T; else throw new ArgumentException("Expected MTerm");
                else if (i < _F.Length + _T.Length) if (value is MFormula F) _F[i - _T.Length] = F; else throw new ArgumentException("Expected MFormula");
                else if (value is MBoundVariable BV) _BV[i - _F.Length - _T.Length] = BV;
                else if (value is MVariable V) _BV[i - _F.Length - _T.Length] = V.CreateBoundInstance(this); else throw new ArgumentException("Expected MBoundVariable or MVariable");
            }
        }

        public virtual bool ContainsVariable(MVariable V)
        {
            foreach (MTerm T in _T)
                if (T.ContainsVariable(V)) return true;
            foreach (MFormula F in _F)
                if (F.ContainsVariable(V)) return true;
            foreach (MBoundVariable BV in _BV)
                if (BV.Identical(V)) return true;
            return false;
        }

        public virtual List<MVariable> MakeFreeVariableList()
        {
            List<MVariable> vars = new List<MVariable>();
            for (int i = 0; i < SubCount; i++)
                vars = vars.Union(this[i].MakeFreeVariableList()).ToList();

            return vars;
        }

        public byte[] GetSubPath(MExpression E, byte[] path = null)
        {
            if (path == null) path = new byte[] { 0 };

            if (this == E) return path;

            for (int i = 0; i < SubCount; i++)
            {
                byte[] ret = this[i].GetSubPath(E, path.Add((byte)i));

                if (ret != null) return ret;
            }

            return null;
        }

        public MExpression GetSub(byte[] path, int position = 1)
        {
            if (position == path.Length) return this;
            return this[path[position]].GetSub(path, position + 1);
        }

        public virtual int CountUndefinedFormulas(List<MFormula> idlist = null)
        {
            bool start = false;
            int c = 0;
            if (idlist == null) start = true;
            if (start) idlist = new List<MFormula>();
            for (int i = 0; i < SubCount; i++)
                c += this[i].CountUndefinedFormulas(idlist);
            if (start)
            {
                idlist = idlist.Distinct().ToList();
                int doubles = 0;
                for (int i = 0; i < idlist.Count; i++)
                {
                    idlist[i].localID = i-doubles;
                    if (!(idlist[i] is MPlaceholderFormula))
                        for (int j = 0; j < i; j++)
                        {
                            if (idlist[i].Identical(idlist[j]))
                            {
                                idlist[i].localID = idlist[j].localID;

                                doubles++;
                                break;
                            }
                        }
                }
                return idlist.Count - doubles;
            }
            return c;
        }
        
        public virtual void BindVariable(MBoundVariable BoundInstance)
        {
            for (int i = 0; i < _T.Length; i++)
                if (_T[i].Identical(BoundInstance._FreeInstance) || (_T[i] is MBoundVariable BV  && BV._FreeInstance.Identical(BoundInstance._FreeInstance)))
                    _T[i] = BoundInstance;

            for (int i = 0; i < _T.Length; i++)
                _T[i].BindVariable(BoundInstance);
            for (int i = 0; i < _F.Length; i++)
                _F[i].BindVariable(BoundInstance);
        }

        public virtual void UpdateBinding()
        {
            for (int i = 0; i < _T.Length; i++)
                _T[i].UpdateBinding();
            for (int i = 0; i < _F.Length; i++)
                _F[i].UpdateBinding();
        }

        public override bool Identical(MObject E, IdTable<MObject, MObject> idTable = null)
        {
            if (this == E) return true;
            if (E.GetType() != GetType()) return false;


            MExpression castE = (MExpression)E;


            //Check for Logic Connective Consistency
            if (_D != castE._D) return false;

            //Check if Lists are null or of different lengths
            if (_T == null || castE._T == null || _T.Length != castE._T.Length) return false;
            if (_F == null || castE._F == null || _F.Length != castE._F.Length) return false;
            if (_BV == null || castE._BV == null || _BV.Length != castE._BV.Length) return false;


            //Check for BoundVariable consistency
            for (int i = 0; i < _BV.Length; i++)
            {
                // Check if any BoundVariables are null or of different Types.
                if (_BV[i] == null || castE._BV[i] == null) return false;

                // Each BoundVariable must have exactly one correspondance.
                if (idTable == null)
                    idTable = new IdTable<MObject, MObject>(IdRuleset.BothUnique);
                if (!idTable.Identify(_BV[i], castE._BV[i])) return false;
            }

            //Check for Term consistency
            for (int i = 0; i < _T.Length; i++)
            {
                // Check if any Terms are null or of different Types.
                if (_T[i] == null || castE._T[i] == null) return false;
                if (_T[i].GetType() != castE._T[i].GetType()) return false;

                if (_T[i] is MBoundVariable)
                {
                    // Each BoundVariable must have exactly one correspondance.
                    if (idTable == null)
                        idTable = new IdTable<MObject, MObject>(IdRuleset.BothUnique);
                    if (!idTable.Identify(_T[i], castE._T[i])) return false;
                }
                // Variables and Functions must be identical.
                else if (!_T[i].Identical(castE._T[i])) return false;
            }

            //Check for Undefined Formula consistensy
            for (int i = 0; i < _F.Length; i++)
            {
                if (_F[i] is MUndefinedPredicateFormula castF)
                {
                    if (!(castE._F[i] is MUndefinedPredicateFormula castEcastF)) return false;
                    if (idTable == null)
                        idTable = new IdTable<MObject, MObject>(IdRuleset.BothUnique);
                    if (!idTable.Identify(castF.PseudoDefinition, castEcastF.PseudoDefinition)) return false;
                    if (!_F[i].Identical(castE._F[i], idTable)) return false;
                }
                else if (!_F[i].Identical(castE._F[i], idTable)) return false;
            }

            return true;
        }

        public void ChangeSub(byte[] path, MExpression New, int position = 1)
        {
            if (path.Length <= 1) throw new Exception("This would mean to replace the whole Expression...");

            if (position == path.Length - 1)
                this[path[position]] = New;
            else
                this[path[position]].ChangeSub(path, New, position + 1);

            UpdateBinding();
        }

        internal virtual void MakeAxiom(MStatement Axiom, bool unmake, List<MVariable> restrictions, bool events)
        {
            foreach (MTerm T in _T) T.MakeAxiom(Axiom, unmake, restrictions, events);
            foreach (MFormula F in _F) F.MakeAxiom(Axiom, unmake, restrictions, events);
        }

        internal MExpression ReplaceFormula(MFormula Old, MFormula New, bool start = true, MExpression original = null)
        {
            MExpression E;
            if (start == true)
            {
                E = (MExpression)Copy();
                original = this;
            }
            else E = this;

            for (int i = 0; i < SubCount; i++)
                if (original[i].Identical(Old)) //replace
                    E[i] = New.Copy() as MFormula;
                else E[i] = E[i].ReplaceFormula(Old, New, false, original[i]); //iterate

            return E;
        }

        internal MExpression ReplacePredicates(IdTable<MUndefinedPredicateFormula, MFormula> idTable, bool start = true, MExpression original = null)
        {
            MExpression E;
            if (start == true)
            {
                E = (MExpression)Copy();
                original = this;
            }
            else E = this;

            // replace here
            // Example: idTable = A(x) << B(x, a).
            // this:    y in { x | A(x) } <=> A(y)
            // result:  y in { x | B(x, a) } <=> B(y, a)
            for (int i = 0; i < SubCount; i++)
                for (int j = 0; j < idTable.Left.Count; j++)
                    if (original[i] is MUndefinedPredicateFormula UPF && UPF.PseudoDefinition.Identical(idTable.Left[j].PseudoDefinition))
                    {
                        MFormula NewFormula = idTable.Right[j];
                        for (int k = 0; k < UPF.PseudoDefinition.argumentCount; k++)
                        {
                            if (!UPF._T[k].Identical(idTable.Left[j]._T[k]))
                            {
                                NewFormula = NewFormula.ReplaceTerm(idTable.Left[j]._T[k], UPF._T[k]) as MFormula;
                                if(idTable.Left[j]._T[k] is MBoundVariable BV)
                                    NewFormula = NewFormula.ReplaceTerm(BV._FreeInstance, UPF._T[k]) as MFormula;
                            }
                        }

                        E[i] = NewFormula;
                    } else E[i] = E[i].ReplacePredicates(idTable, false, original[i]); //iterate


            return E;
        }

        internal MExpression ReplaceVariable(MVariable Old, MTerm New, bool start = true, MExpression original = null)
        {
            MExpression E;
            if (start == true)
            {
                E = (MExpression)Copy();
                original = this;
            }
            else E = this;

            // replace here
            for (int i = 0; i < SubCount; i++)
                if (original[i] == Old)
                {
                    if (New is MFunction f)
                        E[i] = f.Copy() as MFunction;
                    else
                        E[i] = New;
                }
                else
                    E[i] = E[i].ReplaceVariable(Old, New, false, original[i]); //iterate

            //TODO: do we replace BoundVariables as well?

            return E;
        }

        internal MExpression ReplaceTerm(MTerm Old, MTerm New, bool start = true, MExpression original = null)
        {
            MExpression E;
            if (start == true)
            {
                E = (MExpression)Copy();
                original = this;
            }
            else E = this;

            // replace here
            for (int i = 0; i < SubCount; i++)
                if (original[i].Identical(Old))
                {
                    if (New is MFunction f)
                        E[i] = f.Copy() as MFunction;
                    else
                        E[i] = New;
                }
                else
                    E[i] = E[i].ReplaceTerm(Old, New, false, original[i]); // iterate

            return E;
        }

        internal MExpression ReplaceVariables(IdTable<MVariable, MTerm> idTable, bool start = true, MExpression original = null)
        {
            MExpression E;
            if (start == true)
            {
                E = (MExpression)Copy();
                original = this;
            }
            else E = this;

            // replace here
            for (int i = 0; i < SubCount; i++)
                for (int j = 0; j < idTable.Left.Count; j++)
                    if (original[i] == idTable.Left[j])
                    {
                        if (idTable.Right[j] is MFunction f)
                            E[i] = f.Copy() as MFunction;
                        else
                            E[i] = idTable.Right[j];
                    }
                    else E[i] = E[i].ReplaceVariables(idTable, false, original[i]); //iterate

            return E;
        }

        internal MExpression SetTrueFormulas(List<MStatement> trueFormulas, bool start = true, MExpression original = null)
        {
            MExpression E;
            if (start == true)
            {
                //check if maybe the whole thing is trivial
                if (this is MFormula)
                    for (int j = 0; j < trueFormulas.Count; j++)
                        if (Identical(trueFormulas[j]._F))
                            return MTrivialFormula._True;

                E = (MExpression)Copy();
                original = this;
            }
            else E = this;

            // replace here
            for (int i = 0; i < _F.Length; i++)
                for (int j = 0; j < trueFormulas.Count; j++)
                    if (original._F[i].Identical(trueFormulas[j]._F))
                        E._F[i] = MTrivialFormula._True;

            // iterate
            for (int i = 0; i < SubCount; i++)
                E[i] = E[i].SetTrueFormulas(trueFormulas, false, original[i]);

            return E;
        }

        public virtual void MakeUndefinedFormulaList(List<MUndefinedPredicate> pdlist)
        {
            foreach (MFormula F in _F)
                F.MakeUndefinedFormulaList(pdlist);
            foreach (MTerm T in _T)
                T.MakeUndefinedFormulaList(pdlist);
        }

        internal virtual MFormula FindOccurenceOfPredicate(MUndefinedPredicate P)
        {
            for(int i = 0; i<SubCount; i++)
            {
                MFormula pred = this[i].FindOccurenceOfPredicate(P);
                if (pred != null) return pred;
            }

            return null;
        }

        public virtual bool ContainsUndifinedPredicate()
        {
            for (int i = 0; i < SubCount; i++)
                if (this[i].ContainsUndifinedPredicate()) return true;
            return false;
        }

        public virtual bool ContainsUnrestrictedVariable(Validity V, MVariable otherthan = null)
        {
            for (int i = 0; i < SubCount; i++)
                if (this[i].ContainsUnrestrictedVariable(V, otherthan)) return true;
            return false;
        }

        public virtual MVisualisation GetVisualisation(MVisualisationScheme VS = null)
        {
            if(_D == null && VS == null) throw new Exception("GetVisualisation() should not be called on MExpression, only on children.");

            MVisualisation V = new MVisualisation(this,  VS ?? _D?.GetVisualisation(VisualizationID) );
            for (int i = 0; i < SubCount; i++)
                V.SetSub(this[i].GetVisualisation(), i);

            Visualization = V;

            return V;
        }

        public virtual void ReferenceInDocument(MDocument D)
        {
            if (_D != null) D.ReferenceDocument(_D._X.Document);
        }
    }
}
