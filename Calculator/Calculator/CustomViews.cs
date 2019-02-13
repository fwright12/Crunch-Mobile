using System;
using System.Collections.Generic;
using System.Text;

using Xamarin.Forms;
using Xamarin.Forms.Extensions;

namespace Calculator
{
    public class AnythingButton : TouchableAbsoluteLayout
    {
        public event EventHandler Clicked;

        private LongClickableButton button;

        public AnythingButton()
        {
            Children.Add(button = new LongClickableButton(), new Rectangle(0, 0, 1, 1), AbsoluteLayoutFlags.All);
            button.Clicked += (sender, e) => Clicked?.Invoke(sender, e);
        }

        public void SetButtonBorderWidth(double width) => button.BorderWidth = width;
    }

    public class TextImage : AbsoluteLayout
    {
        new public double HeightRequest
        {
            set
            {
                SetLayoutBounds(image, new Rectangle(0, 0, MainPage.parenthesesWidth, value));
            }
        }

        private Image image;
        private string Text;

        public TextImage(Image image, string text)
        {
            //BackgroundColor = Color.Gainsboro;

            Text = text;
            Children.Add(this.image = image, new Rectangle(0, 0, MainPage.parenthesesWidth, 0));
        }

        public override string ToString() => Text;
    }

    /*public class StepperCell : ViewCell
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
    }*/

    public class TouchScreen : StackLayout, ITouchable
    {
        public static Point LastDownEvent;

        public event TouchEventHandler Touch;
        public event ClickEventHandler Click;
        public event TouchEventHandler InterceptedTouch;

        public void OnTouch(Point point, TouchState state)
        {
            if (state == TouchState.Up)
            {
                Drag.End();
                Click?.Invoke(point);
            }

            Touch?.Invoke(point, state);
            InterceptedTouch?.Invoke(point, state);
        }

        public void OnInterceptedTouch(Point point, TouchState state) => InterceptedTouch?.Invoke(point, state);
    }

    public class Canvas : AbsoluteLayout, ITouchable
    {
        public event TouchEventHandler Touch;
        public void OnTouch(Point point, TouchState state) => Touch?.Invoke(point, state);
    }

    public class BannerAd : View { }
}
