using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;

namespace TraceBackend
{
    public partial class FileIDManager
    {
        public MDocument Document;
        uint highestContextFileID;
        public uint NewContextID { get { highestContextFileID++; return highestContextFileID; } }
        uint highestTheoremFileID;
        public uint NewTheoremID { get { highestTheoremFileID++; return highestTheoremFileID; } }
        uint highestDefinitionFileID;
        public uint NewDefinitionID { get { highestDefinitionFileID++; return highestDefinitionFileID; } }
        uint highestDeductionFileID;
        public uint NewDeductionID { get { highestDeductionFileID++; return highestDeductionFileID; } }
        uint highestDeductionStepFileID;
        public uint NewDeductionStepID { get { highestDeductionStepFileID++; return highestDeductionStepFileID; } }

        uint highestContextStatementFileID;
        public uint NewContextStatementID { get { highestContextStatementFileID++; return highestContextStatementFileID; } }
        uint highestTheoremStatementFileID;
        public uint NewTheoremStatementID { get { highestTheoremStatementFileID++; return highestTheoremStatementFileID; } }
        uint highestDefinitionStatementFileID;
        public uint NewDefinitionStatementID { get { highestDefinitionStatementFileID++; return highestDefinitionStatementFileID; } }
        uint highestDeductionStepStatementFileID;
        public uint NewDeductionStepStatementID { get { highestDeductionStepStatementFileID++; return highestDeductionStepStatementFileID; } }

        uint highestVariableFileID;
        public uint NewVariableID { get { highestVariableFileID++; return highestVariableFileID; } }

        public FileIDManager(MDocument D)
        { Document = D; }

        public ContextFileID RequestContextFileID(MContext X)
        {
            ContextFileID ID = new ContextFileID(Document)
            {
                contextFileID = NewContextID
            };
            return ID;
        }

        public TheoremFileID RequestTheoremFileID(MTheorem T)
        {
            TheoremFileID ID = new TheoremFileID(Document)
            {
                contextFileID = T.Context.fileID.contextFileID,
                theoremFileID = NewTheoremID
            };
            return ID;
        }

        public DefinitionFileID RequestDefinitionFileID(MDefinition D)
        {
            DefinitionFileID ID = new DefinitionFileID(Document)
            {
                contextFileID = D._X.fileID.contextFileID,
                definitionFileID = NewDefinitionID
            };
            return ID;
        }

        public DeductionFileID RequestDeductionFileID(MDeduction D)
        {
            DeductionFileID ID = new DeductionFileID(Document)
            {
                contextFileID = D.Theorem.fileID.contextFileID,
                theoremFileID = D.Theorem.fileID.theoremFileID,
                deductionFileID = NewDeductionID
            };
            return ID;
        }

        public DeductionStepFileID RequestDeductionStepFileID(MDeductionStep DS)
        {
            DeductionStepFileID ID = new DeductionStepFileID(Document)
            {
                contextFileID = DS._D.fileID.contextFileID,
                theoremFileID = DS._D.fileID.theoremFileID,
                deductionFileID = DS._D.fileID.deductionFileID,
                deductionStepFileID = NewDeductionStepID
            };
            return ID;
        }


        public ContextStatementFileID RequestContextStatementFileID(MStatement S)
        {
            ContextStatementFileID ID = new ContextStatementFileID(Document)
            {
                contextFileID = S._X.fileID.contextFileID,
                contextStatementFileID = NewContextStatementID
            };
            return ID;
        }

        public TheoremStatementFileID RequestTheoremStatementFileID(MStatement S, MTheorem T)
        {
            TheoremStatementFileID ID = new TheoremStatementFileID(Document)
            {
                contextFileID = S._X.fileID.contextFileID,
                theoremFileID = T.fileID.theoremFileID,
                theoremStatementFileID = NewTheoremStatementID
            };
            return ID;
        }

