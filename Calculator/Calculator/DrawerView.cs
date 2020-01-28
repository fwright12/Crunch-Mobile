using System;
using System.Collections.Generic;
using System.Text;

using Xamarin.Forms;
using Xamarin.Forms.Extensions;

namespace Calculator
{
    public static class DrawerView
    {
        public static bool IsLocked(this ScrollView bindable) => GetVisibleView(bindable) != null;
        public static bool IsLocked(this ListView bindable) => GetVisibleView(bindable) != null;
        public static bool IsLocked(this TableView bindable) => GetVisibleView(bindable) != null;

        public static void SnapTo(this ScrollView bindable, View view = null, double? animationSpeed = null) => SnapToInternal(bindable, view, null, animationSpeed);
        public static void SnapTo(this ListView bindable, View view = null, double? animationSpeed = null) => SnapToInternal(bindable, view, null, animationSpeed);
        public static void SnapTo(this TableView bindable, View view = null, double? animationSpeed = null) => SnapToInternal(bindable, view, null, animationSpeed);

        public static void SnapTo(this ScrollView bindable, double height, double? animationSpeed = null) => SnapToInternal(bindable, null, height, animationSpeed);
        public static void SnapTo(this ListView bindable, double height, double? animationSpeed = null) => SnapToInternal(bindable, null, height, animationSpeed);
        public static void SnapTo(this TableView bindable, double height, double? animationSpeed = null) => SnapToInternal(bindable, null, height, animationSpeed);

        public static void AddSnapPoint(this ListView bindable, params View[] views) => GetSnapPointsList(bindable).AddRange(views);
        public static void AddSnapPoint(this ListView bindable, params double[] heights)
        {
            List<object> snapPoints = GetSnapPointsList(bindable);
            foreach(double d in heights)
            {
                snapPoints.Add(d);
            }
        }

        public static SortedSet<double> GetSnapPoints(this ListView bindable)
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

            if (animationSpeed == null)
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
