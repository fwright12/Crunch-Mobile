using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Calculator;

namespace Graphics
{
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
    }*/

    public abstract class Symbol
    {
        public object view;

        public static implicit operator Android.Views.View(Symbol s)
        {
            return s.view as Android.Views.View;
        }

        public static implicit operator Symbol(Crunch.Term t)
        {
            return new Text(t.value.ToString());
        }
    }

    /*public class Text : Symbol
    {
        public string text;

        public Text(string text)
        {
            this.text = text;
        }

        public static implicit operator Android.Widget.TextView(Text t)
        {
            return t.view as Android.Widget.TextView;
        }
    }*/

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

        public static implicit operator Number(Crunch.Number n)
        {
            return new Number(n.value.ToString());
        }
    }

    public class Cursor : Symbol { }

    /*public class Layout : Symbol
    {
        public static implicit operator Android.Widget.LinearLayout(Layout l)
        {
            return l.view as Android.Widget.LinearLayout;
        }

        public static implicit operator Crunch.Term(Layout e)
        {
            return new Crunch.Number(1);
        }
    }*/

    public class Layout : Symbol
    {
        public LinkedList<Symbol> children = new LinkedList<Symbol>();

        public Layout(params LinkedListNode<Symbol>[] Children)
        {
            foreach(LinkedListNode<Symbol> s in Children)
            {
                children.AddLast(s);
            }
        }

        public Layout(LinkedList<Symbol> Children)
        {
            children = Children;
        }

        public static implicit operator Crunch.Term(Layout layout)
        {
            try
            {
                return Cruncher.Evaluate(layout.children.ToList());
            }
            catch
            {
                return null;
            }
        }
    }

    public class Bar : Symbol { }

    public class Space : Symbol { }

    public class Fraction : Layout
    {
        public Fraction(Layout Numerator, Layout Denominator)
        {
            children.AddLast(Numerator);
            children.AddLast(new Bar());
            children.AddLast(Denominator);
        }

        public Fraction(LinkedListNode<Symbol> text)
        {
            GraphicsEngine.Creator.Enqueue(delegate ()
            {
                print.log("SDFLKJSDKLJ: " + text.Value +", " + text.Previous.Value +", "+ text.Next.Value);
                children.AddLast(new Layout(GetBefore(text.Previous)));
                children.AddLast(new Bar());
                children.AddLast(new Layout(new LinkedListNode<Symbol>(new Space()), Input.select.cursor.GetNode()));
            });
        }

        //public List<IDisplayable> GetWhole(LinkedListNode<Text> start, Func<Text, bool> condition, Func<LinkedListNode<Text>> direction)
        //{

        //}

        public LinkedList<Symbol> GetBefore(LinkedListNode<Symbol> start)
        {
            LinkedList<Symbol> result = new LinkedList<Symbol>();
            Func<Symbol, bool> condition = null;

            if (CheckForExpressionEnd(start.Value))
            {
                condition = CheckForExpressionStart;
            }
            else if (CheckForNumber(start.Value))
            {
                condition = CheckForNumber;
            }

            do
            {
                var temp = start.Previous;
                result.AddFirst(start.GetNode());
                start = temp;
            }
            while (condition(start.Value));

            return result;
        }

        public bool CheckForExpressionStart(Symbol symbol)
        {
            return symbol is Text && (symbol as Text).text == "(";
        }

        public bool CheckForExpressionEnd(Symbol symbol)
        {
            return symbol is Text && (symbol as Text).text == ")";
        }

        public bool CheckForNumber(Symbol symbol)
        {
            return symbol is Number;
        }

        public static implicit operator Crunch.Fraction(Fraction f)
        {
            return new Crunch.Fraction(new Crunch.Number(1));
            //return new Fraction(Evaluate(f.numerator)))
        }
        
        public static implicit operator Fraction(Crunch.Fraction f)
        {
            return new Fraction(new Layout(new LinkedListNode<Symbol>(f.numerator as dynamic)), new Layout(new LinkedListNode<Symbol>(f.denominator as dynamic)));
        }
    }

    /*public abstract class Group : Symbol
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
namespace Calculatora
{
    public partial class Symbol
    {
        public static readonly Symbol Cursor = new Symbol();

        public virtual string text
        {
            get
            {
                return _text;
            }
        }
        private string _text;

        //public Format format = new Format();

        public Symbol() : this("") { }

        public Symbol (string Text)
        {
            _text = Text;
        }

        public virtual Symbol Copy()
        {
            return new Symbol(text);
        }

        public virtual List<Symbol> GetText()
        {
            return new List<Symbol>() { this };
        }
    }

    public class Answer : Symbol
    {
        private Symbol symbol;

        public Answer(Symbol sender)
        {
            symbol = sender;
        }

        public override List<Symbol> GetText()
        {
            return new List<Symbol>() { symbol };
        }
    }

    public class Operand : Symbol
    {
        public Operand(string s) : base(s) { }
    }

    public class Space : Symbol
    {
        public Symbol reference;

        public Space() { }

        public Space(Symbol Reference)
        {
            reference = Reference;
        }
    }

    public class Bar : Symbol { }
}