        public DefinitionStatementFileID RequestDefinitionStatementFileID(MStatement S, MDefinition D)
        {
            DefinitionStatementFileID ID = new DefinitionStatementFileID(Document)
            {
                contextFileID = S._X.fileID.contextFileID,
                definitionFileID = D.fileID.definitionFileID,
                definitionStatementFileID = NewDefinitionStatementID
            };
            return ID;
        }

        public DeductionStepStatementFileID RequestDeductionStepStatementFileID(MStatement S, MDeductionStep DS)
        {
            DeductionStepStatementFileID ID = new DeductionStepStatementFileID(Document)
            {
                contextFileID = S._X.fileID.contextFileID,
                theoremFileID = DS.fileID.theoremFileID,
                deductionFileID = DS.fileID.deductionFileID,
                deductionStepFileID = DS.fileID.deductionStepFileID,
                deductionStepStatementFileID = NewDeductionStepStatementID
            };
            return ID;
        }


        public VariableFileID RequestVariableFileID(MVariable V, MContext X)
        {
            VariableFileID ID = new VariableFileID(Document)
            {
                contextFileID = X.fileID.contextFileID,
                variableFileID = NewVariableID
            };
            return ID;
        }



    }

    #region FileID

    public class FileID
    {
        protected const byte prefFileID = 1;
        protected const byte prefContextFileID = 2;
        protected const byte prefDefinitionFileID = 3;
        protected const byte prefTheoremFileID = 4;
        protected const byte prefDeductionFileID = 5;
        protected const byte prefDeductionStepFileID = 6;

        protected const byte prefContextStatementFileID = 7;
        protected const byte prefDefinitionStatementFileID = 8;
        protected const byte prefTheoremStatementFileID = 9;
        protected const byte prefDeductionStepStatementFileID = 10;

        protected const byte prefVariableFileID = 11;

        protected byte prefix;
        public MDocument Document;
        public FileIDManager IDManager => Document.IDManager;

        internal FileID(MDocument D)
        {
            prefix = prefFileID;
            Document = D;
        }

        public virtual void ToStream(DocumentLoader DL)
        {
            DL.Write(prefix);
            if (DL.Document == Document)
                DL.Write((ushort)0);
            else
            DL.Write((ushort)(DL.Document.IndexOfReferencedDocument(Document) + 1));
        }

        public virtual string ToXML(MDocument D)
        {
            string s = prefix.ToString("X") + ".";
            if (D == Document)
                s += "0";
            else
                s += (D.IndexOfReferencedDocument(Document) + 1).ToString("X");
            return s;
        }

        public void ToXML(MExpressionWriter EW)
        {
            EW.WriteB(ToXML(EW.D));
        }

        protected static MDocument ReadDocument(DocumentLoader DL)
        {
            ushort ind = DL.ReadUShort();
            if (ind == 0) return DL.Document;
            return DL.Document.GetReferencedDocument(ind - 1);
        }
        protected static MDocument ReadDocument(MExpressionReader ER)
        {
            ushort ind = ER.NextUShort();
            if (ind == 0) return ER.DL.Document;
            return ER.DL.Document.GetReferencedDocument(ind - 1);
        }
        protected static MDocument ReadDocument(XMLDocumentLoader DL, string doc)
        {
            ushort ind = ushort.Parse(doc, System.Globalization.NumberStyles.HexNumber);
            if (ind == 0) return DL.Document;
            return DL.Document.GetReferencedDocument(ind - 1);
        }

