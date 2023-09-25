using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TraceBackend
{
    public partial class MTerm
    {
        public new static MTerm FromStream(DocumentLoader DL)
        {
            MTerm T;

            int visID = DL.ReadByte();
            BracketMode bMode = (BracketMode)DL.ReadByte();

            
            if (DL.PDList == null)
            {
                DL.PDList = new List<MUndefinedPredicate>();
                int pdCount = DL.ReadInt();
                for (int i = 0; i < pdCount; i++)
                    DL.PDList.Add(MUndefinedPredicate.FromStream(DL));
            }

            int type = DL.ReadInt();
            switch (type)
            {
                case 0: // 0 - Variable
                    T = MVariable.FromStream(DL);
                    break;
                case 1: // 1 - Constant (OBSOLETE)
                    //T = MConstant.DataFromStream(DL);
                    T = MVariable.FromStream(DL);
                    break;
                case 2: // 2 - Function
                    T = MFunction.FromStream(DL);
                    break;
                case 3: // 3 - ClassTerm (OBSOLETE, redirects to Function)
                    //T = MClassTerm.FromStream(DL);
                    T = MFunction.FromStream(DL);
                    break;
                case 4: // 4 - PlaceholderTerm
                    T = MPlaceholderTerm.FromStream(DL);
                    break;
                default:
                    throw new FileLoadException("Unknown Term Type.");
            }

            T.visID = visID;
            T.brMode = bMode;

            return T;
        }
        public new static MTerm FromStream(XMLDocumentLoader DL, XmlNode node)
        {
            if (node.LocalName != "expression") node = node.FirstChild;

            return FromStream(MExpressionReader.Create(DL, node.InnerText));
        }
        public new static MTerm FromStream(MExpressionReader ER)
        {
            MTerm T;

            if (ER.PDList == null)
            {
                ER.PDList = new List<MUndefinedPredicate>();
                int pdCount = ER.NextUShort();
                for (int i = 0; i < pdCount; i++)
                    ER.PDList.Add(MUndefinedPredicate.FromStream(ER));
            }
            int visID = ER.NextByte();
            BracketMode bMode = (BracketMode)ER.NextByte();



            int type = ER.NextByte();
            switch (type)
            {
                case 0: // 0 - Variable
                    T = MVariable.FromStream(ER);
                    break;
                case 1: // 1 - Constant (OBSOLETE)
                    //T = MConstant.DataFromStream(DL);
                    T = MVariable.FromStream(ER);
                    break;
                case 2: // 2 - Function
                    T = MFunction.FromStream(ER);
                    break;
                case 3: // 3 - ClassTerm (OBSOLETE, redirects to function)
                    //T = MClassTerm.FromStream(ER);
                    T = MFunction.FromStream(ER);
                    break;
                case 4: // 4 - PlaceholderTerm
                    T = MPlaceholderTerm.FromStream(ER);
                    break;
                default:
                    throw new FileLoadException("Unknown Term Type.");
            }

            T.visID = visID;
            T.brMode = bMode;

            return T;
        }
    }

    public partial class MVariable
    {
        FileID[] AxiomIDs;

        public void DataToStream(DocumentLoader DL)
        {
            DL.BeginSubStream();
            fileID.ToStream(DL);

            DL.Write(stringSymbol);
            DL.Write(Axioms.Count);
            foreach (MStatement A in Axioms)
                A.fileID.ToStream(DL);

            DL.EndSubStream();
        }
        public void DataToXML(XMLDocumentLoader DL)
        {
            DL.Xwr.WriteStartElement("variable");
            DL.Xwr.WriteAttributeString("id", fileID.ToXML(DL.Document));

            DL.Xwr.WriteElementString("symbol", stringSymbol);
            
            foreach (MStatement A in Axioms)
                DL.Xwr.WriteElementString("axiom", A.fileID.ToXML(fileID.Document));
            
            DL.Xwr.WriteEndElement();
        }

        public static MVariable DataFromStream(DocumentLoader DL)
        {
            DL.Remember();
            VariableFileID fileID = FileID.FromStream(DL) as VariableFileID;
            MVariable V = new MVariable(fileID, DL.Context, DL.ReadString());
            V.Axioms = new List<MStatement>();
            int aCount = DL.ReadInt();
            V.AxiomIDs = new FileID[aCount];
            for (int i = 0; i < aCount; i++)
            {
                V.AxiomIDs[i] = FileID.FromStream(DL);
                V.Axioms.Add(null);
            }
            return V;
        }
        public static MVariable DataFromStream(XMLDocumentLoader DL, XmlNode node)
        {
            VariableFileID fileID = FileID.FromStream(DL, node.GetAttr("id")) as VariableFileID;
            MVariable V = new MVariable(fileID, DL.Context, node.GetChildNode("symbol").InnerText);
            V.Axioms = new List<MStatement>();
            int aCount = node.ChildNodes.Count-1;
            V.AxiomIDs = new FileID[aCount];
            for (int i = 0; i < aCount; i++)
            {
                V.AxiomIDs[i] = FileID.FromStream(DL, node.ChildNodes[i+1].InnerText);
                V.Axioms.Add(null);
            }
            return V;
        }
        public void FetchAxioms(ProtoDocumentLoader DL)
        {
            for (int i = 0; i < AxiomIDs.Length; i++)
            {
                Axioms[i] = AxiomIDs[i].FindStatement(DL);
            }
        }
        public void FetchProperAxioms(ProtoDocumentLoader DL)
        {
            for (int i = 0; i < AxiomIDs.Length; i++)
            {
                if (!(AxiomIDs[i] is TheoremStatementFileID))
                    Axioms[i] = AxiomIDs[i].FindStatement(DL);
            }
        }
        public void FetchTheoremAxioms(ProtoDocumentLoader DL)
        {
            for (int i = 0; i < AxiomIDs.Length; i++)
            {
                if (AxiomIDs[i] is TheoremStatementFileID)
                    Axioms[i] = AxiomIDs[i].FindStatement(DL);
            }
        }

        public override void ToStream(DocumentLoader DL)
        {
            base.ToStream(DL);

            DL.Write(0);

            fileID.ToStream(DL);
        }

        internal override void ToXML(MExpressionWriter EW)
        {
            base.ToXML(EW);

            EW.Write((byte)0);

            fileID.ToXML(EW);
        }

        public new static MVariable FromStream(DocumentLoader DL)
        {
            VariableFileID fileID = FileID.FromStream(DL) as VariableFileID;
            return fileID.FindVariable(DL);
        }
        public new static MVariable FromStream(MExpressionReader ER)
        {
            VariableFileID fileID = FileID.FromStream(ER) as VariableFileID;
            return fileID.FindVariable(ER.DL);
        }
    }

    public partial class MBoundVariable
    {
        public override void ToStream(DocumentLoader DL)
        {
            _FreeInstance.ToStream(DL);
        }

        internal override void ToXML(MExpressionWriter EW)
        {
            _FreeInstance.ToXML(EW);
        }
    }

    public partial class MFunction
    {
        public override void ToStream(DocumentLoader DL)
        {
            base.ToStream(DL);

            DL.Write(2);

            _D.fileID.ToStream(DL);
            foreach (MTerm a in _T)
                a.ToStream(DL);
            foreach (MBoundVariable v in _BV)
                v.ToStream(DL);
            foreach (MFormula f in _F)
                f.ToStream(DL);
        }

        internal override void ToXML(MExpressionWriter EW)
        {
            base.ToXML(EW);

            EW.Write((byte)2);

            _D.fileID.ToXML(EW);
            foreach (MTerm a in _T)
                a.ToXML(EW);
            foreach (MBoundVariable v in _BV)
                v.ToXML(EW);
            foreach (MFormula f in _F)
                f.ToXML(EW);
        }

        public new static MFunction FromStream(DocumentLoader DL)
        {
            MFunctionSymbol FS = (FileID.FromStream(DL) as DefinitionFileID).FindDefinition(DL) as MFunctionSymbol;
            int tCount = FS.argumentCount;
            MTerm[] T = new MTerm[tCount];
            for (int i = 0; i < tCount; i++)
                T[i] = MTerm.FromStream(DL);

            int bvCount = FS.boundVarCount;
            MVariable[] BV = new MVariable[bvCount];
            for (int i = 0; i < bvCount; i++)
                BV[i] = MTerm.FromStream(DL) as MVariable;

            int fCount = FS.formulaCount;
            MFormula[] F = new MFormula[fCount];
            for (int i = 0; i < fCount; i++)
                F[i] = MFormula.FromStream(DL);

            return new MFunction(FS, T, BV, F);
        }
        public new static MFunction FromStream(MExpressionReader ER)
        {
            MFunctionSymbol FS = (FileID.FromStream(ER) as DefinitionFileID).FindDefinition(ER.DL) as MFunctionSymbol;
            int tCount = FS.argumentCount;
            MTerm[] T = new MTerm[tCount];
            for (int i = 0; i < tCount; i++)
                T[i] = MTerm.FromStream(ER);

            int bvCount = FS.boundVarCount;
            MVariable[] BV = new MVariable[bvCount];
            for (int i = 0; i < bvCount; i++)
                BV[i] = MTerm.FromStream(ER) as MVariable;

            int fCount = FS.formulaCount;
            MFormula[] F = new MFormula[fCount];
            for (int i = 0; i < fCount; i++)
                F[i] = MFormula.FromStream(ER);

            return new MFunction(FS, T, BV, F);
        }
    }
    
    public partial class MPlaceholderTerm
    {
        public override void ToStream(DocumentLoader DL)
        {
            base.ToStream(DL);

            DL.Write(4);
        }

        internal override void ToXML(MExpressionWriter EW)
        {
            base.ToXML(EW);

            EW.Write((byte)4);
        }

        public new static MPlaceholderTerm FromStream(DocumentLoader DL)
        {
            return PlaceholderTerm;
        }
        public new static MPlaceholderTerm FromStream(MExpressionReader ER)
        {
            return PlaceholderTerm;
        }
    }
}
