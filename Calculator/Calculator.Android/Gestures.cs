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
using Xamarin.Forms.Extensions;
using Xamarin.Forms.Platform.Android;
using System.Collections.ObjectModel;

namespace Calculator.Droid
{
    public class Touch : Java.Lang.Object, GestureDetector.IOnGestureListener
    {
        protected Android.Views.View NativeView => Renderer.View;
        protected Element SharedView => Renderer.Element;

        protected IVisualElementRenderer Renderer;
        protected TouchInterface TouchInterface;
        protected GestureDetector GestureDetector;

        public Touch(Context context, IVisualElementRenderer renderer)
        {
            Renderer = renderer;
            GestureDetector = new GestureDetector(context, this);

            Renderer.ElementChanged += ElementChanged;
            ElementChanged(NativeView, new VisualElementChangedEventArgs(null, SharedView as VisualElement));
        }

        protected virtual void ElementChanged(object sender, VisualElementChangedEventArgs e)
        {
            if (e.OldElement == e.NewElement || !(e.NewElement is Xamarin.Forms.View view))
            {
                return;
            }

            TouchInterface = view.GetTouchInterface();
        }

        public virtual bool DispatchTouchEvent(MotionEvent e) => (NativeView as ViewGroup)?.DispatchTouchEvent(e) ?? false;

        public virtual bool OnInterceptTouchEvent(MotionEvent ev) => (NativeView as ViewGroup)?.OnInterceptTouchEvent(ev) ?? false;

        public virtual bool OnTouchEvent(MotionEvent e)
        {
            TouchInterface?.OnTouch(SharedView, new TouchEventArgs(GetTouchLocation(e), e.Action.Convert()));
            return GestureDetector.OnTouchEvent(e) | NativeView.OnTouchEvent(e);
        }

        public virtual bool OnDown(MotionEvent e) => false;

        public virtual bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY) => false;

        public virtual void OnLongPress(MotionEvent e)
        {
            Print.Log("on long press", e.Action);
            GestureStarted(Gesture.LongClick, e);
            //((TouchEvent)Shared.GetValue(GestureExtensions.LongClickEventProperty)).Started(Native.ScaleTouch(Shared, e));
        }

        public virtual bool OnScroll(MotionEvent e1, MotionEvent e2, float distanceX, float distanceY) => false;

        public virtual void OnShowPress(MotionEvent e) { }

        public virtual bool OnSingleTapUp(MotionEvent e) => GestureStarted(Gesture.Click, e);

        private bool GestureStarted(Gesture gesture, MotionEvent e)
        {
            TouchInterface.GestureStarted(gesture, SharedView, GetTouchLocation(e));
            return false;
        }

        public Point GetTouchLocation(MotionEvent e) => NativeView.ScaleTouch(SharedView as VisualElement, e);
    }

    /*public class GestureRecognizerImplementation : Java.Lang.Object,
        INativeGestureRecognizerImplementation<LongClickGestureRecognizer>
    {
        public object AddGestureRecognizer(Xamarin.Forms.View sharedView, object nativeView, LongClickGestureRecognizer gestureRecognizer)
        {
            LongClickEventHandler handler = new LongClickEventHandler((sender, e) =>
            {
                //e.Handled = gestureRecognizer.Invoke(sharedView, new DiscreteTouchEventArgs(Point.Zero));
            });

            //((Android.Views.View)nativeView).LongClick += handler;
            return handler;
        }

        public void RemoveGestureRecognizer(Xamarin.Forms.View sharedView, object nativeView, LongClickGestureRecognizer gestureRecognizer, object nativeHandler) => ((Android.Views.View)nativeView).LongClick -= (LongClickEventHandler)nativeHandler;
        {
            var view = (Android.Views.View)nativeView;

            if (gestureRecognizer is LongClickGestureRecognizer && nativeHandler is EventHandler<Android.Views.View.LongClickEventArgs> handler)
            {
                view.LongClick -= handler;
            }
        }
    }

    public static class AndroidGestures
    {
        //public static GestureDetector GestureDetector = new GestureDetector(null, Implementation);
        //public static GestureRecognizerImplementation Implementation = new GestureRecognizerImplementation();

        /*public static void MakeGestureRecognizable<T>(this ElementChangedEventArgs<T> args, Android.Views.View view)
            where T : Element
        {
            //Print.Log("element changed", (args.NewElement as Xamarin.Forms.View)?.GetGestureRecognizers().Count);
            //Print.Log("\t", view.GetHashCode(), args.OldElement?.GetHashCode(), args.NewElement?.GetHashCode());

            //((TouchHub)args.NewElement.GetValue(Gestures.TouchHubProperty)).OnLongClick();
            //args.NewElement.SetNativeGestureRecognizerImplementation<LongClickGestureRecognizer>(Implementation);
            //args.NewElement.SetNativeImplementation(Gestures.NativeLongClickImplementationProperty, Implementation);
            //args.NewElement.SetNativeImplementation(Gestures.NativeLongClickImplementationProperty, Implementation.A);
            //args.NewElement.SetNativeImplementation(Gestures.NativeAddGestureRecognizerImplementationProperty, AddGestureRecognizer);
            //args.NewElement.SetNativeImplementation(Gestures.NativeRemoveGestureRecognizerImplementationProperty, RemoveGestureRecognizer);

            //((Gestures.Collection)args.NewElement.GetValue(Gestures.GestureRecognizersProperty)).NativeControlCreated(view);
        }

        private static object AddGestureRecognizer(Xamarin.Forms.View shared, object native, IGestureRecognizer gestureRecognizer)
        {
            var view = (Android.Views.View)native;

            Print.Log("added gesture recognizer to", native.GetHashCode());
            if (gestureRecognizer is LongClickGestureRecognizer longClick)
            {
                
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Failed to add unsupported gesture recognizer " + gestureRecognizer.GetType());
                return null;
            }
        }

        private static void RemoveGestureRecognizer(Xamarin.Forms.View shared, object native, IGestureRecognizer gestureRecognizer, object nativeHandler)
        {
            var view = (Android.Views.View)native;

            Print.Log("removing gesture recognizer");
            if (gestureRecognizer is LongClickGestureRecognizer && nativeHandler is EventHandler<Android.Views.View.LongClickEventArgs> handler)
            {
                view.LongClick -= handler;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Failed to remove unsupported gesture recognizer " + gestureRecognizer.GetType());
            }
        }
    }*/
}