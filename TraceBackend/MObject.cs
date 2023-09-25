using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;

namespace TraceBackend
{
    public partial class MObject
    {
        public static List<MObject> list;
        public static uint newID = 0;
        public uint ID;
        public string stringSymbol;

        static MObject()
        {
            list = new List<MObject>();
        }

        public MObject()
        {
            ID = newID; newID++;
            //list.Add(this); //if we do this, all objects have a reference and will always stay in memory.
        }

        public virtual bool Identical(MObject O, IdTable<MObject,MObject> idTable = null)
        {
            if (ID == O.ID) return true;
            else return false;
        }

        public virtual MObject Copy()
        {
            return new MObject();
        }

        public override string ToString()
        {
            if (stringSymbol == " ")
                return "object " + ID.ToString();
            return stringSymbol;
        }

    }

    public class IdTable<TLeft, TRight> 
        where TLeft : MObject 
        where TRight : MObject
    {
        public List<TLeft> Left;
        public List<TRight> Right;
        public IdRuleset ruleset;
        public int Count => Left.Count;

        public IdTable(IdRuleset r)
        {
            Left = new List<TLeft>();
            Right = new List<TRight>();
            ruleset = r;
        }

        public void addPair(TLeft L, TRight R)
        { Left.Add(L); Right.Add(R); }

        public void removePair(int index)
        { Left.RemoveAt(index); Right.RemoveAt(index); }

        ///<summary>
        ///Checks if neither A nor B are currently in the list and the correspondance is not yet listed. 
        ///If not, adds the pair to the list. 
        ///</summary>
        ///<param name="A"></param>
        ///<param name="B"></param>
        ///<returns>
        ///Returns false if there already is an incompatible Identification, true if not.
        ///</returns>
        ///

        public bool Identify(TLeft L, TRight R)
        {
            switch (ruleset) {
                case IdRuleset.NoRules:
                    return IdentifyNoRules(L, R);
                case IdRuleset.LeftUnique:
                    return IdentifyLeftUnique(L, R);
                case IdRuleset.RightUnique:
                    return IdentifyRightUnique(L, R);
                case IdRuleset.BothUnique:
                    return IdentifyBothUnique(L, R);
            }

            throw new ArgumentOutOfRangeException("Unknown Ruleset.");
        }

        private bool IdentifyNoRules(TLeft L, TRight R)
        {
            for (int i = 0; i < Left.Count; i++)
                if (Left[i].Identical(L) && Right[i].Identical(R))
                    return true;

            addPair(L, R);
            return true;
        }

        private bool IdentifyBothUnique(TLeft L, TRight R)
        {
            bool n = true;
            for (int i = 0; i < Left.Count; i++)
            {
                if (Left[i].Identical(L) && !Right[i].Identical(R))
                    return false;
                if (Left[i].Identical(L) && Right[i].Identical(R))
                    n = false;
            }
            for (int i = 0; i < Right.Count; i++)
            {
                if (Right[i].Identical(R) && !Left[i].Identical(L))
                    return false;
                if (Left[i].Identical(L) && Right[i].Identical(R))
                    n = false;
            }
            if (n) { Left.Add(L); Right.Add(R); }
            return true;
        }

        ///<summary>
        ///Checks if A is currently in the list and the correspondance is not yet listed. 
        ///If not, adds the pair to the list. 
        ///</summary>
        ///<returns>
        ///Returns false if A is already incompatibly identified, true if not.
        ///</returns>
        private bool IdentifyLeftUnique(TLeft L, TRight R)
        {
            bool n = true;
            for (int i = 0; i < Left.Count; i++)
            {
                if (Left[i] == L && !Right[i].Identical(R))
                    return false;
                if (Left[i] == L && Right[i].Identical(R))
                    n = false;
            }
            if (n) { Left.Add(L); Right.Add(R); }
            return true;
        }

        ///<summary>
        ///Checks if A is currently in the list and the correspondance is not yet listed. 
        ///If not, adds the pair to the list. 
        ///</summary>
        ///<returns>
        ///Returns false if A is already incompatibly identified, true if not.
        ///</returns>
        private bool IdentifyRightUnique(TLeft L, TRight R)
        {
            bool n = true;
            for (int i = 0; i < Right.Count; i++)
            {
                if (Right[i] == R && !Left[i].Identical(L))
                    return false;
                if (Right[i] == R && Left[i].Identical(L))
                    n = false;
            }
            if (n) { Left.Add(L); Right.Add(R); }
            return true;
        }

        public override string ToString()
        {
            string ret = "";
            for (int i = 0; i < Left.Count; i++)
            {
                ret = ret + Left[i].ToString() + " << " + Right[i].ToString();
                if (i < Left.Count - 1)
                    ret = ret + "\n";
            }
            return ret;
        }

        public string RightToString()
        {
            string ret = "";
            for (int i = 0; i < Left.Count; i++)
            {
                ret = ret + Left[i].ToString();
                if (i < Left.Count - 1)
                    ret = ret + "\n";
            }
            return ret;
        }
    }

    public enum IdRuleset
    {
        NoRules = 0,
        LeftUnique = 1,
        RightUnique = 2,
        BothUnique = 3
    }

    public class Helper
    {
        public static int Pow(int a, int b)
        {
            int ret = 1;
            while (b != 0)
            {
                ret = ret * a;
                b--;
            }
            return ret;
        }

        public static string Indent(string s, int increment = 1)
        {
            string indentString = "";
            for (int i = 0; i < increment; i++)
                indentString = indentString + " ";
            return indentString + s.Replace("\n", "\n" + indentString);
        }

        // Credit to http://www.codeproject.com/Articles/2118/Bypass-Graphics-MeasureString-limitations
        public static SizeF MeasureDisplayString(Graphics graphics, string text, Font font)
        {
            StringFormat format = new StringFormat();
            RectangleF rect = new RectangleF(0, 0, 1000, 1000);
            CharacterRange[] ranges = { new CharacterRange(0, text.Length) };
            Region[] regions = new Region[1];

            format.SetMeasurableCharacterRanges(ranges);

            regions = graphics.MeasureCharacterRanges(text, font, rect, format);
            rect = regions[0].GetBounds(graphics);

            return new SizeF(rect.Right, rect.Bottom);
        }

        public static string GetRelativePath(string from, string to)
        {
            Uri p1 = new Uri(from + "\\");
            Uri p2 = new Uri(to);
            Uri rel = p1.MakeRelativeUri(p2);
            return rel.OriginalString.Replace("/", "\\"); 
        }
    }

    public static class PathExtensions
    {
        public static byte[] Add(this byte[] path, byte addition)
        {
            byte[] ret = new byte[path.Length + 1];
            for (int i = 0; i < path.Length; i++)
                ret[i] = path[i];
            ret[path.Length] = addition;

            return ret;
        }

        public static ushort[] Add(this ushort[] path, ushort addition)
        {
            ushort[] ret = new ushort[path.Length + 1];
            for (int i = 0; i < path.Length; i++)
                ret[i] = path[i];
            ret[path.Length] = addition;

            return ret;
        }
    }
}
