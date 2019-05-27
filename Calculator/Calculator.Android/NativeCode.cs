using Xamarin.Forms.Platform.Android;
using Xamarin.Forms;
using Calculator;
using Calculator.Droid;
using Android.Views;
using Android.Views.InputMethods;
using System;
using Android.Graphics;
using Android.Runtime;
using Android.OS;

using Xamarin.Forms.MathDisplay;
using Android.Gms.Ads;
using Xamarin.Forms.Extensions;

[assembly: ExportRenderer(typeof(Calculator.Canvas), typeof(CanvasRenderer))]
[assembly: ExportRenderer(typeof(Button), typeof(LongClickableButtonRenderer))]
[assembly: ExportRenderer(typeof(TouchableStackLayout), typeof(TouchableStackLayoutRenderer))]
[assembly: ExportRenderer(typeof(BannerAd), typeof(BannerAdRenderer))]
[assembly: ExportRenderer(typeof(TouchScreen), typeof(TouchScreenRenderer))]

namespace Calculator.Droid
{
    public static class ExtensionMethods
    {
        public static bool RelayTouch<T>(this VisualElementRenderer<T> native, MotionEvent e) where T : Xamarin.Forms.View
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

            return native.Element.TryToTouch(native.ScaleTouch(native.Element, e), touchState);
        }

        //public static bool RelayTouch(this Xamarin.Forms.View shared, Android.Views.View native, MotionEvent e) => shared.TryToTouch(native.ScaleTouch(shared, e), (TouchState)e.Action);

