using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Graphics;

namespace Calculator
{
    public class GraphicsEngine
    {
        public static RenderFactory renderFactory = new RenderFactory();
        public static Queue<Action> Creator = new Queue<Action>();
        public static object canvas;
        public static object phantomCursor;

        public static int cursorWidth;
        public static int textHeight;
        public static int textWidth;
        public static int TextSize = 40;

        public BiDictionary<Symbol, object> viewLookup = new BiDictionary<Symbol, object>();
        public Symbol cursor = new Cursor();
        public object cursorObject
        {
            get
            {
                return viewLookup[cursor];
            }
        }

        public Expression root;
        public object rootObject
        {
            get
            {
                return viewLookup[root];
            }
        }
        //public Symbol answer;

        public GraphicsEngine(object relativeLayout, bool hasFocus = true)
        {
            if (hasFocus)
            {
                Input.AddEquation(this);
            }

            print.log("initializing...");
            //Add relative layout to main canvas
            renderFactory.Add(canvas as dynamic, relativeLayout as dynamic);
            //renderFactory.Add(relativeLayout as dynamic, phantomCursor as dynamic);
            //(phantomCursor as Android.Views.View).Visibility = Android.Views.ViewStates.Visible;

            //Add layout for layout, equal sign, and answer to relative layout
            var LayoutEqualsAnswer = renderFactory.BaseLayout();
            renderFactory.Add(relativeLayout as dynamic, LayoutEqualsAnswer);

            //Set up main layout where input will display
            root = new Expression(cursor);
            SetText(LayoutEqualsAnswer, root);

            //print.log(renderFactory.GetParent(rootObject as dynamic).Equals(LayoutEqualsAnswer));

            //Add equal sign
            renderFactory.Add(LayoutEqualsAnswer, renderFactory.Create(new Text("=")), 1);
            //SetText(LayoutEqualsAnswer, new Text("error"), 2);

            //Create answer layout
            /*var right = renderFactory.BaseLayout();
            renderFactory.Add(LayoutEqualsAnswer, right as dynamic, 2);
            viewLookup.Add(answer, right);*/

            //var rootLayout = renderFactory.BaseLayout();
            //renderFactory.Add(layout, root);

            //SetText(root, Equation.cursor.Value);
        }

        public void SetText(object parent, Symbol symbol, int index = 0)
        {
            print.log("set text " + symbol +", "+ (symbol is Expression));
            var view = default(object);

            if (viewLookup.Contains(symbol))
            {
                view = viewLookup[symbol];
            }
            else
            {
                view = renderFactory.Create(symbol as dynamic);
                viewLookup.Add(symbol, view);
            }

            var viewParent = renderFactory.GetParent(view as dynamic);
            if (viewParent == null || !viewParent.Equals(parent as dynamic))
            {
                //print.log("adding");
                renderFactory.Add(parent as dynamic, view as dynamic, index);
            }

            if (symbol is Layout)
            {
                Layout layout = symbol as Layout;

                for (int i = 0; i < layout.Children.Count; i++)
                {
                    SetText(view, layout.Children.ElementAt(i), i);
                }
            }
        }

        public void CheckPadding(object parent)
        {
            Expression temp = viewLookup[parent] as Expression;
            renderFactory.SetPadding(
                parent as dynamic,
                (temp.Children.First.Value is Layout && temp.Children.First.Value.GetType() != typeof(Expression)).ToInt() * cursorWidth,
                (temp.Children.Last.Value is Layout && temp.Children.Last.Value.GetType() != typeof(Expression)).ToInt() * cursorWidth
                );
        }

        private object lastParent;
        private object parent;

