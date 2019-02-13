using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Graphics;

namespace Calculator
{
    /*public class Str
    {
        public Symbol symbol;

        public string text;

        public Str(string text)
        {
            this.text = text;
        }

        public static implicit operator string(Str t)
        {
            return t.text;
        }

        public static implicit operator Str(string s)
        {
            return new Str(s);
        }
    }*/

    /*public class Equation
    {
        public static Dictionary<object, Equation> all = new Dictionary<object, Equation>();
        //public static object canvas;

        public List<Symbol> text1 = new List<Symbol>() { };
        public MathView mathView = new MathView();
        public MathText text = new MathText();
        public GraphicsEngine graphicsEngine;// = new GraphicsEngine();
        public List<Symbol> parsed = new List<Symbol>();

        //public Symbol equalsSign = new Symbol("=");
        public object root;
        public int pos;
        public bool displayAnswerAsFraction = true;

        public CursorListener cursorListener;

        /*public List<Symbol> GetText()
        {
            var temp = new Expression(parsed);
            var answer = temp.answer.Copy();
            if (!displayAnswerAsFraction && answer is Term)
            {
                answer = new Number((answer as Term).value);
            }
            return new List<Symbol>() { temp, equalsSign, new Answer(answer) };
        }

        public Equation()
        {
            //cursor.Value.symbol = new Cursor();
            //texter.AddFirst(cursor);
            layout = new Layout();
            layout.children.AddLast(cursor);

            //root = Input.graphicsHandler.AddLayout(new Format());
            //layout = Input.graphicsHandler.AddEditField();
            //root = Root;

            //graphicsEngine.SetText(new List<Symbol>() { new Expression() });

            /*Input.graphicsHandler.AddChild(canvas, layout, 0);
            Input.graphicsHandler.AddChild(layout, Input.phantomCursor, 0);
            Input.graphicsHandler.AddChild(layout, root, 0);
            Input.graphicsHandler.AddChild(root, Input.graphicsHandler.AddCursor(), 0);

            //mathView.root = root;

            //Input.graphicsHandler.AddChild(root, left.mathView.root = Input.graphicsHandler.AddLayout(new Format()), 0);
            //Input.graphicsHandler.AddChild(root, Input.graphicsHandler.AddText("="), 0);
            //Input.graphicsHandler.AddChild(root, right.mathView.root = Input.graphicsHandler.AddLayout(new Format()), 0);
        }

        public void Insert(object sender)
        {
            insert(sender as dynamic);
            //SetCursor(pos);

            print.log("************************");
            //foreach (Symbol s in text)
            //    print.log(s.text);
            print.log("end");

            parsed = text.Parse();
            //var answer = parsed.Copy(equalsSign, new Expression(parsed).answer.Copy());

            //mathView.SetText(parsed);
            //graphicsEngine.SetText(GetText());
        }

        //public LinkedListNode<Graphics.Symbol> cursor = new LinkedListNode<Graphics.Symbol>(new Graphics.Symbol(""));
        public static LinkedListNode<Str> acursor = new LinkedListNode<Str>("|");
        public LinkedList<Symbol> literalText = new LinkedList<Symbol>();
        public LinkedList<Str> texter = new LinkedList<Str>();

        public List<Symbol> updates = new List<Symbol>();
        public LinkedList<Text> texting = new LinkedList<Text>();

        public static Queue<Action> Creator = new Queue<Action>();
        public Layout layout;
        public static LinkedListNode<Symbol> cursor = new LinkedListNode<Symbol>(new Cursor());

        public void Wrapper(string text)
        {
            //LinkedListNode<Str> node = texter.AddBefore(cursor, text);
            //Symbol symbol = default(Symbol);
            LinkedListNode<Symbol> node = new LinkedListNode<Symbol>(default(Symbol));

            switch (text)
            {
                case "/":
                    node.Value = new Fraction(node);
                    break;
                default:
                    if (Input.IsNumber(text))
                    {
                        node.Value = new Number(text);
                    }
                    else
                    {
                        node.Value = new Text(text);
                    }
                    break;
            }

            cursor.List.AddBefore(cursor, node);

            while (Creator.Count > 0)
            {
                Creator.Dequeue()();
            }

            //graphicsEngine.Add(cursor.Value, node.Value);

            print.log("End");
            foreach(Symbol s in layout.children)
            {
                print.log(s);
            }
            answer(layout);
        }

        private void answer(Crunch.Term sender)
        {
            print.log(sender.value);
        }

        /*private void insert(int start = 0, params string[] text)
        {
            foreach (string str in text)
            {
                texting.Add(str);

                switch (str)
                {
                    case "/":
                        if (cursor.Previous.Value == ")")
                        {

                        }
                        else
                        {

                        }
                        break;
                    default:
                        if (Input.IsNumber(str))
                        {
                            texter.AddBefore(cursor, new Number(str));
                        }
                        else
                        {

                        }

                        break;
                }
            }
        }*/

