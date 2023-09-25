using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System;

namespace TraceBackend
{
    public class ProtoDocumentLoader
    {
        public MDocument Document;
        public MContext Context;
        public MDefinition Definition;
        public MTheorem Theorem;
        public MDeduction Deduction;
        public MDeductionStep DeductionStep;

        public void SetContext(MContext X) { Context = X; Definition = null; Theorem = null; Deduction = null; DeductionStep = null; }
        public void SetDefinition(MDefinition D) { Definition = D; Theorem = null; Deduction = null; DeductionStep = null; }
        public void SetTheorem(MTheorem T) { Theorem = T; Definition = null; Deduction = null; DeductionStep = null; }
        public void SetDeduction(MDeduction D) { Deduction = D; DeductionStep = null; Definition = null; }
        public void SetDeductionStep(MDeductionStep DS) { DeductionStep = DS; Definition = null; }

        public virtual int Navigate(FileID ID)
        {
            throw new Exception();
        }
    }

    public class XMLDocumentLoader : ProtoDocumentLoader
    {
        public XmlWriter Xwr;
        public XmlNode currentNode;

        public XMLDocumentLoader(MDocument d, XmlWriter XWR = null)
        {
            Document = d;
            Xwr = XWR;
        }

        public void StartEl(string localName, string ns) { Xwr.WriteStartElement(localName, ns); }
        public void StartEl(string localName) { Xwr.WriteStartElement(localName); }
        public void EndEl() { Xwr.WriteEndElement(); }
        public void Attr(string localName, string value) { Xwr.WriteAttributeString(localName, value); }

