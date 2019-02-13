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

using Crunch.GraFX;
using Android.Gms.Ads;

[assembly: ExportRenderer(typeof(Calculator.Canvas), typeof(CanvasRenderer))]
[assembly: ExportRenderer(typeof(LongClickableButton), typeof(LongClickableButtonRenderer))]
[assembly: ExportRenderer(typeof(Mask), typeof(MaskRenderer))]
[assembly: ExportRenderer(typeof(ScrollSpy), typeof(ScrollSpyRenderer))]
[assembly: ExportRenderer(typeof(Answer), typeof(TouchEnabledLayoutRenderer))]
[assembly: ExportRenderer(typeof(DockButton), typeof(DockButtonRenderer))]
[assembly: ExportRenderer(typeof(BannerAd), typeof(BannerAdRenderer))]
[assembly: ExportRenderer(typeof(EqualSign), typeof(TextRenderer))]
[assembly: ExportRenderer(typeof(TouchSpy), typeof(LayoutRenderer))]

namespace Calculator.Droid
{
    public static class ExtensionMethods
    {
        public static Xamarin.Forms.Point GetPoint(this Android.Views.View view, MotionEvent e)
        {
            //print.log(new Xamarin.Forms.Point(e.RawX / view.Width, e.RawY / view.Height));
            return new Xamarin.Forms.Point(e.RawX / view.Width, e.RawY / view.Height);
        }
    }

    public class LayoutRenderer : VisualElementRenderer<StackLayout>
    {

        public override bool OnTouchEvent(MotionEvent e)
        {
            (Element as TouchSpy).Touched(RootView.GetPoint(e));
            //Calculator.Drag.UpdatePosition(new Xamarin.Forms.Point(0.5, 0.1));
            
            if (e.Action == MotionEventActions.Up)
            {
                Calculator.Drag.EndDrag();
            }

            return Calculator.Drag.ShouldIntercept;
        }

        public override bool OnInterceptTouchEvent(MotionEvent e)
        {
            return Calculator.Drag.ShouldIntercept;
        }
    }

    public class TextRenderer : LabelRenderer
    {
        public override bool OnTouchEvent(MotionEvent e)
        {
            //var data = Android.Content.ClipData.NewPlainText("canvas", temp.Text);
            //StartDrag(null, new DragShadowBuilder(this), null, 0);

            (Element as EqualSign).Touched(RootView.GetPoint(e));

            if (e.Action == MotionEventActions.Down)
            {
                //RelativeLayout.SetXConstraint(Equation.Focus, Constraint.Constant(0));
                //Calculator.Drag.Touch = Xamarin.Forms.Point.Zero;
                Calculator.Drag.StartDrag();
                //ScrollSpyRenderer.OnTouch = (point) => (Element as EqualSign).Touched(point);
            }

            //(Element as EqualSign).Touched(new Xamarin.Forms.Point());

            return true;
        }
    }

    public class BannerAdRenderer : ViewRenderer<BannerAd, AdView>
    {
        //Note you may want to adjust this, see further down.
        AdView adView;
        AdView CreateNativeAdControl()
        {
            if (adView != null)
                return adView;

            adView = new AdView(Context);
            adView.AdSize = AdSize.Banner;
            adView.AdUnitId = "ca-app-pub-1795523054003202/4422905833";

            //var adParams = new Android.Widget.LinearLayout.LayoutParams(LayoutParams.WrapContent, LayoutParams.WrapContent);

            //adView.LayoutParameters = adParams;

            adView.LoadAd(new AdRequest.Builder().Build());
            return adView;
        }

        protected override void OnElementChanged(ElementChangedEventArgs<BannerAd> e)
        {
            base.OnElementChanged(e);
            if (Control == null)
            {
                CreateNativeAdControl();
                SetNativeControl(adView);
            }
        }
    }

    public class DockButtonRenderer : ButtonRenderer
    {
        public override bool OnTouchEvent(MotionEvent e)
        {
            Input.MoveKeyboard(new Xamarin.Forms.Point(e.RawX / RootView.Width, e.RawY / RootView.Height));

            return false;
        }

        public override bool OnInterceptTouchEvent(MotionEvent ev)
        {
            if (ev.Action == MotionEventActions.Move)
            {
                Input.UndockKeyboard(new Xamarin.Forms.Point(ev.RawX / RootView.Width, ev.RawY / RootView.Height));
                return true;
            }

            return false;
        }
    }

    public class TouchEnabledLayoutRenderer : VisualElementRenderer<StackLayout>
    {
        public TouchEnabledLayoutRenderer()
        {
            Touch += (sender, e) =>
            {
                if (e.Event.Action == MotionEventActions.Up)
                {
                    (Element as Answer).Touched();
                }
            };
        }
    }

    public class ScrollSpyRenderer : VisualElementRenderer<AbsoluteLayout>
    {
        public static Action<Xamarin.Forms.Point> OnTouch = null;

