using System;
using Xamarin.Forms;
using System.Linq;
using System.Collections.ObjectModel;
using System.IO;

using System.Extensions;
using Xamarin.Forms.Extensions;
using Crunch.GraphX;
using Xamarin.Forms.Xaml;

namespace Calculator
{
    //Color 560297
    //&#8801; - hamburger menu ≡
    //&#8942; - kabob menu ⋮

    //Parentheses pictures - 500 font in Word Roboto (80,80,80), default Android ratio is 114 * 443 - trim and resize in Paint to 369 * 1127
    //Radical pictures - 667 font in Paint Segoe UI Symbol (copy from Graphemica), trim completely (609 * 1114)
    //Radical - right half is about 1/2 of horizontal, bottom left takes up about 1/2 of vertical, thickness is 0.1, horizontal bar is about 1/3 of horizontal

    /* To do:
     * v2.1.1
     * Error trying to grab minus signs
     * Not all buttons cause keyboard to scroll back
     * Choice for how to interpret logarithms (base 2 or 10)
     * Threshold for displaying in scientific notation
     * Reset answer defaults
     * 
     * v2.1.2
     * Sorting for terms and expressions
     * Choice for expanded/scrolling keyboard
     * 
     * v2.2
     * Drag and drop equations
     * Make substituted variables equations
     * 
     * v?
     * rendering issue for nested exponents (x^2)^2 ?
     * simplify exponentiated terms 2^(2x) = 4^x
     * change inverse trig interpretations?
     * render (sinx)^2 as sin^2(x)
     */

    public delegate void FocusChangedEventHandler(Calculation oldFocus, Calculation newFocus);
    public delegate void RenderedEventHandler();

    public partial class MainPage : ContentPage
    {
        public static MainPage VisiblePage { get; private set; }
        public static event FocusChangedEventHandler FocusChanged;
        public static Calculation CalculationFocus { get; private set; }

        private static bool keyboardDocked = true;

        public Layout Canvas => canvas;
        public TouchScreen Page => page;
        public RenderedEventHandler OnRendered;

        private Keyboard KeyboardView;
        private BoxView phantomCursor;
        //How much extra space is in the lower right
        private int padding = 100;

        public static double parenthesesWidth;

        public MainPage()
        {
            InitializeComponent();

            /*AbsoluteLayout.SetLayoutBounds(equationsMenu, new Rectangle(0, 0, Device.Idiom == TargetIdiom.Tablet ? 0.3 : 0.8, 1));
            equationsMenu.TranslationX = page.Width - equationsMenu.Width;
            equationsButton.Clicked += async (sender, e) => await equationsMenu.TranslateTo(100, 100);

            equations.ItemsSource = new string[]
            {
                "mono",
                "monodroid",
                "monotouch",
                "monorail",
                "monodevelop",
                "monotone",
                "monopoly",
                "monomodal",
                "mononucleosis"
            };*/

            SettingsMenuSetUp();

            //Set up for keyboard
            WireUpKeyboard(KeyboardView = new Keyboard());

            if (Device.Idiom == TargetIdiom.Tablet)
            {
                phantomCursorField.Children.Add(KeyboardView);
                KeyboardView.MoveTo(-1000, -1000);

                canvasScroll.Scrolled += delegate
                {
                    AdjustKeyboardPosition();
                };
            }
            else
            {
                page.Children.Add(KeyboardView);
            }

            canvas.Touch += AddCalculation;
            FocusChanged += SwitchCalculationFocus;

            //Measuring for cursor
            Label l = new Label() { Text = "(", HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Start };
            l.FontSize = Text.MaxFontSize;
            page.Children.Add(l);
            l.SizeChanged += delegate
            {
                Text.MaxTextHeight = l.Height;
                parenthesesWidth = l.Width;

                Render.CreateLeftParenthesis = () => new TextImage(new Image() { Source = "leftParenthesis.png", HeightRequest = 0, WidthRequest = parenthesesWidth, Aspect = Aspect.Fill }, "(");
                Render.CreateRightParenthesis = () => new TextImage(new Image() { Source = "rightParenthesis.png", HeightRequest = 0, WidthRequest = parenthesesWidth, Aspect = Aspect.Fill }, ")");
                Render.CreateRadical = () => new Image() { Source = "radical.png", HeightRequest = 0, WidthRequest = parenthesesWidth * 2, Aspect = Aspect.Fill };

                page.Children.Remove(l);

                takeMathTest();
            };

            VisiblePage = this;
            Drag.Screen = page;

            //Phantom cursor stuff
            phantomCursor = new CursorView() { Color = Color.Red, IsVisible = false };
            SoftKeyboard.Cursor.SizeChanged += delegate { phantomCursor.HeightRequest = SoftKeyboard.Cursor.Height; };
            phantomCursorField.Children.Add(phantomCursor);

            FixDynamicLag("");
            print.log("main page constructor finished");
        }

