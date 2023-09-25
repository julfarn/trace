using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TraceBackend;

namespace TraceUI
{
    public partial class ElementFinder : Form
    {
        public event EventHandler<ObjectChosenEventArgs> ObjectChosen;
        public event EventHandler<BracketMode> BracketSettingsChanged;
        public event EventHandler<(MVariable, bool)> VariableSettingsChanged;
        public event EventHandler VisSettingsChanged;
        public VisualisationDisplay currentVD;
        EditableText editableText;
        MStatement Axiom;
        List<MVariable> Variables;

        bool loading;

        ElementType AllowedType;

        public ElementFinder()
        {
            InitializeComponent();
            definitionList.ObjectChosen += objectChosen;
            editableText = EditableText.FromString();
        }

        public void ShowForVisualisation(VisualisationDisplay VD, BracketMode bracketMode, bool toplevel, ElementType ET = ElementType.Any)
        {
            if (currentVD != null) currentVD.StopEditing();

            loading = true;

            Point Loc = VD.PointToScreen(new Point(0, 0));
            Loc.Y += VD.Height + 10;

            Show();
            DesktopLocation = Loc;

            currentVD = VD;
            editableText = EditableText.FromString();

            Focus();

            definitionList.Filter(VD.Context, "", ET);
            AllowedType = ET;
            switch (ET)
            {
                case ElementType.Any:
                    //TODO ?
                    break;
                case ElementType.Formula:
                    linkButton1.Mode = LinkMode.Formula;
                    break;
                case ElementType.Term:
                    linkButton1.Mode = LinkMode.Term;
                    break;
                case ElementType.Variable:
                    linkButton1.Mode = LinkMode.Variable;
                    break;
                case ElementType.UndefindedPredicate:
                    linkButton1.Mode = LinkMode.UndefinedPredicate;
                    break;
                case ElementType.QuantifierDefinition:
                    linkButton1.Mode = LinkMode.Quantifier;
                    break;
                case ElementType.PredicateDefinition:
                    linkButton1.Mode = LinkMode.Predicate;
                    break;
                case ElementType.Statement:
                    linkButton1.Mode = LinkMode.Statement;
                    break;
                default:
                    break;
            }

            if (KeyboardHelper.KeyPressed(System.Windows.Input.Key.LeftShift)) linkButton1.StartLinking();

            check_bracket.CheckState = (CheckState)(int)bracketMode;

            if (currentVD.Statement != null && currentVD.Statement.valid.IsAxiom && toplevel)
            {
                Axiom = currentVD.Statement;
                checkLB_freeVars.Items.Clear();
                Variables = Axiom._F.MakeFreeVariableList();
                for(int i = 0; i < Variables.Count; i++)
                {
                    checkLB_freeVars.Items.Add(Variables[i].stringSymbol);
                    checkLB_freeVars.SetItemChecked(i, Axiom.RestrictedVariables.Contains(Variables[i]));
                }
                checkLB_freeVars.Visible = true;
            }
            else
            {
                Axiom = null;
                checkLB_freeVars.Visible = false;
            }

            loading = false;
        }

        public void Filter(string F = "", ElementType ET = ElementType.Any) { definitionList.Filter(currentVD.Context, F, ET); }

        private void objectChosen(object sender, ObjectChosenEventArgs e)
        {
            ObjectChosen?.Invoke(sender, e);
        }

        private void linkButton1_Linked(object sender, LinkedEventArgs e)
        {
            ObjectChosen?.Invoke(sender, new ObjectChosenEventArgs(e.Object));
        }

        private void ElementFinder_FormClosing(object sender, FormClosingEventArgs e)
        {
        }

        private void ElementFinder_VisibleChanged(object sender, EventArgs e)
        {
            if (Visible == false) currentVD = null;
        }

        private void ElementFinder_KeyDown(object sender, KeyEventArgs e)
        {
            char? c = KeyboardHelper.GetCharFromKey(e.KeyData);
            switch (e.KeyData)
            {
                case Keys.Back:
                    editableText.Backspace();
                    currentVD?.UpdateEditedText(editableText);
                    break;

                case Keys.Up:
                    definitionList.SelectPrevious();
                    break;
                case Keys.Down:
                    definitionList.SelectNext();
                    
                    break;

                case Keys.Enter:
                    definitionList.Choose();
                    break;

                default:
                    if (c.HasValue)
                    {
                        editableText.Insert(c.Value);
                        currentVD?.UpdateEditedText(editableText);
                    }
                    break;
            }
            Filter(editableText.Text, AllowedType);
        }
        
        private void ElementFinder_Leave(object sender, EventArgs e)
        {

        }

        private void ElementFinder_Deactivate(object sender, EventArgs e)
        {
            if (LinkButton.IsLinking || currentVD == null) return;

            currentVD.StopEditing();
            currentVD = null;
        }

        private void bt_new_Click(object sender, EventArgs e)
        {
            if(AllowedType == ElementType.Variable || AllowedType == ElementType.Term)
            {
                MVariable newVar = currentVD.Context.CreateVariable(editableText.Text);
                objectChosen(this, new ObjectChosenEventArgs(newVar));
            }

            if(AllowedType == ElementType.UndefindedPredicate || AllowedType == ElementType.Formula)
            {
                int aCount = (int)ud_argCount.Value;
                MUndefinedPredicate UP = new MUndefinedPredicate(aCount, editableText.Text);
                MTerm[] Terms = new MTerm[aCount];
                for (int i = 0; i < aCount; i++) Terms[i] = MTerm.PlaceholderTerm;
                MUndefinedPredicateFormula F = new MUndefinedPredicateFormula(UP, Terms);
                objectChosen(this, new ObjectChosenEventArgs(F));
            }
        }

        private void bt_eq_Click(object sender, EventArgs e)
        {
            if(AllowedType == ElementType.Formula)
            {
                objectChosen(this, new ObjectChosenEventArgs(MEqualityDefinition.DefaultDefinition.GetPlaceholderVisualisation(null).VisualisedObject.Copy()));
            }
        }

        private void bt_neg_Click(object sender, EventArgs e)
        {
            if (AllowedType == ElementType.Formula)
            {
                objectChosen(this, new ObjectChosenEventArgs(MNegationDefinition.DefaultDefinition.GetPlaceholderVisualisation(null).VisualisedObject.Copy()));
            }
        }

        private void bt_true_Click(object sender, EventArgs e)
        {
            if (AllowedType == ElementType.Formula)
            {
                objectChosen(this, new ObjectChosenEventArgs(MTrivialFormula._True));
            }
        }

        private void bt_false_Click(object sender, EventArgs e)
        {
            if (AllowedType == ElementType.Formula)
            {
                objectChosen(this, new ObjectChosenEventArgs(MTrivialFormula._False));
            }
        }

        private void check_bracket_CheckedChanged(object sender, EventArgs e)
        {
            if (loading) return;
            BracketSettingsChanged(this, (BracketMode)(int)check_bracket.CheckState);
            
        }

        private void checkLB_freeVars_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void checkLB_freeVars_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (loading) return;

            VariableSettingsChanged.Invoke(this, (Variables[e.Index], e.NewValue == CheckState.Checked));
        }

        private void bt_vis_Click(object sender, EventArgs e)
        {
            VisSettingsChanged?.Invoke(this, EventArgs.Empty);
        }

        private void button_refresh_Click(object sender, EventArgs e)
        {
            loading = true;

            definitionList.Clear();
            definitionList.AddFromLoadedDocuments();

            loading = false;
        }
    }

}
