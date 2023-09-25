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
using System.Diagnostics;

namespace TraceUI
{
    public partial class VisualisationDisplay : UserControl
    {
        public event EventHandler MClick;
        public event EventHandler ExpressionChanged;
        public event EventHandler CompleteChange;
        public new event EventHandler Click { add { base.Click += value; pB.Click += value; } remove { base.Click -= value; pB.Click -= value; } }
        public event EventHandler<MClickEventArgs> SymbolSelected;

        public bool Editable { get; set; } = true;
        public bool LinkOnly { get; set; } = false;
        MVisualisation v;
        public MVisualisation Visualisation
        {
            get { return v; }
            set
            {
                v = value;
                if (value != null)
                {
                    Redraw(true);

                    if (value.VisualisedObject is MExpression E)
                        Expression = E;
                    else
                        Expression = null;
                }
            }
        }
        MVisualisation HoveredSubV;
        public InteractionMode interactionMode { get; set; } = InteractionMode.subVis;
        public MExpression Expression;
        public MStatement Statement;
        public DocumentStructure Structure;
        public MContext Context => (Parent as DocumentLayoutPanel)?.ParentDE?.Context;

        DateTime lastUpdate;
        public int minRestTime { get; set; }

        MDrawOptions DrawOptions;
        private InteractionMode previousMode;
        private static ElementFinder elementFinder;
        public ElementType Restriction { get; set; } = ElementType.Any;
        MVisualisation activeSubVis;

        static Brush VarNoAx, VarWithAx, VarThisAx, BoundVar;

        static VisualisationDisplay()
        {
            VarNoAx = new SolidBrush(Properties.Settings.Default.VarNoAx);
            VarWithAx = new SolidBrush(Properties.Settings.Default.VarWithAx);
            VarThisAx = new SolidBrush(Properties.Settings.Default.VarThisAx);
            BoundVar = new SolidBrush(Properties.Settings.Default.BoundVar);

            elementFinder = new ElementFinder();
        }

        public enum InteractionMode
        {
            passive = 0,
            subVis = 1,
            symbols = 2,
            editing = 3
        }

        public static VisualisationDisplay FromExpression(MExpression expression, 
            bool editable = false, bool linkonly = false, ElementType restriction = ElementType.Any, InteractionMode mode = InteractionMode.subVis)
        {
            VisualisationDisplay VD = new VisualisationDisplay()
            {
                Visualisation = expression.GetVisualisation(),
                Editable = editable,
                LinkOnly = linkonly,
                Restriction = restriction,
                interactionMode = mode
            };

            return VD;
        }

        public static VisualisationDisplay FromStatement(MStatement Statement,
            bool editable = false, bool linkonly = false, ElementType restriction = ElementType.Any, InteractionMode mode = InteractionMode.subVis)
        {
            VisualisationDisplay VD = FromExpression(Statement?.GetFormula() ?? MFormula.PlaceholderFormula, editable, linkonly, restriction, mode);
            VD.SetStatement(Statement);
            return VD;
        }

        public static VisualisationDisplay FromStructure(DocumentStructure Structure,
           bool editable = false, bool linkonly = false, ElementType restriction = ElementType.Any, InteractionMode mode = InteractionMode.subVis)
        {
            MStatement Statement = Structure.Element as MStatement;
            VisualisationDisplay VD = FromStatement(Statement, editable, linkonly, restriction, mode);
            VD.Structure = Structure;
            return VD;
        }

        public void SetStatement(MStatement S)
        {
            if (Statement != null) {
                Statement.ValidityChanged -= Statement_ValidityChanged;
            }
            Statement = S;
            if (S == null)
            {
                Expression = MFormula.PlaceholderFormula;
            }
            else
            {
                Statement.ValidityChanged += Statement_ValidityChanged;
                Expression = S.GetFormula();
                UpdateValidity();
            }
            UpdateVisualisationFromExpression();
        }

