using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace TraceBackend
{
    public partial class MSymbol : MObject
    {
        static protected float SMALL_FACTOR;

        static MSymbol()
        {
            SMALL_FACTOR = Properties.Settings.Default.SmallFactor;
        }

        public virtual SizeF GetSize(float PPI, bool small = false)
        {
            throw new MissingMethodException();
        }

        internal virtual void Draw(Graphics g, PointF position, float PPI, MDrawSpecification? drawSpec, SyntaxType syntaxType, float xScale = 1.0f, float yScale = 1.0f, bool small = false)
        {
            if (small)
            {
                if (drawSpec.HasValue)
                {
                    SizeF size = GetSize(PPI,small);
                    g.FillRectangle(drawSpec.Value.backColor,
                        (position.X * PPI - xScale * size.Width * PPI / 2F),
                        (position.Y * PPI - yScale * size.Height * PPI / 2F),
                        xScale * size.Width * PPI, yScale * size.Height * PPI);
                    g.DrawRectangle(drawSpec.Value.borderColor,
                        (position.X * PPI - xScale * size.Width * PPI / 2F),
                        (position.Y * PPI - xScale * size.Height * PPI / 2F),
                        xScale * size.Width * PPI, yScale * size.Height * PPI);
                }
            }
            else
            {
                if (drawSpec.HasValue)
                {
                    SizeF size = GetSize(PPI,small);
                    g.FillRectangle(drawSpec.Value.backColor,
                        (position.X * PPI - xScale * size.Width * PPI / 2F),
                        (position.Y * PPI - yScale * size.Height * PPI / 2F),
                        xScale * size.Width * PPI, yScale * size.Height * PPI);
                    g.DrawRectangle(drawSpec.Value.borderColor,
                        (position.X * PPI - xScale * size.Width * PPI / 2F),
                        (position.Y * PPI - xScale * size.Height * PPI / 2F),
                        xScale * size.Width * PPI, yScale * size.Height * PPI);
                }
            }
        }

        internal static Brush GetBrush(MDrawSpecification? drawSpec, SyntaxType syntaxType)
        {
            if (!drawSpec.HasValue) return Brushes.Black;
            else
            {
                switch (syntaxType) {
                    case SyntaxType.DefaultType:
                        return drawSpec.Value.foreColor;
                    case SyntaxType.VarNoAxiom:
                        return drawSpec.Value.VarNoAx;
                    case SyntaxType.VarThisAxiom:
                        return drawSpec.Value.VarThisAx;
                    case SyntaxType.VarWithAxiom:
                        return drawSpec.Value.VarWithAx;
                    case SyntaxType.BoundVar:
                        return drawSpec.Value.BoundVar;
                }
            }
            return Brushes.Pink;
        }

        public override MObject Copy()
        {
            return this;
        }
    }
    
    public partial class MTextSymbol : MSymbol
    {
        string txt;
        public FontCategory font;
        static FontCategory defFont;
        public string Text
        {
            get { return txt; }
            set
            {
                txt = value;
                if (txt == "") txt = "_";
            }
        }

        static MTextSymbol()
        {
            defFont = FontCategory.FormulaCursive;
        }

        public MTextSymbol(string t, FontCategory f = FontCategory.FormulaCursive) : base()
        {
                font = f;

            Text = t;
        }

        public override SizeF GetSize(float PPI, bool small = false)
        {
            SizeF s;
            using (Graphics g = Graphics.FromImage(new Bitmap(5, 5)))
            {
                s = Helper.MeasureDisplayString(g, txt, font.GetFont(small));
            }
            return new SizeF(s.Width / PPI, s.Height / PPI);
        }

        internal override void Draw(Graphics g, PointF position, float PPI, MDrawSpecification? drawSpec, SyntaxType syntaxType, float xScale = 1.0f, float yScale = 1.0f, bool small = false)
        {
            base.Draw(g, position, PPI, drawSpec, syntaxType, xScale, yScale, small);
            Brush b = GetBrush(drawSpec, syntaxType);
            SizeF size = GetSize(PPI, small);
            g.DrawString(Text, font.GetFont(small), b, (position.X - size.Width / 2F) * PPI, (position.Y - size.Height / 2F) * PPI);
        }
    }

    public partial class MShapeSymbol : MSymbol
    {
        public static MShapeSymbol LeftBracket, RightBracket;

        static int tranforms;
        static long transtime;
        static float avg;

        internal SizeF size, smallsize;
        static string PATH_SYMBOLS = "symbols";
        public static MShapeSymbol[] SymbolList;

        PointF[][] curves;
        PointF[][] transformedCurves;
        float ppi;
        PointF pos;
        public string name;

        static MShapeSymbol()
        {
            LoadSymbols();
        }

        void InitTransformedCurves()
        {
            transformedCurves = new PointF[curves.Length][];
            for (int i = 0; i < curves.Length; i++)
            {
                transformedCurves[i] = new PointF[curves[i].Length];
                for(int j = 0; j < curves[i].Length; j++)
                {
                    transformedCurves[i][j] = new PointF();
                }
            }
        }

        static void LoadSymbols()
        {
            if (!Directory.Exists(PATH_SYMBOLS))
                Directory.CreateDirectory(PATH_SYMBOLS);
            string[] symbolPathList = Directory.GetFiles(PATH_SYMBOLS, "*.sym");

            SymbolList = new MShapeSymbol[symbolPathList.Length];
            for (int i = 0; i < SymbolList.Length; i++)
            {
                Stream s = File.OpenRead(symbolPathList[i]);
                SymbolList[i] = MShapeSymbol.FromFile(s);
                s.Close();
            }

            LeftBracket = FromName(Properties.Settings.Default.LeftBracketSymbol);
            RightBracket = FromName(Properties.Settings.Default.RightBracketSymbol);
        }

        public override SizeF GetSize(float PPI, bool small = false)
        {
            if (small)
                return smallsize;

            return size;
            //return new SizeF(size.Width / PPI, size.Height / PPI);
        }

        public static MShapeSymbol FromFile(Stream s)
        {
            BinaryReader r = new BinaryReader(s);

            byte version = r.ReadByte();
            string name = r.ReadString();
            float sizeW = r.ReadSingle();
            float sizeH = r.ReadSingle();
            byte shapeCount = r.ReadByte();

            MShapeSymbol sym = new MShapeSymbol(shapeCount)
            {
                name = name,
                size = new SizeF(sizeW, sizeH),
                smallsize = new SizeF(sizeW * SMALL_FACTOR, sizeH * SMALL_FACTOR)
            };
            for (int i = 0; i < shapeCount; i++)
            {
                ushort pointCount = r.ReadUInt16();

                sym.SetShape(i, pointCount);

                for (int j = 0; j < pointCount; j++)
                {
                    float x = r.ReadSingle();
                    float y = r.ReadSingle();
                    r.ReadByte();

                    sym.SetPoint(i, j, x, y);
                }
            }

            r.Close();
            sym.InitTransformedCurves();
            return sym;
        }

        public static MShapeSymbol FromName(string name)
        {
            return SymbolList.First(sym => sym.name == name);
        }
        
        void TransformCurves(PointF position, float PPI, float xScale = 1.0f, float yScale = 1.0f, bool small = false)
        {
            if (ppi == PPI && position.X == pos.X && position.Y == pos.Y) return;

            if (small)
            {
                for (int i = 0; i < curves.Length; i++)
                {
                    for (int j = 0; j < curves[i].Length; j++)
                    {
                        transformedCurves[i][j].X = (position.X + curves[i][j].X * xScale * SMALL_FACTOR) * PPI;
                        transformedCurves[i][j].Y = (position.Y + curves[i][j].Y * yScale * SMALL_FACTOR) * PPI;
                    }
                }
            }
            else
            {
                for (int i = 0; i < curves.Length; i++)
                {
                    for (int j = 0; j < curves[i].Length; j++)
                    {
                        transformedCurves[i][j].X = (position.X + curves[i][j].X * xScale) * PPI;
                        transformedCurves[i][j].Y = (position.Y + curves[i][j].Y * yScale) * PPI;
                    }
                }
            }

            pos = new PointF(position.X, position.Y);
            ppi = PPI;
        }

        internal override void Draw(Graphics g, PointF position, float PPI, MDrawSpecification? drawSpec, SyntaxType syntaxType, float xScale = 1.0f, float yScale = 1.0f, bool small = false)
        {
            base.Draw(g, position, PPI, drawSpec, syntaxType, xScale, yScale, small);
            /*tranforms++;
            DateTime before = DateTime.Now;*/
            TransformCurves(position, PPI, xScale, yScale, small);
            /*transtime += DateTime.Now.Ticks - before.Ticks;
            avg = transtime / (float)tranforms;
            if (tranforms > 5000) throw new Exception();*/

            Brush b = GetBrush(drawSpec, syntaxType);
            for (int i = 0; i < transformedCurves.Length; i++)
            {
                g.FillClosedCurve(b, transformedCurves[i], System.Drawing.Drawing2D.FillMode.Winding);
            }
        }

        MShapeSymbol(int shapeCount)
        {
            curves = new PointF[shapeCount][];
        }

        void SetShape(int shape, int pointCount)
        {
            curves[shape] = new PointF[pointCount];
        }

        void SetPoint(int shape, int point, float x, float y)
        {
            curves[shape][point] = new PointF(x, y);
        }

        public MVisualisation GetVisualisation()
        {
            MVisualisationScheme VS = new MVisualisationScheme(null);
            VS.AddSymbol(this);
            return new MVisualisation(this, VS);
        }
    }

    public enum FontCategory
    {
        TextFont = 0,
        TextCursive = 1,
        Headline = 2,
        Subheadline = 3,
        FormulaCursive = 4,
        FormulaUpright = 5,
        Calligraphic = 6
    }

    public static class FontExtensions
    {
        static Font FormulaCursiveSmall, FormulaUprightSmall, CaligraphicSmall;

        static Font MakeSmall(Font font)
        {
            return new Font(font.FontFamily, font.Size * Properties.Settings.Default.SmallFactor, font.Style);
        }

        static FontExtensions()
        {
            FormulaCursiveSmall = MakeSmall(Properties.Settings.Default.FormulaCursive);
            FormulaUprightSmall = MakeSmall(Properties.Settings.Default.FormulaUpright);
            CaligraphicSmall = MakeSmall(Properties.Settings.Default.Caligraphic);
        }

        public static Font GetFont(this FontCategory f, bool small = false)
        {
            if (!small)
            {
                switch (f)
                {
                    case FontCategory.TextFont:
                        return Properties.Settings.Default.TextFont;
                    case FontCategory.TextCursive:
                        return Properties.Settings.Default.TextCursive;
                    case FontCategory.Headline:
                        return Properties.Settings.Default.Headline;
                    case FontCategory.Subheadline:
                        return Properties.Settings.Default.Subheadline;
                    case FontCategory.FormulaCursive:
                        return Properties.Settings.Default.FormulaCursive;
                    case FontCategory.FormulaUpright:
                        return Properties.Settings.Default.FormulaUpright;
                    case FontCategory.Calligraphic:
                        return Properties.Settings.Default.Caligraphic;
                }
            }
            else
            {
                switch (f)
                {
                    case FontCategory.TextFont:
                        return Properties.Settings.Default.TextFont;
                    case FontCategory.TextCursive:
                        return Properties.Settings.Default.TextCursive;
                    case FontCategory.Headline:
                        return Properties.Settings.Default.Headline;
                    case FontCategory.Subheadline:
                        return Properties.Settings.Default.Subheadline;
                    case FontCategory.FormulaCursive:
                        return FormulaCursiveSmall;
                    case FontCategory.FormulaUpright:
                        return FormulaUprightSmall;
                    case FontCategory.Calligraphic:
                        return CaligraphicSmall;
                }
            }

            throw new Exception();
        }

        public static void ToStream(this FontCategory f, DocumentLoader DL)
        {
            DL.Write((byte)f);
        }
        public static string ToXML(this FontCategory f, XMLDocumentLoader DL)
        {
            return ((byte)f).ToString("X2");
        }

        public static FontCategory Fromstream(DocumentLoader DL)
        {
            return (FontCategory)DL.ReadByte();
        }
        public static FontCategory Fromstream(string str)
        {
            return (FontCategory)byte.Parse(str, System.Globalization.NumberStyles.HexNumber);
        }
    }
}
