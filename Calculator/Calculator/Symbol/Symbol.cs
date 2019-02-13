using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Calculator;


namespace Graphics
{
    public abstract class Symbol
    {
        public object view;
        public Layout Parent;

        public LinkedListNode<Symbol> Next
        {
            get
            {
                return Parent.Children.Find(this).Next;
            }
        }

        public LinkedListNode<Symbol> Previous
        {
            get
            {
                return Parent.Children.Find(this).Previous;
            }
        }

        public LinkedListNode<Symbol> Node
        {
            get
            {
                return Parent.Children.Find(this);
            }
        }

        public static implicit operator Symbol(Crunch.Number n)
        {
            return new Number(n.value.ToString());
        }

        public static implicit operator Symbol(Crunch.Fraction f)
        {
            return new Fraction(new Expression(f.numerator as dynamic), new Expression(f.denominator as dynamic));
        }

        /*public static implicit operator Symbol(Crunch.Term t)
        {
            return new Number(t.value.ToString());
        }*/
    }

    public class Cursor : Symbol { }
    public class Bar : Symbol { }
    public class Space : Symbol { }

    public class Text : Symbol
    {
        public string text;

        public Text(string str)
        {
            text = str;
        }

        public bool IsOperand()
        {
            return text == "+" || text == "*" || text == "-";
        }
    }

    public class Number : Text
    {
        public Number(string text) : base(text) { }

        public static implicit operator Crunch.Number(Number n)
        {
            return new Crunch.Number(double.Parse(n.text));
        }
    }

    public abstract class Layout : Symbol
    {
        public LinkedList<Symbol> Children = new LinkedList<Symbol>();
        public LinkedList<Symbol> List;

        public void Add(Symbol symbol)
        {
            Add(symbol, Children.Count - 1);
        }

        public void Add(Symbol symbol, int index)
        {

            Children.AddAfter(Children.NodeAt(index).DetachNode(), symbol);
            Parent = this;
        }
    }

    public class Expression : Layout
    {
        public Expression(List<Symbol> children)
        {
            foreach(Symbol s in children)
            {
                Add(s);
            }
        }

        public Expression(params Symbol[] children) : this(children.ToList()) { }

        public static implicit operator Crunch.Term(Expression layout)
        {
            try
            {
                return Cruncher.Evaluate(layout.Children.ToList());
            }
            catch
            {
                return null;
            }
        }
    }

    public class Fraction : Layout
    {
        private Func<LinkedListNode<Symbol>, LinkedListNode<Symbol>> previous = delegate (LinkedListNode<Symbol> node) { return node.Previous; };
        private Func<LinkedListNode<Symbol>, LinkedListNode<Symbol>> next = delegate (LinkedListNode<Symbol> node) { return node.Next; };

        public Fraction(Expression Numerator, Expression Denominator)
        {
            Children.AddLast(Numerator);
            Children.AddLast(new Bar());
            Children.AddLast(Denominator);
        }

        public Fraction(LinkedListNode<Symbol> text)
        {
            GraphicsEngine.Creator.Enqueue(delegate ()
            {
                //print.log("SDFLKJSDKLJ: " + text.Value +", " + text.Previous.Value +", "+ text.Next.Value);
                Children.AddLast(new Expression(GetQuantity(previous, text).ToList()));
                Children.AddLast(new Bar());
                //children.AddLast(new Layout(new LinkedListNode<Symbol>(new Space()), Input.selected.cursor.GetNode()));
                Children.AddLast(new Expression(GetQuantity(next, text).ToList()));
            });
        }

        public LinkedList<Symbol> GetQuantity(Func<LinkedListNode<Symbol>, LinkedListNode<Symbol>> get, LinkedListNode<Symbol> start)
        {
            LinkedList<Symbol> result = new LinkedList<Symbol>();
            Func<Symbol, bool> condition = null;

            if (get(start) != null)
            {
                start = get(start);

                if (start.Value == Input.selected.cursor)
                {
                    result.AddFirst(start.DetachNode());
                    return result;
                }

                if (get == previous && IsExpressionEnd(start.Value))
                {
                    condition = IsExpressionStart;
                }
                else if (get == next && IsExpressionStart(start.Value))
                {
                    condition = IsExpressionEnd;
                }
                else if (!IsNotNumber(start.Value))
                {
                    condition = IsNotNumber;
                }

                do
                {
                    var temp = get(start);
                    result.AddFirst(start.DetachNode());
                    start = temp;
                }
                while (start != null && !condition(start.Value));
            }

            return result;
        }

        public bool IsExpressionStart(Symbol symbol)
        {
            return symbol is Text && (symbol as Text).text == "(";
        }

        public bool IsExpressionEnd(Symbol symbol)
        {
            return symbol is Text && (symbol as Text).text == ")";
        }

        public bool IsNotNumber(Symbol symbol)
        {
            return !(symbol is Number);
        }

        public static implicit operator Crunch.Fraction(Fraction f)
        {
            return new Crunch.Fraction(f.Children.First.Value as dynamic, f.Children.Last.Value as dynamic);
        }
    }

    /*public partial class Symbol
    {
        public Symbol Left;
        public Symbol Right;

        public void HookLeft(Symbol node)
        {
            Left = node;
            node.Right = this;
        }

        public void HookRight(Symbol node)
        {
            Right = node;
            node.Left = this;
        }

        public void AddBefore(Symbol node)
        {
            HookLeft(node.Left);
            HookRight(node);
        }

        public void AddAfter(Symbol node)
        {
            HookRight(node.Right);
            HookLeft(node);
        }

        public void AddFirst(Symbol node)
        {
            node.Left = null;
        }
    }

    public abstract class Group : Symbol
    {
        public Symbol First;
        public Symbol Last;

        public new Symbol Left
        {
            get
            {
                return base.Left;
            }
            set
            {
                base.Left = value;
                if (value == null)
                {
                    First = this;
                }
            }
        }

        public new Symbol Right
        {
            get
            {
                return base.Right;
            }
            set
            {
                base.Right = value;
                if (value == null)
                {
                    Last = this;
                }
            }
        }

        public Group(string text) : base(text) { }
    }*/
}