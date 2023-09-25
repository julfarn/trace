using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using TraceBackend.AI;

namespace TraceBackend
{
    public partial class MDefinition : MObject, IElementWithFileID
    {
        public bool loaded = false;
        protected MVisualisation Placeholder;

        public DefinitionFileID fileID;
        public FileID baseFileID => fileID;
        public MContext _X;
        public List<MStatement> Axioms;
        public List<MVisualisationScheme> visualisations;
        public int defaultVisIndex = 0;
        public MVisualisationScheme DefaultVisualization => visualisations[defaultVisIndex];

        public string LaTeXStart => @"\begin{definition}";

        public string LaTeXEnd => @"\end{definition}";

        private NeuralComplex _mind;
        public string MindPath => Properties.Settings.Default.AIDirectory + "\\" + _X.Document.Name + fileID.ToXML(_X.Document) + ".mind";
        public NeuralComplex Mind => _mind == null ? DefinitionReader.FromFile(MindPath) : _mind;
        public void SaveMind()
        {
            (Mind.Operation(DefinitionReader.lmind) as NeuralNetwork).ToFile();
        }


        public virtual MVisualisation GetPlaceholderVisualisation(MVisualisationScheme scheme, bool update = false)
        {
            throw new InvalidOperationException("call this only on children.");
        }

        protected MDefinition(MContext X) : base()
        {
            _X = X;
            Axioms = new List<MStatement>();
            visualisations = new List<MVisualisationScheme>();
        }

        internal IEnumerable<MStatement> FindAxiomsForVariable(MVariable V)
        {
            if (loaded)
                return Axioms.Where(S => S._F.ContainsVariable(V));

            for (int i = 0; i < Axioms.Count; i++)
                if (Axioms[i] == null) LoadAxiom(i);
            return Axioms.Where(S => S._F.ContainsVariable(V));
        }

        internal MDefinition(DefinitionFileID ID, MContext X) : this(X)
        {
            fileID = ID;
        }

        internal MDefinition(FileIDManager IDM, MContext X) : this(X)
        {
            fileID = IDM.RequestDefinitionFileID(this);
        }

