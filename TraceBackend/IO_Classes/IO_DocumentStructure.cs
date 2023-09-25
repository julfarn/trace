using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TraceBackend
{
    public partial class DocumentStructure
    {
        public void ToStream(DocumentLoader DL)
        {
            DL.Write(Hidden);
            DL.Write(Collapsed);

            if(Element is IElementWithFileID idEl)
            {
                DL.Write(0);
                idEl.baseFileID.ToStream(DL);
            }
            else if(Element is MStatementList sl)
            {
                DL.Write(1);
                sl.Definition.fileID.ToStream(DL);
            }
            else if(Element is MVisualisationScheme vs)
            {
                DL.Write(2);
                vs.Definition.fileID.ToStream(DL);

            }
            else if (Element is MDocumentText tx)
            {
                DL.Write(3);
                tx.ToStream(DL);
            }

            DL.Write(Children.Count);
            foreach (DocumentStructure S in Children)
                S.ToStream(DL);
        }
        public void ToXML(XMLDocumentLoader DL)
        {
            DL.Xwr.WriteStartElement("struct");
            DL.Xwr.WriteAttributeString("hidden", Hidden.ToString());
            DL.Xwr.WriteAttributeString("collapsed", Collapsed.ToString());

            if (Element is IElementWithFileID idEl)
            {
                DL.Xwr.WriteAttributeString("type", "0");
                DL.Xwr.WriteAttributeString("id", idEl.baseFileID.ToXML(DL.Document));
            }
            else if (Element is MStatementList sl)
            {
                DL.Xwr.WriteAttributeString("type", "1");
                DL.Xwr.WriteAttributeString("id", sl.Definition.fileID.ToXML(DL.Document));
            }
            else if (Element is MVisualisationScheme vs)
            {
                DL.Xwr.WriteAttributeString("type", "2");
                DL.Xwr.WriteAttributeString("id", vs.Definition.fileID.ToXML(DL.Document));

            }
            else if (Element is MDocumentText tx)
            {
                DL.Xwr.WriteAttributeString("type", "3");
                DL.Xwr.WriteAttributeString("txt", tx.Text);
            }

            DL.Xwr.WriteAttributeString("children", Children.Count.ToString());
            foreach (DocumentStructure S in Children)
                S.ToXML(DL);

            DL.Xwr.WriteEndElement(); // struct
        }

        public static DocumentStructure FromStream(DocumentLoader DL)
        {
            bool Hidden = DL.ReadBool();
            bool Collapsed = DL.ReadBool();

            int type = DL.ReadInt();
            IDocumentElement DE = null;
            switch (type)
            {
                case 0: // FileID
                    DE = FileID.FromStream(DL).FindElement(DL);
                    break;
                case 1: // StatementList
                    DE = new MStatementList() { Definition = (FileID.FromStream(DL) as DefinitionFileID).FindDefinition(DL) };
                    break;
                case 2: // Visualization Scheme
                    DE = (FileID.FromStream(DL) as DefinitionFileID).FindDefinition(DL).DefaultVisualization; 
                    break;
                case 3: // Text
                    DE = MDocumentText.FromStream(DL);
                    break;
            }

            DocumentStructure DS = Embed(DE);
            DS.Hidden = Hidden;
            DS.Collapsed = Collapsed;
            int cCount = DL.ReadInt();
            for(int i = 0; i<cCount; i++)
            {
                DocumentStructure S = FromStream(DL);
                DS.Children.Add(S); S.Parent = DS;
            }

            return DS;
        }
        public static DocumentStructure FromStream(XMLDocumentLoader DL, XmlNode node)
        {
            bool Hidden = node.GetAttrBool("hidden");
            bool Collapsed = node.GetAttrBool("collapsed");

            int type = node.GetAttrInt("type");
            IDocumentElement DE = null;
            switch (type)
            {
                case 0: // FileID
                    DE = FileID.FromStream(DL, node.GetAttr("id")).FindElement(DL);
                    break;
                case 1: // StatementList
                    DE = new MStatementList() { Definition = (FileID.FromStream(DL, node.GetAttr("id")) as DefinitionFileID).FindDefinition(DL) };
                    break;
                case 2: // Visualization Scheme
                    DE = (FileID.FromStream(DL, node.GetAttr("id")) as DefinitionFileID).FindDefinition(DL).DefaultVisualization;
                    break;
                case 3: // Text
                    DE = MDocumentText.FromStream(node.GetAttr("txt"));
                    break;
            }

            DocumentStructure DS = Embed(DE);
            DS.Hidden = Hidden;
            DS.Collapsed = Collapsed;
            int cCount = node.GetAttrInt("children");
            for (int i = 0; i < cCount; i++)
            {
                DocumentStructure S = FromStream(DL, node.GetChildNode("struct", i));
                DS.Children.Add(S); S.Parent = DS;
            }

            return DS;
        }
    }
}