        public static FileID FromStream(XMLDocumentLoader DL, string xml)
        {
            FileID ID;

            string[] encoded = xml.Split('.');
            byte prefix = Byte.Parse(encoded[0],System.Globalization.NumberStyles.HexNumber);

            switch (prefix)
            {
                case prefFileID:
                    ID = new FileID(ReadDocument(DL, encoded[1]));
                    break;
                case prefContextFileID:
                    ID = ContextFileID.FromStream(DL, encoded);
                    break;
                case prefTheoremFileID:
                    ID = TheoremFileID.FromStream(DL, encoded);
                    break;
                case prefDefinitionFileID:
                    ID = DefinitionFileID.FromStream(DL, encoded);
                    break;
                case prefDeductionFileID:
                    ID = DeductionFileID.FromStream(DL, encoded);
                    break;
                case prefDeductionStepFileID:
                    ID = DeductionStepFileID.FromStream(DL, encoded);
                    break;

                case prefContextStatementFileID:
                    ID = ContextStatementFileID.FromStream(DL, encoded);
                    break;
                case prefDefinitionStatementFileID:
                    ID = DefinitionStatementFileID.FromStream(DL, encoded);
                    break;
                case prefTheoremStatementFileID:
                    ID = TheoremStatementFileID.FromStream(DL, encoded);
                    break;
                case prefDeductionStepStatementFileID:
                    ID = DeductionStepStatementFileID.FromStream(DL, encoded);
                    break;

                case prefVariableFileID:
                    ID = VariableFileID.FromStream(DL, encoded);
                    break;
                default:
                    throw new FileLoadException("Unknown FileID Type!");
            }

            return ID;
        }
        public static FileID FromStream(MExpressionReader ER)
        {
            return FromStream(ER.DL, ER.NextStringB());
        }
        public static FileID FromStream(DocumentLoader DL)
        {
            FileID ID;

            byte prefix = DL.ReadByte();

            switch (prefix)
            {
                case prefFileID:
                    ID = new FileID(ReadDocument(DL));
                    break;
                case prefContextFileID:
                    ID = ContextFileID.FromStream(DL);
                    break;
                case prefTheoremFileID:
                    ID = TheoremFileID.FromStream(DL);
                    break;
                case prefDefinitionFileID:
                    ID = DefinitionFileID.FromStream(DL);
                    break;
                case prefDeductionFileID:
                    ID = DeductionFileID.FromStream(DL);
                    break;
                case prefDeductionStepFileID:
                    ID = DeductionStepFileID.FromStream(DL);
                    break;

                case prefContextStatementFileID:
                    ID = ContextStatementFileID.FromStream(DL);
                    break;
                case prefDefinitionStatementFileID:
                    ID = DefinitionStatementFileID.FromStream(DL);
                    break;
                case prefTheoremStatementFileID:
                    ID = TheoremStatementFileID.FromStream(DL);
                    break;
                case prefDeductionStepStatementFileID:
                    ID = DeductionStepStatementFileID.FromStream(DL);
                    break;

                case prefVariableFileID:
                    ID = VariableFileID.FromStream(DL);
                    break;
                default:
                    throw new FileLoadException("Unknown FileID Type!");
            }

            return ID;
        }

        internal virtual void ApplyContext(MContext X)
        {
            //Call only on ContextFileID. exists here for convenience
        }

        public virtual MStatement FindStatement(ProtoDocumentLoader DL = null)
        {
            if (this is ContextStatementFileID cID)
                return cID.FindStatement(DL);
            if (this is TheoremStatementFileID tID)
                return tID.FindStatement(DL);
            if (this is DeductionStepStatementFileID dsID)
                return dsID.FindStatement(DL);
            if (this is DefinitionStatementFileID dID)
                return dID.FindStatement(DL);
            throw new Exception();
        }

        public IElementWithFileID FindElement(ProtoDocumentLoader DL = null)
        {
            if (this is ContextStatementFileID csID)
                return csID.FindStatement(DL);
            if (this is TheoremStatementFileID tsID)
                return tsID.FindStatement(DL);
            if (this is DeductionStepStatementFileID dssID)
                return dssID.FindStatement(DL);
            if (this is DefinitionStatementFileID defsID)
                return defsID.FindStatement(DL);

            if (this is DeductionStepFileID dsID)
                return dsID.FindDeductionStep(DL);
            if (this is DeductionFileID dID)
                return dID.FindDeduction(DL);
            if (this is TheoremFileID tID)
                return tID.FindTheorem(DL);
            if (this is DefinitionFileID defID)
                return defID.FindDefinition(DL);

            if (this is ContextFileID cID)
                return cID.FindContext(DL);

            return Document ?? DL.Document;
        }

