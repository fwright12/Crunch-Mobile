using System;
using System.Collections.Generic;
using System.Text;

using Xamarin.Forms;
using Xamarin.Forms.Extensions;

namespace Calculator
{
    public partial class MainPage
    {
        private StackLayout Controls;
        private int step = 0;
        private Canvas canvasBackup;
        private Calculation calculationBackup;

        public void Tutorial()
        {
            Settings.Tutorial = true;

            canvasBackup = canvas;
            canvasScroll.Content = canvas = new Canvas() { HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.FillAndExpand };
            canvas.Touch += AddCalculation;

            calculationBackup = CalculationFocus;
            phantomCursorField.HeightRequest = 1;

            Action[] order = new Action[] { Welcome, ExplainCanvas, ExplainAnswerFormats, ExplainKeyboard, End };

            Button back = new Button() { Text = "Back", HorizontalOptions = LayoutOptions.StartAndExpand };
            Button next = new Button() { HorizontalOptions = LayoutOptions.EndAndExpand };

            Action<int> change = (i) =>
            {
                if (i >= 0 && i < order.Length)
                {
                    canvas.Children.Clear();
                    KeyboardView.ClearOverlay();

                    KeyboardView.MaskKeys(true);
                    if (Device.Idiom == TargetIdiom.Tablet)
                    {
                        KeyboardView.IsVisible = false;
                    }

                    back.IsEnabled = i > 0;
                    next.Text = i == order.Length - 2 ? "Let's Go!" : "Next";

                    order[step = i]();
                }
            };

            back.Clicked += (sender, e) => change(step - 1);
            next.Clicked += (sender, e) => change(step + 1);

            canvas.SizeChanged += PreventScroll;
            canvasScroll.SizeChanged += PreventScroll;

            Controls = new StackLayout() { Orientation = StackOrientation.Horizontal };
            Controls.Children.Add(back);
            Controls.Children.Add(next);

            page.Children.Insert(1, Controls);

            if (Device.Idiom == TargetIdiom.Phone)
            {
                Controls.HorizontalOptions = LayoutOptions.FillAndExpand;
            }
            else if (Device.Idiom == TargetIdiom.Tablet)
            {
                page.Padding = new Thickness(0, 50, 0, 0);

                Controls.HorizontalOptions = LayoutOptions.Center;
            }

            change(0);
        }

        private void PreventScroll(object sender, EventArgs e)
        {
            canvas.WidthRequest = canvasScroll.Width;
            canvas.HeightRequest = canvasScroll.Height;
        }

        private void End()
        {
            KeyboardView.MaskKeys(false);
            KeyboardView.IsVisible = true;

            canvas.SizeChanged -= PreventScroll;
            canvasScroll.SizeChanged -= PreventScroll;

            Controls.Remove();
            page.Padding = new Thickness(0, 0, 0, 0);

            ClearCanvas();
            canvas = canvasBackup;
            canvasScroll.Content = canvas;

            FocusOnCalculation(calculationBackup);

            Settings.Tutorial = false;
        }

        private void ExplainKeyboard()
        {
            if (Device.Idiom == TargetIdiom.Tablet)
            {
                KeyboardView.IsVisible = true;
                AddCalculation(new Point(0.25 * canvas.Width, 0.25 * canvas.Height), TouchState.Up);
            }

            KeyboardView.Overlay(
                new Label
                {
                    Text = "Lastly, this is the keyboard",
                    HorizontalTextAlignment = TextAlignment.End,
                    TextColor = Color.White,
                    FontSize = 20
                },
                new Rectangle(0.5, 1 / 8.0, 0.95, AbsoluteLayout.AutoSize),
                AbsoluteLayoutFlags.PositionProportional | AbsoluteLayoutFlags.WidthProportional
                );
            KeyboardView.Overlay(
                new Label
                {
                    Text = "Long press DEL to clear the canvas ➜",
                    TextColor = Color.White,
                    FontSize = 20
                },
                new Rectangle(0.5, 3 / 8.0, 0.95, AbsoluteLayout.AutoSize),
                AbsoluteLayoutFlags.PositionProportional | AbsoluteLayoutFlags.WidthProportional
                );
            KeyboardView.Overlay(
                new Label
                {
                    Text = "Long press any other key and drag to move the cursor",
                    HorizontalTextAlignment = TextAlignment.End,
                    TextColor = Color.White,
                    FontSize = 20
                },
                new Rectangle(0.5, 5 / 8.0, 0.95, AbsoluteLayout.AutoSize),
                AbsoluteLayoutFlags.PositionProportional | AbsoluteLayoutFlags.WidthProportional
                );

            if (KeyboardView.ShowingFullKeyboard)
            {
                KeyboardView.Overlay(
                    new Label
                    {
                        Text = "Drag or tap the dock button to change the keyboard position ➜",
                        TextColor = Color.White,
                        FontSize = 20
                    },
                    new Rectangle(0.5, 7 / 8.0, 0.95, AbsoluteLayout.AutoSize),
                    AbsoluteLayoutFlags.PositionProportional | AbsoluteLayoutFlags.WidthProportional
                    );
            }
            else
            {
                StackLayout scroll = new StackLayout
                {
                    Orientation = StackOrientation.Horizontal,
                    HorizontalOptions = LayoutOptions.Start,
                    Spacing = 0
                };
                scroll.Children.Add(
                    new Label
                    {
                        Text = " ➜",
                        FontSize = 20,
                        TextColor = Color.White,
                        Rotation = 180
                    }
                    );
                scroll.Children.Add(
                    new Label
                    {
                        Text = "Scroll for more operations",
                        TextColor = Color.White,
                        FontSize = 20
                    }
                    );
                KeyboardView.Overlay(
                    scroll,
                    new Rectangle(0.5, 7 / 8.0, 0.95, AbsoluteLayout.AutoSize),
                    AbsoluteLayoutFlags.PositionProportional | AbsoluteLayoutFlags.WidthProportional
                    );
            }
        }

