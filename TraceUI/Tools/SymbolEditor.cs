using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using TraceBackend;

namespace TraceUI
{
    public partial class SymbolEditor : UserControl
    {
        List<List<controlPointF>> PointList;

        bool selected = false;
        int selectedI, selectedJ;
        bool scrolling = false;
        PointF scrollStart, scrollMouseStart;
        float zoom = 1F;
        PointF pixelOffset;
        PointF center;
        SizeF size;

        class controlPointF
        {
            PointF p;
            public float X
            {
                get
                { return p.X; }
                set
                { p.X = value; }
            }
            public float Y
            {
                get
                { return p.Y; }
                set
                { p.Y = value; }
            }

            public controlType type;

            public controlPointF() { p = new PointF(); }
            
            public controlPointF(float x, float y) : this()
            { X = x; Y = y; }

            public void setPointF(PointF point)
            { p = point; }

            public static controlPointF fromPointF(PointF point)
            {
                controlPointF p = new controlPointF();
                p.setPointF(point);
                return p;
            }

            public PointF getPointF() { return p; }

            public enum controlType
            {
                normal = 0,
                corner = 1,
                assisting = 2
            }
        }

        public SymbolEditor()
        {
            InitializeComponent();
        }

        private void centerNsize()
        {
            PointF TopLeftMost = new PointF(10000F, 10000F);
            PointF BotRightMost = new PointF(-10000F, -10000F);

            for (int i = 0; i < PointList.Count; i++)
            {
                for (int j = 0; j < PointList[i].Count; j++)
                {
                    if (TopLeftMost.X > PointList[i][j].X) TopLeftMost.X = PointList[i][j].X;
                    if (TopLeftMost.Y > PointList[i][j].Y) TopLeftMost.Y = PointList[i][j].Y;
                    if (BotRightMost.X < PointList[i][j].X) BotRightMost.X = PointList[i][j].X;
                    if (BotRightMost.Y < PointList[i][j].Y) BotRightMost.Y = PointList[i][j].Y;
                }
            }

            size = new SizeF(BotRightMost.X - TopLeftMost.X, BotRightMost.Y - TopLeftMost.Y);
            center = new PointF(TopLeftMost.X + size.Width / 2F, TopLeftMost.Y + size.Height / 2F);
        }

        private void makeCorner(int i, int j)
        {
            if (PointList[i][j].type == controlPointF.controlType.corner) return;

            controlPointF thi = PointList[i][j];
            controlPointF previous = PointList[i][getPreviousRealPoint(i, j)];
            controlPointF next = PointList[i][getNextRealPoint(i, j)];

            addAssistancePoint(i, j, thi, previous, 0.25F);
            addAssistancePoint(i, j+1, thi, previous, 0.125F);
            addAssistancePoint(i, j+2, thi, previous, 0.0625F);

            addAssistancePoint(i, j + 4, thi, next, 0.0625F);
            addAssistancePoint(i, j + 5, thi, next, 0.125F);
            addAssistancePoint(i, j + 6, thi, next, 0.25F);

            thi.type = controlPointF.controlType.corner;

            draw();
        }

        private void makeNormal (int i ,int j)
        {
            if (PointList[i][j].type == controlPointF.controlType.normal) return;
            PointList[i][j].type = controlPointF.controlType.normal;

            removeAssistancePoint(i, j + 1);
            removeAssistancePoint(i, j + 1);
            removeAssistancePoint(i, j + 1);

            removeAssistancePoint(i, j -3);
            removeAssistancePoint(i, j -3);
            removeAssistancePoint(i, j -3);
        }

        private void removeAssistancePoint(int i, int j)
        {
            if (PointList[i][j].type != controlPointF.controlType.assisting) return;
            PointList[i].RemoveAt(j);
        }