        public override int Navigate(FileID ID)
        {
            bool found = true;
            int ind = 0;
            XmlNode node = Document.XmlD.GetChildNode("backend");
            
            if (ID is ContextFileID contextID)
            {
                NavigateContext(contextID);
            }

            currentNode = node;

            if (found) return ind;
            return 0;


            void NavigateContext(ContextFileID cID)
            {
                found = false;
                node = node.GetChildNode("contexts");

                int ccount = node.GetAttrInt("count");
                for (int i = 0; i < ccount; i++)
                {
                    ind = i;
                    node = node.GetChildNode("context", i);
                    ContextFileID foundCID = FileID.FromStream(this, node.GetAttr("id")) as ContextFileID;
                    if (foundCID.contextFileID == cID.contextFileID)
                    {
                        found = true;

                        if (cID is VariableFileID variableID)
                        {
                            NavigateVariable(variableID);
                        }

                        if (cID is DefinitionFileID defintionID)
                        {
                            NavigateDefinition(defintionID);
                        }

                        if (cID is ContextStatementFileID contextstatementID)
                        {
                            NavigateContextStatement(contextstatementID);
                        }

                        if (cID is TheoremFileID theoremID)
                        {
                            NavigateTheorem(theoremID);
                        }

                        break;
                    }
                    else
                    {
                        node = node.ParentNode;
                    }
                }
            }

            void NavigateVariable(VariableFileID vID)
            {
                found = false;
                node = node.GetChildNode("variables");
                int vcount = node.GetAttrInt("count");
                for (int i = 0; i < vcount; i++)
                {
                    ind = i;
                    node = node.GetChildNode("variable", i);
                    VariableFileID foundDID = FileID.FromStream(this, node.GetAttr("id")) as VariableFileID;
                    if (foundDID.variableFileID == vID.variableFileID)
                    {
                        found = true;
                        break;
                    }
                    else
                    {
                        node = node.ParentNode;
                    }
                }
            }

            void NavigateDefinition(DefinitionFileID dID)
            {
                found = false;
                node = node.GetChildNode("definitions");
                int dcount = node.GetAttrInt("count");
                for (int i = 0; i < dcount; i++)
                {
                    ind = i;
                    node = node.GetChildNode("definition", i);
                    DefinitionFileID foundDID = FileID.FromStream(this, node.GetAttr("id")) as DefinitionFileID;
                    if (foundDID.definitionFileID == dID.definitionFileID)
                    {
                        found = true;

                        if (dID is DefinitionStatementFileID definitionstatementID)
                        {
                            NavigateDefinitionStatement(definitionstatementID);
                        }

                        break;
                    }
                    else
                    {
                        node = node.ParentNode;
                    }
                }
            }

            void NavigateDefinitionStatement(DefinitionStatementFileID dsID)
            {
                found = false;
                node = node.GetChildNode("axioms");
                int dscount = node.GetAttrInt("count");
                for (int i = 0; i < dscount; i++)
                {
                    ind = i;
                    node = node.GetChildNode("statement", i);
                    DefinitionStatementFileID foundDID = FileID.FromStream(this, node.GetAttr("id")) as DefinitionStatementFileID;
                    if (foundDID.definitionStatementFileID == dsID.definitionStatementFileID)
                    {
                        found = true;
                        break;
                    }
                    else
                    {
                        node = node.ParentNode;
                    }
                }
            }

            void NavigateContextStatement(ContextStatementFileID csID)
            {
                found = false;
                node = node.GetChildNode("axioms");
                int cscount = node.GetAttrInt("count");
                for (int i = 0; i < cscount; i++)
                {
                    ind = i;
                    node = node.GetChildNode("statement", i);
                    ContextStatementFileID foundDID = FileID.FromStream(this, node.GetAttr("id")) as ContextStatementFileID;
                    if (foundDID.contextStatementFileID == csID.contextStatementFileID)
                    {
                        found = true;
                        break;
                    }
                    else
                    {
                        node = node.ParentNode;
                    }
                }
            }

            void NavigateTheorem(TheoremFileID tID)
            {
                found = false;
                node = node.GetChildNode("theorems");
                int tcount = node.GetAttrInt("count");
                for (int i = 0; i < tcount; i++)
                {
                    ind = i;
                    node = node.GetChildNode("theorem", i);
                    TheoremFileID foundDID = FileID.FromStream(this, node.GetAttr("id")) as TheoremFileID;
                    if (foundDID.theoremFileID == tID.theoremFileID)
                    {
                        found = true;

                        if (tID is TheoremStatementFileID theoremstatementID)
                        {
                            NavigateTheoremStatement(theoremstatementID);
                        }

                        if (tID is DeductionFileID deductionID)
                        {
                            NavigateDeduction(deductionID);
                        }

                        break;
                    }
                    else
                    {
                        node = node.ParentNode;
                    }
                }
            }

            void NavigateTheoremStatement(TheoremStatementFileID tsID)
            {
                found = false;
                node = node.GetChildNode("statement");
                TheoremStatementFileID foundDID = FileID.FromStream(this, node.GetAttr("id")) as TheoremStatementFileID;
                if (foundDID.theoremStatementFileID == tsID.theoremStatementFileID)
                {
                    found = true;
                }
                else
                {
                    node = node.ParentNode;
                }
            }

            void NavigateDeduction(DeductionFileID dID)
            {
                found = false;
                if (!node.GetAttrBool("hasdeduction")) {  return; } // Theorem has no Deduction
                node = node.GetChildNode("deduction");
                
                DeductionFileID foundDID = FileID.FromStream(this, node.GetAttr("id")) as DeductionFileID;

                if (foundDID.deductionFileID == dID.deductionFileID)
                {
                    found = true;

                    if (dID is DeductionStepFileID deductionstepID)
                    {
                        NavigateDeductionStep(deductionstepID);
                    }
                }
                else
                {
                    node = node.ParentNode;
                }
            }

            void NavigateDeductionStep(DeductionStepFileID dsID)
            {
                found = false;
                int dscount = node.GetAttrInt("stepcount");
                for (int i = 0; i < dscount; i++)
                {
                    ind = i;
                    node = node.GetChildNode("step", i);
                    DeductionStepFileID foundDID = FileID.FromStream(this, node.GetAttr("id")) as DeductionStepFileID;
                    if (foundDID.deductionStepFileID == dsID.deductionStepFileID)
                    {
                        found = true;

                        if (dsID is DeductionStepStatementFileID deductionstepstatementID)
                        {
                            NavigateDeductionStepStatement(deductionstepstatementID);
                        }

                        break;
                    }
                    else
                    {
                        node = node.ParentNode;
                    }
                }
            }

            void NavigateDeductionStepStatement(DeductionStepStatementFileID dssID)
            {
                found = false;
                node = node.GetChildNode("consequence").GetChildNode("statement");
                DeductionStepStatementFileID foundDID = FileID.FromStream(this, node.GetAttr("id")) as DeductionStepStatementFileID;
                if (foundDID.deductionStepStatementFileID == dssID.deductionStepStatementFileID)
                {
                    found = true;
                }
                else
                {
                    node = node.ParentNode.ParentNode;
                }
            }
        }
    }