        public void MoveCursor(object view, bool isPaddedLeft, bool isPaddedRight, int leftOrRight)
        {
            Symbol symbol = Input.selected.viewLookup[view];
            parent = renderFactory.GetParent(view as dynamic);

            if (symbol is Text)
            {
                //SetText(parent, symbol, )
            }
            else if (symbol is Expression)
            {

            }

            if (leftOrRight == 0)
            {
                layout.Children.AddBefore(layout.Children.Find(symbol), symbolCursor);
            }
            else if (leftOrRight == 1)
            {
                layout.Children.AddAfter(layout.Children.Find(symbol), symbolCursor);
            }

            renderFactory.Add(parent as dynamic, cursorObject as dynamic, renderFactory.GetIndex(view as dynamic) + leftOrRight);

            SetText(parent, symbol);

            if (symbol is Text || symbol.GetType() == typeof(Expression))
            {

                var symbolCursor = cursor;

                if (symbol is Expression && (symbol as Expression).Children.Count == 0)
                {
                    (symbol as Expression).Children.AddFirst(symbolCursor);
                }
                else
                {
                    Expression layout = default(Expression);

                    if (symbol is Expression)
                    {
                        layout = symbol as Expression;

                        if (isPaddedLeft)
                        {
                            leftOrRight = 0;
                            symbol = layout.Children.First.Value;
                        }
                        else if (isPaddedRight)
                        {
                            leftOrRight = 1;
                            symbol = layout.Children.Last.Value;
                        }

                        parent = view;// viewLookup[layout];
                        view = viewLookup[symbol];

                        //renderFactory.Add(temp as dynamic, cursorObject as dynamic, renderFactory.GetIndex(viewLookup[symbol] as dynamic) + leftOrRight);
                        //temp.AddView(realCursor, temp.IndexOfChild(Input.selected.viewLookup[symbol] as View) + leftOrRight);
                    }
                    else
                    {
                        layout = viewLookup[parent] as Expression;
                        //parent.AddView(realCursor, parent.IndexOfChild(view) + leftOrRight);
                    }

                    if (leftOrRight == 0)
                    {
                        layout.Children.AddBefore(layout.Children.Find(symbol), symbolCursor);
                    }
                    else if (leftOrRight == 1)
                    {
                        layout.Children.AddAfter(layout.Children.Find(symbol), symbolCursor);
                    }

                    renderFactory.Add(parent as dynamic, cursorObject as dynamic, renderFactory.GetIndex(view as dynamic) + leftOrRight);
                }
            }
        }

        /*public void MoveCursor(object view, bool isPaddedLeft, bool isPaddedRight, int leftOrRight)
        {
            if (!viewLookup.Contains(view))
            {
                return;
            }

            Symbol symbol = Input.selected.viewLookup[view];

            if (symbol is Text || symbol.GetType() == typeof(Expression))
            {
                lastParent = parent;
                parent = renderFactory.GetParent(view as dynamic);

                renderFactory.Remove(cursorObject as dynamic);
                var symbolCursor = cursor.DetachNode();

                if (symbol is Expression && (symbol as Expression).Children.Count == 0)
                {
                    //parent.AddView(realCursor, parent.IndexOfChild(Symbol) + leftOrRight);
                    renderFactory.Add(view as dynamic, cursorObject as dynamic);
                    (symbol as Expression).Children.AddFirst(symbolCursor);
                    parent = renderFactory.GetParent(view as dynamic);
                }
                else
                {
                    Expression layout = default(Expression);

                    if (symbol is Expression)
                    {
                        layout = symbol as Expression;

                        if (isPaddedLeft)
                        {
                            leftOrRight = 0;
                            symbol = layout.Children.First.Value;
                        }
                        else if (isPaddedRight)
                        {
                            leftOrRight = 1;
                            symbol = layout.Children.Last.Value;
                        }

                        parent = view;// viewLookup[layout];
                        view = viewLookup[symbol];

                        //renderFactory.Add(temp as dynamic, cursorObject as dynamic, renderFactory.GetIndex(viewLookup[symbol] as dynamic) + leftOrRight);
                        //temp.AddView(realCursor, temp.IndexOfChild(Input.selected.viewLookup[symbol] as View) + leftOrRight);
                    }
                    else
                    {
                        layout = viewLookup[parent] as Expression;
                        //parent.AddView(realCursor, parent.IndexOfChild(view) + leftOrRight);
                    }

                    if (leftOrRight == 0)
                    {
                        layout.Children.AddBefore(layout.Children.Find(symbol), symbolCursor);
                    }
                    else if (leftOrRight == 1)
                    {
                        layout.Children.AddAfter(layout.Children.Find(symbol), symbolCursor);
                    }

                    renderFactory.Add(parent as dynamic, cursorObject as dynamic, renderFactory.GetIndex(view as dynamic) + leftOrRight);
                }
            }
        }*/

