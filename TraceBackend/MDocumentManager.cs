using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraceBackend
{
    public static class MDocumentManager
    {
        public static List<MDocument> Documents;

        static MDocumentManager()
        {
            Documents = new List<MDocument>();
        }

        public static MDocument CreateDocument()
        {
            MDocument D = new MDocument()
            {
                Contexts = new List<MContext>(),
                ReferencedDocuments = new List<MDocument>(),
                FilePath = "",
                Loaded = true
            };
            D.IDManager = new FileIDManager(D);

            AddDocument(D);
            return D;
        }

        public static MDocument LoadDocument(string path)
        {
            MDocument D = Documents.FirstOrDefault(Doc => path == Doc.FilePath);
            if (D == null)
            {
                D = MDocument.FromFile(path);
                AddDocument(D);
            }
            else
            {
                D.Load();
            }
            return D;
        }

        public static void AddDocument(MDocument D)
        {
            Documents.Add(D);
        }

        static MDocument GetUnloadedDocument(string path)
        {
            MDocument D = MDocument.GetUnloaded(path);
            AddDocument(D);
            return D;
        }

        public static MDocument GetFromPath(string path)
        {
            //Check if document is already listed
            MDocument ret = Documents.FirstOrDefault(D => D.FilePath == path);
            if (ret != null) return ret;

            //If not, load and then return
            return GetUnloadedDocument(path);
        }
    }
}
