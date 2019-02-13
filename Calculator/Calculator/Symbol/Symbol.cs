using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Calculator;
using System.Collections.ObjectModel;

namespace Graphics
{
    public abstract class Symbol
    {
        public object view;
        public Layout Parent;

        public bool HasParent
        {
            get
            {
                return Parent != null;
            }
        }

        public int Index
        {
            get
            {
                return Parent.Children.IndexOf(this);
            }
        }

        public Symbol Next
        {
            get
            {
                return Parent.Children[Parent.Children.IndexOf(this) + 1];
            }
        }

        public Symbol Previous
        {
            get
            {
                return Parent.Children[Parent.Children.IndexOf(this) - 1];
            }
        }

        public void AddAfter(Symbol symbol)
        {
            symbol.Parent.Add(this, symbol.Parent.Children.IndexOf(symbol) + 1);
        }

        public void AddBefore(Symbol symbol)
        {
            symbol.Parent.Add(this, symbol.Parent.Children.IndexOf(symbol));
        }

        public void Remove()
        {
            Parent.Remove(this);
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

    public class Cursor : Symbol
    {
        /*new public Symbol Next;
        new public Symbol Previous;

        new public void AddBefore(Symbol symbol)
        {
            Previous = symbol.Previous;
            Next = symbol;
        }
        
        new public void AddAfter(Symbol symbol)
        {
            Previous = symbol;
            Next = symbol.Next;
        }*/
    }

    public class Text : Symbol
    {
        public string text;
        public bool selectable = false;

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
        public ReadOnlyCollection<Symbol> Children
        {
            get
            {
                return new ReadOnlyCollection<Symbol>(children);
            }
        }

        private List<Symbol> children = new List<Symbol>();

        public void Add(Symbol symbol)
        {
            Add(symbol, children.Count);
        }

        public void Add(Symbol symbol, int index)
        {
            //Remove from old list
            if (symbol.Parent != null)
            {
                symbol.Parent.children.Remove(symbol);
            }
            children.Insert(index, symbol);
            symbol.Parent = this;
        }

        public void Remove(Symbol symbol)
        {
            children.Remove(symbol);
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
                var temp = Cruncher.Evaluate(layout.Children.ToList());
                if (temp is Crunch.Fraction)
                {
                    return (temp as Crunch.Fraction).Simplify();
                }
                else
                {
                    return temp;
                }
            }
            catch
            {
                return null;
            }
        }
    }

    public class Fraction : Layout
    {
        private Func<Symbol, Symbol> previous = delegate (Symbol node) { return node.Previous; };
        private Func<Symbol, Symbol> next = delegate (Symbol node) { return node.Next; };

        public Fraction(Expression Numerator, Expression Denominator)
        {
            Add(Numerator);
            Add(Denominator);
        }

        public Fraction()
        {
            GraphicsEngine.Creator.Enqueue(delegate (Symbol text)
            {
                print.log(text.Parent == null);
                Add(new Expression(GetQuantity(text, false)));
                Add(new Expression(GetQuantity(text, true)));
            });
        }

        public List<Symbol> GetQuantity(Symbol start, bool forward)
        {
            List<Symbol> result = new List<Symbol>();
            List<Symbol> list = start.Parent.Children.ToList();
            if (!forward)
            {
                list.Reverse();
            }

            int index = list.IndexOf(start);
            while (index + 1 < list.Count && (list[index + 1] is Number || list[index + 1] is Cursor))
            {
                result.Add(list[++index]);
                if (result.Last() is Cursor)
                {
                    break;
                }
            }

            if (!forward)
            {
                list.Reverse();
            }
            return result;

            Func<Symbol, bool> condition = null;
            Func<Symbol, Symbol> get = null;

            if (get(start) != null)
            {
                start = get(start);

                if (start == Input.selected.cursor)
                {
                    result.Add(start);
                    return result;
                }

                if (get == previous && IsExpressionEnd(start))
                {
                    condition = IsExpressionStart;
                }
                else if (get == next && IsExpressionStart(start))
                {
                    condition = IsExpressionEnd;
                }
                else if (!IsNotNumber(start))
                {
                    condition = IsNotNumber;
                }

                do
                {
                    var temp = get(start);
                    result.Insert(0, start);
                    start = temp;
                }
                while (start != null && !condition(start));
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

        public bool IsNumber(Symbol symbol)
        {
            return symbol is Number;
        }

        public bool IsNotNumber(Symbol symbol)
        {
            return !(symbol is Number);
        }

        public static implicit operator Crunch.Fraction(Fraction f)
        {
            return new Crunch.Fraction(f.Children.First() as dynamic, f.Children.Last() as dynamic);
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