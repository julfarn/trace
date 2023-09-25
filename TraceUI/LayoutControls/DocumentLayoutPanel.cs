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
using System.IO;

namespace TraceUI
{
    public partial class DocumentLayoutPanel : UserControl
    {
        public int VerticalSpace { get; set; } = 5;
        public bool ListenToVisibility { get; set; } = false;

        public DocumentElement ParentDE;

        public event EventHandler WidthChanged;
        public event EventHandler HeightChanged;

        internal List<RowLayout> Rows;


        public MObject ParentObject => ParentDE?.Object;
        public DocumentStructure ParentStructure => ParentDE?.Structure;
        public DocumentPosition StructuralLocation => ParentDE is DeductionDocumentElement ? DocumentPosition.Deduction : 
            ParentDE is StatementListDocumentElement ? DocumentPosition.AxiomList :
            ParentDE is StatementDocumentElement ? DocumentPosition.Axiom :
            ParentDE is DefinitionDocumentElement ? DocumentPosition.Definition :
            ParentDE is TheoremDocumentElement ? DocumentPosition.Theorem :
            ParentDE == null ? DocumentPosition.Document :
            DocumentPosition.Nowhere;

        public enum DocumentPosition
        {
            Document,
            Deduction,
            AxiomList,
            Axiom,
            Definition,
            Theorem,
            Nowhere
        }
        
        public DocumentLayoutPanel()
        {
            InitializeComponent();
            Rows = new List<RowLayout>();
        }

        public void SetWidth(int W)
        {
            Size = new Size(W, Height);

            WidthChanged?.Invoke(this, new EventArgs());
        }

        public void UpdateHeight(int recalculateFrom = -1)
        {
            if (!Visible) return;

           // MainForm.ActiveMainForm.FreezeScroll();

            if (Rows.Count == 0) { Size = new Size(Width, VerticalSpace); return; }

            if(recalculateFrom >= 0)
            {
                for (int i = recalculateFrom; i < Rows.Count; i++)
                {
                    UpdateRowLocation(i);
                }
            }

            Size = new Size(Width, GetRowPosition(Rows.Count));
            
            HeightChanged?.Invoke(this, EventArgs.Empty);

           // MainForm.ActiveMainForm.ContinueScroll();
        }

        public void InsertRow(Control C, int index = -1) { InsertRow(new RowLayout(C), index); }

        public void InsertRow(RowLayout Row, int index = -1)
        {
            foreach (Control C in Row.Elements)
            {
                Controls.Add(C);
            }

            if(index == -1 || index >= Rows.Count)
            {
                Rows.Add(Row);
                index = Rows.Count - 1;
            }
            else
            {
                Rows.Insert(index, Row);
            }

            Row.SizeChanged += Row_SizeChanged;
            Row.VisibleChanged += Row_VisibleChanged;
            WidthChanged += Row.RaiseWidthChanged;
            Row.RaiseWidthChanged(this, EventArgs.Empty);

            Row.Show();

            UpdateHeight(index);
        }

        public void RemoveRow(Control C, bool dispose)
        {
            for(int i= 0; i< Rows.Count; i++)
            {
                if(Rows[i].Elements.Length>0 && Rows[i].Elements[0] == C)
                {
                    RemoveRow(i, dispose);
                    return;
                }
            }
        }

        public void RemoveRow(RowLayout Row, bool dispose)
        {
            RemoveRow(Rows.IndexOf(Row), dispose);
        }

        public void RemoveRow(int index, bool dispose)
        {
            foreach (Control C in Rows[index].Elements)
                RemoveElement(C, dispose);

            Rows[index].SizeChanged -= Row_SizeChanged;
            Rows[index].VisibleChanged -= Row_VisibleChanged;
            WidthChanged -= Rows[index].RaiseWidthChanged;

            Rows.RemoveAt(index);

            UpdateHeight(index);
        }

        public void HideRow(Control C)
        {
            for (int i = 0; i < Rows.Count; i++)
            {
                if (Rows[i].Elements.Length > 0 && Rows[i].Elements[0] == C)
                {
                    HideRow(i);
                    return;
                }
            }
        }

        public void HideRow(RowLayout Row)
        {
            HideRow(Rows.IndexOf(Row));
        }

        public void HideRow(int index)
        {
            HiddenRow HidingBlock;

            //Is there a Hidden Block BEFORE this row?
            if (index != 0 && Rows[index - 1].Hidden)
            {
                HidingBlock = Rows[index - 1].HidingBlock;
                HidingBlock.HideRow(Rows[index]);
            }
            else
            {
                //Create new HidingBlock
                HidingBlock = HiddenRow.Create(Rows[index]);
                ReplaceRow(Rows[index], HidingBlock, false);
                HidingBlock.Click += HidingBlock_Click;
            }
            if (index != Rows.Count - 1 && Rows[index + 1].Hidden) // There is a HiddenBlock AFTER this row.
            {
                //Join the two Hiding Blocks
                HiddenRow hrBelow = Rows[index + 1].HidingBlock;
                ReplaceRow(hrBelow, hrBelow.HiddenRows[0], false);
                HidingBlock.Append(hrBelow);
                hrBelow.Click -= HidingBlock_Click;
            }
        }

