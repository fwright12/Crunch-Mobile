using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using System.Extensions;
using Xamarin.Forms;
using Xamarin.Forms.Extensions;

namespace Calculator
{
    public class VariableRow : StackLayout
    {
        public static readonly BindableProperty ButtonSizeProperty = BindableProperty.Create("ButtonSize", typeof(double), typeof(VariableRow));

        //public static readonly BindableProperty ExpandedProperty = BindableProperty.Create("Expanded", typeof(bool), typeof(VariableRow));

        public double ButtonSize
        {
            get { return (double)GetValue(ButtonSizeProperty); }
            set { SetValue(ButtonSizeProperty, value); }
        }

        public bool Expanded
        {
            //get => (bool)GetValue(ExpandedProperty);
            //set => SetValue(ExpandedProperty, value);
            get => App.VariableRowExpanded.Value;
            set => App.VariableRowExpanded.Value = value;
        }

        private readonly int RecentlyUsed = 10;
        private readonly uint NumRotations = 1;
        private readonly uint TransitionLength = 750;

        private readonly StackLayout Variables;
        private readonly ScrollView Scroll;
        private readonly StackLayout Bar;
        private readonly LabelButton ExpandButton;

        public VariableRow()
        {
            Resources.Add(new Style(typeof(Button))
            {
                Setters =
                {
                    new Setter { Property = Button.FontSizeProperty, Value = 12 }
                }
            });
            this.WhenDescendantAdded<StackLayout>((stacklayout) =>
            {
                stacklayout.BindingContext = this;
                stacklayout.SetBinding(SpacingProperty, "Spacing");
                stacklayout.SetBinding(OrientationProperty, "Orientation");
            });

            LabelButton keyboard = new Button
            {
                Text = "🌐",
                FontSize = 15,
                FontFamily = CrunchStyle.SYMBOLA_FONT,
            };
            keyboard.Button.Clicked += (sender, e) =>
            {
                SoftKeyboardManager.NextKeyboard();
            };

            Bar = new StackLayout
            {
                Children =
                {
                    keyboard,
                    (Scroll = new ScrollView
                    {
                        Content = Variables = new StackLayout { },
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                        VerticalOptions = LayoutOptions.FillAndExpand,
                        WidthRequest = 0,
                        HeightRequest = 0
                    })
                }
            };
            Variables.WhenPropertyChanged(HeightProperty, (sender, e) =>
            {
                Print.Log("\n\nheight changed");
            });

            App.VariableRowExpanded.Bind<bool>(App.VariableRowExpanded.ValueProperty, value =>
            {
                this.SetValues(value ? LayoutOptions.Fill : LayoutOptions.End, HorizontalOptionsProperty, VerticalOptionsProperty);
                Bar.SetValues(value ? LayoutOptions.FillAndExpand : LayoutOptions.Fill, HorizontalOptionsProperty, VerticalOptionsProperty);
                Bar.SetValues(value ? -1 : 1, WidthRequestProperty, HeightRequestProperty);
            });
            Bar.SizeChanged += (sender, e) =>
            {
                Bar.Opacity = ((Orientation == StackOrientation.Horizontal && Bar.Width > 1) || (Orientation == StackOrientation.Vertical && Bar.Height > 1)).ToInt();
            };
            
            ExpandButton = new Button
            {
                Text = "◁",
                FontFamily = CrunchStyle.SYMBOLA_FONT,
                FontSize = 15,
            };
            ExpandButton.Button.Clicked += (sender, e) => Change();

            Children.Add(Bar);
            Children.Add(ExpandButton);

            foreach (char c in App.Variables)
            {
                Button button = new Button
                {
                    VerticalOptions = LayoutOptions.Start,
                    Text = c.ToString(),
                };
                button.Clicked += (sender, e) =>
                {
                    int index = App.Variables.IndexOf(c);

                    if (index > RecentlyUsed - 1)
                    {
                        App.Variables.RemoveAt(index);
                        App.Variables.Insert(0, c);

                        View v = Variables.Children[index];
                        Variables.Children.RemoveAt(index);
                        Variables.Children.Insert(0, v);

                        Scroll.MakeVisible(v);
                    }

                    KeyboardManager.Type(c.ToString());
                };

                Variables.Children.Add(button);
            }

            this.Bind<double>(ButtonSizeProperty, value =>
            {
                keyboard.SizeRequest(ButtonSize);

                foreach(View child in Variables.Children)
                {
                    child.SizeRequest(ButtonSize);
                }

                ExpandButton.SizeRequest(ButtonSize);
            });

            Scroll.SetBinding<ScrollOrientation, StackOrientation>(ScrollView.OrientationProperty, this, "Orientation", value => value == StackOrientation.Horizontal ? ScrollOrientation.Horizontal : ScrollOrientation.Vertical);

            this.Bind<StackOrientation>(OrientationProperty, value =>
            {
                ExpandButton.Rotation = GetExpandButtonRotation(Expanded, value);
            });
        }

        private double GetExpandButtonRotation(bool expanded, StackOrientation orientation) => (orientation == StackOrientation.Horizontal ? 0 : 90) + (expanded ? 180 + NumRotations * 360 : 0);

        private void Change()
        {            
            bool horizontal = Orientation == StackOrientation.Horizontal;
            double start = Bar.GetValue<double>(horizontal ? WidthProperty : HeightProperty);
            double end = Expanded ? 0 : (horizontal ? AvailableSpace.Width : AvailableSpace.Height) - ButtonSize - Spacing;

            // Get the rotation for the new Expanded value (which will be the opposite of what it is now)
            double rotation = GetExpandButtonRotation(!Expanded, Orientation);
            ExpandButton.RotateTo(rotation, TransitionLength, Easing.SinInOut);

            if (end == 0)
            {
                Expanded = false;
            }
            
            BindableProperty dimensionRequest = horizontal ? WidthRequestProperty : HeightRequestProperty;
            Bar.Animate("Transition", dimensionRequest, start, end, 16, TransitionLength, Easing.SinInOut, (final, cancelled) =>
            {
                if (end > 0)
                {
                    Expanded = true;
                }
            });
        }

        private Size AvailableSpace;

        protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
        {
            AvailableSpace = new Size(widthConstraint, heightConstraint);
            return base.OnMeasure(widthConstraint, heightConstraint);
        }
    }
}