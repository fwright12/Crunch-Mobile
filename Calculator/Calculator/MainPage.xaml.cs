using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.IO;

using System.Extensions;
using Xamarin.Forms;
using Xamarin.Forms.MathDisplay;
using Xamarin.Forms.Extensions;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
using System.Threading;

namespace Calculator
{
    //Color 560297
    //&#8801; - hamburger menu ≡
    //&#8942; - kabob menu ⋮
    //-no-snapshot
    //vxij-udgj-qwrl-vxza

    //Parentheses pictures - 500 font in Word Roboto (80,80,80), default Android ratio is 114 * 443 - trim and resize in Paint to 369 * 1127
    //Radical pictures - 667 font in Paint Segoe UI Symbol (copy from Graphemica), trim completely (609 * 1114)
    //Radical - right half is about 1/2 of horizontal, bottom left takes up about 1/2 of vertical, thickness is 0.1, horizontal bar is about 1/3 of horizontal

    /* To do:
     * v?
     * Makeover - color scheme, better icons
     * 
     * v.?
     * Native drag and drop
     * Access to system keyboard (for more variables)
     * Predefined functions (quadratic formula, physics stuff), ability to add custom ones
     * Flick to delete
     * Update tutorial, tips and tricks
     * Refactor Answer class / form switching system
     * Make it easier to see stuff on phone
     *      Scroll so cursor is always visible
     *      Show answer above to the right of the keyboard when offscreen
     * Refactor substituted variables to be links instead of VariableAssignment dependency lists
     * Refactor editability for Expressions (move code from expression up into MathLayout?)
     * Rework calcuation changed event
     * 
     * v..?
     * Settings
     *      Choice for expanded/scrolling keyboard
     *      Adjust TouchScreen.FatFinger
     * Tablet keyboard doesn't follow moved calculation
     * Long press on arrow keys to send multiple key presses
     * Page margin adjustment in MainPage SettingsMenuSetup method (should be handled in App class, not necessary)
     * Remember keyboard position on tablet
     * iPad split view
     * 
     * Other stuff:
     * Allow dragged views to scroll canvas - https://docs.microsoft.com/en-us/xamarin/ios/user-interface/ios-ui/ui-thread
     * Performance - https://docs.microsoft.com/en-us/xamarin/cross-platform/deploy-test/memory-perf-best-practices#implement-asynchronous-operations
     * Parentheses surrounding logarithms
     * Weirdness with complex fractions (ie (x+y)/z) - how to display (fraction vs expression), simplifying
     * More checking in Operand for identities
     * e^x rounding
     * rendering issue for nested exponents (x^2)^2 ?
     * simplify exponentiated terms 2^(2x) = 4^x
     * change inverse trig interpretations?
     * render (sinx)^2 as sin^2(x)
     * Look for uneccessary hashing
     */

    /*public class SmartListener<T, TProperty>
        where T : class
        where TProperty : class
    {
        public T Value { get; private set; }
        private Func<TProperty, T> Property;

        public SmartListener(T value, Func<TProperty, T> property)
        {
            Value = value;
            Property = property;
        }

        public void CheckForChange(TProperty t)
        {
            T temp = Property(t);

            if (Value != temp)
            {
                Value = temp;
            }
        }
    }*/

    public delegate void FocusChangedEventHandler(Calculation oldFocus, Calculation newFocus);
    public delegate void RenderedEventHandler();

    public partial class MainPage : ContentPage
    {
        //private static MainPage VisiblePage { get; private set; }
        //public static AbsoluteLayout CanvasArea => VisiblePage.canvas;
        public static int FontSize => 20;

        public static double parenthesesWidth;

        public static event FocusChangedEventHandler FocusChanged;
        private static Calculation CalculationFocus;

        //public AbsoluteLayout Canvas => canvas;
        //public Xamarin.Forms.ScrollView Scroll => canvasScroll;
        //public AbsoluteLayout PhantomCursorField => phantomCursorField;
        //public TouchScreen Page => page;
        //private RenderedEventHandler OnRendered;

        private Keyboard KeyboardView;
        private CursorView PhantomCursor;
        //How much extra space is in the lower right
        private int padding = 100;
        private bool keyboardDocked = false;
        //private Size canvasSize;