        private void updateCorner(int i, int j)
        {
            if (PointList[i][j].type != controlPointF.controlType.corner) return;

            controlPointF thi = PointList[i][j];
            controlPointF previous = PointList[i][getPreviousRealPoint(i, j)];
            controlPointF next = PointList[i][getNextRealPoint(i, j)];

            setAssistancePoint(i, j, -3, thi, previous);
            setAssistancePoint(i, j, -2, thi, previous);
            setAssistancePoint(i, j, -1, thi, previous);

            setAssistancePoint(i, j, 1, thi, next);
            setAssistancePoint(i, j, 2, thi, next);
            setAssistancePoint(i, j, 3, thi, next);
        }

        private void updateAllCorners()
        {
            for (int i = 0; i < PointList.Count; i++)
            {
                for (int j = 0; j < PointList[i].Count; j++)
                { updateCorner(i, j); }
            }
        }

        private void updateCornerAround(int i, int j)
        {
            updateCorner(i, j);
            updateCorner(i, getPreviousRealPoint(i, j));
            updateCorner(i, getNextRealPoint(i, j));
        }

        private void setAssistancePoint(int i, int j, int relJ, controlPointF root, controlPointF direction)
        {
            controlPointF p = PointList[i][makeGoodIndex(i, j + relJ)];
            if (p.type != controlPointF.controlType.assisting) return;

            float fraction = 0F;
            switch (relJ)
            {
                case -3:
                    fraction = 0.25F;
                    break;
                case -2:
                    fraction = 0.125F;
                    break;
                case -1:
                    fraction = 0.0625F;
                    break;

                case 1:
                    fraction = 0.0625F;
                    break;
                case 2:
                    fraction = 0.125F;
                    break;
                case 3:
                    fraction = 0.25F;
                    break;

                default: return;
            }

            p.X = root.X + fraction * (direction.X - root.X);
            p.Y = root.Y + fraction * (direction.Y - root.Y);
        }

        private void addAssistancePoint(int i, int j, controlPointF root, controlPointF direction, float fraction)
        {
            controlPointF p = new controlPointF(root.X + fraction * (direction.X - root.X), root.Y + fraction * (direction.Y - root.Y))
            {
                type = controlPointF.controlType.assisting
            };
            PointList[i].Insert(j, p);
        }

        private int getPreviousRealPoint(int i, int j)
        {
            for (int k = -1; k > -8; k--)
            {
                if (PointList[i][makeGoodIndex(i, j+k)].type != controlPointF.controlType.assisting)
                    return makeGoodIndex(i, j+k);
            }
            throw new Exception();
        }

        private int getNextRealPoint(int i, int j)
        {
            for (int k = 1; k < 8; k++)
            {
                if (PointList[i][makeGoodIndex(i, j+k)].type != controlPointF.controlType.assisting)
                    return makeGoodIndex(i, j+k);
            }
            throw new Exception();
        }

        private int makeGoodIndex(int i, int j)
        {
            while (j < 0) j += PointList[i].Count;
            while (j >= PointList[i].Count) j -= PointList[i].Count;
            return j;
        }

        private void WriteToFile(string name)
        {
            string path = "symbols\\" + name + ".sym";

            updateAllCorners();
            centerNsize();

            Stream s = File.OpenWrite(path);
            BinaryWriter w = new BinaryWriter(s);

            byte version = 0;
            byte shapeCount = (byte)PointList.Count;
            w.Write(version);
            w.Write(name);
            w.Write(size.Width);
            w.Write(size.Height);
            w.Write(shapeCount);

            for(int i= 0; i < shapeCount; i++)
            {
                ushort pointCount = (ushort)PointList[i].Count;
                w.Write(pointCount);
                for(int j = 0; j<pointCount;j++)
                {
                    w.Write(PointList[i][j].X - center.X);
                    w.Write(PointList[i][j].Y - center.Y);
                    w.Write((byte)PointList[i][j].type);
                }
            }

            w.Close();
            s.Close();
        }

