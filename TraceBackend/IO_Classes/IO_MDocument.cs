using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TraceBackend
{
    public partial class MDocument
    {
        public bool IsXML = false;
        public ProtoDocumentLoader PDL;
        public DocumentLoader DL => (DocumentLoader)PDL;
        public XMLDocumentLoader XDL => (XMLDocumentLoader)PDL;
        public XmlNode XmlD; //TODO: this should not be present for unloaded documents

        public void ToFile(string path)
        {
            if (!Loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");

            FilePath = path;
            DL.Open(DocumentLoader.RWMode.Writing);

            DL.Write(VERSION);
            DL.BeginSubStream();
            // **** Navigation Point ****

            //Referenced Documents
            DL.BeginSubStream();
            DL.Write(ReferencedDocuments.Count);
            for (int i = 0; i < ReferencedDocuments.Count; i++)
                DL.Write(Helper.GetRelativePath(Directory, ReferencedDocuments[i].FilePath));
            DL.EndSubStream();

            //FileIDManager
            IDManager.ToStream(DL);

            //Contexts
            DL.Write(Contexts.Count);

            foreach (MContext X in Contexts)
                X.ToStream(DL);

            DL.EndSubStream();

            DL.BeginSubStream();
            Structure.ToStream(DL);
            DL.EndSubStream();

            DL.Close();
        }
        public void ToXML(string path)
        {
            if (!Loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");

            FilePath = path;
            XMLDocumentLoader DL = new XMLDocumentLoader(this, XmlWriter.Create(FilePath, new XmlWriterSettings() { Indent=true, NewLineHandling= NewLineHandling.Entitize }));
            DL.Xwr.WriteStartDocument();
            DL.Xwr.WriteStartElement("document");
            DL.Xwr.WriteAttributeString("version", VERSION.ToString());

            DL.Xwr.WriteStartElement("backend");

            //Referenced Documents
            DL.Xwr.WriteStartElement("references");
            DL.Xwr.WriteAttributeString("count", ReferencedDocuments.Count.ToString());
            for (int i = 0; i < ReferencedDocuments.Count; i++)
            {
                DL.Xwr.WriteStartElement("ref");
                DL.Xwr.WriteAttributeString("id", i.ToString());
                DL.Xwr.WriteString(Helper.GetRelativePath(Directory, ReferencedDocuments[i].FilePath));
                DL.Xwr.WriteEndElement();
            }
            DL.Xwr.WriteEndElement(); // references

            //FileIDManager
            IDManager.ToXML(DL.Xwr);

            //Contexts
            DL.Xwr.WriteStartElement("contexts");
            DL.Xwr.WriteAttributeString("count", Contexts.Count.ToString());

            foreach (MContext X in Contexts)
                X.ToXML(DL);

            DL.Xwr.WriteEndElement(); // contexts

            DL.Xwr.WriteEndElement(); //backend

            DL.Xwr.WriteStartElement("structure");
            Structure.ToXML(DL);
            DL.Xwr.WriteEndElement(); // structure

            DL.Xwr.WriteEndElement(); // document
            DL.Xwr.WriteEndDocument();
            DL.Xwr.Close();
        }

        public static MDocument FromFile(string path)
        {
            if (path.EndsWith(".xtr")) return FromFileXML(path);

            MDocument D = new MDocument()
            {
                Contexts = new List<MContext>(),
                ReferencedDocuments = new List<MDocument>(),
                FilePath = path
            };

            D.DL.Open();

            uint ver = D.DL.ReadUShort();

            //prefix
            D.DL.Remember();

            //Referenced Documents
            //prefix
            D.DL.Remember();
            int dcount = D.DL.ReadInt();
            for (int i = 0; i < dcount; i++)
            {
                string dpath = Path.GetFullPath(Path.Combine(D.Directory, D.DL.ReadString()));
                D.ReferencedDocuments.Add(MDocumentManager.GetFromPath(dpath));
            }

            //FileIDManager
            D.IDManager = FileIDManager.FromStream(D.DL);

            //Contexts
            int xCount = D.DL.ReadInt();
            for (int i = 0; i < xCount; i++)
            {
                D.Contexts.Add(MContext.FromStream(D.DL));
            }

            D.DL.Remember();
            D.Structure = DocumentStructure.FromStream(D.DL);

            D.DL.Close();

            D.Loaded = true;
            return D;
        }
        public static MDocument FromFileXML(string path)
        {
            MDocument D = new MDocument()
            {
                Contexts = new List<MContext>(),
                ReferencedDocuments = new List<MDocument>(),
                FilePath = path,
                IsXML = true
            };

            XmlDocument XmlD = new XmlDocument();
            XmlD.Load(path);
            XMLDocumentLoader DL = new XMLDocumentLoader(D);

            D.PDL = DL;
            D.XmlD = XmlD.GetChildNode("document");

            uint ver = Convert.ToUInt32(D.XmlD.Attributes["version"].Value);

            XmlNode backend = D.XmlD.FirstChild;

            //Referenced Documents
            //prefix
            XmlNode references = backend.ChildNodes[0];
            int dcount = Convert.ToInt32(references.Attributes["count"].Value);
            for (int i = 0; i < dcount; i++)
            {
                XmlNode r = references.ChildNodes[i];
                string dpath = Path.GetFullPath(Path.Combine(D.Directory, r.InnerText));
                D.ReferencedDocuments.Add(MDocumentManager.GetFromPath(dpath));
            }

            //FileIDManager
            XmlNode IDManager = backend.ChildNodes[1];
            D.IDManager = FileIDManager.FromStream(DL, IDManager);

            //Contexts
            XmlNode contexts = backend.ChildNodes[2];
            int xCount = Convert.ToInt32(contexts.Attributes["count"].Value);
            for (int i = 0; i < xCount; i++)
            {
                D.Contexts.Add(MContext.FromStream(DL, contexts.ChildNodes[i]));
            }
            
            D.Structure = DocumentStructure.FromStream(DL, D.XmlD.LastChild.FirstChild);

            D.Loaded = true;
            return D;
        }

        public void Load()
        {
            if (Loaded) return;
            if(IsXML) { LoadXML(); return; }

            DL.Open();
            uint ver = DL.ReadUShort();
            uint structPos = DL.Remember(); //Document
            DL.CleanSkip(); //Referenced Documents

            //FileIDManager
            IDManager = FileIDManager.FromStream(DL);


            DL.Close();

            for (int i = 0; i < ReferencedDocuments.Count; i++)
                if (ReferencedDocuments[i] == null) LoadReferencedDocument(i);

            for (int i = 0; i < Contexts.Count; i++)
                GetContext(i).Load();

            DL.Open();
            DL.JumpTo(structPos);
            DL.Remember();
            Structure = DocumentStructure.FromStream(DL);
            DL.Close();

            Loaded = true;
        }

        public void LoadXML()
        {
            if (Loaded) return;

            XmlNode backend = XmlD.GetChildNode("backend");

            //FileIDManager
            IDManager = FileIDManager.FromStream(XDL, backend.GetChildNode("id-manager"));
            
            for (int i = 0; i < ReferencedDocuments.Count; i++)
                if (ReferencedDocuments[i] == null) LoadReferencedDocumentXML(i);

            for (int i = 0; i < Contexts.Count; i++)
                GetContext(i).Load();

            Structure = DocumentStructure.FromStream(XDL, XmlD.GetChildNode("structure").FirstChild);

            Loaded = true;
        }

        public static MDocument GetUnloaded(string path)
        {
            if (path.EndsWith(".xtr")) return GetUnloadedXML(path);

            MDocument D = new MDocument()
            {
                FilePath = path
            };

            D.DL.Open();

            uint ver = D.DL.ReadUShort();

            D.DL.Remember(); //Document
            D.DL.Remember(); //Referenced Documents
            int dcount = D.DL.ReadInt();
            D.DL.Skip();
            D.DL.CleanSkip(); //ID Manager
            //D.DL.Remember(); // Contexts
            int xCount = D.DL.ReadInt();
            D.DL.Close();

            D.Loaded = false;
            D.ReferencedDocuments = new MDocument[dcount].ToList();
            D.Contexts = new MContext[xCount].ToList();

            return D;
        }
        public static MDocument GetUnloadedXML(string path)
        {
            MDocument D = new MDocument()
            {
                FilePath = path,
                IsXML = true
            };

            XmlDocument XmlD = new XmlDocument();
            XmlD.Load(path);
            XMLDocumentLoader DL = new XMLDocumentLoader(D);

            D.XmlD = XmlD.GetChildNode("document");
            D.PDL = DL;

            uint ver = (uint)D.XmlD.GetAttrInt("version");
            XmlNode backend = D.XmlD.GetChildNode("backend");
            
            int dcount = backend.GetChildNode("references").GetAttrInt("count");
            int xCount = backend.GetChildNode("contexts").GetAttrInt("count");

            D.Loaded = false;
            D.ReferencedDocuments = new MDocument[dcount].ToList();
            D.Contexts = new MContext[xCount].ToList();

            return D;
        }

        private void LoadReferencedDocument(int index)
        {
            if (IsXML) { LoadReferencedDocumentXML(index); return; }

            DL.Navigate(fileID);
            DL.Remember(); // Ref. Docs

            int dcount = DL.ReadInt();
            if (index >= dcount) { DL.Close(); return; }

            for (int i = 0; i < index; i++)
            {
                DL.ReadString(); // skipping earlierer referenced documents
            }

            string dpath = Path.GetFullPath(Path.Combine(Directory, DL.ReadString()));
            ReferencedDocuments[index] = MDocumentManager.GetFromPath(dpath);

            DL.Close();
        }
        private void LoadReferencedDocumentXML(int index)
        {
            XDL.Navigate(fileID);
            XmlNode docs = XDL.currentNode.GetChildNode("references");

            int dcount = docs.GetAttrInt("count");
            if (index >= dcount) { throw new Exception(); }

            XmlNode reference = docs.GetChildNode("ref", index);

            string dpath = Path.GetFullPath(Path.Combine(Directory, reference.InnerText));
            ReferencedDocuments[index] = MDocumentManager.GetFromPath(dpath);
        }

        private void LoadContext(int index)
        {
            if (IsXML) { LoadContextXML(index); return; }

            DL.Navigate(fileID);
            DL.CleanSkip(); // ref. Docs
            DL.CleanSkip(); // IDManager

            int cCount = DL.ReadInt();
            if (index >= cCount) { DL.Close(); return; }

            for (int i = 0; i < index; i++)
            {
                DL.CleanSkip(); // skipping earlierer Contexts
            }

            DL.Remember();
            ContextFileID ID = FileID.FromStream(DL) as ContextFileID;
            Contexts[index] = MContext.GetUnloaded(DL, ID);
            DL.Close();
        }
        private void LoadContextXML(int index)
        {
            XDL.Navigate(fileID);

            XmlNode contexts = XDL.currentNode.GetChildNode("contexts");

            int cCount = contexts.GetAttrInt("count");
            if (index >= cCount) { throw new Exception(); }

            XmlNode context = contexts.GetChildNode("context", index);

            ContextFileID ID = FileID.FromStream(XDL, context.GetAttr("id")) as ContextFileID;
            Contexts[index] = MContext.GetUnloaded(XDL, context, ID);
        }

        private MContext LoadContext(ContextFileID ID)
        {
            if (IsXML) { return LoadContextXML(ID); }

            int index = DL.Navigate(ID.MakeContextFileID());
            Contexts[index] = MContext.GetUnloaded(DL, ID.MakeContextFileID());
            DL.Close();
            return Contexts[index];
        }
        private MContext LoadContextXML(ContextFileID ID)
        {
            int index = XDL.Navigate(ID.MakeContextFileID());
            Contexts[index] = MContext.GetUnloaded(XDL, XDL.currentNode, ID.MakeContextFileID());
            return Contexts[index];
        }
    }
}
