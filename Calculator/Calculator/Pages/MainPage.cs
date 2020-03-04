using System;
using System.Extensions;
using System.Runtime.CompilerServices;
using Xamarin.Forms;
using Xamarin.Forms.Extensions;
using Xamarin.Forms.MathDisplay;

namespace Calculator
{
    //Color 560297
    //&#8801; - hamburger menu ≡
    //&#8942; - kabob menu ⋮
    //-no-snapshot
    //rryg-ylpj-xxex-fvqu
    // 🌐|⟨|⟩|√|🔗|⭧|🡕

    //Parentheses pictures - 500 font in Word Roboto (80,80,80), default Android ratio is 114 * 443 - trim and resize in Paint to 369 * 1127
    //Radical pictures - 667 font in Paint Segoe UI Symbol (copy from Graphemica), trim completely (609 * 1114)
    //Radical - right half is about 1/2 of horizontal, bottom left takes up about 1/2 of vertical, thickness is 0.1, horizontal bar is about 1/3 of horizontal

    /* TODO:
     * v2.4.2
     * Keyboard paging
     * Crash on rotation from landscape to portrait from settings page
     * Converted show tips dialog to xaml
     * 
     * Make calculations touch transparent
     * Android Button Touch renderer long press problems
     * Make TouchInterface use variant eventhandler
     * 
     * v3.0
     * Makeover - color scheme, better icons
     * Native drag and drop
     * Use more native controls
     * Basic calculator mode
     * 
     * v.?
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
     * render (sinx)^2 as sin^2(x)
     * Look for uneccessary hashing
     */

    public delegate void FocusChangedEventHandler(Calculation oldFocus, Calculation newFocus);
    public delegate void RenderedEventHandler();

    public class MainPage : ContentPage
    {
        public enum Display
        {
            CondensedPortrait,
            CondensedLandscape,
            Expanded
        };

        public static BindableProperty SafeAreaInsetsProperty => Xamarin.Forms.PlatformConfiguration.iOSSpecific.Page.SafeAreaInsetsProperty;

        private static readonly BindableProperty DisplayModeProperty = BindableProperty.Create("DisplayMode", typeof(Display), typeof(MainPage),
            propertyChanging: (bindable, oldValue, newValue) => HandleDisplayModePropertyChange(bindable, oldValue, newValue, true),
            propertyChanged: (bindable, oldValue, newValue) => HandleDisplayModePropertyChange(bindable, oldValue, newValue, false));

        private Thickness SafeAreaInsets
        {
            get
            {
                Thickness value = (Thickness)GetValue(SafeAreaInsetsProperty);
                return new Thickness(
                    Math.Max(CrunchStyle.PAGE_PADDING, value.Left),
                    Math.Max(CrunchStyle.PAGE_PADDING, value.Top),
                    Math.Max(CrunchStyle.PAGE_PADDING, value.Right),
                    Math.Max(CrunchStyle.PAGE_PADDING, value.Bottom));
            }
        }

        public Display DisplayMode
        {
            get => (Display)GetValue(DisplayModeProperty);
            private set => SetValue(DisplayModeProperty, value);
        }

        public bool Collapsed => GetValue(DisplayModeProperty) is Display displayMode && (displayMode == Display.CondensedPortrait || displayMode == Display.CondensedLandscape);

        public StackOrientation Orientation => DisplayMode == Display.CondensedLandscape ? StackOrientation.Horizontal : StackOrientation.Vertical;

        public static int FontSize => 20;

        public static double ParenthesesWidth;

        public event FocusChangedEventHandler FocusChanged;
        private Calculation CalculationFocus;

        protected CursorView PhantomCursor;
        //How much extra space is in the lower right
        protected int ExtraPadding = 100;

        //private CrunchKeyboard VirtualKeyboard;
        public readonly View FullKeyboardView;
        protected readonly AbsoluteLayout KeyboardMask;
        protected readonly AbsoluteLayout CanvasArea;
        public readonly StackLayout KeyboardAndVariables;
        protected readonly CrunchKeyboard CrunchKeyboard;
        public readonly VariableRow Variables;

        private bool IsKeyboardDocked => App.KeyboardPosition.Value.Equals(KeyboardHidden);
        protected readonly Point KeyboardHidden = new Point(-1000, -1000);

        //private readonly AbsoluteLayout Screen;
        //private readonly StackLayout Page;
        protected readonly AbsoluteLayout Screen;
        protected readonly ScrollView CanvasScroll;
        protected Canvas Canvas;

