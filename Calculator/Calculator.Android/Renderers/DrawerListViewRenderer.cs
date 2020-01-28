using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Interop;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(Calculator.FunctionsDrawer.ListView), typeof(Calculator.Droid.DrawerListViewRenderer))]

namespace Calculator.Droid
{
    public class DrawerListViewRenderer : ListViewRenderer
    {
        public DrawerListViewRenderer(Context context) : base(context) { }

        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.ListView> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement is FunctionsDrawer.ListView oldElement)
            {
                oldElement.ScrollToRequest -= ScrollToRequest;
            }
            if (e.NewElement is FunctionsDrawer.ListView newElement)
            {
                newElement.ScrollToRequest += ScrollToRequest;
            }
            
            return;

            Control.ScrollStateChanged += (sender, e1) =>
            {
                Print.Log("scroll state changed", e1.ScrollState);
            };
            Control.ScrollChange += (sender, e1) =>
            {
                Print.Log("scroll change", e1.OldScrollY, e1.ScrollY);
            };
        }

        private void ScrollToRequest(object sender, FunctionsDrawer.ListView.ScrollToRequestEventArgs e) => ScrollTo(e.X, e.Y);

        public override bool DispatchTouchEvent(MotionEvent e)
        {
            return !((Element as FunctionsDrawer.ListView)?.OnScrollEvent(new Point(e.RawX, e.RawY), e.Action.Convert()) ?? true) && base.DispatchTouchEvent(e);
        }

        protected override bool OverScrollBy(int deltaX, int deltaY, int scrollX, int scrollY, int scrollRangeX, int scrollRangeY, int maxOverScrollX, int maxOverScrollY, bool isTouchEvent)
        {
            Print.Log("over scroll by", deltaY, scrollY, scrollRangeY, maxOverScrollY, isTouchEvent);
            return base.OverScrollBy(deltaX, deltaY, scrollX, scrollY, scrollRangeX, scrollRangeY, maxOverScrollX, maxOverScrollY, isTouchEvent);
        }

        protected override void OnOverScrolled(int scrollX, int scrollY, bool clampedX, bool clampedY)
        {
            Print.Log("over scrolled", scrollY, clampedY);
            base.OnOverScrolled(scrollX, scrollY, clampedX, clampedY);
        }

        protected override void InitializeScrollbars(Android.Content.Res.TypedArray a)
        {
            Print.Log("initializing scrollbars");
            base.InitializeScrollbars(a);
        }

        protected override int ComputeVerticalScrollOffset()
        {
            int ans = base.ComputeVerticalScrollOffset();
            Print.Log("vertical scroll offset", ans);
            return ans;
        }

        protected override bool AwakenScrollBars()
        {
            bool ans = base.AwakenScrollBars();
            Print.Log("awaken scroll bars?", ans);
            return ans;
        }

        protected override int ComputeVerticalScrollExtent()
        {
            int ans = base.ComputeVerticalScrollExtent();
            Print.Log("vertical scroll extent", ans);
            return ans;
        }

        protected override int ComputeVerticalScrollRange()
        {
            int ans = base.ComputeVerticalScrollRange();
            Print.Log("vertical scroll range", ans);
            return ans;
        }

        public override void ScrollTo(int x, int y)
        {
            Print.Log("scrolling to", x, y);
            base.ScrollTo(x, y);
        }

        protected override void OnScrollChanged(int l, int t, int oldl, int oldt)
        {
            base.OnScrollChanged(l, t, oldl, oldt);
            Print.Log("scroll changed", l, t, oldl, oldt);
        }
    }
}