using System;
using System.Collections.Generic;
using System.Text;

using Xamarin.Forms;
using Xamarin.Forms.Extensions;

namespace Calculator
{
    public static class DrawerView
    {
        public class ScrollSpy
        {
            public delegate bool TouchEventHandler(Point point, TouchState state);

            public View Drawer { get; set; }
            private readonly ListView Scrollable;

            public ScrollSpy(ListView scrollable)
            {
                Drawer = Scrollable = scrollable;

                scrollable.Scrolled += (sender, e) =>
                {
                    LastScrollY = e.ScrollY;
                    //Print.Log("scrolled", e.ScrollY);

#if __IOS__
                    if (!ShouldScroll)
                    {
                        Scrollable.ScrollToPosition(0, 0);
                    }
#endif
                };
            }

            private bool ShouldScroll = false;
            private double LastTouch;
            private double LastScrollY;

            public bool OnSwipeEvent(Point point, TouchState state)
            {
                FunctionsDrawer parent = Scrollable.Parent<FunctionsDrawer>();

                double distance = state == TouchState.Down ? 0 : LastTouch - point.Y;
                //Print.Log("touch", LastScrollY, ShouldScroll, state, LastTouch, point.Y);
                //Print.Log("touch", state, distance / time, distance, time);
                LastTouch = point.Y;

                if (!ShouldScroll)
                {
                    //SortedSet<double> snapPoints = this.GetSnapPoints();
                    //HeightRequest = (HeightRequest + delta).Bound(snapPoints.Min, snapPoints.Max);
                    Drawer.HeightRequest = Math.Min(parent.MaxDrawerHeight, Drawer.HeightRequest + distance);

                    if (state == TouchState.Up)
                    {
                        double speed = parent.TransitionSpeed;

                        if (distance > 0 || (distance == 0 && Math.Abs(Drawer.Height - parent.MaxDrawerHeight) < Math.Abs(Drawer.Height - parent.Keyboard.Height)))
                        {
                            Drawer.SnapTo(parent.MaxDrawerHeight, speed);
                        }
                        else
                        {
                            Drawer.SnapTo(parent.Keyboard, speed);
                        }
                    }
                }

                ShouldScroll = Drawer.Height == parent.MaxDrawerHeight && LastScrollY >= 0;

                return !ShouldScroll;
            }
        }

        public static BindableProperty SwipeListenerProperty = BindableProperty.CreateAttached("SwipeListener", typeof(ScrollSpy), typeof(DrawerView), null, defaultValueCreator: value => new ScrollSpy((dynamic)value));

        public static ScrollSpy GetSwipeListener(this ListView listView) => (ScrollSpy)listView.GetValue(SwipeListenerProperty);

        public static void SetSwipeListener(this ListView listView, ScrollSpy listener) => listView.SetValue(SwipeListenerProperty, listener);



        public static bool IsLocked(this View bindable) => GetVisibleView(bindable) != null;

        public static void SnapTo(this View bindable, View view = null, double? animationSpeed = null) => SnapToInternal(bindable, view, null, animationSpeed);

        public static void SnapTo(this View bindable, double height, double? animationSpeed = null) => SnapToInternal(bindable, null, height, animationSpeed);

        public static void AddSnapPoint(this View bindable, params View[] views) => GetSnapPointsList(bindable).AddRange(views);
        public static void AddSnapPoint(this View bindable, params double[] heights)
        {
            List<object> snapPoints = GetSnapPointsList(bindable);
            foreach(double d in heights)
            {
                snapPoints.Add(d);
            }
        }

        public static SortedSet<double> GetSnapPoints(this View bindable)
        {
            SortedSet<double> result = new SortedSet<double>();
            
            foreach(object o in GetSnapPointsList(bindable))
            {
                result.Add(o is double ? (double)o : ((View)o).Height);
            }

            return result;
        }



        private static BindableProperty SnapPointsProperty = BindableProperty.CreateAttached("SnapPoints", typeof(List<object>), typeof(DrawerView), new List<object>());

        private static BindableProperty VisibleViewProperty = BindableProperty.CreateAttached("VisibleView", typeof(View), typeof(DrawerView), null);

        private static View GetVisibleView(BindableObject view) => (View)view.GetValue(VisibleViewProperty);

        private static void SetVisibleView(BindableObject view, View value) => view.SetValue(VisibleViewProperty, value);

        private static List<object> GetSnapPointsList(BindableObject bindable) => (List<object>)bindable.GetValue(SnapPointsProperty);

        private static readonly string MOVING_ANIMATION_HANDLE = "Move";

        private static EventHandler HeightChanged;

        private static void SnapToInternal(this VisualElement bindable, View view, double? unsafeHeight, double? animationSpeed = null)
        {
            double height = unsafeHeight ?? view?.Height ?? -1;
            Print.Log("snapping to", height, bindable.Height, animationSpeed);

            if (animationSpeed == null || bindable.Height == height)
            {
                bindable.HeightRequest = height;
            }
            else
            {
                bindable.AnimateAtSpeed(MOVING_ANIMATION_HANDLE, VisualElement.HeightRequestProperty, bindable.Height, height, 16, (double)animationSpeed, Easing.SpringOut);
            }

            View visibleView = GetVisibleView(bindable);
            if (view != visibleView)
            {
                if (visibleView != null)
                {
                    visibleView.SizeChanged -= HeightChanged;
                }

                HeightChanged = new EventHandler((sender, e) => SnapToInternal(bindable, sender as View, null));
                if (view != null)
                {
                    view.SizeChanged += HeightChanged;
                }

                SetVisibleView(bindable, view);

                //Orientation = VisibleView == null ? ScrollOrientation.Vertical : ScrollOrientation.Neither;
            }
        }

        private static double Bounce(double value)
        {
            double a = 3.5 * Math.PI * value;
            double c = Math.Abs(Math.Pow(a, 1.5)) + 1;
            return 1 - Math.Abs(Math.Cos(a) * 1 / c);
        }
    }
}
