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
    public partial class ElementDisplay : UserControl
    {
        public bool Highlighted { get { return BorderStyle == BorderStyle.FixedSingle; } set { BorderStyle = value ? BorderStyle.FixedSingle : BorderStyle.None; } }
        public MDefinition Definition { private set; get; }
        public MVariable Variable { private set; get; }
        public new event EventHandler Click
        {
            add
            {
                base.Click += value;
                base.DoubleClick += value;
                foreach (Control control in Controls)
                {
                    control.Click += value;
                    control.DoubleClick += value;
                }
            }
            remove
            {
                base.Click -= value;
                base.DoubleClick -= value;
                foreach (Control control in Controls)
                {
                    control.Click -= value;
                    control.DoubleClick -= value;
                }
            }
        }

        public ElementType type;
        private ToolTip toolTip;

        public static ElementDisplay FromDefinition(MDefinition D)
        {
            ElementDisplay DD = new ElementDisplay();
            DD.visualisationDisplay.Visualisation = D.GetPlaceholderVisualisation(null);
            DD.label.Text = D.stringSymbol;
            DD.Definition = D;

            if (D is MFunctionSymbol) DD.type = ElementType.Term;
            else if (D is MQuantifier) DD.type = ElementType.QuantifierDefinition;
            else DD.type = ElementType.Formula;

            return DD;
        }

        internal static ElementDisplay FromVariable(MVariable V)
        {
            ElementDisplay DD = new ElementDisplay();
            DD.visualisationDisplay.Visualisation = V.GetVisualisation();
            DD.label.Text = V.stringSymbol;
            DD.Variable = V;
            DD.type = ElementType.Variable;

            return DD;
        }

        internal static ElementDisplay FromSymbol(MShapeSymbol S)
        {
            ElementDisplay DD = new ElementDisplay();
            DD.visualisationDisplay.Visualisation = S.GetVisualisation();
            DD.label.Text = S.name;
            DD.type = ElementType.Symbol;

            return DD;
        }

        internal MObject GetElement()
        {
            if (Variable != null)
                return Variable;
            if (Definition != null)
                return Definition.GetPlaceholderVisualisation(null).VisualisedObject.Copy();

            return visualisationDisplay.Visualisation.VisualisedObject.Copy();
        }

        public ElementDisplay()
        {
            InitializeComponent();
            toolTip = new ToolTip();
            toolTip.AutoPopDelay = 1000000;
            toolTip.SetToolTip(label, "");
        }

        internal void ContainerWidthChanged(object sender, EventArgs e)
        {
            if (sender is DocumentLayoutPanel layoutPanel)
            {
                Size = new Size(layoutPanel.Width, Height);
            }
        }

        private void AdjustLayout()
        {
            label.Location = new Point(visualisationDisplay.Location.X + visualisationDisplay.Width + 5, (visualisationDisplay.Height - label.Height) / 2);
            Size = new Size(Width, visualisationDisplay.Location.Y +  visualisationDisplay.Height + 5);
        }

        private void visualisationDisplay_SizeChanged(object sender, EventArgs e)
        {
            AdjustLayout();
        }

        private void label_TextChanged(object sender, EventArgs e)
        {
            AdjustLayout();
        }

        private void label_MouseHover(object sender, EventArgs e)
        {
            toolTip.SetToolTip(label, Definition?.ToString() ?? Variable?.ToString() ?? "Error");
        }
    }

    public enum ElementType
    {
        Any = 0,
        Variable = 1,
        Term = 2,
        Formula = 3,
        Symbol = 4,
        UndefinedFormula = 5,
        UndefindedPredicate = 6,
        QuantifierDefinition = 7,
        PredicateDefinition = 8,
        Statement = 9
    }

    public static class ElementTypeExtensions
    {
        public static bool AllowsFor(this ElementType self, ElementType other)
        {
            if (self == other) return true;

            switch (self)
            {
                case ElementType.Any:
                    return true;
                case ElementType.Formula:
                    if (other == ElementType.PredicateDefinition ||
                        other == ElementType.QuantifierDefinition ||
                        other == ElementType.UndefindedPredicate ||
                        other == ElementType.UndefinedFormula)
                        return true;
                    else
                        return false;
                case ElementType.Term:
                    if (other == ElementType.Variable)
                        return true;
                    else
                        return false;
                default:
                        return false;
            }
        }
    }
}
