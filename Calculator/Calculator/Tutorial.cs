using System;
using System.Collections.Generic;
using System.Text;

using System.Extensions;
using Xamarin.Forms;
using Xamarin.Forms.Extensions;

namespace Calculator
{
    public enum Fit { Uniform, Tight }

    public class CachedLayout : Layout<View>
    {
        public int Visible { get; private set; }
        private Fit Fit = Fit.Uniform;

        public void Show(int child)
        {
            if (child < 0 || child >= Children.Count)
            {
                return;
            }

            Children[Visible].IsVisible = false;
            Children[Visible = child].IsVisible = true;

            if (Fit == Fit.Tight)
            {
                InvalidateMeasure();
            }
        }

        protected override void LayoutChildren(double x, double y, double width, double height)
        {
            for (int i = 0; i < Children.Count; i++)
            {
                if (Fit == Fit.Uniform)
                {
                    LayoutChildIntoBoundingRegion(Children[i], new Rectangle(Point.Zero, new Size(width, height)));
                }
                else
                {
                    LayoutChildIntoBoundingRegion(Children[i], new Rectangle(Point.Zero, Sizes[i]));
                }
            }
        }

        private List<Size> Sizes;

        protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
        {
            //Print.Log(widthConstraint, heightConstraint);
            Size min = base.OnMeasure(widthConstraint, heightConstraint).Minimum;
            //Print.Log("\n\nmeasuring children");
            Sizes = new List<Size>();
            foreach (View view in Children)
            {
                Sizes.Add(view.Measure(widthConstraint, heightConstraint).Request);
            }

            Size request = Sizes[Visible];

            if (Fit == Fit.Uniform)
            {
                foreach(Size size in Sizes)
                {
                    request = new Size(Math.Max(request.Width, size.Width), Math.Max(request.Height, size.Height));
                }
            }

            return new SizeRequest(request, min);
        }

        protected override void OnAdded(View view)
        {
            base.OnAdded(view);
            view.IsVisible = false;
        }
    }

    public class Tutorial : StackLayout
    {
        public event SimpleEventHandler Completed;

        private readonly List<string[]> info = new List<string[]>()
        {
            new string[] { "canvas.gif", "Tap anywhere on the canvas to add an equation" },
            new string[] { "answer forms.gif", "Tap answers to see different forms, or the deg/rad label to toggle degrees and radians" },
            //new string[] { "answer forms.gif", "Tap an answer (or the deg/rad label) to see them in different forms" },
            new string[] { "move equations.gif", "Drag the equals sign to move an equation on the canvas" },
            new string[] { "drag drop answers.gif", "Drag and drop live answers between calculations" },
            //new string[] { "variables.gif", "Set values for unknown variables" },
            //new string[] { "editing.gif", "Go back and make changes anytime" },
        };

        private CachedLayout GIFLayout;
        private AbsoluteLayout Welcome;
        private Label Description;

        private Button Back;
        private Button Next;

        private int Step;

        public Tutorial()
        {
            HorizontalOptions = LayoutOptions.Center;
            VerticalOptions = LayoutOptions.FillAndExpand;
            Orientation = StackOrientation.Vertical;

            if (Device.Idiom == TargetIdiom.Phone)
            {
                info.Add(new string[] { "scroll keyboard.gif", "Scroll the keyboard for more operations" });
            }
            
            Welcome = new AbsoluteLayout
            {
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.FillAndExpand,
            };
            Welcome.Children.Add(new Label
            {
                Text = "Welcome to Crunch!",
                FontSize = 33,
                HorizontalTextAlignment = TextAlignment.Center,
            }, new Rectangle(0.5, 0.25, -1, -1), AbsoluteLayoutFlags.PositionProportional);
            Welcome.Children.Add(new Label
            {
                Text = "A new kind of calculator",
                FontSize = 20,
                HorizontalTextAlignment = TextAlignment.Center,
            }, new Rectangle(0.5, 0.75, -1, -1), AbsoluteLayoutFlags.PositionProportional);

            Next = new Button
            {
                HorizontalOptions = LayoutOptions.EndAndExpand,
                BackgroundColor = Color.Transparent,
            };
            Back = new Button
            {
                Text = "Back",
                BackgroundColor = Color.Transparent,
            };
            Next.Clicked += (sender, e) =>
            {
                if (Step + 1 == info.Count + 1)
                {
                    Completed?.Invoke();
                }
                else
                {
                    Set(Step + 1);
                }
            };
            Back.Clicked += (sender, e) =>
            {
                Set(Step - 1);
            };

            StackLayout nav = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
            };
            nav.Children.Add(Back);
            nav.Children.Add(Next);

            Description = new Label
            {
                FontSize = 20,
                HorizontalTextAlignment = TextAlignment.Center
            };

            GIFLayout = new CachedLayout
            {
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.FillAndExpand,
                //Fit = Fit.Uniform
            };
            GIFLayout.Children.Add(Welcome);
            foreach (string[] s in info)
            {
                GIF gif = new GIF(s[0])
                {
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center,
                };
                gif.Loaded += async () =>
                {
                    //await System.Threading.Tasks.Task.Delay(2000);
                    //gif.IsVisible = false;
                };

                GIFLayout.Children.Add(gif);
            }

            Children.Add(Description);
            Children.Add(GIFLayout);
            Children.Add(nav);

            Set(0);
        }

        private async void Set(int step)
        {
            if (step < 0 || step >= GIFLayout.Children.Count)
            {
                return;
            }

            try
            {
                await (GIFLayout.Children[step] as GIF)?.ResetGIF();
            }
            catch { }
            GIFLayout.Show(step);

            Back.IsEnabled = step > 0;
            Next.Text = step + 1 == info.Count + 1 ? "Done" : "Next";
            Description.Text = step > 0 ? info[step - 1][1] : "";

            Step = step;
        }
    }

    public partial class MainPage
    {
        private StackLayout Controls;
        private int step = 0;
        private Canvas canvasBackup;
        private Calculation calculationBackup;

        public void OldTutorial()
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
                    ClearOverlay();

                    KeyboardMask.IsVisible = true;
                    if (Device.Idiom == TargetIdiom.Tablet)
                    {
                        AbsoluteLayout.SetLayoutBounds(FullKeyboardView, new Rectangle(-1000, -1000, -1, -1));
                        //KeyboardView.IsVisible = false;
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
            KeyboardMask.IsVisible = false;
            AbsoluteLayout.SetLayoutBounds(FullKeyboardView, new Rectangle(1, 1, -1, -1));
            //KeyboardView.IsVisible = true;

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
                AbsoluteLayout.SetLayoutBounds(FullKeyboardView, new Rectangle(1, 1, -1, -1));
                //KeyboardView.IsVisible = true;
                //AddCalculation(new Point(0.25 * canvas.Width, 0.25 * canvas.Height), TouchState.Up);
            }

            Overlay(
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
            Overlay(
                new Label
                {
                    Text = "Long press DEL to clear the canvas ➜",
                    TextColor = Color.White,
                    FontSize = 20
                },
                new Rectangle(0.5, 3 / 8.0, 0.95, AbsoluteLayout.AutoSize),
                AbsoluteLayoutFlags.PositionProportional | AbsoluteLayoutFlags.WidthProportional
                );
            Overlay(
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

            if (Device.Idiom == TargetIdiom.Tablet)
            {
                Overlay(
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
                Overlay(
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
