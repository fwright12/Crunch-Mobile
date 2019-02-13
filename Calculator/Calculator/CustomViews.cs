using System;
using System.Collections.Generic;
using System.Text;

using Xamarin.Forms;
using Xamarin.Forms.Extensions;

namespace Calculator
{
    public class StepperCell : ViewCell
    {
        public static readonly BindableProperty TextProperty = BindableProperty.Create("Text", typeof(string), typeof(StepperCell), propertyChanged: (bindable, old, value) => (bindable as StepperCell).text.Text = value.ToString());

        public static readonly BindableProperty ValueProperty = BindableProperty.Create("Value", typeof(string), typeof(StepperCell), propertyChanged: (bindable, old, value) => (bindable as StepperCell).stepper.Value = double.Parse(value.ToString()));
        public static readonly BindableProperty MinimumProperty = BindableProperty.Create("Minimum", typeof(string), typeof(StepperCell), propertyChanged: (bindable, old, value) => (bindable as StepperCell).stepper.Minimum = double.Parse(value.ToString()));
        public static readonly BindableProperty MaximumProperty = BindableProperty.Create("Maximum", typeof(string), typeof(StepperCell), propertyChanged: (bindable, old, value) => (bindable as StepperCell).stepper.Maximum = double.Parse(value.ToString()));
        public static readonly BindableProperty IncrementProperty = BindableProperty.Create("Increment", typeof(string), typeof(StepperCell), propertyChanged: (bindable, old, value) => (bindable as StepperCell).stepper.Increment = double.Parse(value.ToString()));

        public static readonly BindableProperty DisplayValueProperty = BindableProperty.Create("ShowValue", typeof(string), typeof(StepperCell), propertyChanged: (bindable, old, value) => (bindable as StepperCell).value.IsVisible = (bool)value);

        public string Text { get; set; }

        public string Value { get; set; }
        public string Minimum { get; set; }
        public string Maximum { get; set; }
        public string Increment { get; set; }

        public bool ShowValue { get; set; }

        public EventHandler<ValueChangedEventArgs> ValueChanged
        {
            set { stepper.ValueChanged += value; }
        }

        private Label text;
        private Label value;
        private Stepper stepper;

        public StepperCell()
        {
            StackLayout layout = new StackLayout { Orientation = StackOrientation.Horizontal, Padding = new Thickness(25, 0, 0, 0) };
            layout.Children.Add(text = new Label { VerticalOptions = LayoutOptions.Center });
            layout.Children.Add(value = new Label { VerticalOptions = LayoutOptions.Center });
            layout.Children.Add(stepper = new Stepper { VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.EndAndExpand });

            stepper.ValueChanged += (sender, e) => value.Text = e.NewValue.ToString();
            
            View = layout;
        }
    }

    public class TouchScreen : StackLayout, ITouchable
    {
        public static Point LastDownEvent;

        public event TouchEventHandler Touch;
        public void OnTouch(Point point, TouchState state)
        {
            if (state == TouchState.Up)
            {
                Drag.End();
            }

            Touch?.Invoke(point, state);
        }
    }

    public class Canvas : AbsoluteLayout, ITouchable
    {
        public event TouchEventHandler Touch;
        public void OnTouch(Point point, TouchState state) => Touch?.Invoke(point, state);
    }

    public class LongClickableButton : Button
    {
        public event LongClickEventHandler LongClick;
        public void OnLongClick() => LongClick?.Invoke();
    }

    public class DockButton : Button, ITouchable
    {
        public event TouchEventHandler Touch;
        public void OnTouch(Point point, TouchState state) => Touch?.Invoke(point, state);
    }

    public class BannerAd : View { }
}