        private void Expression_Changed(object sender, EventArgs e)
        {
            if (Statement != null)
            {
                Statement.InvokeExpressionChanged();
            }
        }

        void UpdateValidity()
        {
            if (Statement == null) return;
            DrawOptions.SetGlobalOptions(Brushes.Transparent, Statement.valid.GetBrush(), VarNoAx, VarWithAx, VarThisAx, BoundVar);
        }

        private void Statement_ValidityChanged(object sender, EventArgs e)
        {
            UpdateValidity();
            Redraw(true);
        }

        internal void Redraw(bool recalculate)
        {
            if (v != null)
            {
                MainForm.ActiveMainForm.FreezeScroll();
                
                Image I = v.Draw(MainForm.PPI, DrawOptions, recalculate);
                pB.Image = I;
                Size = new Size((int)(v.size.Width * MainForm.PPI + 1.5F), (int)(v.size.Height * MainForm.PPI + 0.5F));

                MainForm.ActiveMainForm.ContinueScroll();
            }
        }

        public VisualisationDisplay()
        {
            InitializeComponent();
            minRestTime = 50;
            DrawOptions = new MDrawOptions();
            DrawOptions.SetGlobalOptions(Brushes.Transparent, Brushes.Black, VarNoAx, VarWithAx, VarThisAx, BoundVar);
            pB.Click += HandleMClick;
            ExpressionChanged += Expression_Changed;
        }

        private void VisualisationDisplay_Load(object sender, EventArgs e)
        {
            if (Editable && Visualisation == null)
            {
                if (Restriction == ElementType.Formula ||
                    Restriction == ElementType.PredicateDefinition ||
                    Restriction == ElementType.QuantifierDefinition ||
                    Restriction == ElementType.UndefindedPredicate ||
                    Restriction == ElementType.UndefinedFormula)
                    Visualisation = MFormula.PlaceholderFormula.GetVisualisation();
                if (Restriction == ElementType.Term)
                    Visualisation = MTerm.PlaceholderTerm.GetVisualisation();
                if (Restriction == ElementType.Variable)
                    Visualisation = MVariable.PlaceholderVariable.GetVisualisation();
            }
        }

        public void StartEditing(MVisualisation subVis)
        {
            previousMode = interactionMode;
            interactionMode = InteractionMode.editing;
            Focus();

            SetActiveSubVis(subVis);

            ElementType ET = ElementType.Any;
            bool toplevel = false; ;
            if (activeSubVis == Visualisation)
            {
                ET = Restriction;
                toplevel = true;
            }
            else
            {
                if (activeSubVis.VisualisedObject is MFormula) ET = ElementType.Formula;
                else if (activeSubVis.VisualisedObject is MTerm) ET = ElementType.Term;
                if (activeSubVis.VisualisedObject is MBoundVariable) ET = ElementType.Variable;
            }
            elementFinder.ShowForVisualisation(this, activeSubVis.BracketMode, toplevel, ET);
            elementFinder.ObjectChosen += ElementFinder_ElementChosen;
            elementFinder.BracketSettingsChanged += BracketSettingsChanged;
            elementFinder.VariableSettingsChanged += ElementFinder_VariableSettingsChanged;
            elementFinder.VisSettingsChanged += VisSettingsChanged;
        }

        public void StopEditing()
        {
            if (interactionMode != InteractionMode.editing) return;

            MainForm.ActiveMainForm.FreezeScroll();

            interactionMode = previousMode;
            elementFinder.ObjectChosen -= ElementFinder_ElementChosen;
            elementFinder.BracketSettingsChanged -= BracketSettingsChanged;
            elementFinder.VariableSettingsChanged -= ElementFinder_VariableSettingsChanged;
            elementFinder.VisSettingsChanged -= VisSettingsChanged;
            Focus();
            elementFinder.Hide();

            UpdateVisualisationFromExpression();

            MainForm.ActiveMainForm.ContinueScroll();
        }

