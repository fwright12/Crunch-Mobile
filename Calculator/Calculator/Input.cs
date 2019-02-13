using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Graphics;

namespace Calculator
{
    public static class Input
    {
        public static IUserInterface UI;
        
        public static List<Symbol> adding = new List<Symbol>();
        public static int minHeight;

        //Stuff that should be somewhere else
        public static int cursorWidth;
        public static int textHeight;
        public static int textWidth;
        public static int TextSize = 40;

        public static Dictionary<string, List<string>> supportedFunctions = new Dictionary<string, List<string>>()
        {
            { "Pythagorean Theorem", new List<string> {"(", "(", "a", ")", "^", "(", "2", ")", ")", "+", "(", "(", "b", ")", "^", "(", "2", ")", ")", "=", "(", "(", "c", ")", "^", "(", "2", ")", ")"} }
        };

        public static void AddEquation()
        {
            //Hide functionality menu, and show keyboard
            UI.HideFunctionsMenu();
            UI.ShowKeyboard();

            //Input.Send("9", "+", "(", "7", "+", "6", ")", "/", "(", "8", "/", "4", ")");
        }

        public static void Send(params string[] s)
        {
            /*if (selected != null)
            {
                selected.Wrapper(s);
                //selected.Insert(Wrap(s));
            }*/
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
    }
}