        public MainPage()
        {
            Text.MaxTextHeight = App.TextHeight;
            ParenthesesWidth = App.TextWidth;

            Text.CreateLeftParenthesis = () => new TextImage(new Image() { Source = "leftParenthesis.png", HeightRequest = 0, WidthRequest = App.TextWidth, Aspect = Aspect.Fill }, "(");
            Text.CreateRightParenthesis = () => new TextImage(new Image() { Source = "rightParenthesis.png", HeightRequest = 0, WidthRequest = App.TextWidth, Aspect = Aspect.Fill }, ")");
            Text.CreateRadical = () => new Image() { Source = "radical.png", HeightRequest = 0, WidthRequest = App.TextWidth * 2, Aspect = Aspect.Fill };

            Content = Screen = new AbsoluteLayout
            {
                Children =
                {
                    {
                        (CanvasArea = new AbsoluteLayout
                        {
                            Children =
                            {
                                {
                                    (CanvasScroll = new ScrollView
                                    {
                                        Content = Canvas = new Canvas { },
                                        Orientation = ScrollOrientation.Both,
                                    }),
                                    new Rectangle(0, 0, 1, 1),
                                    AbsoluteLayoutFlags.SizeProportional
                                },
                                (PhantomCursor = new CursorView
                                {
                                    BindingContext = SoftKeyboard.Cursor,
                                    Color = Color.Red,
                                    IsVisible = false,
                                })
                            }
                        }),
                        new Rectangle(0, 0, 1, 1),
                        AbsoluteLayoutFlags.SizeProportional
                    }
                }
            };
            PhantomCursor.SetBinding(HeightRequestProperty, "Height");
            
            //Canvas.Children.Add(new Label { Text = "⚙⛭", FontFamily = CrunchStyle.SYMBOLA_FONT, FontSize = 50 }, new Point(0, 100));
            //canvas.Children.Add(new Label { Text = "˂<‹〈◁❬❰⦉⨞⧼︿＜⏴⯇🞀", FontFamily = CrunchStyle.SYMBOLA_FONT }, new Point(0, 100));
            //canvas.Children.Add(new Label { Text = "🌐\u1F310\u1F30F\u1F311", FontFamily = CrunchStyle.SYMBOLA_FONT }, new Point(0, 200));
            /*canvas.Children.Add(new Expression(Render.Math("log_(4)-9")), new Point(100, 100));
            canvas.Children.Add(new Expression(Render.Math("log_-9(4)")), new Point(200, 200));
            canvas.Children.Add(new Expression(Render.Math("log_-9-4")), new Point(300, 300));*/

            CrunchKeyboard = new CrunchKeyboard();

            Variables = new VariableRow
            {
                BindingContext = CrunchKeyboard,
                ButtonSize = 33
            };
            Variables.SetBinding(StackLayout.SpacingProperty, "Spacing");
            Variables.SetBinding<StackOrientation, StackOrientation>(StackLayout.OrientationProperty, this, "Orientation", StackLayoutExtensions.Invert);

            KeyboardAndVariables = new TouchableStackLayout
            {
                Children =
                {
                    Variables,
                    CrunchKeyboard
                }
            };
            KeyboardAndVariables.SetBinding(StackLayout.OrientationProperty, this, "Orientation");

            KeyboardMask = new AbsoluteLayout
            {
                BackgroundColor = Color.Gray,
                Opacity = 0.875,
            };

            PhantomCursor.Bind<bool>(IsVisibleProperty, value =>
            {
                if (value && SoftKeyboardManager.Current == CrunchKeyboard)
                {
                    Screen.Children.Add(KeyboardMask, new Rectangle(CrunchKeyboard.PositionOn(Screen), CrunchKeyboard.Bounds.Size), AbsoluteLayoutFlags.None);
                }
                else
                {
                    KeyboardMask.Remove();
                }
            });
            
            Screen.Children.Add(FullKeyboardView = FunctionsDrawerSetup());

            AbsoluteLayout.SetLayoutFlags(FullKeyboardView, AbsoluteLayoutFlags.PositionProportional);
            this.Bind<Display>(DisplayModeProperty, value =>
            {
                Point location = Point.Zero;
                
                if (value == Display.Expanded)
                {
                    location = App.KeyboardPosition.Value;
                }
                else if (value == Display.CondensedPortrait)
                {
                    location = new Point(0.5, 1);
                }
                else if (value == Display.CondensedLandscape)
                {
                    location = new Point(1, 0.5);
                }
                
                AbsoluteLayoutExtensions.SetLayoutBounds(FullKeyboardView, location.X, location.Y);//, height: value == Display.CondensedLandscape ? Height : -1);
            });

            void SetClosedDrawerHeight() => FunctionsDrawer.SetDrawerHeight(true, SoftKeyboardManager.Size.Height + (Orientation == StackOrientation.Vertical ? Variables.ButtonSize + KeyboardAndVariables.Spacing : 0));
            void SetOpenDrawerHeight() => FunctionsDrawer.SetDrawerHeight(false, Height * (DisplayMode == Display.Expanded ? 0.9 : 1) - (DisplayMode == Display.CondensedPortrait ? SafeAreaInsets.Top + SettingsButtonSize + CrunchStyle.PAGE_PADDING : 0));

            this.WhenPropertyChanged(HeightProperty, (sender, e) => SetOpenDrawerHeight());
            this.Bind<StackOrientation>("Orientation", value => SetClosedDrawerHeight());

            //Xamarin.Forms.PlatformConfiguration.iOSSpecific.Page.SetUseSafeArea(this, true);
            this.Bind<Thickness>(SafeAreaInsetsProperty, value =>
            {
                SetOpenDrawerHeight();
                CrunchKeyboard.SafeAreaChanged(SafeAreaInsets);
                AdjustPadding();
                SetCanvasSafeArea();
            });
            this.WhenPropertyChanged(DisplayModeProperty, (sender, e) =>
            {
                SetOpenDrawerHeight();
                AdjustPadding();
                SetCanvasSafeArea();
            });
            SizeChanged += (sender, e) =>
            {
                OnscreenKeyboardSizeChanged();
                //SetCanvasSafeArea();
            };
            
            SoftKeyboardManager.SizeChanged += (sender, e) =>
            {
                OnscreenKeyboardSizeChanged();
                SetClosedDrawerHeight();
                SetCanvasSafeArea();
            };
            
            SoftKeyboardManager.KeyboardChanged += (sender, e) => AdjustPadding();
            
            //Set up for keyboards
            SystemKeyboard.Setup(Screen);
            WireUpKeyboard(CrunchKeyboard);
            SoftKeyboardManager.AddKeyboard(SystemKeyboard.Instance, CrunchKeyboard);
            SoftKeyboardManager.SwitchTo(CrunchKeyboard);
            
            SettingsMenuSetUp();

            CanvasScroll.Scrolled += AdjustKeyboardPosition;
            //CanvasScroll.Scrolled += AdjustKeyboard;
            Canvas.Touch += AddCalculation;
            FocusChanged += SwitchCalculationFocus;
            
            Canvas.DescendantAdded += OnDescendantAdded;
            SizeChanged += (sender, e) => OnSizeChanged();
            CanvasArea.WhenPropertyChanged(PaddingProperty, (sender, e) => OnSizeChanged());
            CanvasArea.WhenPropertyChanged(WidthProperty, (sender, e) => LoadAd());
            
            void FixDynamicLag(object o) => Print.Log(o as dynamic);
            FixDynamicLag("");
        }

