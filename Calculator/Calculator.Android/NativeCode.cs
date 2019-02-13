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

using Crunch.GraphX;
using Android.Gms.Ads;

[assembly: ExportRenderer(typeof(Calculator.Canvas), typeof(CanvasRenderer))]
[assembly: ExportRenderer(typeof(LongClickableButton), typeof(LongClickableButtonRenderer))]
[assembly: ExportRenderer(typeof(Answer), typeof(StackLayoutRenderer))]
[assembly: ExportRenderer(typeof(DockButton), typeof(DockButtonRenderer))]
[assembly: ExportRenderer(typeof(BannerAd), typeof(BannerAdRenderer))]
[assembly: ExportRenderer(typeof(Text), typeof(TextRenderer))]
[assembly: ExportRenderer(typeof(TouchScreen), typeof(TouchScreenRenderer))]

namespace Calculator.Droid
{
    public static class ExtensionMethods
    {
        public static void RelayTouch(this Xamarin.Forms.View shared, Android.Views.View native, MotionEvent e)
        {
            shared.TryToTouch(native.ScaleTouch(shared, e), (int)e.Action);
        }

        public static Xamarin.Forms.Point ScaleTouch(this Android.Views.View native, Xamarin.Forms.View shared, MotionEvent e)
        {
            return new Xamarin.Forms.Point(shared.Width * e.GetX() / native.Width, shared.Height * e.GetY() / native.Height);
        }
    }

    public class TouchScreenRenderer : VisualElementRenderer<StackLayout>
    {
        public override bool OnTouchEvent(MotionEvent e)
        {
            Element.RelayTouch(this, e);
            return true;
        }

        public override bool OnInterceptTouchEvent(MotionEvent e)
        {
            if (e.Action == MotionEventActions.Down)
            {
                TouchScreen.LastDownEvent = this.ScaleTouch(Element, e);
            }
            if (Xamarin.Forms.Drag.ShouldIntercept)
            {
                OnTouchEvent(e);
            }
            return Xamarin.Forms.Drag.ShouldIntercept;
        }
    }

    public class TextRenderer : LabelRenderer
    {
        public TextRenderer()
        {
            Touch += (sender, e) => Element.RelayTouch(sender as Android.Views.View, e.Event);
        }
    }

    public class DockButtonRenderer : ButtonRenderer
    {
        public override bool OnTouchEvent(MotionEvent e)
        {
            Element.RelayTouch(this, e);
            return true;
        }

        public override bool OnInterceptTouchEvent(MotionEvent e)
        {
            if (e.Action == MotionEventActions.Move)
            {
                return OnTouchEvent(e);
            }

            return false;
        }
    }

    public class StackLayoutRenderer : VisualElementRenderer<StackLayout>
    {
        public StackLayoutRenderer()
        {
            Touch += (sender, e) => Element.RelayTouch(sender as Android.Views.View, e.Event);
        }

        public override bool OnInterceptTouchEvent(MotionEvent ev)
        {
            return true;
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
                Control.LongClick += (sender, args) => (Element as LongClickableButton).OnLongClick();
            }
        }
    }

    public class CanvasRenderer : VisualElementRenderer<AbsoluteLayout>
    {
        public CanvasRenderer()
        {
            Touch += (sender, e) => Element.RelayTouch(sender as Android.Views.View, e.Event);
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