        public MainPage()
        {
            InitializeComponent();

            SettingsMenuSetUp();

            //Set up for keyboard
            WireUpKeyboard(KeyboardView = new Keyboard());
            
            if (Device.Idiom == TargetIdiom.Tablet)
            {
                //phantomCursorField.Children.Add(KeyboardView);
                //KeyboardView.MoveOnAbsoluteLayout(new Point(1, 1), AbsoluteLayoutFlags.PositionProportional);

                phantomCursorField.Children.Add(KeyboardView, new Rectangle(1, 1, -1, -1), AbsoluteLayoutFlags.PositionProportional);
                //KeyboardView.MoveTo(-1000, -1000);
                //AbsoluteLayout.SetLayoutFlags(KeyboardView, AbsoluteLayoutFlags.None);

                canvasScroll.Scrolled += (sender, e) =>
                {
                    AdjustKeyboardPosition();
                };
                //phantomCursorField.HeightRequest = 0;
                /*phantomCursorField.SizeChanged += (sender, e) =>
                {
                    if (canvasSize != null && !keyboardDocked)
                    {
                        if (KeyboardView.Parent == null)
                        {
                            //phantomCursorField.Children.Add(KeyboardView, )
                        }
                        Point pos = new Point(
                            KeyboardView.X / (canvasSize.Width - KeyboardView.Width),
                            KeyboardView.Y / (canvasSize.Height - KeyboardView.Height)
                            );
                        KeyboardView.MoveTo(
                            (phantomCursorField.Width - KeyboardView.Width) * pos.X,
                            (phantomCursorField.Height - KeyboardView.Height) * pos.Y
                            );
                    }

                    canvasSize = phantomCursorField.Bounds.Size;
                };*/
            }
            else
            {
                page.Children.Add(KeyboardView);
            }

            canvas.Touch += AddCalculation;
            FocusChanged += SwitchCalculationFocus;

            //Measuring for cursor
            Label l = new Label()
            {
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start,
                Text = "(",
                FontSize = Text.MaxFontSize,
            };
            l.SizeChanged += delegate
            {
                Text.MaxTextHeight = l.Height;
                parenthesesWidth = l.Width;

                Text.CreateLeftParenthesis = () => new TextImage(new Image() { Source = "leftParenthesis.png", HeightRequest = 0, WidthRequest = parenthesesWidth, Aspect = Aspect.Fill }, "(");
                Text.CreateRightParenthesis = () => new TextImage(new Image() { Source = "rightParenthesis.png", HeightRequest = 0, WidthRequest = parenthesesWidth, Aspect = Aspect.Fill }, ")");
                Text.CreateRadical = () => new Image() { Source = "radical.png", HeightRequest = 0, WidthRequest = parenthesesWidth * 2, Aspect = Aspect.Fill };

                page.Children.Remove(l);

#if SAMPLE
                ScreenShot();
#endif

                //canvas.Children.Add(new Equation("(9)*6"), new Point(100, 100));

                /*canvas.Children.Add(new Expression(Render.Math("log_(4)-9")), new Point(100, 100));
                canvas.Children.Add(new Expression(Render.Math("log_-9(4)")), new Point(200, 200));
                canvas.Children.Add(new Expression(Render.Math("log_-9-4")), new Point(300, 300));*/
                //takeMathTest();
            };
            page.Children.Add(l);

            //VisiblePage = this;

            //Phantom cursor stuff
            PhantomCursor = new CursorView()
            {
                Color = Color.Red,
                IsVisible = false
            };
            phantomCursorField.Children.Add(PhantomCursor);

            SoftKeyboard.Cursor.SizeChanged += (sender, e) => PhantomCursor.HeightRequest = SoftKeyboard.Cursor.Height;

            ScrollCanvas = new Animation(v =>
            {
                canvasScroll.ScrollToAsync(canvasScroll.X + ScrollDirection.X * scrollSpeed, canvasScroll.Y + ScrollDirection.Y * scrollSpeed, false);
            }, 0, 1);

            // Touch stuff
            DescendantAdded += (sender, e) =>
            {
                if (e.Element is Calculation)
                {
                    //(e.Element as Calculation).Touch += DragOnCanvas;
                }
                else if (e.Element is Equation)
                {
                    Equation temp = e.Element as Equation;

                    if (e.Element.GetType() == typeof(Equation))
                    {
                        temp.LHS.Touch += EquationMoveCursor;
                    }
                    if (temp.RHS is Answer)
                    {
                        temp.RHS.Touch += DragAnswer;
                    }

                    temp.Touch += DragOnCanvas;
                }
                else if (e.Element is Link)
                {
                    Link link = e.Element as Link;

                    link.PropertyChanged += (sender1, e1) =>
                    {
                        if (e1.PropertyName == ContentView.ContentProperty.PropertyName)
                        {
                            link.MathContent.Touch += DragLink;
                        }
                    };
                }
            };

            FixDynamicLag("");

#if !DEBUG
            if (Settings.Tutorial)
            {
                Tutorial();
            }
#else
            Settings.Tutorial = false;
            //Tutorial();
#endif
        }

