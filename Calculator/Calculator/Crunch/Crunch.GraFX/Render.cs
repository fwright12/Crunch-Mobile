using System;
using System.Collections.Generic;
using System.Text;

using Xamarin.Forms;

namespace Crunch.GraFX
{
    using Resolver = Action<Quantity, Node<object>>;

    public static class Render
    {
        private static OrderedDictionary<string, Resolver> operations = new OrderedDictionary<string, Resolver>();

        static Render()
        {
            Resolver exponent = (q, n) => Parse.BinaryOperator(q, n, (o1, o2) => new Quantity(o1, new Exponent(o2.Wrap())));
            Resolver fraction = (q, n) => Parse.BinaryOperator(q, n, (o1, o2) => new Fraction(new Expression(o1.Wrap()), new Expression(o2.Wrap())));

            operations.Add("^", exponent);
            operations.Add("/", fraction);
        }

        public static View[] Math(string str)
        {
            Action<Quantity> parentheses = (q) => { q.AddFirst("("); q.AddLast(")"); };
            Func<object, object> negate = (o) => "-" + o.ToString();
            /*Resolver negate = (q, n) =>
            {
                //if (n.Previous != null && n.Next != null && (n.Previous.Value is string && operations.ContainsKey(n.Previous.Value.ToString())))
                //{
                    //Parse.UnaryOperator(q, n, (o) => "-" + o.ToString());
                //}
            };*/

            print.log("parsing " + str);
            return Parse.Math(str, operations, negate, parentheses).Wrap();
        }

        public static View[] Wrap(this object o)
        {
            View view = null;
            if (o == null)
            {
                return new View[0];
            }
            else if (o is Quantity)
            {
                List<View> list = new List<View>();

                Node<object> node = (o as Quantity).First;
                while (node != null)
                {
                    foreach (View v in node.Value.Wrap())
                    {
                        list.Add(v);
                    }
                    node = node.Next;
                }

                return list.ToArray();
            }
            else if (o is View)
            {
                view = o as View;
            }
            else
            {
                string str = o.ToString();

                if (str == "-")
                {
                    view = new Minus();
                }
                else
                {
                    List<View> views = new List<View>();
                    foreach (char chr in str)
                    {
                        char c = chr;
                        string pad = (c == '*' || c == '+') ? " " : "";
                        if (c == '*')
                        {
                            c = '×';
                        }
                        views.Add(new Text(pad + c + pad));
                    }
                    return views.ToArray();
                }
            }

            return new View[] { view };
        }
    }
}
