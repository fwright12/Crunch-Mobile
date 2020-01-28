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

namespace Calculator.iOS
{
    public class TouchableStackLayoutRenderer : VisualElementRenderer<StackLayout>
    {
        public TouchableStackLayoutRenderer()
        {
            TouchScreenRenderer.AddDrag(this);
        }

        protected override void OnElementChanged(ElementChangedEventArgs<StackLayout> e)
        {
            base.OnElementChanged(e);

            if (!(e.NewElement is TouchableStackLayout element) || element.LongClicked == null)
            {
                return;
            }
            
            //element.RequestAddLongClick += (sender, e1) =>
            //{
                UILongPressGestureRecognizer longPress = new UILongPressGestureRecognizer();
                longPress.AddTarget(() =>
                {
                    if (longPress.State == UIGestureRecognizerState.Began)
                    {
                        Print.Log("recognized", longPress.State, Element is TouchableStackLayout, element, element?.LongClicked == null);
                        element.LongClicked(element, new TouchEventArgs(longPress.LocationInView(this).Convert(), longPress.State.Convert()));
                    }
                });
                AddGestureRecognizer(longPress);
            //};
        }

        public override UIView HitTest(CGPoint point, UIEvent uievent)
        {
            UIView view = base.HitTest(point, uievent);
            return view == this && Element is TouchableStackLayout tsl && !tsl.ShouldIntercept && tsl.LongClicked == null ? null : view;
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