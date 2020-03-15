using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using Xamarin.Forms.Extensions;
using CoreGraphics;

[assembly: ExportRenderer(typeof(TouchableStackLayout), typeof(Calculator.iOS.TouchableStackLayoutRenderer))]
//[assembly: ExportRenderer(typeof(View), typeof(Calculator.iOS.TouchEnabledViewRenderer))]

namespace Calculator.iOS
{
    public class TouchEnabledViewRenderer : ViewRenderer<View, UIView>
    {
        protected Touch Touch;

        public TouchEnabledViewRenderer()
        {
            Touch = new Touch(this);
        }
    }

    public class TouchEnabledVisualElementRenderer<TElement> : VisualElementRenderer<TElement>
        where TElement : VisualElement
    {
        protected Touch Touch;

        public TouchEnabledVisualElementRenderer()
        {
            Touch = Create(this); 
        }

        public static Touch Create(IVisualElementRenderer renderer) => new Touch(renderer);

        /*public override void TouchesBegan(NSSet touches, UIEvent evt) => Touch.TouchesBegan(touches, evt);

        public override void TouchesMoved(NSSet touches, UIEvent evt) => Touch.TouchesMoved(touches, evt);

        public override void TouchesEnded(NSSet touches, UIEvent evt) => Touch.TouchesEnded(touches, evt);

        public override void TouchesCancelled(NSSet touches, UIEvent evt) => Touch.TouchesCancelled(touches, evt);

        public override UIView HitTest(CGPoint point, UIEvent uievent) => Touch.HitTest(point, uievent);*/
    }

    public class TouchableStackLayoutRenderer : VisualElementRenderer<StackLayout>
    {
        public TouchableStackLayoutRenderer() : base()
        {
            TouchScreenRenderer.AddDrag(this);
            new Touch(this);
        }

        protected override void OnElementChanged(ElementChangedEventArgs<StackLayout> e)
        {
            base.OnElementChanged(e);
            
            //e.NewElement?.MakeGestureRecognizable(NativeView);
            return;
            if (!(e.NewElement is TouchableStackLayout element) || element.LongClicked == null)
            {
                //return;
            }
            
            //element.RequestAddLongClick += (sender, e1) =>
            //{
                UILongPressGestureRecognizer longPress = new UILongPressGestureRecognizer();
                longPress.AddTarget(() =>
                {
                    if (longPress.State == UIGestureRecognizerState.Began)
                    {
                        Print.Log("recognized", longPress.State, Element is TouchableStackLayout, e.NewElement);
                        //element.LongClicked(element, new TouchEventArgs(longPress.LocationInView(this).Convert(), longPress.State.Convert()));
                    }
                });
                AddGestureRecognizer(longPress);
            //};
        }

        public override UIView HitTest(CGPoint point, UIEvent uievent)
        {
            UIView view = base.HitTest(point, uievent);
            return view == this && Element is TouchableStackLayout tsl && !tsl.ShouldIntercept && GestureRecognizers.Length <= 1 ? null : view;
        }

        /*public TouchableStackLayoutRenderer()
        {
            UITapGestureRecognizer tap = new UITapGestureRecognizer();
            tap.ShouldRecognizeSimultaneously = (a, b) => !(Element as TouchableStackLayout).ShouldIntercept;
            tap.AddTarget(() =>
            {
                if (tap.State == UIGestureRecognizerState.Ended)
                {
                    TouchScreenRenderer.RelayDrag(tap);
                }
            });
            AddGestureRecognizer(tap);*/

        /*UIPanGestureRecognizer pan = new UIPanGestureRecognizer();
        //pan.ShouldRecognizeSimultaneously = (a, b) => !TouchScreen.Active;
        pan.AddTarget(() =>
        {
            Print.Log("panning");
            if (TouchScreen.Active)
            {
                TouchScreenRenderer.RelayDrag(pan);
            }
        });
        AddGestureRecognizer(pan);
        }*/

        /*public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            base.TouchesBegan(touches, evt);

            UITouch touch = touches.AnyObject as UITouch;
            if (touch != null && touch.TapCount == 1 && touches.Count == 1)
            {
                //Element.RelayTouch(this, touch.LocationInView(this), TouchState.Down);
            }
        }

        public override void TouchesMoved(NSSet touches, UIEvent evt)
        {
            base.TouchesMoved(touches, evt);
            
            UITouch touch = touches.AnyObject as UITouch;
            if (touch != null && touch.TapCount == 1 && touches.Count == 1)
            {
                //Element.RelayTouch(this, touch.LocationInView(this), TouchState.Moving);
            }
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);
            //TouchScreenRenderer.RelayDrag(touches.AnyObject as UITouch, TouchState.Up);
            //Drag.OnTouch(Point.Zero, TouchState.Up);
        }*/
    }
}