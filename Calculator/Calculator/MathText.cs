using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculator
{
    public class MathText : List<Symbol>
    {
        /*public new int IndexOf(Symbol sender)
        {
            if (base.IndexOf(Symbol.Cursor) < base.IndexOf(sender))
            {
                return base.IndexOf(sender) - 1;
            }
            else
            {
                return base.IndexOf(sender);
            }
        }*/

        //Parse inputed list of symbols for expressions, fractions, and exponents
        public List<Symbol> Parse()
        {
            return Parse(this);
        }

        public List<Symbol> Parse(List<Symbol> sender)
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
                    Text other = findMatching(list[i] as Text);

                    if (list.Contains(other))
                    {
                        int index = list.IndexOf(other);

                        //Get everything in between the parentheses
                        List<Symbol> temp = Parse(list.GetRange(i + 1, index - i - 1));
                        //Remove the parentheses and everything in between
                        list.RemoveRange(i, index - i + 1);

                        if (temp.Count == 1 && (temp[0].GetType() == typeof(Fraction) || temp[0].GetType() == typeof(Exponent)))
                        {
                            list.Insert(i, temp[0]);
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
            print.log("----parsed list-----");*/

            return result;
        }

        public Text findMatching(Text first)
        {
            int dir = 1;
            if (first.text == ")")
                dir = -1;

            int count = 0;

            int index;
            for (index = IndexOf(first) + dir; index < Count && index > -1; index += dir)
            {
                if ((dir == 1 && this[index].text == "(") || (dir == -1 && this[index].text == ")"))
                {
                    count++;
                }
                if ((dir == -1 && this[index].text == "(") || (dir == 1 && this[index].text == ")"))
                {
                    if (count == 0)
                        break;

                    count--;
                }
            }

            if (index < Count)
            {
                return this[index] as Text;
            }
            else
            {
                return null;
            }
        }
    }
}
