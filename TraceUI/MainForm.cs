using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using TraceBackend;
using System.IO;

namespace TraceUI
{
    public class NoStupidScrollPanel : Panel
    {
        protected override Point ScrollToControl(Control activeControl)
        {
            return DisplayRectangle.Location;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // NoStupidScrollPanel
            // 
            this.Leave += new System.EventHandler(this.NoStupidScrollPanel_Leave);
            this.ResumeLayout(false);

        }

        private void NoStupidScrollPanel_Leave(object sender, EventArgs e)
        {

        }
    }

    public partial class MainForm : Form
    {
        public MDocument Document;
        internal static List<MainForm> MainForms;
        public static ApplicationContext appContext;
        static float real_PPI;
        static float zoom;
        public static float PPI;
        public static MainForm ActiveMainForm;
        Point CurrentPoint;
        VisualisationDisplay HoveredVD;
        int freezeCount = 0;

        static MainForm()
        {
            MainForms = new List<MainForm>();
        }

        public MainForm()
        {
            InitializeComponent();
            /*this.DocumentPanel.HorizontalScroll.Maximum = 0;
            this.DocumentPanel.AutoScroll = false;
            this.DocumentPanel.HorizontalScroll.Visible = false;
            this.DocumentPanel.AutoScroll = true;
            */
            DocumentPanel.Visible = false;
            MainForms.Add(this);

            ActiveMainForm = this;

            if(Path.GetExtension(Application.StartupPath) == ".tr")
                UIFromDocument(MDocumentManager.LoadDocument(Application.StartupPath));
        }

        public MainForm(MDocument D) : this()
        {
            UIFromDocument(D);
        }

        private void MainForm_Closing(object sender, EventArgs e)
        {
            MainForms.Remove(this);
            if (MainForms.Count != 0)
                appContext.MainForm = MainForms[0];
        }


        public void FreezeScroll()
        {
            freezeCount++;
            if (freezeCount > 1) return;
            CurrentPoint = new Point();
            CurrentPoint = PagePanel.AutoScrollPosition;
            PagePanel.SuspendLayout();
        }

        public void ContinueScroll()
        {
            freezeCount--;
            if (freezeCount > 0) return;
            PagePanel.AutoScrollPosition = new Point(Math.Abs(PagePanel.AutoScrollPosition.X), Math.Abs(CurrentPoint.Y));
            PagePanel.ResumeLayout();
            PagePanel.HorizontalScroll.Visible = false;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            zoom = 1.0F;
            Graphics g = CreateGraphics();
            real_PPI = g.DpiX;
            PPI = real_PPI * zoom;

            DocumentPanel.SetWidth(PagePanel.ClientRectangle.Width - 40);
            PagePanel.HorizontalScroll.Visible = false;
        }

        public void UIFromDocument(MDocument D)
        {
            if (D == null) return;
            if(Document != null)
            {
                MainForm MF = new MainForm(D);
                MF.Show();
                return;
            }
            Document = D;

            DocumentPanel.Visible = true;

            foreach (DocumentStructure X in Document.Structure.Children)
            {
                if (X.Element is MContext)
                    foreach (DocumentStructure DS in X.Children)
                    {
                        if (DS.Element is MDefinition Def)
                        {
                            DocumentElement DE = DefinitionDocumentElement.FromDefinition(Def, DS);
                            DocumentPanel.InsertRow(DE);
                            DE.Show();
                        }
                        else if (DS.Element is MStatement Ax)
                        {
                            DocumentElement DE = StatementDocumentElement.FromStatement(Ax, DS);
                            DocumentPanel.InsertRow(DE);
                            DE.Show();
                        }
                        else if (DS.Element is MTheorem T)
                        {
                            DocumentElement DE = TheoremDocumentElement.FromTheorem(T, DS);
                            DocumentPanel.InsertRow(DE);
                            DE.Show();
                        }
                        else if (DS.Element is MDocumentText Tx)
                        {
                            TextControl TC = TextControl.FromDocumentText(Tx);
                            DocumentPanel.InsertRow(TC);
                            TC.Show();
                        }
                    }
                else if (X.Element is MDocumentText Tx)
                {
                    TextControl TC = TextControl.FromDocumentText(Tx);
                    DocumentPanel.InsertRow(TC);
                    TC.Show();
                }
            }

            DocumentPanel.SetWidth(PagePanel.Width - 40);

            PagePanel.HorizontalScroll.Visible = false;
        }