        private void AdjustPadding()
        {
            Thickness safeAreaInsets = SafeAreaInsets;

            FunctionsDrawer.FunctionsList.ListView.Margin = DisplayMode == Display.CondensedPortrait ? new Thickness(safeAreaInsets.Left, 0, safeAreaInsets.Right, 0) : new Thickness(0);
            FunctionsDrawer.Padding = DisplayMode == Display.CondensedLandscape ? new Thickness(0, safeAreaInsets.Top, safeAreaInsets.Right, safeAreaInsets.Bottom) : new Thickness(0);

            FunctionsDrawer.FunctionsList.EditingToolbar.Padding = new Thickness(0, 0, DisplayMode == Display.CondensedLandscape ? 0 : safeAreaInsets.Right, DisplayMode == Display.CondensedPortrait && SoftKeyboardManager.Current == CrunchKeyboard ? safeAreaInsets.Bottom : 0);
        }

        private void SetCanvasSafeArea()
        {
            Thickness canvasPadding = SafeAreaInsets;

            if (DisplayMode == Display.CondensedPortrait)
            //if (SoftKeyboardManager.Size.Width >= CanvasArea.Width)
            {
                canvasPadding.Bottom = SoftKeyboardManager.Size.Height + CrunchStyle.PAGE_PADDING;
            }
            else if (DisplayMode == Display.CondensedLandscape)
            //else if (SoftKeyboardManager.Size.Height >= CanvasArea.Height)
            {
                canvasPadding.Right = SoftKeyboardManager.Size.Width + Variables.ButtonSize + KeyboardAndVariables.Spacing + CrunchStyle.PAGE_PADDING;
            }

            CanvasArea.Margin = canvasPadding;
        }

