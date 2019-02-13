using System;
using System.Collections.Generic;
using System.Text;

namespace Crunch
{
    using Resolver = Action<Quantity, Node<object>>;

    public static partial class Math
    {
        private static OrderedDictionary<string, Resolver> operations = new OrderedDictionary<string, Resolver>();
        static Func<object, object> negator = (o) => o.parse() * -1;
        static Func<object, object> negate = (o) => negator(o);

        static Math()
        {
            Resolver juxtapose = (q, n) =>
            {
                Node<object> node = new Node<object>("*");
                q.Splice(n.Previous, node);
                q.operations.TryGet("*").Push(node);
            };

            operations.Add("", juxtapose);
            operations.Add("^", (q, n) => Parse.BinaryOperator(q, n, (o1, o2) => o1.parse() ^ o2.parse()));
            operations.Add("/", (q, n) => Parse.BinaryOperator(q, n, (o1, o2) => o1.parse() / o2.parse()));
            operations.Add("*", (q, n) => Parse.BinaryOperator(q, n, (o1, o2) => o1.parse() * o2.parse()));
            operations.Add("+", (q, n) => Parse.BinaryOperator(q, n, (o1, o2) => o1.parse() + o2.parse()));
            operations.Add("-", (q, n) => Parse.BinaryOperator(q, n, (o1, o2) => o1.parse() - o2.parse()));
        }

        public static Operand Evaluate(string str)
        {
            bool a = false;
            //a = true;
            if (a)
            {
                Quantity q = Parse.Math(str, operations, negate: negator);
                print.log("q is " + q, q.parse().GetType().ToString());
                return q.parse();
            }

            try
            {
                Quantity q = Parse.Math(str, operations, negate: negator);
                print.log("q is " + q, q.parse().GetType().ToString());
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
                if (s.Length > 1 || operations.ContainsKey(s))
                {
                    throw new Exception();
                }

                return new Variable(s[0]);
            }
        }
    }
}