        public virtual bool IsSubFileID(FileID ID)
        {
            return Document == ID.Document;
        }
    }

    public class ContextFileID : FileID
    {
        internal uint contextFileID;

        internal ContextFileID(MDocument D) : base(D)
        {
            prefix = prefContextFileID;
        }

        public override void ToStream(DocumentLoader DL)
        {
            base.ToStream(DL);
            DL.Write(contextFileID);
        }

        public override string ToXML(MDocument D)
        {
            return base.ToXML(D) + "." + contextFileID.ToString("X");
        }

        internal new static ContextFileID FromStream(DocumentLoader DL)
        {
            ContextFileID ID = new ContextFileID(ReadDocument(DL))
            {
                contextFileID = DL.ReadUInt()
            };
            return ID;
        }
        internal new static ContextFileID FromStream(XMLDocumentLoader DL, string[] encoded)
        {
            ContextFileID ID = new ContextFileID(ReadDocument(DL, encoded[1]))
            {
                contextFileID = uint.Parse(encoded[2], System.Globalization.NumberStyles.HexNumber)
            };
            return ID;
        }

        internal ContextFileID MakeContextFileID()
        {
            return new ContextFileID(Document) { contextFileID = contextFileID };
        }

        public MContext FindContext(ProtoDocumentLoader DL = null)
        {
            if (DL != null && DL.Context != null && Document == DL.Document && DL.Context.fileID.contextFileID == contextFileID)
                return DL.Context;

            return Document.GetContext(this);
        }

        internal override void ApplyContext(MContext X)
        {
            contextFileID = (X.fileID as ContextFileID).contextFileID;
        }

        public override bool IsSubFileID(FileID ID)
        {
            if (ID is ContextFileID cID)
                return base.IsSubFileID(ID) && cID.contextFileID == contextFileID;
            return false;
        }
    }

    public class TheoremFileID : ContextFileID
    {
        internal uint theoremFileID;

        internal TheoremFileID(MDocument D) : base(D)
        {
            prefix = prefTheoremFileID;
        }

        public override void ToStream(DocumentLoader DL)
        {
            base.ToStream(DL);
            DL.Write(theoremFileID);
        }

        public override string ToXML(MDocument D)
        {
            return base.ToXML(D) + "." + theoremFileID.ToString("X");
        }

        internal new static TheoremFileID FromStream(DocumentLoader DL)
        {
            TheoremFileID ID = new TheoremFileID(ReadDocument(DL))
            {
                contextFileID = DL.ReadUInt(),
                theoremFileID = DL.ReadUInt()
            };
            return ID;
        }
        internal new static TheoremFileID FromStream(XMLDocumentLoader DL, string[] encoded)
        {
            TheoremFileID ID = new TheoremFileID(ReadDocument(DL, encoded[1]))
            {
                contextFileID = uint.Parse(encoded[2], System.Globalization.NumberStyles.HexNumber),
                theoremFileID = uint.Parse(encoded[3], System.Globalization.NumberStyles.HexNumber)
            };
            return ID;
        }

        internal TheoremFileID MakeTheoremFileID()
        {
            return new TheoremFileID(Document) { contextFileID = contextFileID, theoremFileID = theoremFileID };
        }

        public MTheorem FindTheorem(ProtoDocumentLoader DL = null)
        {
            MContext X = FindContext(DL);
            if (X == null) throw new Exception();
            
            if (DL != null && DL.Theorem != null && Document == DL.Document && DL.Theorem.fileID.theoremFileID == theoremFileID)
                return DL.Theorem;

            return X.GetTheorem(this);
        }

        public override bool IsSubFileID(FileID ID)
        {
            if (ID is TheoremFileID cID)
                return base.IsSubFileID(ID) && cID.theoremFileID == theoremFileID;
            return false;
        }
    }

    public class DeductionFileID : TheoremFileID
    {
        internal uint deductionFileID;