        private View FunctionsDrawerSetup()
        {
            AddFunction addFunctionLayout = new AddFunction
            {
                IsVisible = false,
                CornerRadius = 20,
                BackgroundColor = Color.White,
                HasShadow = false,
                Padding = new Thickness(0, 0, 0, 5),
            };
            FunctionsDrawer = new FunctionsDrawer(KeyboardAndVariables, addFunctionLayout)
            {
                DropArea = Canvas,
            };

            FunctionsDrawer.Content.SetBinding<Color, StackOrientation>(BackgroundColorProperty, CrunchKeyboard, "Orientation", value => value == StackOrientation.Horizontal ? Color.Transparent : Color.White);
            KeyboardAndVariables.SetBinding<Color, StackOrientation>(BackgroundColorProperty, CrunchKeyboard, "Orientation", value => value == StackOrientation.Horizontal ? Color.Transparent : CrunchStyle.BACKGROUND_COLOR);

            ContentView portraitAddFunctionLayout = new ContentView();
            portraitAddFunctionLayout.SetBinding<View, Display>(ContentView.ContentProperty, this, "DisplayMode", value => value == Display.Expanded ? addFunctionLayout : null);

            ContentView landscapeAddFunctionLayout = new ContentView();
            void SetVisible()
            {
                portraitAddFunctionLayout.IsVisible = addFunctionLayout.IsVisible && DisplayMode == Display.Expanded;
                landscapeAddFunctionLayout.IsVisible = addFunctionLayout.IsVisible && DisplayMode != Display.Expanded;
            }
            CanvasArea.Children.Add(landscapeAddFunctionLayout, new Rectangle(0.5, 0.5, 1, -1), AbsoluteLayoutFlags.PositionProportional | AbsoluteLayoutFlags.WidthProportional);
            this.Bind<Display>(DisplayModeProperty, value =>
            {
                landscapeAddFunctionLayout.Margin = value == Display.CondensedPortrait ? new Thickness(0, 0, 0, Variables.ButtonSize) : new Thickness(0);
                landscapeAddFunctionLayout.Content = value == Display.Expanded ? null : addFunctionLayout;
                SetVisible();

                AbsoluteLayoutExtensions.SetLayoutLocation(landscapeAddFunctionLayout, new Point(0.5, value == Display.CondensedPortrait ? 1 : 0.5));
            });

            addFunctionLayout.Bind<bool>(IsVisibleProperty, value => SetVisible());

            return new StackLayout
            {
                Orientation = StackOrientation.Vertical,
                Children =
                {
                    portraitAddFunctionLayout,
                    FunctionsDrawer
                }
            };
        }

        private void OnscreenKeyboardSizeChanged()
        {
            double width = SoftKeyboardManager.Size.Width;
            double height = SoftKeyboardManager.Size.Height;
            Print.Log("onscreen keyboard size changed", width, height, SoftKeyboardManager.Current);
            Print.Log("\t" + Bounds.Size);

            Display displayMode;

            if (SoftKeyboardManager.Current == CrunchKeyboard && CrunchKeyboard.Orientation == StackOrientation.Vertical)
            {
                width += KeyboardAndVariables.Spacing + Variables.ButtonSize;
            }

            if (width >= Width)
            {
                displayMode = Display.CondensedPortrait;
            }
            else if (height >= Height)
            {
                displayMode = Display.CondensedLandscape;
            }
            else
            {
                displayMode = Display.Expanded;
            }

            AbsoluteLayoutExtensions.SetLayoutSize(FullKeyboardView, new Size(width, displayMode == Display.CondensedLandscape ? height : -1));
            FunctionsDrawer.FunctionsList.Margin = new Thickness(0, 0, 0, SoftKeyboardManager.Current == SystemKeyboard.Instance ? height + KeyboardAndVariables.Spacing : 0);

            DisplayMode = displayMode;
        }

#if DEBUG
        double yPosForRenderTests;
        private void RenderTests(string question, string answer)
        {
            Expression e = new Expression(Reader.Render(question));
            Canvas.Children.Add(e, new Point(0, yPosForRenderTests));
            yPosForRenderTests += e.Measure().Height + 50;
            Canvas.HeightRequest = yPosForRenderTests;
        }
#endif

