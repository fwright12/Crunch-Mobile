using System;
using System.Collections.Generic;
using System.Text;

using Xamarin.Forms;
using Xamarin.Forms.Extensions;

namespace Calculator
{
    public class LabelButton : AnythingButton
    {
        public Label Text { get; private set; }

        public LabelButton(string text = "")
        {
            Children.Add(Button = new LongClickableButton());
            Children.Add(Text = new Label
            {
                Text = text,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
            }, new Rectangle(0.5, 0.5, 1, 1), AbsoluteLayoutFlags.All);
            Button.InputTransparent = false;
        }
    }

    public class AnythingButton : AbsoluteLayout
    {
        public LongClickableButton Button { get; protected set; }

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
    }

    public class TextImage : AbsoluteLayout, Xamarin.Forms.MathDisplay.IMathView
    {
        new public double HeightRequest
        {
            set
            {
                SetLayoutBounds(image, new Rectangle(0, 0, MainPage.ParenthesesWidth, value));
            }
        }

        public double Middle => 0.5;

        public double FontSize { set { } }

        private Image image;
        private string Text;

        public TextImage(Image image, string text)
        {
            //BackgroundColor = Color.Gainsboro;

            Text = text;
            Children.Add(this.image = image, new Rectangle(0, 0, MainPage.ParenthesesWidth, 0));
        }

        public override string ToString() => Text;

        public string ToLatex() => ToString();
    }

    public class Canvas : AbsoluteLayout, ITouchable
    {
        public event EventHandler<TouchEventArgs> Touch;
        public bool ShouldIntercept => Touch != null;

        public void OnTouch(Point point, TouchState state) => Touch?.Invoke(this, new TouchEventArgs(point, state));
    }

    public class BannerAd : View { }
}
