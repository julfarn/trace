using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TraceBackend
{
    public partial class FileIDManager
    {
        internal static FileIDManager FromStream(DocumentLoader DL)
        {
            //prefix
            DL.ReadUInt();

            FileIDManager IDM = new FileIDManager(DL.Document)
            {
                highestContextFileID = DL.ReadUInt(),
                highestTheoremFileID = DL.ReadUInt(),
                highestDefinitionFileID = DL.ReadUInt(),
                highestDeductionFileID = DL.ReadUInt(),
                highestDeductionStepFileID = DL.ReadUInt(),

                highestContextStatementFileID = DL.ReadUInt(),
                highestTheoremStatementFileID = DL.ReadUInt(),
                highestDefinitionStatementFileID = DL.ReadUInt(),
                highestDeductionStepStatementFileID = DL.ReadUInt(),

                highestVariableFileID = DL.ReadUInt()
            };
            return IDM;
        }

        internal static FileIDManager FromStream(XMLDocumentLoader DL, XmlNode node)
        {
            FileIDManager IDM = new FileIDManager(DL.Document)
            {
                
                highestContextFileID = (uint)node.GetChildNode("type", "context").GetAttrInt("value"),
                highestTheoremFileID = (uint)node.GetChildNode("type", "theorem").GetAttrInt("value"),
                highestDefinitionFileID = (uint)node.GetChildNode("type", "definition").GetAttrInt("value"),
                highestDeductionFileID = (uint)node.GetChildNode("type", "deduction").GetAttrInt("value"),
                highestDeductionStepFileID = (uint)node.GetChildNode("type", "deductionstep").GetAttrInt("value"),

                highestContextStatementFileID = (uint)node.GetChildNode("type", "contextstatement").GetAttrInt("value"),
                highestTheoremStatementFileID = (uint)node.GetChildNode("type", "theoremstatement").GetAttrInt("value"),
                highestDefinitionStatementFileID = (uint)node.GetChildNode("type", "definitionstatement").GetAttrInt("value"),
                highestDeductionStepStatementFileID = (uint)node.GetChildNode("type", "deductionstepstatement").GetAttrInt("value"),

                highestVariableFileID = (uint)node.GetChildNode("type", "variable").GetAttrInt("value")
            };
            return IDM;
        }

        internal void ToStream(DocumentLoader DL)
        {
            DL.BeginSubStream();

            DL.Write(highestContextFileID);
            DL.Write(highestTheoremFileID);
            DL.Write(highestDefinitionFileID);
            DL.Write(highestDeductionFileID);
            DL.Write(highestDeductionStepFileID);

            DL.Write(highestContextStatementFileID);
            DL.Write(highestTheoremStatementFileID);
            DL.Write(highestDefinitionStatementFileID);
            DL.Write(highestDeductionStepStatementFileID);

            DL.Write(highestVariableFileID);

            DL.EndSubStream();
        }

        internal void ToXML(XmlWriter Xwr)
        {
            Xwr.WriteStartElement("id-manager");

            Xwr.WriteStartElement("max-id");
            Xwr.WriteAttributeString("type", "context");
            Xwr.WriteAttributeString("value", highestContextFileID.ToString());
            Xwr.WriteEndElement();
            Xwr.WriteStartElement("max-id");
            Xwr.WriteAttributeString("type", "theorem");
            Xwr.WriteAttributeString("value", highestTheoremFileID.ToString());
            Xwr.WriteEndElement();
            Xwr.WriteStartElement("max-id");
            Xwr.WriteAttributeString("type", "definition");
            Xwr.WriteAttributeString("value", highestDefinitionFileID.ToString());
            Xwr.WriteEndElement();
            Xwr.WriteStartElement("max-id");
            Xwr.WriteAttributeString("type", "deduction");
            Xwr.WriteAttributeString("value", highestDeductionFileID.ToString());
            Xwr.WriteEndElement();
            Xwr.WriteStartElement("max-id");
            Xwr.WriteAttributeString("type", "deductionstep");
            Xwr.WriteAttributeString("value", highestDeductionStepFileID.ToString());
            Xwr.WriteEndElement();

            Xwr.WriteStartElement("max-id");
            Xwr.WriteAttributeString("type", "contextstatement");
            Xwr.WriteAttributeString("value", highestContextStatementFileID.ToString());
            Xwr.WriteEndElement();
            Xwr.WriteStartElement("max-id");
            Xwr.WriteAttributeString("type", "theoremstatement");
            Xwr.WriteAttributeString("value", highestTheoremStatementFileID.ToString());
            Xwr.WriteEndElement();
            Xwr.WriteStartElement("max-id");
            Xwr.WriteAttributeString("type", "definitionstatement");
            Xwr.WriteAttributeString("value", highestDefinitionStatementFileID.ToString());
            Xwr.WriteEndElement();
            Xwr.WriteStartElement("max-id");
            Xwr.WriteAttributeString("type", "deductionstepstatement");
            Xwr.WriteAttributeString("value", highestDeductionStepStatementFileID.ToString());
            Xwr.WriteEndElement();

            Xwr.WriteStartElement("max-id");
            Xwr.WriteAttributeString("type", "variable");
            Xwr.WriteAttributeString("value", highestVariableFileID.ToString());
            Xwr.WriteEndElement();

            Xwr.WriteEndElement();
        }
    }
}