        private void ReadFromFile(string name)
        {
            string path = "symbols\\" + name + ".sym";
            
            Stream s = File.OpenRead(path);
            BinaryReader r = new BinaryReader(s);
            
            byte version = r.ReadByte();
            r.ReadString();
            r.ReadSingle();
            r.ReadSingle();
            byte shapeCount = r.ReadByte();

            PointList = new List<List<controlPointF>>();
            
            for (int i = 0; i < shapeCount; i++)
            {
                ushort pointCount = r.ReadUInt16();
                
                PointList.Add(new List<controlPointF>());

                for (int j = 0; j < pointCount; j++)
                {
                    float x = r.ReadSingle();
                    float y = r.ReadSingle();
                    byte t = r.ReadByte();

                    controlPointF p = new controlPointF(x, y)
                    {
                        type = (controlPointF.controlType)t
                    };
                    PointList[i].Add(p);
                }
            }

            r.Close();
            s.Close();
        }
        
        private PointF[] drawTransformArray(int i)
        {
            PointF[] arr = new PointF[PointList[i].Count];
            for(int j = 0; j< arr.Length;j++)
                arr[j] = projection(i, j);
            return arr;
        }

        private PointF projection(int i, int j)
        {
            PointF p = new PointF(PointList[i][j].X, PointList[i][j].Y);
            p.X = p.X * MainForm.PPI * zoom + pixelOffset.X;
            p.Y = p.Y * MainForm.PPI * zoom + pixelOffset.Y;
            return p;
        }

        private PointF reverseProject(PointF p)
        {
            PointF ret = new PointF(p.X, p.Y);
            ret.X = (ret.X - pixelOffset.X) / MainForm.PPI / zoom;
            ret.Y = (ret.Y - pixelOffset.Y) / MainForm.PPI / zoom;
            return ret;
        }

        private PointF getMousePos()
        {
            return reverseProject(pictureBox1.PointToClient(Cursor.Position));
        }

        private void draw()
        {
            Bitmap b = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            Graphics g = Graphics.FromImage(b);
            g.Clear(Color.White);

            // reference lines
            g.DrawLine(Pens.Aqua, 0F, -0.088F * zoom * MainForm.PPI + pixelOffset.Y, pictureBox1.Width, -0.088F * zoom * MainForm.PPI + pixelOffset.Y);
            g.DrawLine(Pens.DarkBlue, 0F,-0.05F * zoom * MainForm.PPI + pixelOffset.Y, pictureBox1.Width, -0.05F * zoom * MainForm.PPI + pixelOffset.Y);
            g.DrawLine(Pens.DarkBlue, 0F, 0.05F * zoom * MainForm.PPI + pixelOffset.Y, pictureBox1.Width, 0.05F * zoom * MainForm.PPI + pixelOffset.Y);

            for (int i = 0; i < PointList.Count; i++)
            {
                PointF[] transformedPoints = drawTransformArray(i);
                g.FillClosedCurve(Brushes.Gray, transformedPoints, System.Drawing.Drawing2D.FillMode.Winding);
                for (int j = 0; j < transformedPoints.Length; j++)
                {
                    Pen p = Pens.Black;
                    if (PointList[i][j].type == controlPointF.controlType.assisting)
                        p = Pens.Gold;
                    if (selected && selectedI == i && selectedJ == j) p = Pens.Red;
                    if (PointList[i][j].type == controlPointF.controlType.normal)
                        g.DrawEllipse(p, transformedPoints[j].X - 5F, transformedPoints[j].Y - 5F, 10F, 10F);
                    if (PointList[i][j].type == controlPointF.controlType.corner)
                        g.DrawRectangle(p, transformedPoints[j].X - 5F, transformedPoints[j].Y - 5F, 10F, 10F);
                }
            }

            pictureBox1.Image = b;
        }

        private bool checkProximity(int i, int j, PointF p, float threshold)
        {
            if (PointList[i][j].type == controlPointF.controlType.assisting) return false;
            float dx = p.X - PointList[i][j].X;
            float dy = p.Y - PointList[i][j].Y;
            if (dx * dx + dy * dy <= threshold * threshold) return true;
            return false;
        }

