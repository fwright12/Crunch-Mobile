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




        private static BindableProperty VisibleViewProperty = BindableProperty.CreateAttached("VisibleView", typeof(View), typeof(DrawerView), null);

        private static View GetVisibleView(BindableObject view) => (View)view.GetValue(VisibleViewProperty);

        private static void SetVisibleView(BindableObject view, View value) => view.SetValue(VisibleViewProperty, value);

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
                bindable.AnimateAtSpeed(MOVING_ANIMATION_HANDLE, VisualElement.HeightRequestProperty, bindable.Height, height, 16, (double)animationSpeed, Easing.BounceOut);
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
    }
}
