using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace TraceBackend
{
    public partial class MDocument : IElementWithFileID
    {
        public bool Loaded = false;
        const ushort VERSION = 0;
        public string FilePath;
        public string Directory => Path.GetDirectoryName(FilePath);
        public string Name => Path.GetFileNameWithoutExtension(FilePath);
        public List<MContext> Contexts;
        public List<MDocument> ReferencedDocuments;
        internal FileIDManager IDManager;
        public DocumentStructure Structure;
        public FileID fileID;
        public FileID baseFileID => fileID;

        public string LaTeXStart =>
            @"\documentclass{article}
            \usepackage{amsthm}

            \newtheorem{definition}{Definition}
            \newtheorem{theorem}{Theorem}
            \newtheorem{axiom}{Axiom}


            \begin{document}

            \title{" + Name + @"}
            \maketitle
            ";

        public string LaTeXEnd => @"\end{document}";

        public MDocument()
        {
            
            //IDManager = new FileIDManager(this);
            fileID = new FileID(this);
            PDL = new DocumentLoader(this);
            Structure = DocumentStructure.Embed(this);
        }

        public MContext CreateContext(DocumentStructure AfterStructure = null)
        {
            if (!Loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");

            MContext X = new MContext(IDManager, this) {
                SuperContexts = new List<MContext>(),
                Variables = new List<MVariable>(),
                Definitions = new List<MDefinition>(),
                Axioms = new List<MStatement>(),
                Theorems = new List<MTheorem>()
            };
            if (AfterStructure == null)
                Contexts.Insert(0, X);
            else
                Contexts.Insert(Contexts.IndexOf(AfterStructure.Element as MContext) + 1, X);
            X.loaded = true;

            if (AfterStructure == null)
                DocumentStructure.Embed(X, Structure);
            else
                DocumentStructure.Insert(X, AfterStructure);
            return X;
        }

        //These methods are to be used when we introduce multiple contexts per document
        /*public MTheorem CreateTheorem(DocumentStructure AfterStructure)
        {
            int newindex = -1;
            MContext C = null;
            if (AfterStructure == null) C = CreateContext(); //TODO: Don't just create a new context, add to the beginning of the next. When creating a new, reorganize supercontexts.
            else
            {
                DocumentStructure prev = AfterStructure?.Previous<IElementWithFileID>();
                if (prev == null) { newindex = 0; C = AfterStructure.Parent.Element as MContext; }
                else
                {
                    if (prev.Element is MDefinition AD)
                    {
                        C = AD._X;
                        if (C.GetDefinition(C.Definitions.Count - 1) != AD || C.Axioms.Count != 0)
                        {
                            C = AfterStructure.Parent.SplitContext(AfterStructure).Element as MContext;
                            AfterStructure = null;
                        }
                    }
                    if (prev.Element is MStatement AA)
                    {
                        C = AA._X;
                        if (C.GetAxiom(C.Axioms.Count - 1) != AA)
                        {
                            C = AfterStructure.Parent.SplitContext(AfterStructure).Element as MContext;
                            AfterStructure = null;
                        }
                    }
                    if (prev.Element is MTheorem AT)
                    {
                        C = AT.Context;
                        newindex = C.Theorems.IndexOf(AT) + 1;
                    }
                    if (C == null) return null;
                }
            }

            MTheorem T = C.CreateTheorem(newindex);
            MStatement S = T.CreateStatement(MFormula.PlaceholderFormula);
            MDeduction D = T.CreateDeduction();

            DocumentStructure TStruct;
            if (AfterStructure == null)
                TStruct = DocumentStructure.Embed(T, Structure.GetByElement(C));
            else
                TStruct = DocumentStructure.Insert(T, AfterStructure);

            DocumentStructure.Embed(S, TStruct, true);
            DocumentStructure.Embed(D, TStruct, true);

            return T;
        }

        private MDefinition CreateDefinition(DocumentStructure AfterStructure, int tp, int aCount = 0, int vCount = 0)
        {
            int newindex = -1;
            MContext C = null;

            if (AfterStructure == null) C = CreateContext();
            else
            {
                DocumentStructure prev = AfterStructure?.Previous<IElementWithFileID>();
                if (prev == null) { newindex = 0; C = AfterStructure.Parent.Element as MContext; }
                else
                {
                    if (prev.Element is MDefinition AD)
                    {
                        C = AD._X;
                        if (C.GetDefinition(C.Definitions.Count - 1) != AD)
                            newindex = C.Definitions.IndexOf(AD) + 1;
                    }
                    if (prev.Element is MStatement AA)
                    {
                        C = AfterStructure.Parent.SplitContext(AfterStructure).Element as MContext;
                        AfterStructure = null;
                    }
                    if (prev.Element is MTheorem AT)
                    {
                        C = AfterStructure.Parent.SplitContext(AfterStructure).Element as MContext;
                        AfterStructure = null;
                    }
                    if (C == null) return null;
                }
            }

            MDefinition D = null;

            switch (tp)
            {
                case 0:
                    D = C.CreateBinaryConnective(index: newindex);
                    break;
                case 1:
                    D = C.CreateFunctionSymbol(aCount, vCount, index: newindex);
                    break;
                case 2:
                    D = C.CreateClass(vCount, index: newindex); 
                    break;
                case 3:
                    D = C.CreatePredicate(aCount, index: newindex);
                    break;
                case 4:
                    D = C.CreateQuantifier(vCount, index: newindex);
                    break;

                default:
                    throw new ArgumentException("Unknown Definition Type");
            }

            MVisualisationScheme V = D.DefaultVisualization; //TODO: Check if this makes sense

            DocumentStructure DStruct;
            if (AfterStructure == null)
                DStruct = DocumentStructure.Embed(D, Structure.GetByElement(C));
            else
                DStruct = DocumentStructure.Insert(D, AfterStructure);

            DocumentStructure.Embed(V, DStruct, true);
            DocumentStructure.Embed(new MStatementList() { Definition = D }, DStruct, true);

            return D;
        }

        public MStatement CreateAxiom(DocumentStructure AfterStructure)
        {
            int newindex = -1;
            MContext C = null;

            if (AfterStructure == null) C = CreateContext();
            else
            {
                DocumentStructure prev = AfterStructure?.Previous<IElementWithFileID>();
                if (prev == null) { newindex = 0; C = AfterStructure.Parent.Element as MContext; }
                else
                {
                    if (prev.Element is MDefinition AD)
                    {
                        C = AD._X;
                        if (C.GetDefinition(C.Definitions.Count - 1) != AD)
                        {
                            C = AfterStructure.Parent.SplitContext(AfterStructure).Element as MContext;
                            AfterStructure = null;
                        }
                    }
                    if (prev.Element is MStatement AA)
                    {
                        C = AA._X;
                        if (C.GetAxiom(C.Axioms.Count - 1) != AA)
                            newindex = C.Axioms.IndexOf(AA) + 1;
                    }
                    if (prev.Element is MTheorem AT)
                    {
                        C = AfterStructure.Parent.SplitContext(AfterStructure).Element as MContext;
                        AfterStructure = null;
                    }
                    if (C == null) return null;
                }
            }

            MStatement S = C.CreateAxiom(MFormula.PlaceholderFormula, index: newindex);

            if (AfterStructure == null)
                DocumentStructure.Embed(S, Structure.GetByElement(C));
            else
                DocumentStructure.Insert(S, AfterStructure);

            return S;
        }*/


        public MTheorem CreateTheorem(DocumentStructure AfterStructure)
        {
            int newindex = -1;
            MContext C;
            if (Contexts.Count == 0) C = CreateContext();
            else C = Contexts[0];
            if (AfterStructure != null)
            {
                DocumentStructure prev = AfterStructure?.Previous<IElementWithFileID>();
                if (prev == null)
                {
                    newindex = 0;
                }
                else
                {
                    if (prev.Element is MTheorem AT)
                    {
                        newindex = C.Theorems.IndexOf(AT) + 1;
                    }
                }
            }

            MTheorem T = C.CreateTheorem(newindex);
            MStatement S = T.CreateStatement(MFormula.PlaceholderFormula);
            MDeduction D = T.CreateDeduction();

            DocumentStructure TStruct;
            if (AfterStructure == null)
                TStruct = DocumentStructure.Embed(T, Structure.GetByElement(C));
            else
                TStruct = DocumentStructure.Insert(T, AfterStructure);

            DocumentStructure.Embed(S, TStruct, true);
            DocumentStructure.Embed(D, TStruct, true);

            return T;
        }

        private MDefinition CreateDefinition(DocumentStructure AfterStructure, int tp, int aCount = 0, int vCount = 0, int fCount = 0)
        {
            int newindex = -1;
            MContext C;
            if (Contexts.Count == 0) C = CreateContext();
            else C = Contexts[0];

            if (AfterStructure != null)
            {
                DocumentStructure prev = AfterStructure?.Previous<IElementWithFileID>();
                if (prev == null)
                {
                    newindex = 0;
                }
                else
                {
                    if (prev.Element is MDefinition AD)
                    {
                        C = AD._X;
                        if (C.GetDefinition(C.Definitions.Count - 1) != AD)
                            newindex = C.Definitions.IndexOf(AD) + 1;
                    }
                }
            }

            MDefinition D = null;

            switch (tp)
            {
                case 0:
                    D = C.CreateBinaryConnective(index: newindex);
                    break;
                case 1:
                    D = C.CreateFunctionSymbol(aCount, vCount, fCount, index: newindex);
                    break;
                case 2: //OBSOLETE
                    //D = C.CreateClass(vCount, index: newindex);
                    break;
                case 3: //OBSOLETE
                    //D = C.CreatePredicate(aCount, index: newindex);
                    break;
                case 4:
                    D = C.CreateQuantifier(aCount, vCount, fCount, index: newindex);
                    break;

                default:
                    throw new ArgumentException("Unknown Definition Type");
            }

            MVisualisationScheme V = D.DefaultVisualization; //TODO: Check if this makes sense

            DocumentStructure DStruct;
            if (AfterStructure == null)
                DStruct = DocumentStructure.Embed(D, Structure.GetByElement(C));
            else
                DStruct = DocumentStructure.Insert(D, AfterStructure);

            DocumentStructure.Embed(V, DStruct, true);
            DocumentStructure.Embed(new MStatementList() { Definition = D }, DStruct, true);

            return D;
        }

        public MStatement CreateAxiom(DocumentStructure AfterStructure)
        {
            int newindex = -1;
            MContext C;
            if (Contexts.Count == 0) C = CreateContext();
            else C = Contexts[0];

            if (AfterStructure != null)
            {
                DocumentStructure prev = AfterStructure?.Previous<IElementWithFileID>();
                if (prev == null)
                {
                    newindex = 0;
                }
                else
                {
                    if (prev.Element is MStatement AA)
                    {
                        if (C.GetAxiom(C.Axioms.Count - 1) != AA)
                            newindex = C.Axioms.IndexOf(AA) + 1;
                    }
                }
            }

            MStatement S = C.CreateAxiom(MFormula.PlaceholderFormula, index: newindex);

            if (AfterStructure == null)
                DocumentStructure.Embed(S, Structure.GetByElement(C));
            else
                DocumentStructure.Insert(S, AfterStructure);

            return S;
        }

        public MBinaryConnective CreateBinaryConnective(DocumentStructure AfterStructure)
        {
            MBinaryConnective BC = CreateDefinition(AfterStructure, 0) as MBinaryConnective;

            return BC;
        }
        public MFunctionSymbol CreateFunction(DocumentStructure AfterStructure, int aCount, int vCount, int fCount)
        {
            MFunctionSymbol F = CreateDefinition(AfterStructure, 1, aCount:aCount, vCount:vCount, fCount:fCount) as MFunctionSymbol;

            return F;
        }
        public MQuantifier CreateQuantifier(DocumentStructure AfterStructure, int tCount, int vCount, int fCount)
        {
            MQuantifier Q = CreateDefinition(AfterStructure, 4, aCount: tCount, vCount: vCount, fCount: fCount) as MQuantifier;

            return Q;
        }

        private MDeductionStep CreateDeductionStep(DocumentStructure AfterStructure, int tp, MStatement Premise)
        {
            int newindex = -1;
            MDeduction D = AfterStructure.Parent.Element as MDeduction ?? AfterStructure.Element as MDeduction;
            MDeductionStep DS;

            if (AfterStructure.Element is MDeduction) newindex = 0;
            else
            {
                DocumentStructure prev = AfterStructure.Previous<MDeductionStep>();
                if (prev == null) newindex = 0;
                else newindex = D.Steps.IndexOf(prev.Element as MDeductionStep) + 1;
            }

            switch (tp)
            {
                case 0:
                    DS = D.CreatePredicateSpecificatingStep(Premise, index: newindex);
                    break;
                case 1:
                    DS = D.CreateTrivialisingStep(Premise, index: newindex);
                    break;
                case 2:
                    DS = D.CreateSubstitutingStep(Premise, index: newindex);
                    break;
                case 3:
                    DS = D.CreateTermSubstitutingStep(Premise, index: newindex);
                    break;
                case 4:
                    DS = D.CreateFormulaSubstitutingStep(Premise, index: newindex);
                    break;
                case 5:
                    DS = D.CreateUniversallyGeneralisingStep(Premise, index: newindex);
                    break;
                case 6:
                    DS = D.CreateUniversallyInstantiatingStep(Premise, index: newindex);
                    break;
                case 7:
                    DS = D.CreateExistentiallyGeneralisingStep(Premise, index: newindex);
                    break;
                case 8:
                    DS = D.CreateExistentiallyInstantiatingStep(Premise, index: newindex);
                    break;
                case 9:
                    DS = D.CreateRAAStep(Premise, index: newindex);
                    break;
                case 10:
                    DS = D.CreateAssumptionStep(Premise, index: newindex);
                    break;
                case 11:
                    DS = D.CreateConditionInstantiatingStep(Premise, index: newindex);
                    break;

                default:
                    throw new ArgumentException("Unknown DeductionStep Type");
            }


            DocumentStructure DSStruct;
            if (AfterStructure.Element is MDeduction)
                DSStruct = DocumentStructure.Embed(DS, Structure.GetByElement(D));
            else
                DSStruct = DocumentStructure.Insert(DS, AfterStructure);

            return DS;
        }

        public MPredicateSpecificationDeductionStep CreatePredicateSpecification(DocumentStructure AfterStructure, MStatement Premise)
        { return CreateDeductionStep(AfterStructure, 0, Premise) as MPredicateSpecificationDeductionStep; }
        public MTrivialisationDeductionStep CreateTrivialization(DocumentStructure AfterStructure, MStatement Premise)
        { return CreateDeductionStep(AfterStructure, 1, Premise) as MTrivialisationDeductionStep; }
        public MVariableSubstitutionDeductionStep CreateVariableSubstitution(DocumentStructure AfterStructure, MStatement Premise)
        { return CreateDeductionStep(AfterStructure, 2, Premise) as MVariableSubstitutionDeductionStep; }
        public MTermSubstitutionDeductionStep CreateTermSubstitution(DocumentStructure AfterStructure, MStatement Premise)
        { return CreateDeductionStep(AfterStructure, 3, Premise) as MTermSubstitutionDeductionStep; }
        public MFormulaSubstitutionDeductionStep CreateFormulaSubstitution(DocumentStructure AfterStructure, MStatement Premise)
        { return CreateDeductionStep(AfterStructure, 4, Premise) as MFormulaSubstitutionDeductionStep; }
        public MUniversalGeneralisationDeductionStep CreateUniversalGeneralization(DocumentStructure AfterStructure, MStatement Premise)
        { return CreateDeductionStep(AfterStructure, 5, Premise) as MUniversalGeneralisationDeductionStep; }
        public MUniversalInstantiationDeductionStep CreateUniversalInstantiation(DocumentStructure AfterStructure, MStatement Premise)
        { return CreateDeductionStep(AfterStructure, 6, Premise) as MUniversalInstantiationDeductionStep; }
        public MExistentialGeneralisationDeductionStep CreateExistentialGeneralization(DocumentStructure AfterStructure, MStatement Premise)
        { return CreateDeductionStep(AfterStructure, 7, Premise) as MExistentialGeneralisationDeductionStep; }
        public MExistentialInstantiationDeductionStep CreateExistentialInstantiation(DocumentStructure AfterStructure, MStatement Premise)
        { return CreateDeductionStep(AfterStructure, 8, Premise) as MExistentialInstantiationDeductionStep; }
        public MRAADeductionStep CreateReductioAdAbsurdum(DocumentStructure AfterStructure, MStatement Premise)
        { return CreateDeductionStep(AfterStructure, 9, Premise) as MRAADeductionStep; }
        public MAssumptionDeductionStep CreateAssumption(DocumentStructure AfterStructure, MStatement Premise)
        { return CreateDeductionStep(AfterStructure, 10, Premise) as MAssumptionDeductionStep; }
        public MConditionInstantiationDeductionStep CreateConditionInstantiation(DocumentStructure AfterStructure, MStatement Premise)
        { return CreateDeductionStep(AfterStructure, 11, Premise) as MConditionInstantiationDeductionStep; }

        public void ReferenceDocument(MDocument D)
        {
            if (!Loaded) return; //throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");

            if (this == D) return;
            if (ReferencedDocuments.Contains(D)) return;
            ReferencedDocuments.Add(D);
        }

        internal int IndexOfReferencedDocument(MDocument D)
        {
            if (ReferencedDocuments.Contains(D)) return ReferencedDocuments.IndexOf(D);
            if (Loaded)
            {
                ReferenceDocument(D);
                return ReferencedDocuments.Count - 1;
            }
            else return 0;
        }

        public MContext GetContext(int index)
        {
            if (Contexts[index] == null) LoadContext(index);

            return Contexts[index];
        }
        public MContext GetContext(ContextFileID ID)
        {
            //Look for loaded context
            foreach(MContext X in Contexts)
                if(X != null && X.fileID.IsSubFileID(ID)) return X;

            if (Loaded) return null;
            //Look for unloaded contexts
            return LoadContext(ID);
        }

        public MDocument GetReferencedDocument(int index)
        {
            if (ReferencedDocuments[index] == null) LoadReferencedDocument(index);

            return ReferencedDocuments[index];
        }

        public override string ToString()
        {
            string ret = "DOCUMENT\n--------\n";
            foreach (MContext X in Contexts)
                ret = ret + X.ToString();
            ret = ret + "END OF DOCUMENT";
            return ret;
        }

        public string GetLaTeX()
        {
            return Structure.GetLaTeX();
        }
    }

    public interface IDocumentElement
    {
        string LaTeXStart { get; }
        string LaTeXEnd { get; }
    }

    public interface IElementWithFileID : IDocumentElement
    {
        FileID baseFileID { get; }
    }
}