        public void Wrapper(params string[] text)
        {
            LinkedListNode<Symbol> node = null;

            foreach (string str in text)
            {
                node = new LinkedListNode<Symbol>(default(Symbol));

                switch (str)
                {
                    case "/":
                        node.Value = new Fraction(node);
                        break;
                    default:
                        if (Input.IsNumber(str))
                        {
                            node.Value = new Number(str);
                        }
                        else
                        {
                            node.Value = new Text(str);
                        }
                        break;
                }

                cursor.Parent.Children.AddBefore(cursor.Node, node);
            }

            while (Creator.Count > 0)
            {
                Creator.Dequeue()();
            }

            SetText(renderFactory.GetParent(cursorObject as dynamic) as dynamic, node.Value, renderFactory.GetIndex(viewLookup[cursor.Value] as dynamic));

            /*if (text.Length > 1)
            {
                SetText(renderFactory.GetParent(rootObject as dynamic), root);
            }
            else
            {
                SetText(renderFactory.GetParent(cursorObject as dynamic) as dynamic, node.Value, renderFactory.GetIndex(viewLookup[cursor.Value] as dynamic));
            }*/

            SetAnswer(root);
        }

        //private object answer;

        public void SetAnswer(Crunch.Term calculated)
        {
            print.log("SDJFLSDJL " + calculated);

            try
            {
                //renderFactory.Remove(answer as dynamic);
                ((rootObject as Android.Views.View).Parent as Android.Views.ViewGroup).RemoveViewAt(2);
            }
            catch { }
            finally
            {
                Symbol answer = default(Symbol);

                if (calculated == null)
                {
                    answer = new Text("error");
                }
                else
                {
                    answer = calculated as dynamic;
                }

                //renderFactory.GetParent(rootObject as dynamic).SetBackgroundColor(Android.Graphics.Color.Red);
                //renderFactory.Add(renderFactory.GetParent(rootObject as dynamic) as dynamic, answer as dynamic, 2);
                if (calculated != null)
                SetText(renderFactory.GetParent(rootObject as dynamic) as dynamic, answer, 2);
            }

            //print.log("LSKDJFLKSJDFLK " + answer.value +", "+ (answer as dynamic).GetType());
            //answer = calculated;
            //renderFactory.Remove()
            //SetText(right, answer);
        }
    }

