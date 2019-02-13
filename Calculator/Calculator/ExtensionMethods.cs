using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Extensions;
using Xamarin.Forms;
using Crunch.GraphX;

namespace Calculator
{
    public static class ExtensionMethods
    {

    }
}

/*namespace System
{
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
        public static void Switch<T>(ref T a, ref T b)
        {
            var c = a;
            a = b;
            b = c;
        }

        public static T Last<T>(this IList<T> list) => list[list.Count - 1];

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

        public static int ToInt(this bool sender) => sender ? 1 : 0;
        public static bool ToBool(this int sender) => sender == 1 ? true : false;

        public static bool IsInt(this double value) => value == (int)value;

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
        public static void MoveTo(this View view, Point position) => MoveTo(view, position.X, position.Y);
        public static void MoveTo(this View view, double x, double y)
        {
            if (view.Parent is AbsoluteLayout)
            {
                AbsoluteLayout.SetLayoutBounds(view, new Rectangle(x, y, -1, -1));
            }
            else
            {
                throw new Exception("View is not a child of an absolute layout");
            }
        }

        public static bool IsEditable(this Layout<View> layout) => layout is Expression && (layout as Expression).Editable;

        public static Point PositionOn(this View child, View parent)
        {
            if (child == parent || child is null)
            {
                return Point.Zero;
            }

            return child.ParentView().PositionOn(parent).Add(new Point(child.X, child.Y + child.TranslationY));
        }

        public static View ParentView(this View view) => view.Parent as View;

        public static Point Add(this Point p1, Point p2) => new Point(p1.X + p2.X, p1.Y + p2.Y);
        public static Point Subtract(this Point p1, Point p2) => new Point(p1.X - p2.X, p1.Y - p2.Y);
        public static Point Multiply(this Point p1, double d) => new Point(p1.X * d, p1.Y * d);

        public static int Index(this View view) => (view.Parent as Layout<View>).Children.IndexOf(view);

        public static bool HasParent(this Element element) => element.Parent != null;

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
}*/
