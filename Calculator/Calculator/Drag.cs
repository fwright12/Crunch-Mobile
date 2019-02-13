using System;
using System.Collections.Generic;
using System.Text;

using Xamarin.Forms.Extensions;
using Calculator;

namespace Xamarin.Forms
{
    public static class Drag
    {
        public static TouchScreen Screen;
        public static bool ShouldIntercept = false;

        private static View view;
        private static Rectangle bounds;
        private static double speed;
        private static Point lastTouch;
        private static Point lastPosition;

        public static void BeginDrag(this View v, Rectangle boundedArea, double moveSpeed = 1)
        {
            view = v;
            bounds = boundedArea;
            speed = moveSpeed;
            Screen.Touch += UpdatePosition;

            lastTouch = TouchScreen.LastDownEvent;
            lastPosition = new Point(view.X, view.Y);

            ShouldIntercept = true;
        }

        private static void UpdatePosition(Point point, TouchState state)
        {
            lastPosition = lastPosition.Add(point.Subtract(lastTouch).Multiply(speed));
            lastTouch = point;

            (view as View).MoveView(bounds, lastPosition);
        }

        public static void End()
        {
            Screen.Touch -= UpdatePosition;
            ShouldIntercept = false;
        }

        public static readonly Action<View, Point> MoveOnAbsoluteLayout = (v, p) => v.MoveTo(p);

        public static readonly Action<View, Point> MoveOnRelativeLayout = (v, p) =>
        {
            RelativeLayout.SetXConstraint(v, Constraint.Constant(p.X));
            RelativeLayout.SetYConstraint(v, Constraint.Constant(p.Y));
        };

        public static readonly Action<View, Point> MoveOnLayout = (v, p) =>
        {
            v.TranslationX = p.X;
            v.TranslationY = p.Y;
        };

        public static readonly Rectangle Unbounded = new Rectangle(double.NegativeInfinity, double.NegativeInfinity, double.PositiveInfinity, double.PositiveInfinity);

        public static void MoveView(this View v, Rectangle bounds, Point destination)
        {
            Action<View, Point> move = v.Parent is AbsoluteLayout ? MoveOnAbsoluteLayout : MoveOnLayout;
            Point p = new Point(
                System.Math.Max(bounds.X, System.Math.Min(bounds.X + bounds.Width - v.Width, destination.X)),
                System.Math.Max(bounds.Y, System.Math.Min(bounds.Y + bounds.Height - v.Height, destination.Y))
                );
            move(v, p);
        }
    }
}
