using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

using Calculator;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using Xamarin.Forms.MathDisplay;
using Xamarin.Forms.Extensions;
using Calculator.iOS;
using CoreGraphics;

using Google.MobileAds;
using System.ComponentModel;

[assembly: ExportRenderer(typeof(Calculator.Canvas), typeof(CanvasRenderer))]
[assembly: ExportRenderer(typeof(LongClickableButton), typeof(LongClickableButtonRenderer))]
[assembly: ExportRenderer(typeof(TouchableStackLayout), typeof(TouchableStackLayoutRenderer))]
[assembly: ExportRenderer(typeof(BannerAd), typeof(BannerAdRenderer))]
[assembly: ExportRenderer(typeof(TouchScreen), typeof(TouchScreenRenderer))]

namespace Calculator.iOS
{
    public static class ExtensionMethods
    {
        public static bool RelayTouch(this View shared, UIView native, CGPoint point, TouchState state) => shared.TryToTouch(native.ScaleTouch(shared, point), (int)state);

        public static Point ScaleTouch(this UIView native, View shared, CGPoint point) => new Point(shared.Width * point.X / native.Frame.Width, shared.Height * point.Y / native.Frame.Height);
    }

    public class TouchScreenRenderer : VisualElementRenderer<StackLayout>
    {
        public static UIPanGestureRecognizer pan = new UIPanGestureRecognizer();
        private static TouchScreenRenderer Instance;

        public TouchScreenRenderer()
        {
            Instance = this;
            
            pan.AddTarget(() => {
                if (Drag.Active)
                {
                    RelayDrag(pan);
                }
            });
            AddGestureRecognizer(pan);
        }

        public static void RelayDrag(UIGestureRecognizer gesture) => Drag.OnTouch(Instance.ScaleTouch(Instance.Element, gesture.LocationInView(Instance)), gesture.State == UIGestureRecognizerState.Ended ? TouchState.Up : TouchState.Moving);

        public static void RelayDrag(UITouch touch, TouchState state) => Drag.OnTouch(Instance.ScaleTouch(Instance.Element, touch.LocationInView(Instance)), state);

        public override UIView HitTest(CGPoint point, UIEvent uievent)
        {
            Drag.OnTouch(Instance.ScaleTouch(Instance.Element, point), TouchState.Down);
            Drag.Screen.OnInterceptedTouch(Instance.ScaleTouch(Instance.Element, pan.LocationInView(Instance)), TouchState.Down);
            return base.HitTest(point, uievent);
        }
    }

    public class TouchableStackLayoutRenderer : VisualElementRenderer<StackLayout>
    {
        public TouchableStackLayoutRenderer()
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
            AddGestureRecognizer(tap);

            UIPanGestureRecognizer pan = new UIPanGestureRecognizer();
            pan.ShouldRecognizeSimultaneously = (a, b) => !Drag.Active;
            pan.AddTarget(() =>
            {
                if (Drag.Active)
                {
                    TouchScreenRenderer.RelayDrag(pan);
                }
            });
            AddGestureRecognizer(pan);
        }

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            base.TouchesBegan(touches, evt);
            
            UITouch touch = touches.AnyObject as UITouch;
            if (touch != null && touch.TapCount == 1 && touches.Count == 1)
            {
                Element.RelayTouch(this, touch.LocationInView(this), TouchState.Down);
            }
        }

        public override void TouchesMoved(NSSet touches, UIEvent evt)
        {
            base.TouchesMoved(touches, evt);

            UITouch touch = touches.AnyObject as UITouch;
            if (touch != null && touch.TapCount == 1 && touches.Count == 1)
            {
                Element.RelayTouch(this, touch.LocationInView(this), TouchState.Moving);
            }
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);
            Drag.OnTouch(Point.Zero, TouchState.Up);
        }
    }

    public class CanvasRenderer : VisualElementRenderer<AbsoluteLayout>
    {
        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesBegan(touches, evt);
            
            UITouch touch = touches.AnyObject as UITouch;
            if (touch != null && touch.TapCount == 1 && touches.Count == 1)
            {
                Element.RelayTouch(this, touch.LocationInView(this), TouchState.Up);
            }
        }
    }

    public class LongClickableButtonRenderer : ButtonRenderer
    {
        UIPanGestureRecognizer pan = new UIPanGestureRecognizer();

        public LongClickableButtonRenderer()
        {
            UILongPressGestureRecognizer longPress = new UILongPressGestureRecognizer();
            longPress.AddTarget(() =>
            {
                if (longPress.State == UIGestureRecognizerState.Began)
                {
                    (Element as LongClickableButton).OnLongClick();
                }
                else if (longPress.State == UIGestureRecognizerState.Ended)
                {
                    TouchScreenRenderer.RelayDrag(longPress);
                }
            });
            longPress.ShouldRecognizeSimultaneously = (gesture1, gesture2) => gesture2 == TouchScreenRenderer.pan || gesture2 == pan;
            AddGestureRecognizer(longPress);
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
        {
            base.OnElementChanged(e);

            if (Element != null)
            {
                Element parent = Element;
                while (parent != null && !((parent = parent.Parent) is ScrollView)) { }

                //if (Element is LongClickableButton && (Element as LongClickableButton).Draggable)
                if (parent == null)
                {
                    //pan.ShouldRecognizeSimultaneously = (a, b) => !Drag.Active;
                    pan.AddTarget(() =>
                    {
                        if (Drag.Active)
                        {
                            TouchScreenRenderer.RelayDrag(pan);
                        }
                        else if (pan.State == UIGestureRecognizerState.Began)
                        {
                            Element.RelayTouch(this, pan.LocationInView(this), TouchState.Moving);
                        }
                    });
                    AddGestureRecognizer(pan);
                }
            }
        }
    }

    //ca-app-pub-1795523054003202/6023719192

    //test
    //ca-app-pub-3940256099942544/2934735716

    public class BannerAdRenderer : ViewRenderer<BannerAd, BannerView>
    {
        //Real
        string AdUnitId = "ca-app-pub-1795523054003202/6023719192";

        //Test
        //string AdUnitId = "ca-app-pub-3940256099942544/2934735716";

        protected override void OnElementChanged(ElementChangedEventArgs<BannerAd> e)
        {
            base.OnElementChanged(e);
            if (Control == null)
            {
                SetNativeControl(CreateBannerView());
            }
            if (e.OldElement == null)
            {
                //e.NewElement.WidthRequest = MainPage.MaxBannerWidth;
                //e.NewElement.HeightRequest = (int)Math.Ceiling(MainPage.MaxBannerWidth * 50.0 / 320.0);
            }
        }

        private BannerView CreateBannerView()
        {
            /*var bannerView = new BannerView(AdSizeCons.GetFromCGSize(new CGSize(
                MainPage.MaxBannerWidth,
                (int)Math.Ceiling(MainPage.MaxBannerWidth * 50.0 / 320.0)
                )))*/
            var bannerView = new BannerView(AdSizeCons.GetFromCGSize(new CGSize(MainPage.MaxBannerWidth, 50)))
            {
                AdUnitID = AdUnitId,
                RootViewController = GetVisibleViewController()
            };

            bannerView.LoadRequest(GetRequest());
            
            Request GetRequest()
            {
                var request = Request.GetDefaultRequest();
                return request;
            }

            return bannerView;
        }

        private UIViewController GetVisibleViewController()
        {
            var windows = UIApplication.SharedApplication.Windows;
            foreach (var window in windows)
            {
                if (window.RootViewController != null)
                {
                    return window.RootViewController;
                }
            }
            return null;
        }
    }
}