    public class DocumentLoader : ProtoDocumentLoader
    {
        public RWMode Mode => Reader == null ? RWMode.Writing : RWMode.Reading;
        private int openCount;
        private List<long> previousPositions;

        private BinaryReader Reader;
        private BinaryWriter Writer;
        private List<BinaryWriter> SubStreams;
        public List<MUndefinedPredicate> PDList;

        uint rememberposition;
        uint rememberlength;

        public DocumentLoader(MDocument d)
        {
            Document = d;
            SubStreams = new List<BinaryWriter>();
            previousPositions = new List<long>();
        }

        public void Open(RWMode mode = RWMode.Reading)
        {
            switch (mode)
            {
                case RWMode.Reading:
                    if (Mode == RWMode.Writing && openCount != 0)
                        throw new IOException("DocumentLoader is in Writing Mode, cannot open a stream for reading.");
                    if(openCount == 0)
                    {
                        Reader = new BinaryReader(new FileStream(Document.FilePath, FileMode.Open));
                    }
                    else
                    {
                        previousPositions.Add(Reader.BaseStream.Position);
                    }
                    openCount++;
                    Reader.BaseStream.Position = 0;
                    break;
                case RWMode.Writing:
                    if (Mode == RWMode.Reading && openCount != 0)
                        throw new IOException("DocumentLoader is in Reading Mode, cannot open a stream for writing.");
                    if (openCount == 0)
                    {
                        Writer = new BinaryWriter(new FileStream(Document.FilePath, FileMode.OpenOrCreate));
                    }
                    openCount++;
                    break;
            }
        }

        public void Close()
        {
            if (openCount == 0) return;

            openCount--;
            if (openCount == 0)
            {
                if (Mode == RWMode.Reading)
                {
                    Reader.Close();
                    Reader = null;
                }
                else
                {
                    Writer.Close();
                    Writer = null;
                }
            }
            else if (Mode == RWMode.Reading)
            {
                Reader.BaseStream.Position = previousPositions[previousPositions.Count-1];
                previousPositions.RemoveAt(previousPositions.Count - 1);
            }
        }

