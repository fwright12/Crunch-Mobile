using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Graphics;

namespace Calculator
{
    public static class Control<TView, TLayout> where TLayout : TView
    {
        public static TView phantomCursor;
        public static IRenderFactory<TView, TLayout> renderFactory;

        public static TLayout canvas;
        public static TView cursor;

        public static void Right()
        {
            Cursor.Right();
            UpdateCursor();
        }

        public static void Left()
        {
            Cursor.Left();
            UpdateCursor();
        }

        public static void Delete()
        {
            if (Cursor.Index > 0)
            {
                Symbol prev = Cursor.Parent.Children[Cursor.Index - 1];
                prev.Remove();
                Cursor.Delete();

                renderFactory.Remove(viewLookup[prev]);
                viewLookup.Remove(prev);

                SetAnswer(Cursor.root as dynamic);
            }
        }

        public static void Initialize(IRenderFactory<TView, TLayout> RenderFactory, TLayout Canvas)
        {
            renderFactory = RenderFactory;
            canvas = Canvas;

            phantomCursor = renderFactory.CreatePhantomCursor();
            cursor = renderFactory.CreateCursor();

            renderFactory.Initialize(canvas, phantomCursor);
            renderFactory.Add(canvas, phantomCursor);
        }

        public static void AddEquation(Point pos)
        {
            //Add relative layout to main canvas
            TLayout background = renderFactory.AddEquation(pos);

            renderFactory.Add(canvas, background);
            SetText(background, new Expression(Cursor.root = new Expression(), new Text("=", false)));
            UpdateCursor();
        }

        public static Point lastPos;
        private static float increase = 1f;

        public static void MovePhantomCursor(Point pos)
        {     
            //Physically move the phantom cursor
            Point globalPos = renderFactory.MovePhantomCursor(phantomCursor, new Point((pos.x - lastPos.x) * increase, (pos.y - lastPos.y) * increase));
            lastPos = new Point(pos);

            TView view = renderFactory.GetViewAt(canvas, globalPos);
            var leftOrRight = renderFactory.LeftOrRight(view, globalPos.x);

            if (viewLookup.Contains(view) && ((viewLookup[view] is Text && (viewLookup[view] as Text).selectable) || viewLookup[view] is Expression))
            {
                var symbol = viewLookup[view];

                bool changed;
                if (symbol is Expression)
                {
                    Expression e = symbol as Expression;
                    changed = Cursor.Set(e, e.Children.Count * leftOrRight);
                }
                else
                {
                    changed = Cursor.Set(symbol.Parent, symbol.Index + leftOrRight);
                }

                if (changed)
                {
                    UpdateCursor();
                }
            }
        }

        public static void KeyboardInput(string key)
        {
            print.log(key);

            Symbol node = default(Symbol);

            switch (key)
            {
                case "/":
                    node = new Fraction();
                    break;
                default:
                    if (key.IsNumber())
                    {
                        node = new Number(key);
                    }
                    else
                    {
                        node = new Text(key);
                    }
                    break;
            }

            node.Add();

            bool flag = false;
            while (Control.Creator.Count > 0)
            {
                Control.Creator.Dequeue()(node);
                flag = true;
            }

            SetText(node);

            if (flag)
            {
                UpdateCursor();
            }

            SetAnswer(Cursor.root as dynamic);
        }

        private static void UpdateCursor()
        {
            renderFactory.Remove(cursor);
            renderFactory.Add((TLayout)viewLookup[Cursor.Parent], cursor, Cursor.Index);
        }

        private static BiDictionary<Symbol, TView> viewLookup = new BiDictionary<Symbol, TView>();

        /// <summary>
        /// Assumes that the symbol being passed has a parent symbol and that the parent symbol has an associated view
        /// </summary>
        /// <param name="symbol"></param>
        private static void SetText(Symbol symbol)
        {
            if (symbol.HasParent && viewLookup.Contains(symbol.Parent))
            {
                SetText((TLayout)viewLookup[symbol.Parent], symbol, symbol.Parent.Children.IndexOf(symbol));
            }
            else
            {
                throw new Exception("The given symbol either is does not have a parent symbol, or the parent does not have an associated view. Alternatively, pass a parent and index");
            }
        }

        private static void SetText(TLayout parent, Symbol symbol, int index = 0)
        {
            print.log("set text " + symbol);
            var view = default(TView);

            if (viewLookup.Contains(symbol))
            {
                view = viewLookup[symbol];
            }
            else
            {
                view = renderFactory.Create(symbol as dynamic);
                viewLookup.Add(symbol, view);
            }

            object currentParent = renderFactory.GetParent(view as dynamic);
            if (currentParent == null || !currentParent.Equals(parent) || renderFactory.GetIndex(view as dynamic) != index)
            {
                renderFactory.Add(parent as dynamic, view as dynamic, index);
            }

            if (symbol.HasParent && symbol.Parent is Expression)
            {
                renderFactory.CheckPadding(parent, symbol.Parent.Children.First() is Fraction, symbol.Parent.Children.Last() is Fraction);
            }

            if (symbol is Layout)
            {
                Layout layout = symbol as Layout;

                for (int i = 0; i < layout.Children.Count; i++)
                {
                    SetText((TLayout)view, layout.Children[i], i);
                }
            }
        }

        private static bool isDecimal = false;

        private static void SetAnswer(Crunch.Term calculated)
        {
            Layout root = Cursor.root.Parent;

            if (root.Children.Count == 3)
            {
                renderFactory.Remove(viewLookup[root.Children[2]]);
                root.Children[2].Remove();
            }

            Symbol answer = default(Symbol);
            if (calculated == null)
            {
                answer = new Text("error");
            }
            else
            {
                answer = calculated as dynamic;
                if (isDecimal)
                {
                    answer = new Crunch.Number(calculated.value);
                }
            }

            answer.AddAfter(root.Children[1]);
            SetText(answer);
            renderFactory.AttachClickListener(viewLookup[answer], delegate { isDecimal = !isDecimal; SetAnswer(Cursor.root as dynamic); });

            /*print.log("SDJFLSDJL " + calculated);

            try
            {
                //renderFactory.Remove(answer as dynamic);
                //((rootObject as Android.Views.View).Parent as Android.Views.ViewGroup).RemoveViewAt(2);
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
            //SetText(right, answer);*/
        }
    }
}