        public void Hover(VisualisationDisplay VD)
        {
            HoveredVD = VD;
        }

        public void Unhover()
        {
            HoveredVD = null;
        }

        private void MainForm_Click(object sender, EventArgs e)
        {

        }

        private void MainForm_GotFocus(object sender, EventArgs e)
        {
            ActiveMainForm = this;
            Text = "Trace (ACTIVE)";
        }

        private void MainForm_LostFocus(object sender, EventArgs e)
        {
            Text = "Trace";
        }

        private void NewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UIFromDocument(MDocumentManager.CreateDocument());
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (OpenDialog.ShowDialog() == DialogResult.OK)
            { 
                UIFromDocument(MDocumentManager.LoadDocument(OpenDialog.FileName));
            }
        }

        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (HoveredVD == null) return;

            HoveredVD.Key(sender, e);
        }

        private void PagePanel_SizeChanged(object sender, EventArgs e)
        {
            DocumentPanel.SetWidth(PagePanel.ClientRectangle.Width - 5);
        }

        private void OpenDialog_FileOk(object sender, CancelEventArgs e)
        {

        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(SaveDialog.ShowDialog() == DialogResult.OK)
            {
                Document.ToFile(SaveDialog.FileName);
            }
        }

        private void saveXMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SaveXMLDialog.ShowDialog() == DialogResult.OK)
            {
                Document.ToXML(SaveXMLDialog.FileName);
            }
        }

        private void latexToolStripMenuItem_Click(object sender, EventArgs e)
        {
            File.WriteAllText("latex.tex", Document.GetLaTeX());
        }

        private void DocumentPanel_HeightChanged(object sender, EventArgs e)
        {
            PagePanel.AutoScrollMinSize = new Size(0, DocumentPanel.Height + PagePanel.Height / 2);
        }
    }

    public static class CursorManager
    {
        static Cursor DefCursor;
        static Cursor OverrideCursor;

        public static Cursor InsertVertical;
        public static Cursor Link;
        public static Cursor LinkFormula;
        public static Cursor LinkTerm;
        public static Cursor LinkStatement;
        public static Cursor LinkVariable;
        public static Cursor LinkUndefinedPredicate;
        public static Cursor LinkQuantifier;
        public static Cursor LinkPredicate;

        static CursorManager()
        {
            DefCursor = Cursors.Default;

            InsertVertical = FromByteArray(Properties.Resources.CURinsertVertical);
            Link = FromByteArray(Properties.Resources.CURlink);
            LinkFormula = FromByteArray(Properties.Resources.CURlinkFormula);
            LinkTerm = FromByteArray(Properties.Resources.CURlinkTerm);
            LinkStatement = FromByteArray(Properties.Resources.CURlinkStatement);
            LinkVariable = FromByteArray(Properties.Resources.CURlinkVariable);
            LinkUndefinedPredicate = FromByteArray(Properties.Resources.CURlinkUndefinedPredicate);
            LinkQuantifier = FromByteArray(Properties.Resources.CURlinkQuantifier);
            LinkPredicate = FromByteArray(Properties.Resources.CURlinkPredicate);
        }

        public static Cursor FromByteArray(byte[] array)
        {
            using (MemoryStream memoryStream = new MemoryStream(array))
            {
                return new Cursor(memoryStream);
            }
        }

        public static void SetCursor(Cursor C)
        {
            DefCursor = C;
            if (OverrideCursor == null)
                foreach (MainForm mainForm in MainForm.MainForms)
                    mainForm.Cursor = DefCursor;
        }

        public static void UnsetCursor()
        {
            DefCursor = Cursors.Default;
            if (OverrideCursor == null)
                foreach (MainForm mainForm in MainForm.MainForms)
                    mainForm.Cursor = DefCursor;
        }

        public static void SetOverrideCursor(Cursor C)
        {
            OverrideCursor = C;
            foreach (MainForm mainForm in MainForm.MainForms)
               mainForm.Cursor = OverrideCursor; 
        }

        public static void UnsetOverrideCursor()
        {
            OverrideCursor = null;
            foreach (MainForm mainForm in MainForm.MainForms)
               mainForm.Cursor = DefCursor;
        }
    }
}
