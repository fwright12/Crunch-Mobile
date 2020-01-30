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

[assembly: ExportRenderer(typeof(Xamarin.Forms.ListView), typeof(Calculator.Droid.DrawerListViewRenderer))]

namespace Calculator.Droid
{
    public class DrawerListViewRenderer : ListViewRenderer
    {
        public DrawerListViewRenderer(Context context) : base(context) { }

        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.ListView> e)
        {
            base.OnElementChanged(e);

            Scrollable.ScrollRequestProperty.ListenFor(ScrollToRequest, e.OldElement, e.NewElement);

            if (e.NewElement is Element)
            {
                Control.ScrollChange += (sender, e1) =>
                {
                    if (!ScrollStarted)
                    {
                        //Control.SmoothScrollToPosition(0);
                    }
                    ScrollStarted = true;
                };
            }

            return;

            Control.ScrollStateChanged += (sender, e1) =>
            {
                Print.Log("scroll state changed", e1.ScrollState);
            };
        }

        private bool ScrollStarted = false;

        private void ScrollToRequest(object sender, ScrollToPositionRequestEventArgs e)
        {
            Control.SmoothScrollToPosition(0);
        }

        public override bool DispatchTouchEvent(MotionEvent e)
        {
            if (e.Action == MotionEventActions.Up)
            {
                ScrollStarted = false;
            }

            //return base.DispatchTouchEvent(e);
            //bool block = ScrollStarted && !((Element as IScrollable)?.OnScrollEvent(new Point(e.RawX, e.RawY), e.Action.Convert()) ?? true);

            bool block = ScrollStarted && ((Element as Xamarin.Forms.ListView)?.GetSwipeListener().OnSwipeEvent(new Point(e.RawX, e.RawY), e.Action.Convert()) ?? false);

            return block || base.DispatchTouchEvent(e);
        }
    }
}