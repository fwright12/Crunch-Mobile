using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculator
{
    public class print
    {
        public static void log(object p)
        {
            System.Diagnostics.Debug.WriteLine(p);
        }
    }

    public static class ExtensionMethods
    {
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
