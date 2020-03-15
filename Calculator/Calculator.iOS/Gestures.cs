using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using CoreGraphics;
using Foundation;
using UIKit;

using Xamarin.Forms;
using Xamarin.Forms.Extensions;
using Xamarin.Forms.Platform.iOS;

namespace Calculator.iOS
{
    public class Touch //: TouchInterface
    {
        public static Func<IVisualElementRenderer, Touch> Create = (renderer) => new Touch(renderer);

        protected UIView NativeView => Renderer.NativeView;
        protected Element SharedView => Renderer.Element;

        protected IVisualElementRenderer Renderer;
        protected TouchInterface TouchInterface;
        protected Dictionary<Gesture, UIGestureRecognizer> GestureRecognizers = new Dictionary<Gesture, UIGestureRecognizer>
        {
            { Gesture.Click, new UITapGestureRecognizer() },
            { Gesture.Pinch, new UIPinchGestureRecognizer() },
            { Gesture.Rotation, new UIRotationGestureRecognizer() },
            { Gesture.Swipe, new UISwipeGestureRecognizer() },
            { Gesture.Pan, new UIPanGestureRecognizer() },
            { Gesture.LongClick, new UILongPressGestureRecognizer() }
        };

        private UITouchGestureRecognizer AllTouches;

        public Touch(IVisualElementRenderer renderer)
        {
            Renderer = renderer;

            AllTouches = new UITouchGestureRecognizer
            {
                ShouldRecognizeSimultaneously = (a, b) => true,
                CancelsTouchesInView = false
            };
            AllTouches.AddTarget(() =>
            {
                TouchInterface?.OnTouch(SharedView, new TouchEventArgs(AllTouches.LocationInView(NativeView).Convert(), AllTouches.State.Convert()));
                //(renderer.Element as View)?.GetInterceptedTouches().OnTouch(renderer.Element, new TouchEventArgs(AllTouches.LocationInView(renderer.NativeView).Convert(), AllTouches.State.Convert()));
            });
            NativeView.AddGestureRecognizer(AllTouches);

            foreach(KeyValuePair<Gesture, UIGestureRecognizer> kvp in GestureRecognizers)
            {
                Gesture gesture = kvp.Key;
                UIGestureRecognizer gestureRecognizer = kvp.Value;

                gestureRecognizer.AddTarget(() =>
                {
                    if (gestureRecognizer.State == UIGestureRecognizerState.Began)
                    {
                        GestureStarted(gesture, gestureRecognizer);
                    }
                });
            }

            renderer.ElementChanged += ElementChanged;
            ElementChanged(NativeView, new VisualElementChangedEventArgs(null, SharedView as VisualElement));
            
            /*UILongPressGestureRecognizer longPress = new UILongPressGestureRecognizer { };
            longPress.AddTarget(() =>
            {
                if (longPress.State == UIGestureRecognizerState.Began)
                {
                    //[0:] gesture recognizer 442357985 attached to - 1571525676 for -1575243898
                    //[0:] long press began, -1575243898, -1571525676, 442357985

                    //Print.Log("long press began", shared.GetHashCode(), native.GetHashCode(), longPress.GetHashCode());
                    //LongClick(renderer.Element, new TouchEventArgs());
                    TouchInterface.GestureStarted(Gesture.LongClick, SharedView, new DiscreteTouchEventArgs(longPress.LocationInView(NativeView).Convert()));
                    //((DiscreteTouchEvent)renderer.Element.GetValue(GestureExtensions.LongClickEventProperty)).Started(LongPress.LocationInView(renderer.NativeView).Convert());
                }
            });*/

            /*if (((IList)shared.GetValue(GestureExtensions.LongClickEventProperty)).Count > 0)
            {
                Print.Log("adding long press");
                native.AddGestureRecognizer(longPress);
            }
            return;*/

            /*EventHandler<VisualElementChangedEventArgs> handler = (sender, e) => MonitorListeners(
                new Pair(GestureExtensions.LongClickEventProperty, LongPress));
            renderer.ElementChanged += handler;
            renderer.ElementChanged += System.Extensions.Events.CallAndReturn<VisualElementChangedEventArgs>(() => MonitorListeners(
                new Pair(GestureExtensions.LongClickEventProperty, LongPress)));*/
        }

