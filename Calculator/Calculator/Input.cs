using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculator
{
    public delegate void CursorListener(object sender);

    public static class Input
    {
        public static Equation selected;

        //public static IGraphicsHandler graphicsHandler;
        public static IGraphicsHandler graphicsHandler;// = Xamarin.Forms.DependencyService.Get<IGraphicsHandler>();

        public static object cursor;
        public static object phantomCursor;
        public static List<Symbol> adding = new List<Symbol>();

        public static void Send(string s)
        {
            if (selected != null)
            {
                selected.Insert(Wrap(s));
            }
        }

        public static void Set(Equation sender)
        {
            if (selected != null)
            {
                graphicsHandler.acursor -= selected.cursorListener;
            }
            graphicsHandler.acursor += sender.cursorListener;
            selected = sender;
        }

        public static void SetCursor(object test, int i = 0)
        {
            
        }

        public static Symbol Wrap(string s)
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
                    return new Text(s);
                default:
                    if (Input.IsNumber(s))
                    {
                        return new Number(double.Parse(s));
                    }
                    else if (Input.IsVariable(s))
                    {
                        return new Text(s);
                    }
                    else
                    {
                        return new Function(s);
                    }
            }
        }

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
