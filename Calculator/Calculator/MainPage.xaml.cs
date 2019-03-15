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
     * v2.1.4/5
     *      iOS Drag calculations on canvas
     * Answer shouldn't drag on iOS
     * iOS radical and parenthesis images
     * iOS app icon and splash screen
     *      Bug with sin in denominator
     * Ads not showing up
     * On Android look at - ad/menu button, keyboard permanent keys, buttons in settings
     * Orientation change with keyboard undocked
     * Tutorial - explain scrolling canvas, moving equations, fractions?
     * Add equations by typing
     * 
     * v
     * Drag class shouldn't be static
     * 
     * v2.2
     * Buggy tutorial - drag on keyboard with no calculations before entering tutorial, tap anywhere with two calculations before entering tutorial
     * Drag and drop equations
     * Make substituted variables equations
     * Color scheme
     * Substituted variable equations need to be mathviews
     * 
     * v?
     * Add equation on start typing, without clicking
     * Move cursor by touching equation
     * Parentheses surrounding logarithms
     * Weirdness with complex fractions (ie (x+y)/z) - how to display (fraction vs expression), simplifying
     * More checking in Operand for identities
     * e^x rounding
     * Choice for expanded/scrolling keyboard
     * rendering issue for nested exponents (x^2)^2 ?
     * simplify exponentiated terms 2^(2x) = 4^x
     * change inverse trig interpretations?
     * render (sinx)^2 as sin^2(x)
     * Look for uneccessary hashing
     */

    public delegate void FocusChangedEventHandler(Calculation oldFocus, Calculation newFocus);
    public delegate void RenderedEventHandler();

    public partial class MainPage : ContentPage
    {
        public static MainPage VisiblePage { get; private set; }
        public static event FocusChangedEventHandler FocusChanged;
        public static Calculation CalculationFocus { get; private set; }
        public static CursorView phantomCursor { get; private set; }
        public static int FontSize => 20;

        private static bool keyboardDocked = true;

        public Layout Canvas => canvas;
        public TouchScreen Page => page;
        public RenderedEventHandler OnRendered;

        private Keyboard KeyboardView;
        //How much extra space is in the lower right
        private int padding = 100;
        private Size canvasSize;

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
                phantomCursorField.HeightRequest = 0;
                phantomCursorField.SizeChanged += (sender, e) =>
                {
                    if (canvasSize != null && !keyboardDocked)
                    {
                        Point pos = new Point(
                            KeyboardView.X / (canvasSize.Width - KeyboardView.Width),
                            KeyboardView.Y / (canvasSize.Height - KeyboardView.Height)
                            );
                        KeyboardView.MoveTo(
                            (phantomCursorField.Width - KeyboardView.Width) * pos.X,
                            (phantomCursorField.Height - KeyboardView.Height) * pos.Y
                            );
                    }

                    canvasSize = new Size(phantomCursorField.Width, phantomCursorField.Height);
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

                Text.CreateLeftParenthesis = () => new TextImage(new Image() { Source = "leftParenthesis.png", HeightRequest = 0, WidthRequest = parenthesesWidth, Aspect = Aspect.Fill }, "(");
                Text.CreateRightParenthesis = () => new TextImage(new Image() { Source = "rightParenthesis.png", HeightRequest = 0, WidthRequest = parenthesesWidth, Aspect = Aspect.Fill }, ")");
                Text.CreateRadical = () => new Image() { Source = "radical.png", HeightRequest = 0, WidthRequest = parenthesesWidth * 2, Aspect = Aspect.Fill };

                page.Children.Remove(l);

                /*canvas.Children.Add(new Expression(Render.Math("log_(4)-9")), new Point(100, 100));
                canvas.Children.Add(new Expression(Render.Math("log_-9(4)")), new Point(200, 200));
                canvas.Children.Add(new Expression(Render.Math("log_-9-4")), new Point(300, 300));*/
                //takeMathTest();
            };

            //Appearing += (sender, e) => SafePadding(this);
            //LayoutChanged += (sender, e) => SafePadding(this);

            VisiblePage = this;
            //Drag.Screen = page;

            //Phantom cursor stuff
            phantomCursor = new CursorView()
            {
                Color = Color.Red,
                IsVisible = false
            };
            phantomCursorField.Children.Add(phantomCursor);
            
            SoftKeyboard.Cursor.SizeChanged += (sender, e) => phantomCursor.HeightRequest = SoftKeyboard.Cursor.Height;

            FixDynamicLag("");
            print.log("main page constructor finished");

            /*canvas.Children.Add(new BoxView { BackgroundColor = Color.White }, new Rectangle(0, 0, 1, 0.2), AbsoluteLayoutFlags.All);
            canvas.Children.Add(new BoxView { BackgroundColor = new Color(240, 240, 240) }, new Rectangle(0, 0.2, 1, 0.2), AbsoluteLayoutFlags.All);
            canvas.Children.Add(new BoxView { BackgroundColor = Color.FromHex("#ebeae8") }, new Rectangle(0, 0.4, 1, 0.2), AbsoluteLayoutFlags.All);
            canvas.Children.Add(new BoxView { BackgroundColor = Color.NavajoWhite }, new Rectangle(0, 0.6, 1, 0.2), AbsoluteLayoutFlags.All);
            canvas.Children.Add(new BoxView { BackgroundColor = Color.WhiteSmoke }, new Rectangle(0, 1, 1, 0.2), AbsoluteLayoutFlags.All);*/

#if !DEBUG
            if (Settings.Tutorial)
            {
                Tutorial();
            }
#endif
        }

        void FixDynamicLag(object o) => print.log(o as dynamic);
        private readonly int MenuButtonWidth = 44;
        private double SettingsMenuWidth = 400;

        public static int MaxBannerWidth = 320;
        private double canvasWidthHorizontal => KeyboardView.Width;
        private static BannerAd ad;

        private void SettingsMenuSetUp()
        {
            AnythingButton settingsMenuButton = new AnythingButton();
            for (double i = 0.25; i < 1; i += 0.25)
            {
                settingsMenuButton.Children.Add(new BoxView() { Color = Color.Black, InputTransparent = true }, new Rectangle(0.5, i, 0.6, 0.075), AbsoluteLayoutFlags.All);
            }
            settingsMenuButton.SetButtonBorderWidth(0);

            if (Device.Idiom == TargetIdiom.Phone)
            {
                Grid grid = new Grid();
                grid.RowDefinitions.Add(new RowDefinition() { Height = 50 });
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = 50 });
                grid.ColumnDefinitions.Add(new ColumnDefinition());
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = 50 });

                StackLayout layout = new StackLayout { HorizontalOptions = LayoutOptions.FillAndExpand };
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
                        layout.Children.Add(ad = new BannerAd() { HorizontalOptions = LayoutOptions.Center });
                    }
                };

                grid.Children.Add(settingsMenuButton, 0, 0);
                grid.Children.Add(layout, 1, 0);

                ContentPage settings = new SettingsPage();
                //settings.Appearing += (sender, e) => SafePadding(settings);
                //settings.LayoutChanged += (sender, e) => SafePadding(settings);
                settings.LayoutChanged += (sender, e) => settings.Padding = settings.On<Xamarin.Forms.PlatformConfiguration.iOS>().SafeAreaInsets();
                settings.Appearing += (sender, e) => settings.Padding = settings.On<Xamarin.Forms.PlatformConfiguration.iOS>().SafeAreaInsets();
                settings.Disappearing += (sender, e) => Settings.Save();
                settingsMenuButton.Clicked += (sender, e) => Navigation.PushAsync(settings, true);

                header.Children.Insert(0, grid);

                /*AbsoluteLayout layout = new AbsoluteLayout() { HorizontalOptions = LayoutOptions.FillAndExpand };
                layout.Children.Add(settingsMenuButton, new Rectangle(0, 0.5, MenuButtonWidth, MenuButtonWidth), AbsoluteLayoutFlags.YProportional);
                //layout.SizeChanged += (sender, e) => layout.Children.Add(settingsMenuButton, new Rectangle(0, 0, layout.Height, layout.Height), AbsoluteLayoutFlags.None);
                layout.SizeChanged += delegate
                {
                    MaxBannerWidth = Math.Min(320, (int)(layout.Width - (MenuButtonWidth + 10) * 2));
                    //layout.HeightRequest = Math.Max((int)Math.Ceiling(MaxBannerWidth * 50.0 / 320.0), MenuButtonWidth + 10);

                    if (ad != null)
                    {
                        layout.Children.Remove(ad);
                    }
                    layout.Children.Add(ad = new BannerAd(), new Rectangle(0.5, 0.5, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize), AbsoluteLayoutFlags.PositionProportional);
                };

                header.Children.Insert(0, layout);*/
                //NavigationPage.SetTitleView(this, layout);
            }
            else if (Device.Idiom == TargetIdiom.Tablet)
            {
                Xamarin.Forms.NavigationPage.SetTitleView(this, new BannerAd() { HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center });
                //page.InterceptedTouch += (point, state) => (App.Current.MainPage as MasterDetailPage).IsPresented = false;

                //screen.Children.Add(new BannerAd(), new Rectangle(0.5, 0, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize), AbsoluteLayoutFlags.PositionProportional);

                /*SettingsMenu settings = new SettingsMenu() { BackgroundColor = Color.WhiteSmoke };
                screen.Children.Add(settings, new Rectangle(0, 0, SettingsMenuWidth, 1), AbsoluteLayoutFlags.HeightProportional);
                screen.Children.Add(settingsMenuButton, new Rectangle(0, 0, MenuButtonWidth, MenuButtonWidth), AbsoluteLayoutFlags.None);

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
                settings.SetVisible = setVisibility;

                settingsMenuButton.Clicked += (sender, e) => setVisibility(settings.TranslationX < 0);
                page.InterceptedTouch += (point, state) => setVisibility(false);

                setVisibility(false);*/
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
                        if (CalculationFocus == null)
                        {
                            AddCalculation(new Point(0, 0), TouchState.Up);
                        }

                        SoftKeyboard.Type(key.Output);
                        CalculationFocus.SetAnswer();
                    };
                    key.LongClick += () => EnterCursorMode(KeyboardCursorMode(), 2);
                }
            }
        }

        private double width = -1;
        private double height = -1;
        //private int VERTICAL_PAGE_PADDING => Device.RuntimePlatform == Device.iOS && page.Orientation == StackOrientation.Vertical ? 0 : 10;

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
                if (oldFocus.Main.LHS.ChildCount() == 0)
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

        private void ClearCanvas()
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

        private void SetCursorMode(bool onOrOff)
        {
            phantomCursor.IsVisible = onOrOff;
            KeyboardView.MaskKeys(onOrOff);
        }

        private Point KeyboardCursorMode()
        {
            if (Device.Idiom == TargetIdiom.Tablet && !keyboardDocked)
            {
                return new Point(Drag.LastTouch.X, KeyboardView.Y - SoftKeyboard.Cursor.Height).Add(new Point(canvasScroll.ScrollX, canvasScroll.ScrollY)); ;
                //phantomCursor.MoveTo(Drag.LastTouch.X, KeyboardView.Y - SoftKeyboard.Cursor.Height);
            }
            else
            {
                Point temp = SoftKeyboard.Cursor.PositionOn(canvas);
                return temp;
                if (temp.X >= 0 && temp.X <= canvasScroll.Width && temp.Y >= 0 && temp.Y <= canvas.Height)
                {
                    phantomCursor.MoveTo(temp);
                }
            }
        }

        public void EnterCursorMode(Point point, int speed = 1)
        {
            if (CalculationFocus == null)
            {
                return;
            }

            point = point.Add(new Point(-canvasScroll.ScrollX, -canvasScroll.ScrollY));

            if (point.X < 0 || point.X > canvasScroll.Width || point.Y < 0 || point.Y > canvasScroll.Height)
            {
                point = new Point((canvasScroll.Width - phantomCursor.Width) / 2, (canvasScroll.Height - phantomCursor.Height) / 2);
            }

            phantomCursor.MoveTo(point);
            phantomCursor.HeightRequest = SoftKeyboard.Cursor.Height;

            SetCursorMode(true);
            phantomCursor.BeginDrag(phantomCursorField.Bounds, speed);
            Drag.Dragging += MoveCursor;
            //page.Touch += MoveCursor;
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
            Drag.Dragging -= MoveCursor;
            //page.Touch -= MoveCursor;
        }

        private Thread thread;
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

        private void MoveCursor(Drag.State state)
        {
            if (state == Drag.State.Ended)
            {
                ExitCursorMode();
            }
            else
            {
                /*if (Device.RuntimePlatform != Device.iOS && (thread == null || !thread.IsAlive) && shouldScrollX + shouldScrollY != 0)
                {
                    thread = new Thread(scrollCanvas);
                    thread.Start();
                }*/

                //Get the coordinates of the cursor relative to the entire screen
                Point loc = new Point(canvasScroll.ScrollX + phantomCursor.X + phantomCursor.Width / 2, canvasScroll.ScrollY + phantomCursor.Y + phantomCursor.Height / 2);
                View view = GetViewAt(canvas, ref loc);

                if (view != null && (view is Text || view.GetType() == typeof(Expression)))
                {
                    int leftOrRight = (int)Math.Round(loc.X / view.Width);

                    if (view.GetType() == typeof(Expression))
                    {
                        Expression e = view as Expression;
                        SoftKeyboard.MoveCursor(e, Math.Min(e.ChildCount(), e.ChildCount() * leftOrRight));
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
    }
}
 