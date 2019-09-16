using System;
using System.Extensions;
using Xamarin.Forms;
using Xamarin.Forms.Extensions;
using Xamarin.Forms.MathDisplay;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

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
     * v3.0
     * Makeover - color scheme, better icons
     * Basic calculator mode
     * 
     * v2.3
     * Variable row
     * Access to system keyboard
     * Choice for expanded/scrolling keyboard
     * Long press on arrow keys to send multiple key presses
     * Remember keyboard position on tablet
     * Tablet keyboard doesn't follow moved calculation
     * Dock preference reset on canvas cleared
     * 
     * v2.4
     * Predefined functions (quadratic formula, physics stuff), ability to add custom ones
     * 
     * v.?
     * Native drag and drop
     * Flick to delete
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
     *      Adjust TouchScreen.FatFinger
     * Page margin adjustment in MainPage SettingsMenuSetup method (should be handled in App class, not necessary)
     * iPad split view
     * Double tap to prevent scroll back
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
        public static int FontSize => 20;

        public static double ParenthesesWidth;

        public static event FocusChangedEventHandler FocusChanged;
        private static Calculation CalculationFocus;

        private CursorView PhantomCursor;
        //How much extra space is in the lower right
        private int ExtraPadding = 100;

        //private CrunchKeyboard VirtualKeyboard;
        private Layout<View> KeyboardView;
        private Layout<View> FullKeyboardView;
        private AbsoluteLayout KeyboardMask;

        private bool IsKeyboardDocked => Settings.KeyboardPosition.Equals(KeyboardHidden);
        private readonly Point KeyboardHidden = new Point(-100, -100);

        public MainPage()
        {
            InitializeComponent();

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
                double parenthesesWidth = l.Width;
                ParenthesesWidth = parenthesesWidth;

                Text.CreateLeftParenthesis = () => new TextImage(new Image() { Source = "leftParenthesis.png", HeightRequest = 0, WidthRequest = parenthesesWidth, Aspect = Aspect.Fill }, "(");
                Text.CreateRightParenthesis = () => new TextImage(new Image() { Source = "rightParenthesis.png", HeightRequest = 0, WidthRequest = parenthesesWidth, Aspect = Aspect.Fill }, ")");
                Text.CreateRadical = () => new Image() { Source = "radical.png", HeightRequest = 0, WidthRequest = parenthesesWidth * 2, Aspect = Aspect.Fill };

                page.Children.Remove(l);
            };
            page.Children.Add(l);

            SettingsMenuSetUp();

            CrunchKeyboard crunchKeyboard = new CrunchKeyboard();

            AbsoluteLayout maskedKeys = new AbsoluteLayout
            {
                BindingContext = crunchKeyboard,
            };
            maskedKeys.SetBinding(IsVisibleProperty, "IsVisible");
            maskedKeys.Children.Add(crunchKeyboard);
            maskedKeys.Children.Add(KeyboardMask = new AbsoluteLayout
            {
                IsVisible = false,
                BackgroundColor = Color.Gray,
                Opacity = 0.875
            }, new Rectangle(0, 0, 1, 1), AbsoluteLayoutFlags.All);

            VariableRow variables = new VariableRow
            {
                BindingContext = crunchKeyboard,
            };
            variables.SetBinding(VariableRow.SpacingProperty, "Spacing");
            crunchKeyboard.SizeChanged += (sender, e) =>
            {
                variables.ButtonSize = crunchKeyboard.ButtonWidth(crunchKeyboard.Width, crunchKeyboard.Columns) / 2;
            };

            page.PropertyChanged += (sender, e) =>
            {
                if (!e.IsProperty(StackLayout.OrientationProperty))
                {
                    return;
                }

                StackOrientation opposite = (StackOrientation)(1 - ((int)page.Orientation));
                crunchKeyboard.Orientation = opposite;
            };
            // Make sure orientation property changed gets triggered
            page.Orientation = StackOrientation.Horizontal;
            page.Orientation = StackOrientation.Vertical;

            if (Device.Idiom == TargetIdiom.Tablet)
            {
                FullKeyboardView = maskedKeys;
                phantomCursorField.Children.Add(variables);

                FullKeyboardView = new StackLayout
                {
                    Orientation = StackOrientation.Vertical
                };
                FullKeyboardView.Children.Add(maskedKeys);

                /*maskedKeys.Children.Add(variables);
                variables.SizeChanged += (sender, e) =>
                {
                    if (variables.Orientation == StackOrientation.Horizontal)
                    {
                        AbsoluteLayout.SetLayoutBounds(variables, new Rectangle(1, -variables.Height, -1, -1));
                        AbsoluteLayout.SetLayoutFlags(variables, AbsoluteLayoutFlags.XProportional);
                    }
                    else
                    {
                        AbsoluteLayout.SetLayoutBounds(variables, new Rectangle(-variables.Width, 1, -1, -1));
                        AbsoluteLayout.SetLayoutFlags(variables, AbsoluteLayoutFlags.YProportional);
                    }
                };*/

                FullKeyboardView.PropertyChanged += (sender, e) =>
                {
                    if (IsKeyboardDocked || e.PropertyName != AbsoluteLayout.LayoutBoundsProperty.PropertyName)
                    {
                        return;
                    }

                    Settings.KeyboardPosition = AbsoluteLayout.GetLayoutBounds(FullKeyboardView).Location;
                };

                phantomCursorField.Children.Add(FullKeyboardView, new Rectangle(Settings.KeyboardPosition, new Size(-1, -1)), AbsoluteLayoutFlags.PositionProportional);

                canvasScroll.Scrolled += (sender, e) =>
                {
                    AdjustKeyboardPosition();
                };
            }
            else
            {
                phantomCursorField.Children.Add(variables, new Rectangle(1, 1, -1, -1), AbsoluteLayoutFlags.PositionProportional);
                page.Children.Add(FullKeyboardView = maskedKeys);
            }

            KeyboardManager.KeyboardChanged += (keyboard) =>
            {
                variables.RemoveBinding(StackLayout.OrientationProperty);

                if (keyboard == crunchKeyboard)
                {
                    variables.SetBinding(StackLayout.OrientationProperty, "Orientation");
                }
                else if (keyboard == SystemKeyboard.Instance)
                {
                    variables.Orientation = StackOrientation.Horizontal;
                }

                if (Device.Idiom == TargetIdiom.Tablet)
                {
                    variables.Remove();

                    if (keyboard == crunchKeyboard)
                    {
                        FullKeyboardView.Children.Insert(0, variables);
                        variables.HorizontalOptions = LayoutOptions.End;
                    }
                    else if (keyboard == SystemKeyboard.Instance)
                    {
                        phantomCursorField.Children.Add(variables, new Rectangle(1, 1, -1, -1), AbsoluteLayoutFlags.PositionProportional);
                    }
                }
            };

            //Set up for keyboards
            SystemKeyboard.Setup(screen);
            WireUpKeyboard(crunchKeyboard);
            KeyboardManager.AddKeyboard(SystemKeyboard.Instance, crunchKeyboard);

            canvas.Touch += AddCalculation;
            FocusChanged += SwitchCalculationFocus;

