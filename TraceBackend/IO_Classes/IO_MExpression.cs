using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TraceBackend
{
    public partial class MExpression
    {
        public override void ToStream(DocumentLoader DL)
        {
            DL.Write((byte)visID);
            DL.Write((byte)brMode);
            
            if (DL.PDList == null)
            {
                DL.PDList = new List<MUndefinedPredicate>();
                MakeUndefinedFormulaList(DL.PDList);

                DL.Write(DL.PDList.Count);
                foreach (MUndefinedPredicate PD in DL.PDList)
                    PD.ToStream(DL);
            }
        }

        public override void ToXML(XMLDocumentLoader DL)
        {
            MExpressionWriter EW = MExpressionWriter.Create(DL.Document);
            MakeUndefinedFormulaList(EW.PDList);

            EW.Write((ushort)EW.PDList.Count);
            foreach (MUndefinedPredicate PD in EW.PDList)
                PD.ToXML(EW);

            ToXML(EW);
            DL.Xwr.WriteElementString("expression", EW.Fetch());
        }

        internal virtual void ToXML(MExpressionWriter EW)
        {
            EW.Write((byte)visID);
            EW.Write((byte)brMode);
        }

        public void ReadVisSettings(DocumentLoader DL)
        {
            visID = DL.ReadByte();
            brMode = (BracketMode)DL.ReadByte();
        }

        public void ReadVisSettings(MExpressionReader ER)
        {
            visID = ER.NextByte();
            brMode = (BracketMode)ER.NextByte();
        }
    }

    public class MExpressionWriter
    {
        public List<MUndefinedPredicate> PDList;
        StringBuilder sb;

        public MDocument D;

        public static MExpressionWriter Create(MDocument Document)
        {
            MExpressionWriter EW = new MExpressionWriter()
            {
                PDList = new List<MUndefinedPredicate>(),
                sb = new StringBuilder(),
                D = Document
            };
            return EW;
        }

        public string Fetch()
        { return sb.ToString(); }

        string hex(bool b) { return b ? "T" : "F"; }
        string hex(byte b) { return b.ToString("X2"); }
        string hex(uint i) { return i.ToString("X8"); }
        string hex(ushort s) { return s.ToString("X4"); }

        public void Write(bool b)
        { sb.Append(hex(b)); }
        public void Write(byte b)
        { sb.Append(hex(b)); }
        public void Write(uint i)
        { sb.Append(hex(i)); }
        public void Write(ushort u)
        { sb.Append(hex(u)); }

        public void Write(string s)
        {
            sb.Append(hex((ushort)s.Length));
            sb.Append(s);
        }
        public void WriteB(string s)
        {
            sb.Append(hex((byte)s.Length));
            sb.Append(s);
        }
    }
    public class MExpressionReader
    {
        public List<MUndefinedPredicate> PDList;
        String data;
        int pos = 0;

        public XMLDocumentLoader DL;
        

        public static MExpressionReader Create(XMLDocumentLoader DL, string str)
        {
            MExpressionReader ER = new MExpressionReader()
            {
                data = str,
                DL = DL
            };
            return ER;
        }

        string next(int length) { string ret = data.Substring(pos, length);  pos += length; return ret; }

        public bool NextBool() { return next(1) == "T"; }
        public byte NextByte() { return byte.Parse(next(2), System.Globalization.NumberStyles.HexNumber); }
        public ushort NextUShort() { return ushort.Parse(next(4), System.Globalization.NumberStyles.HexNumber); }
        public uint NextUInt() { return uint.Parse(next(8), System.Globalization.NumberStyles.HexNumber); }
        public string NextString() { return next(NextUShort()); }
        public string NextStringB() { return next(NextByte()); }
    }
}
