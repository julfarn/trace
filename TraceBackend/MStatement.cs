using System;
using System.Collections.Generic;
using System.Linq;

namespace TraceBackend
{
    public partial class MStatement : MObject, IElementWithFileID
    {
        public event EventHandler ExpressionChanged; //TODO: Invoke ... more?
        public event EventHandler ValidityChanged;

        public List<MVariable> RestrictedVariables; //If this is an Axiom or an Assumption, the Variables here will treat this as an Axiom.

        public FileID fileID;
        public FileID baseFileID => fileID;
        public MFormula _F;
        public MContext _X;

        Validity _valid;
        public Validity valid
        {
            get { return _valid; }
            set
            {
                if (!loaded) return;

                if(_valid != null)
                    _valid.ValidityChanged -= Valid_ValidityChanged;
                _valid = value;
                if(_valid != null)
                    _valid.ValidityChanged += Valid_ValidityChanged;
                ValidityChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public string LaTeXStart => valid.IsAxiom ? @"\begin{axiom}
\begin{equation}
" + _F.Visualization.GetLaTeX() + @"
\end{equation}
\end{axiom}" : @"\begin{equation}
" + _F.Visualization.GetLaTeX() + @"
\end{equation}";
        public string LaTeXEnd => @"";

        private void Valid_ValidityChanged(object sender, EventArgs e)
        {
            ValidityChanged?.Invoke(this, EventArgs.Empty);
        }

        private MStatement(MContext X, MFormula F, Validity V) : base()
        {
            _X = X;
            ExpressionChanged += Expression_Changed;
            ValidityChanged += MStatement_ValidityChanged;
            _valid = V;
            if (_valid != null)
                _valid.ValidityChanged += Valid_ValidityChanged;
            _F = F;

            RestrictedVariables = new List<MVariable>();
        }

        public void AddRestrictedVariable(MVariable V)
        {
            RestrictedVariables.Add(V);
            ValidityChanged?.Invoke(this, EventArgs.Empty);
        }

        public void RemoveRestrictedVariable(MVariable V)
        {
            RestrictedVariables.Remove(V);
            ValidityChanged?.Invoke(this, EventArgs.Empty);
        }

        private void MStatement_ValidityChanged(object sender, EventArgs e)
        {
            _F.MakeAxiom(this, !valid.IsAxiom, RestrictedVariables, true);
        }

        private void Expression_Changed(object sender, EventArgs e)
        {
            if (valid.IsAxiom) _F.MakeAxiom(this, false, RestrictedVariables, true);
        }

        internal MStatement(FileID ID, MContext X, MFormula F, Validity V) : this(X, F, V)
        {
            fileID = ID;
        }

        internal MStatement(FileIDManager IDM, MContext X, MFormula F, Validity V) : this(X, F, V)
        {
            fileID = IDM.RequestContextStatementFileID(this);
            loaded = true;
        }
        internal MStatement(FileIDManager IDM, MTheorem T, MContext X, MFormula F, Validity V) : this(X, F, V)
        {
            fileID = IDM.RequestTheoremStatementFileID(this, T);
            loaded = true;
        }
        internal MStatement(FileIDManager IDM, MDefinition D, MContext X, MFormula F, Validity V) : this(X, F, V)
        {
            fileID = IDM.RequestDefinitionStatementFileID(this, D);
            loaded = true;
        }
        internal MStatement(FileIDManager IDM, MDeductionStep DS, MContext X, MFormula F, Validity V) : this(X, F, V)
        {
            fileID = IDM.RequestDeductionStepStatementFileID(this, DS);
            loaded = true;
        }

        public MFormula GetFormula()
        {
            if (_F == null) Load();
            return _F;
        }

        public void InvokeExpressionChanged() { ExpressionChanged?.Invoke(this, EventArgs.Empty); }

        public void SetFormula(MFormula F)
        {
            if (loaded)
            {
                _F = F;
                ExpressionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void UpdateAxiom(bool events)
        {
            _F.MakeAxiom(this, !valid.IsAxiom, RestrictedVariables, events);
        }

        public override bool Identical(MObject O, IdTable<MObject,MObject> idTable = null)
        {
            if (O is MStatement castO)
            {
                if (_F.Identical(castO._F) && _X.Identical(castO._X))
                    return true;
            }
            return false;
        }

        public void Evaluate()
        {
            if(loaded)
            if (GetFormula().IsTautology()) valid = Validity.Valid;
        }

        internal void TransferToNewContext(MContext X)
        {
            _X = X;
            fileID.ApplyContext(X);
        }

        public override string ToString()
        {
            return _F.ToString();
        }

        public MVisualisation GetVisualisation()
        {
            return _F.GetVisualisation();
        }

        public enum MStatementLocation
        {
            InContext = 0,
            InDefinition = 1,
            InTheorem = 2,
            InDeductionStep = 3
        }

        internal bool IsTautology()
        {
            bool t = GetFormula().IsTautology();
            if (t) valid = Validity.Valid;
            return t;
        }

        public bool ContainsUnrestrictedVariable(MVariable otherthan = null)
        {
            return _F.ContainsUnrestrictedVariable(valid, otherthan);
        }
    }

    public partial class Validity
    {
        private static Validity _Valid, _Invalid, _Axiom;
        public static Validity Valid => _Valid.Copy();
        public static Validity Invalid => _Invalid.Copy();
        public static Validity Axiom => _Axiom.Copy();
        private bool _valid;
        private bool _axiom;
        public bool IsDependent { get { return Conditions != null && Conditions.Count > 0; } }
        public bool IsValid
        {
            get { return _valid; }
            set
            {
                if (_valid != value) { _valid = value; ValidityChanged?.Invoke(this, EventArgs.Empty); }
            }
        }
        public bool IsAxiom
        {
            get { return _axiom; }
            set
            {
                if (_axiom != value) { _axiom = value; ValidityChanged?.Invoke(this, EventArgs.Empty); }
            }
        }
        public bool IsAssumption => IsAxiom && IsDependent;

        public List<MStatement> Conditions;

        public event EventHandler ValidityChanged;

        static Validity()
        {
            _Valid = new Validity() { _valid = true, _axiom = false };
            _Invalid = new Validity() { _valid = false, _axiom = false };
            _Axiom = new Validity() { _valid = true, _axiom = true };
        }

        public void AddDependence(MStatement Statement)
        {
            if (Conditions == null) Conditions = new List<MStatement>();
            Conditions.Add(Statement);
        }

        public void RemoveDependence(MStatement Statement)
        {
            Conditions.Remove(Statement);
        }

        public void Invalidate(bool sendEvents)
        {
            if (!_valid) return;
            _valid = false;
            if (sendEvents) ValidityChanged?.Invoke(this, EventArgs.Empty);
        }

        public Validity Copy()
        {
            Validity V = new Validity();
            V._valid = _valid;
            V._axiom = _axiom;
            V.Conditions = Conditions?.ToList();
            return V;
        }

        public static Validity operator &(Validity v1, Validity v2)
        {
            Validity v = new Validity();

            v._valid = v1._valid && v2._valid;
            v._axiom = v1._axiom && v2._axiom;

            if (v1.Conditions != null)
            {
                if (v2.Conditions != null)
                {
                    v.Conditions = v1.Conditions.Union(v2.Conditions).Distinct().ToList();
                }
                else
                    v.Conditions = v1.Conditions.ToList();
            }
            else if (v2.Conditions != null)
                v.Conditions = v2.Conditions.ToList();

            return v;
        }

        public bool Equivalent(Validity V)
        {
            if (Conditions != null)
            {
                if (V.Conditions == null || V.Conditions.Count != Conditions.Count) return false;

                for (int i = 0; i < Conditions.Count; i++)
                    if (Conditions[i] != V.Conditions[i]) return false;
            }
            else if (V.Conditions != null) return false;

            return _valid == V._valid && _axiom == V._axiom;
        }

        internal void SelfReference(MStatement S)
        {
            if (Conditions == null) return;

            for (int i = 0; i < Conditions.Count; i++)
                if (Conditions[i] == null) Conditions[i] = S;
        }
    }
    
}
