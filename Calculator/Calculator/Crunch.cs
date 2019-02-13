using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Crunch;

namespace Calculator
{
    public static class Cruncher
    {
        public static Term Evaluate(List<Graphics.Symbol> calculate)
        {
            //Combine numbers
            for (int i = 0; i < calculate.Count; i++)
            {
                if (calculate[i].GetType() == typeof(Graphics.Number))
                {
                    string number = "";

                    int j = i;
                    while (j < calculate.Count && calculate[j].GetType() == typeof(Graphics.Number))
                    {
                        number += (calculate[j] as Graphics.Number).text;
                        j++;
                    }

                    calculate.RemoveRange(i, j - i);
                    calculate.Insert(i, new Graphics.Number(number));
                }
                /*else if (calculate[i].GetType() != typeof(Graphics.Text) || calculate[i].GetType() != typeof(Graphics.Layout))
                {
                    calculate.RemoveAt(i--);
                }*/
            }

            List<Term> terms = new List<Term>();
            List<Graphics.Text> operands = new List<Graphics.Text>();

            print.log("start");
            foreach (Graphics.Symbol s in calculate)
                print.log(s);
            //var list = new List<Graphics.Symbol>();
            foreach(Graphics.Symbol s in calculate)
            {
                print.log(s);
                try
                {
                    //CheckForImplicitOperator(s as dynamic);
                    terms.Add(s as dynamic);
                }
                catch
                {
                    print.log("threw error");
                    if (s is Graphics.Text)
                    {
                        if ((s as Graphics.Text).IsOperand())
                        {
                            operands.Add(s as Graphics.Text);
                        }
                    }
                }
            }

            foreach (Term t in terms)
                print.log(t);
            foreach (Graphics.Text t in operands)
                print.log(t.text);

            //Multiplication
            for (int i = 0; i < operands.Count; i++)
            {
                if (operands[i].text == "*")
                {
                    operate("*", ref terms, ref operands, ref i);
                }
            }

            //Addition and Subtraction
            for (int i = 0; i < operands.Count; i++)
            {
                operate(operands[i].text, ref terms, ref operands, ref i);
            }

            /*for (int i = 0; i < list.Count; i++)
            {
                if (i % 2 == 0)
                {
                    terms.Add(list[i] as dynamic);
                }
                else
                {
                    operands.Add(list[i] as Graphics.Text);
                }
            }*/

            /*print.log("end");
            foreach (Graphics.Symbol s in list)
                print.log(s.GetType());

            //Multiplication
            for (int i = 1; i < list.Count; i += 2)
            {
                if ((list[i] as Graphics.Text).text == "*")
                {
                    operate("*", ref list, ref i);
                }
            }
            print.log("doing addition");
            //Addition and Subtraction
            for (int i = 1; i < list.Count; i += 2)
            {
                print.log(i);

                operate((list[i] as Graphics.Text).text, ref list, ref i);

                print.log("adding " + i);
                foreach (Graphics.Symbol s in list)
                    print.log(s.GetType());
            }*/

            /*var operands = new List<string>();
            var terms = new List<Term>();

            print.log("start");
            for (int i = 0; i < calculate.Count; i++)
            {
                print.log(calculate[i]);
                try
                {
                    terms.Add(calculate[i] as dynamic);
                }
                catch
                {
                    print.log("threw error");
                    if (calculate[i].GetType() == typeof(Graphics.Text) && (calculate[i] as Graphics.Text).IsOperand())
                    {
                        operands.Add((calculate[i] as Graphics.Text).text);
                    }
                }
            }
            foreach (Term t in terms)
                print.log(t);
            foreach (string s in operands)
                print.log(s);

            for (int i = 0; i < calculate.Count; i++)
            {
                if (calculate[i].GetType() == typeof(Graphics.Text) && calculate[i].GetType() != typeof(Graphics.Number))
                {
                    //calculate[i - 1] = operate((calculate[i - 1] as Graphics.Number), (calculate[i] as Graphics.Text).text, (calculate[i + 1] as Graphics.Number));
                }
            }*/

            print.log("reached return " + terms[0]);
            return (terms[0] as dynamic) + new Number(0);
        }

        private static void operate(string operation, ref List<Term> terms, ref List<Graphics.Text> operands, ref int i)
        {
            if (operation == "+")
            {
                terms[i] = (terms[i] as dynamic) + (terms[i + 1] as dynamic);
            }
            else if (operation == "-")
            {
                terms[i] = terms[i] - terms[i + 1];
            }
            else if (operation == "*")
            {
                terms[i] = (terms[i] as dynamic) * (terms[i + 1] as dynamic);
            }

            terms.RemoveAt(i + 1);
            operands.RemoveAt(i--);

            /*Term prev = list[i - 1] as dynamic;
            Term next = list[i + 1] as dynamic;

            print.log(next.GetType() + ", " + prev.GetType());

            if (operation == "+")
            {
                list[i - 1] = (prev as dynamic) + (next as dynamic);
            }
            else if (operation == "-")
            {
                list[i - 1] = (prev as dynamic) - (next as dynamic);
            }
            else if (operation == "*")
            {
                list[i - 1] = (prev as dynamic) * (next as dynamic);
            }
            //print.log(answer.GetType());
            //list[i - 1] = (answer as dynamic);
            print.log("operated " + list[i - 1]);
            list.RemoveRange(i, 2);
            i -= 2;*/
        }