        public void UpdateVisualisationFromExpression()
        {
            if(Expression != null)
            Visualisation = Expression.GetVisualisation();
        }

        public void UpdateEditedText(EditableText text)
        {
            if (text == null) return;
            if (interactionMode != InteractionMode.editing) return;

            if (activeSubVis.VisualisedObject is MPlaceholderFormula PF)
            {
                PF.SetText(text.DisplayText);
                Redraw(true);
            }
            else if (activeSubVis.VisualisedObject is MPlaceholderTerm PT)
            {
                PT.SetText(text.DisplayText);
                Redraw(true);
            }
            // TODO: PlaceholderVariable
        }

        private void ReplaceActiveSub(MExpression New)
        {
            if (Expression == null) return;

            if (activeSubVis == Visualisation)
            {
                Expression = LinkOnly ? New : New.Copy() as MExpression;
                CompleteChange?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                Expression.ChangeSub(activeSubVis.path, New.Copy() as MExpression);
                ExpressionChanged?.Invoke(this, new EventArgs());
            }
        }

        private void ElementFinder_ElementChosen(object sender, ObjectChosenEventArgs e)
        {
            if (Expression == null) throw new Exception("Dev fucked up, Expression should not be null");

            if (e.Object is MStatement S)
            {
                if (S._X.Document != MainForm.ActiveMainForm.Document) MainForm.ActiveMainForm.Document.ReferenceDocument(S._X.Document);
                if (S._F != null) S._F.ReferenceInDocument(MainForm.ActiveMainForm.Document);
                SetStatement(S);
                CompleteChange?.Invoke(this, EventArgs.Empty);
            }
            else if (e.Object is MExpression E)
            {
                E.ReferenceInDocument(MainForm.ActiveMainForm.Document);
                ReplaceActiveSub(E);
            }

            StopEditing();
        }

        private void BracketSettingsChanged(object sender, BracketMode e)
        {
            if (Expression == null) throw new Exception("Dev fucked up, Expression should not be null");

            if (activeSubVis.VisualisedObject is MExpression E)
            {
                E.BracketMode = e;
                UpdateVisualisationFromExpression();
            }
            else
            {
                activeSubVis.BracketMode = e;
                Redraw(true);
            }
            //StopEditing();
        }

        private void ElementFinder_VariableSettingsChanged(object sender, (MVariable v, bool val) e)
        {
            if (Statement == null) throw new Exception("Dev fucked up, Expression should not be null");

            if (e.val) Statement.AddRestrictedVariable(e.v);
            else Statement.RemoveRestrictedVariable(e.v);

            Redraw(true);
            //StopEditing();
        }

        private void VisSettingsChanged(object sender, EventArgs e)
        {
            MExpression Ex = (activeSubVis?.VisualisedObject as MExpression);
            if (Ex == null || Ex._D == null) throw new Exception();

            if (Ex.VisualizationID == Ex._D.visualisations.Count - 1)
                Ex.VisualizationID = 0;
            else
                Ex.VisualizationID++;

            UpdateVisualisationFromExpression();
        }

        public void ClearDrawOptions(MDrawOptions.Tag Tag = MDrawOptions.Tag.all)
        {
            DrawOptions.Clear(Tag);
        }

        public void AddSubVisHighlight(MVisualisation subVis, Color color, MDrawOptions.Tag Tag = MDrawOptions.Tag.unspecified)
        {
            DrawOptions.AddVSpecification(subVis, new SolidBrush(color), Brushes.Black, Pens.Black, Tag);
        }

        public void AddSymbolHighlight(MVisualisation subVis, int symbolIndex, MDrawOptions.Tag Tag = MDrawOptions.Tag.unspecified)
        {
            if(Tag == MDrawOptions.Tag.editinghighlight)
                DrawOptions.AddSSpecification(subVis, symbolIndex, new SolidBrush(Properties.Settings.Default.SymbolHoverHighlight), Brushes.White, Pens.Black, Tag);
            else
                DrawOptions.AddSSpecification(subVis, symbolIndex, Brushes.White, Brushes.Black, Pens.Black, Tag);
        }