        private readonly int MAX_TUTORIAL_SIZE_ABSOLUTE = 400;
        private readonly double MAX_TUTORIAL_SIZE_PERCENT = 0.75;

        public void Tutorial()
        {
            if (Device.RuntimePlatform == Device.Android)
            {
                OldTutorial();
                return;
            }

            if (Settings.Tutorial)
            {
                //return;
            }

            Settings.Tutorial = true;
            header.IsEnabled = false;
            KeyboardView.IsEnabled = false;
            Tutorial tutorial = new Tutorial();
            Color Background = Color.White;

            Frame frame = new Frame
            {
                Content = tutorial,
                BorderColor = Background,
                BackgroundColor = Background,
                CornerRadius = 10,
                Padding = new Thickness(20, 20, 20, 0),
                //Margin = new Thickness(100),
                VerticalOptions = LayoutOptions.FillAndExpand
            };

            /*if (Device.Idiom == TargetIdiom.Phone)
            {
                screen.Children.Add(frame, new Rectangle(0.5, 0.5, 0.9, -1), AbsoluteLayoutFlags.PositionProportional | AbsoluteLayoutFlags.WidthProportional);
            }
            else
            {
                frame.WidthRequest = 400;
                screen.Children.Add(frame, new Rectangle(0.5, 0.5, -1, -1), AbsoluteLayoutFlags.PositionProportional);
            }*/

            screen.Children.Add(frame, new Rectangle(0.5, 0.5, -1, -1), AbsoluteLayoutFlags.PositionProportional);
            
            EventHandler sizing = (sender, e) =>
            {
                //frame.WidthRequest = -1;
                //frame.HeightRequest = -1;
                //frame.WidthRequest = Math.Min(MAX_ABSOLUTE_TUTORIAL_SIZE, MAX_PERCENT_TUTORIAL_SIZE * screen.Width);
                //frame.HeightRequest = Math.Min(MAX_ABSOLUTE_TUTORIAL_SIZE, MAX_PERCENT_TUTORIAL_SIZE * screen.Height);

                double width = Math.Min(MAX_TUTORIAL_SIZE_ABSOLUTE, MAX_TUTORIAL_SIZE_PERCENT * screen.Width);
                frame.WidthRequest = frame.Width > width ? width : -1;

                double height = Math.Min(MAX_TUTORIAL_SIZE_ABSOLUTE, MAX_TUTORIAL_SIZE_PERCENT * screen.Height);
                frame.HeightRequest = frame.Height > height ? height : -1;
            };
            sizing(null, null);

            screen.SizeChanged += sizing;
            frame.SizeChanged += sizing;

            tutorial.Completed += () =>
            {
                frame.Remove();
                screen.SizeChanged -= sizing;
                header.IsEnabled = true;
                KeyboardView.IsEnabled = true;
                Settings.Tutorial = false;
            };

            //GIF gif = new GIF("test.gif") { WidthRequest = 200, HeightRequest = 200 };
            //header.Children.Add(gif);
            //screen.Children.Add(gif, new Rectangle(0.5, 0.5, -1, -1), AbsoluteLayoutFlags.PositionProportional);
        }

#if DEBUG
        double yPosForRenderTests;
        private void RenderTests(string question, string answer)
        {
            Expression e = new Expression(Reader.Render(question));
            canvas.Children.Add(e, new Point(0, yPosForRenderTests));
            yPosForRenderTests += e.Measure().Height + 50;
            canvas.HeightRequest = yPosForRenderTests;
        }
#endif

        void FixDynamicLag(object o) => Print.Log(o as dynamic);

        public static int MaxBannerWidth = 320;
        //private double canvasWidthHorizontal => KeyboardView.Width;
        private static BannerAd ad;

