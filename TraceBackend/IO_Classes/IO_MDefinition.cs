using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TraceBackend
{
    public partial class MDefinition
    {
        public override void ToStream(DocumentLoader DL)
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");
            /*
                Saving Format:
                in individuals 
                {
                    (begin substream)
                    1st the DefinitionType
                    2nd the FileID
                    3rd the Individual stuff
                }
                here 
                {
                    4th axioms
                    5th the visualisations
                    (end substream)
                }
            */
            DL.BeginSubStream();
            DL.Write((byte)Axioms.Count);
            foreach (MStatement a in Axioms)
                a.ToStream(DL);
            DL.EndSubStream();
            DL.Write((byte)visualisations.Count);
            foreach (MVisualisationScheme vs in visualisations)
                vs.ToStream(DL);
            DL.EndSubStream();
        }

        public override void ToXML(XMLDocumentLoader DL)
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");
            /*
                Saving Format:
                in individuals 
                {
                    (begin substream)
                    1st the DefinitionType
                    2nd the FileID
                    3rd the Individual stuff
                }
                here 
                {
                    4th axioms
                    5th the visualisations
                    (end substream)
                }
            */

            DL.Xwr.WriteStartElement("axioms");
            DL.Xwr.WriteAttributeString("count", Axioms.Count.ToString());
            foreach (MStatement a in Axioms)
                a.ToXML(DL);
            DL.Xwr.WriteEndElement(); // axioms
            DL.Xwr.WriteStartElement("visualizations");
            DL.Xwr.WriteAttributeString("count", visualisations.Count.ToString());
            foreach (MVisualisationScheme vs in visualisations)
                vs.ToXML(DL);
            DL.Xwr.WriteEndElement(); //visualizations
            DL.Xwr.WriteEndElement(); // definition
        }

        public new static MDefinition FromStream(DocumentLoader DL)
        {
            /* 
                0 : Constant (OBSOLETE)
                1 : FunctionSymbol
                2 : Predicate
                3 : BinaryConnective
                4 : Quantifier

                Loading Format:
                here
                {
                    (prefix)
                    1st the DefinitionType
                }
                in individuals 
                {
                    2nd the FileID
                    3rd the Individual stuff
                }
                here 
                {
                    4th axioms
                    5th the visualisations
                }
            */

            //prefix
            DL.Remember();

            int type = DL.ReadByte();

            MDefinition D;

            switch (type)
            {
                case 0: //Constant (OBSOLETE)
                    //D = new MConstantDefinition(DL.Context, new MConstant());
                    throw new FileLoadException("Unknown kind of Definition detected.");
                case 1: //FunctionSymbol
                    D = MFunctionSymbol.FromStream(DL);
                    break;
                case 2: //Predicate (OBSOLETE)
                    //D = MPredicate.FromStream(DL);
                    D = MQuantifier.FromStream(DL);
                    break;
                case 3: //BinaryConnective
                    D = MBinaryConnective.FromStream(DL);
                    break;
                case 4: //Quantifier
                    D = MQuantifier.FromStream(DL);
                    break;
                case 5: //NegationDefinition
                    D = MNegationDefinition.FromStream(DL);
                    break;
                case 6: //EqualityDefinition
                    D = MEqualityDefinition.FromStream(DL);
                    break;
                case 7: //ClassTermDefinition (OBSOLETE)
                    //D = MClassDefinition.FromStream(DL);
                    D = MFunctionSymbol.FromStream(DL);
                    break;
                default:
                    throw new FileLoadException("Unknown kind of Definition detected.");
            }

            DL.SetDefinition(D);

            DL.Remember();
            byte aCount = DL.ReadByte();
            for (int i = 0; i < aCount; i++)
                D.Axioms.Add(MStatement.FromStream(DL));
            byte vCount = DL.ReadByte();
            for (int i = 0; i < vCount; i++)
                D.visualisations.Add(MVisualisationScheme.FromStream(DL));
            D.loaded = true;

            return D;
        }
        public new static MDefinition FromStream(XMLDocumentLoader DL, XmlNode node)
        {
            /* 
                0 : Constant (OBSOLETE)
                1 : FunctionSymbol
                2 : Predicate
                3 : BinaryConnective
                4 : Quantifier

                Loading Format:
                here
                {
                    (prefix)
                    1st the DefinitionType
                }
                in individuals 
                {
                    2nd the FileID
                    3rd the Individual stuff
                }
                here 
                {
                    4th axioms
                    5th the visualisations
                }
            */
            

            int type = node.GetAttrInt("type");

            MDefinition D;

            switch (type)
            {
                case 0: //Constant (OBSOLETE)
                    //D = new MConstantDefinition(DL.Context, new MConstant());
                    throw new FileLoadException("Unknown kind of Definition detected.");
                case 1: //FunctionSymbol
                    D = MFunctionSymbol.FromStream(DL, node);
                    break;
                case 2: //Predicate (OBSOLETE)
                    //D = MPredicate.FromStream(DL, node);
                    D = MQuantifier.FromStream(DL, node);
                    break;
                case 3: //BinaryConnective
                    D = MBinaryConnective.FromStream(DL, node);
                    break;
                case 4: //Quantifier
                    D = MQuantifier.FromStream(DL, node);
                    break;
                case 5: //NegationDefinition
                    D = MNegationDefinition.FromStream(DL, node);
                    break;
                case 6: //EqualityDefinition
                    D = MEqualityDefinition.FromStream(DL, node);
                    break;
                case 7: //ClassTermDefinition (OBSOLETE)
                    //D = MClassDefinition.FromStream(DL, node);
                    D = MFunctionSymbol.FromStream(DL, node);
                    break;
                default:
                    throw new FileLoadException("Unknown kind of Definition detected.");
            }

            DL.SetDefinition(D);

            XmlNode axioms = node.GetChildNode("axioms");
            int aCount = axioms.GetAttrInt("count");
            for (int i = 0; i < aCount; i++)
                D.Axioms.Add(MStatement.FromStream(DL, axioms.ChildNodes[i]));
            XmlNode visualizations = node.GetChildNode("visualizations");
            int vCount = visualizations.GetAttrInt("count");
            for (int i = 0; i < vCount; i++)
                D.visualisations.Add(MVisualisationScheme.FromStream(DL, visualizations.ChildNodes[i]));
            D.loaded = true;

            return D;
        }

        public void Load()
        {
            if (loaded) return;

            GetVisualisations();
            for (int i = 0; i < Axioms.Count; i++)
                GetAxiom(i).Load();

            loaded = true;
        }

        public static MDefinition GetUnloaded(DocumentLoader DL)
        {
            int type = DL.ReadByte();

            MDefinition D;

            switch (type)
            {
                case 0: //Constant (OBSOLETE)
                    throw new FileLoadException("Unknown kind of Definition detected.");
                case 1: //FunctionSymbol
                    D = MFunctionSymbol.FromStream(DL); // we can use FromStream here instead of a GetUnloaded-function because the specific data consists of only some bytes and strings.
                    break;
                case 2: //Predicate (OBSOLETE)
                    //D = MPredicate.FromStream(DL);
                    D = MQuantifier.FromStream(DL);
                    break;
                case 3: //BinaryConnective
                    D = MBinaryConnective.FromStream(DL);
                    break;
                case 4: //Quantifier
                    D = MQuantifier.FromStream(DL);
                    break;
                case 5: //NegationDefinition
                    D = MNegationDefinition.FromStream(DL);
                    break;
                case 6: //EqualityDefinition
                    D = MEqualityDefinition.FromStream(DL);
                    break;
                case 7: //ClassTermDefinition (OBSOLETE)
                    //D = MClassDefinition.FromStream(DL);
                    D = MFunctionSymbol.FromStream(DL); // we can use FromStream here instead of a GetUnloaded-function because the specific data consists of only some bytes and strings.
                    break;
                default:
                    throw new FileLoadException("Unknown kind of Definition detected.");
            }

            DL.SetDefinition(D);

            DL.Remember();
            byte aCount = DL.ReadByte();
            D.Axioms = new MStatement[aCount].ToList();
            byte vCount = DL.ReadByte();
            D.visualisations = new MVisualisationScheme[vCount].ToList();

            return D;
        }
        public static MDefinition GetUnloaded(XMLDocumentLoader DL, XmlNode node)
        {
            int type = node.GetAttrInt("type");

            MDefinition D;

            switch (type)
            {
                case 0: //Constant (OBSOLETE)
                    throw new FileLoadException("Unknown kind of Definition detected.");
                case 1: //FunctionSymbol
                    D = MFunctionSymbol.FromStream(DL, node); // we can use FromStream here instead of a GetUnloaded-function because the specific data consists of only some bytes and strings.
                    break;
                case 2: //Predicate (OBSOLETE)
                    //D = MPredicate.FromStream(DL, node);
                    D = MQuantifier.FromStream(DL, node);
                    break;
                case 3: //BinaryConnective
                    D = MBinaryConnective.FromStream(DL, node);
                    break;
                case 4: //Quantifier
                    D = MQuantifier.FromStream(DL, node);
                    break;
                case 5: //NegationDefinition
                    D = MNegationDefinition.FromStream(DL, node);
                    break;
                case 6: //EqualityDefinition
                    D = MEqualityDefinition.FromStream(DL, node);
                    break;
                case 7: //ClassTermDefinition (OBSOLETE)
                    //D = MClassDefinition.FromStream(DL, node);
                    D = MFunctionSymbol.FromStream(DL, node); // we can use FromStream here instead of a GetUnloaded-function because the specific data consists of only some bytes and strings.
                    break;
                default:
                    throw new FileLoadException("Unknown kind of Definition detected.");
            }

            DL.SetDefinition(D);

            int aCount = node.GetChildNode("axioms").Count();
            D.Axioms = new MStatement[aCount].ToList();
            int vCount = node.GetChildNode("visualizations").Count();
            D.visualisations = new MVisualisationScheme[vCount].ToList();

            return D;
        }

        private void LoadVisualizations()
        {
            if (_X.Document.IsXML) { LoadVisualizationsXML(); return; }

            DocumentLoader DL = _X.Document.DL;
            DL.Navigate(fileID);
            DL.SetContext(_X);
            DL.SetDefinition(this);

            DL.ReadByte(); // type
            FileID.FromStream(DL); //fileID
            DL.CleanSkip(); //Individual Stuff
            DL.CleanSkip(); //Axioms

            byte vCount = DL.ReadByte();
            for (int i = 0; i < vCount; i++)
            {
                visualisations[i] = MVisualisationScheme.FromStream(DL);
            }
            DL.Close();
        }
        private void LoadVisualizationsXML()
        {
            XMLDocumentLoader DL = _X.Document.XDL;
            DL.Navigate(fileID);
            DL.SetContext(_X);
            DL.SetDefinition(this);

            XmlNode visys = DL.currentNode.GetChildNode("visualizations");
            int vCount = visys.Count();
            for (int i = 0; i < vCount; i++)
            {
                visualisations[i] = MVisualisationScheme.FromStream(DL, visys.GetChildNode("visualization", i));
            }
        }

        private void LoadAxiom(int index)
        {
            if (_X.Document.IsXML) { LoadAxiomXML(index); return; }

            DocumentLoader DL = _X.Document.DL;
            DL.Navigate(fileID);
            DL.SetContext(_X);
            DL.SetDefinition(this);

            DL.ReadByte(); // type
            FileID.FromStream(DL); //fileID
            DL.CleanSkip(); //Individual Stuff

            DL.Remember();
            byte aCount = DL.ReadByte();
            if (index >= aCount) { DL.Close(); return; }

            for (int i = 0; i < index; i++)
            {
                DL.CleanSkip(); // skipping earlierer Axioms
            }

            DL.Remember();
            FileID ID = FileID.FromStream(DL);
            Axioms[index] = MStatement.GetUnloaded(DL, ID);

            DL.Close();
        }
        private void LoadAxiomXML(int index)
        {
            XMLDocumentLoader DL = _X.Document.XDL;
            DL.Navigate(fileID);
            DL.SetContext(_X);
            DL.SetDefinition(this);

            XmlNode axioms = DL.currentNode.GetChildNode("axioms");
            int aCount = axioms.Count();
            if (index >= aCount) { throw new Exception(); }

            XmlNode ax = axioms.GetChildNode("statement", index);
            Axioms[index] = MStatement.GetUnloaded(DL, ax, ax.ID(DL));
        }

        private MStatement LoadAxiom(FileID ID)
        {
            if (_X.Document.IsXML) { return LoadAxiomXML(ID); }

            DocumentLoader DL = _X.Document.DL;
            int index = DL.Navigate(ID);
            DL.SetContext(_X);
            DL.SetDefinition(this);
            Axioms[index] = MStatement.GetUnloaded(DL, ID);
            DL.Close();
            return Axioms[index];
        }
        private MStatement LoadAxiomXML(FileID ID)
        {
            XMLDocumentLoader DL = _X.Document.XDL;
            int index = DL.Navigate(ID);
            DL.SetContext(_X);
            DL.SetDefinition(this);
            Axioms[index] = MStatement.GetUnloaded(DL, DL.currentNode, ID);
            return Axioms[index];
        }
    }

    public partial class MLogicConnective : MDefinition
    {
        public override void ToStream(DocumentLoader DL)
        {
            base.ToStream(DL);
        }
    }

    public partial class MFunctionSymbol : MLogicConnective
    {
        public override void ToStream(DocumentLoader DL)
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");

            DL.BeginSubStream();
            DL.Write((byte)1);
            fileID.ToStream(DL);

            DL.BeginSubStream();
            DL.Write((byte)argumentCount);
            DL.Write((byte)boundVarCount);
            DL.Write((byte)formulaCount);
            DL.Write(stringSymbol);
            DL.EndSubStream();

            base.ToStream(DL);
        }

        public override void ToXML(XMLDocumentLoader DL)
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");

            DL.Xwr.WriteStartElement("definition");
            DL.Xwr.WriteAttributeString("type", "1");
            DL.Xwr.WriteAttributeString("id", fileID.ToXML(DL.Document));

            DL.Xwr.WriteStartElement("functionsymbol");
            DL.Xwr.WriteAttributeString("argcount", argumentCount.ToString());
            DL.Xwr.WriteAttributeString("boundvarcount", boundVarCount.ToString());
            DL.Xwr.WriteAttributeString("formulacount", formulaCount.ToString());
            DL.Xwr.WriteString(stringSymbol);
            DL.Xwr.WriteEndElement(); //function symbol

            base.ToXML(DL);
        }

        public new static MFunctionSymbol FromStream(DocumentLoader DL)
        {
            DefinitionFileID fileID = FileID.FromStream(DL) as DefinitionFileID;
            DL.Remember();
            return new MFunctionSymbol(fileID, DL.Context, DL.ReadByte(), DL.ReadByte(), DL.ReadByte(), DL.ReadString());
        }
        public new static MFunctionSymbol FromStream(XMLDocumentLoader DL, XmlNode node)
        {
            DefinitionFileID fileID = FileID.FromStream(DL, node.GetAttr("id")) as DefinitionFileID;
            XmlNode functionsymbol = node.GetChildNode("functionsymbol");
            return new MFunctionSymbol(fileID, DL.Context, 
                functionsymbol.GetAttrInt("argcount"), 
                functionsymbol.GetAttrInt("boundvarcount"),
                functionsymbol.GetAttrInt("formulacount"), 
                functionsymbol.InnerText);
        }
    }

    public partial class MBinaryConnective : MLogicConnective
    {
        public override void ToStream(DocumentLoader DL)
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");

            DL.BeginSubStream();
            DL.Write((byte)3);
            fileID.ToStream(DL);

            DL.BeginSubStream();
            byte b = 0;
            if (_tt) b += 1;
            if (_tf) b += 2;
            if (_ft) b += 4;
            if (_ff) b += 8;

            DL.Write(b);

            DL.Write(stringSymbol);
            DL.EndSubStream();

            base.ToStream(DL);
        }

        public override void ToXML(XMLDocumentLoader DL)
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");

            DL.Xwr.WriteStartElement("definition");
            DL.Xwr.WriteAttributeString("type", "3");
            DL.Xwr.WriteAttributeString("id", fileID.ToXML(DL.Document));

            DL.Xwr.WriteStartElement("bincon");
            byte b = 0;
            if (_tt) b += 1;
            if (_tf) b += 2;
            if (_ft) b += 4;
            if (_ff) b += 8;
            DL.Xwr.WriteAttributeString("truthtable", b.ToString("X"));
            DL.Xwr.WriteString(stringSymbol);
            DL.Xwr.WriteEndElement(); //bincon

            base.ToXML(DL);
        }

        public new static MBinaryConnective FromStream(DocumentLoader DL)
        {
            DefinitionFileID fileID = FileID.FromStream(DL) as DefinitionFileID;

            DL.Remember();
            byte b = DL.ReadByte();

            bool tt = 0 != (1 & b);
            bool tf = 0 != (2 & b);
            bool ft = 0 != (4 & b);
            bool ff = 0 != (8 & b);

            return new MBinaryConnective(fileID, DL.Context, tt, tf, ft, ff, DL.ReadString());
        }
        public new static MBinaryConnective FromStream(XMLDocumentLoader DL, XmlNode node)
        {
            DefinitionFileID fileID = FileID.FromStream(DL, node.GetAttr("id")) as DefinitionFileID;
            XmlNode bincon = node.GetChildNode("bincon");
            byte b = byte.Parse(bincon.GetAttr("truthtable"), System.Globalization.NumberStyles.HexNumber);
            bool tt = 0 != (1 & b);
            bool tf = 0 != (2 & b);
            bool ft = 0 != (4 & b);
            bool ff = 0 != (8 & b);
            return new MBinaryConnective(fileID, DL.Context, tt, tf, ft, ff, bincon.InnerText);
        }
    }

    public partial class MQuantifier : MLogicConnective
    {
        public override void ToStream(DocumentLoader DL)
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");

            DL.BeginSubStream();
            DL.Write((byte)4);
            fileID.ToStream(DL);

            DL.BeginSubStream();
            DL.Write(termCount);
            DL.Write(boundVarCount);
            DL.Write(formulaCount);
            DL.Write(stringSymbol);
            DL.Write((byte)type);
            DL.EndSubStream();

            base.ToStream(DL);
        }

        public override void ToXML(XMLDocumentLoader DL)
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");

            DL.Xwr.WriteStartElement("definition");
            DL.Xwr.WriteAttributeString("type", "4");
            DL.Xwr.WriteAttributeString("id", fileID.ToXML(DL.Document));

            DL.Xwr.WriteStartElement("quantifier");
            DL.Xwr.WriteAttributeString("type", ((byte)type).ToString());
            DL.Xwr.WriteAttributeString("termcount", termCount.ToString());
            DL.Xwr.WriteAttributeString("boundvarcount", boundVarCount.ToString());
            DL.Xwr.WriteAttributeString("formulacount", formulaCount.ToString());
            DL.Xwr.WriteString(stringSymbol);
            DL.Xwr.WriteEndElement(); //quantifier

            base.ToXML(DL);
        }

        public new static MQuantifier FromStream(DocumentLoader DL)
        {
            DefinitionFileID fileID = FileID.FromStream(DL) as DefinitionFileID;
            DL.Remember();
            MQuantifier Q = new MQuantifier(fileID, DL.Context, DL.ReadInt(), DL.ReadInt(), DL.ReadInt(), DL.ReadString())
            { type = (QuantifierType)DL.ReadByte() };
            return Q;
        }
        public new static MQuantifier FromStream(XMLDocumentLoader DL, XmlNode node)
        {
            DefinitionFileID fileID = FileID.FromStream(DL, node.GetAttr("id")) as DefinitionFileID;
            XmlNode quantifier = node.GetChildNode("quantifier");
            return new MQuantifier(fileID, DL.Context,
                quantifier.GetAttrInt("termcount"),
                quantifier.GetAttrInt("boundvarcount"),
                quantifier.GetAttrInt("formulacount"),
                quantifier.InnerText)
            { type = (QuantifierType)(quantifier.GetAttrInt("type")) };
        }
    }

    public partial class MNegationDefinition : MLogicConnective
    {
        public override void ToStream(DocumentLoader DL)
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");

            DL.BeginSubStream();
            DL.Write((byte)5);
            fileID.ToStream(DL);

            DL.BeginSubStream();
            DL.Write(stringSymbol);
            DL.EndSubStream();

            base.ToStream(DL);
        }

        public override void ToXML(XMLDocumentLoader DL)
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");

            DL.Xwr.WriteStartElement("definition");
            DL.Xwr.WriteAttributeString("type", "5");
            DL.Xwr.WriteAttributeString("id", fileID.ToXML(DL.Document));

            DL.Xwr.WriteStartElement("negation");
            DL.Xwr.WriteString(stringSymbol);
            DL.Xwr.WriteEndElement(); //negation

            base.ToXML(DL);
        }

        public new static MNegationDefinition FromStream(DocumentLoader DL)
        {
            DefinitionFileID fileID = FileID.FromStream(DL) as DefinitionFileID;
            DL.Remember();
            MNegationDefinition N = new MNegationDefinition(fileID, DL.Context, DL.ReadString());

            return N;
        }
        public new static MNegationDefinition FromStream(XMLDocumentLoader DL, XmlNode node)
        {
            DefinitionFileID fileID = FileID.FromStream(DL, node.GetAttr("id")) as DefinitionFileID;
            XmlNode negation = node.GetChildNode("negation");
            return new MNegationDefinition(fileID, DL.Context, negation.InnerText);
        }
    }

    public partial class MEqualityDefinition : MDefinition
    {
        public override void ToStream(DocumentLoader DL)
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");

            DL.BeginSubStream();
            DL.Write((byte)6);
            fileID.ToStream(DL);

            DL.BeginSubStream();
            DL.Write(stringSymbol);
            DL.EndSubStream();

            base.ToStream(DL);
        }

        public override void ToXML(XMLDocumentLoader DL)
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");

            DL.Xwr.WriteStartElement("definition");
            DL.Xwr.WriteAttributeString("type", "6");
            DL.Xwr.WriteAttributeString("id", fileID.ToXML(DL.Document));

            DL.Xwr.WriteStartElement("equality");
            DL.Xwr.WriteString(stringSymbol);
            DL.Xwr.WriteEndElement(); //equality

            base.ToXML(DL);
        }

        public new static MEqualityDefinition FromStream(DocumentLoader DL)
        {
            DefinitionFileID fileID = FileID.FromStream(DL) as DefinitionFileID;
            DL.Remember();
            MEqualityDefinition EQ = new MEqualityDefinition(fileID, DL.Context, DL.ReadString());

            return EQ;
        }
        public new static MEqualityDefinition FromStream(XMLDocumentLoader DL, XmlNode node)
        {
            DefinitionFileID fileID = FileID.FromStream(DL, node.GetAttr("id")) as DefinitionFileID;
            XmlNode equality = node.GetChildNode("equality");
            return new MEqualityDefinition(fileID, DL.Context, equality.InnerText);
        }
    }
    
}
