using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Graphics;

namespace Calculator
{
    public delegate void CursorListener(object sender);

    public static partial class Input
    {

    }

    public static partial class Input
    {
        public static Equation selected;
        public static GraphicsEngine select;

        //public static IGraphicsHandler graphicsHandler;

        public static object cursor;
        public static object phantomCursor;
        public static List<Symbol> adding = new List<Symbol>();
        public static int minHeight;

        public static List<Graphics.Symbol> GetBefore(this LinkedListNode<Graphics.Text> sender)
        {
            return new List<Graphics.Symbol>();
        }

        public static LinkedListNode<Graphics.Symbol> GetNode(this LinkedListNode<Graphics.Symbol> sender)
        {
            if (sender.List != null)
            {
                sender.List.Remove(sender);
            }
            return sender;
        }

        public static List<Symbol> Copy(this List<Symbol> sender, params Symbol[] add)
        {
            List<Symbol> temp = new List<Symbol>();
            foreach (Symbol s in sender)
                temp.Add(s);
            foreach (Symbol s in add)
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

        public static void Send(params string[] s)
        {
            if (select != null)
            {
                select.Wrapper(s);
                //selected.Insert(Wrap(s));
            }
        }

        public static void Set(GraphicsEngine sender)
        {
            select = sender;
        }

        public static void SetCursor(object test, int i = 0)
        {
            
        }

        /*public static Symbol Wrap(string s)
        {
            switch (s)
            {
                case "^":
                case "*":
                case "/":
                case "-":
                case "+":
                    return new Operand(s);
                case "(":
                case ")":
                    return new Symbol(s);
                default:
                    if (Input.IsNumber(s))
                    {
                        return new Number(double.Parse(s));
                    }
                    else if (Input.IsVariable(s))
                    {
                        return new Symbol(s);
                    }
                    else
                    {
                        return new Function(s);
                    }
            }
        }*/

        //Parse inputed list of symbols for expressions, fractions, and exponents
        /*public static List<Symbol> Parse(List<Symbol> sender)
        {
            List<Symbol> list = new List<Symbol>();
            foreach (Symbol s in sender)
                list.Add(s);

            List<Symbol> result = new List<Symbol>();

            //Search for and create expressions
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].text == "(")
                {
                    int index = MathText.findMatching(list[i] as Text, list);

                    if (index < list.Count)
                    {
                        List<Symbol> temp = Parse(list.GetRange(i + 1, index - i - 1));
                        print.log("aljsdfkl;ajsd;klfjakl;sdjfkl;ajsdfl; " + temp.Count);
                        temp.Insert(0, list[i]);
                        temp.Add(list[index]);

                        list.RemoveRange(i, index - i + 1);

                        if (temp.Count == 3 && (temp[1].GetType() == typeof(Fraction) || temp[1].GetType() == typeof(Exponent)))
                        {
                            list.Insert(i, temp[1]);
                        }
                        else
                        {
                            list.Insert(i, new Expression(temp));
                        }
                    }
                }
            }

            for (int i = 0; i < list.Count; i++)
            {
                if (i + 1 < list.Count && list[i + 1].text == "/")
                {
                    result.Add(new Fraction(list[i], list[i + 2]));
                    i += 2;
                }
                else if (i + 1 < list.Count && list[i + 1].text == "^")
                {
                    Exponent temp = new Exponent(list[i], list[i + 2]);
                    //temp.format = new Format(gravity: "bottom");
                    list[i + 2].format = new Format(padding: 50, gravity: "bottom");

                    result.Add(temp);
                    i += 2;
                }
                else
                {
                    result.Add(list[i]);
                }
            }

            /*print.log("----parsed list-----");
            foreach (Symbol s in result)
                print.log(s + ", " + s.GetHashCode());
            print.log("----parsed list-----");

            return result;
        }*/

        public static bool IsNumber(string s)
        {
            char chr = s[0];

            return (chr >= 48 && chr <= 57) || chr == 46;

            try
            {
                double.Parse(s);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsVariable(string s)
        {
            return s.Length == 1 && s[0] >= 97 && s[0] <= 122;
        }
    }
}