        //public virtual UIView HitTest(CGPoint point, UIEvent uievent) => Renderer.NativeView.HitTest(point, uievent);

        public virtual void TouchesBegan(NSSet touches, UIEvent evt) { }

        public virtual void TouchesCancelled(NSSet touches, UIEvent evt) { }

        public virtual void TouchesEnded(NSSet touches, UIEvent evt) { }

        public virtual void TouchesMoved(NSSet touches, UIEvent evt) { }

        protected virtual void ElementChanged(object sender, VisualElementChangedEventArgs e)
        {
            if (e.OldElement == e.NewElement || !(e.NewElement is View view))
            {
                return;
            }

            TouchInterface = view.GetTouchInterface();

            foreach (KeyValuePair<Gesture, UIGestureRecognizer> kvp in GestureRecognizers)
            {
                ObservableCollection<object> observableCollection;
                if (TouchInterface.TryGetValue(kvp.Key, out observableCollection))// is IList list)
                {
                    var gestureRecognizer = kvp.Value;

                    /*if (list is INotifyCollectionChanged observableCollection)
                    {
                        
                    }*/

                    observableCollection.CollectionChanged += (sender1, e1) => ListenersChanged(observableCollection, gestureRecognizer);
                    ListenersChanged(observableCollection, gestureRecognizer);
                }
            }
        }

        protected virtual void Click(UITapGestureRecognizer gestureRecognizer) => GestureStarted(Gesture.Click, gestureRecognizer);

        protected virtual void Pinch(UIPinchGestureRecognizer gestureRecognizer) => GestureStarted(Gesture.Pinch, gestureRecognizer);

        protected virtual void Rotation(UIRotationGestureRecognizer gestureRecognizer) => GestureStarted(Gesture.Rotation, gestureRecognizer);

        protected virtual void Swipe(UISwipeGestureRecognizer gestureRecognizer) => GestureStarted(Gesture.Swipe, gestureRecognizer);

        protected virtual void Pan(UIPanGestureRecognizer gestureRecognizer) => GestureStarted(Gesture.Pan, gestureRecognizer);

        protected virtual void Click(UILongPressGestureRecognizer gestureRecognizer) => GestureStarted(Gesture.LongClick, gestureRecognizer);

        protected void GestureStarted(Gesture gesture, UIGestureRecognizer gestureRecognizer) => TouchInterface.GestureStarted(gesture, SharedView, new DiscreteTouchEventArgs(gestureRecognizer.LocationInView(NativeView).Convert()));

        private void ListenersChanged(IList listeners, UIGestureRecognizer gestureRecognizer)
        {
            UIView view = Renderer.NativeView;

            if (listeners.Count == 0)
            {
                view.RemoveGestureRecognizer(gestureRecognizer);
            }
            else if (!view.GestureRecognizers.Contains(gestureRecognizer))
            {
                Print.Log("adding gesture recognizer", gestureRecognizer.GetType());
                view.AddGestureRecognizer(gestureRecognizer);

                /*foreach(UIGestureRecognizer temp in view.GestureRecognizers)
                {
                    if (temp.GetType() == gestureRecognizer.GetType())
                    {
                        //Print.Log("would return", view.GestureRecognizers.Length, temp.GetType(), gestureRecognizer.GetType());
                        return;
                    }
                }*/

                //Print.Log("gesture recognizer " + gestureRecognizer.GetHashCode() + " attached to " + view.GetHashCode() + " for " + shared.GetHashCode());
                //Print.Log("adding gesture recognizer", view.GestureRecognizers.Length, view.GetHashCode(), gestureRecognizer.GetHashCode());
            }
        }
    }
}