        private void SettingsMenuSetUp()
        {
            AnythingButton settingsMenuButton = new AnythingButton();
            for (double i = 0.25; i < 1; i += 0.25)
            {
                settingsMenuButton.Children.Add(
                    new BoxView() { Color = Color.Black },
                    new Rectangle(0.5, i, 0.6, 0.075),
                    AbsoluteLayoutFlags.All
                    );
            }
            settingsMenuButton.SetButtonBorderWidth(0);

            if (Device.Idiom == TargetIdiom.Phone)
            {
                Grid grid = new Grid();
                grid.RowDefinitions.Add(new RowDefinition() { Height = 50 });
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = 50 });
                grid.ColumnDefinitions.Add(new ColumnDefinition());
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = 50 });

                StackLayout layout = new StackLayout
                {
                    HorizontalOptions = LayoutOptions.FillAndExpand
                };
                layout.SizeChanged += (sender, e) =>
                {
                    int width = Math.Min(320, (int)layout.Width);

                    if (MaxBannerWidth != width)
                    {
                        MaxBannerWidth = width;

                        if (ad != null)
                        {
                            layout.Children.Remove(ad);
                        }
#if !SAMPLE
                        layout.Children.Add(ad = new BannerAd() { HorizontalOptions = LayoutOptions.Center });
#endif
                    }
                };

                grid.Children.Add(settingsMenuButton, 0, 0);
                grid.Children.Add(layout, 1, 0);

                ContentPage settings = new SettingsPage();
                settings.LayoutChanged += (sender, e) => settings.Padding = settings.On<Xamarin.Forms.PlatformConfiguration.iOS>().SafeAreaInsets();
                settings.Appearing += (sender, e) => settings.Padding = settings.On<Xamarin.Forms.PlatformConfiguration.iOS>().SafeAreaInsets();
                settings.Disappearing += (sender, e) => Settings.Save();
                settingsMenuButton.Button.Clicked += (sender, e) => Navigation.PushAsync(settings, true);

                header.Children.Insert(0, grid);
            }
            else if (Device.Idiom == TargetIdiom.Tablet)
            {
#if !SAMPLE
                Xamarin.Forms.NavigationPage.SetTitleView(this, new BannerAd() { HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center });
#endif
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
                else if (text == Key.UP)
                {
                    key.Clicked += (sender, e) =>
                    {
                        if (!SoftKeyboard.Up())
                        {
                            CalculationFocus.Up();
                        }
                    };
                }
                else if (text == Key.DOWN)
                {
                    key.Clicked += (sender, e) =>
                    {
                        if (!SoftKeyboard.Down())
                        {
                            CalculationFocus.Down();
                        }
                    };
                }
                else if (text == Key.DELETE)
                {
                    key.Clicked += (sender, e) =>
                    {
                        Print.Log("delete button pressed");
                        if (CalculationFocus != null)
                        {
                            SoftKeyboard.Delete();
                        }
                    };
                    key.LongClick += async delegate
                    {
                        if (!Settings.ClearCanvasWarning || await DisplayAlert("Wait!", "Are you sure you want to clear the canvas?", "Yes", "No"))
                        {
                            ClearCanvas();
                        }
                    };
                }
                else if (text == Key.DOCK)
                {
                    key.Clicked += delegate
                    {
                        DockKeyboard(!keyboardDocked);
                    };
                    key.Touch += (sender, e) =>
                    {
                        if (e.State == TouchState.Moving)
                        {
                            DockKeyboard(false);
                            TouchScreen.BeginDrag(keyboard, phantomCursorField);
                        }
                    };
                }
                else
                {
                    key.Clicked += delegate
                    {
                        if (CalculationFocus == null)
                        {
                            AddCalculation(new Point(0, 0), TouchState.Up);
                        }

                        SoftKeyboard.Type(key.Output);
                    };
                    key.LongClick += () =>
                    {
                        Point start;
                        if (Device.Idiom == TargetIdiom.Tablet && !keyboardDocked)
                        {
                            start = new Point(TouchScreen.LastTouch.X, KeyboardView.PositionOn(phantomCursorField).Y - SoftKeyboard.Cursor.Height);
                        }
                        else
                        {
                            start = SoftKeyboard.Cursor.PositionOn(phantomCursorField);
                        }

                        EnterCursorMode(start, 2);
                    };
                }
            }
        }

        private double width = -1;
        private double height = -1;

        protected override void OnSizeAllocated(double width, double height)
        {
            page.Orientation = height > width || Device.Idiom == TargetIdiom.Tablet ? StackOrientation.Vertical : StackOrientation.Horizontal;

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
                oldFocus.SizeChanged -= AdjustKeyboardPosition;
                if (oldFocus.Children.Count == 0)
                {
                    oldFocus.Remove();
                }
            }

            if (newFocus != null)
            {
                newFocus.SizeChanged += AdjustKeyboardPosition;
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

        private void AddCalculation(object sender, TouchEventArgs e) => AddCalculation(e.Point, e.State);

        private void AddCalculation(Point point, TouchState state)
        {
            if (state != TouchState.Up)
            {
                return;
            }

            Calculation calculation = new Calculation() { RecognizeVariables = true };
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

            Equation equation = new Equation();
            SoftKeyboard.MoveCursor(equation.LHS);

            calculation.Add(equation);
            canvas.Children.Add(calculation, point);
        }

        private void DragOnCanvas(object sender, TouchEventArgs e)
        {
            View draggable = sender as Calculation ?? (sender as View)?.Parent<Calculation>() ?? sender as View;
            if (draggable != null && e.State == TouchState.Moving)
            {
                double backup = TouchScreen.FatFinger;
                TouchScreen.FatFinger = 0;
                TouchScreen.BeginDrag(draggable, canvas);
                TouchScreen.FatFinger = backup;
            }
        }

        private void EquationMoveCursor(object sender, TouchEventArgs e)
        {
            if (sender is View && e.State == TouchState.Moving)
            {
                EnterCursorMode((sender as View).PositionOn(phantomCursorField).Add(e.Point));//.Add(new Point(-MainPage.phantomCursor.Width / 2, -Text.MaxTextHeight))));
            }
        }

        private void DragAnswer(object sender, TouchEventArgs e)
        {
            Answer answer = sender as Answer;
            if (answer != null && e.State == TouchState.Moving)
            {
                Link link = new Link(answer);
                link.MathContent.Touch += DragLink;
                TouchScreen.BeginDrag(link, phantomCursorField, answer);
                StartDraggingLink(link);
            }
        }

        private void DragLink(object sender, TouchEventArgs e)
        {
            Link link = (sender as View)?.Parent as Link;
            if (link != null && e.State == TouchState.Moving)
            {
                if (!TouchScreen.Active)
                {
                    StartDraggingLink(link);
                }
                TouchScreen.BeginDrag(link, phantomCursorField);
            }
        }

        private void StartDraggingLink(Link link)
        {
            BoxView placeholder = link.StartDrag();

            TouchScreen.Dragging += (state) =>
            {
                Tuple<Expression, int> target = ExampleDrop(link);

                if (state == DragState.Moving && target != null)
                {
                    target.Item1.Insert(target.Item2, placeholder);
                }
            };
        }

        private void ClearCanvas()
        {
            canvas.Children.Clear();
            canvas.WidthRequest = (canvas.Parent as View).Width;
            canvas.HeightRequest = (canvas.Parent as View).Height;
            if (Device.Idiom == TargetIdiom.Tablet)
            {
                //KeyboardView.MoveTo(-1000, -1000);
                keyboardDocked = true;
            }

            FocusOnCalculation(null);
        }

        private void SetCursorMode(bool onOrOff)
        {
            PhantomCursor.IsVisible = onOrOff;
            KeyboardView.MaskKeys(onOrOff);
        }

        public void EnterCursorMode(Point start, double speed = 1)
        {
            if (CalculationFocus == null)
            {
                return;
            }

            PhantomCursor.HeightRequest = SoftKeyboard.Cursor.Height;

            SetCursorMode(true);

            //Put the cursor in the middle of the screen if it's off the screen
            if (!new Rectangle(Point.Zero, phantomCursorField.Bounds.Size).Contains(new Rectangle(start, PhantomCursor.Bounds.Size)))
            {
                start = new Point((phantomCursorField.Width - PhantomCursor.Width) / 2, (phantomCursorField.Height - PhantomCursor.Height) / 2);
            }
            TouchScreen.BeginDrag(PhantomCursor, phantomCursorField, start, speed);
            
            TouchScreen.Dragging += (state) =>
            {
                if (state == DragState.Ended)
                {
                    ExitCursorMode();
                }
                else
                {
                    Tuple<Expression, int> target = ExampleDrop(PhantomCursor);
                    if (target != null)
                    {
                        SoftKeyboard.MoveCursor(target.Item1, target.Item2);
                    }
                }
            };
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
            //Drag.Dragging -= MoveCursor;
            //page.Touch -= MoveCursor;
        }

        //private Thread thread;
        private int shouldScrollX => (int)Math.Truncate(PhantomCursor.X / (canvasScroll.Width - PhantomCursor.Width) * 2 - 1);
        private int shouldScrollY => (int)Math.Truncate(PhantomCursor.Y / (canvasScroll.Height - PhantomCursor.Height) * 2 - 1);
        private readonly double scrollSpeed = 0.025;
        private double preciseScrollX, preciseScrollY;

        private void scrollCanvas()
        {
            preciseScrollX = canvasScroll.ScrollX;
            preciseScrollY = canvasScroll.ScrollY;
            while (shouldScrollX + shouldScrollY != 0 && PhantomCursor.IsVisible)
            {
                preciseScrollX += shouldScrollX * scrollSpeed;
                preciseScrollY += shouldScrollY * scrollSpeed;
                canvasScroll.ScrollToAsync(preciseScrollX, preciseScrollY, false);
            }
        }

        private Point ScrollDirection = Point.Zero;
        private Animation ScrollCanvas;

        public Tuple<Expression, int> ExampleDrop(View dragging)
        {
            /*if (Device.RuntimePlatform != Device.iOS && (thread == null || !thread.IsAlive) && shouldScrollX + shouldScrollY != 0)
            {
                thread = new Thread(scrollCanvas);
                thread.Start();
            }*/
            //ScrollCanvas.Commit(this, "ScrollCanvas", length / 255, length, Easing.Linear, (v, c) => Value.BackgroundColor = Color.Transparent, () => false);

            //Get the coordinates of the cursor relative to the entire screen
            Point loc;
            Point temp = new Point(canvasScroll.ScrollX + dragging.X + dragging.Width / 2, canvasScroll.ScrollY + dragging.Y + dragging.Height / 2);
            View view = GetViewAt(canvas, temp, out loc);
            
            if (view == null)
            {
                return null;
            }

            int leftOrRight = (int)Math.Round(loc.X / view.Width);

            if (view.GetType() == typeof(Expression))
            {
                Expression e = view as Expression;

                if (e.Editable && loc.X <= e.PadLeft || loc.X >= e.Width - e.PadRight)
                {
                    return new Tuple<Expression, int>(e, Math.Min(e.ChildCount(), e.ChildCount() * leftOrRight));
                }
            }
            else if (view.Parent is Expression)
            {
                Expression parent = view.Parent as Expression;

                if (parent.Editable)
                {
                    return new Tuple<Expression, int>(parent, parent.IndexOf(view) + leftOrRight);
                }
            }

            /*if (view != null && ((view.GetType() == typeof(Expression) && (view as Expression).Editable) || view.Parent is Expression))
            {

                if (view.GetType() == typeof(Expression))
                {
                    Expression e = view as Expression;
                    if (loc.X <= e.PadLeft || loc.X >= e.Width - e.PadRight)
                    {
                        //SoftKeyboard.MoveCursor(e, Math.Min(e.ChildCount(), e.ChildCount() * leftOrRight));
                        return new Tuple<Expression, int>(e, Math.Min(e.ChildCount(), e.ChildCount() * leftOrRight));
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (view.Parent is Expression)
                {
                    Expression parent = view.Parent as Expression;
                    return new Tuple<Expression, int>(parent, parent.IndexOf(view) + leftOrRight);
                    //SoftKeyboard.MoveCursor((view as Text).Parent, (view.Parent as Expression).IndexOf(view) + leftOrRight);
                }
            }*/

            return null;
        }

        private View GetViewAt(Layout<View> layout, Point point, out Point scaled)
        {
            //int i = 0;
            //for (; i < layout.Children.Count; i++)
            foreach (View child in layout.Children)
            {
                //View child = layout.Children[i];

                //Is the point inside the bounds that this child occupies?
                //if (pos.X >= child.X && pos.X <= child.X + child.Width && pos.Y >= child.Y && pos.Y <= child.Y + child.Height)
                if (child.Bounds.Contains(point))
                {
                    point = point.Subtract(child.Bounds.Location);

                    if (child is Layout<View>)
                    {
                        return GetViewAt(child as Layout<View>, point, out scaled);
                    }
                    else if (layout.Editable())
                    {
                        scaled = point;
                        return child;
                    }

                    /*else if (parent.Editable())
                    {
                        ans = child;
                    }*/

                    //break;
                }
            }

            /*Expression e = parent as Expression;
            if (i == parent.Children.Count && e != null && e.Editable && (pos.X <= e.PadLeft || pos.X >= e.Width - e.PadRight))
            {
                ans = parent;
            }*/

            scaled = point;
            return layout;
        }
    }
}
 