        public override int Navigate(FileID ID) 
        {
            bool found = false;
            Open();
            int ind = 0;

            ReadUShort(); //Version
            ReadUInt(); //DocumentLength

            found = true;
            if (ID is ContextFileID contextID)
            {
                NavigateContext(contextID);
            }

            if (found) return ind;
            return 0;


            void NavigateContext(ContextFileID cID)
            {
                found = false;
                CleanSkip(); //Referenced Documents
                CleanSkip(); //IDManager

                int ccount = ReadInt();
                for (int i = 0; i < ccount; i++)
                {
                    ind = i;
                    Remember();
                    ContextFileID foundCID = FileID.FromStream(this) as ContextFileID;
                    if (foundCID.contextFileID == cID.contextFileID)
                    {
                        found = true;

                        if(cID is VariableFileID variableID)
                        {
                            NavigateVariable(variableID);
                        }

                        if (cID is DefinitionFileID defintionID)
                        {
                            NavigateDefinition(defintionID);
                        }

                        if (cID is ContextStatementFileID contextstatementID)
                        {
                            NavigateContextStatement(contextstatementID);
                        }

                        if (cID is TheoremFileID theoremID)
                        {
                            NavigateTheorem(theoremID);
                        }

                        break;
                    }
                    else
                    {
                        Skip();
                    }
                }

            }

            void NavigateVariable(VariableFileID vID)
            {
                found = false;
                CleanSkip(); //SuperContexts

                Remember(); //Variables
                uint vcount = ReadUInt();
                for (int i = 0; i < vcount; i++)
                {
                    ind = i;
                    Remember();
                    VariableFileID foundDID = FileID.FromStream(this) as VariableFileID;
                    if (foundDID.variableFileID == vID.variableFileID)
                    {
                        found = true;
                        FullReturn();
                        break;
                    }
                    else
                    {
                        Skip();
                    }
                }
            }

            void NavigateDefinition(DefinitionFileID dID)
            {
                found = false;
                CleanSkip(); //SuperContexts
                CleanSkip(); //Variables

                Remember(); //Definitions
                ushort dcount = ReadUShort();
                for (int i = 0; i < dcount; i++)
                {
                    ind = i;
                    Remember();
                    ReadByte(); //DefinitionType
                    DefinitionFileID foundDID = FileID.FromStream(this) as DefinitionFileID;
                    if (foundDID.definitionFileID == dID.definitionFileID)
                    {
                        found = true;

                        if (dID is DefinitionStatementFileID definitionstatementID)
                        {
                            NavigateDefinitionStatement(definitionstatementID);
                        }
                        else
                        {
                            Return();
                        }

                        break;
                    }
                    else
                    {
                        Skip();
                    }
                }
            }

            void NavigateDefinitionStatement(DefinitionStatementFileID dsID)
            {
                found = false;
                CleanSkip(); //IndividualStuff

                Remember();
                byte dscount = ReadByte();
                for (int i = 0; i < dscount; i++)
                {
                    ind = i;
                    Remember();
                    DefinitionStatementFileID foundDID = FileID.FromStream(this) as DefinitionStatementFileID;
                    if (foundDID.definitionStatementFileID == dsID.definitionStatementFileID)
                    {
                        found = true;
                        break;
                    }
                    else
                    {
                        Skip();
                    }
                }
            }

            void NavigateContextStatement(ContextStatementFileID csID)
            {
                found = false;
                CleanSkip(); //SuperContexts
                CleanSkip(); //Variables
                CleanSkip(); //Definitions

                Remember(); //Axioms
                ushort cscount = ReadUShort();
                for (int i = 0; i < cscount; i++)
                {
                    ind = i;
                    Remember();
                    ContextStatementFileID foundDID = FileID.FromStream(this) as ContextStatementFileID;
                    if (foundDID.contextStatementFileID == csID.contextStatementFileID)
                    {
                        found = true;
                        break;
                    }
                    else
                    {
                        Skip();
                    }
                }
            }

            void NavigateTheorem (TheoremFileID tID)
            {
                found = false;
                CleanSkip(); //SuperContexts
                CleanSkip(); //Variables
                CleanSkip(); //Definitions
                CleanSkip(); //Axioms

                Remember(); //Theorems
                ushort tcount = ReadUShort();
                for (int i = 0; i < tcount; i++)
                {
                    ind = i;
                    Remember();
                    TheoremFileID foundDID = FileID.FromStream(this) as TheoremFileID;
                    if (foundDID.theoremFileID == tID.theoremFileID)
                    {
                        found = true;

                        if(tID is TheoremStatementFileID theoremstatementID)
                        {
                            NavigateTheoremStatement(theoremstatementID);
                        }

                        if(tID is DeductionFileID deductionID)
                        {
                            NavigateDeduction(deductionID);
                        }

                        break;
                    }
                    else
                    {
                        Skip();
                    }
                }
            }
            
            void NavigateTheoremStatement(TheoremStatementFileID tsID)
            {
                found = false;
                Remember();
                TheoremStatementFileID foundDID = FileID.FromStream(this) as TheoremStatementFileID;
                if (foundDID.theoremStatementFileID == tsID.theoremStatementFileID)
                {
                    found = true;
                }
                else
                {
                    Skip();
                }
            }

            void NavigateDeduction(DeductionFileID dID)
            {
                found = false;
                CleanSkip(); //Statement
                if (!ReadBool()) { found = false; return; } // Theorem has no Deduction
                
                Remember();
                DeductionFileID foundDID = FileID.FromStream(this) as DeductionFileID;
                
                if (foundDID.deductionFileID == dID.deductionFileID)
                {
                    found = true;

                    if(dID is DeductionStepFileID deductionstepID)
                    {
                        NavigateDeductionStep(deductionstepID);
                    }
                }
                else
                {
                    Skip();
                }
            }

            void NavigateDeductionStep(DeductionStepFileID dsID)
            {
                found = false;
                uint dscount = ReadUShort();
                for (int i = 0; i < dscount; i++)
                {
                    ind = i;
                    Remember();
                    ReadByte(); //DeductionStepType
                    DeductionStepFileID foundDID = FileID.FromStream(this) as DeductionStepFileID;
                    if (foundDID.deductionStepFileID == dsID.deductionStepFileID)
                    {
                        found = true;

                        if (dsID is DeductionStepStatementFileID deductionstepstatementID)
                        {
                            NavigateDeductionStepStatement(deductionstepstatementID);
                        }
                        else
                        {
                            Return();
                        }

                        break;
                    }
                    else
                    {
                        Skip();
                    }
                }
            }

            void NavigateDeductionStepStatement(DeductionStepStatementFileID dssID)
            {
                found = false;
                ReadBool(); //reduce
                FileID.FromStream(this); //FileID of premise

                Remember();
                DeductionStepStatementFileID foundDID = FileID.FromStream(this) as DeductionStepStatementFileID;
                if (foundDID.deductionStepStatementFileID == dssID.deductionStepStatementFileID)
                {
                    found = true;
                }
                else
                {
                    Skip();
                }
            }
        }

