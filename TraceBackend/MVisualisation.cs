using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraceBackend
{
    public partial class MVisualisationScheme : IDocumentElement
    {
        /*
         INDICES: CHILDREN FIRST, THEN SYMBOLS !
             */
        public MDefinition Definition;
        private int overrideChildren = 0;
        public int Children => Definition == null ? overrideChildren : Definition.CountSubVisualisations();  // how many subVisualisations (from definition)
        public BracketSetting myBracket;
        public BracketSetting[] Brackets;

        public List<MSymbol> Symbols;
        public MArrangementTree arrangement;

        public string LaTeXStart => @"";
        public string LaTeXEnd => @"";

        public string Latex = "";

        internal MVisualisationScheme(MDefinition D, int children = 0)
        {
            Definition = D;
            overrideChildren = children;
            Symbols = new List<MSymbol>();
            arrangement = new MArrangementTree(-1);
            Brackets = new BracketSetting[Children];
        }

        bool Arrange(int index, MArrangementTree relativeTo, MAnchor Anchor = MAnchor.right, float x = 0F, float y = 0F)
        {
            MArrangementTree tree = new MArrangementTree(index, relativeTo, Anchor, x, y);
            return tree.FitIn(arrangement);
        }

        public bool Arrange(int index, int relativeTo = -1, MAnchor Anchor = MAnchor.right, float x = 0F, float y = 0F)
        {
            return Arrange(index, arrangement.FindByIndex(relativeTo), Anchor, x, y);
        }

        public void SetParent(int index, int relativeTo = -1)
        {
            MArrangementTree self = arrangement.FindByIndex(index);
            self.CutOff();
            arrangement.FindByIndex(relativeTo).AddBranch(self);
        }

        public void SetAnchor(int index, MAnchor Anchor)
        {
            arrangement.FindByIndex(index).anchor = Anchor;
        }

        public void SetXOffset(int index, float x)
        {
            arrangement.FindByIndex(index).xOff = x;
        }

        public void SetYOffset(int index, float y)
        {
            arrangement.FindByIndex(index).yOff = y;
        }

        public void SetText(int index, string text)
        {
            index -= Children;
            if (0 <= index && index < Symbols.Count && Symbols[index] is MTextSymbol TS) TS.Text = text;
        }

        public void SetSmall(int index, bool small)
        {
            arrangement.FindByIndex(index).MakeSmall = small;
        }
        public void SetGrowX(int index, bool grow)
        {
            arrangement.FindByIndex(index).GrowAlongChildren.x = grow;
        }
        public void SetGrowY(int index, bool grow)
        {
            arrangement.FindByIndex(index).GrowAlongChildren.y = grow;
        }

        public string GetText(int index)
        {
            index -= Children;
            if (0 <= index && index < Symbols.Count && Symbols[index] is MTextSymbol TS) return TS.Text;
            return "";
        }

        public MSymbol GetSymbol(int index)
        {
            index -= Children;
            if (0 <= index && index < Symbols.Count) return Symbols[index];
            return null;
        }

        public int AddSymbol(MSymbol symbol, int relativeTo = -1, MAnchor Anchor = MAnchor.right, float x = 0F, float y = 0F)
        {
            Symbols.Add(symbol);
            int idx = Children + Symbols.Count - 1;
            Arrange(idx, relativeTo, Anchor, x, y);
            return idx;
        }

        public void RemoveSymbol(int index)
        {
            if (index < Children) return;
            MArrangementTree branch = arrangement.FindByIndex(index);
            while(branch.Branches.Count > 0)
            {
                MArrangementTree bb = branch.Branches[0];
                bb.CutOff();
                branch.Trunk.AddBranch(bb);
            }
            branch.CutOff();
            arrangement.ShiftIndizes(index);
            Symbols.RemoveAt(index-Children);
        }

        public void HideChild(int index)
        {
            if (index >= Children) return;
            MArrangementTree branch = arrangement.FindByIndex(index);
            while (branch.Branches.Count > 0)
            {
                MArrangementTree bb = branch.Branches[0];
                bb.CutOff();
                branch.Trunk.AddBranch(bb);
            }
            branch.CutOff();
        }
    }

    public partial class MArrangementTree
    {
        public float xOff, yOff;
        public MAnchor anchor;
        
        public (bool x, bool y) GrowAlongChildren;
        public bool MakeSmall;
        
        internal MArrangementTree Trunk;
        public int myIndex;
        public bool isRoot => myIndex == -1;
        public List<MArrangementTree> Branches;

        public MArrangementTree(int index, MArrangementTree Reference = null, MAnchor Anchor = MAnchor.right, float x = 0F, float y = 0F)
        {
            Trunk = Reference;
            myIndex = index;
            Branches = new List<MArrangementTree>();

            anchor = Anchor;
            xOff = x; yOff = y;
        }

        internal void ShiftIndizes(int index)
        {
            if (myIndex > index) myIndex--;
            foreach (MArrangementTree b in Branches)
                b.ShiftIndizes(index);
        }

        public void AddBranch(MArrangementTree b)
        {
            b.Trunk = this; //TODO: Loop prevention
            Branches.Add(b);
        }

        public void RemoveBranch(MArrangementTree b)
        {
            b.Trunk = null;
            Branches.Remove(b);
        }

        public void CutOff()
        {
            Trunk.RemoveBranch(this);
        }

        public MArrangementTree CreateBranch(int index, MAnchor Anchor = MAnchor.right, float x = 0F, float y = 0F)
        {
            MArrangementTree b;
            b = new MArrangementTree(index, this, Anchor, x, y);
            AddBranch(b);
            return b;
        }

        public bool FitIn(MArrangementTree tree)
        {
            if (Trunk == tree)
            {
                tree.AddBranch(this);
                return true;
            }
            foreach (MArrangementTree branch in tree.Branches)
                if (FitIn(branch)) return true;
            return false;
        }

        public MArrangementTree FindByIndex(int index)
        {
            if (index == myIndex)
                return this;

            foreach (MArrangementTree branch in Branches)
            {
                MArrangementTree found = branch.FindByIndex(index);
                if (found != null) return found;
            }
            return null;
        }
        
        public override string ToString()
        {
            return "Arrangement Tree (index: " + myIndex.ToString() + ")";
        }
    }

    public class MArrangementTreeInstance
    {
        internal MArrangementTree _;

        internal MArrangementTreeInstance Trunk;
        internal List<MArrangementTreeInstance> Branches;

        internal PointF Offset; // offset of the center of the symbol/subVis w.r.t. the center of the branch.
        internal SizeF Position; // Position of the center of the branch w.r.t to the top-left corner of the top-level visualization
        internal SizeF Size;
        internal float StretchX = 1.0f; //stretch factors for symbols (brackets, fraction lines, ...)
        internal float StretchY = 1.0f;
        bool isSmall;
        internal MVisualisation visualization;

        MArrangementTreeInstance(MArrangementTree original, MVisualisation v)
        {
            _ = original;
            visualization = v;
            Branches = new List<MArrangementTreeInstance>();
        }

        internal static MArrangementTreeInstance FromVisualization(MVisualisation v, MArrangementTree tree = null)
        {
            if (tree == null) tree = v.Scheme.arrangement;
            MArrangementTreeInstance instance = new MArrangementTreeInstance(tree, v);
            instance.isSmall = tree.MakeSmall;

            foreach (MArrangementTree branch in tree.Branches)
            {
                MArrangementTreeInstance branchInstance = FromVisualization(v, branch);
                branchInstance.Trunk = instance;
                instance.Branches.Add(branchInstance);
                if (instance.isSmall) branchInstance.isSmall = true;
            }

            return instance;
        }

        public MArrangementTreeInstance FindByIndex(int index)
        {
            if (_.myIndex == index) return this;
            foreach(MArrangementTreeInstance branch in Branches)
            {
                MArrangementTreeInstance ret = branch.FindByIndex(index);
                if (ret != null) return ret;
            }
            return null;
        }

        public SizeF GetSize()
        {
            SizeF s = visualization.GetSubSize(_.myIndex, isSmall);  // size of this symbol or subvisualisation

            // Root of coordinate system lies in the center of this Symbol or SubVis.
            PointF TopLeftMost = new PointF(-s.Width / 2F, -s.Height / 2F);
            PointF BotRightMost = new PointF(s.Width / 2F, s.Height / 2F);

            foreach (MArrangementTreeInstance branch in Branches)
            {
                SizeF bSize = branch.GetSize();
                switch (branch._.anchor)
                {
                    case MAnchor.right:
                        // how far up does the branch go?
                        if (-bSize.Height / 2F + branch._.yOff < TopLeftMost.Y)
                            TopLeftMost.Y = -bSize.Height / 2F + branch._.yOff;

                        // how far right does the branch go?
                        if (s.Width / 2F + bSize.Width + branch._.xOff > BotRightMost.X)
                            BotRightMost.X = s.Width / 2F + bSize.Width + branch._.xOff;
                        // how far down does the branch go?
                        if (bSize.Height / 2F + branch._.yOff > BotRightMost.Y)
                            BotRightMost.Y = bSize.Height / 2F + branch._.yOff;
                        break;
                    case MAnchor.bottom:
                        // how far left does the branch go?
                        if (-bSize.Width / 2F + branch._.xOff < TopLeftMost.X)
                            TopLeftMost.X = -bSize.Width / 2F + branch._.xOff;

                        // how far down does the branch go?
                        if (s.Height / 2F + bSize.Height + branch._.yOff > BotRightMost.Y)
                            BotRightMost.Y = s.Height / 2F + bSize.Height + branch._.yOff;
                        // how far right does the branch go?
                        if (bSize.Width / 2F + branch._.xOff > BotRightMost.X)
                            BotRightMost.X = bSize.Width / 2F + branch._.xOff;
                        break;
                    case MAnchor.left:
                        // how far up does the branch go?
                        if (-bSize.Height / 2F + branch._.yOff < TopLeftMost.Y)
                            TopLeftMost.Y = -bSize.Height / 2F + branch._.yOff;

                        // how far left does the branch go?
                        if (-s.Width / 2F - bSize.Width + branch._.xOff < TopLeftMost.X)
                            TopLeftMost.X = -s.Width / 2F - bSize.Width + branch._.xOff;
                        // how far down does the branch go?
                        if (bSize.Height / 2F + branch._.yOff > BotRightMost.Y)
                            BotRightMost.Y = bSize.Height / 2F + branch._.yOff;
                        break;
                    case MAnchor.top:
                        // how far left does the branch go?
                        if (-bSize.Width / 2F + branch._.xOff < TopLeftMost.X)
                            TopLeftMost.X = -bSize.Width / 2F + branch._.xOff;

                        // how far up does the branch go?
                        if (-s.Height / 2F - bSize.Height + branch._.yOff < TopLeftMost.Y)
                            TopLeftMost.Y = -s.Height / 2F - bSize.Height + branch._.yOff;
                        // how far right does the branch go?
                        if (bSize.Width / 2F + branch._.xOff > BotRightMost.X)
                            BotRightMost.X = bSize.Width / 2F + branch._.xOff;
                        break;
                }
            }

            if (_.isRoot && visualization.bracket)
            {
                Size = new SizeF(BotRightMost.X - TopLeftMost.X + MShapeSymbol.LeftBracket.GetSize(0, isSmall).Width + MShapeSymbol.RightBracket.GetSize(0, isSmall).Width, BotRightMost.Y - TopLeftMost.Y);
                Offset = new PointF(-Size.Width / 2F - TopLeftMost.X + MShapeSymbol.LeftBracket.GetSize(0, isSmall).Width, -Size.Height / 2F - TopLeftMost.Y);
            }
            else
            {
                Size = new SizeF(BotRightMost.X - TopLeftMost.X, BotRightMost.Y - TopLeftMost.Y);
                Offset = new PointF(-Size.Width / 2F - TopLeftMost.X, -Size.Height / 2F - TopLeftMost.Y);
            }

            if (_.GrowAlongChildren.x) StretchX = Size.Width / visualization.Scheme.Symbols[_.myIndex-visualization.Scheme.Children].GetSize(visualization.ppi,isSmall).Width;
            else StretchX = 1.0f;
            if (_.GrowAlongChildren.y) StretchY = Size.Height / visualization.Scheme.Symbols[_.myIndex - visualization.Scheme.Children].GetSize(visualization.ppi,isSmall).Height;
            else StretchY = 1.0f;

            return Size;
        }

        public void CalculateOffset(SizeF Off, bool startVis)
        {
            if (_.isRoot)
            {
                if (startVis)
                    Off = new SizeF(Size.Width / 2F, Size.Height / 2F);

                //if (visualization.bracket)
                    //Off.Width += MShapeSymbol.LeftBracket.size.Width;
                visualization.position = Off.ToPointF();
            }

            visualization.CalculateMemberOffset(_.myIndex, PointF.Add(Offset, Off));

            SizeF mySubSize = visualization.PrecalculatedSubSize(_.myIndex, isSmall);

            foreach (MArrangementTreeInstance branch in Branches)
            {
                switch (branch._.anchor)
                {
                    case MAnchor.right:
                        branch.CalculateOffset(new SizeF(
                            Off.Width + Offset.X + (mySubSize.Width + branch.Size.Width) / 2F + branch._.xOff,
                            Off.Height + Offset.Y + branch._.yOff), startVis);
                        break;
                    case MAnchor.bottom:
                        branch.CalculateOffset(new SizeF(
                            Off.Width + Offset.X + branch._.xOff,
                            Off.Height + Offset.Y + (mySubSize.Height + branch.Size.Height) / 2F + branch._.yOff),startVis);
                        break;
                    case MAnchor.left:
                        branch.CalculateOffset(new SizeF(
                            Off.Width + Offset.X - (mySubSize.Width + branch.Size.Width) / 2F + branch._.xOff,
                            Off.Height + Offset.Y + branch._.yOff), startVis);
                        break;
                    case MAnchor.top:
                        branch.CalculateOffset(new SizeF(
                            Off.Width + Offset.X + branch._.xOff,
                            Off.Height + Offset.Y - (mySubSize.Height + branch.Size.Height) / 2F + branch._.yOff),  startVis);
                        break;
                }
            }

            Position = Off;
        }

        public void Draw(Graphics g, float PPI, bool startVis, MDrawOptions DrawOptions)
        {
            visualization.DrawMember(_.myIndex, g, PointF.Add(Offset, Position), PPI, DrawOptions, isSmall);

            if (_.isRoot && visualization.bracket)
            {
                MShapeSymbol.LeftBracket.Draw(g, new PointF(Position.Width - Size.Width /2 + MShapeSymbol.LeftBracket.GetSize(0, isSmall).Width / 2, Position.Height + Offset.Y), 
                    PPI, null, SyntaxType.DefaultType, yScale: Size.Height/ MShapeSymbol.LeftBracket.GetSize(0, isSmall).Height, small:isSmall);
                MShapeSymbol.RightBracket.Draw(g, new PointF(Position.Width + Size.Width / 2 - MShapeSymbol.RightBracket.GetSize(0, isSmall).Width / 2, Position.Height + Offset.Y),
                    PPI, null, SyntaxType.DefaultType, yScale: Size.Height / MShapeSymbol.RightBracket.GetSize(0, isSmall).Height, small:isSmall);
            }

            foreach (MArrangementTreeInstance branch in Branches)
            {
                branch.Draw(g, PPI, startVis, DrawOptions);
            }
        }
    }

    public enum BracketMode //To be in line with CheckState enum.
    {
        Auto = 2,
        ManualYes = 1,
        ManualNo = 0
    }

    public enum MAnchor
    {
        right = 0, 
        bottom = 1,
        left = 2,
        top = 3
    }

    public enum SyntaxType
    {
        DefaultType = 0,
        VarNoAxiom = 1,
        VarWithAxiom = 2,
        VarThisAxiom = 3,
        BoundVar = 4
    }

    public class MVisualisation
    {
        public MObject VisualisedObject;
        public MVisualisationScheme Scheme;
        internal MArrangementTreeInstance Arrangement;
        public MVisualisation[] SubVisualisations;
        public Image image;
        public SizeF size;
        public float ppi;
        public PointF position;
        public PointF[] symbolPosition;
        public byte[] path;
        BracketMode _bmode = BracketMode.Auto;
        public BracketMode BracketMode
        {
            get { return _bmode; }
            set
            {
                _bmode = value;
                if (Arrangement.Trunk != null && 0 <= Arrangement.Trunk._.myIndex && Arrangement.Trunk._.myIndex < Scheme.Brackets.Length)
                    CalculateBrackets(true, Scheme.Brackets[Arrangement.Trunk._.myIndex]);
                else
                    CalculateBrackets(true);
            }
        }
        internal bool bracket = false;
        Bitmap BMP;
        SyntaxType syntaxType;

        

        public MVisualisation(MObject visualizedObject, MVisualisationScheme scheme)
        {
            VisualisedObject = visualizedObject;
            Scheme = scheme;
            SubVisualisations = new MVisualisation[scheme.Children];
            symbolPosition = new PointF[scheme.Symbols.Count];
            path = new byte[] { 0 };
            Arrangement = MArrangementTreeInstance.FromVisualization(this);
        }

        public void SetSub (MVisualisation vis, int index = 0)
        {
            SubVisualisations[index] = vis;
            UpdateSubPaths();
        }

        internal void CalculateBrackets(bool AlsoOnSubvis, BracketSetting parentSetting = BracketSetting.No)
        {
            if (AlsoOnSubvis)
            {
                for (int i = 0; i < SubVisualisations.Count(); i++)
                    SubVisualisations[i].CalculateBrackets(true, Scheme.Brackets[i]);
            }

            if(BracketMode == BracketMode.ManualNo)
            {
                bracket = false; return;
            }
            if(BracketMode == BracketMode.ManualYes)
            {
                bracket = true; return;
            }

            //Force dominates
            if (parentSetting == BracketSetting.Force || Scheme.myBracket == BracketSetting.Force)
            {
                bracket = true; return;
            }
            //No Conflict
            if ((parentSetting == BracketSetting.No || parentSetting == BracketSetting.Suppress) &&
                (Scheme.myBracket == BracketSetting.No || Scheme.myBracket == BracketSetting.Suppress))
            {
                bracket = false; return;
            }
            //No Conflict
            if (parentSetting == BracketSetting.Yes && Scheme.myBracket == BracketSetting.Yes)
            {
                bracket = true; return;
            }
            //Suppress Suppresses
            if ((parentSetting == BracketSetting.Yes && Scheme.myBracket == BracketSetting.Suppress) ||
                (parentSetting == BracketSetting.Suppress && Scheme.myBracket == BracketSetting.Yes))
            {
                bracket = false; return;
            }
            //Yes wins over no
            if ((parentSetting == BracketSetting.Yes && Scheme.myBracket == BracketSetting.No) ||
                (parentSetting == BracketSetting.Yes && Scheme.myBracket == BracketSetting.Yes))
            {
                bracket = true; return;
            }
            throw new System.Exception();
        }

        private void UpdateSubPaths()
        {
            for(int i = 0; i< SubVisualisations.Length; i++)
            {
                if (SubVisualisations[i] != null)
                {
                    SubVisualisations[i].path = path.Add((byte)i);
                    SubVisualisations[i].UpdateSubPaths();
                }
            }
        }

        public bool Hover(PointF pos)
        {
            pos = new PointF(pos.X / ppi, pos.Y / ppi);
            return pos.X >= position.X - size.Width * .5F && pos.Y >= position.Y - size.Height * .5F
                && pos.X <= position.X + size.Width * .5F && pos.Y <= position.Y + size.Height * .5F;
        }

        public bool HoverSymbol(PointF pos, int symbolIndex)
        {
            pos = new PointF(pos.X / ppi, pos.Y / ppi);
            SizeF size = Scheme.Symbols[symbolIndex].GetSize(ppi); //TODO: small
            return pos.X >= symbolPosition[symbolIndex].X - size.Width * .5F 
                && pos.Y >= symbolPosition[symbolIndex].Y - size.Height * .5F
                && pos.X <= symbolPosition[symbolIndex].X + size.Width * .5F 
                && pos.Y <= symbolPosition[symbolIndex].Y + size.Height * .5F;
        }

        public MVisualisation GetTopHovered(PointF pos)
        {
            if (!Hover(pos)) return null;

            foreach (MVisualisation v in SubVisualisations)
            {
                MVisualisation hov = v.GetTopHovered(pos);
                if (hov != null) return hov;
            }

            return this;
        }

        public int GetTopHoveredSymbolIndex(PointF pos)
        {
            for(int i = 0; i < symbolPosition.Length; i++)
            {
                if (HoverSymbol(pos, i)) return i;
            }
            return -1;
        }

        public SizeF PrecalculatedSubSize(int index, bool small)
        {
            if (index == -1) return new SizeF();
            if (index < Scheme.Children)
            {
                return SubVisualisations[index].size;
            }
            else
            {
                return Scheme.Symbols[index - Scheme.Children].GetSize(ppi, small);
            }
        }

        public SizeF GetSubSize(int index, bool small)
        {
            if (index == -1) return new SizeF();
            if(index < Scheme.Children)
            {
                return SubVisualisations[index].GetSize(ppi);
            }
            else
            {
                return Scheme.Symbols[index - Scheme.Children].GetSize(ppi, small);
            }
        }

        internal void DrawMember(int index, Graphics g, PointF pos, float PPI, MDrawOptions DrawOptions, bool small)
        {
            if (index == -1) return;
            if (index < Scheme.Children)
            {
                SubVisualisations[index].IterDraw(g, pos, PPI, false, DrawOptions);
            }
            else
            {
                MDrawSpecification? drawSpec = DrawOptions.ContainsSym(this, index - Scheme.Children) ?? DrawOptions.globalSpec;
                MArrangementTreeInstance SymbolArrangement = Arrangement.FindByIndex(index);
                Scheme.Symbols[index - Scheme.Children].Draw(g, pos, PPI, drawSpec, syntaxType, SymbolArrangement.StretchX, SymbolArrangement.StretchY, small:small);
                symbolPosition[index - Scheme.Children] = pos;
            }
        }

        internal void CalculateMemberOffset(int index, PointF pos)
        {
            if (index == -1) return;
            if (index < Scheme.Children)
            {
                SubVisualisations[index].Arrangement.CalculateOffset(new SizeF(pos), false);
            }
        }

        internal void IterDraw(Graphics g, PointF pos, float PPI, bool startVis, MDrawOptions DrawOptions)
        {
            if (VisualisedObject is MVariable var)
            {
                if (var is MBoundVariable) syntaxType = SyntaxType.BoundVar;
                else if (var.HasAxioms) syntaxType = SyntaxType.VarWithAxiom;
                else syntaxType = SyntaxType.VarNoAxiom;
            }
            else
                syntaxType = SyntaxType.DefaultType;

            //TODO: Make BackColor a Property of MVisualization
            if(VisualisedObject is MPlaceholderFormula)
            {
                g.FillRectangle(Brushes.LightBlue, (pos.X - size.Width / 2F) * PPI, (pos.Y - size.Height / 2F) * PPI, size.Width * PPI, size.Height * PPI);
            }
            if (VisualisedObject is MPlaceholderTerm)
            {
                g.FillRectangle(Brushes.LightGreen, (pos.X - size.Width / 2F) * PPI, (pos.Y - size.Height / 2F) * PPI, size.Width * PPI, size.Height * PPI);
            }
            if (VisualisedObject is MBoundVariable V && V._FreeInstance == MVariable.PlaceholderVariable)
            {
                g.FillRectangle(Brushes.LightSeaGreen, (pos.X - size.Width / 2F) * PPI, (pos.Y - size.Height / 2F) * PPI, size.Width * PPI, size.Height * PPI);
            }

            MDrawSpecification? specification = DrawOptions.ContainsVis(this);
            if (specification.HasValue)
            {
                g.FillRectangle(specification.Value.backColor, (pos.X - size.Width / 2F) * PPI, (pos.Y - size.Height / 2F) * PPI, size.Width * PPI, size.Height * PPI);
            }

            Arrangement.Draw(g, PPI, startVis, DrawOptions);
        }

        public Bitmap Draw(float PPI, MDrawOptions DrawOptions, bool recalculateSizes)
        {
            if (recalculateSizes)
            {
                GetSize(PPI);
                Arrangement.CalculateOffset(new SizeF(0, 0), true);
            }

            if(BMP == null || (BMP.Width != (int)(size.Width * PPI + 0.5) || BMP.Height != (int)(size.Height * PPI + 0.5)))
                BMP = new Bitmap((int)(size.Width * PPI + 0.5), (int)(size.Height * PPI + 0.5));
            
            Graphics g = Graphics.FromImage(BMP);
            g.Clear(Color.White);

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

            
            IterDraw(g, new PointF(size.Width / 2F, size.Height / 2F), PPI, true, DrawOptions);

            return BMP;
        }

        public SizeF GetSize(float PPI)
        {
            ppi = PPI;
            size = Arrangement.GetSize();
            return size;
        }

        public string GetLaTeX()
        {
            string Latex = Scheme.Latex;
            if (Latex == "") Latex = VisualisedObject.stringSymbol;
            else
            {
                string[] components = Latex.Split('%');

                for (int i = 1; i < components.Length; i += 2)
                {
                    components[i] = "{" + SubVisualisations[Convert.ToInt32(components[i])].GetLaTeX() + "}";
                }

                Latex = String.Concat(components);
            }

            if (bracket)
                Latex = @"\left( " + Latex + @"\right)";

            return Latex;
        }
    }

    public enum BracketSetting
    {
        No = 0,
        Yes = 1,
        Suppress = 2,
        Force = 3
    }

    public class MDrawOptions
    {
        internal List<MDrawSpecification> vSpecifications, sSpecifications;
        internal MDrawSpecification globalSpec;
        internal static MDrawSpecification defaultGlobals;

        static MDrawOptions()
        {
            defaultGlobals = new MDrawSpecification(null, -1, Brushes.Transparent, Brushes.Black, Pens.Transparent, Tag.global); //TODO: needs to be reworked
        }

        public MDrawOptions()
        {
            vSpecifications = new List<MDrawSpecification>();
            sSpecifications = new List<MDrawSpecification>();
            globalSpec = defaultGlobals;
        }

        public void AddVSpecification(MVisualisation v, Brush back, Brush fore, Pen border, Tag tag = Tag.unspecified)
        {
            vSpecifications.Add(new MDrawSpecification(v, -1, back, fore, border, tag));
        }

        public void SetGlobalOptions(Brush back, Brush fore)
        {
            globalSpec = new MDrawSpecification(null, -1, back, fore, new Pen(back), Tag.global);
        }
        public void SetGlobalOptions(Brush back, Brush fore, Brush varnoax, Brush varwithax, Brush varthisax, Brush boundvar)
        {
            globalSpec = new MDrawSpecification(null, -1, back, fore, new Pen(back), varwithax, varnoax, varthisax, boundvar, Tag.global);
        }

        internal MDrawSpecification? ContainsVis(MVisualisation v)
        {
            foreach (MDrawSpecification vSpec in vSpecifications)
                if (vSpec.visualisation == v) return vSpec;
            return null;
        }

        public void AddSSpecification(MVisualisation v, int s, Brush back, Brush fore, Pen border, Tag tag = Tag.unspecified)
        {
            sSpecifications.Add(new MDrawSpecification(v, s, back, fore, border, tag));
        }

        internal MDrawSpecification? ContainsSym(MVisualisation v, int s)
        {
            foreach (MDrawSpecification sSpec in sSpecifications)
                if (sSpec.visualisation == v && sSpec.symbolIndex == s) return sSpec;
            return null;
        }

        public void Clear(Tag T = Tag.all)
        {
            if(T== Tag.all)
            {
                vSpecifications.Clear();
                sSpecifications.Clear();
                return;
            }

            vSpecifications.RemoveAll(vS => vS.Tag == T);
            sSpecifications.RemoveAll(sS => sS.Tag == T);
        }

        public enum Tag
        {
            all = 0,
            unspecified = 1,
            hoverhighlight = 2,
            editinghighlight = 3,
            global = 4,
            syntax = 5
        }
          
    }

    internal struct MDrawSpecification
    {
        internal MVisualisation visualisation;
        internal int symbolIndex;
        internal Brush backColor;
        internal Brush foreColor;
        internal Pen borderColor;
        internal MDrawOptions.Tag Tag;

        //type-specifics
        internal Brush VarNoAx;
        internal Brush VarThisAx;
        internal Brush VarWithAx;
        internal Brush BoundVar;

        internal MDrawSpecification(MVisualisation v, int s, Brush back, Brush fore, Pen border, MDrawOptions.Tag tag = MDrawOptions.Tag.unspecified)
        {
            visualisation = v;
            symbolIndex = s;
            backColor = back;
            foreColor = fore;
            borderColor = border;
            Tag = tag;

            VarNoAx = fore;
            VarWithAx = fore;
            VarThisAx = fore;
            BoundVar = fore;
        }

        internal MDrawSpecification(MVisualisation v, int s, Brush back, Brush fore, Pen border, Brush varwith, Brush varno, Brush varthis, Brush boundvar, MDrawOptions.Tag tag = MDrawOptions.Tag.unspecified)
        {
            visualisation = v;
            symbolIndex = s;
            backColor = back;
            foreColor = fore;
            borderColor = border;
            Tag = tag;

            VarNoAx = varno;
            VarWithAx = varwith;
            VarThisAx = varthis;
            BoundVar = boundvar;
        }
    }
}
