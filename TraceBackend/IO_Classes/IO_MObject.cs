using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TraceBackend
{
    public partial class MObject
    {
        public virtual void ToStream(DocumentLoader DL)
        {

        }

        public virtual void ToXML(XMLDocumentLoader DL)
        {

        }

        public static MObject FromStream(DocumentLoader DL)
        {
            throw new FileLoadException("FromStream should not be called directly on MObject, only on children.");
        }
    }
}
