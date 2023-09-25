using System.Collections.Generic;

namespace TraceBackend
{
    public partial class DocumentStructure
    {
        public List<DocumentStructure> Children;
        public DocumentStructure Parent;
        public bool Collapsed;
        public bool Hidden;
        public IDocumentElement Element;


        DocumentStructure()
        {
            Children = new List<DocumentStructure>();
        }

        public static DocumentStructure Embed(IDocumentElement DocEl, DocumentStructure Parent = null, bool last = false)
        {
            DocumentStructure DS = new DocumentStructure() { Element = DocEl };

            DS.Parent = Parent;
            if (Parent != null)
                if(last) Parent.Children.Add(DS);
                else Parent.Children.Insert(0, DS);

            return DS;
        }

        public static DocumentStructure Insert(IDocumentElement DocEl, DocumentStructure After)
        {
            DocumentStructure DS = new DocumentStructure() { Element = DocEl };

            DS.Parent = After.Parent;
            if (After.Parent != null) After.Parent.Children.Insert(After.Parent.Children.IndexOf(After) + 1, DS);

            return DS;
        }

        public DocumentStructure Previous<T>()
        {
            if (Element is T) return this;
            if (Parent == null) return null;
            int idx = Parent.Children.IndexOf(this) - 1;
            if (idx < 0) return null;
            else return Parent.Children[idx].Previous<T>();
        }

        public DocumentStructure SplitContext(DocumentStructure After) //TODO: This cannot split texts. Or can it? Who knows? Lets find out!
        {
            if (!(Element is MContext X)) throw new System.Exception("SplitContext called on Structure that is not a Context.");
            MDocument D = Parent.Element as MDocument;

            MContext X2 = D.CreateContext(this);
            X2.AddSuperContext(X);
            DocumentStructure DS = D.Structure.GetByElement(X2);

            int nextIndex = Children.IndexOf(After)+1;

            while(nextIndex < Children.Count)
            {
                DocumentStructure transStruct = Children[nextIndex];
                transStruct.CutOff();
                transStruct.FitIn(DS, true);

                if (transStruct.Element is MDefinition Def)
                    TransferDefinition(Def);
                else if (transStruct.Element is MStatement Ax)
                    TransferAxiom(Ax);
                else if (transStruct.Element is MTheorem Theo)
                    TransferTheorem(Theo);
            }

            return DS;

            void TransferDefinition(MDefinition Def)
            {
                X2.AddDefinition(Def);
                X.RemoveDefinition(Def);
                Def.TransferToNewContext(X2);
            }
            void TransferAxiom(MStatement Ax)
            {
                X2.AddAxiom(Ax);
                X.RemoveAxiom(Ax);
                Ax.TransferToNewContext(X2);
            }
            void TransferTheorem(MTheorem Theo)
            {
                X2.AddTheorem(Theo);
                X.RemoveTheorem(Theo);
                Theo.TransferToNewContext(X2);
            }
        }

        public void CutOff()
        {
            if (Parent != null)
                Parent.Children.Remove(this);
            Parent = null;
        }

        public void FitIn(DocumentStructure parent, bool last = false)
        {
            Parent = parent;
            if (last) Parent.Children.Add(this);
            else Parent.Children.Insert(0, this);
        }

        public void Delete()
        {
            CutOff();
        }

        public ushort[] GetPath()
        {
            if (Parent == null) return new ushort[0];

            return Parent.GetPath().Add((ushort)Parent.Children.IndexOf(this));
        }

        public DocumentStructure GetByPath(ushort[] path, byte pos = 0)
        {
            if (pos == path.Length) return this;
            return Children[path[pos]].GetByPath(path, (byte)(pos + 1));
        }

        public DocumentStructure GetByElement(IDocumentElement E)
        {
            if (Element == E) return this;

            foreach (DocumentStructure DS in Children)
            {
                DocumentStructure R = DS.GetByElement(E);
                if (R != null) return R;
            }

            return null;
        }

        public string GetLaTeX()
        {

            string Latex = Element.LaTeXStart;
            Latex += "\n";

            for(int i = 0; i < Children.Count; i++)
            {
                if(!Children[i].Hidden)
                Latex += Children[i].GetLaTeX() + "\n";
            }

            return Latex + Element.LaTeXEnd;
        }
    }

    public class MStatementList : IDocumentElement
    {
        public MDefinition Definition;
        public string LaTeXStart =>@"";

        public string LaTeXEnd => @"";

        //Dummy type
    }
}
