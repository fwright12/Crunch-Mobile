using System;
using System.Collections.Generic;
using System.Text;

using Xamarin.Forms;
using Xamarin.Forms.Extensions;

namespace Calculator
{
    public class AnythingButton : AbsoluteLayout
    {
        public LongClickableButton Button { get; private set; }

        public AnythingButton()
        {
            Children.Add(Button = new LongClickableButton(), new Rectangle(0, 0, 1, 1), AbsoluteLayoutFlags.All);
            //Button.Clicked += (sender, e) => Clicked?.Invoke(sender, e);
            Button.InputTransparent = false;
        }

        protected override void OnChildAdded(Element child)
        {
            base.OnChildAdded(child);

            if (child is VisualElement)
            {
                (child as VisualElement).InputTransparent = true;
            }
        }

        public void SetButtonBorderWidth(double width) => Button.BorderWidth = width;
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

    public class Canvas : AbsoluteLayout, ITouchable
    {
        public event TouchEventHandler Touch;
        public bool ShouldIntercept => Touch != null;

        public void OnTouch(Point point, TouchState state) => Touch?.Invoke(point, state);
    }

    public class BannerAd : View { }
}
