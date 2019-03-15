using System;
using System.Collections.Generic;
using System.Text;

using Xamarin.Forms;
using Xamarin.Forms.Extensions;

namespace Calculator
{
    public class AnythingButton : AbsoluteLayout
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

    public class TouchScreen : StackLayout, ITouchable
    {
        public event TouchEventHandler Touch;
        public event ClickEventHandler Click;
        public event TouchEventHandler InterceptedTouch;

        private static TouchScreen Instance;

        public TouchScreen()
        {
            if (Instance == null)
            {
                Instance = this;
                Drag.Screen = this;
            }
            else
            {
                throw new Exception("There can only be one instance of TouchScreen");
            }
        }

        public void OnTouch(Point point, TouchState state)
        {
            if (state == TouchState.Up)
            {
                //Drag.End();
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
