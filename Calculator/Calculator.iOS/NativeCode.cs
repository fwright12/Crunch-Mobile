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

[assembly: ExportRenderer(typeof(Canvas), typeof(CanvasRenderer))]
[assembly: ExportRenderer(typeof(LongClickableButton), typeof(LongClickableButtonRenderer))]
[assembly: ExportRenderer(typeof(TouchableStackLayout), typeof(TouchableStackLayoutRenderer))]
[assembly: ExportRenderer(typeof(BannerAd), typeof(BannerAdRenderer))]
[assembly: ExportRenderer(typeof(TouchScreen), typeof(TouchScreenRenderer))]
[assembly: ExportRenderer(typeof(SystemKeyboard.KeyboardEntry), typeof(KeyboardEntryRenderer))]

namespace Calculator.iOS
{
    public class KeyboardEntryRenderer : EntryRenderer
    {
        private bool HiddenBySystem = false;

        public KeyboardEntryRenderer()
        {
            UIKeyboard.Notifications.ObserveWillHide((sender, e) =>
            {
                Print.Log("keyboard will hide");
                HiddenBySystem = true;
                AdjustResize(0);
            });
            UIKeyboard.Notifications.ObserveWillShow((sender, e) =>
            {
                HiddenBySystem = false;

                if (Element is SystemKeyboard.KeyboardEntry keyboard && !keyboard.Showing)
                {
                    Control.EndEditing(true);
                }
            });
            
            UIKeyboard.Notifications.ObserveDidShow((sender, e) =>
            {
                AdjustResize(UIKeyboard.FrameEndFromNotification(e.Notification).Height);
            });
        }

        private void AdjustResize(double newKeyboardHeight)
        {
            Layout<View> Parent = Element.Parent<Layout<View>>();
            Thickness margin = Parent.Margin;
            margin.Bottom = newKeyboardHeight;
            Parent.Margin = margin;

            return;
            AbsoluteLayout layout = Element.Parent<AbsoluteLayout>();
            layout.HeightRequest = layout.Height - newKeyboardHeight;
            //AbsoluteLayout.SetLayoutBounds(Placeholder, new Rectangle(0, 1, -1, newKeyboardHeight));

            return;
            Page root = Element.Parent<Page>();
            Thickness padding = root.Padding;
            Print.Log(root, padding.Bottom, padding.Top);
            //padding.Bottom -= LastKeyboardHeight;
            //padding.Bottom += LastKeyboardHeight = newKeyboardHeight;
            //padding.Bottom += newKeyboardHeight;

            root.Padding = padding;
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            if (Control == null)
            {
                return;
            }

            Control.ShouldEndEditing = (sender) =>
            {
                Print.Log("should end editing", Hidden, HiddenBySystem);
                return Element is SystemKeyboard.KeyboardEntry keyboard && (!keyboard.Showing || HiddenBySystem);
            };
        }

        public override UIKeyCommand[] KeyCommands => new UIKeyCommand[]
        {
            UIKeyCommand.Create(UIKeyCommand.LeftArrow, 0, new ObjCRuntime.Selector("LeftArrow")),
            UIKeyCommand.Create(UIKeyCommand.RightArrow, 0, new ObjCRuntime.Selector("RightArrow")),
            UIKeyCommand.Create(UIKeyCommand.UpArrow, 0, new ObjCRuntime.Selector("UpArrow")),
            UIKeyCommand.Create(UIKeyCommand.DownArrow, 0, new ObjCRuntime.Selector("DownArrow"))
        };
        
        [Export("LeftArrow")]
        private void LeftArrow() => KeyboardManager.MoveCursor(KeyboardManager.CursorKey.Left);

        [Export("RightArrow")]
        private void RightArrow() => KeyboardManager.MoveCursor(KeyboardManager.CursorKey.Right);

        [Export("UpArrow")]
        private void UpArrow() => KeyboardManager.MoveCursor(KeyboardManager.CursorKey.Up);

        [Export("DownArrow")]
        private void DownArrow() => KeyboardManager.MoveCursor(KeyboardManager.CursorKey.Down);
    }

    public static class ExtensionMethods
    {
        public static bool RelayTouch<T>(this VisualElementRenderer<T> native, CGPoint point, TouchState state) where T : View => native.Element.TryToTouch(native.ScaleTouch(native.Element, point), state);

        public static void RelayTouch<T>(this VisualElementRenderer<T> native, NSSet touches, TouchState state) where T : View
        {
            UITouch touch = touches.AnyObject as UITouch;
            if (touch != null && touch.TapCount == 1 && touches.Count == 1)
            {
                native.RelayTouch(touch.LocationInView(native), state);
                //Element.RelayTouch(this, touch.LocationInView(this), TouchState.Up);
            }
        }

        public static bool RelayTouch<T>(this VisualElementRenderer<T> native, UIGestureRecognizer gesture) where T : View
        {
            TouchState touchState;

            if (gesture.State == UIGestureRecognizerState.Began)
            {
                touchState = TouchState.Down;
            }
            else if (gesture.State == UIGestureRecognizerState.Changed)
            {
                touchState = TouchState.Moving;
            }
            else if (gesture.State == UIGestureRecognizerState.Ended || gesture.State == UIGestureRecognizerState.Cancelled)
            {
                touchState = TouchState.Up;
            }
            else
            {
                return native.Element is ITouchable;
            }

            return native.RelayTouch(gesture.LocationInView(native), touchState);
        }

