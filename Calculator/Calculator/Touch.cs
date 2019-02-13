using System;
using System.Collections.Generic;
using System.Text;

using Xamarin.Forms;

namespace Calculator
{
    public delegate void DragEventHandler();
    public delegate void MoveEventHandler(Point pos);
    public enum TouchState { Up, Down, Moving };

    public static class Drag
    {
        public static Action<Point> Move;

        public static Point Touch;
        public static bool ShouldIntercept = false;

        public static void StartDrag()
        {
            ShouldIntercept = true;
        }

        public static void UpdatePosition(Point point)
        {
            Move?.Invoke(point.Subtract(Touch));
            Touch = point;
        }

        public static void EndDrag()
        {
            ShouldIntercept = false;
            Move = null;
        }
    }
}
