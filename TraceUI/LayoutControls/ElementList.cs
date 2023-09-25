using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TraceBackend;


namespace TraceUI
{
    public partial class ElementList : UserControl
    {
        public event EventHandler<ObjectChosenEventArgs> ObjectChosen;
        public ElementDisplay SelectedElement;

        public ElementList()
        {
            InitializeComponent();
        }

        internal void Clear()
        {
            while(listPanel.Rows.Count > 0)
                listPanel.RemoveRow(0, true);
        }

        public static ElementList CompleteLoadedList()
        {
            throw new NotImplementedException();
        }

        public void AddFromLoadedDocuments(bool FormulaDefinitions = true, bool TermDefinitions = true, bool Variables = true)
        {
            foreach (MDocument D in MDocumentManager.Documents)
                if (D.Loaded) AddFromDocument(D, FormulaDefinitions, TermDefinitions, Variables);
        }

        public void AddFromDocument(MDocument D, bool FormulaDefinitions = true, bool TermDefinitions = true, bool Variables = true)
        {
            for(int i = 0; i < D.Contexts.Count; i++)
            {
                MContext X = D.GetContext(i);
                AddFromContext(X, FormulaDefinitions, TermDefinitions, Variables);
            }
        }

        public void AddFromContext(MContext X, bool FormulaDefinitions = true, bool TermDefinitions = true, bool Variables = true)
        {
            for (int i = 0; i < X.Definitions.Count; i++)
            {
                MDefinition D = X.GetDefinition(i);
                if (D is MFunctionSymbol)
                {
                    if (TermDefinitions)
                        AddDefinition(D);
                }
                else if (FormulaDefinitions) AddDefinition(D);
            }

            if (Variables)
                for (int i = 0; i < X.Variables.Count; i++)
                {
                    AddVariable(X.GetVariable(i));
                }
        }
        
        public void AddDefinition(MDefinition D)
        {
            ElementDisplay DD = ElementDisplay.FromDefinition(D);
            listPanel.InsertRow(DD);
            DD.Click += ElementDisplay_Click;
            DD.Show();
        }

        public void AddVariable(MVariable V)
        {
            ElementDisplay DD = ElementDisplay.FromVariable(V);
            listPanel.InsertRow(DD);
            DD.Click += ElementDisplay_Click;
            DD.Show();
        }

        public void AddSymbol(MShapeSymbol S)
        {
            ElementDisplay DD = ElementDisplay.FromSymbol(S);
            listPanel.InsertRow(DD);
            DD.Click += ElementDisplay_Click;
            DD.Show();
        }

        private void ElementDisplay_Click(object sender, EventArgs e)
        {
            ElementDisplay ED;
            if (sender is ElementDisplay) ED = sender as ElementDisplay;
            else
            {
                ED = (sender as Control).Parent as ElementDisplay;
            }

            if (ED.Highlighted)
            {
                Choose();
            }

            Select(ED);
        }

        public void Choose()
        {
            if(SelectedElement != null)
            {
                ObjectChosen?.Invoke(SelectedElement, new ObjectChosenEventArgs(SelectedElement.GetElement()));
            }
        }

        public void Select(ElementDisplay ED)
        {
            foreach (ElementDisplay ED_ in listPanel.Controls)
            {
                if (ED_.Highlighted && ED_ != ED)
                    ED_.Highlighted = false;
            }

            SelectedElement = ED;

            ED.Highlighted = true;
        }

        public void SelectNext()
        {
            for (int i = 0; i < listPanel.Controls.Count - 1; i++)
                if (listPanel.Controls[i] is ElementDisplay ED)
                    if (ED.Highlighted && listPanel.Controls[i + 1] is ElementDisplay nextED)
                    {
                        Select(nextED);
                        return;
                    }
            if (listPanel.Controls.Count != 0 && listPanel.Controls[0] is ElementDisplay ED3)
                Select(ED3);
        }

        public void SelectPrevious()
        {
            for (int i = 1; i < listPanel.Controls.Count; i++)
                if (listPanel.Controls[i] is ElementDisplay ED)
                    if (ED.Highlighted && listPanel.Controls[i - 1] is ElementDisplay prevED)
                    {
                        Select(prevED);
                        return;
                    }
            if (listPanel.Controls.Count != 0 && listPanel.Controls[listPanel.Controls.Count - 1] is ElementDisplay ED3)
                Select(ED3);
        }

        public void Filter(MContext X = null, string F = "", ElementType ET = ElementType.Any)
        {
            F = F.ToLower();

            foreach (ElementDisplay DD in listPanel.Controls)
            {
                if (!ET.AllowsFor(DD.type)) DD.Visible = false;
                else
                {
                    if (DD.Definition != null)
                    {
                        if (DD.Definition.stringSymbol.ToLower().Contains(F))
                            DD.Visible = true;
                        else
                            DD.Visible = false;
                    }
                    else if (DD.Variable != null)
                    {
                        if (X == null || DD.Variable.fileID.FindContext() == X || DD.Variable.HasAxioms)
                        {
                            if (DD.Variable.stringSymbol.ToLower().Contains(F))
                                DD.Visible = true;
                            else
                                DD.Visible = false;
                        }
                        else
                            DD.Visible = false;
                    }
                }
            }

            listPanel.UpdateHeight(0);
        }
    }

    public class ObjectChosenEventArgs : EventArgs
    {
        public MObject Object;

       public ObjectChosenEventArgs(MObject O) : base()
        {
            Object = O;
        }
    }
}
