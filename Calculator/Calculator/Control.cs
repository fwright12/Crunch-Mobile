using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Graphics;

namespace Calculator
{
    public delegate void InitializeEventHandler();
    public delegate void AddEquationEventHandler(Point pos);
    public delegate void MoveCursorEventHandler(Point pos, Action<Symbol, int> moveRealCursor);
    public delegate void CursorMovedEventHandler();
    public delegate void KeyboardInputEventHandler(Symbol node);
    public delegate void UpdateCursorEventHandler();

    public class Control
    {
        public static event InitializeEventHandler Initialize;
        public static event AddEquationEventHandler AddEquation;
        public static event MoveCursorEventHandler MoveCursor;
        public static event CursorMovedEventHandler CursorMoved;
        public static event KeyboardInputEventHandler KeyboardInput;

        public static event UpdateCursorEventHandler UpdateCursor;
        public static void OnUpdateCursor() => UpdateCursor();

        public static void OnInitialize() => Initialize();
        public static void OnAddEquation(Point pos) => AddEquation(pos);
        public static void OnCursorMoved() => CursorMoved();

        public static void OnMoveCursor(Point pos)
        {
            Action<Symbol, int> action = delegate (Symbol symbol, int leftOrRight)
            {
                /*cursor.Remove();

                if (symbol is Expression)
                {
                    Expression expression = symbol as Expression;

                    if (expression.Children.Count == 0)
                    {
                        expression.Add(cursor);
                    }
                    else
                    {
                        symbol = expression.Children[(expression.Children.Count - 1) * leftOrRight];
                    }
                }

                //SetCursor(symbol.Parent, symbol.Index);

                if (leftOrRight == 0)
                {
                    cursor.AddBefore(symbol);
                }
                else if (leftOrRight == 1)
                {
                    cursor.AddAfter(symbol);
                }*/
            };

            MoveCursor(pos, action);
        }

        /*public static void SetCursor(Layout parent, int index)
        {
            if (index == 0)
            {
                cursor.AddBefore(parent.Children[0]);
            }
            else
            {
                cursor.AddAfter(parent.Children[index - 1]);
            }

            SetText(cursor);
        }*/

        //public static Cursor cursor = new Cursor();
        public static Queue<Action<Symbol>> Creator = new Queue<Action<Symbol>>();

        public static void OnKeyboardInput(string key)
        {
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
            //node.AddBefore(cursor);

            while (Creator.Count > 0)
            {
                Creator.Dequeue()(node);
            }

            KeyboardInput(node);
        }
    }
}