#if SAMPLE
            ScreenShot();
#endif

            //canvas.Children.Add(new Equation("(9)*6"), new Point(100, 100));

            /*canvas.Children.Add(new Expression(Render.Math("log_(4)-9")), new Point(100, 100));
            canvas.Children.Add(new Expression(Render.Math("log_-9(4)")), new Point(200, 200));
            canvas.Children.Add(new Expression(Render.Math("log_-9-4")), new Point(300, 300));*/
            //takeMathTest();

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

                    temp.Touch += MoveCalculation;
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
            phantomCursorField.IsEnabled = false;
            FullKeyboardView.IsEnabled = false;
            Tutorial tutorial = new Tutorial();
            Color Background = Color.White;

            Frame frame = new Frame
            {
                Content = tutorial,
                BorderColor = Background,
                BackgroundColor = Background,
                CornerRadius = 10,
                Padding = new Thickness(20, 20, 20, 0),
                VerticalOptions = LayoutOptions.FillAndExpand
            };

            screen.Children.Add(frame, new Rectangle(0.5, 0.5, -1, -1), AbsoluteLayoutFlags.PositionProportional);

            EventHandler sizing = (sender, e) =>
            {
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
                phantomCursorField.IsEnabled = true;
                FullKeyboardView.IsEnabled = true;
                Settings.Tutorial = false;
            };
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
        private static BannerAd ad;

        private FunctionsDrawer FunctionsDrawer;

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
            settingsMenuButton.Button.BorderWidth = 0;

            /*FunctionsDrawer = new FunctionsDrawer(canvas) { };
            screen.Children.Add(FunctionsDrawer, new Rectangle(-0, 0, -1, 1), AbsoluteLayoutFlags.HeightProportional);

            Button functionsMenuButton = new Button
            {
                Text = "f(x)",
                FontSize = 25,
            };
            functionsMenuButton.Clicked += (sender, e) =>
            {
                FunctionsDrawer.ChangeStatus();
            };*/

            if (Device.Idiom == TargetIdiom.Phone)
            {
                Grid grid = new Grid();
                grid.RowDefinitions.Add(new RowDefinition() { Height = 50 });
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = 50 });
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
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
                //grid.Children.Add(functionsMenuButton, 2, 0);

                ContentPage settings = new SettingsPage();
                settings.LayoutChanged += (sender, e) => settings.Padding = settings.On<Xamarin.Forms.PlatformConfiguration.iOS>().SafeAreaInsets();
                settings.Appearing += (sender, e) => settings.Padding = settings.On<Xamarin.Forms.PlatformConfiguration.iOS>().SafeAreaInsets();
                settings.Disappearing += (sender, e) => Settings.Save();
                settingsMenuButton.Button.Clicked += (sender, e) => Navigation.PushAsync(settings, true);

                //header.Children.Insert(0, grid);
                phantomCursorField.Children.Add(grid, new Rectangle(0, 0, 1, -1), AbsoluteLayoutFlags.PositionProportional | AbsoluteLayoutFlags.WidthProportional);
            }
            else if (Device.Idiom == TargetIdiom.Tablet)
            {
#if !SAMPLE
                Xamarin.Forms.NavigationPage.SetTitleView(this, new BannerAd() { HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center });
#endif
            }
        }

        private int RepeatedKeyPressSpeed = 100;
        private bool KeyDown = false;

        private void WireUpKeyboard(CrunchKeyboard keyboard)
        {
            foreach (Key key in keyboard)
            {
                string text = key.Output;

                if (key is CursorKey)// || text == KeyboardManager.BACKSPACE.ToString())
                {
                    CursorKey cursorKey = key as CursorKey;

                    if (cursorKey == null ||
                        cursorKey.Key == KeyboardManager.CursorKey.Left ||
                        cursorKey.Key == KeyboardManager.CursorKey.Right ||
                        cursorKey.Key == KeyboardManager.CursorKey.Up ||
                        cursorKey.Key == KeyboardManager.CursorKey.Down)
                    {
                        key.LongClick += async (sender, e) =>
                        {
                            Key key1 = sender as Key;
                            KeyDown = true;

                            while (KeyDown)
                            {
                                if (key1 is CursorKey)
                                {
                                    KeyboardManager.MoveCursor((key1 as CursorKey).Key);
                                }
                                else
                                {
                                    KeyboardManager.Type(key1.Output);
                                }

                                await System.Threading.Tasks.Task.Delay(RepeatedKeyPressSpeed);
                            }
                        };
                        key.Touch += (sender, e) =>
                        {
                            Print.Log("touching key", e.State);
                            if (e.State == TouchState.Up)
                            {
                                KeyDown = false;
                            }
                        };
                    }
                }

                if (text == KeyboardManager.BACKSPACE.ToString())
                {
                    key.LongClick += async (sender, e) =>
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
                        DockKeyboard(!IsKeyboardDocked);
                    };
                    key.Touch += (sender, e) =>
                    {
                        if (e.State == TouchState.Moving)
                        {
                            DockKeyboard(false);
                            TouchScreen.BeginDrag(FullKeyboardView, phantomCursorField);
                        }
                    };
                }
                else if (!(key is CursorKey))
                {
                    key.LongClick += (sender, e) =>
                    {
                        if (SoftKeyboard.Cursor.Parent == null)
                        {
                            return;
                        }

                        Point start;
                        if (Device.Idiom == TargetIdiom.Tablet && !IsKeyboardDocked)
                        {
                            start = new Point(TouchScreen.LastTouch.X, FullKeyboardView.PositionOn(phantomCursorField).Y - SoftKeyboard.Cursor.Height);
                        }
                        else
                        {
                            start = SoftKeyboard.Cursor.PositionOn(phantomCursorField);
                        }

                        EnterCursorMode(start, 2);
                    };
                }
            }

            KeyboardManager.CursorMoved += (key) =>
            {
                if (CalculationFocus == null)
                {
                    return;
                }

                if (key == KeyboardManager.CursorKey.Left)
                {
                    SoftKeyboard.Left();
                }
                else if (key == KeyboardManager.CursorKey.Right)
                {
                    SoftKeyboard.Right();
                }
                else if (key == KeyboardManager.CursorKey.Up)
                {
                    if (!SoftKeyboard.Up())
                    {
                        CalculationFocus?.Up();
                    }
                }
                else if (key == KeyboardManager.CursorKey.Down)
                {
                    if (!SoftKeyboard.Down())
                    {
                        CalculationFocus?.Down();
                    }
                }
            };

            KeyboardManager.Typed += (keystroke) =>
            {
                if (keystroke == KeyboardManager.BACKSPACE)
                {
                    Print.Log("delete button pressed");
                    if (CalculationFocus != null)
                    {
                        SoftKeyboard.Delete();
                    }
                }
                else
                {
                    if (CalculationFocus == null)
                    {
                        AddCalculation(new Point(0, Device.Idiom == TargetIdiom.Phone ? 50 : 0), TouchState.Up);
                    }

                    SoftKeyboard.Type(keystroke.ToString());
                }
            };
        }

        private double width = -1;
        private double height = -1;

        protected override void OnSizeAllocated(double width, double height)
        {
            //Print.Log("\n\n\ncurrent orientation: " + page.Orientation + "\nnew orientation: " + (height > width || Device.Idiom == TargetIdiom.Tablet ? StackOrientation.Vertical : StackOrientation.Horizontal));
            page.Orientation = height > width || Device.Idiom == TargetIdiom.Tablet ? StackOrientation.Vertical : StackOrientation.Horizontal;

            base.OnSizeAllocated(width, height);
            //Print.Log("\n\n\n\norientation change");
            if (this.width != width || this.height != height)
            {
                this.width = width;
                this.height = height;

                ExtraPadding = (int)Math.Min(page.Width, page.Height);
            }
        }

        private void DockKeyboard(bool isDocked)
        {
            if (isDocked)
            {
                Settings.KeyboardPosition = new Point(KeyboardHidden.X, KeyboardHidden.Y);
                AdjustKeyboardPosition();
            }
            else
            {
                Settings.KeyboardPosition = AbsoluteLayout.GetLayoutBounds(FullKeyboardView).Location;
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
            if (Device.Idiom == TargetIdiom.Tablet && IsKeyboardDocked && CalculationFocus != null)
            {
                FullKeyboardView.MoveTo(CalculationFocus.X - canvasScroll.ScrollX, CalculationFocus.Y + CalculationFocus.Height - canvasScroll.ScrollY);
            }
        }

        private void AddCalculation(object sender, TouchEventArgs e) => AddCalculation(e.Point, e.State);

        public void AddCalculation(Point point, TouchState state)
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

                if (canvas.Width - p.X < ExtraPadding)
                {
                    canvas.WidthRequest = p.X + ExtraPadding;
                }
                if (canvas.Height - p.Y < ExtraPadding)
                {
                    canvas.HeightRequest = p.Y + ExtraPadding;
                }
            };

            Equation equation = new Equation();
            SoftKeyboard.MoveCursor(equation.LHS);

            calculation.Add(equation);
            canvas.Children.Add(calculation, point);
        }

        private void MoveCalculation(object sender, TouchEventArgs e)
        {
            View draggable = sender as Calculation ?? (sender as View)?.Parent<Calculation>() ?? sender as View;
            if (draggable != null && e.State == TouchState.Moving)
            {
                double backup = TouchScreen.FatFinger;
                TouchScreen.FatFinger = 0;
                TouchScreen.BeginDrag(draggable, canvas);
                TouchScreen.FatFinger = backup;

                TouchScreen.Dragging += (e1) =>
                {
                    if (e1 != DragState.Ended)
                    {
                        return;
                    }

                    AdjustKeyboardPosition();
                };
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

            if (IsKeyboardDocked)
            {
                AbsoluteLayout.SetLayoutBounds(FullKeyboardView, new Rectangle(KeyboardHidden, new Size(-1, -1)));
            }

            FocusOnCalculation(null);
        }

        private void SetCursorMode(bool onOrOff)
        {
            PhantomCursor.IsVisible = onOrOff;
            KeyboardMask.IsVisible = onOrOff;
        }

        public void ClearOverlay() => KeyboardMask.Children.Clear();
        public void Overlay(View view, Rectangle bounds, AbsoluteLayoutFlags flags = AbsoluteLayoutFlags.None) => KeyboardMask.Children.Add(view, bounds, flags);

        public void EnterCursorMode(Point start, double speed = 1)
        {
            if (CalculationFocus == null)
            {
                return;
            }

            //PhantomCursor.HeightRequest = SoftKeyboard.Cursor.Height;

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
            while (!(root is Calculation))
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
 