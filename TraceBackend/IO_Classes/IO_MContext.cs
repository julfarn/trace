using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TraceBackend
{
    public partial class MContext
    {
        public override void ToStream(DocumentLoader DL)
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");
            /*
                1 - SuperContexts
                2 - Variables
                3 - Definitions
                4 - Axioms
                5 - Theorems
            */

            DL.BeginSubStream();

            fileID.ToStream(DL);

            DL.BeginSubStream();
            DL.Write((uint)SuperContexts.Count);
            foreach (MContext sc in SuperContexts)
            {
                DL.BeginSubStream();
                sc.fileID.ToStream(DL);
                DL.EndSubStream();
            }
            DL.EndSubStream();

            DL.BeginSubStream();
            DL.Write((uint)Variables.Count);
            foreach (MVariable v in Variables)
                v.DataToStream(DL);
            DL.EndSubStream();

            DL.BeginSubStream();
            DL.Write((ushort)Definitions.Count);
            foreach (MDefinition d in Definitions)
                d.ToStream(DL);
            DL.EndSubStream();

            DL.BeginSubStream();
            DL.Write((ushort)Axioms.Count);
            foreach (MStatement a in Axioms)
                a.ToStream(DL);
            DL.EndSubStream();

            DL.BeginSubStream();
            DL.Write((ushort)Theorems.Count);
            foreach (MTheorem t in Theorems)
                t.ToStream(DL);
            DL.EndSubStream();

            DL.EndSubStream();
        }

        public override void ToXML(XMLDocumentLoader DL)
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");
            /*
                1 - SuperContexts
                2 - Variables
                3 - Definitions
                4 - Axioms
                5 - Theorems
            */

            DL.Xwr.WriteStartElement("context");
            DL.Xwr.WriteAttributeString("id", fileID.ToXML(Document));

            DL.Xwr.WriteStartElement("supercontexts");
            DL.Xwr.WriteAttributeString("count", SuperContexts.Count.ToString());
            foreach (MContext sc in SuperContexts)
            {
                DL.Xwr.WriteElementString("sc", sc.fileID.ToXML(Document));
            }
            DL.Xwr.WriteEndElement(); //supercontexts

            DL.Xwr.WriteStartElement("variables");
            DL.Xwr.WriteAttributeString("count", Variables.Count.ToString());
            foreach (MVariable v in Variables)
                v.DataToXML(DL);
            DL.Xwr.WriteEndElement(); //variables

            DL.Xwr.WriteStartElement("definitions");
            DL.Xwr.WriteAttributeString("count", Definitions.Count.ToString());
            foreach (MDefinition d in Definitions)
                d.ToXML(DL);
            DL.Xwr.WriteEndElement(); //definitions

            DL.Xwr.WriteStartElement("axioms");
            DL.Xwr.WriteAttributeString("count", Axioms.Count.ToString());
            foreach (MStatement a in Axioms)
                a.ToXML(DL);
            DL.Xwr.WriteEndElement(); //axioms

            DL.Xwr.WriteStartElement("theorems");
            DL.Xwr.WriteAttributeString("count", Theorems.Count.ToString());
            foreach (MTheorem t in Theorems)
                t.ToXML(DL);
            DL.Xwr.WriteEndElement(); //theorems

            DL.Xwr.WriteEndElement(); // context
        }

        public new static MContext FromStream(DocumentLoader DL)
        {
            //prefix
            DL.Remember();

            ContextFileID fileID = FileID.FromStream(DL) as ContextFileID;

            MContext X = new MContext(fileID)
            {
                Axioms = new List<MStatement>(),
                Variables = new List<MVariable>(),
                Definitions = new List<MDefinition>(),
                SuperContexts = new List<MContext>(),
                Theorems = new List<MTheorem>(),
                loaded = true
            };
            DL.SetContext(X);

            DL.Remember();
            uint scCount = DL.ReadUInt();
            for (int i = 0; i < scCount; i++)
            {
                DL.Remember();
                X.SuperContexts.Add((FileID.FromStream(DL) as ContextFileID).FindContext(DL));
            }

            DL.Remember();
            uint vCount = DL.ReadUInt();
            for (int i = 0; i < vCount; i++)
                X.AddVariable(MVariable.DataFromStream(DL));

            DL.Remember();
            ushort dCount = DL.ReadUShort();
            for (int i = 0; i < dCount; i++)
                X.AddDefinition(MDefinition.FromStream(DL));

            DL.Remember();
            ushort aCount = DL.ReadUShort();
            for (int i = 0; i < aCount; i++)
                X.AddAxiom(MStatement.FromStream(DL));

            foreach (MVariable v in X.Variables)
                v.FetchProperAxioms(DL);

            DL.Remember();
            ushort tCount = DL.ReadUShort();
            for (int i = 0; i < tCount; i++)
                X.AddTheorem(MTheorem.FromStream(DL));

            foreach (MVariable v in X.Variables)
                v.FetchTheoremAxioms(DL);

            return X;
        }

        public new static MContext FromStream(XMLDocumentLoader DL, XmlNode node)
        {
            ContextFileID fileID = FileID.FromStream(DL, node.GetAttr("id")) as ContextFileID;

            MContext X = new MContext(fileID)
            {
                Axioms = new List<MStatement>(),
                Variables = new List<MVariable>(),
                Definitions = new List<MDefinition>(),
                SuperContexts = new List<MContext>(),
                Theorems = new List<MTheorem>(),
                loaded = true
            };
            DL.SetContext(X);

            XmlNode supercontexts = node.GetChildNode("supercontexts");
            uint scCount = (uint)supercontexts.GetAttrInt("count");
            for (int i = 0; i < scCount; i++)
            {
                XmlNode sc = supercontexts.ChildNodes[i];
                X.SuperContexts.Add((FileID.FromStream(DL, sc.InnerText) as ContextFileID).FindContext(DL));
            }

            XmlNode variables = node.GetChildNode("variables");
            uint vCount = (uint)variables.GetAttrInt("count");
            for (int i = 0; i < vCount; i++)
                X.AddVariable(MVariable.DataFromStream(DL, variables.ChildNodes[i]));

            XmlNode definitions = node.GetChildNode("definitions");
            uint dCount = (uint)definitions.GetAttrInt("count");
            for (int i = 0; i < dCount; i++)
                X.AddDefinition(MDefinition.FromStream(DL, definitions.ChildNodes[i]));

            XmlNode axioms = node.GetChildNode("axioms");
            uint aCount = (uint)axioms.GetAttrInt("count");
            for (int i = 0; i < aCount; i++)
                X.AddAxiom(MStatement.FromStream(DL, axioms.ChildNodes[i]));

            foreach (MVariable v in X.Variables)
                v.FetchTheoremAxioms(DL);

            XmlNode theorems = node.GetChildNode("theorems");
            uint tCount = (uint)theorems.GetAttrInt("count");
            for (int i = 0; i < tCount; i++)
                X.AddTheorem(MTheorem.FromStream(DL, theorems.ChildNodes[i]));

            foreach (MVariable v in X.Variables)
                v.FetchProperAxioms(DL);

            return X;
        }

        public void Load()
        {
            if (loaded) return;

            for (int i = 0; i < Axioms.Count; i++)
                GetAxiom(i).Load();
            for (int i = 0; i < Variables.Count; i++)
                GetVariable(i);
            for (int i = 0; i < Definitions.Count; i++)
                GetDefinition(i).Load();
            for (int i = 0; i < SuperContexts.Count; i++)
                GetSuperContext(i);
            for (int i = 0; i < Theorems.Count; i++)
                GetTheorem(i).Load();

            loaded = true;
        }

        public static MContext GetUnloaded(DocumentLoader DL, ContextFileID ID)
        {
            DL.Remember();
            uint scCount = DL.ReadUInt();
            DL.Skip();
            DL.Remember();
            uint vCount = DL.ReadUInt();
            DL.Skip();
            DL.Remember();
            ushort dCount = DL.ReadUShort();
            DL.Skip();
            DL.Remember();
            ushort aCount = DL.ReadUShort();
            DL.Skip();
            DL.Remember();
            ushort tCount = DL.ReadUShort();


            MContext X = new MContext(ID)
            {
                Axioms = new MStatement[aCount].ToList(),
                Variables = new MVariable[(int)vCount].ToList(),
                Definitions = new MDefinition[dCount].ToList(),
                SuperContexts = new MContext[scCount].ToList(),
                Theorems = new MTheorem[tCount].ToList()
            };

            return X;
        }
        public static MContext GetUnloaded(XMLDocumentLoader DL, XmlNode node, ContextFileID ID)
        {
            MContext X = new MContext(ID)
            {
                Axioms = new MStatement[node.GetChildNode("axioms").GetAttrInt("count")].ToList(),
                Variables = new MVariable[node.GetChildNode("variables").GetAttrInt("count")].ToList(),
                Definitions = new MDefinition[node.GetChildNode("definitions").GetAttrInt("count")].ToList(),
                SuperContexts = new MContext[node.GetChildNode("supercontexts").GetAttrInt("count")].ToList(),
                Theorems = new MTheorem[node.GetChildNode("theorems").GetAttrInt("count")].ToList()
            };

            return X;
        }

        private void LoadAxiom(int index)
        {
            if (Document.IsXML) { LoadAxiomXML(index); return; }

            DocumentLoader DL = Document.DL;
            DL.Navigate(fileID);
            DL.SetContext(this);

            DL.CleanSkip(); //SuperContexts
            DL.CleanSkip(); //Variables
            DL.CleanSkip(); //Definitions

            DL.Remember();
            ushort aCount = DL.ReadUShort();
            if (index >= aCount) { DL.Close(); return; }

            for (int i = 0; i < index; i++)
            {
                DL.CleanSkip(); // skipping earlierer Axioms
            }
            
            DL.Remember();
            ContextStatementFileID ID = FileID.FromStream(DL) as ContextStatementFileID;
            
            Axioms[index] = MStatement.GetUnloaded(DL, ID);
            DL.Close();
        }
        private void LoadAxiomXML(int index)
        {
            XMLDocumentLoader DL = Document.XDL;
            DL.Navigate(fileID);
            DL.SetContext(this);

            XmlNode axioms = DL.currentNode.GetChildNode("axioms");
            int aCount = axioms.GetAttrInt("count");
            if (index >= aCount) { throw new Exception(); }

            XmlNode axiom = axioms.GetChildNode("statement", index);
            ContextStatementFileID ID = FileID.FromStream(DL, axiom.GetAttr("id")) as ContextStatementFileID;

            Axioms[index] = MStatement.GetUnloaded(DL, axiom, ID);
        }

        private MStatement LoadAxiom(ContextStatementFileID ID)
        {
            if (Document.IsXML) { return LoadAxiomXML(ID); }

            DocumentLoader DL = Document.DL;
            int index = DL.Navigate(ID);
            DL.SetContext(this);

            Axioms[index] = MStatement.GetUnloaded(DL, ID);
            DL.Close();
            return Axioms[index];
        }
        private MStatement LoadAxiomXML(ContextStatementFileID ID)
        {
            XMLDocumentLoader DL = Document.XDL;
            int index = DL.Navigate(ID);
            DL.SetContext(this);

            Axioms[index] = MStatement.GetUnloaded(DL, DL.currentNode, ID);
            return Axioms[index];
        }

        private void LoadDefinition(int index)
        {
            if (Document.IsXML) { LoadDefinitionXML(index); return; }

            DocumentLoader DL = Document.DL;
            DL.Navigate(fileID);
            DL.SetContext(this);

            DL.CleanSkip(); //SuperContexts
            DL.CleanSkip(); //Variables

            DL.Remember();
            ushort dCount = DL.ReadUShort();
            if (index >= dCount) { DL.Close(); return; }

            for (int i = 0; i < index; i++)
            {
                DL.CleanSkip(); // skipping earlierer Definitions
            }

            DL.Remember();
            DL.ReadByte(); //DefinitionType
            DefinitionFileID ID = FileID.FromStream(DL) as DefinitionFileID;
            DL.Return();
            Definitions[index] = MDefinition.GetUnloaded(DL);
            DL.Close();
        }
        private void LoadDefinitionXML(int index)
        {
            XMLDocumentLoader DL = Document.XDL;
            DL.Navigate(fileID);
            DL.SetContext(this);

            XmlNode defns = DL.currentNode.GetChildNode("definitions");
            int dCount = defns.Count();
            if (index >= dCount) { throw new Exception(); }

            XmlNode defn = defns.GetChildNode("definition", index);
            DefinitionFileID ID = defn.ID(DL) as DefinitionFileID;
            Definitions[index] = MDefinition.GetUnloaded(DL, defn);
        }

        private MDefinition LoadDefinition(DefinitionFileID ID)
        {
            if (Document.IsXML) { return LoadDefinitionXML(ID); }

            DocumentLoader DL = Document.DL;
            int index = DL.Navigate(ID);
            DL.SetContext(this);
            Definitions[index] = MDefinition.GetUnloaded(DL);
            DL.Close();
            return Definitions[index];
        }
        private MDefinition LoadDefinitionXML(DefinitionFileID ID)
        {
            XMLDocumentLoader DL = Document.XDL;
            int index = DL.Navigate(ID);
            DL.SetContext(this);
            Definitions[index] = MDefinition.GetUnloaded(DL, DL.currentNode);
            return Definitions[index];
        }

        private void LoadTheorem(int index)
        {
            if (Document.IsXML) { LoadTheoremXML(index); return; }

            DocumentLoader DL = Document.DL;
            DL.Navigate(fileID);
            DL.SetContext(this);
            DL.CleanSkip(); //SuperContexts
            DL.CleanSkip(); //Variables
            DL.CleanSkip(); //Definitions
            DL.CleanSkip(); //Axioms

            DL.Remember();
            ushort tCount = DL.ReadUShort();
            if (index >= tCount) { DL.Close(); return; }

            for (int i = 0; i < index; i++)
            {
                DL.CleanSkip(); // skipping earlierer Theorems
            }

            DL.Remember();
            TheoremFileID ID = FileID.FromStream(DL) as TheoremFileID;
            Theorems[index] = MTheorem.GetUnloaded(DL, ID);
            DL.Close();
        }
        private void LoadTheoremXML(int index)
        {
            XMLDocumentLoader DL = Document.XDL;
            DL.Navigate(fileID);
            DL.SetContext(this);

            XmlNode thms = DL.currentNode.GetChildNode("theorems");
            int tCount = thms.Count();
            if (index >= tCount) { throw new Exception(); }

            XmlNode thm = thms.GetChildNode("theorem", index);
            TheoremFileID ID = thm.ID(DL) as TheoremFileID;
            Theorems[index] = MTheorem.GetUnloaded(DL, thm, ID);
        }

        private MTheorem LoadTheorem(TheoremFileID ID)
        {
            if (Document.IsXML) { return LoadTheoremXML(ID); }

            DocumentLoader DL = Document.DL;
            int index = DL.Navigate(ID.MakeTheoremFileID());
            DL.SetContext(this);

            Theorems[index] = MTheorem.GetUnloaded(DL, ID);
            DL.Close();
            return Theorems[index];
        }
        private MTheorem LoadTheoremXML(TheoremFileID ID)
        {
            XMLDocumentLoader DL = Document.XDL;
            int index = DL.Navigate(ID.MakeTheoremFileID());
            DL.SetContext(this);

            Theorems[index] = MTheorem.GetUnloaded(DL, DL.currentNode, ID);
            return Theorems[index];
        }

        private void LoadVariable(int index)
        {
            if (Document.IsXML) { LoadVariableXML(index); return; }

            DocumentLoader DL = Document.DL;
            DL.Navigate(fileID);
            DL.SetContext(this);
            DL.CleanSkip(); //SuperContexts

            DL.Remember();
            uint vCount = DL.ReadUInt();
            if (index >= vCount) { DL.Close(); return; }

            for (int i = 0; i < index; i++)
            {
                DL.CleanSkip(); // skipping earlierer Variables
            }

            Variables[index] = MVariable.DataFromStream(DL);

            Variables[index].FetchAxioms(DL); //HACK: this might not be the correct position

            DL.Close();
        }
        private void LoadVariableXML(int index)
        {
            XMLDocumentLoader DL = Document.XDL;
            DL.Navigate(fileID);
            DL.SetContext(this);

            XmlNode vars = DL.currentNode.GetChildNode("variables");
            int vCount = vars.Count();
            if (index >= vCount) { throw new Exception(); }

            XmlNode v = vars.GetChildNode("variable", index);
            Variables[index] = MVariable.DataFromStream(DL, v);
            Variables[index].FetchAxioms(DL); //HACK: this might not be the correct position
        }

        private MVariable LoadVariable(VariableFileID ID)
        {
            if (Document.IsXML) { return LoadVariableXML(ID); }

            DocumentLoader DL = Document.DL;
            int index = DL.Navigate(ID);
            DL.SetContext(this);

            Variables[index] = MVariable.DataFromStream(DL);
            Variables[index].FetchAxioms(DL); //HACK: this might not be the correct position
            DL.Close();
            return Variables[index];
        }
        private MVariable LoadVariableXML(VariableFileID ID)
        {
            XMLDocumentLoader DL = Document.XDL;
            int index = DL.Navigate(ID);
            DL.SetContext(this);

            Variables[index] = MVariable.DataFromStream(DL, DL.currentNode);
            Variables[index].FetchAxioms(DL); //HACK: this might not be the correct position
            return Variables[index];
        }

        private void LoadSuperContext(int index)
        {
            if (Document.IsXML) { LoadSuperContextXML(index); return; }

            DocumentLoader DL = Document.DL;
            DL.Navigate(fileID);
            DL.SetContext(this);

            DL.Remember();
            uint scCount = DL.ReadUInt();
            if (index >= scCount) { DL.Close(); return; }

            for (int i = 0; i < index; i++)
            {
                DL.CleanSkip(); // skipping earlierer SuperContexts
            }

            SuperContexts[index] = (FileID.FromStream(DL) as ContextFileID).FindContext(DL);

            DL.Close();
        }
        private void LoadSuperContextXML(int index)
        {
            XMLDocumentLoader DL = Document.XDL;
            DL.Navigate(fileID);
            DL.SetContext(this);

            XmlNode sctxs = DL.currentNode.GetChildNode("supercontexts");
            int scCount = sctxs.Count();
            if (index >= scCount) { throw new Exception(); }

            XmlNode sctx = sctxs.GetChildNode("sc", index);
            SuperContexts[index] = (FileID.FromStream(DL, sctx.InnerText) as ContextFileID).FindContext(DL);
        }

        private MContext LoadSuperContext(ContextFileID ID)
        {
            if (Document.IsXML) { return LoadSuperContextXML(ID); }

            DocumentLoader DL = Document.DL;
            DL.Navigate(fileID);
            DL.SetContext(this);

            DL.Remember();
            uint scCount = DL.ReadUInt();

            int index = 0;

            while (index < scCount)
            {
                DL.Remember();
                if (ID.IsSubFileID(FileID.FromStream(DL) as ContextFileID)) break;
   
                index++;
            }

            SuperContexts[index] = ID.FindContext(DL);
            DL.Close();
            return SuperContexts[index];
        }
        private MContext LoadSuperContextXML(ContextFileID ID)
        {
            XMLDocumentLoader DL = Document.XDL;
            DL.Navigate(fileID);
            DL.SetContext(this);

            XmlNode sctxs = DL.currentNode.GetChildNode("supercontexts");
            int scCount = sctxs.Count();

            int index = 0;

            while (index < scCount)
            {
                XmlNode sctx = sctxs.GetChildNode("sc", index);
                if (ID.IsSubFileID(FileID.FromStream(DL,sctx.InnerText) as ContextFileID)) break;

                index++;
            }

            SuperContexts[index] = ID.FindContext(DL);
            return SuperContexts[index];
        }
    }

    public partial class MStatement
    {
        public bool loaded = false;

        public override void ToStream(DocumentLoader DL)
        {if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");
            DL.BeginSubStream();

            fileID.ToStream(DL);
            
            DL.PDList = null;
            DL.BeginSubStream();
            _F.ToStream(DL);
            valid.ToStream(DL);

            DL.Write(RestrictedVariables.Count);
            foreach (MVariable V in RestrictedVariables)
                V.ToStream(DL);
            DL.EndSubStream();

            DL.EndSubStream();
        }

        public override void ToXML(XMLDocumentLoader DL)
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");
            DL.Xwr.WriteStartElement("statement");
            DL.Xwr.WriteAttributeString("id", fileID.ToXML(DL.Document));
            
            _F.ToXML(DL);
            valid.ToXML(DL);

            DL.Xwr.WriteStartElement("restrictedvars");
            DL.Xwr.WriteAttributeString("count", RestrictedVariables.Count.ToString());
            foreach (MVariable V in RestrictedVariables)
                DL.Xwr.WriteElementString("v", V.fileID.ToXML(DL.Document));
            DL.Xwr.WriteEndElement(); //freevars

            DL.Xwr.WriteEndElement(); // statement
        }

        public new static MStatement FromStream(DocumentLoader DL)
        {
            //prefix
            DL.Remember();

            FileID ID = FileID.FromStream(DL);
            DL.PDList = null;
            DL.Remember();
            MStatement S = new MStatement(ID, DL.Context, MFormula.FromStream(DL), Validity.FromStream(DL, ID));
            S.valid.SelfReference(S);

            int fvCount = DL.ReadInt();
            List<MVariable> fvList = new List<MVariable>();
            for (int i = 0; i < fvCount; i++)
                fvList.Add(MTerm.FromStream(DL)as MVariable);
            S.RestrictedVariables = fvList;
            /*if (S.valid.IsAxiom)
                S._F.MakeAxiom(S, false, fvList, false);*/

            S.loaded = true;
            return S;
        }
        public new static MStatement FromStream(XMLDocumentLoader DL, XmlNode node)
        {
            FileID ID = FileID.FromStream(DL, node.GetAttr("id"));
            MStatement S = new MStatement(ID, DL.Context, MFormula.FromStream(DL, node.ChildNodes[0]), Validity.FromStream(DL, node.ChildNodes[1], ID));
            S.valid.SelfReference(S);

            XmlNode freevars = node.GetChildNode("restrictedvars");
            int fvCount = freevars.GetAttrInt("count");
            List<MVariable> fvList = new List<MVariable>();
            for (int i = 0; i < fvCount; i++)
                fvList.Add((FileID.FromStream(DL, freevars.ChildNodes[i].InnerText)as VariableFileID).FindVariable(DL));
            S.RestrictedVariables = fvList;
            /*if (S.valid.IsAxiom)
              S._F.MakeAxiom(S, false, fvList, false);*/
            //S.RestrictedVariables = S._F.MakeFreeVariableList().Except(S.RestrictedVariables).ToList();

            S.loaded = true;
            return S;
        }

        public static MStatement GetUnloaded(DocumentLoader DL, FileID ID)
        {
            DL.Remember(); //Formula & FreeVariables

            DL.PDList = null;
            MFormula.FromStream(DL);
            MStatement S = new MStatement(ID, DL.Context, null, Validity.FromStream(DL));
            S.valid.SelfReference(S);
            DL.Skip();

            return S;
        }
        public static MStatement GetUnloaded(XMLDocumentLoader DL, XmlNode node, FileID ID)
        {
            MFormula.FromStream(DL, node.GetChildNode("expression"));
            MStatement S = new MStatement(ID, DL.Context, null, Validity.FromStream(DL, node.GetChildNode("val")));
            S.valid.SelfReference(S);
            return S;
        }

        internal void Load()
        {
            if (loaded) return;
            if (_X.Document.IsXML) { LoadXML(); return; }


            DocumentLoader DL = _X.Document.DL;
            DL.Navigate(fileID);
            DL.Context = _X;
            DL.Remember();
            DL.PDList = null;
            _F = MFormula.FromStream(DL);

            DL.CleanSkip(); //Valid

            int fvCount = DL.ReadInt();
            List<MVariable> fvList = new List<MVariable>();
            for (int i = 0; i < fvCount; i++)
                fvList.Add(MVariable.FromStream(DL));
            RestrictedVariables = fvList;
            /*if (valid.IsAxiom)
                _F.MakeAxiom(this, false, fvList);*/
            DL.Close();
            loaded = true;
        }
        internal void LoadXML()
        {
            if (loaded) return;

            XMLDocumentLoader DL = _X.Document.XDL;
            DL.Navigate(fileID);
            DL.Context = _X;
            XmlNode statementNode = DL.currentNode;
            _F = MFormula.FromStream(DL, statementNode.GetChildNode("expression"));

            XmlNode fvs = statementNode.GetChildNode("restrictedvars");
            int fvCount = fvs.Count();
            List<MVariable> fvList = new List<MVariable>();
            for (int i = 0; i < fvCount; i++)
                fvList.Add((FileID.FromStream(DL, fvs.ChildNodes[i].InnerText) as VariableFileID).FindVariable(DL));
            RestrictedVariables = fvList;
            /*if (valid.IsAxiom)
                _F.MakeAxiom(this, false, fvList);*/
            loaded = true;
        }
    }

    public partial class Validity
    {
        internal void ToStream(DocumentLoader DL)
        {
            DL.BeginSubStream();
            DL.Write(_valid);
            DL.Write(_axiom);
            DL.Write(Conditions?.Count ?? 0);
            if (Conditions != null)
                foreach (MStatement C in Conditions)
                    C.fileID.ToStream(DL);
            DL.EndSubStream();
        }

        internal void ToXML(XMLDocumentLoader DL)
        {
            DL.Xwr.WriteStartElement("val");
            DL.Xwr.WriteAttributeString("v", _valid.ToString());
            DL.Xwr.WriteAttributeString("a", _axiom.ToString());
            DL.Xwr.WriteAttributeString("c", (Conditions?.Count ?? 0).ToString());
            if (Conditions != null)
                foreach (MStatement C in Conditions)
                    DL.Xwr.WriteElementString("c", C.fileID.ToXML(DL.Document));
            DL.Xwr.WriteEndElement(); //val
        }

        internal static Validity FromStream(DocumentLoader DL, FileID ID = null)
        {
            DL.Remember();
            Validity V = new Validity();
            V._valid = DL.ReadBool();
            V._axiom = DL.ReadBool();
            int cCount = DL.ReadInt();
            for (int i = 0; i < cCount; i++)
            {
                FileID DepID = FileID.FromStream(DL);
                MStatement S;
                if (ID != null && ID.IsSubFileID(DepID))            //To deal with self-references
                    S = null;
                else
                    S = DepID.FindStatement(DL);

                V.AddDependence(S);
            }
            return V;
        }
        internal static Validity FromStream(XMLDocumentLoader DL, XmlNode node, FileID ID = null)
        {
            Validity V = new Validity();
            V._valid = node.GetAttrBool("v");
            V._axiom = node.GetAttrBool("a");
            int cCount = node.GetAttrInt("c");
            for (int i = 0; i < cCount; i++)
            {
                FileID DepID = FileID.FromStream(DL, node.ChildNodes[i].InnerText);
                MStatement S;
                if (ID != null && ID.IsSubFileID(DepID))            //To deal with self-references
                    S = null;
                else
                    S = DepID.FindStatement(DL);

                V.AddDependence(S);
            }
            return V;
        }
    }
}
