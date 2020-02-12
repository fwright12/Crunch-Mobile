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

            e.NewElement.SetNativeImplementation(Scrollable.NativeScrollImplementationProperty, ScrollToRequest);
            //Scrollable.ScrollRequestProperty.ListenFor(ScrollToRequest, e.OldElement, e.NewElement);
            
            Control.ScrollChange += (sender, e1) =>
            {
                if (!ScrollStarted)
                {
                    //Control.SmoothScrollToPosition(0);
                }
                ScrollStarted = true;
            };
            /*MainActivity.ContextMenuAppeared += (sender, e1) =>
            {
                Print.Log("context menu appeared");
                (Element as FunctionsDrawer.ListView).ContextActionsShowing = true;
            };*/
        }

        private bool ScrollStarted = false;

        private void ScrollToRequest(ScrollToPositionRequestEventArgs e)
        {
            Control.SmoothScrollToPosition(0);
        }

        public override bool DispatchTouchEvent(MotionEvent e)
        {
            bool block = (e.Action == MotionEventActions.Down || ScrollStarted) && ((Element as Xamarin.Forms.ListView)?.GetSwipeListener().OnSwipeEvent(new Point(e.RawX, e.RawY), e.Action.Convert()) ?? false);

            if (e.Action != MotionEventActions.Move)
            {
                ScrollStarted = false;
            }

            if (e.Action == MotionEventActions.Down || !block)
            {
                block = base.DispatchTouchEvent(e);
            }

            return block;
        }
    }
}