        private void ExplainAnswerFormats()
        {
            canvas.Children.Add(
                new Label
                {
                    Text = "Answers can be tapped to cycle through different formats",
                    FontSize = 20
                },
                new Rectangle(0, 0, 0.9, AbsoluteLayout.AutoSize),
                AbsoluteLayoutFlags.PositionProportional | AbsoluteLayoutFlags.WidthProportional
                );

            canvas.Children.Add(new Equation("sin30*2/3"), new Rectangle(0.5, 0.5, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize), AbsoluteLayoutFlags.PositionProportional);

            //canvas.Children.Add(new Equation("1+1/2"), new Rectangle(0.05, 0.4, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize), AbsoluteLayoutFlags.PositionProportional);
            //canvas.Children.Add(new Equation("sin90"), new Rectangle(0.95, 0.6, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize), AbsoluteLayoutFlags.PositionProportional);

            canvas.Children.Add(
                new Label
                {
                    Text = "And tapping the deg/rad label will toggle degrees and radians",
                    FontSize = 20,
                    HorizontalTextAlignment = TextAlignment.End,
                    VerticalTextAlignment = TextAlignment.End
                },
                new Rectangle(1, 1, 0.9, AbsoluteLayout.AutoSize),
                AbsoluteLayoutFlags.PositionProportional | AbsoluteLayoutFlags.WidthProportional
                );
        }

        private void ExplainCanvas()
        {
            canvas.Children.Add(
                new Label
                {
                    Text = "This area is the canvas, where you can enter calculations",
                    FontSize = 18
                },
                new Rectangle(0, 0, 0.9, AbsoluteLayout.AutoSize),
                AbsoluteLayoutFlags.PositionProportional | AbsoluteLayoutFlags.WidthProportional
                );
            canvas.Children.Add(
                new Label
                {
                    Text = "Calculations can be added by tapping anywhere, and moved by dragging the answer or equals sign",
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                    FontSize = 18
                },
                new Rectangle(0.5, 0.5, 0.9, AbsoluteLayout.AutoSize),
                AbsoluteLayoutFlags.PositionProportional | AbsoluteLayoutFlags.WidthProportional
                );
            canvas.Children.Add(
                new Label
                {
                    Text = "More space will be added as necessary; scroll to access it",
                    FontSize = 18,
                    HorizontalTextAlignment = TextAlignment.End,
                    VerticalTextAlignment = TextAlignment.End
                },
                new Rectangle(1, 1, 0.9, AbsoluteLayout.AutoSize),
                AbsoluteLayoutFlags.PositionProportional | AbsoluteLayoutFlags.WidthProportional
                );
        }

        private void Welcome()
        {
            canvas.Children.Add(
                new Label
                {
                    Text = "Welcome to Crunch!",
                    FontSize = 25,
                    HorizontalTextAlignment = TextAlignment.Center
                },
                new Rectangle(0.5, 1.0 / 3.0, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize),
                AbsoluteLayoutFlags.PositionProportional
                );
            canvas.Children.Add(
                new Label
                {
                    Text = "The calculator designed for your " + (Device.Idiom == TargetIdiom.Phone ? "smartphone" : "tablet"),
                    FontSize = 15,
                    HorizontalTextAlignment = TextAlignment.Center
                },
                new Rectangle(0.5, 2.0 / 3.0, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize),
                AbsoluteLayoutFlags.PositionProportional
                );
        }
    }
}