        private void HidingBlock_Click(object sender, EventArgs e)
        {
            Unhide(sender as HiddenRow);
        }

        public void Unhide(HiddenRow Row)
        {
            ReplaceRow(Row, Row.HiddenRows[0], false);
            Row.Dissolve();
            Row.Click -= HidingBlock_Click;
        }

        public void ReplaceRow(RowLayout Old, RowLayout New, bool dispose)
        {
            int index = Rows.IndexOf(Old);
            RemoveRow(index, dispose);
            InsertRow(New, index);
        }

        private void Row_VisibleChanged(object sender, EventArgs e)
        {
            if (ListenToVisibility)
            {
                int index = Rows.IndexOf(sender as RowLayout);

                UpdateHeight(index);
            }
        }

        private void Row_SizeChanged(object sender, EventArgs e)
        {
            int index = Rows.IndexOf(sender as RowLayout);
            
            UpdateHeight(index);
        }

        public void RemoveElement(Control C, bool dispose)
        {
            RemoveElement(Controls.IndexOf(C), dispose);
        }

        public void RemoveElement(int index, bool dispose)
        {
            Control C = Controls[index];
            Controls.RemoveAt(index);
            C.SizeChanged -= Row_SizeChanged;
            if(C is DocumentElement DE)
                WidthChanged -= DE.ContainerWidthChanged;
            C.Hide();
            if(dispose)C.Dispose();
            
            UpdateHeight(index);
        }

        private void UpdateRowLocation(int index)
        {
            Rows[index].Reposition(GetRowPosition(index));
        }

        private int GetRowPosition(int index)
        {
            if (index == 0) return VerticalSpace;

            if (!(Rows[index - 1].Visible)) return Rows[index - 1].Position;

            return Rows[index - 1].Position + Rows[index - 1].Height + VerticalSpace;
        }

        /// <summary>
        /// Checks if the given position lies in a gap between rows.
        /// </summary>
        /// <param name="position">y-Coordinate</param>
        /// <returns>Index of the next row if position is in a gap, -1 if not.</returns>
        private int InbetweenRows(int position)
        {
            if (position < 0) return -1;

            for(int i = 0; i< Rows.Count; i++)
            {
                while (!(Rows[i] is HiddenRow) && Rows[i].Hidden && i < Rows.Count - 1) i++;
                if (position < Rows[i].Position) return i;
                if (position < Rows[i].Position + Rows[i].Height) return -1;
            }

            return Rows.Count;
        }

        private void ClearAllInsertionRows()
        {
            for(int i = 0; i< Rows.Count; i++)
                if (Rows[i] is InsertionRow)
                    RemoveRow(Rows[i], true);
        }

        private void DocumentLayoutPanel_Load(object sender, EventArgs e)
        {

        }

        private void DocumentLayoutPanel_Click(object sender, EventArgs e)
        {
            ClearAllInsertionRows();
            int ind = InbetweenRows(PointToClient(Cursor.Position).Y);
            if (ind == -1) return;

            InsertionBar IB;
            switch (StructuralLocation)
            {
                case DocumentPosition.Document:
                    IB = InsertionBar.InDocument(this, (ind > 0) ? Rows[ind - 1].BackendStructure : null);
                    break;
                case DocumentPosition.Deduction:
                    IB = InsertionBar.InDeduction(this, ParentObject as MDeduction, (ind > 0) ? Rows[ind - 1].BackendStructure : ParentStructure);
                    break;
                case DocumentPosition.AxiomList:
                    IB = InsertionBar.InAxiomList(this, ParentObject as MDefinition, (ind > 0) ? Rows[ind - 1].BackendStructure : ParentStructure);
                    break;
                case DocumentPosition.Axiom:
                    IB = null;
                    break;
                case DocumentPosition.Definition:
                    IB = InsertionBar.InDefinition(this, ParentObject as MDefinition, (ind > 0) ? Rows[ind - 1].BackendStructure : ParentStructure);
                    break;
                case DocumentPosition.Theorem:
                    IB = InsertionBar.InTheorem(this, ParentObject as MTheorem, (ind > 0) ? Rows[ind - 1].BackendStructure : ParentStructure);
                    break;

                default:
                    IB = null;
                    break;
            }

            if (IB != null)
            {
                InsertionRow IR = new InsertionRow(IB);
                InsertRow(IR, ind);
            }
        }

        private void DocumentLayoutPanel_MouseEnter(object sender, EventArgs e)
        {
            CursorManager.SetCursor(CursorManager.InsertVertical);
        }

        private void DocumentLayoutPanel_MouseLeave(object sender, EventArgs e)
        {
            CursorManager.UnsetCursor();
        }
    }

    public class RowLayout
    {
        public event EventHandler WidthChanged;
        public event EventHandler SizeChanged;
        public event EventHandler VisibleChanged;