        private double SettingsButtonSize = 50;
        private BannerAd ad;

        private FunctionsDrawer FunctionsDrawer;
        protected AnythingButton SettingsMenuButton;
        private Button FunctionsMenuButton;

        private void SettingsMenuSetUp()
        {
            SettingsMenuButton = new Button
            {
                BorderWidth = 0
            };
            for (double i = 0.25; i < 1; i += 0.25)
            {
                SettingsMenuButton.Children.Add(new BoxView() { Color = Color.Black }, new Rectangle(0.5, i, 0.6, 0.075), AbsoluteLayoutFlags.All);
            }
            SettingsMenuButton.Button.Clicked += (sender, e) => App.Current.ShowSettings();

            FunctionsMenuButton = new Button
            {
                Text = "f(x)",
                FontSize = NamedSize.Large.On<Button>(),
            };
            FunctionsMenuButton.Clicked += (sender, e) =>
            {
                FunctionsDrawer.ChangeStatus();
            };

            CanvasArea.Children.Add(SettingsMenuButton, new Rectangle(0, 0, SettingsButtonSize, SettingsButtonSize), AbsoluteLayoutFlags.PositionProportional);
            CanvasArea.Children.Add(FunctionsMenuButton, new Rectangle(1, 0, SettingsButtonSize, SettingsButtonSize), AbsoluteLayoutFlags.PositionProportional);

            CanvasArea.Children.Add(AdSpace = new ContentView(), new Point(0.5, 0));
            AbsoluteLayout.SetLayoutFlags(AdSpace, AbsoluteLayoutFlags.PositionProportional);
#if DEBUG
            App.Current.SetBinding<bool, bool>(Screenshots.InSampleModeProperty, AdSpace, "IsVisible", convertBack: value => !value, mode: BindingMode.OneWayToSource);
#endif
        }

        private ContentView AdSpace;

        private void WireUpKeyboard(CrunchKeyboard keyboard)
        {
            foreach (Key key in keyboard)
            {
                string text = key.Output;

                // Long press DEL to clear the canvas
                if (text == KeyboardManager.BACKSPACE.ToString())
                {
                    key.LongClick += async (sender, e) =>
                    {
                        if (!App.ClearCanvasWarning.Value || await DisplayAlert("Wait!", "Are you sure you want to clear the canvas?", "Yes", "No"))
                        {
                            ClearCanvas();
                        }
                    };
                }
                // Long press on any other key triggers cursor mode
                else if (!(key is CursorKey))
                {
                    key.LongClick += (sender, e) =>
                    {
                        if (!SoftKeyboard.Cursor.IsDescendantOf(Canvas))
                        {
                            return;
                        }

                        Point start;
                        if (!Collapsed && !IsKeyboardDocked)
                        {
                            start = new Point(TouchScreen.FirstTouch.X, FullKeyboardView.PositionOn(Screen).Y - SoftKeyboard.Cursor.Height);
                        }
                        else
                        {
                            start = SoftKeyboard.Cursor.PositionOn(Screen);
                        }

                        EnterCursorMode(start, 2);
                    };
                }
            }

            // Dock the keyboard when the dock button is pressed, or move it when it's dragged
            CrunchKeyboard.DockButton.Clicked += (sender, e) =>
            {
                DockKeyboard(!IsKeyboardDocked);
            };
            CrunchKeyboard.DockButton.Touch += (sender, e) =>
            {
                if (!Collapsed && e.State == TouchState.Moving)
                {
                    DockKeyboard(false);

                    TouchScreen.Dragging += (sender1, e1) =>
                    {
                        App.KeyboardPosition.Value = AbsoluteLayout.GetLayoutBounds(FullKeyboardView).Location;
                    };
                    TouchScreen.BeginDrag(FullKeyboardView, Screen);
                }
            };
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            KeyboardManager.CursorMoved += CursorMoved;
            KeyboardManager.Typed += Typed;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            KeyboardManager.CursorMoved -= CursorMoved;
            KeyboardManager.Typed -= Typed;
        }

