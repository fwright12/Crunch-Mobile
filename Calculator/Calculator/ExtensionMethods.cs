using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Calculator;

namespace System
{
    public enum Direction { Backward = -1, Forward = 0 };

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

    public static class ExtensionMethods
    {
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

        public static bool IsNumber(this string str)
        {
            char chr = str[0];
            return (chr >= 48 && chr <= 57) || chr == 46;

            //return str.Length == 1 && str[0] >= 97 && str[0] <= 122;
        }

        public static bool IsOperand(this string str)
        {
            return str.Trim() == "+" || str.Trim() == "*" || str.Trim() == "-";
        }

        public static int Bound(this int value, int low, int high)
        {
            if (value < low)
                value = low;
            else if (value > high)
                value = high;

            return value;
        }

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
        public static Element ParentElement(this View view)
        {
            return view.Parent;
        }

        public static bool Selectable(this View view)
        {
            /*if (view is Text)
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
            }*/

            return true;
        }

        public static void SetSelectable(this View view, bool selectable)
        {
            /*if (view is Text)
            {
                (view as Text).Selectable = selectable;
            }
            else if (view is Expression)
            {
                (view as Expression).Selectable = selectable;
            }*/
        }

        public static Point Add(this Point p1, Point p2)
        {
            return new Point(p1.X + p2.X, p1.Y + p2.Y);
        }
    
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