        public override bool OnTouchEvent(MotionEvent e)
        {
            OnTouch(new Xamarin.Forms.Point(e.GetX() / Width, e.GetY() / Height));
            
            if (e.Action == MotionEventActions.Up)
            {
                OnTouch = null;
            }

            return OnTouch != null;
        }

        public override bool OnInterceptTouchEvent(MotionEvent e)
        {
            if (e.Action == MotionEventActions.Down)
            {
                MainPage.isTouchingCanvas = true;
            }
            else if (e.Action == MotionEventActions.Up)
            {
                MainPage.isTouchingCanvas = false;
            }

            return OnTouch != null;
        }
    }

    public class LongClickableButtonRenderer : ButtonRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                Control.SetAllCaps(false);
                Control.LongClick += (sender, args) => Input.LongClickDown(Element, MaskRenderer.point, true);
            }
        }
    }

    public class MaskRenderer : VisualElementRenderer<StackLayout>
    {
        public static Xamarin.Forms.Point point;

        public override bool OnTouchEvent(MotionEvent e)
        {
            if (e.Action == MotionEventActions.Move)
            {
                Input.MoveCursor(new Xamarin.Forms.Point(e.RawX / RootView.Width, e.RawY / RootView.Height));
                //Input.MoveCursor(new Xamarin.Forms.Point(e.GetX() / Width, e.GetY() / Height));
            }
            else if (e.Action == MotionEventActions.Up)
            {
                Input.LongClickDown(null, Xamarin.Forms.Point.Zero, false);
            }

            return false;
        }

        public override bool OnInterceptTouchEvent(MotionEvent ev)
        {
            point = new Xamarin.Forms.Point(ev.RawX / RootView.Width, ev.RawY / RootView.Height);

            if (MainPage.IsInCursorMode)
            {
                if (ev.Action == MotionEventActions.Move)
                {
                    return true;
                }
                else if (ev.Action == MotionEventActions.Up)
                {
                    Input.LongClickDown(null, Xamarin.Forms.Point.Zero, false);
                }
            }

            return false;
        }
    }

    public class CanvasRenderer : VisualElementRenderer<Xamarin.Forms.RelativeLayout>
    {
        public CanvasRenderer()
        {
            Touch += (sender, e) =>
            {
                if (e.Event.Action == MotionEventActions.Up)
                {
                    print.log(e.Event.GetX(), e.Event.GetY(), e.Event.RawX, e.Event.RawY);
                    Input.CanvasTouch(new Xamarin.Forms.Point(e.Event.GetX() / (sender as Android.Views.View).Width, e.Event.GetY() / (sender as Android.Views.View).Height));
                    //Input.CanvasTouch(new Point(e.Event.GetX(), e.Event.GetY()));
                }
            };

            Drag += dropEquation;
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

                    print.log("dropped");

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

    /*public class SoftkeyboardDisabledEntryRenderer : EntryRenderer
    {
        public SoftkeyboardDisabledEntryRenderer()
        {
            //LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.MatchParent);
            //temp.TextSize = Control.TextSize;
            //Control.SetPadding(1, 0, 1, 0);
            //Control.Background = null;
            //Control.SetCursorVisible(true);
            //temp.Gravity = GravityFlags.Center;

            //Control.ViewAttachedToWindow += delegate { Control.RequestFocus(); };
        }

        private void focus()
        {
            Control.RequestFocus();
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);
            Input.Focus = focus;
            //Control.RequestFocus();
            print.log("here");
            Control.SetPadding(1, 0, 1, 0);
            Control.Background = null;
            Control.SetCursorVisible(true);

            return;

            if (e.NewElement != null)
            {
                ((SoftKeyboardDisabledEntry)e.NewElement).PropertyChanging += OnPropertyChanging;
            }

            if (e.OldElement != null)
            {
                ((SoftKeyboardDisabledEntry)e.OldElement).PropertyChanging -= OnPropertyChanging;
            }

            if (Control != null)
            {
                //Control.SetPadding(1, 0, 0, 0);
                //Control.Background = null;
                //Control.SetBackgroundColor(Android.Graphics.Color.Transparent);
                //Control.SetCursorVisible(true);
            }

            // Disable the Keyboard on Focus
            Control.ShowSoftInputOnFocus = false;
        }

        private void OnPropertyChanging(object sender, PropertyChangingEventArgs propertyChangingEventArgs)
        {
            // Check if the view is about to get Focus
            if (propertyChangingEventArgs.PropertyName == VisualElement.IsFocusedProperty.PropertyName)
            {
                // in case if the focus was moved from another Entry
                // Forcefully dismiss the Keyboard 
                //InputMethodManager imm = (InputMethodManager)this.Context.GetSystemService(Android.Content.Context.InputMethodService);
                //imm.HideSoftInputFromWindow(this.Control.WindowToken, 0);
            }
        }
    }*/
}