        private float getTangent(int i, int j, PointF p)
        {
            if (PointList[i][j].type == controlPointF.controlType.assisting) return 1000F;
            PointF p1 = PointList[i][j].getPointF();
            PointF p2 = j < PointList[i].Count - 1 ? PointList[i][getNextRealPoint(i,j)].getPointF() : PointList[i][getNextRealPoint(i,0)].getPointF();

            float x1 = p2.X - p1.X;
            float x2 = p2.Y - p1.Y;
            float y1 = p.X - p1.X;
            float y2 = p.Y - p1.Y;

            float sp = x1 * y1 + x2 * y2;
            if (sp < 0 || sp > x1 * x1 + x2 * x2) return 1000F;

            return Math.Abs(x1 * y2 - x2 * y1) / (float)Math.Sqrt(x1 * x1 + x2 * x2);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            draw();
        }

        private void SymbolEditor_Load(object sender, EventArgs e)
        {

            PointList = new List<List<controlPointF>>();
            pixelOffset = new PointF(pictureBox1.Width / 2F, pictureBox1.Height / 2F);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            WriteToFile(textBox1.Text);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != "")
            {
                ReadFromFile(textBox1.Text);
                draw();
            }
            else
            {
                SymbolPicker.ShowPicker(PointToScreen(button2.Location));
                SymbolPicker.SymbolPicked += SymbolPicker_SymbolPicked;
            }
        }

        private void SymbolPicker_SymbolPicked(object sender, ObjectChosenEventArgs e)
        {
            textBox1.Text = (e.Object as MShapeSymbol).name;
            ReadFromFile(textBox1.Text);
            draw();
            SymbolPicker.HidePicker();
        }

