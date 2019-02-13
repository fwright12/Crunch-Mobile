using System;
using System.Collections.Generic;
using System.Text;

namespace Crunch
{
    using Resolver = Func<object, object, object>;

    public static class Parse
    {
        public static Quantity Math(string str, Resolver exponent = null, Resolver divide = null, Resolver multiply = null, Resolver subtract = null, Resolver add = null)
        {
            LinkedList<Quantity> p = new LinkedList<Quantity>();
            //p.AddLast(new Quantity());

            str = "(" + str + ")";
            Node<object> last = new Node<object>("");

            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];
                //print.log("char " + c);

                if (c.IsOpening())
                {
                    p.AddLast(new Quantity());
                }
                else if (c.IsClosing())
                {
                    Quantity q = p.Pop().Value;

                    Tuple<LinkedList<Node<object>>, Resolver>[] stuff = new Tuple<LinkedList<Node<object>>, Resolver>[]
                    {
                        new Tuple<LinkedList<Node<object>>, Resolver>(q.e, exponent),
                        new Tuple<LinkedList<Node<object>>, Resolver>(q.d, divide),
                        new Tuple<LinkedList<Node<object>>, Resolver>(q.m, multiply),
                        new Tuple<LinkedList<Node<object>>, Resolver>(q.a, add),
                        new Tuple<LinkedList<Node<object>>, Resolver>(q.s, subtract)
                    };

                    for (int j = 0; j < 5; j++)
                    {
                        while (stuff[j].Item2 != null && stuff[j].Item1.Count > 0)
                        {
                            Node<object> n = stuff[j].Item1.Pop().Value;
                            object result = stuff[j].Item2(n.Previous?.Value, n.Next?.Value);

                            //Replace node and the things on either side of it with the resolved result
                            Node<object> before = n.Previous?.Previous;
                            q.Remove(n.Previous?.Previous, n.Next?.Next);
                            q.Splice(before, new Node<object>(result));
                        }
                    }

                    if (p.Count == 0)
                    {
                        if (i + 1 == str.Length)
                        {
                            return q;
                        }
                        p.AddLast(new Quantity());
                    }
                    p.Last.Value.AddLast(q);
                }
                else
                {
                    Node<object> node = new Node<object>(str[i]);
                    //p.Last.Value.AddLast(node);

                    if (c == '^')
                    {
                        p.Last.Value.e.AddLast(node);
                    }
                    else if (c == '/')
                    {
                        p.Last.Value.d.AddLast(node);
                    }
                    else if (c == '*')
                    {
                        p.Last.Value.m.AddLast(node);
                    }
                    else if (c == '-' && !(p.Last.Value.Last == null || (p.Last.Value.Last.Value.GetType() == typeof(char) && p.Last.Value.Last.Value.ToString().IsOperand())))
                    {
                        p.Last.Value.s.AddLast(node);
                    }
                    else if (c == '+')
                    {
                        p.Last.Value.a.AddLast(node);
                    }
                    else
                    {
                        node = null;
                        if (p.Last.Value.Last != last)
                        {
                            p.Last.Value.AddLast(last = new Node<object>(""));
                        }
                        last.Value = last.Value.ToString() + str[i];
                    }

                    if (node != null)
                    {
                        p.Last.Value.AddLast(node);
                    }
                }

                if (i + 1 == str.Length)
                {
                    for (int j = 0; j < p.Count; j++)
                    {
                        str += ")";
                    }
                }
            }

            return p.Last.Value;
        }
    }
}