        internal DeductionFileID(MDocument D) : base(D)
        {
            prefix = prefDeductionFileID;
        }

        public override void ToStream(DocumentLoader DL)
        {
            base.ToStream(DL);
            DL.Write(deductionFileID);
        }

        public override string ToXML(MDocument D)
        {
            return base.ToXML(D) + "." + deductionFileID.ToString("X");
        }

        internal new static DeductionFileID FromStream(DocumentLoader DL)
        {
            DeductionFileID ID = new DeductionFileID(ReadDocument(DL))
            {
                contextFileID = DL.ReadUInt(),
                theoremFileID = DL.ReadUInt(),
                deductionFileID = DL.ReadUInt()
            };
            return ID;
        }
        internal new static DeductionFileID FromStream(XMLDocumentLoader DL, string[] encoded)
        {
            DeductionFileID ID = new DeductionFileID(ReadDocument(DL, encoded[1]))
            {
                contextFileID = uint.Parse(encoded[2], System.Globalization.NumberStyles.HexNumber),
                theoremFileID = uint.Parse(encoded[3], System.Globalization.NumberStyles.HexNumber),
                deductionFileID = uint.Parse(encoded[4], System.Globalization.NumberStyles.HexNumber)
            };
            return ID;
        }

        internal DeductionFileID MakeDeductionFileID()
        {
            return new DeductionFileID(Document)
            { contextFileID = contextFileID, theoremFileID = theoremFileID, deductionFileID = deductionFileID };
        }

        public MDeduction FindDeduction(ProtoDocumentLoader DL = null)
        {
            MTheorem T = FindTheorem(DL);
            if (T == null) throw new Exception();
            
            if (DL != null && DL.Deduction != null && Document == DL.Document && DL.Deduction.fileID.deductionFileID == deductionFileID)
                return DL.Deduction;

            return T.GetDeduction();
        }

        public override bool IsSubFileID(FileID ID)
        {
            if (ID is DeductionFileID cID)
                return base.IsSubFileID(ID) && cID.deductionFileID == deductionFileID;
            return false;
        }
    }

    public class DeductionStepFileID : DeductionFileID
    {
        internal uint deductionStepFileID;

        internal DeductionStepFileID(MDocument D) : base(D)
        {
            prefix = prefDeductionStepFileID;
        }

        public override void ToStream(DocumentLoader DL)
        {
            base.ToStream(DL);
            DL.Write(deductionStepFileID);
        }

        public override string ToXML(MDocument D)
        {
            return base.ToXML(D) + "." + deductionStepFileID.ToString("X");
        }

        internal new static DeductionStepFileID FromStream(DocumentLoader DL)
        {
            DeductionStepFileID ID = new DeductionStepFileID(ReadDocument(DL))
            {
                contextFileID = DL.ReadUInt(),
                theoremFileID = DL.ReadUInt(),
                deductionFileID = DL.ReadUInt(),
                deductionStepFileID = DL.ReadUInt()
            };
            return ID;
        }
        internal new static DeductionStepFileID FromStream(XMLDocumentLoader DL, string[] encoded)
        {
            DeductionStepFileID ID = new DeductionStepFileID(ReadDocument(DL, encoded[1]))
            {
                contextFileID = uint.Parse(encoded[2], System.Globalization.NumberStyles.HexNumber),
                theoremFileID = uint.Parse(encoded[3], System.Globalization.NumberStyles.HexNumber),
                deductionFileID = uint.Parse(encoded[4], System.Globalization.NumberStyles.HexNumber),
                deductionStepFileID = uint.Parse(encoded[5], System.Globalization.NumberStyles.HexNumber)
            };
            return ID;
        }

        internal DeductionStepFileID MakeDeductionStepFileID()
        {
            return new DeductionStepFileID(Document)
            { contextFileID = contextFileID, theoremFileID = theoremFileID, deductionFileID = deductionFileID, deductionStepFileID = deductionStepFileID };
        }

