using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using CoreGraphics;

using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using Xamarin.Forms.Extensions;

namespace Calculator.iOS
{
    public static class ExtensionMethods
    {
        public static bool RelayTouch(this IVisualElementRenderer renderer, Point point, TouchState state) => (renderer as ITouchableVisualElementRenderer)?.Touch(point, state) ?? renderer.Element.TryToTouch(point, state);

        public static bool RelayTouch(this IVisualElementRenderer renderer, UIGestureRecognizer gesture) => renderer.RelayTouch(gesture.LocationInView(renderer), gesture.State.Convert());

        public static void RelayTouch(this IVisualElementRenderer native, NSSet touches, TouchState state)
        {
            if (touches.AnyObject is UITouch touch && touch.TapCount == 1 && touches.Count == 1)
            {
                native.RelayTouch(touch.LocationInView(native.NativeView).Convert(native), state);
            }
        }

        public static Point LocationInView(this UIGestureRecognizer gesture, IVisualElementRenderer view) => gesture.LocationInView(view.NativeView).Convert(view);

        public static TouchState Convert(this UIGestureRecognizerState state)
        {
            switch (state)
            {
                case UIGestureRecognizerState.Began:
                    return TouchState.Down;
                case UIGestureRecognizerState.Changed:
                    return TouchState.Moving;
                case UIGestureRecognizerState.Ended:
                case UIGestureRecognizerState.Cancelled:
                    return TouchState.Up;
                default:
                    return (TouchState)(-1);
            }
        }

        public static Point Convert(this CGPoint point) => new Point(point.X, point.Y);

        public static Point Convert(this CGPoint point, IVisualElementRenderer reference) => new Point(reference.Element.Width * point.X / reference.NativeView.Frame.Width, reference.Element.Height * point.Y / reference.NativeView.Frame.Height);
    }
}