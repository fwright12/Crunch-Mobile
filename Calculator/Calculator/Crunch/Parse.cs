using System;
using System.Collections.Generic;
using System.Text;

namespace Crunch
{
    using Resolver = Action<Quantity, Node<object>>;

    public static class Parse
    {
        public static readonly HashSet<char> Reserved = new HashSet<char>() { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '+', '-', '*', '×', '/', '(', ')', '.' };

        public static bool IsNegativeSign(this Node<object> node) => (node != null && node.Value is string && node.Value.ToString() == "-") && (node.Previous == null || (node.Previous.Value is string && node.Previous.Value.ToString().IsOperand()));

        private static Node<object> Next(this Node<object> node, Quantity q)
        {
            if (node.Next.IsEqualTo("-"))
            {
                q.Remove(node.Next.Next);
            }
            return node.Next;
        }

        public static void BinaryOperator(Quantity q, Node<object> n, Func<object, object, object> f) => q.Replace(n.Previous, n.Next, new Node<object>(f(n.Previous?.Value, n.Next?.Value)));

        public static void UnaryOperator(Quantity q, Node<object> n, Func<object, object> f) => q.Replace(n, n.Next, new Node<object>(f(n.Next?.Value)));
        
        public static bool IsEqualTo<T>(this Node<T> node, string str) => node != null && node.Value is string && node.Value.ToString() == str;

        public static void Push<T>(this LinkedList<Node<T>> list, Node<T> node)
        {
            if (node.IsEqualTo("^"))
            {
                list.AddLast(node);
            }
            else
            {
                list.AddFirst(node);
            }
        }

        /// <summary>
        /// Removes the last node of the LinkedList and returns it. Returns null if the list is empty
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static LinkedListNode<T> Pop<T>(this LinkedList<T> list)
        {
            LinkedListNode<T> temp = list.Last;
            if (list.Count > 0)
            {
                list.RemoveLast();
            }
            return temp;
        }

        public static Quantity Math(string str, OrderedDictionary<string, Resolver> operations, Func<object, object> negate = null, Action<Quantity> parentheses = null)
        {
            LinkedList<Quantity> p = new LinkedList<Quantity>();

            str = "(" + str + ")";

            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];

                if (c.IsOpening())
                {
                    p.AddLast(new Quantity());
                }
                else if (c.IsClosing())
                {
                    Quantity q = p.Pop().Value;

                    for (int j = 0; j < operations.Count; j++)
                    {
                        string key = operations[j].Key;

                        if (q.operations.ContainsKey(key))
                        {
                            LinkedList<Node<object>> stack = q.operations[key];

                            while (stack.Count > 0)
                            {
                                Node<object> n = stack.Pop().Value;

                                //This node is no longer in the list, skip it
                                //if (n.Previous == null && n.Next == null) continue;

                                /*if (n.Next.IsEqualTo("-"))
                                {
                                    print.log(";laskjdflk;jask;ldf");
                                    Node<object> temp = n.Next;
                                    UnaryOperator(q, n.Next, negate);
                                    temp.Previous = null;
                                    temp.Next = null;
                                }*/

                                bool flag = true;
                                if (n.Value is string && (n.Value.ToString()[0] == ' ' || (n.Previous == null && n.Value.ToString() == "-")))
                                {
                                    n.Next.Value = negate(n.Next.Value);
                                    flag = n.Value.ToString() != "-";
                                }

                                if (flag) operations[j].Value(q, n);
                                else q.Remove(n);

                                /*object result = operations[key](n.Previous?.Value, n.Next?.Value);

                                //Replace node and the things on either side of it with the resolved result
                                Node<object> before = n.Previous?.Previous;
                                q.Remove(n.Previous?.Previous, n.Next?.Next);
                                q.Splice(before, new Node<object>(result));*/
                            }
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

                    parentheses?.Invoke(q);
                    p.Last.Value.AddLast(q);
                }
                else
                {
                    string s = c.ToString();
                    Node<object> node = new Node<object>(s);
                    p.Last.Value.AddLast(node);

                    if (operations.ContainsKey(s))
                    {
                        p.Last.Value.operations.TryGet(s).Push(node);

                        //Next thing is a minus sign (so it's really a negative)
                        if (i + 1 < str.Length && str[i + 1] == '-')
                        {
                            i++;
                            node.Value = " " + s;
                        }
                    }
                    else
                    {
                        if (s.IsNumber())
                        {
                            while (i + 1 < str.Length && str[i + 1].ToString().IsNumber())
                            {
                                node.Value = node.Value.ToString() + str[++i];
                            }
                        }
                        if (node.Previous != null && (!(node.Previous.Value is string) || !operations.ContainsKey(node.Previous.Value.ToString().Trim())))
                        {
                            p.Last.Value.operations.TryGet("").Push(node);
                        }
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

            throw new Exception("Error parsing math");
        }
    }
}
