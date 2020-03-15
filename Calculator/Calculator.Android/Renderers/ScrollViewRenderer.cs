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

using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(Xamarin.Forms.ScrollView), typeof(Calculator.Droid.ScrollViewRenderer))]

namespace Calculator.Droid
{
    public class ScrollViewRenderer : Xamarin.Forms.Platform.Android.ScrollViewRenderer
    {
        protected Touch TouchImplementation;

        public ScrollViewRenderer(Context context) : base(context)
        {
            TouchImplementation = new Touch(context, this);
        }

        public override bool DispatchTouchEvent(MotionEvent e) => TouchImplementation.DispatchTouchEvent(e) | base.DispatchTouchEvent(e);

        public override bool OnInterceptTouchEvent(MotionEvent ev) => TouchImplementation.OnInterceptTouchEvent(ev) | base.OnInterceptTouchEvent(ev);

        public override bool OnTouchEvent(MotionEvent e)
        {
            if (e.Action == MotionEventActions.Up)
            {
                //Snap();
            }
            return TouchImplementation.OnTouchEvent(e) | base.OnTouchEvent(e);
        }

        private void Snap()
        {
            var scrollView = (Xamarin.Forms.ScrollView)Element;

            if (scrollView.IsPagingEnabled())
            {
                ScrollViewExtensions.GetPagingBehavior(scrollView).Snap();
            }
        }
    }
}