        private void pictureBox1_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0) zoomIn();
            if (e.Delta < 0) zoomOut();
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (selected)
                {
                    PointList[selectedI][selectedJ].setPointF(getMousePos());
                    updateCornerAround(selectedI, selectedJ);
                    draw();
                }
                else if (scrolling)
                {
                    PointF m = pictureBox1.PointToClient(Cursor.Position);
                    pixelOffset.X = m.X - scrollMouseStart.X + scrollStart.X;
                    pixelOffset.Y = m.Y - scrollMouseStart.Y + scrollStart.Y;
                    draw();
                }
            }

        }

        private void deletePoint(int i, int j)
        {
            controlPointF p = PointList[i][j];
            if (p.type == controlPointF.controlType.assisting) return;

            if (p.type == controlPointF.controlType.corner)
            {
                PointList[i].RemoveRange(j - 3, 7);
            }

            if (p.type == controlPointF.controlType.normal)
                PointList[i].RemoveAt(j);

            updateAllCorners();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if(selected)
            {
                deletePoint(selectedI, selectedJ);
                selected = false;
                draw();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            List<controlPointF> l = new List<controlPointF>
            {
                new controlPointF(0F, -0.5F),
                new controlPointF(-0.3F, 0.3F),
                new controlPointF(0.3F, 0.3F)
            };
            PointList.Add(l);
            draw();
        }

        private void zoomIn()
        {
            zoom = zoom * 1.3F;
            draw();
        }

        private void zoomOut()
        {
            zoom = zoom / 1.3F;
            draw();
        }

        private void resetZoom()
        {
            zoom = 1F;
            draw();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            zoomOut();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            zoomIn();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (PointList[selectedI][selectedJ].type == controlPointF.controlType.normal)
                makeCorner(selectedI, selectedJ);
            else if (PointList[selectedI][selectedJ].type == controlPointF.controlType.corner)
                makeNormal(selectedI, selectedJ);
        }

        public void snapUp(int i, int j)
        {
            float nearest = -100000F;

            for (int k = 0; k < PointList.Count; k++)
                for (int l = 0; l < PointList[k].Count; l++)
                    if (k != i || l != j && PointList[k][l].type != controlPointF.controlType.assisting)
                        if (PointList[k][l].Y < PointList[i][j].Y && PointList[k][l].Y > nearest)
                            nearest = PointList[k][l].Y;

            if (nearest > -90000F)
            {
                PointList[i][j].Y = nearest;
                updateAllCorners();
                draw();
            }
        }

        public void snapDown(int i, int j)
        {
            float nearest = 100000F;

            for (int k = 0; k < PointList.Count; k++)
                for (int l = 0; l < PointList[k].Count; l++)
                    if (k != i || l != j && PointList[k][l].type != controlPointF.controlType.assisting)
                        if (PointList[k][l].Y > PointList[i][j].Y && PointList[k][l].Y < nearest)
                            nearest = PointList[k][l].Y;

            if (nearest < 90000F)
            {
                PointList[i][j].Y = nearest;
                updateAllCorners();
                draw();
            }
        }

        public void snapLeft(int i, int j)
        {
            float nearest = -100000F;

            for (int k = 0; k < PointList.Count; k++)
                for (int l = 0; l < PointList[k].Count; l++)
                    if (k != i || l != j && PointList[k][l].type != controlPointF.controlType.assisting)
                        if (PointList[k][l].X < PointList[i][j].X && PointList[k][l].X > nearest)
                            nearest = PointList[k][l].X;

            if (nearest > -90000F)
            {
                PointList[i][j].X = nearest;
                updateAllCorners();
                draw();
            }
        }

        public void snapRight(int i, int j)
        {
            float nearest = 100000F;

            for (int k = 0; k < PointList.Count; k++)
                for (int l = 0; l < PointList[k].Count; l++)
                    if (k != i || l != j && PointList[k][l].type != controlPointF.controlType.assisting)
                        if (PointList[k][l].X > PointList[i][j].X && PointList[k][l].X < nearest)
                            nearest = PointList[k][l].X;

            if (nearest < 90000F)
            {
                PointList[i][j].X = nearest;
                updateAllCorners();
                draw();
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            if (selected)
                snapDown(selectedI, selectedJ);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            //up
            if (selected)
                snapUp(selectedI, selectedJ);
        }

        private void button11_Click(object sender, EventArgs e)
        {
            if (selected)
                snapLeft(selectedI, selectedJ);
        }

        private void button10_Click(object sender, EventArgs e)
        {
            if (selected)
                snapRight(selectedI, selectedJ);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            resetZoom();
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            selected = false;
            scrolling = false;
            for (int i = 0; i < PointList.Count; i++)
            {
                for (int j = 0; j < PointList[i].Count; j++)
                    if (checkProximity(i, j, getMousePos(), 5F / MainForm.PPI / zoom))
                    {
                        selected = true;
                        selectedI = i; selectedJ = j;
                        return;
                    }
            }

            //Add new Points
            float smallestTan = 100F;
            int smallestI = 0, smallestJ = 0;
            for (int i = 0; i < PointList.Count; i++)
            {
                for (int j = 0; j < PointList[i].Count; j++)
                {
                    if (PointList[i][j].type != controlPointF.controlType.assisting)
                    {
                        float thisTan = getTangent(i, j, getMousePos());
                        if (thisTan < smallestTan)
                        {
                            smallestI = i; smallestJ = j;
                            smallestTan = thisTan;
                        }
                    }
                }
            }

            if(smallestTan < 10F / MainForm.PPI / zoom)
            {
                int newJ = smallestJ + 1;
                if (PointList[smallestI][smallestJ].type == controlPointF.controlType.corner)
                    newJ += 3;
                PointList[smallestI].Insert(newJ, controlPointF.fromPointF(getMousePos()));
                selected = true;
                selectedI = smallestI; selectedJ = newJ;
                draw();
                return;
            }

            scrolling = true;
            scrollStart = new PointF(pixelOffset.X, pixelOffset.Y);
            scrollMouseStart = pictureBox1.PointToClient(Cursor.Position);
        }
    }
}
