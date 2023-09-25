using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace TraceBackend
{
    public partial class MContext : MObject, IElementWithFileID
    {
        public bool loaded = false;
        public static MContext Global;

        public MDocument Document;

        public List<MContext> SuperContexts;
        public List<MStatement> Axioms;
        public List<MDefinition> Definitions;
        public List<MTheorem> Theorems;
        public List<MVariable> Variables;

        public ContextFileID fileID;
        public FileID baseFileID => fileID;

        public string LaTeXStart => @"";
        public string LaTeXEnd => @"";

        //static MContext()
        //{ Global = new MContext(); }

        private MContext():base()
        {
            
        }

        internal MContext(ContextFileID ID) : this()
        {
            fileID = ID;
            Document = fileID.Document;
        }

        internal MContext(FileIDManager IDM, MDocument D) : this()
        {
            fileID = IDM.RequestContextFileID(this);
            Document = fileID.Document;
        }

        public MVariable CreateVariable(string symbol = "x")
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");

            MVariable V = new MVariable(Document.IDManager, this, symbol);
            Variables.Add(V);
            return V;
        }

        internal List<MStatement> FindAxiomsForVariable(MVariable V, DocumentLoader DL = null)
        {
            if(V == MVariable.PlaceholderVariable) return new List<MStatement>(0);

            //inefficient
            if (DL == null)
            {
                if (!ContainsSuperContext(V.fileID.MakeContextFileID().FindContext())) return new List<MStatement>(0);
            }
            else if (!ContainsSuperContext(V.fileID.MakeContextFileID().FindContext(DL))) return new List<MStatement>(0);

            List<MStatement> ret = new List<MStatement>();
            if (!Variables.Contains(V))
                for (int i = 0; i < SuperContexts.Count; i++)
                {
                    MContext X = GetSuperContext(i);
                    ret.AddRange(X.FindAxiomsForVariable(V, DL));
                }

            ret.AddRange(Axioms.Where(S => S._F.ContainsVariable(V)));

            for (int i = 0; i < Definitions.Count; i++)
            {
                MDefinition D = GetDefinition(i);
                ret.AddRange(D.FindAxiomsForVariable(V));
            }

            return ret;
        }

        public bool AddSuperContext(MContext X)
        {
            if (X == this) return true;
            if (SuperContexts.Contains(X)) return true;
            if (X.ContainsSuperContext(this)) return false;
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");
            SuperContexts.Add(X);
            Document.ReferenceDocument(X.Document);
            return true;
        }
        internal void AddAxiom(MStatement S) {if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited."); Axioms.Add(S); }
        internal void AddVariable(MVariable V) {if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited."); Variables.Add(V); }
        internal void AddDefinition(MDefinition D) {if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited."); Definitions.Add(D); }
        internal void AddTheorem(MTheorem T) {if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited."); Theorems.Add(T); }

        public void RemoveAxiom(MStatement A) { if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited."); Axioms.Remove(A); A.valid.Invalidate(true); }
        public void RemoveVariable(MVariable V) { if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited."); Variables.Remove(V); }
        public void RemoveDefinition(MDefinition D) { if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited."); Definitions.Remove(D); }
        public void RemoveTheorem(MTheorem T) { if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited."); Theorems.Remove(T); }


        public MContext GetSuperContext(int index)
        {
            if (SuperContexts[index] == null) LoadSuperContext(index);

            return SuperContexts[index];
        }
        public MContext GetSuperContext(ContextFileID ID)
        {
            //Look for loaded SuperContexts
            foreach (MContext X in SuperContexts)
                if (X != null && X.fileID.IsSubFileID(ID)) return X;

            if (loaded) throw new Exception();
            //Look for unloaded SuperContexts
            return LoadSuperContext(ID);
        }
        public MStatement GetAxiom(int index)
        {
            if (Axioms[index] == null) LoadAxiom(index);

            return Axioms[index];
        }
        public MStatement GetAxiom(ContextStatementFileID ID)
        {
            //Look for loaded Axioms
            foreach (MStatement A in Axioms)
                if (A != null && A.fileID.IsSubFileID(ID)) return A;

            if (loaded) throw new Exception();
            //Look for unloaded contexts
            return LoadAxiom(ID);
        }
        public MVariable GetVariable(int index)
        {
            if (Variables[index] == null) LoadVariable(index);

            return Variables[index];
        }
        public MVariable GetVariable(VariableFileID ID)
        {
            //Look for loaded Variables
            foreach (MVariable V in Variables)
                if (V != null && V.fileID.IsSubFileID(ID)) return V;

            if (loaded) throw new Exception();
            //Look for unloaded contexts
            return LoadVariable(ID);
        }
        public MDefinition GetDefinition(int index)
        {
            if (Definitions[index] == null) LoadDefinition(index);

            return Definitions[index];
        }
        public MDefinition GetDefinition(DefinitionFileID ID)
        {
            //Look for loaded Definitions
            foreach (MDefinition D in Definitions)
                if (D != null && D.fileID.IsSubFileID(ID)) return D;

            if (loaded) throw new Exception();
            //Look for unloaded Definitions
            return LoadDefinition(ID);
        }
        public MTheorem GetTheorem(int index)
        {
            if (Theorems[index] == null) LoadTheorem(index);

            return Theorems[index];
        }
        public MTheorem GetTheorem(TheoremFileID ID)
        {
            //Look for loaded Theorems
            foreach (MTheorem T in Theorems)
                if (T != null && T.fileID.IsSubFileID(ID)) return T;

            if (loaded) throw new Exception();
            //Look for unloaded Theorems
            return LoadTheorem(ID);
        }

        public MStatement CreateAxiom(MFormula F, int index = -1)
        {if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");
            MStatement A = new MStatement(Document.IDManager, this, F, Validity.Axiom);
            if (index < 0)
                Axioms.Add(A);
            else
                Axioms.Insert(index, A);
            A.loaded = true;
            return A;
        }

        public MTheorem CreateTheorem(int index = -1)
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");
            MTheorem T = new MTheorem(Document.IDManager, this);
            if (index < 0)
                Theorems.Add(T);
            else
                Theorems.Insert(index, T);
            T.loaded = true;
            return T;
        }

        public MFunctionSymbol CreateFunctionSymbol(int aCount, int bvCount, int fCount, string symbol = "f", int index = -1)
        { if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");
            MFunctionSymbol F = new MFunctionSymbol(Document.IDManager, this, aCount, bvCount, fCount, symbol);
            if (index < 0)
                Definitions.Add(F);
            else
                Definitions.Insert(index, F);
            F.loaded = true;

            MVisualisationScheme V = F.CreateVisualisation();


            return F;
        }

        public MBinaryConnective CreateBinaryConnective(bool tt = false, bool tf = false, bool ft = false, bool ff = false, string symbol = "~", int index = -1)
        {if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");
            MBinaryConnective C = new MBinaryConnective(Document.IDManager, this, tt, tf, ft, ff, symbol);
            if (index < 0)
                Definitions.Add(C);
            else
                Definitions.Insert(index, C);

            C.loaded = true;

            MVisualisationScheme V = C.CreateVisualisation();
            return C;
        }

        public MQuantifier CreateQuantifier(int tCount, int bvCount, int fCount, string symbol = "ForAll", int index = -1)
        {if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");
            MQuantifier Q = new MQuantifier(Document.IDManager, this, tCount, bvCount, fCount, symbol);
            if (index < 0)
                Definitions.Add(Q);
            else
                Definitions.Insert(index, Q);

            Q.loaded = true;

            MVisualisationScheme V = Q.CreateVisualisation();
            
            return Q;
        }

        public bool ContainsSuperContext(MContext X)
        {
            // not optimal: top-level super context should be searched first, then deeper ones. Also, ContainsSuperContext may be called on the same Context many times.
            // TODO: Rework
            
            if (Identical(X)) return true;
            for(int i = 0; i< SuperContexts.Count; i++)
            {
                MContext S = GetSuperContext(i);
                if(!Identical(S))                   // TODO: Temporary, shouldnt be necessary
                    if (S.ContainsSuperContext(X)) return true;
            }

            return false;
        }

        public override string ToString()
        {
            string ret = "Context.\nAxioms:\n";

            for (int i = 0; i < Axioms.Count; i++)
                ret = ret + "Axiom " + (i + 1).ToString() + ": \n" + Helper.Indent(Axioms[i].ToString()) + "\n";
            ret = ret + "Definitions:\n";
            for (int i = 0; i < Definitions.Count; i++)
                ret = ret + "Definition " + (i + 1).ToString() + ": \n" + Helper.Indent(Definitions[i].ToString()) + "\n";

            ret = ret + "Theorems:\n";
            for (int i = 0; i < Theorems.Count; i++)
                ret = ret + "Theorem " + (i + 1).ToString() + ": \n" + Helper.Indent(Theorems[i].ToString()) + "\n";

            return ret;
        }

    }
}
