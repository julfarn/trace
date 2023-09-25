using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TraceBackend
{
    public partial class MVisualisationScheme {
        public void ToStream(DocumentLoader DL)
        {
            DL.Write(overrideChildren);
            DL.Write(Symbols.Count);
            foreach (MSymbol s in Symbols)
                s.ToStream(DL);

            DL.Write(Latex);

            DL.Write((byte)myBracket);
            for (int i = 0; i < Children;i++)
                DL.Write((byte)Brackets[i]);

            arrangement.ToStream(DL);
        }
        public void ToXML(XMLDocumentLoader DL)
        {
            DL.Xwr.WriteStartElement("visualization");
            DL.Xwr.WriteAttributeString("ovch", overrideChildren.ToString());
            DL.Xwr.WriteAttributeString("symbols", Symbols.Count.ToString());
            foreach (MSymbol s in Symbols)
                s.ToXML(DL);

            DL.Xwr.WriteElementString("latex", Latex);

            DL.Xwr.WriteStartElement("brackets");
            string brstr = ((byte)myBracket).ToString("X2");
            for (int i = 0; i < Children; i++)
                brstr += ((byte)Brackets[i]).ToString("X2");
            DL.Xwr.WriteString(brstr);
            DL.Xwr.WriteEndElement(); //brackets

            arrangement.ToXML(DL);
            DL.Xwr.WriteEndElement(); //visualization
        }

        public static MVisualisationScheme FromStream(DocumentLoader DL)
        {
            MVisualisationScheme VS = new MVisualisationScheme(DL.Definition, DL.ReadInt());

            int sCount = DL.ReadInt();
            List<MSymbol> symbols = new List<MSymbol>();
            for (int i = 0; i < sCount; i++)
                symbols.Add(MSymbol.FromStream(DL));
            VS.Symbols = symbols;

            VS.Latex = DL.ReadString();

            VS.myBracket = (BracketSetting)DL.ReadByte();
            for (int i = 0; i < VS.Children; i++)
                VS.Brackets[i] = (BracketSetting)DL.ReadByte();

            VS.arrangement = MArrangementTree.FromStream(DL);

            return VS;
        }
        public static MVisualisationScheme FromStream(XMLDocumentLoader DL, XmlNode node)
        {
            MVisualisationScheme VS = new MVisualisationScheme(DL.Definition, node.GetAttrInt("ovch"));

            int sCount = node.GetAttrInt("symbols");
            List<MSymbol> symbols = new List<MSymbol>();
            for (int i = 0; i < sCount; i++)
                symbols.Add(MSymbol.FromStream(DL, node.GetChildNode("sym", i)));
            VS.Symbols = symbols;

            VS.Latex = node.GetChildNode("latex").InnerText;

            string brstring = node.GetChildNode("brackets").InnerText;
            VS.myBracket = (BracketSetting)byte.Parse(brstring.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            for (int i = 0; i < VS.Children; i++)
                VS.Brackets[i] = (BracketSetting)byte.Parse(brstring.Substring(2*i+2, 2), System.Globalization.NumberStyles.HexNumber);

            VS.arrangement = MArrangementTree.FromStream(DL, node.GetChildNode("tree"));

            return VS;
        }
    }

    public partial class MArrangementTree {
        public void ToStream(DocumentLoader DL)
        {
            DL.Write(myIndex);
            DL.Write((int)anchor);
            DL.Write(xOff);
            DL.Write(yOff);
            DL.Write(GrowAlongChildren.x);
            DL.Write(GrowAlongChildren.y);
            DL.Write(MakeSmall);

            DL.Write(Branches.Count);
            foreach (MArrangementTree b in Branches)
                b.ToStream(DL);
        }
        public void ToXML(XMLDocumentLoader DL)
        {
            DL.Xwr.WriteStartElement("tree");
            DL.Xwr.WriteAttributeString("myind", myIndex.ToString());
            DL.Xwr.WriteAttributeString("anchor", ((int)anchor).ToString());
            DL.Xwr.WriteAttributeString("xOff", xOff.ToString());
            DL.Xwr.WriteAttributeString("yOff", yOff.ToString());
            DL.Xwr.WriteAttributeString("xGrow", GrowAlongChildren.x.ToString());
            DL.Xwr.WriteAttributeString("yGrow", GrowAlongChildren.y.ToString());
            DL.Xwr.WriteAttributeString("small", MakeSmall.ToString());
            DL.Xwr.WriteAttributeString("branches", Branches.Count.ToString());
            foreach (MArrangementTree b in Branches)
                b.ToXML(DL);
            DL.Xwr.WriteEndElement(); //tree
        }

        public static MArrangementTree FromStream(DocumentLoader DL, MArrangementTree reference = null)
        {
            int index = DL.ReadInt();
            MAnchor anchor = (MAnchor)DL.ReadInt();
            float xOff = DL.ReadFloat();
            float yOff = DL.ReadFloat();

            MArrangementTree tree = new MArrangementTree(index, reference, anchor, xOff, yOff);

            tree.GrowAlongChildren.x = DL.ReadBool();
            tree.GrowAlongChildren.y = DL.ReadBool();
            tree.MakeSmall = DL.ReadBool();

            int bCount = DL.ReadInt();
            for (int i = 0; i < bCount; i++)
                tree.AddBranch(FromStream(DL, tree));

            return tree;
        }
        public static MArrangementTree FromStream(XMLDocumentLoader DL, XmlNode node, MArrangementTree reference = null)
        {
            int index = node.GetAttrInt("myind");
            MAnchor anchor = (MAnchor)node.GetAttrInt("anchor");
            float xOff = node.GetAttrFloat("xOff");
            float yOff = node.GetAttrFloat("yOff");

            MArrangementTree tree = new MArrangementTree(index, reference, anchor, xOff, yOff);

            tree.GrowAlongChildren.x = node.GetAttrBool("xGrow");
            tree.GrowAlongChildren.y = node.GetAttrBool("yGrow");
            tree.MakeSmall = node.GetAttrBool("small");

            int bCount = node.GetAttrInt("branches");
            for (int i = 0; i < bCount; i++)
                tree.AddBranch(FromStream(DL, node.ChildNodes[i], tree));

            return tree;
        }
    }

    public partial class MSymbol {
        public virtual void ToStream(DocumentLoader DL)
        {
            throw new InvalidOperationException("MSymbol.ToStream should not be called directly, only on children.");
        }

        public static MSymbol FromStream(DocumentLoader DL)
        {
            int type = DL.ReadInt();
            if (type == 0)
                return MTextSymbol.FromStream(DL);
            if (type == 1)
                return MShapeSymbol.FromStream(DL);

            throw new FileLoadException("Unknown Symbol Type");
        }
        public static MSymbol FromStream(XMLDocumentLoader DL, XmlNode node)
        {
            int type = node.GetAttrInt("t");
            if (type == 0)
                return MTextSymbol.FromStream(DL, node);
            if (type == 1)
                return MShapeSymbol.FromStream(DL, node);

            throw new FileLoadException("Unknown Symbol Type");
        }
    }

    public partial class MTextSymbol {

        public override void ToStream(DocumentLoader DL)
        {
            DL.Write(0);  // 0 - TextSymbol

            font.ToStream(DL);
            DL.Write(Text);
        }
        public override void ToXML(XMLDocumentLoader DL)
        {
            DL.Xwr.WriteStartElement("sym");
            DL.Xwr.WriteAttributeString("t", "0"); // 0 - TextSymbol

            DL.Xwr.WriteAttributeString("f", font.ToXML(DL));
            DL.Xwr.WriteString(Text);

            DL.Xwr.WriteEndElement();
        }

        public new static MTextSymbol FromStream(DocumentLoader DL)
        {
            FontCategory font = FontExtensions.Fromstream(DL);
            string text = DL.ReadString();

            return new MTextSymbol(text, font);
        }
        public new static MTextSymbol FromStream(XMLDocumentLoader DL, XmlNode node)
        {
            FontCategory font = FontExtensions.Fromstream(node.GetAttr("f"));
            return new MTextSymbol(node.InnerText, font);
        }
    }

    public partial class MShapeSymbol {
        public override void ToStream(DocumentLoader DL)
        {
            DL.Write(1); // 1 - ShapeSymbol

            DL.Write(name);
        }
        public override void ToXML(XMLDocumentLoader DL)
        {
            DL.Xwr.WriteStartElement("sym");
            DL.Xwr.WriteAttributeString("t", "1"); // 1 - ShapeSymbol
            
            DL.Xwr.WriteString(name);

            DL.Xwr.WriteEndElement();
        }

        public new static MShapeSymbol FromStream(DocumentLoader DL)
        {
            return FromName(DL.ReadString());
        }
        public new static MShapeSymbol FromStream(XMLDocumentLoader DL, XmlNode node)
        {
            return FromName(node.InnerText);
        }
    }
}