        public static Point ScaleTouch(this UIView native, View shared, CGPoint point) => new Point(shared.Width * point.X / native.Frame.Width, shared.Height * point.Y / native.Frame.Height);
    }

    public class TouchScreenRenderer : VisualElementRenderer<AbsoluteLayout>
    {
        //public static UIPanGestureRecognizer Pan { get; private set; } = new UIPanGestureRecognizer();
        //private static TouchScreenRenderer Instance;

        public TouchScreenRenderer()
        {
            /*if (Instance != null)
            {
                return;
            }

            Instance = this;*/

            UIPanGestureRecognizer pan = AddDrag(this);
            pan.ShouldRecognizeSimultaneously = (a, b) => true;
            
            UITouchGestureRecognizer touch = new UITouchGestureRecognizer()
            {
                CancelsTouchesInView = false
            };
            touch.ShouldRecognizeSimultaneously = (a, b) => true;
            touch.AddTarget(() =>
            {
                if (TouchScreen.Active && touch.State == UIGestureRecognizerState.Ended)
                {
                    this.RelayTouch(touch);
                }
            });
            AddGestureRecognizer(touch);
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

        public static UIPanGestureRecognizer AddDrag<T>(VisualElementRenderer<T> native) where T : View
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
            native.AddGestureRecognizer(pan);
            return pan;
        }

        //public static void RelayDrag(UIGestureRecognizer gesture) => (Instance.Element as TouchScreen)?.OnTouch(Instance.ScaleTouch(Instance.Element, gesture.LocationInView(Instance)), gesture.State == UIGestureRecognizerState.Ended ? TouchState.Up : TouchState.Moving);

        //public static void RelayDrag(UITouch touch, TouchState state) => (Instance.Element as TouchScreen)?.OnTouch(Instance.ScaleTouch(Instance.Element, touch.LocationInView(Instance)), state);

        public override UIView HitTest(CGPoint point, UIEvent uievent)
        {
            //Print.Log("hit test on touchscreen");
            //Drag.OnTouch(Instance.ScaleTouch(Instance.Element, point), TouchState.Down);
            //(Element as TouchScreen)?.OnTouch(Instance.ScaleTouch(Instance.Element, point), TouchState.Down);
            (Element as TouchScreen)?.OnInterceptedTouch(this.ScaleTouch(Element, point), TouchState.Down);
            return base.HitTest(point, uievent);
        }
    }

    public class TouchableStackLayoutRenderer : VisualElementRenderer<StackLayout>
    {
        public TouchableStackLayoutRenderer()
        {
            TouchScreenRenderer.AddDrag(this);
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

    public class CanvasRenderer : VisualElementRenderer<AbsoluteLayout>
    {
        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);
            this.RelayTouch(touches, TouchState.Up);
        }
    }

    public class LongClickableButtonRenderer : ButtonRenderer
    {
        //UIPanGestureRecognizer pan = new UIPanGestureRecognizer();

        public LongClickableButtonRenderer()
        {
            UIPanGestureRecognizer tgr = TouchScreenRenderer.AddDrag(this);
            tgr.CancelsTouchesInView = false;

            UILongPressGestureRecognizer longPress = new UILongPressGestureRecognizer
            {
                CancelsTouchesInView = false
            };
            longPress.AddTarget(() =>
            {
                if (longPress.State == UIGestureRecognizerState.Began)
                {
                    (Element as LongClickableButton)?.OnLongClick();
                }
                else if (longPress.State == UIGestureRecognizerState.Ended)
                {
                    this.RelayTouch(longPress);
                }
                /*else if (longPress.State == UIGestureRecognizerState.Ended)
                {
                    //TouchScreenRenderer.RelayDrag(longPress);
                }*/
            });
            //longPress.ShouldRecognizeSimultaneously = (gesture1, gesture2) => gesture2 == TouchScreenRenderer.Pan || gesture2 == pan;
            AddGestureRecognizer(longPress);
        }

        /*protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
        {
            base.OnElementChanged(e);

            if (Element != null)
            {
                Element parent = Element;
                while (parent != null && !((parent = parent.Parent) is ScrollView)) { }

                //if (Element is LongClickableButton && (Element as LongClickableButton).Draggable)
                if (parent == null)
                {
                    //TouchScreenRenderer.AddDrag(this);
                    //pan.ShouldRecognizeSimultaneously = (a, b) => !Drag.Active;
                    /*pan.AddTarget(() =>
                    {
                        if (TouchScreen.Active)
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
        }*/
    }

    //ca-app-pub-1795523054003202/6023719192

    //test
    //ca-app-pub-3940256099942544/2934735716

    public class BannerAdRenderer : ViewRenderer<BannerAd, BannerView>
    {
        string AdUnitId =
#if DEBUG
            //Test
            "ca-app-pub-3940256099942544/2934735716";
#else
            //Real
            "ca-app-pub-1795523054003202/6023719192";
#endif

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