        public static Xamarin.Forms.Point ScaleTouch(this Android.Views.View native, Xamarin.Forms.View shared, MotionEvent e) => new Xamarin.Forms.Point(shared.Width * e.GetX() / native.Width, shared.Height * e.GetY() / native.Height);
    }

    public class TouchScreenRenderer : VisualElementRenderer<StackLayout>
    {
        public TouchScreenRenderer() { }

        //private void DragTouch(MotionEvent e) => Xamarin.Forms.Drag.OnTouch(this.ScaleTouch(Element, e), (TouchState)e.Action);

        public override bool OnTouchEvent(MotionEvent e)
        {
            bool wants = base.OnTouchEvent(e);

            //Xamarin.Forms.Drag.Screen.RelayTouch(this, e);
            if (TouchScreen.Active)
            {
                return this.RelayTouch(e);
                //return Element.RelayTouch(this, e);
                //DragTouch(e);
            }
            return wants;
        }

        /*public override bool DispatchTouchEvent(MotionEvent e)
        {
            Element.RelayTouch(this, e);
            //(Element as TouchScreen)?.OnInterceptedTouch(this.ScaleTouch(Element, e), (TouchState)e.Action);

            if (TouchScreen.Active)
            {
                OnTouchEvent(e);
                e.Action = MotionEventActions.Cancel;
                base.DispatchTouchEvent(e);
                return true;
            }
            else
            {
                return base.DispatchTouchEvent(e);
            }
        }*/

        public override bool OnInterceptTouchEvent(MotionEvent e)
        {
            base.OnInterceptTouchEvent(e);

            (Element as TouchScreen)?.OnInterceptedTouch(this.ScaleTouch(Element, e), (TouchState)e.Action);
            //Element.RelayTouch(this, e);
            //Xamarin.Forms.Drag.Screen.OnInterceptedTouch(this.ScaleTouch(Element, e), (TouchState)(int)e.Action);

            /*if (e.Action == MotionEventActions.Down)
            {
                //TouchScreen.LastDownEvent = this.ScaleTouch(Element, e);
                //Xamarin.Forms.Drag.OnTouch(this.ScaleTouch(Element, e), (TouchState)e.Action);
                //DragTouch(e);
                Element.RelayTouch(this, e);
            }*/
            if (TouchScreen.Active && e.Action == MotionEventActions.Up)
            {
                this.RelayTouch(e);
                //return true;
                //OnTouchEvent(e);
            }
            //return false;
            return TouchScreen.Active;
        }

        public override void RequestDisallowInterceptTouchEvent(bool disallowIntercept)
        {
            Parent?.RequestDisallowInterceptTouchEvent(disallowIntercept);
        }
    }

    public class TouchableStackLayoutRenderer : VisualElementRenderer<StackLayout>
    {
        public TouchableStackLayoutRenderer() { }

        public override bool OnTouchEvent(MotionEvent e)
        {
            bool wants = base.OnTouchEvent(e);
            //Print.Log("touch", wants, e.Action, "intercepting: " + (Element is TouchableStackLayout && (Element as TouchableStackLayout).ShouldIntercept), Element, Element?.GetType());
            
            //print.log("touch event", e.Action, Element is TouchableStacLayout && (Element as TouchableStackLayout).ShouldIntercept, test);
            if (Element is TouchableStackLayout && (Element as TouchableStackLayout).ShouldIntercept)
            {
                return this.RelayTouch(e);
                //return Element.RelayTouch(this, e);
            }
            else
            {
                return wants;
            }
        }

        public override bool OnInterceptTouchEvent(MotionEvent ev)
        {
            bool wants = base.OnInterceptTouchEvent(ev);

            if (Element is TouchableStackLayout && (Element as TouchableStackLayout).ShouldIntercept)
            {
                if (ev.Action == MotionEventActions.Move)
                {
                    return true;
                }
                else
                {
                    Parent?.RequestDisallowInterceptTouchEvent(ev.Action == MotionEventActions.Down);
                }
            }

            return wants;
        }
    }

    public class LongClickableButtonRenderer : ButtonRenderer
    {
        public LongClickableButtonRenderer() { }

        protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                //Control.SetMinWidth(0);
                //Control.SetMinimumWidth(0);
                Control.SetAllCaps(false);
                Control.LongClick += (sender, args) => (Element as LongClickableButton)?.OnLongClick();
            }
        }

        public override bool OnTouchEvent(MotionEvent e) => this.RelayTouch(e);// Element.RelayTouch(this, e);

        public override bool OnInterceptTouchEvent(MotionEvent e)
        {
            if (e.Action == MotionEventActions.Move)
            {
                return OnTouchEvent(e);
            }

            return false;
        }
    }

    public class CanvasRenderer : VisualElementRenderer<AbsoluteLayout>
    {
        public CanvasRenderer()
        {
            Touch += (sender, e) => this.RelayTouch(e.Event);// Element.RelayTouch(sender as Android.Views.View, e.Event);
            //Drag += dropEquation;
        }

        private void dropEquation(object sender, DragEventArgs e)
        {
            var evt = e.Event;

            switch (evt.Action)
            {
                case DragAction.Started:
                    /* To register your view as a potential drop zone for the current view being dragged
                     * you need to set the event as handled
                     */
                    e.Handled = true;

                    /* An important thing to know is that drop zones need to be visible (i.e. their Visibility)
                     * property set to something other than Gone or Invisible) in order to be considered. A nice workaround
                     * if you need them hidden initially is to have their layout_height set to 1.
                     */

                    break;
                case DragAction.Entered:
                case DragAction.Exited:
                    /* These two states allows you to know when the dragged view is contained atop your drop zone.
                     * Traditionally you will use that tip to display a focus ring or any other similar mechanism
                     * to advertise your view as a drop zone to the user.
                     */

                    break;
                case DragAction.Drop:
                    /* This state is used when the user drops the view on your drop zone. If you want to accept the drop,
                     *  set the Handled value to true like before.
                     */
                    e.Handled = true;
                    /* It's also probably time to get a bit of the data associated with the drag to know what
                     * you want to do with the information.
                     */

                    Print.Log("dropped");

                    break;
                case DragAction.Ended:
                    /* This is the final state, where you still have possibility to cancel the drop happened.
                     * You will generally want to set Handled to true.
                     */
                    e.Handled = true;
                    break;
            }
        }
    }

    public class BannerAdRenderer : ViewRenderer<BannerAd, AdView>
    {
        public BannerAdRenderer() { }

        //Note you may want to adjust this, see further down.
        private AdView adView;
        private AdView CreateNativeAdControl()
        {
            if (adView != null)
                return adView;

            adView = new AdView(Context);
            //adView.AdSize = AdSize.Banner;
            adView.AdSize = new AdSize(MainPage.MaxBannerWidth, (int)Math.Ceiling(MainPage.MaxBannerWidth * 50.0 / 320.0));
            adView.AdUnitId = "ca-app-pub-1795523054003202/4422905833";

            adView.LoadAd(new AdRequest.Builder().Build());

            return adView;
        }

        protected override void OnElementChanged(ElementChangedEventArgs<BannerAd> e)
        {
            base.OnElementChanged(e);
            if (Control == null)
            {
                //MainPage.LoadAd = CreateNativeAdControl;
                CreateNativeAdControl();
                SetNativeControl(adView);
            }
        }
    }
}