        private void Typed(char keystroke)
        {
            if (keystroke == KeyboardManager.BACKSPACE)
            {
                //if (CalculationFocus != null)
                SoftKeyboard.Delete();
            }
            else
            {
                //if (CalculationFocus == null)
                //if (SoftKeyboard.Cursor.Parent<Canvas>() == null)
                if (!SoftKeyboard.Cursor.IsDescendantOf(this))
                {
                    AddCalculation();
                }

                SoftKeyboard.Type(keystroke.ToString());
            }
        }

        private void CursorMoved(KeyboardManager.CursorKey key)
        {
            if (CalculationFocus == null)
            {
                //return;
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
                    SoftKeyboard.Cursor.Parent<Calculation>()?.Up();
                }
            }
            else if (key == KeyboardManager.CursorKey.Down)
            {
                if (!SoftKeyboard.Down())
                {
                    SoftKeyboard.Cursor.Parent<Calculation>()?.Down();
                }
            }
        }

        public static int MaxBannerWidth = -1;

        private void LoadAd()
        {
            int width = (int)Math.Min(320, CanvasArea.Width - CrunchStyle.PAGE_PADDING * 2 - SettingsButtonSize * 2);

            if (width != MaxBannerWidth)
            {
                MaxBannerWidth = width;

                AdSpace.Content = ad = new BannerAd();
                AbsoluteLayout.SetLayoutBounds(AdSpace, new Rectangle(0.5, 0, width, -1));
            }
        }

        private void OnSizeChanged()
        {
            ExtraPadding = (int)Math.Max(Width, Height);
        }

