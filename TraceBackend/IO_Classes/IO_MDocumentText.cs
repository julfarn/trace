using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraceBackend
{
    public partial class MDocumentText : IDocumentElement
    {
        public string LaTeXStart => Text;
        public string LaTeXEnd => @"";

        public void ToStream(DocumentLoader DL)
        {
            DL.Write(Text);
        }

        public static MDocumentText FromStream(DocumentLoader DL)
        {
            string txt = DL.ReadString();
            MDocumentText Text = new MDocumentText() { Text = txt };
            return Text;
        }
        public static MDocumentText FromStream(string txt)
        {
            MDocumentText Text = new MDocumentText() { Text = txt };
            return Text;
        }
    }
}
