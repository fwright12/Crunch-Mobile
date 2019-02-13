using System;
using System.Collections.Generic;
using System.Text;

namespace Crunch
{
    public delegate object Operation(params object[] operands);

    public class Operator
    {
        public Operation Operate;
        public int[] Targets;
        public Operator(Operation operate, params int[] targets) { Operate = operate; Targets = targets; }
    }

    public static class Parse
    {
        public static readonly HashSet<char> Reserved = new HashSet<char>() { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '+', '-', '*', '×', '/', '(', ')', '.' };

        public static Operator BinaryOperator(Func<object, object, object> f) => new Operator((o) => f(o[0], o[1]), -1, 1);

        public static bool IsEqualTo<T>(this Node<T> node, string str) => node != null && node.Value is string && node.Value.ToString() == str;

        public static void Push<T>(this LinkedList<Node<T>> list, Node<T> node)
        {
            string text = node != null && node.Value is string ? node.Value.ToString() : "";
            if (text == "^" || text == "sin" || text == "cos" || text == "tan")
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

        /// <summary>
        /// Parse math
        /// </summary>
        /// <param name="str"><summary>string to be parsed</summary></param>
        /// <param name="operations"><summary>operations to execute. Add strings that should be recognized as operations, and the corresponding operation that should occur</summary></param>
        /// <param name="negate">include if you would like negative signs (ie 6+-4) to be recognized</param>
        /// <param name="parentheses"></param>
        /// <param name="juxtapose">set true if you would like juxtaposition (ie 6(1+2)) to be recognized</param>
        /// <returns></returns>
        public static Quantity Math(string str, OrderedTrie<Operator> operations, Func<object, object> negate = null, Action<Quantity> parentheses = null)
        {
            Operator negator = new Operator((o) => negate(o[0]), 1);
            LinkedList<Quantity> p = new LinkedList<Quantity>();

            /*bool juxtapose = operations.Contains("*").ToBool();
            Action<Node<object>> checkForJuxtapose = (node) =>
            {
                //Check backwards
                if (juxtapose && node?.Previous != null && (!(node.Previous.Value is string) || !operations.Contains(node.Previous.Value.ToString()).ToBool()))
                {
                    Node<object> n = new Node<object>("*");
                    p.Last.Value.Splice(node.Previous, n);
                    p.Last.Value.operations.TryGet("*").Push(n);
                }
            };*/

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
                                Node<object> node = stack.Pop().Value;
                                Operator op = (node.IsEqualTo("-") && node.Previous == null) ? negator : operations[j].Value;

                                List<object> operands = new List<object>();
                                foreach (int num in op.Targets)
                                {
                                    Node<object> operand = node + num;

                                    if (operand == node.Next && operand.IsEqualTo("-"))
                                    {
                                        operand.Next.Value = negate(operand.Next.Value);
                                        q.Remove(operand);
                                        operand = node.Next;
                                    }

                                    operands.Add(q.Remove(operand)?.Value);
                                }

                                node.Value = op.Operate(operands.ToArray());
                            }
                        }

                        if (key == "*")
                        {
                            Node<object> node = q.First;
                            Operator juxtapose = operations[j].Value;

                            while (node != null && node.Next != null)
                            {
                                if (node.IsEqualTo("+") || node.IsEqualTo("-") || node.Next.IsEqualTo("+") || node.Next.IsEqualTo("-"))
                                {
                                    node = node.Next;
                                }
                                else
                                {
                                    node.Value = juxtapose.Operate(node.Value, node.Next.Value);
                                    q.Remove(node.Next);
                                }
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
                    //q.AddFirst("(");
                    //q.AddLast(")");
                    p.Last.Value.AddLast(q);
                    //checkForJuxtapose(p.Last.Value.Last);
                }
                else
                {
                    //Keep track of the end position of the longest possible operation we find
                    int operation = i;
                    for (int j = i; j < str.Length; j++)
                    {
                        TrieContains search = operations.Contains(str.Substring(i, j - i + 1));
                        //At this point there is no operation that starts like this
                        if (search == TrieContains.No)
                        {
                            break;
                        }
                        //We found an operation, but it might not be the longest one
                        if (search == TrieContains.Full)
                        {
                            operation = j + 1;
                        }
                    }

                    string s = operation > i ? str.Substring(i, operation - i) : c.ToString();
                    Node<object> node = new Node<object>(s);
                    p.Last.Value.AddLast(node);

                    if (operation > i)
                    {
                        bool isNegativeSign = s == "-" && node.Previous != null && node.Previous.Value is string && operations.Contains(node.Previous.Value.ToString()).ToBool();

                        if (!isNegativeSign)
                        {
                            p.Last.Value.operations.TryGet(s).Push(node);
                        }
                        i = operation - 1;
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
                        //checkForJuxtapose(node);
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