        public MDeductionStep FindDeductionStep(ProtoDocumentLoader DL = null)
        {
            MDeduction D = FindDeduction(DL);
            if (D == null) throw new Exception();
            
            if (DL != null && DL.DeductionStep != null && Document == DL.Document && DL.DeductionStep.fileID.deductionStepFileID == deductionStepFileID)
                return DL.DeductionStep;

            return D.GetStep(this);
        }

        public override bool IsSubFileID(FileID ID)
        {
            if (ID is DeductionStepFileID cID)
                return base.IsSubFileID(ID) && cID.deductionStepFileID == deductionStepFileID;
            return false;
        }
    }

    public class DefinitionFileID : ContextFileID
    {
        internal uint definitionFileID;

        internal DefinitionFileID(MDocument D) : base(D)
        {
            prefix = prefDefinitionFileID;
        }

        public override void ToStream(DocumentLoader DL)
        {
            base.ToStream(DL);
            DL.Write(definitionFileID);
        }

        public override string ToXML(MDocument D)
        {
            return base.ToXML(D) + "." + definitionFileID.ToString("X");
        }

        internal new static DefinitionFileID FromStream(DocumentLoader DL)
        {
            DefinitionFileID ID = new DefinitionFileID(ReadDocument(DL))
            {
                contextFileID = DL.ReadUInt(),
                definitionFileID = DL.ReadUInt()
            };
            return ID;
        }
        internal new static DefinitionFileID FromStream(XMLDocumentLoader DL, string[] encoded)
        {
            DefinitionFileID ID = new DefinitionFileID(ReadDocument(DL, encoded[1]))
            {
                contextFileID = uint.Parse(encoded[2], System.Globalization.NumberStyles.HexNumber),
                definitionFileID = uint.Parse(encoded[3], System.Globalization.NumberStyles.HexNumber)
            };
            return ID;
        }

        public MDefinition FindDefinition(ProtoDocumentLoader DL = null)
        {
            MContext X = FindContext(DL);
            if (X == null) throw new Exception();

            if (DL != null && DL.Definition != null && Document == DL.Document && DL.Definition.fileID.definitionFileID == definitionFileID)
                return DL.Definition;
            
            return X.GetDefinition(this);
        }

        public override bool IsSubFileID(FileID ID)
        {
            if (ID is DefinitionFileID cID)
                return base.IsSubFileID(ID) && cID.definitionFileID == definitionFileID;
            return false;
        }
    }

    public class ContextStatementFileID : ContextFileID
    {
        internal uint contextStatementFileID;

        internal ContextStatementFileID(MDocument D) : base(D)
        {
            prefix = prefContextStatementFileID;
        }

        public override void ToStream(DocumentLoader DL)
        {
            base.ToStream(DL);
            DL.Write(contextStatementFileID);
        }

        public override string ToXML(MDocument D)
        {
            return base.ToXML(D) + "." + contextStatementFileID.ToString("X");
        }

        internal new static ContextStatementFileID FromStream(DocumentLoader DL)
        {
            ContextStatementFileID ID = new ContextStatementFileID(ReadDocument(DL))
            {
                contextFileID = DL.ReadUInt(),
                contextStatementFileID = DL.ReadUInt()
            };
            return ID;
        }
        internal new static ContextStatementFileID FromStream(XMLDocumentLoader DL, string[] encoded)
        {
            ContextStatementFileID ID = new ContextStatementFileID(ReadDocument(DL, encoded[1]))
            {
                contextFileID = uint.Parse(encoded[2], System.Globalization.NumberStyles.HexNumber),
                contextStatementFileID = uint.Parse(encoded[3], System.Globalization.NumberStyles.HexNumber)
            };
            return ID;
        }

        public override MStatement FindStatement(ProtoDocumentLoader DL = null)
        {
            MContext X = FindContext(DL);
            if (X == null) throw new Exception();

            return X.GetAxiom(this);
        }

