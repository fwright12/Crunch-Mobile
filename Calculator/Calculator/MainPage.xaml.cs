using System;
using Xamarin.Forms;
using System.Linq;
using System.Collections.ObjectModel;
using System.IO;

using System.Extensions;
using Xamarin.Forms.Extensions;
using Crunch.GraphX;

namespace Calculator
{
    //Color 560297
    //&#8801; - hamburger menu ≡
    //&#8942; - kabob menu ⋮

    //Parentheses pictures - 500 font in Word Roboto (80,80,80), default Android ratio is 114 * 443 - trim and resize in Paint to 369 * 1127
    //Radical pictures - 667 font in Paint Segoe UI Symbol (copy from Graphemica), trim completely (609 * 1114)
    //Radical - right half is about 1/2 of horizontal, bottom left takes up about 1/2 of vertical, thickness is 0.1, horizontal bar is about 1/3 of horizontal

    /* To do:
     * Make substituted variables equations
     * Choice for how to interpret logarithms (base 2 or 10)
     * Choice for expanded/scrolling keyboard
     * 
     * Better looking parentheses - expand vertically
     * Add radical sign
     * Support for logarithms
     * Keyboard doesn't move when variable is hidden because answer is null
     * 
     * rendering issue for nested exponents (x^2)^2 ?
     * simplify exponentiated terms 2^(2x) = 4^x
     * change inverse trig interpretations?
     * render (sinx)^2 as sin^2(x)
     * 
     * Done:
     * Broke moving calculations
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

        private BoxView phantomCursor;
        //How much extra space is in the lower right
        private int padding = 100;

        public static double parenthesesWidth;

        public MainPage()
        {
            InitializeComponent();

            SettingsMenuSetup();

            AbsoluteLayout.SetLayoutBounds(equationsMenu, new Rectangle(0, 0, Device.Idiom == TargetIdiom.Tablet ? 0.3 : 0.8, 1));
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
            };

            Button dock;
            if (Device.Idiom == TargetIdiom.Tablet)
            {
                dock = new DockButton();

                page.Children.RemoveAt(1);
                phantomCursorField.Children.Add(keyboardContainer);
                keyboardContainer.MoveTo(-1000, -1000);

                dock.Text = "▽";
                dock.Clicked += delegate
                {
                    DockKeyboard(!keyboardDocked);
                };
                (dock as DockButton).Touch += (point, state) =>
                {
                    if (state == TouchState.Moving)
                    {
                        DockKeyboard(false);
                        keyboardContainer.BeginDrag(phantomCursorField.Bounds);
                    }
                };

                canvasScroll.Scrolled += delegate
                {
                    AdjustKeyboardPosition();
                };
            }
            else
            {
                dock = new Button();
            }
            permanentKeys.Children.Add(dock);

            canvas.Touch += AddCalculation;
            FocusChanged += SwitchCalculationFocus;

            //Keyboard formatting
            buttonFormat(keyboard);

            //Measuring for cursor
            Label l = new Label() { Text = "(", HorizontalOptions = LayoutOptions.Start };
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
            page.InterceptedTouch += (point, state) =>
            {
                if (settingsMenu.IsVisible)
                {
                    SetMenuVisibility(false);
                    App.SaveSettings();
                }
            };

            //Button hookup
            left.Clicked += (sender, e) => SoftKeyboard.Left();
            right.Clicked += (sender, e) => SoftKeyboard.Right();
            delete.Clicked += delegate
            {
                if (CalculationFocus != null)
                {
                    SoftKeyboard.Delete();
                    CalculationFocus.SetAnswer();
                }
            };
            delete.LongClick += ClearCanvas;

            //Phantom cursor stuff
            phantomCursor = new CursorView() { Color = Color.Red, IsVisible = false };
            SoftKeyboard.Cursor.SizeChanged += delegate { phantomCursor.HeightRequest = SoftKeyboard.Cursor.Height; };
            phantomCursorField.Children.Add(phantomCursor);

            FixDynamicLag("");
            print.log("main page constructor finished");
        }

        void FixDynamicLag(object o) => print.log(o as dynamic);

        private double width = -1;
        private double height = -1;

        public static int MaxBannerWidth = 320;
        private static double canvasWidthHorizontal;
        private static BannerAd ad;

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            
            if (this.width != width || this.height != height)
            {
                if (this.width == -1 && this.height == -1)
                {
                    if (Device.Idiom == TargetIdiom.Phone) {
                        int smaller = (int)(page.Width < page.Height ? page.Width : page.Height);
                        int larger = (int)(page.Width > page.Height ? page.Width : page.Height);
                        
                        int smallestPossibleSpace = (int)Math.Min(
                            smaller - Padding.Right - 50,
                            larger - Math.Min(smaller / columnsOnScreen, larger / 8) * 4 - page.Spacing
                            );

                        //phantomCursorField.Children.Add(new BoxView { Color = Color.Red }, new Rectangle(0.5, 0.5, larger - Math.Min(smaller / columnsOnScreen, larger / 8) * 4 - page.Spacing, 50), AbsoluteLayoutFlags.PositionProportional);

                        MaxBannerWidth = (int)Math.Min(MaxBannerWidth, smallestPossibleSpace - 50 - Padding.Left);
                    }

                    ad = new BannerAd();
                    phantomCursorField.Children.Add(ad);
                    AbsoluteLayout.SetLayoutFlags(ad, AbsoluteLayoutFlags.PositionProportional);
                }

                this.width = width;
                this.height = height;

                settingsMenuWidth = Math.Min(maxSettingsMenuWidth, page.Width - Padding.Left - 50);
                AbsoluteLayout.SetLayoutBounds(settingsMenu, new Rectangle(0, 0, settingsMenuWidth, 1));
                SetMenuVisibility(settingsMenu.TranslationX == 0);

                (decimalPrecision.Parent.Parent as StackLayout).Orientation = smallerSettingsMenu ? StackOrientation.Vertical : StackOrientation.Horizontal;
                (fractiondecimal.View as Toggle).Orientation = smallerSettingsMenu ? StackOrientation.Vertical : StackOrientation.Horizontal;
                (degrad.View as Toggle).Orientation = smallerSettingsMenu ? StackOrientation.Vertical : StackOrientation.Horizontal;

                padding = (int)Math.Min(page.Width, page.Height);

                keyboardScroll.ScrollToAsync(keypad, ScrollToPosition.End, false);
                layoutKeyboard();

                double adCenter = Device.Idiom == TargetIdiom.Phone && width > height ? (60 + canvasWidthHorizontal - MaxBannerWidth) / (2 * (canvasWidthHorizontal - MaxBannerWidth)) : 0.5;
                AbsoluteLayout.SetLayoutBounds(ad, new Rectangle(adCenter, 0, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
            }
        }

        private int rows = 4;
        private int columns = 7;
        private readonly int columnsOnScreen = 5;
        private readonly int undockedButtonSize = 75;
        private readonly double permanentKeysIncrease = 1.25;

        private void layoutKeyboard()
        {
            //Buttons are squares with dimensions equal to 1/5 of the (smaller) available space
            double buttonSize = orient(page.Width, page.Height) / columnsOnScreen;
            if (Device.Idiom == TargetIdiom.Tablet)
            {
                buttonSize = undockedButtonSize;
            }

            //Right and left sized individually so both fit in one column
            right.WidthRequest = buttonSize * permanentKeysIncrease / 2;
            left.WidthRequest = buttonSize * permanentKeysIncrease / 2;

            keypad.ColumnDefinitions.Clear();
            keypad.RowDefinitions.Clear();

            double height = buttonSize;
            double width = buttonSize;

            if (Device.Idiom == TargetIdiom.Phone)
            {
                page.Orientation = orient(StackOrientation.Vertical, StackOrientation.Horizontal);
                keyboard.Orientation = orient(StackOrientation.Horizontal, StackOrientation.Vertical);
                permanentKeys.Orientation = orient(StackOrientation.Vertical, StackOrientation.Horizontal);

                if (page.Orientation == StackOrientation.Vertical)
                {
                    //Permanent keys are slightly wider than rest of buttons
                    permanentKeys.WidthRequest = buttonSize * permanentKeysIncrease;

                    //Recalculate button size
                    width = (page.Width - buttonSize * permanentKeysIncrease) / (columnsOnScreen - 1);
                    height = Math.Min(buttonSize, page.Height / 8);

                    canvasWidthHorizontal = page.Width;
                    phantomCursorField.HeightRequest = page.Height - height * 4 - page.Spacing;
                }
                else if (page.Orientation == StackOrientation.Horizontal)
                {
                    width = Math.Min(buttonSize, page.Width / 8);

                    //Permanent keys equally sized using space not occupied by left and right
                    double temp = (width * (columnsOnScreen - 1) - buttonSize * permanentKeysIncrease) / (rows - 1);
                    foreach (View v in permanentKeys.Children)
                    {
                        if (v is Button)
                        {
                            v.WidthRequest = temp;
                        }
                    }

                    phantomCursorField.WidthRequest = canvasWidthHorizontal = page.Width - width * 4 - page.Spacing;
                    keyboardContainer.WidthRequest = width * 4;
                }
            }

            //Button width
            for (int i = 0; i < columns - 1; i++)
            {
                keypad.ColumnDefinitions.Add(new ColumnDefinition { Width = width });
            }
            foreach(Button b in parentheses.Children)
            {
                b.WidthRequest = width / 2;
            }

            //Button height
            for (int i = 0; i < rows; i++)
            {
                keypad.RowDefinitions.Add(new RowDefinition { Height = height });
            }
            foreach(View v in permanentKeys.Children)
            {
                v.HeightRequest = height;
            }
        }

        private void buttonFormat(Layout parent)
        {
            for (int i = 0; i < parent.Children.Count; i++)
            {
                View v = parent.Children[i] as View;

                if (v is Button)
                {
                    Button b = v as Button;

                    //Dynamic sizing for some buttons
                    if (b.Parent == arrowKeys)
                    {
                        b.SizeChanged += delegate { b.FontSize = Math.Floor(33 * b.Height / 75); };
                    }
                    else if (b.Text?.Length > 1) //b == info || b == delete)
                    {
                        b.SizeChanged += delegate { b.FontSize = Math.Floor(33 * b.Width / 75 * 5 / (4 + b.Text.Length)); };
                    }
                    else
                    {
                        b.SizeChanged += delegate { b.FontSize = Math.Floor(33 * Math.Max(b.Height, b.Width) / 75); };
                    }

                    if (b.Parent == keypad || b.Parent == parentheses)
                    {
                        b.Clicked += delegate
                        {
                            if (CalculationFocus != null)
                            {
                                SoftKeyboard.Type(b.StyleId ?? b.Text);
                                if (keypad.Children.IndexOf(b) % (columns - 1) <= columns - columnsOnScreen)
                                {
                                    keyboardScroll.ScrollToAsync(keypad, ScrollToPosition.End, false);
                                }
                                CalculationFocus.SetAnswer();
                            }
                        };
                        (b as LongClickableButton).LongClick += EnterCursorMode;
                    }
                }
                else if (v is Layout)
                {
                    buttonFormat(v as Layout);
                }
            }
        }

        private T orient<T>(T verticalOption, T horizontalOption)
        {
            if (height > width)
            {
                return verticalOption;
            }
            return horizontalOption;
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

        private void AdjustKeyboardPosition(object sender, EventArgs e) => AdjustKeyboardPosition();

        private void AdjustKeyboardPosition()
        {
            if (Device.Idiom == TargetIdiom.Tablet && keyboardDocked && CalculationFocus != null)
            {
                keyboardContainer.MoveTo(CalculationFocus.X - canvasScroll.ScrollX, CalculationFocus.Y + CalculationFocus.Height - canvasScroll.ScrollY);
            }
        }

        private static void FocusOnCalculation(Calculation newFocus)
        {
            FocusChanged?.Invoke(CalculationFocus, newFocus);
            CalculationFocus = newFocus;
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
                    keyboardContainer.MoveTo(-1000, -1000);
                    keyboardDocked = true;
                }

                FocusOnCalculation(null);
            }
        }

        private void SetCursorMode(bool onOrOff)
        {
            phantomCursor.IsVisible = onOrOff;
            keyboardMask.IsVisible = onOrOff;
        }

        private void EnterCursorMode()
        {
            if (CalculationFocus == null)
            {
                return;
            }

            if (Device.Idiom == TargetIdiom.Tablet && !keyboardDocked)
            {
                phantomCursor.MoveTo(TouchScreen.LastDownEvent.X, keyboardContainer.Y - SoftKeyboard.Cursor.Height);
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
                    Crunch.Engine.Testing.Debug = needToShowWork;
                    Equation e = new Equation(question);
                    canvas.Children.Add(e);

                    if (needToShowWork)
                    {
                        e.Children.Add(new Text("          --------------------->      "));
                        e.Children.Add(new Expression(Render.Math(testcases.Answers[i].ToString())));

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
 