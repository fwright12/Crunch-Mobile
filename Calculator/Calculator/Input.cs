using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

using Calculator.Graphics;

namespace Calculator
{
    public class Input
    {
        public static int cursorWidth;
        public static int textHeight;
        public static int textWidth;
        public static int TextSize = 40;

        public static View phantomCursor;
        public static IRenderFactory<View, Layout> renderFactory;

        public static Layout canvas;

        private static MainPage graphics;

        public static void Started(MainPage _graphics)
        {
            graphics = _graphics;
        }

        public static void Delete()
        {
            if (Cursor.Delete())
            {
                graphics.Remove(Cursor.Parent.Children[Cursor.Index]);
                graphics.SetAnswer();
            }
        }

        public static void KeyboardSwipe(int direction)
        {
            graphics.ChangeKeyboard(direction);
        }

        public static void CanvasTouch(Point pos)
        {
            graphics.AddEquation(pos);
        }

        public static void LongClickDown(bool isDown) => graphics.CursorMode(isDown);

        public static void CursorMoved(Point pos)
        {
            graphics.MovePhantomCursor(pos);
        }

        public static void Key(string key)
        {            
            Symbol node = default(Symbol);

            switch (key)
            {
                case "/":
                    node = new Fraction();
                    break;
                case "^":
                    node = new Exponent();
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

            while (Fraction.Creator.Count > 0)
            {
                Fraction.Creator.Dequeue()(node);
            }

            graphics.SetText(node);
            graphics.SetAnswer();
            graphics.UpdateCursor();
        }
    }
}