        public override bool IsSubFileID(FileID ID)
        {
            if (ID is ContextStatementFileID cID)
                return base.IsSubFileID(ID) && cID.contextStatementFileID == contextStatementFileID;
            return false;
        }
    }

    public class TheoremStatementFileID : TheoremFileID
    {
        internal uint theoremStatementFileID;

        internal TheoremStatementFileID(MDocument D) : base(D)
        {
            prefix = prefTheoremStatementFileID;
        }

        public override void ToStream(DocumentLoader DL)
        {
            base.ToStream(DL);
            DL.Write(theoremStatementFileID);
        }

        public override string ToXML(MDocument D)
        {
            return base.ToXML(D) + "." + theoremStatementFileID.ToString("X");
        }

        internal new static TheoremStatementFileID FromStream(DocumentLoader DL)
        {
            TheoremStatementFileID ID = new TheoremStatementFileID(ReadDocument(DL))
            {
                contextFileID = DL.ReadUInt(),
                theoremFileID = DL.ReadUInt(),
                theoremStatementFileID = DL.ReadUInt()
            };
            return ID;
        }
        internal new static TheoremStatementFileID FromStream(XMLDocumentLoader DL, string[] encoded)
        {
            TheoremStatementFileID ID = new TheoremStatementFileID(ReadDocument(DL, encoded[1]))
            {
                contextFileID = uint.Parse(encoded[2], System.Globalization.NumberStyles.HexNumber),
                theoremFileID = uint.Parse(encoded[3], System.Globalization.NumberStyles.HexNumber),
                theoremStatementFileID = uint.Parse(encoded[4], System.Globalization.NumberStyles.HexNumber)
            };
            return ID;
        }

        public override MStatement FindStatement(ProtoDocumentLoader DL = null)
        {
            MTheorem T = FindTheorem(DL);
            if (T == null) throw new Exception();

            return T.GetStatement();
        }

        public override bool IsSubFileID(FileID ID)
        {
            if (ID is TheoremStatementFileID cID)
                return base.IsSubFileID(ID) && cID.theoremStatementFileID == theoremStatementFileID;
            return false;
        }
    }

    public class DeductionStepStatementFileID : DeductionStepFileID
    {
        internal uint deductionStepStatementFileID;

        internal DeductionStepStatementFileID(MDocument D) : base(D)
        {
            prefix = prefDeductionStepStatementFileID;
        }

        public override void ToStream(DocumentLoader DL)
        {
            base.ToStream(DL);
            DL.Write(deductionStepStatementFileID);
        }

        public override string ToXML(MDocument D)
        {
            return base.ToXML(D) + "." + deductionStepStatementFileID.ToString("X");
        }

        internal new static DeductionStepStatementFileID FromStream(DocumentLoader DL)
        {
            DeductionStepStatementFileID ID = new DeductionStepStatementFileID(ReadDocument(DL))
            {
                contextFileID = DL.ReadUInt(),
                theoremFileID = DL.ReadUInt(),
                deductionFileID = DL.ReadUInt(),
                deductionStepFileID = DL.ReadUInt(),
                deductionStepStatementFileID = DL.ReadUInt()
            };
            return ID;
        }
        internal new static DeductionStepStatementFileID FromStream(XMLDocumentLoader DL, string[] encoded)
        {
            DeductionStepStatementFileID ID = new DeductionStepStatementFileID(ReadDocument(DL, encoded[1]))
            {
                contextFileID = uint.Parse(encoded[2], System.Globalization.NumberStyles.HexNumber),
                theoremFileID = uint.Parse(encoded[3], System.Globalization.NumberStyles.HexNumber),
                deductionFileID = uint.Parse(encoded[4], System.Globalization.NumberStyles.HexNumber),
                deductionStepFileID = uint.Parse(encoded[5], System.Globalization.NumberStyles.HexNumber),
                deductionStepStatementFileID = uint.Parse(encoded[6], System.Globalization.NumberStyles.HexNumber)
            };
            return ID;
        }

