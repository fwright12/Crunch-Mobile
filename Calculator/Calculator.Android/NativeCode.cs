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

[assembly: ExportRenderer(typeof(GestureRelativeLayout), typeof(AndroidRelativeLayoutRenderer))]
[assembly: ExportRenderer(typeof(LongClickableButton), typeof(LongClickableButtonRenderer))]
[assembly: ExportRenderer(typeof(Mask), typeof(MaskRenderer))]
[assembly: ExportRenderer(typeof(ScrollSpy), typeof(ScrollSpyRenderer))]
[assembly: ExportRenderer(typeof(Answer), typeof(TouchEnabledViewRenderer))]
[assembly: ExportRenderer(typeof(DockButton), typeof(DockButtonRenderer))]
//[assembly: ExportRenderer(typeof(BannerAd), typeof(BannerAdRenderer))]

namespace Calculator.Droid
{
    /*public class BannerAdRenderer : ViewRenderer
    {
        string adUnitId = string.Empty;
        //Note you may want to adjust this, see further down.
        AdSize adSize = AdSize.SmartBanner;
        AdView adView;
        AdView CreateNativeAdControl()
        {
            if (adView != null)
                return adView;

            // This is a string in the Resources/values/strings.xml that I added or you can modify it here. This comes from admob and contains a / in it
            adUnitId = Forms.Context.Resources.GetString(Resource.String.banner_ad_unit_id);
            adView = new AdView(Forms.Context);
            adView.AdSize = adSize;
            adView.AdUnitId = adUnitId;

            var adParams = new LinearLayout.LayoutParams(LayoutParams.WrapContent, LayoutParams.WrapContent);

            adView.LayoutParameters = adParams;

            adView.LoadAd(new AdRequest
                            .Builder()
                            .Build());
            return adView;
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Controls.AdControlView> e)
        {
            base.OnElementChanged(e);
            if (Control == null)
            {
                CreateNativeAdControl();
                SetNativeControl(adView);
            }
        }
    }*/

    /*public class BannerAdRenderer : ViewRenderer
    {
        /// <summary>
        /// Used for registration with dependency service
        /// </summary>
        public static void Init() { }

        /// <summary>
        /// reload the view and hit up google admob 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.View> e)
        {
            base.OnElementChanged(e);

            //convert the element to the control we want
            var adMobElement = Element as BannerAdRenderer;

            if ((adMobElement != null) && (e.OldElement == null))
            {
                BannerAd ad = new BannerAd(Context);
                ad.adSize = AdSize.Banner;
                ad.AdUnitId = adMobElement.AdUnitId;
                var requestbuilder = new AdRequest.Builder();
                ad.LoadAd(requestbuilder.Build());
                this.SetNativeControl(ad);
            }
        }
    }*/

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

    public class TouchEnabledViewRenderer : VisualElementRenderer<Xamarin.Forms.View>
    {
        public TouchEnabledViewRenderer()
        {
            Touch += (sender, e) =>
            {
                if (e.Event.Action == MotionEventActions.Up)
                {
                    Input.ViewTouched(Element as Answer);
                }
            };
        }
    }

    public class ScrollSpyRenderer : VisualElementRenderer<AbsoluteLayout>
    {
        public override bool OnTouchEvent(MotionEvent e)
        {
            return false;
        }

        public override bool OnInterceptTouchEvent(MotionEvent ev)
        {
            if (ev.Action == MotionEventActions.Down)
            {
                MainPage.isTouchingCanvas = true;
            }
            else if (ev.Action == MotionEventActions.Up)
            {
                MainPage.isTouchingCanvas = false;
            }

            return false;
        }
    }

    public class LongClickableButtonRenderer : ButtonRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {                
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

    public class AndroidRelativeLayoutRenderer : VisualElementRenderer<Xamarin.Forms.RelativeLayout>
    {
        public AndroidRelativeLayoutRenderer()
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