    /*public partial class GraphicsEngine
    {
        public GraphicsEngine<Android.Views.View, Android.Views.ViewGroup> AndroidGraphics = new GraphicsEngine<Android.Views.View, Android.Views.ViewGroup>();
        //public GraphicsEngine<Xamarin.Forms.View, Xamarin.Forms.Layout> XamarinGraphics = new GraphicsEngine<Xamarin.Forms.View, Xamarin.Forms.Layout>();

        public void Add(Symbol before, Symbol s)
        {
            AndroidGraphics.Add(before, s);
        }

        public void SetText(List<Symbol> list)
        {
            //AndroidGraphics.Start(list);
        }
    }

    public class GraphicsEngine<TView, TLayout> where TLayout : TView
    {
        public static IRenderFactory<TView, TLayout> RenderFactory;
        //public static TLayout canvas;

        public TLayout root;
        public BiDictionary<Symbol, TView> viewLookup = new BiDictionary<Symbol, TView>();
        //public Dictionary<string, TView> views = new Dictionary<LinkedListNode<string>, TView>();

        public void Initialize(TLayout layout)
        {
            print.log("initializing...");
            //RenderFactory.Add(canvas, layout);

            root = RenderFactory.BaseLayout();
            RenderFactory.Add(layout, root);

            SetText(root, Equation.cursor.Value);

            //var temp = new Expression();
            //Start(new List<Symbol>() { temp });
            //RenderFactory.Add((TLayout)viewLookup[temp], (TView)(Input.cursor = RenderFactory.Cursor()));
        }

        public void Add(Symbol before, Symbol s)
        {
            SetText(RenderFactory.GetParent(viewLookup[before]), s, RenderFactory.GetIndex(viewLookup[before]));
        }

        public void SetText(TLayout parent, Symbol symbol, int index = 0)
        {
            TView view = default(TView);

            if (viewLookup.Contains(symbol))
            {
                view = viewLookup[symbol];
            }
            else
            {
                view = RenderFactory.Create(symbol as dynamic);
                viewLookup.Add(symbol, view);
            }

            var viewParent = RenderFactory.GetParent(view);
            if (viewParent == null || !viewParent.Equals(parent))
            {
                RenderFactory.Add(parent, view, index);
            }

            if (symbol is Layout)
            {
                Layout layout = symbol as Layout;

                for (int i = 0; i < layout.children.Count; i++)
                {
                    SetText((TLayout)view, layout.children.ElementAt(i), i);
                }
            }
        }

        public void Start(params LinkedListNode<Text>[] list)
        {
            //var all = expand(list);

            /*print.log("removing...");
            //Remove all views that are still being shown, but no longer should be
            var temp = new Symbol[viewLookup.value1List.Count];
            viewLookup.value1List.CopyTo(temp);
            foreach (Symbol s in temp)
            {
                if (!all.Contains(s))
                {
                    print.log("removed " + s + ", " + s.text);// +", "+ views[s]);
                    //Input.graphicsHandler.RemoveChild(views[s].parent, views[s].child);
                    RenderFactory.Remove(viewLookup[s]);
                    //shown.Remove(views[s].child);
                    viewLookup.Remove(s);
                }
            }

            //SetText(root, list, false);

            //root = (TLayout)viewLookup[list[0]];
        }

        public void Match(LinkedListNode<string> sender)
        {
            switch (sender.Value)
            {
                case "^":
                    LinkedListNode<string> pointer = sender;
                    if (pointer.Previous.Value == ")")
                    {
                        do
                        {
                            pointer = sender.Previous;
                        }
                        while (pointer.Value != "(");
                    }
                    else
                    {
                        while (Input.IsNumber(pointer.Previous.Value))
                        {
                            pointer = pointer.Previous;
                        }
                    }
                    break;
            }
        }

        /*public void SetTexta(TLayout parent, LinkedListNode<Graphics.Symbol> start)
        {
            int count = 0;

            do
            {
                string text = start.Value.Text;

                if (!viewLookup.Contains(start.Value))
                {
                    var view = RenderFactory.Create(start.Value as dynamic);
                    RenderFactory.Add(parent, view, count++);
                    viewLookup.Add(start.Value, view); //new Graphic(parent, list[i], i)

                    if (text == "{")
                    {
                        SetTexta((TLayout)view, start.Next);
                    }
                }

                start = start.Next;
            }
            while (start != null && start.Value.Text != "}");
        }

        public void SetText(TLayout parent, List<string> list, bool parend)
        {
            for (int i = 0; i < list.Count; i++)
            {
                //print.log(i + ", " + "look here " + list[i].text + ", " + viewLookup.Contains(list[i]) + ", " + list[i].GetHashCode());

                //Symbol symbol = list[i];

                /*if ((list[i].text == "(" || list[i].text == ")") && !parend)
                {
                    all.Remove(list[i]);
                    list.RemoveAt(i--);
                    continue;
                }
                else if (!viewLookup.Contains(symbol))
                {
                    //numberFlag = i > 0 && i < list.Count - 1 && list[i - 1] is Number && list[i + 1] is Number;

                    var view = RenderFactory.Create(symbol as dynamic);
                    RenderFactory.Add(parent, view, i);
                    viewLookup.Add(symbol, view); //new Graphic(parent, list[i], i)

                    print.log("adding new " + list[i]);

                    if (Input.adding.Contains(symbol))
                    {
                        Input.adding.Remove(symbol);
                    }
                }
                else if (!RenderFactory.GetParent(viewLookup[symbol]).Equals(parent))
                {
                    //views[list[i]].SetParent(i, parent);
                    RenderFactory.Add(parent, viewLookup[symbol], i);
                }

                if (symbol.GetText().Count > 0 && symbol != symbol.GetText()[0])
                {
                    var temp = viewLookup[symbol];
                    viewLookup.Remove(symbol);

                    bool canParend = false;// list[i].GetType() == typeof(Expression) && list[i].format == null && sender.GetType() == typeof(Expression);

                    SetText((TLayout)temp, symbol.GetText(), canParend);
                    /*if (symbol is Expression)
                    {
                        //print.log(viewLookup[list[list.Count - 1]].GetType() + ", " + viewLookup[parent].GetType());
                        if (viewLookup[symbol.GetText()[symbol.GetText().Count - 1]] is TLayout)
                        {
                            RenderFactory.SetPadding((TLayout)temp, 10);
                        }
                        else
                        {
                            RenderFactory.SetPadding((TLayout)temp, 0);
                        }
                    }

                    viewLookup.Add(symbol, temp);
                }
                /*else if (!shown.Keys.Contains(views[list[i]].child))
                {
                    shown.Add(views[list[i]].child, list[i]);
                }
            }
        }

        public void CheckOverlap()
        {
            foreach(Symbol s in viewLookup.value1List)
            {
                RenderFactory.IsOverlapping(viewLookup[s], (TView)GraphicsEngine.phantomCursor, s);
            }
        }

        /*public List<Symbol> expand(List<Symbol> sender)
        {
            List<Symbol> list = new List<Symbol>();
            foreach (Symbol s in sender)
                list.Add(s);

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].GetText().Count > 0 && list[i] != list[i].GetText()[0])
                {
                    list.InsertRange(i + 1, list[i].GetText());
                }
            }

            return list;
        }
    }*/
}
