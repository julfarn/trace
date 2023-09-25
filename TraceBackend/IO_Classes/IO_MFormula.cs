using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TraceBackend
{
    public partial class MFormula
    {
        public override void ToStream(DocumentLoader DL)
        {
            base.ToStream(DL);

        }

        internal override void ToXML(MExpressionWriter EW)
        {
            base.ToXML(EW);
        }

        public new static MFormula FromStream(DocumentLoader DL)
        {
            int visID = DL.ReadByte();
            BracketMode bMode = (BracketMode)DL.ReadByte();

            if (DL.PDList == null)
            {
                DL.PDList = new List<MUndefinedPredicate>();
                int pdCount = DL.ReadInt();
                for (int i = 0; i < pdCount; i++)
                    DL.PDList.Add(MUndefinedPredicate.FromStream(DL));
            }

            MFormula F;
            int type = DL.ReadInt();

            switch (type)
            {
                case 0: // 0 - UndefinedFormula (OBSOLETE)
                    throw new FileLoadException("Obsolete Formula Type.");
                case 1: // 1 - TrivialFormula
                    F = MTrivialFormula.FromStream(DL);
                    break;
                case 2: // 2 - PredicateFormula (OBSOLETE)
                    //F = MPredicateFormula.FromStream(DL);
                    F = MQuantifierFormula.FromStream(DL);
                    break;
                case 3: // 3 - EqualityFormula
                    F = MEqualityFormula.FromStream(DL);
                    break;
                case 4: // 4 - NegationFormula
                    F = MNegationFormula.FromStream(DL);
                    break;
                case 5: // 5 - BinaryConnectiveFormula
                    F = MBinaryConnectiveFormula.FromStream(DL);
                    break;
                case 6: // 6 - QuantifierFormula
                    F = MQuantifierFormula.FromStream(DL);
                    break;
                case 7: // 7 - UndefinedPredicateFormula
                    F = MUndefinedPredicateFormula.FromStream(DL);
                    break;
                case 8: // 8 - PlaceholderFormula
                    F = MPlaceholderFormula.FromStream(DL);
                    break;
                default:
                    throw new FileLoadException("Unkown Formula Type.");
            }

            F.visID = visID;
            F.brMode = bMode;

            return F;
        }
        public new static MFormula FromStream(XMLDocumentLoader DL, XmlNode node)
        {
            if (node.LocalName != "expression") node = node.FirstChild;

            return FromStream(MExpressionReader.Create(DL, node.InnerText));
        }
        public new static MFormula FromStream(MExpressionReader ER)
        {
            if (ER.PDList == null)
            {
                ER.PDList = new List<MUndefinedPredicate>();
                int pdCount = ER.NextUShort();
                for (int i = 0; i < pdCount; i++)
                    ER.PDList.Add(MUndefinedPredicate.FromStream(ER));
            }

            int visID = ER.NextByte();
            BracketMode bMode = (BracketMode)ER.NextByte();

            MFormula F;
            int type = ER.NextByte();

            switch (type)
            {
                case 0: // 0 - UndefinedFormula (OBSOLETE)
                   throw new FileLoadException("Obsolete Formula Type.");
                case 1: // 1 - TrivialFormula
                    F = MTrivialFormula.FromStream(ER);
                    break;
                case 2: // 2 - PredicateFormula (OBSOLETE)
                    //F = MPredicateFormula.FromStream(ER);
                    F = MQuantifierFormula.FromStream(ER);
                    break;
                case 3: // 3 - EqualityFormula
                    F = MEqualityFormula.FromStream(ER);
                    break;
                case 4: // 4 - NegationFormula
                    F = MNegationFormula.FromStream(ER);
                    break;
                case 5: // 5 - BinaryConnectiveFormula
                    F = MBinaryConnectiveFormula.FromStream(ER);
                    break;
                case 6: // 6 - QuantifierFormula
                    F = MQuantifierFormula.FromStream(ER);
                    break;
                case 7: // 7 - UndefinedPredicateFormula
                    F = MUndefinedPredicateFormula.FromStream(ER);
                    break;
                case 8: // 8 - PlaceholderFormula
                    F = MPlaceholderFormula.FromStream(ER);
                    break;
                default:
                    throw new FileLoadException("Unkown Formula Type.");
            }

            F.visID = visID;
            F.brMode = bMode;

            return F;
        }
    }

    public partial class MUndefinedPredicate
    {
        public override void ToStream(DocumentLoader DL)
        {
            DL.Write(stringSymbol);
            DL.Write(argumentCount);
        }

        public void ToXML(MExpressionWriter EW)
        {
            EW.Write(stringSymbol);
            EW.Write((byte)argumentCount);
        }

        public new static MUndefinedPredicate FromStream(DocumentLoader DL)
        {
            string symbol = DL.ReadString();
            int argCount = DL.ReadInt();
            return new MUndefinedPredicate(argCount, symbol);
        }

        public new static MUndefinedPredicate FromStream(MExpressionReader ER)
        {
            string symbol = ER.NextString();
            int argCount = ER.NextByte();
            return new MUndefinedPredicate(argCount, symbol);
        }
    }

    public partial class MUndefinedPredicateFormula
    {
        public override void ToStream(DocumentLoader DL)
        {
            base.ToStream(DL);

            DL.Write(7);

            DL.Write(DL.PDList.IndexOf(PseudoDefinition));

            for (int i = 0; i < _T.Length; i++)
            {
                _T[i].ToStream(DL);
            }
        }

        internal override void ToXML(MExpressionWriter EW)
        {
            base.ToXML(EW);

            EW.Write((byte)7);

            EW.Write((ushort)EW.PDList.IndexOf(PseudoDefinition));

            for (int i = 0; i < _T.Length; i++)
            {
                _T[i].ToXML(EW);
            }
        }

        public new static MUndefinedPredicateFormula FromStream(DocumentLoader DL)
        {
            int pdIndex = DL.ReadInt();
            MTerm[] T = new MTerm[DL.PDList[pdIndex].argumentCount];
            for (int i = 0; i < DL.PDList[pdIndex].argumentCount; i++)
            {
                T[i] = MTerm.FromStream(DL);
            }
            return new MUndefinedPredicateFormula(DL.PDList[pdIndex], T);
        }
        public new static MUndefinedPredicateFormula FromStream(MExpressionReader ER)
        {
            int pdIndex = ER.NextUShort();
            MTerm[] T = new MTerm[ER.PDList[pdIndex].argumentCount];
            for (int i = 0; i < ER.PDList[pdIndex].argumentCount; i++)
            {
                T[i] = MTerm.FromStream(ER);
            }
            return new MUndefinedPredicateFormula(ER.PDList[pdIndex], T);
        }
    }

    public partial class MTrivialFormula
    {
        public override void ToStream(DocumentLoader DL)
        {
            base.ToStream(DL);

            DL.Write(1);

            DL.Write(_V);
        }

        internal override void ToXML(MExpressionWriter EW)
        {
            base.ToXML(EW);

            EW.Write((byte)1);

            EW.Write(_V);
        }

        public new static MTrivialFormula FromStream(DocumentLoader DL)
        {
            if (DL.ReadBool() == true) return _True; return _False;
        }
        public new static MTrivialFormula FromStream(MExpressionReader ER)
        {
            if (ER.NextBool()) return _True; return _False;
        }
    }

    public partial class MPlaceholderFormula
    {
        public override void ToStream(DocumentLoader DL)
        {
            base.ToStream(DL);

            DL.Write(8);
        }

        internal override void ToXML(MExpressionWriter EW)
        {
            base.ToXML(EW);

            EW.Write((byte)8);
        }

        public new static MPlaceholderFormula FromStream(DocumentLoader DL)
        {
            return PlaceholderFormula;
        }
        public new static MPlaceholderFormula FromStream(MExpressionReader ER)
        {
            return PlaceholderFormula;
        }
    }
    
    public partial class MEqualityFormula
    {
        public override void ToStream(DocumentLoader DL)
        {
            base.ToStream(DL);
            DL.Write(3);

            _T[0].ToStream(DL);
            _T[1].ToStream(DL);
        }

        internal override void ToXML(MExpressionWriter EW)
        {
            base.ToXML(EW);
            EW.Write((byte)3);

            _T[0].ToXML(EW);
            _T[1].ToXML(EW);
        }

        public new static MEqualityFormula FromStream(DocumentLoader DL)
        {
            return new MEqualityFormula(MTerm.FromStream(DL), MTerm.FromStream(DL));
        }
        public new static MEqualityFormula FromStream(MExpressionReader ER)
        {
            return new MEqualityFormula(MTerm.FromStream(ER), MTerm.FromStream(ER));
        }
    }

    public partial class MNegationFormula
    {
        public override void ToStream(DocumentLoader DL)
        {
            base.ToStream(DL);

            DL.Write(4);

            _F[0].ToStream(DL);
        }

        internal override void ToXML(MExpressionWriter EW)
        {
            base.ToXML(EW);

            EW.Write((byte)4);

            _F[0].ToXML(EW);
        }

        public new static MNegationFormula FromStream(DocumentLoader DL)
        {
            return new MNegationFormula(MFormula.FromStream(DL));
        }
        public new static MNegationFormula FromStream(MExpressionReader ER)
        {
            return new MNegationFormula(MFormula.FromStream(ER));
        }
    }

    public partial class MBinaryConnectiveFormula
    {
        public override void ToStream(DocumentLoader DL)
        {
            base.ToStream(DL);

            DL.Write(5);

            _D.fileID.ToStream(DL);
            _F[0].ToStream(DL);
            _F[1].ToStream(DL);
        }

        internal override void ToXML(MExpressionWriter EW)
        {
            base.ToXML(EW);

            EW.Write((byte)5);

            _D.fileID.ToXML(EW);
            _F[0].ToXML(EW);
            _F[1].ToXML(EW);
        }

        public new static MBinaryConnectiveFormula FromStream(DocumentLoader DL)
        {
            MBinaryConnective C = (FileID.FromStream(DL) as DefinitionFileID).FindDefinition(DL) as MBinaryConnective;
            return new MBinaryConnectiveFormula(C, MFormula.FromStream(DL), MFormula.FromStream(DL));
        }
        public new static MBinaryConnectiveFormula FromStream(MExpressionReader ER)
        {
            MBinaryConnective C = (FileID.FromStream(ER) as DefinitionFileID).FindDefinition(ER.DL) as MBinaryConnective;
            return new MBinaryConnectiveFormula(C, MFormula.FromStream(ER), MFormula.FromStream(ER));
        }
    }

    public partial class MQuantifierFormula
    {
        public override void ToStream(DocumentLoader DL)
        {
            base.ToStream(DL);

            DL.Write(6);

            _D.fileID.ToStream(DL);
            foreach(MTerm t in _T)
                t.ToStream(DL);
            foreach (MBoundVariable v in _BV)
                v.ToStream(DL);
            foreach (MFormula f in _F)
                f.ToStream(DL);
        }

        internal override void ToXML(MExpressionWriter EW)
        {
            base.ToXML(EW);

            EW.Write((byte)6);

            _D.fileID.ToXML(EW);
            foreach (MTerm f in _T)
                f.ToXML(EW);
            foreach (MBoundVariable v in _BV)
                v.ToXML(EW);
            foreach (MFormula f in _F)
                f.ToXML(EW);
        }

        public new static MQuantifierFormula FromStream(DocumentLoader DL)
        {
            MQuantifier Q = (FileID.FromStream(DL) as DefinitionFileID).FindDefinition(DL) as MQuantifier;
            MTerm[] T = new MTerm[Q.termCount];
            for (int i = 0; i < Q.termCount; i++)
                T[i] = MTerm.FromStream(DL);
            MVariable[] bvs = new MVariable[Q.boundVarCount];
            for (int i = 0; i < Q.boundVarCount; i++)
                bvs[i] = MTerm.FromStream(DL) as MVariable;
            MFormula[] F = new MFormula[Q.formulaCount];
            for (int i = 0; i < Q.formulaCount; i++)
                F[i] = MFormula.FromStream(DL);

            return new MQuantifierFormula(Q, T, bvs, F);
        }
        public new static MQuantifierFormula FromStream(MExpressionReader ER)
        {
            MQuantifier Q = (FileID.FromStream(ER) as DefinitionFileID).FindDefinition(ER.DL) as MQuantifier;
            MTerm[] T = new MTerm[Q.termCount];
            for (int i = 0; i < Q.termCount; i++)
                T[i] = MTerm.FromStream(ER);
            MVariable[] bvs = new MVariable[Q.boundVarCount];
            for (int i = 0; i < Q.boundVarCount; i++)
                bvs[i] = MTerm.FromStream(ER) as MVariable;
            MFormula[] F = new MFormula[Q.formulaCount];
            for (int i = 0; i < Q.formulaCount; i++)
                F[i] = MFormula.FromStream(ER);

            return new MQuantifierFormula(Q, T, bvs, F);
        }
    }
}