        /*private static Term operate(Term t1, string operation, Term t2)
        {
            if (operation == "+")
            {
                return t1 + t2;
            }
            else if (operation == "*")
            {
                return t1 * t2;
            }
            else
            {
                throw new NotSupportedException();
            }
        }*/

        /*public static double radDegMode = 180 / Math.PI;

        public static Term Evaluate(Expression sent)
        {
            List<Symbol> calculate = new List<Symbol>();
            foreach (Symbol s in sent.parts)
                calculate.Add(s);

            foreach (Symbol s in calculate)
                print.log(s.text);
            //calculate.Remove(Symbol.Cursor);

            //Combine numbers
            for (int i = 0; i < calculate.Count; i++)
            {
                if (calculate[i].GetType() == typeof(Number))
                {
                    string number = "";

                    int j = i;
                    while (j < calculate.Count && calculate[j].GetType() == typeof(Number))
                    {
                        number += calculate[j].text;
                        j++;
                    }

                    calculate.RemoveRange(i, j - i);
                    calculate.Insert(i, new Number(double.Parse(number)));
                }
            }

            //Evaluate exponents
            for (int i = 0; i < calculate.Count; i++)
            {
                if (calculate[i].format == Format.Exponent)
                {
                    throw new Exception();
                    calculate[i - 1] = new Exponent(Expression.Wrap(calculate[i - 1]), Expression.Wrap(calculate[i]));
                    calculate.RemoveAt(i);
                    i--;
                }
            }

            //Evaluate expressions and remove text
            for (int i = 0; i < calculate.Count; i++)
            {
                Type t = calculate[i].GetType();

                if (t == typeof(Expression))
                {
                    calculate[i] = ((Expression)calculate[i]).answer as Term;
                }
                else if (t == typeof(Symbol))
                {
                    calculate.RemoveAt(i--);
                }
                else if (t == typeof(Fraction))
                {
                    calculate[i] = (calculate[i] as Fraction).Simplify();
                }
            }

            //Calculate functions
            for (int i = 0; i < calculate.Count; i++)
            {
                if (calculate[i].GetType() == typeof(Function))
                {
                    calculate[i] = (calculate[i] as Function).evaluate(calculate[i + 1] as Term);
                    calculate.RemoveAt(i + 1);
                }
            }

            //Convert subtraction to addition
            for (int i = 1; i < calculate.Count; i += 2)
            {
                if (((Operand)calculate[i]).text == "-")
                {
                    calculate[i] = new Operand("+");
                    calculate[i + 1] = ((dynamic)calculate[i + 1]).Multiply(new Number(-1));
                }
            }

            //Multiplication
            for (int i = 0; i < calculate.Count; i++)
            {
                if (calculate[i].text == "*")
                {
                    List<Symbol> temp = next(calculate, 1, i - 1, "+", "-");
                    calculate.RemoveRange(i - 1, temp.Count);
                    calculate.Insert(i - 1, operate("*", temp));

                    i = -1;
                }
            }

            //Addition
            return operate("+", calculate).Simplify();
        }

        public static Term operate(string o, List<Symbol> list)
        {
            Term value = (Term)list[0];

            //Get all terms in the list
            List<Term> terms = new List<Term>();
            foreach (Symbol s in list)
            {
                Term temp = s as Term;
                if (temp != null)
                {
                    terms.Add(temp);
                }
            }

            while (terms.Count > 1)
            {
                if (o == "*")
                    terms[0] = ((dynamic)terms[0]).Multiply((dynamic)terms[1]);
                else if (o == "+")
                    terms[0] = ((dynamic)terms[0]).Add((dynamic)terms[1]);

                terms.RemoveAt(1);
            }

            return terms[0];
        }

        public static List<Symbol> next(List<Symbol> list, int dir, params string[] stops)
        {
            return next(list, dir, Input.selected.pos + dir, stops);
        }

        public static List<Symbol> next(List<Symbol> list, int dir, int start, params string[] stops)
        {
            List<Symbol> answer = new List<Symbol>();

            int index = start;
            while (index < list.Count && index > -1)
            {
                if (stops.Contains(list[index].text))
                    break;

                answer.Add(list[index]);
                index += dir;
            }

            return answer;
        }*/
    }
}
