using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Calculator;
using Crunch.GraFX;

namespace System
{
    public static class StringClassification
    {
        public static bool IsOpening(this char c) => c == '(' || c == '{' || c == '[';
        public static bool IsClosing(this char c) => c == ')' || c == '}' || c == ']';
        public static bool IsOperand(this string s) => s.Length == 1 && (s == "/" || s == "×" || s == "+" || s == "*" || s == "-" || s == "^");
        public static bool IsNumber(this string s) => s.Length == 1 && ((s[0] >= 48 && s[0] <= 57) || s[0] == 46);
    }

    public class print
    {
        public static void log(params object[] p)
        {
            string s = p[0].ToString();
            for (int i = 1; i < p.Length; i++)
                s += ", " + p[i];
            Diagnostics.Debug.WriteLine(s);
        }
    }

    public enum Direction { Forward = 1, Backward = -1 };

    public static class ExtensionMethods
    {
        public static bool ToBool(this Crunch.TrieContains tc) => tc == Crunch.TrieContains.Full;

        public static T Last<T>(this List<T> list) => list[list.Count - 1];

        public static TValue TryGet<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key) where TValue : new()
        {
            if (!dict.ContainsKey(key))
            {
                dict.Add(key, new TValue());
            }
            return dict[key];
        }

        public static string Simple(this string str)
        {
            str = str.Trim();
            switch (str)
            {
                case "÷": return "/";
                case "×": return "*";
                default: return str;
            }
        }

        public static int ToInt(this bool sender)
        {
            if (sender)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        public static bool IsWhole(this double value) => value == (int)value;

        public static int Bound(this int value, int low, int high)
        {
            if (value < low)
                value = low;
            else if (value > high)
                value = high;

            return value;
        }

        /// <summary>
        /// Checks to see if value is between low and high (inclusive)
        /// </summary>
        /// <param name="value"></param>
        /// <param name="low"></param>
        /// <param name="high"></param>
        /// <returns></returns>
        public static bool IsBetween(this int value, int low, int high)
        {
            return value >= low && value <= high;
        }
    }
}

namespace Xamarin.Forms
{
    public static class ExtensionMethods
    {
        public static void Move(this View v, Action<View, Point> move, Rectangle bounds, Point increase)
        {
            print.log(bounds, v.X, v.Y);
            Point p = new Point(
                Math.Max(bounds.X, Math.Min(bounds.X + bounds.Width - v.Width, v.X + increase.X)),
                Math.Max(bounds.Y, Math.Min(bounds.Y + bounds.Height - v.Height, v.Y + increase.Y))
                );
            move(v, p);
        }

        public static Point PositionOn(this View child, View parent)
        {
            if (child == parent || child is null)
            {
                return Point.Zero;
            }

            return child.ParentView().PositionOn(parent).Add(new Point(child.X, child.Y + child.TranslationY));
        }

        public static View ParentView(this View view)
        {
            return view.Parent as View;
        }

        public static bool Selectable(this View view)
        {
            if (view is Text)
            {
                return (view as Text).Selectable;
            }
            else if (view is Expression)
            {
                return (view as Expression).Selectable;
            }
            else
            {
                return false;
            }
        }

        public static void SetSelectable(this View view, bool selectable)
        {
            if (view is Text)
            {
                (view as Text).Selectable = selectable;
            }
            else if (view is Expression)
            {
                (view as Expression).Selectable = selectable;
            }
        }

        public static Point Add(this Point p1, Point p2) => new Point(p1.X + p2.X, p1.Y + p2.Y);
        public static Point Subtract(this Point p1, Point p2) => new Point(p1.X - p2.X, p1.Y - p2.Y);

        public static int Index(this View view)
        {
            /*if (view.Parent is Expression)
            {
                return (view.Parent as Expression).IndexOf(view);
            }*/
            return (view.Parent as Layout<View>).Children.IndexOf(view);
        }

        public static bool HasParent(this Element element)
        {
            return element.Parent != null;
        }

        public static bool Remove(this View view)
        {
            try
            {
                (view.Parent as Layout<View>).Children.Remove(view);
                return true;
            }
            catch
            {
                print.log("View did not have a parent that could be cast to Layout<View>");
                return false;
            }
        }
    }
}