        public override MStatement FindStatement(ProtoDocumentLoader DL = null)
        {
            MDeductionStep DS = FindDeductionStep(DL);
            if (DS == null) throw new Exception();

            return DS.GetConsequence();
        }

        public override bool IsSubFileID(FileID ID)
        {
            if (ID is DeductionStepStatementFileID cID)
                return base.IsSubFileID(ID) && cID.deductionStepStatementFileID == deductionStepStatementFileID;
            return false;
        }
    }

    public class DefinitionStatementFileID : DefinitionFileID
    {
        internal uint definitionStatementFileID;

        internal DefinitionStatementFileID(MDocument D) : base(D)
        {
            prefix = prefDefinitionStatementFileID;
        }

        public override void ToStream(DocumentLoader DL)
        {
            base.ToStream(DL);
            DL.Write(definitionStatementFileID);
        }

        public override string ToXML(MDocument D)
        {
            return base.ToXML(D) + "." + definitionStatementFileID.ToString("X");
        }

        internal new static DefinitionStatementFileID FromStream(DocumentLoader DL)
        {
            DefinitionStatementFileID ID = new DefinitionStatementFileID(ReadDocument(DL))
            {
                contextFileID = DL.ReadUInt(),
                definitionFileID = DL.ReadUInt(),
                definitionStatementFileID = DL.ReadUInt()
            };
            return ID;
        }
        internal new static DefinitionStatementFileID FromStream(XMLDocumentLoader DL, string[] encoded)
        {
            DefinitionStatementFileID ID = new DefinitionStatementFileID(ReadDocument(DL, encoded[1]))
            {
                contextFileID = uint.Parse(encoded[2], System.Globalization.NumberStyles.HexNumber),
                definitionFileID = uint.Parse(encoded[3], System.Globalization.NumberStyles.HexNumber),
                definitionStatementFileID = uint.Parse(encoded[4], System.Globalization.NumberStyles.HexNumber)
            };
            return ID;
        }

        public override MStatement FindStatement(ProtoDocumentLoader DL = null)
        {
            MDefinition D = FindDefinition(DL);
            if (D == null) throw new Exception();

            return D.GetAxiom(this);
        }
        
        public override bool IsSubFileID(FileID ID)
        {
            if (ID is DefinitionStatementFileID cID)
                return base.IsSubFileID(ID) && cID.definitionStatementFileID == definitionStatementFileID;
            return false;
        }
    }

    public class VariableFileID : ContextFileID
    {
        internal uint variableFileID;

        internal VariableFileID(MDocument D) : base(D)
        {
            prefix = prefVariableFileID;
        }

        public override void ToStream(DocumentLoader DL)
        {
            base.ToStream(DL);
            DL.Write(variableFileID);
        }

        public override string ToXML(MDocument D)
        {
            return base.ToXML(D) + "." + variableFileID.ToString("X");
        }

        internal new static VariableFileID FromStream(DocumentLoader DL)
        {
            VariableFileID ID = new VariableFileID(ReadDocument(DL))
            {
                contextFileID = DL.ReadUInt(),
                variableFileID = DL.ReadUInt()
            };
            return ID;
        }
        internal new static VariableFileID FromStream(XMLDocumentLoader DL, string[] encoded)
        {
            VariableFileID ID = new VariableFileID(ReadDocument(DL, encoded[1]))
            {
                contextFileID = uint.Parse(encoded[2], System.Globalization.NumberStyles.HexNumber),
                variableFileID = uint.Parse(encoded[3], System.Globalization.NumberStyles.HexNumber)
            };
            return ID;
        }

        public MVariable FindVariable(ProtoDocumentLoader DL = null)
        {
            MContext X = FindContext(DL);
            if (X == null) throw new Exception();

            return X.GetVariable(this);
        }

        public override bool IsSubFileID(FileID ID)
        {
            if (ID is VariableFileID cID)
                return base.IsSubFileID(ID) && cID.variableFileID == variableFileID;
            return false;
        }
    }

    #endregion
}