        private void VisualisationDisplay_Paint(object sender, PaintEventArgs e)
        {
            //redraw();
        }

        private void VisualisationDisplay_Click(object sender, EventArgs e)
        {
            HoveredSubV = v.GetTopHovered(PointToClient(Cursor.Position));
            if (HoveredSubV != null)
            {
                if (LinkButton.IsLinking)
                {
                    switch (LinkButton.LinkMode)
                    {
                        case LinkMode.Statement:
                            LinkButton.LinkObject(Statement);
                            break;
                        default:
                            LinkButton.LinkObject(HoveredSubV.VisualisedObject);
                            break;
                    }

                    return;
                }

                if (Editable)
                {
                    StopEditing();
                    if (LinkOnly) HoveredSubV = Visualisation;
                    StartEditing(HoveredSubV);
                }

                if (interactionMode == InteractionMode.symbols)
                {
                    int hoveredSymbol = HoveredSubV.GetTopHoveredSymbolIndex(PointToClient(Cursor.Position));
                    ClearDrawOptions(MDrawOptions.Tag.all);
                    if (hoveredSymbol != -1)
                    {
                        AddSymbolHighlight(HoveredSubV, hoveredSymbol, MDrawOptions.Tag.editinghighlight);
                        SymbolSelected?.Invoke(this, new MClickEventArgs(HoveredSubV, hoveredSymbol));
                    }
                }
            }
        }

        private void SetActiveSubVis(MVisualisation vis)
        {
            activeSubVis = vis;
            ClearDrawOptions();
            DrawOptions.AddVSpecification(activeSubVis, Brushes.LightGray, Brushes.Black, Pens.DarkBlue);
            Redraw(false);
        }

        private void HandleMClick(object sender, EventArgs e)
        {
            MVisualisation HoveredSubV = v.GetTopHovered(PointToClient(Cursor.Position));
            
            int hoveredSymbol = HoveredSubV != null ? HoveredSubV.GetTopHoveredSymbolIndex(PointToClient(Cursor.Position)) : -1;

            MClickEventArgs ea = new MClickEventArgs(HoveredSubV, hoveredSymbol);
            
            OnMClick(ea);
        }

        protected virtual void OnMClick(MClickEventArgs e)
        {
            MClick?.Invoke(this, e);
        }

        private void VisualisationDisplay_MouseMove(object sender, MouseEventArgs e)
        {
            if ((DateTime.Now - lastUpdate).Milliseconds < minRestTime) return;
            lastUpdate = DateTime.Now;

            if (interactionMode == InteractionMode.passive) return;
            if (v == null) return;

            ClearDrawOptions(MDrawOptions.Tag.hoverhighlight);

            HoveredSubV = v.GetTopHovered(PointToClient(Cursor.Position));
            if (HoveredSubV != null)
            {
                if (interactionMode == InteractionMode.symbols) // This is only for the VisualisationDesigner
                {
                    int hoveredSymbol = HoveredSubV.GetTopHoveredSymbolIndex(PointToClient(Cursor.Position));
                    if (hoveredSymbol != -1)
                        AddSymbolHighlight(HoveredSubV, hoveredSymbol, MDrawOptions.Tag.hoverhighlight);
                }
                else if (LinkButton.IsLinking) // Highlight if HoveredSubV can be linked
                {
                    bool allowed = false;

                    allowed = LinkButton.LinkMode.AllowsFor(HoveredSubV.VisualisedObject);

                    if(LinkButton.LinkMode == LinkMode.Statement && Statement != null) { HoveredSubV = Visualisation; allowed = true; }

                    if(allowed)
                        AddSubVisHighlight(HoveredSubV, Properties.Settings.Default.LinkHoverHighlight, MDrawOptions.Tag.hoverhighlight);
                }
                else if(Editable) // Highlight if HoveredSubV can be edited
                {
                    if (LinkOnly) HoveredSubV = Visualisation;

                    if (interactionMode == InteractionMode.subVis || interactionMode == InteractionMode.editing)
                        AddSubVisHighlight(HoveredSubV, Properties.Settings.Default.HoverHighlight, MDrawOptions.Tag.hoverhighlight);
                }
            }
            Redraw(false);
        }

