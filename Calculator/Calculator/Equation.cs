using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculator
{
    public class Equation
    {
        public static Dictionary<object, Equation> all = new Dictionary<object, Equation>();
        public static object canvas;

        public List<Symbol> text1 = new List<Symbol>() { };
        public MathView mathView = new MathView();
        public MathText text = new MathText();
        public GraphicsEngine graphicsEngine = new GraphicsEngine();

        public Text equals = new Text("=");
        public object root;
        public object layout;
        public int pos;

        public CursorListener cursorListener;

        public Equation()
        {
            root = Input.graphicsHandler.AddLayout(new Format());
            layout = Input.graphicsHandler.AddEditField();

            cursorListener = new CursorListener(SetCursor);

            Input.graphicsHandler.AddChild(canvas, layout, 0);
            Input.graphicsHandler.AddChild(layout, Input.phantomCursor, 0);
            Input.graphicsHandler.AddChild(layout, root, 0);
            Input.graphicsHandler.AddChild(root, Input.graphicsHandler.AddCursor(), 0);

            mathView.root = root;

            //Input.graphicsHandler.AddChild(root, left.mathView.root = Input.graphicsHandler.AddLayout(new Format()), 0);
            //Input.graphicsHandler.AddChild(root, Input.graphicsHandler.AddText("="), 0);
            //Input.graphicsHandler.AddChild(root, right.mathView.root = Input.graphicsHandler.AddLayout(new Format()), 0);
        }

        public void Insert(object sender)
        {
            insert(sender as dynamic);
            //SetCursor(pos);

            print.log("************************");
            foreach (Symbol s in text)
                print.log(s.text);
            print.log("end");

            List<Symbol> parsed = text.Parse();

            //Crunch.Evaluate(new Expression(parsed));

            //mathView.SetText(parsed);
            graphicsEngine.SetText(parsed);
        }

        private void insert(Symbol sender)
        {
            text.Insert(pos, sender);

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
                        text.Insert(pos, new Text(")"));
                        text.Insert(GetNumber(text[pos - 1] as Number)[0], new Text("("));
                    }
                    else if (text[pos - 1].text != ")")
                    {
                        //parend(text[pos - 1]);
                        //parend(pos - 1);
                    }

                    //text.Insert(text.IndexOf(text.findMatching(text[text.IndexOf(sender) + 1] as Text)) + 1, new Text(")"));
                    //text.Insert(text.IndexOf(text.findMatching(text[text.IndexOf(sender) - 1] as Text)), new Text("("));
                    text.Insert(findMatching(text[text.IndexOf(sender) + 1] as Text, text) + 1, new Text(")"));
                    text.Insert(findMatching(text[text.IndexOf(sender) - 1] as Text, text), new Text("("));

                    //SetCursor(text.IndexOf(sender) + 2);
                    pos = text.IndexOf(sender) + 2;

                    break;
                case "(":
                    text.Insert(pos + 1, new Text(")"));
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
            print.log(mathView.shown[sender].text);

            int index = 0;

            text.Remove(Symbol.Cursor);
            text.Insert(index, Symbol.Cursor);
            pos = text.IndexOf(Symbol.Cursor);
            print.log("KKKKKKKKKKKKK " + pos);
            //pos 
        }

        public static int findMatching(Text first, List<Symbol> inList)
        {
            int dir = 1;
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
            text.Insert(Math.Min(index + isNull, text.Count), new Text(")"));
            text.Insert(Math.Max(index, 0), new Text("("));
        }

        private void parend(Symbol s)
        {
            parend(text.IndexOf(s), 1);
        }
    }
}