        void FixDynamicLag(object o) => print.log(o as dynamic);
        private readonly int MenuButtonWidth = 44;
        private double SettingsMenuWidth = 400;

        private void SettingsMenuSetUp()
        {
            AnythingButton settingsMenuButton = new AnythingButton();
            for (double i = 0.25; i < 1; i += 0.25)
            {
                settingsMenuButton.Children.Add(new BoxView() { Color = Color.Black }, new Rectangle(0.5, i, 0.6, 0.075), AbsoluteLayoutFlags.All);
            }
            settingsMenuButton.SetButtonBorderWidth(0);

            if (Device.Idiom == TargetIdiom.Phone)
            {
                AbsoluteLayout layout = new AbsoluteLayout() { VerticalOptions = LayoutOptions.CenterAndExpand, HorizontalOptions = LayoutOptions.FillAndExpand };
                layout.Children.Add(settingsMenuButton, new Rectangle(0, 0.5, MenuButtonWidth, MenuButtonWidth), AbsoluteLayoutFlags.YProportional);
                //layout.SizeChanged += (sender, e) => layout.Children.Add(settingsMenuButton, new Rectangle(0, 0, layout.Height, layout.Height), AbsoluteLayoutFlags.None);
                layout.SizeChanged += delegate
                {
                    MaxBannerWidth = Math.Min(320, (int)(layout.Width - (44 + 10) * 2));

                    if (ad != null)
                    {
                        layout.Children.Remove(ad);
                    }
                    layout.Children.Add(ad = new BannerAd(), new Rectangle(0.5, 0.5, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize), AbsoluteLayoutFlags.PositionProportional);
                };

                ContentPage settings = new ContentPage() { Content = new SettingsMenu(), Title = "Settings" };
                settings.Disappearing += (sender, e) => Settings.Save();
                settingsMenuButton.Clicked += (sender, e) => Navigation.PushAsync(settings, true);

                NavigationPage.SetTitleView(this, layout);
            }
            else if (Device.Idiom == TargetIdiom.Tablet)
            {
                ContentView settings = new SettingsMenu() { BackgroundColor = Color.LightGray };
                screen.Children.Add(settings, new Rectangle(0, 0, SettingsMenuWidth, 1), AbsoluteLayoutFlags.HeightProportional);
                screen.Children.Add(settingsMenuButton, new Rectangle(0, 0, MenuButtonWidth, MenuButtonWidth), AbsoluteLayoutFlags.None);
                phantomCursorField.Children.Add(new BannerAd(), new Rectangle(0.5, 0, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize), AbsoluteLayoutFlags.PositionProportional);

                Action<bool> setVisibility = (visible) =>
                {
                    if (settings.TranslationX == 0 == visible)
                    {
                        return;
                    }

                    settings.TranslateTo((SettingsMenuWidth + Padding.Left) * (visible.ToInt() - 1), 0);
                    settingsMenuButton.TranslateTo((SettingsMenuWidth + Padding.Left) * visible.ToInt(), 0);
                    
                    if (!visible)
                    {
                        Settings.Save();
                    }
                };

                settingsMenuButton.Clicked += (sender, e) => setVisibility(settings.TranslationX < 0);
                page.InterceptedTouch += (point, state) => setVisibility(false);

                setVisibility(false);
            }
        }

        private void WireUpKeyboard(Keyboard keyboard)
        {
            foreach(Key key in keyboard)
            {
                string text = key.Output;

                if (text == Key.LEFT)
                {
                    key.Clicked += (sender, e) => SoftKeyboard.Left();
                }
                else if (text == Key.RIGHT)
                {
                    key.Clicked += (sender, e) => SoftKeyboard.Right();
                }
                else if (text == Key.DELETE)
                {
                    key.Clicked += delegate
                    {
                        if (CalculationFocus != null)
                        {
                            SoftKeyboard.Delete();
                            CalculationFocus.SetAnswer();
                        }
                    };
                    key.LongClick += ClearCanvas;
                }
                else if (text == Key.DOCK)
                {
                    key.Clicked += delegate
                    {
                        DockKeyboard(!keyboardDocked);
                    };
                    key.Touch += (point, state) =>
                    {
                        if (state == TouchState.Moving)
                        {
                            DockKeyboard(false);
                            keyboard.BeginDrag(phantomCursorField.Bounds);
                        }
                    };
                }
                else
                {
                    key.Clicked += delegate
                     {
                         if (CalculationFocus != null)
                         {
                             SoftKeyboard.Type(key.Output);
                             CalculationFocus.SetAnswer();
                         }
                     };
                    key.LongClick += EnterCursorMode;
                }
            }
        }