        private void VisualisationDisplay_MouseLeave(object sender, EventArgs e)
        {
            if (interactionMode == InteractionMode.passive) return;
            if (v == null) return;
            ClearDrawOptions(MDrawOptions.Tag.hoverhighlight);
            Redraw(true);
        }

        private void pB_MouseMove(object sender, MouseEventArgs e)
        {
            VisualisationDisplay_MouseMove(sender, e);
        }

        private void pB_MouseLeave(object sender, EventArgs e)
        {
            VisualisationDisplay_MouseLeave(sender, e);
            MainForm.ActiveMainForm.Unhover();
        }

        private void VisualisationDisplay_SizeChanged(object sender, EventArgs e)
        {
            //if (Parent is FlowLayoutPanel) redraw();
        }

        private void VisualisationDisplay_KeyDown(object sender, KeyEventArgs e)
        {
            //Debug.WriteLine("Key Down at " + Expression?.ToString() ?? "Unknown");
        }

        private void pB_MouseEnter(object sender, EventArgs e)
        {
           if(LinkButton.IsLinking) Focus(); // this is to prevent sudden scrolling when linking.

            MainForm.ActiveMainForm.Hover(this);
        }

        internal void Key(object sender, KeyEventArgs e)
        {
            //Debug.WriteLine("Key down at " + Expression.ToString());
            HoveredSubV = v.GetTopHovered(PointToClient(Cursor.Position));
            if (HoveredSubV == null) return;

            char? c = KeyboardHelper.GetCharFromKey(e.KeyData);
            activeSubVis = HoveredSubV;
            if(c== 'v')
            {
                VisSettingsChanged(sender, EventArgs.Empty);
                return;
            }
            if (c == 'b')
            {
                BracketMode bm;
                switch (activeSubVis.BracketMode)
                {
                    case BracketMode.ManualYes:
                        bm = BracketMode.ManualNo;
                        break;
                    case BracketMode.ManualNo:
                        bm = BracketMode.Auto;
                        break;
                    default:
                        bm = BracketMode.ManualYes;
                        break;
                }

                BracketSettingsChanged(sender, bm);
            }
        }
    }

    public class EditableText
    {
        public string Text { get; private set; }
        public int Position { get; set; }
        public string DisplayText { get
            {
                if (Text == "") return "_";
                if(PositionIsLegal)
                {
                    return Text.Insert(Position, "|");
                }
                return Text;
            } }
        private bool PositionIsLegal { get { return Position >= 0 && Position <= Text.Length; } }

        public void Insert(string C) { if (!PositionIsLegal) return; Text = Text.Insert(Position, C); Position++; }
        public void Insert(char C) { Insert(C.ToString()); }

        public void Backspace()
        {
            if (!PositionIsLegal || Position == 0) return;

            Position--;
            Text = Text.Remove(Position);
        }

        public void Delete()
        {
            if (!PositionIsLegal || Position == Text.Length) return;

            Text = Text.Remove(Position);
        }

        public static EditableText FromString(string S = "")
        {
            EditableText ET = new EditableText()
            {
                Text = S,
                Position = 0
            };
            return ET;
        }

        private EditableText() { }
    }

    public class MClickEventArgs : EventArgs
    {
        public MVisualisation Visualisation;
        public int symbolIndex;

        public MClickEventArgs (MVisualisation vis, int sym) : base()
        {
            Visualisation = vis;
            symbolIndex = sym;
        }
    }
}
