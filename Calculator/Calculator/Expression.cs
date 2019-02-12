using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculator
{
    public class Expression : Symbol
    {
        public static double radDegMode = 180 / Math.PI;

        public List<Symbol> parts;

        public override string text
        {
            get
            {
                return answer.text;
            }
        }

        public Symbol answer
        {
            get
            {
                print.log("call from answer");

                try
                {
                    return evaluate();
                }
                catch
                {
                    print.log("error");
                    if (parts.Count == 0)
                        return new Text("");
                    else
                        return new Text("error");
                }
            }
        }

        public static List<Symbol> next(List<Symbol> list, int dir, params string[] stops)
        {
            return next(list, dir, Input.pos + dir, stops);
        }

        public static List<Symbol> next(List<Symbol> list, int dir, int start, params string[] stops)
        {
            List<Symbol> answer = new List<Symbol>();

            int index = start;
            while(index < list.Count && index > -1)
            {
                if (stops.Contains(list[index].text))
                    break;

                answer.Add(list[index]);
                index += dir;
            }
            
            return answer;
        }

        public Expression()
        {
            parts = new List<Symbol>();// { new Space() };
        }

        public Expression(Symbol s)
        {
            parts = new List<Symbol>() { s };
        }

        public Expression(List<Symbol> list)
        {
            parts = list;
        }

        public override List<Symbol> GetText()
        {
            List<Symbol> result = new List<Symbol>();

            foreach(Symbol s in parts)
            {
                result.Add(s);
            }

            return result;
        }

        public static Expression Wrap(Symbol sender)
        {
            if (sender.GetType() == typeof(Expression))
                return sender as Expression;
            else
                return new Expression(sender);
        }

        private Term evaluate()
        {
            //parts = new List<Symbol>() { new Number(63), new Operand("+"), new Number(9) };
            List<Symbol> calculate = new List<Symbol>();
            foreach (Symbol s in parts)
                calculate.Add(s);

            print.log("evaluating...");
            foreach (Symbol s in calculate)
                print.log(s);

            //Exponents
            for (int i = 0; i < calculate.Count; i++)
            {
                if (calculate[i].format == Format.Exponent)
                {
                    calculate[i - 1] = new Exponent(Wrap(calculate[i - 1]), Wrap(calculate[i]));
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
                else if (t == typeof(Text))
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
                    print.log("LSDFL:JSDFL:KSDF:L ");
                    //print.log("LSDFL:JSDFL:KSDF:L " + (calculate[i + 1] as Expression).answer);
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
                    calculate[i + 1] = ((Term)calculate[i + 1]).Multiply(new Number(-1));
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

        private Term operate(string o, List<Symbol> list)
        {
            Term value = (Term)list[0];

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

        public override Symbol Copy()
        {
            List<Symbol> result = new List<Symbol>();

            foreach (Symbol s in parts)
                result.Add(s.Copy());

            return new Expression(result);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(Expression))
                return false;

            return GetHashCode() == (obj as Expression).GetHashCode();
        }

        public override int GetHashCode()
        {
            int result = 0;

            foreach (Symbol s in parts)
            {
                if (s != Input.lastAdded)
                    result = (result + s.GetHashCode()) % int.MaxValue;
            }

            return result;
        }

        public void FixWeirdError()
        {
            operate("+", new List<Symbol>() { new Number(1), new Number(1) });
        }
    }

    public class print
    {
        public static void log(object p)
        {
            System.Diagnostics.Debug.WriteLine(p);
        }
    }
}
