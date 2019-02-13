using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Crunch.GraFX;

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

        public static void CanvasTouch(Point pos)
        {
            graphics.AddEquation(pos);
        }

        public static void ViewTouched(Answer answer)
        {
            answer.SwitchFormat();
        }

        public static void LongClickDown(Element element, Point pos, bool isDown) => graphics.LongClickDown(element, pos, isDown);

        public static void CursorMode(Point pos, bool isCursorMode)
        {
            graphics.CursorMode(pos, isCursorMode);
        }

        public static void MoveCursor(Point pos)
        {
            graphics.MovePhantomCursor(pos);
        }

        public static void UndockKeyboard(Point pos)
        {
            graphics.InitMoveKeyboard(pos);
        }

        public static void MoveKeyboard(Point pos)
        {
            graphics.MoveKeyboard(pos);
        }
    }
}
