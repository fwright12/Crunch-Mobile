using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Xamarin.Forms.Extensions;

namespace Calculator.Droid
{
    public static class ExtensionMethods
    {
        public static bool RelayTouch<T>(this VisualElementRenderer<T> native, MotionEvent e) where T : VisualElement => native.RelayTouch(e, native.Element.TryToTouch);

        public static bool RelayTouch<T>(this VisualElementRenderer<T> native, MotionEvent e, Func<Point, TouchState, bool> relay) where T : VisualElement
        {
            TouchState touchState;

            if (e.Action == MotionEventActions.Down)
            {
                touchState = TouchState.Down;
            }
            else if (e.Action == MotionEventActions.Move)
            {
                touchState = TouchState.Moving;
            }
            else if (e.Action == MotionEventActions.Up)
            {
                touchState = TouchState.Up;
            }
            else
            {
                return native.Element is ITouchable;
            }

            return relay(native.ScaleTouch(native.Element, e), touchState);
        }

        public static Point ScaleTouch(this Android.Views.View native, VisualElement shared, MotionEvent e) => new Point(shared.Width * e.GetX() / native.Width, shared.Height * e.GetY() / native.Height);
    }
}