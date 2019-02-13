using System;
using System.Collections.Generic;
using System.Text;

namespace Crunch
{
    public static partial class Math
    {
        private static OrderedTrie<Operator> operations = new OrderedTrie<Operator>();
        static Func<object, object> negator = (o) => o.parse() * -1;

        static Math()
        {
            operations.Add("sin", UnaryOperator((o) => trig(System.Math.Sin, o)));
            operations.Add("cos", UnaryOperator((o) => trig(System.Math.Cos, o)));
            operations.Add("tan", UnaryOperator((o) => trig(System.Math.Tan, o)));
            operations.Add("^", BinaryOperator((o1, o2) => o1 ^ o2));
            operations.Add("/", BinaryOperator((o1, o2) => o1 / o2));
            operations.Add("*", BinaryOperator((o1, o2) => o1 * o2));
            operations.Add("-", BinaryOperator((o1, o2) => o1 - o2));
            operations.Add("+", BinaryOperator((o1, o2) => o1 + o2));
        }

        private static double trig(Func<double, double> f, Operand o)
        {
            Term t = (Term)(o.Simplify() as dynamic);
            if (t.IsConstant)
            {
                return f(t.Coefficient * System.Math.PI / 180);
            }
            else
            {
                throw new Exception("Cannot operate on non-constant value");
            }
        }

        public static Operator BinaryOperator(Func<Operand, Operand, Operand> f) => Parse.BinaryOperator((o1, o2) => f(o1.parse(), o2.parse()));

        public static Operator UnaryOperator(Func<Operand, Operand> f) => new Operator((o) => f(o[0].parse()), 1);

        public static Operand Evaluate(string str)
        {
            try
            {
                Quantity q = Parse.Math(str, operations, negate: negator);
                print.log("q is " + q);
                return q.parse();
            }
            catch (Exception e)
            {
                print.log("error evaluating", e.Message);
                return null;
            }
        }

        private static Operand parse(this object str)
        {
            while (str is Quantity)
            {
                str = (str as Quantity).First.Value;
            }
            if (str is Operand)
            {
                return str as Operand;
            }

            string s = str.ToString();
            if (s.Substring(0, 1).IsNumber())
            {
                return double.Parse(s);
            }
            else
            {
                if (s.Length > 1 || operations.Contains(s).ToBool())
                {
                    throw new Exception();
                }

                return new Variable(s[0]);
            }
        }
    }
}