        private double width = -1;
        private double height = -1;

        public static int MaxBannerWidth = 320;
        private double canvasWidthHorizontal => KeyboardView.Width;
        private static BannerAd ad;

        protected override void OnSizeAllocated(double width, double height)
        {
            page.Orientation = height > width ? StackOrientation.Vertical : StackOrientation.Horizontal;

            base.OnSizeAllocated(width, height);
            
            if (this.width != width || this.height != height)
            {
                this.width = width;
                this.height = height;

                padding = (int)Math.Min(page.Width, page.Height);
            }
        }

        private void DockKeyboard(bool isDocked)
        {
            keyboardDocked = isDocked;

            if (keyboardDocked)
            {
                AdjustKeyboardPosition();
            }
        }

        private void SwitchCalculationFocus(Calculation oldFocus, Calculation newFocus)
        {
            if (oldFocus != null)
            {
                oldFocus.LayoutChanged -= AdjustKeyboardPosition;
                if (oldFocus.Main.LHS.ChildCount() == 0)
                {
                    oldFocus.Remove();
                }
            }

            if (newFocus != null)
            {
                newFocus.LayoutChanged += AdjustKeyboardPosition;
            }
        }

        private static void FocusOnCalculation(Calculation newFocus)
        {
            FocusChanged?.Invoke(CalculationFocus, newFocus);
            CalculationFocus = newFocus;
        }

        private void AdjustKeyboardPosition(object sender, EventArgs e) => AdjustKeyboardPosition();

        private void AdjustKeyboardPosition()
        {
            if (Device.Idiom == TargetIdiom.Tablet && keyboardDocked && CalculationFocus != null)
            {
                KeyboardView.MoveTo(CalculationFocus.X - canvasScroll.ScrollX, CalculationFocus.Y + CalculationFocus.Height - canvasScroll.ScrollY);
            }
        }

        private void AddCalculation(Point point, TouchState state)
        {
            if (state != TouchState.Up)
            {
                return;
            }

            Equation equation = new Equation();
            SoftKeyboard.MoveCursor(equation.LHS);

            Calculation calculation = new Calculation(equation) { RecognizeVariables = true };
            FocusOnCalculation(calculation);

            calculation.SizeChanged += delegate
            {
                Point p = point.Add(new Point(calculation.Width, calculation.Height));

                if (canvas.Width - p.X < padding)
                {
                    canvas.WidthRequest = p.X + padding;
                }
                if (canvas.Height - p.Y < padding)
                {
                    canvas.HeightRequest = p.Y + padding;
                }
            };

            canvas.Children.Add(calculation, point);
        }

        private async void ClearCanvas()
        {
            if (!Settings.ClearCanvasWarning || await DisplayAlert("Wait!", "Are you sure you want to clear the canvas?", "Yes", "No"))
            {
                canvas.Children.Clear();
                canvas.WidthRequest = (canvas.Parent as View).Width;
                canvas.HeightRequest = (canvas.Parent as View).Height;
                if (Device.Idiom == TargetIdiom.Tablet)
                {
                    KeyboardView.MoveTo(-1000, -1000);
                    keyboardDocked = true;
                }

                FocusOnCalculation(null);
            }
        }

        private void SetCursorMode(bool onOrOff)
        {
            phantomCursor.IsVisible = onOrOff;
            KeyboardView.MaskKeys(onOrOff);
        }

        private void EnterCursorMode()
        {
            if (CalculationFocus == null)
            {
                return;
            }

            if (Device.Idiom == TargetIdiom.Tablet && !keyboardDocked)
            {
                phantomCursor.MoveTo(TouchScreen.LastDownEvent.X, KeyboardView.Y - SoftKeyboard.Cursor.Height);
            }
            else
            {
                Point temp = SoftKeyboard.Cursor.PositionOn(canvas).Add(new Point(-canvasScroll.ScrollX, -canvasScroll.ScrollY));
                if (temp.X >= 0 && temp.X <= canvasScroll.Width && temp.Y >= 0 && temp.Y <= canvas.Height)
                {
                    phantomCursor.MoveTo(temp);
                }
            }

            phantomCursor.HeightRequest = SoftKeyboard.Cursor.Height;

            SetCursorMode(true);
            phantomCursor.BeginDrag(phantomCursorField.Bounds, 2);
            page.Touch += MoveCursor;
        }

