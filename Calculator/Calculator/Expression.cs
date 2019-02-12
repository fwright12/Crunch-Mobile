using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculator
{
    public class Expression : Symbol
    {
        public static Expression selected;

        public static double radDegMode = 180 / Math.PI;

        public List<Symbol> parts;
        public bool Parend
        {
            set
            {
                if (value != parend)
                {
                    if (value)
                    {
                        parts.Insert(parts.Count, new Text(")"));
                        parts.Insert(0, new Text("("));
                    }
                    else
                    {
                        parts.RemoveAt(parts.Count);
                        parts.RemoveAt(0);
                    }

                    parend = value;
                }
            }
        }

        private bool parend = false;

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
                //return evaluate();

                try
                {
                    print.log("call fron answer");
                    return evaluate();
                }
                catch
                {
                    if (parts.Count == 0)
                        return new Text("");
                    else
                        return new Text("error");
                }
            }
        }

        //public List<Operation> operations = new List<Operation>();
        
            //private Quantity answer;

        /*public Symbol AddChild(Symbol sender, string s)
        {
            sender.parent = this;
            sender.graphicalObject = graphicsHandler.Create(sender);
            graphicsHandler.Insert(sender, pos);
            sender.value = s;

            return sender;
        }*/

        public void Insert(Symbol sender)
        {
            /*sender.parent = this;
            if (sender.graphicalObject == null)
                sender.graphicalObject = graphicsHandler.Create(sender);
            graphicsHandler.Insert(sender, pos);*/

            Insert(Input.pos, sender);
            //answer.value = evaluate();
        }

        public void Insert(int pos, Symbol sender)
        {
            /*if (parts.Count == 1 && parts[0].GetType() == typeof(Space))
            {
                parts.RemoveAt(0);
            }*/

            int add = 0;
            if (parend)
                add = 1;

            if (parts.Count == 1 + add && parts[add].GetType() == typeof(Space))
            {
                parts.RemoveAt(add);
            }

            parts.Insert(pos, sender);
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
                //print.log(index + ", " + parts[index].text + ", " + stops[0] +", "+ stops[1]);
                if (stops.Contains(list[index].text))
                    break;

                answer.Add(list[index]);
                index += dir;
            }
            
            return answer;
        }

        public Expression()
        {
            parts = new List<Symbol>() { new Space() };
            //Input.pos++;
            //parend = true;
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
        public Term evaluate()
        {
            //parts = new List<Symbol>() { new Number(63), new Operand("+"), new Number(9) };
            List<Symbol> calculate = new List<Symbol>();
            foreach (Symbol s in parts)
                calculate.Add(s);
            //calculate.RemoveAt(0);

            print.log("evaluating...");
            foreach (Symbol s in calculate)
                print.log(s);

            for (int i = 0; i < calculate.Count; i++)
            {
                Type t = calculate[i].GetType();

                if (t == typeof(Expression))
                {
                    calculate[i] = ((Expression)calculate[i]).evaluate();
                }
                else if (t == typeof(Text))
                {
                    calculate.RemoveAt(i--);
                }
            }

            for (int i = 0; i < calculate.Count; i++)
            {
                if (calculate[i].GetType() == typeof(Function))
                {
                    calculate[i] = new Number(((Function)calculate[i]).value);
                    calculate.RemoveAt(i + 1);
                }
            }

            for (int i = 1; i < calculate.Count; i+=2)
            {
                if (((Operand)calculate[i]).text == "-")
                {
                    calculate[i] = new Operand("+");
                    calculate[i + 1] = ((Term)calculate[i + 1]).Multiply(new Number(-1));
                }
            }

            //print.log("***converted to addition***");
            //foreach (Symbol s in calculate)
            //    print.log(s.text);

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

            return operate("+", calculate);
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
                    terms[0] = terms[0].Multiply((dynamic)terms[1]);
                else if (o == "+")
                    terms[0] = terms[0].Add((dynamic)terms[1]);

                terms.RemoveAt(1);
            }

            return terms[0];
        }

        public static List<Symbol> Parse(List<Symbol> list)
        {
            List<Symbol> result = new List<Symbol>();

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].text == "/")
                {
                    Expression[] frac = new Expression[2];

                    for (int j = 0; j < 2; j++) {
                        try
                        {
                            frac[j] = new Expression(new List<Symbol>() { list[i + (j * 2 - 1)] });
                        }
                        catch
                        {
                            frac[j] = new Expression();
                        }
                    }

                    result[result.Count - 1] = new Fraction(frac[0], frac[1]);

                    i++;
                }
                else
                {
                    result.Add(list[i]);
                }
            }

            print.log("----parsed list-----");
            foreach (Symbol s in result)
                print.log(s);
            print.log("----parsed list-----");

            return result;
        }

        /*public override bool Equals(object obj)
        {
            return (obj as Expression).parts == parts;
        }*/

        public void FixWeirdError()
        {
            operate("+", new List<Symbol>() { new Number(1), new Number(1) });
        }

        /*public Term evaluatea()
        {
            print.log("evaluating...");
            foreach (Symbol s in parts)
                print.log(s.text);

            for (int i = 0; i < parts.Count; i++)
            {
                if (parts[i].text == "*")
                {
                    //parts[i - 1] = next(i - 1, "+", "-").answer;

                    print.log("***new list***");
                    foreach (Symbol s in parts)
                        print.log(s.text);

                    i = -1;
                }
            }

            List<Operation> operations = new List<Operation>();
            Term axis = parts[0] as Number;

            for (int i = 1; i < parts.Count; i += 2)
            {
                //print.log(parts[i].GetType() + ", " + parts[i + 1].GetType());
                operations.Add(new Operation((Operand)parts[i], (Term)parts[i + 1]));
            }

            foreach (Operation o in operations)
            {
                axis = o.operate(axis);
                print.log("value " + axis.value);
            }

            //axis.value = " = " + axis.value;
            return axis;
        }*/

        /*public Expression(List<string> list)
        {
            try
            {
                if (list.Contains("+") || list.Contains("-"))
                {
                    while (list.Count > 0)
                    {
                        int next;
                        for (next = 1; next < list.Count; next++)
                            if (list[next] == "+" || list[next] == "-")
                                break;

                        //operations.Add(new Operation(list[0], new Expression(list.GetRange(1, next - 1)).evaluate()));

                        print.log("between " + next);
                        foreach (string s in list.GetRange(1, next - 1))
                            print.log(s);

                        list = list.GetRange(next, list.Count - next);

                        print.log("remaining");
                        foreach (string s in list)
                            print.log(s);
                    }
                }
                else
                {
                    //answer = Quantity.Parse(list[0]);
                    for (int i = 1; i < list.Count; i += 2)
                        operations.Add(new Operation(list[i], Quantity.Parse(list[i + 1])));
                }

                evaluated = delegate { return evaluate(); };
            }
            catch
            {
                evaluated = delegate { return "error"; };
            }
            //answer = double.Parse(list[0]);
            list.RemoveAt(0);

            print.log("starting");

            //list = new List<string>() { "45", "+", "234", "-", "23", "*", "8", "/", "78", "+", "3" };
            while (list.Count > 0)
            {
                if (list[0] == "+" || list[0] == "-")
                {
                    int next;
                    for (next = 1; next < list.Count; next++)
                        if (list[next] == "+" || list[next] == "-")
                            break;

                    //operations.Add(new Operation(list[0], new Expression(list.GetRange(1, next - 1)).evaluate()));

                    print.log("between " + next);
                    foreach (string s in list.GetRange(1, next - 1))
                        print.log(s);

                    list = list.GetRange(next, list.Count - next);

                    print.log("remaining");
                    foreach (string s in list)
                        print.log(s);
                }
                else
                {
                    operations.Add(new Operation(list[0], Quantity.Parse(list[1])));
                }
            }

            /*double start = double.Parse(list[0]);

            for (int i = 1; i < list.Count; i++)
            {
                if (list[i] == "+" || list[i] == "-")
                {
                    test.Add(new Operation(list[i]));
                }
                else
                {
                    test[test.Count - 1].number.Append(new Operation(list[i], double.Parse(list[i + 1])));
                }
            }
        }*/

        public void add(double sender)
        {
            //expression.Add(new Quantity(sender));
        }

        public void add(string sender)
        {

        }

        /*public string evaluate(string exp)
        {
            List<double> numbers = new List<double>();
            List<string> operands = new List<string>();
            List<string> functions = new List<string>();

            string answer = "error";

            while (exp.Contains("("))
            {
                int index = exp.IndexOf("(");

                string inside = "";
                int i;
                for (i = index + 1; i < exp.Length; i++)
                {
                    char charAt = exp[i];

                    if (charAt == '(')
                    {
                        index = i;
                    }
                    else if (charAt == ')')
                    {
                        print.log("before " + exp.Substring(index + 1, i - index - 1));
                        inside = evaluate(exp.Substring(index + 1, i - index - 1));
                        break;
                    }
                }
                print.log(i +", "+ exp.Length);
                if (i == exp.Length)
                    return answer;

                exp = exp.Substring(0, index) + inside + exp.Substring(i + 1);
                print.log("after " + exp + ", " + "\"" + inside + "\"");
            }

            print.log("next");

            int pos = 0;
            string num = "";
            string func = "";

            exp += "+";

            while (pos < exp.Length)
            {
                char charAt = exp[pos];

                if (charAt == 46 || (charAt >= 48 && charAt <= 57))
                {
                    num += charAt.ToString();
                }
                else if (charAt == 42 || charAt == 43 || charAt == 45 || charAt == 47 || charAt == 94)
                {
                    try
                    {
                        if (func == "")
                            numbers.Add(double.Parse(num));
                        else
                        {
                            numbers.Add(function(func, double.Parse(num)));
                            func = "";
                        }
                    }
                    catch
                    {
                        return answer;
                    }

                    print.log(pos);
                    num = "";
                    operands.Add(charAt.ToString());
                }
                else
                {
                    func += charAt.ToString();
                }

                pos++;
            }

            operands.RemoveAt(operands.Count - 1);

            string[] pemdas = new string[] { "^", "*", "/", "+", "-" };

            //"3+9/3-5*6.9"

            print.log("numbers");
            foreach (double d in numbers)
                print.log(d);
            print.log("operands");
            foreach (string s in operands)
                print.log(s);
            print.log("start");

            foreach (string s in pemdas)
            {
                while (operands.IndexOf(s) != -1)
                {
                    int index = operands.IndexOf(s);

                    //print.log(index);
                    //print.log(numbers[index] +", "+ s +", "+ numbers[index + 1]);

                    numbers.Insert(index, operate(numbers[index], s, numbers[index + 1]));
                    numbers.RemoveRange(index + 1, 2);
                    operands.Remove(s);
                }
            }

            return numbers[0].ToString();
        }*/

        private static double operate(double num1, string operand, double num2)
        {
            switch (operand)
            {
                case "^":
                    return Math.Pow(num1, num2);
                case "+":
                    return num1 + num2;
                case "-":
                    return num1 - num2;
                case "*":
                    return num1 * num2;
                case "/":
                    return num1 / num2;
                default:
                    throw new NotSupportedException();
            }
        }

        private static double function(string func, double num)
        {
            switch (func)
            {
                case "sin":
                    return Math.Sin(num) * radDegMode;
                case "cos":
                    return Math.Cos(num) * radDegMode;
                case "tan":
                    return Math.Tan(num) * radDegMode;
                case "sqrt":
                    return Math.Sqrt(num);
                case "log":
                    return Math.Log10(num);
                case "ln":
                    return Math.Log(num, Math.E);
                default:
                    throw new NotSupportedException();
            }
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