        public Control[] Elements;
        public Alignment Align;
        public enum Alignment { Left, Center, Right }
        public DocumentStructure BackendStructure => (Elements[0] as DocumentElement)?.Structure ?? (Elements[0] as VisualisationDisplay)?.Structure ?? (Elements[0] as TextControl)?.Structure;
        public MObject BackendObject => BackendStructure.Element as MObject;

        public int Position;
        public int Height { get { int hmax = 0; foreach (Control C in Elements) if (C.Height > hmax && C.Visible) hmax = C.Height; return hmax; } }
        //public int Height => Elements[0].Height;
        public bool Visible { get { bool vis = false; foreach (Control C in Elements) vis = vis || C.Visible;
                return vis; } }
        public bool Hidden = false;
        public HiddenRow HidingBlock;

        private void SetUp()
        {
            foreach(Control E in Elements)
            {
                E.SizeChanged += Element_SizeChanged;
                E.VisibleChanged += Element_VisibleChanged;

                if (E is DocumentElement DE)
                    WidthChanged += DE.ContainerWidthChanged;
                if (E is ElementDisplay ED)
                {
                    WidthChanged += ED.ContainerWidthChanged;
                    //ED.ContainerWidthChanged(this, new EventArgs());
                }
            }
        }

        public void Show()
        {
            foreach (Control C in Elements) C.Show();
        }

        public void Hide() { foreach (Control C in Elements) C.Hide(); }

        internal void RaiseWidthChanged(object sender, EventArgs e)
        {
            WidthChanged?.Invoke(sender, e);
        }

        private void Element_VisibleChanged(object sender, EventArgs e)
        {
            VisibleChanged?.Invoke(this, e);
        }

        private void Element_SizeChanged(object sender, EventArgs e)
        {
            SizeChanged?.Invoke(this, e);
        }

        public RowLayout(Control el = null, Alignment al = Alignment.Left)
        {
            Align = al;
            Elements = new Control[] { el };
            SetUp();
        }
        public RowLayout(Control e1, Control e2, Alignment al = Alignment.Left)
        {
            Align = al;
            Elements = new Control[] { e1, e2 };
            SetUp();
        }
        public RowLayout(Control e1, Control e2, Control e3, Alignment al = Alignment.Left)
        {
            Align = al;
            Elements = new Control[] { e1, e2, e3 };
            SetUp();
        }
        public RowLayout(Control e1, Control e2, Control e3, Control e4, Alignment al = Alignment.Left)
        {
            Align = al;
            Elements = new Control[] { e1, e2, e3, e4 };
            SetUp();
        }

        public void Reposition(int Y = -1) //TODO ?
        {
            if (Y == -1) Y = Position;
            Position = Y;

            Elements[0].Location = new Point(0, Y);
            for (int i = 1; i < Elements.Length; i++)
                Elements[i].Location = new Point(Elements[i - 1].Location.X + Elements[i-1].Width + 20, Y); 
        }
    }

    public class InsertionRow : RowLayout
    {
        public InsertionRow(InsertionBar Bar): base(Bar)
        {
            InsertionBar.lastCreated.Row = this;
        }
    }

    public class HiddenRow : RowLayout
    {
        public event EventHandler Click;

        public List<RowLayout> HiddenRows;
        private PictureBox PB => Elements[0] as PictureBox;
        public new DocumentStructure BackendStructure => HiddenRows[0]?.BackendStructure;

        public static HiddenRow Create(RowLayout Row = null)
        {
            HiddenRow HR = new HiddenRow(new PictureBox());
            if (Row != null)
            {
                HR.HideRow(Row);
            }
            return HR;
        }

        private HiddenRow(Control C) : base(C)
        {
            HiddenRows = new List<RowLayout>();
            Hidden = true;
            HidingBlock = this;
            PB.Click += PB_Click;
            PB.Size = new Size(200, 10);
        }

        private void PB_Click(object sender, EventArgs e)
        {
            Click?.Invoke(this, EventArgs.Empty);
        }

        public void HideRow(RowLayout Row)
        {
            HiddenRows.Add(Row);
            Row.Hide();
            Row.Hidden = true;
            Row.HidingBlock = this;
            Redraw();
        }

        public void Append(HiddenRow other)
        {
            foreach(RowLayout row in other.HiddenRows)
            {
                HiddenRows.Add(row);
                row.Hide();
                row.HidingBlock = this;
            }
            Redraw();
        }

        public void Dissolve()
        {
            foreach(RowLayout Row in HiddenRows)
            {
                Row.Show();
                Row.Hidden = false;
                Row.HidingBlock = null;
                if (Row.BackendStructure != null)
                    Row.BackendStructure.Hidden = false;
            }
            HiddenRows = null;
        }

        private void Redraw()
        {
            Bitmap b = new Bitmap(PB.ClientSize.Width, PB.ClientSize.Height);
            Graphics g = Graphics.FromImage(b);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            g.Clear(Color.White);

            int dist = 10;

            Pen col = Pens.Gray;

            for (int i = 0; i < HiddenRows.Count; i++)
                g.DrawEllipse(col, i * dist + dist - 3, 2, 7, 7);

            PB.Image = b;
        }
    }
}