        private void ExitCursorMode()
        {
            //Climb up to the top of the tree structure
            Element root = SoftKeyboard.Cursor;
            while (!(root is Calculation) )
            {
                root = root.Parent;
            }

            //Focus has changed
            if (root != CalculationFocus)
            {
                FocusOnCalculation(root as Calculation);
                AdjustKeyboardPosition();
            }

            SetCursorMode(false);
            page.Touch -= MoveCursor;
        }

        private System.Threading.Thread thread;
        private int shouldScrollX => (int)Math.Truncate(phantomCursor.X / (canvasScroll.Width - phantomCursor.Width) * 2 - 1);
        private int shouldScrollY => (int)Math.Truncate(phantomCursor.Y / (canvasScroll.Height - phantomCursor.Height) * 2 - 1);
        private readonly double scrollSpeed = 0.025;
        private double preciseScrollX, preciseScrollY;

        private void scrollCanvas()
        {
            preciseScrollX = canvasScroll.ScrollX;
            preciseScrollY = canvasScroll.ScrollY;
            while (shouldScrollX + shouldScrollY != 0 && phantomCursor.IsVisible)
            {
                preciseScrollX += shouldScrollX * scrollSpeed;
                preciseScrollY += shouldScrollY * scrollSpeed;
                canvasScroll.ScrollToAsync(preciseScrollX, preciseScrollY, false);
            }
        }

        private void MoveCursor(Point point, TouchState state)
        {
            if (state == TouchState.Up)
            {
                ExitCursorMode();
            }
            else
            {

                if ((thread == null || !thread.IsAlive) && shouldScrollX + shouldScrollY != 0)
                {
                    thread = new System.Threading.Thread(scrollCanvas);
                    thread.Start();
                }

                //Get the coordinates of the cursor relative to the entire screen
                Point loc = new Point(canvasScroll.ScrollX + phantomCursor.X + phantomCursor.Width / 2, canvasScroll.ScrollY + phantomCursor.Y + phantomCursor.Height / 2);
                View view = GetViewAt(canvas, ref loc);

                if (view is Text || view is Expression)
                {
                    int leftOrRight = (int)Math.Round(loc.X / view.Width);

                    if (view.GetType() == typeof(Expression))
                    {
                        Expression e = view as Expression;
                        SoftKeyboard.MoveCursor(e, Math.Min(e.Children.Count, e.Children.Count * leftOrRight));
                    }
                    else if (view is Text)
                    {
                        SoftKeyboard.MoveCursor((view as Text).Parent, (view.Parent as Expression).IndexOf(view) + leftOrRight);
                    }
                }
            }
        }

        private View GetViewAt(Layout<View> parent, ref Point pos)
        {
            View ans = null;

            int i = 0;
            for (; i < parent.Children.Count; i++)
            {
                View child = parent.Children[i];
                
                //Is the point inside the bounds that this child occupies?
                if (pos.X >= child.X && pos.X <= child.X + child.Width && pos.Y >= child.Y && pos.Y <= child.Y + child.Height)
                {
                    pos = pos.Add(new Point(-child.X, -(child.Y + child.TranslationY)));
                    
                    if (child is Layout<View>)
                    {
                        ans = GetViewAt(child as Layout<View>, ref pos);
                    }
                    else if (parent.Editable())
                    {
                        ans = child;
                    }

                    break;
                }
            }
            
            Expression e = parent as Expression;
            if (i == parent.Children.Count && e != null && e.Editable && (pos.X <= e.PadLeft || pos.X >= e.Width - e.PadRight))
            {
                ans = parent;
            }

            return ans;
        }

        private void takeMathTest()
        {
            Crunch.Engine.MathTest testcases = Crunch.Engine.Testing.Test();

            int num = testcases.Questions.Count;
            if (num == 0) return;

            int correct = 0;
            int incorrect = 0;

            for (int i = 0; i < num; i++)
            {
                string question = testcases.Questions[i];
                bool needToShowWork = !testcases.CheckQuestion(i, question);

                if (Crunch.Engine.Testing.DisplayCorrectAnswers || needToShowWork)
                {
                    Crunch.Engine.Testing.ShowWork = needToShowWork;
                    Equation e = new Equation(question);
                    Crunch.Engine.Testing.ShowWork = false;
                    canvas.Children.Add(e);

                    if (needToShowWork)
                    {
                        if (testcases.Answers[i] != null)
                        {
                            e.Children.Add(new Text("          --------------------->      "));
                            e.Children.Add(new Expression(Render.Math(testcases.Answers[i].ToString())));
                        }

                        e.TranslateTo(0, 50 + incorrect++ * 150);
                    }
                    else
                    {
                        e.TranslateTo(1000, 50 + correct++ * 150);
                    }
                }
            }

            canvas.HeightRequest = 50 + Math.Max(correct, incorrect) * 150;
            canvas.WidthRequest = 2000;
        }
    }
}
 