        private void DockKeyboard(bool isDocked)
        {
            if (isDocked)
            {
                App.KeyboardPosition.Value = new Point(KeyboardHidden.X, KeyboardHidden.Y);
                AdjustKeyboardPosition();
            }
            else
            {
                App.KeyboardPosition.Value = AbsoluteLayout.GetLayoutBounds(FullKeyboardView).Location;
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

        private void FocusOnCalculation(Calculation newFocus)
        {
            FocusChanged?.Invoke(CalculationFocus, newFocus);
            CalculationFocus = newFocus;
        }
        
        private Point LastScroll = Point.Zero;

        public void AdjustKeyboard(object sender, ScrolledEventArgs e)
        {
            Point delta = new Point(LastScroll.X - e.ScrollX, LastScroll.Y - e.ScrollY);
            LastScroll = new Point(e.ScrollX, e.ScrollY);
            
            if (!Collapsed && IsKeyboardDocked)
            {
                FullKeyboardView.MoveTo(FullKeyboardView.X + delta.X, FullKeyboardView.Y + delta.Y);
            }
        }

        private void AdjustKeyboardPosition(object sender, EventArgs e) => AdjustKeyboardPosition();

        private void AdjustKeyboardPosition()
        {
            if (!Collapsed && IsKeyboardDocked && CalculationFocus != null)
            {
                Point offset = CanvasScroll.PositionOn(FullKeyboardView.Parent<View>());
                FullKeyboardView.MoveTo(CalculationFocus.X - CanvasScroll.ScrollX + offset.X, CalculationFocus.Y + CalculationFocus.Height - CanvasScroll.ScrollY + offset.Y);
            }
        }

        private void AddCalculation(object sender, TouchEventArgs e)
        {
            if (e.State != TouchState.Up)
            {
                return;
            }

            AddCalculation(e.Point);
        }

        public void AddCalculation(Point? location = null)
        {
            Point point = location.HasValue ? location.Value : new Point(CanvasScroll.ScrollX, CanvasScroll.ScrollY + SettingsButtonSize);

            Calculation calculation = new Calculation() { RecognizeVariables = true };
            FocusOnCalculation(calculation);

            calculation.SizeChanged += delegate
            {
                Point p = point.Add(new Point(calculation.Width, calculation.Height));

                if (Canvas.Width - p.X < ExtraPadding)
                {
                    Canvas.WidthRequest = p.X + ExtraPadding;
                }
                if (Canvas.Height - p.Y < ExtraPadding)
                {
                    Canvas.HeightRequest = p.Y + ExtraPadding;
                }
            };

            Equation equation = new Equation();
            SoftKeyboard.MoveCursor(equation.LHS);

            calculation.Add(equation);
            Canvas.Children.Add(calculation, point);
        }

        private void OnDescendantAdded(object sender, ElementEventArgs e)
        {
            if (e.Element is Calculation)
            {
                //(e.Element as Calculation).Touch += DragOnCanvas;
            }
            else if (e.Element is Equation equation)
            {
                if (e.Element.GetType() == typeof(Equation))
                {
                    equation.LHS.Touch += EquationMoveCursor;
                }
                if (equation.RHS is Answer)
                {
                    equation.RHS.Touch += DragAnswer;
                }

                equation.Touch += MoveCalculation;
            }
            else if (e.Element is Link link)
            {
                link.PropertyChanged += (sender1, e1) =>
                {
                    if (e1.PropertyName == ContentView.ContentProperty.PropertyName)
                    {
                        link.MathContent.Touch += DragLink;
                    }
                };
            }
        }

#if false
        private void OnDescendantAdded1(object sender, ElementEventArgs e)
        {
            if (e.Element is Calculation)
            {
                //(e.Element as Calculation).Touch += DragOnCanvas;
            }
            else if (e.Element is Equation equation)
            {
                if (e.Element.GetType() == typeof(Equation))
                {
                    equation.LHS.Touch += EquationMoveCursor;
                }
                if (equation.RHS is Answer)
                {
                    equation.RHS.Touch += (sender1, e1) => Drag(equation, e1, Canvas, (sender2, e2) =>
                    {
                        /*Link link = new Link(answer);
                        link.MathContent.Touch += DragLink;
                        TouchScreen.BeginDrag(link, PhantomCursorField, answer);
                        StartDraggingLink(link);*/
                    });
                    //equation.RHS.Touch += DragAnswer;
                }

                //equation.Touch += MoveCalculation;
                equation.Touch += (sender1, e1) => Drag(sender1 as Calculation ?? (sender1 as View)?.Parent<Calculation>() ?? sender1 as View, e1, Canvas, (sender2, e2) =>
                {
                    if (e2.Value != DragState.Ended)
                    {
                        return;
                    }

                    AdjustKeyboardPosition();
                });
            }
            else if (e.Element is Link link)
            {
                BoxView placeholder = new BoxView();

                link.WhenPropertyChanged(ContentView.ContentProperty, (sender1, e1) =>
                {
                    ((Link)sender1).MathContent.MakeDraggable(PhantomCursorField, (sender2, e2) =>
                    {
                        if (e2.Value == DragState.Started)
                        {
                            placeholder = link.StartDrag();
                        }
                        else
                        {
                            Tuple<Expression, int> target = ExampleDrop(link);

                            if (e2.Value == DragState.Moving && target != null)
                            {
                                target.Item1.Insert(target.Item2, placeholder);
                            }
                        }
                    });
                });

                /*link.PropertyChanged += (sender1, e1) =>
                {
                    if (e1.PropertyName == ContentView.ContentProperty.PropertyName)
                    {
                        link.MathContent.Touch += DragLink;
                    }
                };*/
            }
        }

        private void Drag(View sender, TouchEventArgs e, Layout<View> dropArea, EventHandler<EventArgs<DragState>> onDrag = null, View start = null)
        {
            if (e.State == TouchState.Moving)
            {
                TouchScreen.BeginDrag(sender, dropArea, start ?? sender);
                if (onDrag != null)
                {
                    TouchScreen.Dragging += onDrag;
                }
            }
        }
#endif

        private void MoveCalculation(object sender, TouchEventArgs e)
        {
            View draggable = sender as Calculation ?? (sender as View)?.Parent<Calculation>() ?? sender as View;
            if (draggable != null && e.State == TouchState.Moving)
            {
                double backup = TouchScreen.FatFinger;
                TouchScreen.FatFinger = 0;
                TouchScreen.BeginDrag(draggable, Canvas);
                TouchScreen.FatFinger = backup;

                TouchScreen.Dragging += (sender1, e1) =>
                {
                    if (e1.Value != DragState.Ended)
                    {
                        return;
                    }

                    AdjustKeyboardPosition();
                };
            }
        }

        private void EquationMoveCursor(object sender, TouchEventArgs e)
        {
            if (sender is View view && e.State == TouchState.Down)
            {
                EnterCursorMode(view.PositionOn(Screen).Add(e.Point));//.Add(new Point(-MainPage.phantomCursor.Width / 2, -Text.MaxTextHeight))));
            }
        }

        private void DragAnswer(object sender, TouchEventArgs e)
        {
            if (sender is Answer answer && e.State == TouchState.Moving)
            {
                Link link = new Link(answer);
                link.MathContent.Touch += DragLink;
                TouchScreen.BeginDrag(link, Screen, answer);
                StartDraggingLink(link);
            }
        }

        private void DragLink(object sender, TouchEventArgs e)
        {
            if ((sender as View)?.Parent is Link link && e.State == TouchState.Moving)
            {
                if (!TouchScreen.Active)
                {
                    StartDraggingLink(link);
                }
                TouchScreen.BeginDrag(link, Screen);
            }
        }

        private void StartDraggingLink(Link link)
        {
            BoxView placeholder = link.StartDrag();

            TouchScreen.Dragging += (sender, e) =>
            {
                Tuple<Expression, int> target = ExampleDrop(link);

                if (e.Value == DragState.Moving && target != null)
                {
                    target.Item1.Insert(target.Item2, placeholder);
                }
            };
        }

        private void ClearCanvas()
        {
            Canvas.Children.Clear();
            Canvas.WidthRequest = (Canvas.Parent as View).Width;
            Canvas.HeightRequest = (Canvas.Parent as View).Height;

            if (IsKeyboardDocked)
            {
                AbsoluteLayout.SetLayoutBounds(FullKeyboardView, new Rectangle(KeyboardHidden, new Size(-1, -1)));
            }

            FocusOnCalculation(null);
        }

        public void EnterCursorMode(Point start, double speed = 1)
        {
            //if (CalculationFocus == null)
            //if (SoftKeyboard.Cursor.Parent == null)
            /*if (!SoftKeyboard.Cursor.IsDescendantOf(Canvas))
            {
                return;
            }*/

            PhantomCursor.IsVisible = true;

            //Put the cursor in the middle of the screen if it's off the screen
            if (!new Rectangle(Point.Zero, Screen.Bounds.Size).Contains(new Rectangle(start, PhantomCursor.Bounds.Size)))
            {
                start = new Point((Screen.Width - PhantomCursor.Width) / 2, (Screen.Height - PhantomCursor.Height) / 2);
            }
            TouchScreen.BeginDrag(PhantomCursor, Screen, start, speed);

            TouchScreen.Dragging += (sender, e) =>
            {
                if (e.Value == DragState.Ended)
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
            Calculation root = SoftKeyboard.Cursor.Root<Calculation>();
            /*Element root = SoftKeyboard.Cursor;
            while (!(root is Calculation))
            {
                root = root.Parent;
            }*/

            //Focus has changed
            if (root != CalculationFocus)
            {
                FocusOnCalculation(root);
                AdjustKeyboardPosition();
            }

            PhantomCursor.IsVisible = false;
        }

        //private Thread thread;
        private int shouldScrollX => (int)Math.Truncate(PhantomCursor.X / (CanvasScroll.Width - PhantomCursor.Width) * 2 - 1);
        private int shouldScrollY => (int)Math.Truncate(PhantomCursor.Y / (CanvasScroll.Height - PhantomCursor.Height) * 2 - 1);
        private readonly double scrollSpeed = 0.025;
        private double preciseScrollX, preciseScrollY;

        private void scrollCanvas()
        {
            preciseScrollX = CanvasScroll.ScrollX;
            preciseScrollY = CanvasScroll.ScrollY;
            while (shouldScrollX + shouldScrollY != 0 && PhantomCursor.IsVisible)
            {
                preciseScrollX += shouldScrollX * scrollSpeed;
                preciseScrollY += shouldScrollY * scrollSpeed;
                CanvasScroll.ScrollToAsync(preciseScrollX, preciseScrollY, false);
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
            Point temp = new Point(CanvasScroll.ScrollX + dragging.X + dragging.Width / 2, CanvasScroll.ScrollY + dragging.Y + dragging.Height / 2).Subtract(CanvasScroll.PositionOn(Screen));
            View view = GetViewAt(Canvas, temp, out loc);
            // Try this instead?
            //Canvas.GetChildElements(temp);
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

        private static void HandleDisplayModePropertyChange(object bindable, object newValue, object oldValue, bool changing)
        {
            MainPage mainPage = (MainPage)bindable;
            Display oldDisplayMode = (Display)oldValue;
            Display newDisplayMode = (Display)newValue;

            void MethodCall(string propertyName)
            {
                if (changing)
                {
                    mainPage.OnPropertyChanging(propertyName);
                }
                else
                {
                    mainPage.OnPropertyChanged(propertyName);
                }
            }

            if (oldDisplayMode == Display.CondensedLandscape || newDisplayMode == Display.CondensedLandscape)
            {
                MethodCall("Orientation");
            }
            if (oldDisplayMode == Display.Expanded || newDisplayMode == Display.Expanded)
            {
                MethodCall("Collapsed");
            }
        }
    }
}
 