        public MStatement CreateAxiom(MFormula F, int index = -1)
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");
            MStatement A = new MStatement(fileID.IDManager, this, _X, F, Validity.Axiom);
            if (index < 0)
                Axioms.Add(A);
            else
                Axioms.Insert(index, A);
            A.loaded = true;
            return A;
        }
        public virtual MVisualisationScheme CreateVisualisation(bool skeleton = true)
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");
            MVisualisationScheme v = new MVisualisationScheme(this);
            visualisations.Add(v);
            return v;
        }


        public MStatement GetAxiom(int index)
        {
            if (Axioms[index] == null) LoadAxiom(index);

            return Axioms[index];
        }
        public MStatement GetAxiom(DefinitionStatementFileID ID)
        {
            //Look for loaded Axioms
            foreach (MStatement A in Axioms)
                if (A != null && A.fileID.IsSubFileID(ID)) return A;

            if (loaded) return null;
            //Look for unloaded contexts
            return LoadAxiom(ID);
        }
        public MVisualisationScheme GetVisualisation(int index)
        {
            if (visualisations.Count < index || visualisations[index] == null) LoadVisualizations();
            return visualisations[index];
        }
        public void GetVisualisations()
        {
            if (visualisations.Count == 0 || visualisations[0] == null) LoadVisualizations();
        }

        public virtual int CountSubVisualisations()
        {
            return 0;
        }

        internal virtual int GetTermIndex(int index)
        {
            return 0;
        }

        internal virtual int GetFormulaIndex(int index)
        {
            return 0;
        }

        internal void TransferToNewContext(MContext X)
        {
            _X = X;
            fileID.ApplyContext(_X);
            foreach (MStatement Ax in Axioms)
            {
                Ax.TransferToNewContext(X);
            }
        }

        public override string ToString()
        {
            if (Axioms.Count == 0) return "";
            string ret = "Axioms:\n";
            foreach (MStatement A in Axioms)
            {
                ret = ret + A.ToString() + "\n";
            }
            return ret;
        }

    }

    public partial class MLogicConnective : MDefinition
    {
        public MLogicConnective(MContext X) : base(X)
        {

        }

    }

    public partial class MFunctionSymbol : MLogicConnective
    {
        public int argumentCount = 0;
        public int boundVarCount = 0;
        public int formulaCount = 0;

        protected MFunctionSymbol(MContext X, int aCount, int bvCount, int fCount, string symbol = "f") : base(X)
        {
            argumentCount = aCount;
            stringSymbol = symbol;
            boundVarCount = bvCount;
            formulaCount = fCount;
        }

        public override MVisualisation GetPlaceholderVisualisation(MVisualisationScheme scheme, bool update = false)
        {
            if (Placeholder == null || update)
            {
                //Placeholder
                MTerm[] t = new MTerm[argumentCount];
                for (int i = 0; i < argumentCount; i++) t[i] = MTerm.PlaceholderTerm;
                MVariable[] v = new MVariable[boundVarCount];
                for (int i = 0; i < boundVarCount; i++) v[i] = MVariable.PlaceholderVariable;
                MFormula[] f = new MFormula[formulaCount];
                for (int i = 0; i < formulaCount; i++) f[i] = MFormula.PlaceholderFormula;
                MFunction pF = new MFunction(this, t, v, f);
                Placeholder = pF.GetVisualisation(scheme);
            }
            return Placeholder;
        }

        public MFunctionSymbol(DefinitionFileID ID, MContext X, int aCount, int bvCount, int fCount, string symbol = "f") : this(X, aCount, bvCount, fCount, symbol)
        {
            fileID = ID;
        }

        public MFunctionSymbol(FileIDManager IDM, MContext X, int aCount, int bvCount, int fCount, string symbol = "f") : this(X, aCount, bvCount, fCount, symbol)
        {
            fileID = IDM.RequestDefinitionFileID(this);
        }

        public override MVisualisationScheme CreateVisualisation(bool skeleton = true)
        {
            MVisualisationScheme V =  base.CreateVisualisation();
            if (skeleton)
            {
                int lastInd = V.AddSymbol(new MTextSymbol(stringSymbol, FontCategory.FormulaCursive));
                for (int i = 0; i < CountSubVisualisations(); i++)
                {
                    V.Arrange(i, lastInd);
                    lastInd = i;
                }
            }
            return V;
        }

        public override int CountSubVisualisations()
        {
            return argumentCount + boundVarCount + formulaCount;
        }

        internal override int GetTermIndex(int index)
        {
            return index;
        }

        internal override int GetFormulaIndex(int index)
        {
            return argumentCount + boundVarCount + index;
        }

        public override string ToString()
        {
            return argumentCount.ToString() + "-ary function symbol " + stringSymbol + "\n" + base.ToString();
        }
    }
    
    public partial class MBinaryConnective : MLogicConnective
    {
        public bool _tt, _tf, _ft, _ff;
        public bool IsEquivalence => _tt && !_tf && !_ft && _ff;
        public bool IsImplication => _tt && !_tf && _ft && _ff;

        protected MBinaryConnective(MContext X, bool tt = false, bool tf = false, bool ft = false, bool ff = false, string symbol = "~") : base(X)
        {
            _tt = tt;
            _tf = tf;
            _ft = ft;
            _ff = ff;
            stringSymbol = symbol;

        }

        public override MVisualisation GetPlaceholderVisualisation(MVisualisationScheme scheme, bool update = false)
        {
            if (Placeholder == null || update)
            {
                //Placeholder
                MBinaryConnectiveFormula pF = new MBinaryConnectiveFormula(this, MFormula.PlaceholderFormula, MFormula.PlaceholderFormula);
                Placeholder = pF.GetVisualisation(scheme);
            }
            return Placeholder;
        }

        public MBinaryConnective(DefinitionFileID ID, MContext X, bool tt = false, bool tf = false, bool ft = false, bool ff = false, string symbol = "~") : this(X, tt, tf, ft, ff, symbol)
        {
            fileID = ID;
        }

        public MBinaryConnective(FileIDManager IDM, MContext X, bool tt = false, bool tf = false, bool ft = false, bool ff = false, string symbol = "~") : this(X, tt, tf, ft, ff, symbol)
        {
            fileID = IDM.RequestDefinitionFileID(this);
        }

        public override MVisualisationScheme CreateVisualisation(bool skeleton = true)
        {
            MVisualisationScheme V = base.CreateVisualisation();
            if (skeleton)
            {
                int lastInd = V.AddSymbol(new MTextSymbol(stringSymbol, FontCategory.FormulaUpright));
                V.Arrange(0, lastInd, MAnchor.left);
                V.Arrange(1, lastInd, MAnchor.right);
            }
            return V;
        }

        public override int CountSubVisualisations()
        {
            return 2;
        }

        internal override int GetTermIndex(int index)
        {
            return 0;
        }

        internal override int GetFormulaIndex(int index)
        {
            return index;
        }

        public override string ToString()
        {
            return "Binary Connective " + stringSymbol + " with truth Table:\n" +
                "A B | A " + stringSymbol + " B\n" +
                bts(true) + " " + bts(true) + " |   " + bts(_tt) + "\n" +
                bts(true) + " " + bts(false) + " |   " + bts(_tf) + "\n" +
                bts(false) + " " + bts(true) + " |   " + bts(_ft) + "\n" +
                bts(false) + " " + bts(false) + " |   " + bts(_ff) + "\n" +
                base.ToString();

            string bts(bool b) { return b ? "t" : "f"; }
        }
    }

    public partial class MQuantifier : MLogicConnective
    {
        public static MQuantifier PlaceholderQuantifier;
        public QuantifierType type = QuantifierType.Other;
        public int termCount = 0;
        public int boundVarCount = 0;
        public int formulaCount = 0;
        public bool IsPredicate => boundVarCount == 0 && formulaCount == 0;

        static MQuantifier()
        {
            PlaceholderQuantifier = new MQuantifier(MContext.Global,0, 1, 1, "(Quantifier)") // TODO: Is this needed? Is a 1-ary quantifier the right choice?
            { loaded = true };
            PlaceholderQuantifier.CreateVisualisation(false);
            PlaceholderQuantifier.DefaultVisualization.AddSymbol(new MTextSymbol("(Quantifier)", FontCategory.FormulaUpright));
        }

        public MQuantifier(MContext X, int tCount, int bvCount, int fCount, string symbol = "ForAll") : base(X)
        {
            stringSymbol = symbol;
            termCount = tCount;
            boundVarCount = bvCount;
            formulaCount = fCount;
        }

        public override MVisualisationScheme CreateVisualisation(bool skeleton = true)
        {
            MVisualisationScheme V = base.CreateVisualisation();
            if (skeleton)
            {
                int lastInd = V.AddSymbol(new MTextSymbol(stringSymbol, FontCategory.FormulaUpright));
                for (int i = 0; i < CountSubVisualisations(); i++)
                {
                    V.Arrange(i, lastInd);
                    lastInd = i;
                }
            }

            return V;
        }

        public override MVisualisation GetPlaceholderVisualisation(MVisualisationScheme scheme, bool update = false)
        {
            if (Placeholder == null || update)
            {
                //Placeholder
                MQuantifierFormula pF = new MQuantifierFormula(this, 
                    Enumerable.Repeat(MTerm.PlaceholderTerm, termCount).ToArray(), 
                    Enumerable.Repeat(MVariable.PlaceholderVariable, boundVarCount).ToArray(),
                    Enumerable.Repeat(MFormula.PlaceholderFormula, formulaCount).ToArray());
                Placeholder = pF.GetVisualisation(scheme);
            }
            return Placeholder;
        }

        public MQuantifier(DefinitionFileID ID, MContext X, int tCount, int bvCount, int fCount, string symbol = "ForAll") : this(X,tCount, bvCount, fCount, symbol)
        {
            fileID = ID;
        }

        public MQuantifier(FileIDManager IDM, MContext X, int tCount, int bvCount, int fCount, string symbol = "ForAll") : this(X, tCount, bvCount, fCount, symbol)
        {
            fileID = IDM.RequestDefinitionFileID(this);
        }

        public override int CountSubVisualisations()
        {
            return termCount+boundVarCount + formulaCount;
        }

        internal override int GetTermIndex(int index)
        {
            return index;
        }

        internal override int GetFormulaIndex(int index)
        {
            return index + termCount + boundVarCount;
        }

        public override string ToString()
        {
            switch (type)
            {
                case QuantifierType.Existential:
                    return "Existential Quantifier " + stringSymbol + "\n" + base.ToString();
                case QuantifierType.Universal:
                    return "Universal Quantifier " + stringSymbol + "\n" + base.ToString();
                case QuantifierType.Other:
                    return "Non-internal Quantifier " + stringSymbol + "\n" + base.ToString();
                default:
                    return "Buggy Quantifier " + stringSymbol + "\n" + base.ToString();
            }
        }

        public enum QuantifierType
        {
            Universal = 0,
            Existential = 1,
            Other = 2
        }
    }

    public partial class MNegationDefinition : MLogicConnective
    {
        public static MNegationDefinition DefaultDefinition;

        static MNegationDefinition()
        {
            DefaultDefinition = new MNegationDefinition(MContext.Global) { loaded = true };
            MVisualisationScheme VS = DefaultDefinition.CreateVisualisation();
            //VS.AddSymbol(new MTextSymbol("Not "));
            VS.AddSymbol(MShapeSymbol.FromName("negation"));
            VS.Arrange(0, 1, MAnchor.right);
            VS.Latex = @"\neg %0%";
        }

        public MNegationDefinition(MContext X, string symbol = "~") : base(X)
        {
            stringSymbol = symbol;

        }

        public override MVisualisation GetPlaceholderVisualisation(MVisualisationScheme scheme, bool update = false)
        {
            if (Placeholder == null || update)
            {
                //Placeholder
                MNegationFormula pF = new MNegationFormula(MFormula.PlaceholderFormula);
                Placeholder = pF.GetVisualisation(scheme);
            }
            return Placeholder;
        }

        public MNegationDefinition(DefinitionFileID ID, MContext X, string symbol = "-.") : this(X, symbol)
        {
            fileID = ID;
        }

        public MNegationDefinition(FileIDManager IDM, MContext X, string symbol = "-.") : this(X, symbol)
        {
            fileID = IDM.RequestDefinitionFileID(this);
        }

        public override int CountSubVisualisations()
        {
            return 1;
        }

        internal override int GetTermIndex(int index)
        {
            return 0;
        }

        internal override int GetFormulaIndex(int index)
        {
            return 0;
        }

        public override string ToString()
        {
            return "(Dummy Negation Definition) " + stringSymbol + "\n" + base.ToString();
        }

        public enum QuantifierType
        {
            Universal = 0,
            Existential = 1,
            Other = 2
        }
    }

    public partial class MEqualityDefinition : MDefinition
    {
        public static MEqualityDefinition DefaultDefinition;

        static MEqualityDefinition()
        {
            DefaultDefinition = new MEqualityDefinition(MContext.Global) { loaded = true };
            MVisualisationScheme VS = DefaultDefinition.CreateVisualisation();
            int mainsym = VS.AddSymbol(new MTextSymbol(" = ", FontCategory.FormulaUpright));
            VS.Arrange(0, mainsym, MAnchor.left);
            VS.Arrange(1, mainsym, MAnchor.right);
            VS.Latex = @"%0% = %1%";
        }

        public MEqualityDefinition(MContext X, string symbol = "=") : base(X)
        {
            stringSymbol = symbol;

        }

        public override MVisualisation GetPlaceholderVisualisation(MVisualisationScheme scheme, bool update = false)
        {
            if (Placeholder == null || update)
            {
                //Placeholder
                MEqualityFormula pF = new MEqualityFormula(MTerm.PlaceholderTerm, MTerm.PlaceholderTerm);
                Placeholder = pF.GetVisualisation(scheme);
            }
            return Placeholder;
        }

        public MEqualityDefinition(DefinitionFileID ID, MContext X, string symbol = "=") : this(X, symbol)
        {
            fileID = ID;
        }

        public MEqualityDefinition(FileIDManager IDM, MContext X, string symbol = "=") : this(X, symbol)
        {
            fileID = IDM.RequestDefinitionFileID(this);
        }

        public override int CountSubVisualisations()
        {
            return 2;
        }

        internal override int GetTermIndex(int index)
        {
            return index;
        }

        internal override int GetFormulaIndex(int index)
        {
            return 0;
        }

        public override string ToString()
        {
            return "(Dummy Equality Definition) " + stringSymbol + "\n" + base.ToString();
        }

        public enum QuantifierType
        {
            Universal = 0,
            Existential = 1,
            Other = 2
        }
    }
}