        /*private void inserta(Symbol sender)
        {
            literalText.AddBefore(cursor, sender);

            switch (sender.text)
            {
                case "/":
                    if (cursor.Value.Left is Expression)
                    {

                    }
                    else
                    {
                        Symbol first = cursor.Value;
                        while (first.Left is Number)
                        {
                            first = first.Left;
                        }
                        sender.AddFirst(first);
                        cursor.Value.add
                    }

                    break;
                default:
                    /*Graphics.Symbol symbol;

                    if (Input.IsNumber(text))
                    {
                        /*if (cursor.Next.Value is Number)
                        {
                            cursor.Next.Value.AddFirst(adding);
                        }
                        else if (cursor.Previous.Value is Number)
                        {
                            cursor.Previous.Value.AddLast(adding);
                        }
                        else
                        {
                            texter.AddBefore(cursor, new Graphics.Number());
                        }
                        new Number(text);
                    }
                    else
                    {
                        symbol = new Symbol(text);
                    }

                    sender.AddBefore(cursor.Value);
                    
                    break;
            }
        }

        private void insert(Symbol sender)
        {
            /*text.Insert(pos, sender);

            switch (sender.text)
            {
                case "^":
                case "/":
                    //By default, null parend the denominator
                    //if (pos + 1 > text.Count - 1 || text[pos + 1].text != "(")
                    //{
                        parend(pos + 1);
                    //}

                    //If there's nothing before, null parend the numerator
                    if (pos - 1 < 0)
                    {
                        parend(0);
                    }
                    //Otherwise if it's a number, make sure to get the whole thing
                    else if (text[pos - 1] is Number)
                    {
                        text.Insert(pos, new Symbol(")"));
                        text.Insert(GetNumber(text[pos - 1] as Number)[0], new Symbol("("));
                    }
                    else if (text[pos - 1].text != ")")
                    {
                        //parend(text[pos - 1]);
                        //parend(pos - 1);
                    }

                    //text.Insert(text.IndexOf(text.findMatching(text[text.IndexOf(sender) + 1] as Text)) + 1, new Text(")"));
                    //text.Insert(text.IndexOf(text.findMatching(text[text.IndexOf(sender) - 1] as Text)), new Text("("));
                    text.Insert(findMatching(text[text.IndexOf(sender) + 1] as Symbol, text) + 1, new Symbol(")"));
                    text.Insert(findMatching(text[text.IndexOf(sender) - 1] as Symbol, text), new Symbol("("));

                    //SetCursor(text.IndexOf(sender) + 2);
                    pos = text.IndexOf(sender) + 2;

                    break;
                case "(":
                    text.Insert(pos + 1, new Symbol(")"));
                    pos--;
                    break;
                default:
                    pos++;
                    break;
            }

            //pos = text.IndexOf(Symbol.Cursor);
            //pos++;
        }

        public void SetCursor(object sender)
        {
            int index = 0;

            //text.Remove(Symbol.Cursor);
            //text.Insert(index, Symbol.Cursor);
            //pos = text.IndexOf(Symbol.Cursor);
            print.log("KKKKKKKKKKKKK " + pos);
            //pos 
        }

        public static int findMatching(Symbol first, List<Symbol> inList)
        {
            /*int dir = 1;
            if (first.text == ")")
                dir = -1;

            int count = 0;

            int index;
            for (index = inList.IndexOf(first) + dir; index < inList.Count && index > -1; index += dir)
            {
                if ((dir == 1 && inList[index].text == "(") || (dir == -1 && inList[index].text == ")"))
                {
                    count++;
                }
                if ((dir == -1 && inList[index].text == "(") || (dir == 1 && inList[index].text == ")"))
                {
                    if (count == 0)
                        break;

                    count--;
                }
            }

            return index;
            return 1;
        }

        private int[] GetNumber(Number sender)
        {
            int first = text.IndexOf(sender);
            while (--first > -1 && text[first] is Number) { }

            int last = text.IndexOf(sender);
            while (++last < text.Count && text[last] is Number) { }

            return new int[2] { first + 1, last - 1};
        }

        //Add parentheses around symbol at 'index'
        private void parend(int index, int isNull = 0)
        {
            //text.Insert(Math.Min(index + isNull, text.Count), new Symbol(")"));
            //text.Insert(Math.Max(index, 0), new Symbol("("));
        }

        private void parend(Symbol s)
        {
            parend(text.IndexOf(s), 1);
        }
    }*/
}
