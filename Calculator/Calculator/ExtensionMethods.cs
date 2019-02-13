using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculator
{
    public class print
    {
        public static void log(params object[] p)
        {
            string s = p[0].ToString();
            for (int i = 1; i < p.Length; i++)
                s += ", " + p[i];
            System.Diagnostics.Debug.WriteLine(s);
        }
    }

    public static class ExtensionMethods
    {
        public static void Remove(this Xamarin.Forms.View view)
        {
            try
            {
                (view.Parent as Xamarin.Forms.Layout<Xamarin.Forms.View>).Children.Remove(view);
            }
            catch
            {
                print.log("View did not have a parent that could be cast to Layout<View>");
            }
        }

        public static bool IsNumber(this string str)
        {
            char chr = str[0];
            return (chr >= 48 && chr <= 57) || chr == 46;

            //return str.Length == 1 && str[0] >= 97 && str[0] <= 122;
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

        public static LinkedListNode<Graphics.Symbol> NodeAt(this LinkedList<Graphics.Symbol> list, int index)
        {
            return list.Find(list.ElementAt(index));
        }

        public static List<Graphics.Symbol> GetBefore(this LinkedListNode<Graphics.Text> sender)
        {
            return new List<Graphics.Symbol>();
        }

        public static int ToInt(this bool sender)
        {
            if (sender)
                return 1;
            else
                return 0;
        }

        public static LinkedListNode<Graphics.Symbol> DetachNode(this LinkedListNode<Graphics.Symbol> sender)
        {
            if (sender.List != null)
            {
                sender.List.Remove(sender);
            }
            return sender;
        }

        public static List<Graphics.Symbol> Copy(this List<Graphics.Symbol> sender, params Graphics.Symbol[] add)
        {
            List<Graphics.Symbol> temp = new List<Graphics.Symbol>();
            foreach (Graphics.Symbol s in sender)
                temp.Add(s);
            foreach (Graphics.Symbol s in add)
                temp.Add(s);
            return temp;
        }

        public static LinkedListNode<Graphics.Symbol> Next<T>(this LinkedListNode<Graphics.Symbol> sender)
        {
            LinkedListNode<Graphics.Symbol> main = sender.Next;

            while (main.Value.GetType() != typeof(T))
            {
                main = main.List.First;
            }

            return main;
        }

        public static void FixDynamicLag(object o)
        {
            print.log(o as dynamic);
        }
    }
}
