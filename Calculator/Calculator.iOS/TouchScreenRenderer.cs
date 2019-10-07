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

[assembly: ExportRenderer(typeof(Page), typeof(Calculator.iOS.TouchScreenRenderer))]
[assembly: ExportRenderer(typeof(MasterDetailPage), typeof(Calculator.iOS.MasterDetailRenderer))]

namespace Calculator.iOS
{
    public class MasterDetailRenderer : TabletMasterDetailRenderer
    {
        protected override void OnElementChanged(VisualElementChangedEventArgs e)
        {
            base.OnElementChanged(e);

            if (MasterDetailPage == null)
            {
                return;
            }


            MasterDetailPage.PropertyChanged += (sender, e1) =>
            {
                if (!e1.IsProperty(MasterDetailPage.MasterBehaviorProperty))
                {
                    return;
                }

                UpdatePreferredDisplayMode();
            };
            
            UpdatePreferredDisplayMode();
        }

        private void UpdatePreferredDisplayMode()
        {
            if (!IsSplit)
            {
                PreferredDisplayMode = MasterDetailPage.IsPresented ? UISplitViewControllerDisplayMode.PrimaryOverlay : UISplitViewControllerDisplayMode.PrimaryHidden;
            }
        }


        private bool IsSplit =>
            MasterDetailPage.MasterBehavior == MasterBehavior.Split ||
            (MasterDetailPage.MasterBehavior == MasterBehavior.SplitOnLandscape && true) ||
            (MasterDetailPage.MasterBehavior == MasterBehavior.SplitOnPortrait && true);
    }

    public interface ITouchableVisualElementRenderer : IVisualElementRenderer
    {
        bool Touch(Point point, TouchState state);
    }

    //public class TouchScreenRenderer : VisualElementRenderer<AbsoluteLayout>
    public class TouchScreenRenderer : PageRenderer, ITouchableVisualElementRenderer
    {
        //public static UIPanGestureRecognizer Pan { get; private set; } = new UIPanGestureRecognizer();
        //private static TouchScreenRenderer Instance;

        protected override void OnElementChanged(VisualElementChangedEventArgs e)
        {
            base.OnElementChanged(e);

            if (NativeView == null)
            {
                return;
            }
            
            UIPanGestureRecognizer pan = AddDrag(this);
            pan.ShouldReceiveTouch = (r, t) => ShouldRelayTouch;
            pan.ShouldRecognizeSimultaneously = (a, b) => true;

            UITouchGestureRecognizer touch = new UITouchGestureRecognizer()
            {
                CancelsTouchesInView = false,
                ShouldReceiveTouch = (r, t) => ShouldRelayTouch,
                ShouldRecognizeSimultaneously = (a, b) => true
            };
            
            touch.AddTarget(() =>
            {
                if (touch.State == UIGestureRecognizerState.Began)
                {
                    TouchScreen.OnInterceptedTouch(touch.LocationInView(this), TouchState.Down);
                }

                if (TouchScreen.Active && touch.State == UIGestureRecognizerState.Ended)
                {
                    this.RelayTouch(touch);
                }
            });
            NativeView.AddGestureRecognizer(touch);
        }

        /*public TouchScreenRenderer()
        {
            UITapGestureRecognizer tgr = new UITapGestureRecognizer();
            tgr.ShouldRecognizeSimultaneously = (a, b) => true;
            tgr.AddTarget(() =>
            {
                Print.Log("touchscreen tap", tgr.State);
                if (TouchScreen.Active && tgr.State == UIGestureRecognizerState.Ended)
                {
                    Element.RelayTouch(this, tgr, TouchState.Up);
                }
                //Element.RelayTouch(this, tgr.LocationInView(this), tgr.State);
            });
            AddGestureRecognizer(tgr);

            //Pan.ShouldReceiveTouch = (r, t) => true;
            /*Pan.AddTarget(() =>
            {
                //Print.Log("touchscreen pan", Pan.State);
                if (TouchScreen.Active)
                {
                    RelayDrag(Pan);
                }
            });
            AddGestureRecognizer(Pan);
        }*/

        public static UIPanGestureRecognizer AddDrag(IVisualElementRenderer native)
        {
            UIPanGestureRecognizer pan = new UIPanGestureRecognizer();

            pan.ShouldReceiveTouch = (r, t) => native.Element is ITouchable && (native.Element as ITouchable).ShouldIntercept;
            pan.AddTarget(() =>
            {
                if (!(native is TouchScreenRenderer ^ TouchScreen.Active))
                {
                    //Print.Log("relaying pan to", pan.State, native.Element, native.Element.GetType(), TouchScreen.Active);
                    native.RelayTouch(pan);
                }
            });
            native.NativeView.AddGestureRecognizer(pan);

            return pan;
        }

        public bool Touch(Point point, TouchState state)
        {
            TouchScreen.OnTouch(point, state);
            return true;
        }

        private bool ShouldRelayTouch => Element == TouchScreen.Instance;

        //public static void RelayDrag(UIGestureRecognizer gesture) => (Instance.Element as TouchScreen)?.OnTouch(Instance.ScaleTouch(Instance.Element, gesture.LocationInView(Instance)), gesture.State == UIGestureRecognizerState.Ended ? TouchState.Up : TouchState.Moving);

        //public static void RelayDrag(UITouch touch, TouchState state) => (Instance.Element as TouchScreen)?.OnTouch(Instance.ScaleTouch(Instance.Element, touch.LocationInView(Instance)), state);

        /*public override UIView HitTest(CGPoint point, UIEvent uievent)
        {
            //Print.Log("hit test on touchscreen");
            //Drag.OnTouch(Instance.ScaleTouch(Instance.Element, point), TouchState.Down);
            //(Element as TouchScreen)?.OnTouch(Instance.ScaleTouch(Instance.Element, point), TouchState.Down);
            (Element as TouchScreen2)?.OnInterceptedTouch(this.ScaleTouch(Element, point), TouchState.Down);
            return base.HitTest(point, uievent);
        }*/
    }
}