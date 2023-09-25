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
    public partial class LinkButton : UserControl
    {
        static LinkButton CurrentlyLinkingButton;
        public static bool IsLinking { get { return CurrentlyLinkingButton != null; } }
        public static LinkMode LinkMode => CurrentlyLinkingButton?.Mode ?? LinkMode.Formula;

        private event EventHandler<LinkedEventArgs> Linked_;
        public event EventHandler<LinkedEventArgs> Linked { add { Linked_ += value; } remove { Linked_ -= value; } }

        public LinkMode Mode { get; set; }

        public LinkButton()
        {
            InitializeComponent();
        }

        public void StartLinking()
        {
            CurrentlyLinkingButton = this;
            switch (Mode)
            {
                case LinkMode.Formula:
                    CursorManager.SetOverrideCursor(CursorManager.LinkFormula);
                    break;
                case LinkMode.Statement:
                    CursorManager.SetOverrideCursor(CursorManager.LinkStatement);
                    break;
                case LinkMode.Term:
                    CursorManager.SetOverrideCursor(CursorManager.LinkTerm);
                    break;
                case LinkMode.Variable:
                    CursorManager.SetOverrideCursor(CursorManager.LinkVariable);
                    break;
                case LinkMode.UndefinedPredicate:
                    CursorManager.SetOverrideCursor(CursorManager.LinkUndefinedPredicate);
                    break;
                case LinkMode.Predicate:
                    CursorManager.SetOverrideCursor(CursorManager.LinkPredicate);
                    break;
                case LinkMode.Quantifier:
                    CursorManager.SetOverrideCursor(CursorManager.LinkQuantifier);
                    break;

                default:
                    CursorManager.SetOverrideCursor(CursorManager.Link);
                    break;
            }
        }

        public static void StopLinking()
        {
            CurrentlyLinkingButton = null;
            CursorManager.UnsetOverrideCursor();
        }

        public static void LinkObject(MObject O)
        {
            CurrentlyLinkingButton?.Link(O);
        }

        private void Link(MObject O)
        {
            if (O != null)
            {
                Linked_?.Invoke(this, new LinkedEventArgs(O));
                StopLinking();
            }
        }

        private void button_Click(object sender, EventArgs e)
        {
            if (IsLinking)
                StopLinking();
            else
                StartLinking();
        }
    }

    public class LinkedEventArgs : EventArgs
    {
        public MObject Object;

        public LinkedEventArgs(MObject O)
        {
            if (O is MStatement)
                Object = O;
            else if (O is MFormula F)
                Object = F;
            else if (O is MTerm T)
                Object = T;
            else
                Object = O;
        }
    }

    public enum LinkMode
    {
        Statement = 0,
        Formula = 1,
        Term = 2,
        Variable = 3,
        Quantifier = 4,
        Predicate = 5,
        UndefinedPredicate = 7
    }

    public static class LinkModeExtensions
    {
        public static bool AllowsFor(this LinkMode self, MObject obj)
        {
            switch (self)
            {
                case LinkMode.Statement:
                    return obj is MStatement;
                case LinkMode.Formula:
                    return obj is MFormula;
                case LinkMode.Term:
                    return obj is MTerm;
                case LinkMode.Variable:
                    return obj is MVariable;
                case LinkMode.Quantifier:
                    return obj is MQuantifierFormula;
                case LinkMode.Predicate:
                    return obj is MQuantifierFormula P && P.IsPredicate;
                case LinkMode.UndefinedPredicate:
                    return obj is MUndefinedPredicateFormula;
                default:
                    throw new Exception("Unknown LinkMode");

            }
        }
    }
}