        public uint Remember()
        {
            rememberlength = ReadUInt();
            rememberposition = (uint)Reader.BaseStream.Position;
            return rememberposition + rememberlength;
        }
        public void Skip()
        {
            JumpTo(rememberposition + rememberlength);
        }
        public void JumpTo(uint Position)
        {
            Reader.BaseStream.Position = Position;
        }
        public void CleanSkip()
        {
            Remember();
            Skip();
        }
        public void Return()
        {
            Reader.BaseStream.Position = rememberposition;
        }
        public void FullReturn()
        {
            Reader.BaseStream.Position = rememberposition-4;
        }

        public void BeginSubStream()
        {
            MemoryStream s = new MemoryStream();
            BinaryWriter w = new BinaryWriter(s, Encoding.Default, true);
            SubStreams.Add(Writer);
            Writer = w;
        }

        public void EndSubStream()
        {
            // write length prefix
            SubStreams[SubStreams.Count - 1].Write((uint)Writer.BaseStream.Length);

            (Writer.BaseStream as MemoryStream).WriteTo(SubStreams[SubStreams.Count - 1].BaseStream);

            Writer.BaseStream.Close();
            Writer.Close();

            Writer = SubStreams[SubStreams.Count - 1];
            SubStreams.Remove(Writer);
        }

        public string ReadAscii() { return Reader.ReadString(); }
        public string ReadString()
        {
            int l = Reader.ReadUInt16();
            return Encoding.UTF8.GetString(Reader.ReadBytes(l));
        }
        public int ReadInt() { return Reader.ReadInt32(); }
        public uint ReadUInt() { return Reader.ReadUInt32(); }
        public float ReadFloat() { return Reader.ReadSingle(); }
        public short ReadShort() { return Reader.ReadInt16(); }
        public ushort ReadUShort() { return Reader.ReadUInt16(); }
        public byte ReadByte() { return Reader.ReadByte(); }
        public bool ReadBool() { return Reader.ReadBoolean(); }

