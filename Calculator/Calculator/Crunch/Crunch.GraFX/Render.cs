using System;
using System.Collections.Generic;
using System.Text;

using Xamarin.Forms;

namespace Crunch.GraFX
{
    using Resolver = Func<object, object, object>;

    public static class Render
    {
        public static View[] Math(string str)
        {
            Resolver exponent = (o1, o2) =>
            {
                Quantity q = new Quantity();
                foreach(View v in o1.Wrap())
                {
                    q.AddLast(v);
                }
                q.AddLast(new Exponent(o2.Wrap()));
                return q;
            };
            Resolver fraction = (o1, o2) => new Fraction(new Expression(o1.Wrap()), new Expression(o2.Wrap()));

            return Parse.Math(str, exponent: exponent, divide: fraction).Wrap();
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
                    if (node.Value is Quantity && !((node.Value as Quantity).Last?.Value is Expression))
                    {
                        list.Add(new Text("("));
                    }
                    foreach (View v in node.Value.Wrap())
                    {
                        list.Add(v);
                    }
                    if (node.Value is Quantity && !((node.Value as Quantity).Last?.Value is Expression))
                    {
                        list.Add(new Text(")"));
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
                    string pad = (str == "*" || str == "+") ? " " : "";
                    if (str == "*")
                    {
                        str = "×";
                    }
                    view = new Text(pad + str + pad);
                }
            }

            return new View[] { view };
        }
    }
}
