using Android.Content;
using Android.Views;
using Xamarin.Forms;

[assembly: ExportRenderer(typeof(ScrollView), typeof(Calculator.Droid.ScrollViewRenderer))]

namespace Calculator.Droid
{
    public class ScrollViewRenderer : Xamarin.Forms.Platform.Android.ScrollViewRenderer
    {
        protected Touch TouchImplementation;

        public ScrollViewRenderer(Context context) : base(context)
        {
            TouchImplementation = new Touch(context, this);
        }

        public override bool DispatchTouchEvent(MotionEvent e) => !(Element is ScrollView scrollView && scrollView.GetIsScrollEnabled()) && e.Action == MotionEventActions.Move ? false : TouchImplementation.DispatchTouchEvent(e) | base.DispatchTouchEvent(e);

        public override bool OnInterceptTouchEvent(MotionEvent ev) => TouchImplementation.OnInterceptTouchEvent(ev) | base.OnInterceptTouchEvent(ev);

        public override bool OnTouchEvent(MotionEvent e)
        {
            if (!Element.IsEnabled)
            {
                //Print.Log("here", ChildCount);
                //return false;
            }
            
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