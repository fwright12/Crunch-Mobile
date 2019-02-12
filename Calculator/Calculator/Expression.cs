using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculator
{
    /*public class EMDAS
    {
        public List<Operation> operations = new List<Operation>();

        public EMDAS()
        {
            operations = list;
        }

        public double MD()
        {
            double first = operations[0];

            foreach (Operation o in operations)
            {

            }
        }
    }*/

    public class Operation
    {
        private Operand operand;
        public dynamic term;

        public Operation (Operand o, Term t)
        {
            operand = o;
            term = t;

            if (operand.text == "-")
            {
                operand = new Operand("+");
                term = term.Multiply(new Number(-1));
            }
        }

        public Term operate(Term sent)
        {
            switch (operand.text)
            {
                case "+":
                    return sent.Add(term);
                case "*":
                    return sent.Multiply(term);
                default:
                    throw new NotSupportedException();
            }
        }
    }

    public class Expression
    {
        public static Expression selected;

        public static double radDegMode = 180 / Math.PI;

        public List<Symbol> parts;
        public Symbol answer
        {
            get
            {
                try
                {
                    return evaluate();
                }
                catch
                {
                    return new Operand("error");
                }
            }
        }

        public List<Quantity> expression = new List<Quantity>();
        //public List<Operation> operations = new List<Operation>();
        
        public Func<string> evaluated;
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

            parts.Insert(Input.pos, sender);
            //answer.value = evaluate();
        }

        public Expression prev(params string[] stops)
        {
            return prev(Input.pos, stops);
        }

        public Expression prev(int start, params string[] stops)
        {
            int index;
            for (index = start; index > -1; index--)
            {
                if (stops.Contains(parts[index].text))
                    break;
            }

            foreach (Symbol s in parts.GetRange(index, start - index))
                print.log(s.ToString());

            return new Expression();
        }

        public Expression()
        {
            parts = new List<Symbol>();
            //answer = new Symbol();

            /*answer = new Symbol();
            answer.parent = this;
            answer.graphicalObject = graphicsHandler.Create(answer);
            graphicsHandler.Insert(answer, pos);*/
        }

        public Term evaluate()
        {
            print.log("starting");
            foreach (Symbol s in parts)
                print.log(s.text);

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
        }

        public Expression(List<Symbol> list)
        {

        }

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
