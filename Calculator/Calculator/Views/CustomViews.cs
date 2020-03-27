using System;
using System.Collections.Generic;
using System.Text;

using Xamarin.Forms;
using Xamarin.Forms.Extensions;

namespace Calculator
{
    public class LabelButton : AnythingButton
    {
        public Label Label { get; private set; }

        public LabelButton() : base(new Button()) { }
        public LabelButton(Button button) : base(button)
        {
            Label = new Label
            {
                BindingContext = button,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                //Text = button.Text,
            };
            BindButtonTextProperties(Label);

            button.SetBinding(Button.TextColorProperty, button, "BackgroundColor");
            /*button.Bind<string>(Button.TextProperty, value =>
            {
                button.Text = "";
                if (value != null && value.Length > 0)
                {
                    Label.Text = value;
                }
            });*/
            //button.Text = "";
            Children.Add(Label, new Rectangle(0.5, 0.5, -1, -1), AbsoluteLayoutFlags.PositionProportional);
        }

        public static implicit operator LabelButton(Button button) => new LabelButton(button);

        private static void BindButtonTextProperties(Label label)
        {
            label.SetBinding(Label.TextProperty, "Text");
            label.SetBinding(Label.FontAttributesProperty, "FontAttributes");
            label.SetBinding(Label.FontFamilyProperty, "FontFamily");
            label.SetBinding(Label.FontProperty, "Font");
            label.SetBinding(Label.FontSizeProperty, "FontSize");
            //label.SetBinding(Label.TextColorProperty, "TextColor");
        }
    }

    public class AnythingButton : AbsoluteLayout
    {
        public Button Button { get; private set; }

        public AnythingButton() : this(new LongClickableButton()) { }

        protected AnythingButton(Button button)
        {
            Button = button;

            Children.Insert(0, Button);
            SetLayoutBounds(Button, new Rectangle(0, 0, 1, 1));
            SetLayoutFlags(Button, AbsoluteLayoutFlags.All);

            Button.InputTransparent = false;
        }

        public static implicit operator AnythingButton(Button button) => new AnythingButton(button);

        protected override void OnChildAdded(Element child)
        {
            base.OnChildAdded(child);

            if (child is VisualElement visualElement)
            {
                visualElement.InputTransparent = true;
            }
        }

        protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint) => Button?.Measure(widthConstraint, heightConstraint) ?? base.OnMeasure(widthConstraint, heightConstraint);
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
