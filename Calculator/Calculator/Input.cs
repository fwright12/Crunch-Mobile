using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Calculator
{
    public class Input
    {
        public static int cursorWidth;
        public static double textHeight;
        public static int textWidth;
        public static int TextSize = 40;

        private static MainPage graphics;

        public static void Started(MainPage _graphics)
        {
            graphics = _graphics;
        }

        public static void Delete()
        {
            if (Cursor.Delete())
            {
                Cursor.Parent.RemoveAt(Cursor.Index);
                graphics.SetAnswer();
            }
        }

        public static void CanvasTouch(Point pos)
        {
            graphics.AddEquation(pos);
        }

        public static void LongClickDown(string text, bool isDown)
        {
            print.log("long click");

            if (text == "<X")
            {
                graphics.clearCanvas();
            }
            else
            {
                graphics.CursorMode(isDown);
            }
        }

        public static void CursorMode(bool isCursorMode)
        {
            graphics.CursorMode(isCursorMode);
        }

        public static void MoveCursor(Point pos)
        {
            graphics.MovePhantomCursor(pos);
        }

        public static Action Focus;

        public static void Key(string key)
        {
            print.log("a " + key + " was pressed");
            Expression backward = null;
            Expression forward = null;
            View node = parseKey(key, ref backward, ref forward);

            Cursor.Add(node);
            if (forward != null)
            {
                Grab(forward, node, 1);
            }
            if (backward != null)
            {
                Grab(backward, node, -1);
            }

            graphics.SetAnswer();
        }

        /// <summary>
        /// Populate this expresssion with the value in <paramref name="parent"/> that precedes or follows
        /// the value at the index <paramref name="index"/>
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="index"></param>
        /// <param name="direction"></param>
        private static void Grab(Expression target, View adding, int direction)
        {
            Layout<View> temp = adding.Parent as Layout<View>;
            IList<View> list = temp.Children;
            int index = temp.Children.IndexOf(adding);

            if ((index + direction).IsBetween(0, list.Count - 1))
            {
                View view = list[index + direction];

                if (view is Number)
                {
                    print.log("grabbing " + (view as Number).Text);
                    while ((index + direction).IsBetween(0, list.Count - 1) && list[index + direction] is Number)
                    {
                        index += direction;
                        target.Insert(target.ChildCount * (direction + 1) / -2, list[index]);
                    }
                }
                else if (view is BoxView || view is Fraction)
                {
                    target.Add(view);
                }
            }
        }

        private static View parseKey(string key, ref Expression backward, ref Expression forward)
        {
            switch (key)
            {
                case "/":
                    return new Fraction(backward = new Expression(), forward = new Expression());
                case "^":
                    return forward = new Exponent();
                default:
                    if (key.IsNumber())
                    {
                        return new Number(key);
                    }
                    else if (key == "-")
                    {
                        return new Minus();
                    }
                    else
                    {
                        return new Text(key);
                    }
            }
        }
    }
}