        public void Write(string s)
        {
            Write((ushort) Encoding.UTF8.GetByteCount(s)); // ushort allows for 65k bytes, so >30k symbols
            Writer.Write(Encoding.UTF8.GetBytes(s));
        }
        public void Write(int i) { Writer.Write(i); }
        public void Write(uint i) { Writer.Write(i); }
        public void Write(float f) { Writer.Write(f); }
        public void Write(short s) { Writer.Write(s); }
        public void Write(ushort u) { Writer.Write(u); }
        public void Write(byte b) { Writer.Write(b); }
        public void Write(bool b) { Writer.Write(b); }

        public enum RWMode
        {
            Reading = 0,
            Writing = 1
        }
    }

    public static class XmlNodeExtensions
    {
        public static bool HasChildNode(this XmlNode node, string name)
        {
            for (int i = 0; i < node.ChildNodes.Count; i++)
                if (node.ChildNodes[i].LocalName == name) return true;
            return false;
        }
        public static bool HasChildNode(this XmlNode node, string name, string attr, string val)
        {
            for (int i = 0; i < node.ChildNodes.Count; i++)
                if (node.ChildNodes[i].LocalName == name
                    && node.ChildNodes[i].Attributes[attr] != null
                    && node.ChildNodes[i].Attributes[attr].Value == val)
                    return true;
            return false;
        }
        public static bool HasChildNode(this XmlNode node, string attr, string val)
        {
            for (int i = 0; i < node.ChildNodes.Count; i++)
                if (node.ChildNodes[i].Attributes[attr] != null
                    && node.ChildNodes[i].Attributes[attr].Value == val)
                    return true;
            return false;
        }

        public static XmlNode GetChildNode(this XmlNode node, string name)
        {
            for (int i = 0; i < node.ChildNodes.Count; i++)
                if (node.ChildNodes[i].LocalName == name) return node.ChildNodes[i];
            throw new System.Exception();
        }
        public static XmlNode GetChildNode(this XmlNode node, string name, string attr, string val)
        {
            for (int i = 0; i < node.ChildNodes.Count; i++)
                if (node.ChildNodes[i].LocalName == name
                    && node.ChildNodes[i].Attributes[attr] != null
                    && node.ChildNodes[i].Attributes[attr].Value == val)
                    return node.ChildNodes[i];
            throw new System.Exception();
        }
        public static XmlNode GetChildNode(this XmlNode node, string attr, string val)
        {
            for (int i = 0; i < node.ChildNodes.Count; i++)
                if (node.ChildNodes[i].Attributes[attr] != null
                    && node.ChildNodes[i].Attributes[attr].Value == val)
                    return node.ChildNodes[i];
            throw new System.Exception();
        }
        public static XmlNode GetChildNode(this XmlNode node, string name, int index)
        {
            for (int i = 0; i < node.ChildNodes.Count; i++)
                if (node.ChildNodes[i].LocalName == name)
                {
                    if (index == 0)
                        return node.ChildNodes[i];
                    index--;
                }
            throw new System.Exception();
        }

        public static string GetAttr(this XmlNode node, string attr) { return node.Attributes[attr].Value; }
        public static int GetAttrInt(this XmlNode node, string attr) { return Convert.ToInt32(node.Attributes[attr].Value); }
        public static float GetAttrFloat(this XmlNode node, string attr) { return Convert.ToSingle(node.Attributes[attr].Value); }
        public static bool GetAttrBool(this XmlNode node, string attr) { return bool.Parse(node.Attributes[attr].Value); }
        public static bool HasAttr(this XmlNode node, string attr) { try { string s = node.Attributes[attr].Value; return true; } catch (System.Exception ex) { return false; } }

        //special attributes

        public static int Count (this XmlNode node) { return GetAttrInt(node, "count"); }
        public static FileID ID (this XmlNode node, XMLDocumentLoader DL) { return FileID.FromStream(DL, node.GetAttr("id")); }
    }
}
