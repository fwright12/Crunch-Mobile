using System;
using System.Collections.Generic;
using System.Text;

using Xamarin.Forms.Extensions;
using Calculator;

namespace Xamarin.Forms
{
    public delegate void DragEventHandler(Drag.State state);

    public static class Drag
    {
        public enum State { Moving, Ended };

        public static TouchScreen Screen;
        public static event DragEventHandler Dragging;
        public static Point LastTouch { get; private set; }
        public static bool Active { get; private set; } = false;

        private static View view;
        private static Rectangle bounds;
        private static double speed;
        private static Point lastPosition;

        public static void BeginDrag(this View v, Rectangle boundedArea, double moveSpeed = 1)
        {
            view = v;
            bounds = boundedArea;
            speed = moveSpeed;
            //Screen.Touch += UpdatePosition;

            //lastTouch = TouchScreen.LastDownEvent;
            lastPosition = new Point(view.X, view.Y);

            Active = true;
        }

        public static void OnTouch(Point point, TouchState state)
        {
            if (state == TouchState.Down)
            {
                LastTouch = point;
            }
            else if (Active)
            {
                if (state == TouchState.Moving)
                {
                    UpdatePosition(point, state);
                    Dragging?.Invoke(State.Moving);
                }
                else if (state == TouchState.Up)
                {
                    Active = false;
                    Dragging?.Invoke(State.Ended);
                }
            }
        }

        private static void UpdatePosition(Point point, TouchState state)
        {
            lastPosition = lastPosition.Add(point.Subtract(LastTouch).Multiply(speed));
            LastTouch = point;
            //print.log("dragging", point, LastTouch, point.Subtract(LastTouch).Multiply(speed), lastPosition);
            
            view.MoveView(bounds, lastPosition);
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
                Math.Max(0, Math.Min(bounds.Width - v.Width, destination.X)),
                Math.Max(0, Math.Min(bounds.Height - v.Height, destination.Y))
                );
            move(v, p);
        }
    }
}
