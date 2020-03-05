using System;
using System.Collections.Generic;
using System.Text;

using System.Extensions;
using Xamarin.Forms;
using Xamarin.Forms.Extensions;

namespace Calculator
{
#if false
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
#endif

    public class MainPageTutorial : MainPage
    {
        public event SimpleEventHandler Completed;

        private int Step = 0;

        /*private ScrollView CanvasScroll;
        private Canvas Canvas;
        private AbsoluteLayout KeyboardMask;
        private CrunchKeyboard CrunchKeyboard;
        private AbsoluteLayout PhantomCursorField;
        private StackLayout FullKeyboardView;

        private Point KeyboardHidden = Point.Zero;*/

        public MainPageTutorial() : base()
        {
            ExtraPadding = 0;
            CanvasScroll.Remove();
            SettingsMenuButton.Remove();

            Button back = new Button() { Text = "Back", HorizontalOptions = LayoutOptions.StartAndExpand };
            Button next = new Button() { HorizontalOptions = LayoutOptions.EndAndExpand };

            Action[] order = new Action[] { Welcome, ExplainCanvas, ExplainAnswerFormats, ExplainKeyboard, End };

            Action<int> change = (i) =>
            {
                if (i >= 0 && i < order.Length)
                {
                    Canvas.Children.Clear();
                    KeyboardMask.Children.Clear();

                    KeyboardMask.IsVisible = true;
                    if (!CrunchKeyboard.IsCondensed)
                    {
                        CrunchKeyboard.IsVisible = false;
                        //MoveKeyboard(KeyboardHidden);
                    }

                    back.IsEnabled = i > 0;
                    next.Text = i == order.Length - 2 ? "Let's Go!" : "Next";

                    order[Step = i]();
                }
            };

            back.Clicked += (sender, e) => change(Step - 1);
            next.Clicked += (sender, e) => change(Step + 1);

            Canvas.HorizontalOptions = LayoutOptions.FillAndExpand;
            Canvas.VerticalOptions = LayoutOptions.FillAndExpand;

            StackLayout main = new StackLayout
            {
                Orientation = StackOrientation.Vertical,
                Children =
                {
                    Canvas,
                    new StackLayout
                    {
                        Orientation = StackOrientation.Horizontal,
                        Children =
                        {
                            back,
                            next,
                        },
                        HorizontalOptions = LayoutOptions.Center,
                        Padding = new Thickness(0, 0, 0, Variables.ButtonSize)
                    }
                }
            };
            CanvasArea.Children.Add(main, new Rectangle(0, 0, 1, 1), AbsoluteLayoutFlags.SizeProportional);

            CrunchKeyboard.SizeChanged += (sender, e) => change(0);

            /*CrunchKeyboard.SizeChanged += (sender, e) =>
            {
                if (CrunchKeyboard.IsCondensed)
                {
                    AbsoluteLayout.SetLayoutBounds(main, new Rectangle(Point.Zero, CrunchKeyboard.Orientation == StackOrientation.Horizontal ? new Size(CrunchKeyboard.Width, Height - Padding.Top - Padding.Bottom - CrunchKeyboard.Height) : new Size(Width - Padding.Left - Padding.Right - CrunchKeyboard.Width, CrunchKeyboard.Height)));
                }
                else
                {
                    AbsoluteLayout.SetLayoutBounds(main, new Rectangle(0, 0, 1, 1));
                    AbsoluteLayout.SetLayoutFlags(main, AbsoluteLayoutFlags.All);
                }

                AbsoluteLayout.SetLayoutBounds(KeyboardMask, new Rectangle(Point.Zero, CrunchKeyboard.Bounds.Size));
                AbsoluteLayout.SetLayoutFlags(KeyboardMask, AbsoluteLayoutFlags.None);

                change(0);
            };*/
        }

        private void End()
        {
            Completed?.Invoke();
        }

        private void MoveKeyboard(Point point)
        {
            AbsoluteLayout.SetLayoutBounds(FullKeyboardView, new Rectangle(point, AbsoluteLayout.GetLayoutBounds(FullKeyboardView).Size));
        }

        private void ExplainKeyboard()
        {
            PhantomCursor.IsVisible = true;
            if (!CrunchKeyboard.IsCondensed)
            {
                CrunchKeyboard.IsVisible = true;
                MoveKeyboard(new Point(1, 0.8));
                //AbsoluteLayout.SetLayoutBounds(FullKeyboardView, new Rectangle(1, 1, -1, -1));
                //KeyboardView.IsVisible = true;
                //AddCalculation(new Point(0.25 * canvas.Width, 0.25 * canvas.Height), TouchState.Up);
            }

            KeyboardMask.Children.Add(
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
            KeyboardMask.Children.Add(
                new Label
                {
                    Text = "Long press DEL to clear the canvas",// ➜",
                    TextColor = Color.White,
                    FontSize = 20
                },
                new Rectangle(0.5, 3 / 8.0, 0.95, AbsoluteLayout.AutoSize),
                AbsoluteLayoutFlags.PositionProportional | AbsoluteLayoutFlags.WidthProportional
                );
            KeyboardMask.Children.Add(
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

            if (!CrunchKeyboard.IsCondensed)
            {
                KeyboardMask.Children.Add(
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
                KeyboardMask.Children.Add(
                    scroll,
                    new Rectangle(0.5, 7 / 8.0, 0.95, AbsoluteLayout.AutoSize),
                    AbsoluteLayoutFlags.PositionProportional | AbsoluteLayoutFlags.WidthProportional
                    );
            }
        }

        private void ExplainAnswerFormats()
        {
            Canvas.Children.Add(
                new Label
                {
                    Text = "Answers can be tapped to cycle through different formats",
                    FontSize = 20
                },
                new Rectangle(0, 0, 0.9, AbsoluteLayout.AutoSize),
                AbsoluteLayoutFlags.PositionProportional | AbsoluteLayoutFlags.WidthProportional
                );

            Canvas.Children.Add(new Equation("sin30*2/3"), new Rectangle(0.5, 0.5, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize), AbsoluteLayoutFlags.PositionProportional);

            //canvas.Children.Add(new Equation("1+1/2"), new Rectangle(0.05, 0.4, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize), AbsoluteLayoutFlags.PositionProportional);
            //canvas.Children.Add(new Equation("sin90"), new Rectangle(0.95, 0.6, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize), AbsoluteLayoutFlags.PositionProportional);

            Canvas.Children.Add(
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
            Canvas.Children.Add(
                new Label
                {
                    Text = "This area is the canvas, where you can enter calculations",
                    FontSize = 18
                },
                new Rectangle(0, 0, 0.9, AbsoluteLayout.AutoSize),
                AbsoluteLayoutFlags.PositionProportional | AbsoluteLayoutFlags.WidthProportional
                );
            Canvas.Children.Add(
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
            Canvas.Children.Add(
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
            Canvas.Children.Add(
                new Label
                {
                    Text = "Welcome to Crunch!",
                    FontSize = 25,
                    HorizontalTextAlignment = TextAlignment.Center
                },
                new Rectangle(0.5, 1.0 / 3.0, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize),
                AbsoluteLayoutFlags.PositionProportional
                );
            Canvas.Children.Add(
                new Label
                {
                    Text = "The calculator designed for the digital age",
                    FontSize = 15,
                    HorizontalTextAlignment = TextAlignment.Center
                },
                new Rectangle(0.5, 2.0 / 3.0, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize),
                AbsoluteLayoutFlags.PositionProportional
                );
        }
    }
}
