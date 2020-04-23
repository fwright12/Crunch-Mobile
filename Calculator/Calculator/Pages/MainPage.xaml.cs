using System;
using System.Extensions;
using System.Runtime.CompilerServices;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Forms.Extensions;
using Xamarin.Forms.MathDisplay;

namespace Calculator
{
    //Color 560297
    //&#8801; - hamburger menu ≡
    //&#8942; - kabob menu ⋮
    //-no-snapshot
    // 🌐|⟨|⟩|√|🔗|⭧|🡕

    //Parentheses pictures - 500 font in Word Roboto (80,80,80), default Android ratio is 114 * 443 - trim and resize in Paint to 369 * 1127
    //Radical pictures - 667 font in Paint Segoe UI Symbol (copy from Graphemica), trim completely (609 * 1114)
    //Radical - right half is about 1/2 of horizontal, bottom left takes up about 1/2 of vertical, thickness is 0.1, horizontal bar is about 1/3 of horizontal

    /* TODO:
     * v3.0
     * Makeover - color scheme, better icons
     * Use more native controls
     * Basic calculator mode
     * 
     * v3.1
     * Native drag and drop
     * Flick to delete
     * Make calculations touch transparent
     * Long press to drag (everything?) w/ haptics
     * Double tap to highlight
     * Global variables
     * 
     * Android Button Touch renderer long press problems
     * Make TouchInterface use variant eventhandler
     * 
     * v.?
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
    //public delegate void RenderedEventHandler();

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage
    {
        public enum Display { CondensedPortrait, CondensedLandscape, Expanded };
        public enum CalculatorMode { Crunch, Basic }

        public static BindableProperty ModeProperty = BindableProperty.Create("Mode", typeof(CalculatorMode), typeof(MainPage), CalculatorMode.Crunch);

        private static readonly BindableProperty DisplayModeProperty = BindableProperty.Create("DisplayMode", typeof(Display), typeof(MainPage),
            propertyChanging: (bindable, oldValue, newValue) => HandleDisplayModePropertyChange(bindable, oldValue, newValue, true),
            propertyChanged: (bindable, oldValue, newValue) => HandleDisplayModePropertyChange(bindable, oldValue, newValue, false));

        public Thickness SafeAreaInsets
        {
            get
            {
                Thickness value = (Thickness)GetValue(Xamarin.Forms.PlatformConfiguration.iOSSpecific.Page.SafeAreaInsetsProperty);
                return new Thickness(
                    Math.Max(App.PAGE_PADDING, value.Left),
                    Math.Max(App.PAGE_PADDING, value.Top),
                    Math.Max(App.PAGE_PADDING, value.Right),
                    Math.Max(App.PAGE_PADDING, value.Bottom));
            }
        }

        public CalculatorMode Mode
        {
            get => (CalculatorMode)GetValue(ModeProperty);
            set => SetValue(ModeProperty, value);
        }

        public Display DisplayMode
        {
            get => (Display)GetValue(DisplayModeProperty);
            private set => SetValue(DisplayModeProperty, value);
        }

        public bool Collapsed { get; private set; }// => GetValue(DisplayModeProperty) is Display displayMode && (displayMode == Display.CondensedPortrait || displayMode == Display.CondensedLandscape);

        public StackOrientation Orientation => DisplayMode == Display.CondensedLandscape ? StackOrientation.Horizontal : StackOrientation.Vertical;

        private bool IsKeyboardDocked => App.KeyboardPosition.Value.Equals(KeyboardHidden);

        private readonly double SETTINGS_BUTTON_SIZE = 50;
        private readonly Point KeyboardHidden = new Point(-1000, -1000);

        //How much extra space is in the lower right
        private int ExtraPadding = 100;

        public static double ParenthesesWidth;
        public event FocusChangedEventHandler FocusChanged;
        private Calculation CalculationFocus;
        //private StackLayout FullKeyboardView => FunctionsDrawer.Parent as StackLayout;

        //private CrunchKeyboard CrunchKeyboard => KeyboardAndVariables;
        private Grid Variables => CrunchKeyboard.Variables;

        public static uint ModeTransitionLength = 2500;

        public MainPage()
        {
            Text.MaxTextHeight = App.TextHeight;
            ParenthesesWidth = App.TextWidth;

            Text.CreateLeftParenthesis = () => new ImageText { Text = "(", HeightRequest = 0, WidthRequest = App.TextWidth, Aspect = Aspect.Fill };
            Text.CreateRightParenthesis = () => new ImageText { Text = ")", HeightRequest = 0, WidthRequest = App.TextWidth, Aspect = Aspect.Fill };
            Text.CreateRadical = () => new Image() { Source = new FontImageSource { Glyph = "⎷", FontFamily = (OnPlatform<string>)App.Current.Resources["SymbolaFont"] }, HeightRequest = 0, WidthRequest = App.TextWidth * 2, Aspect = Aspect.Fill };

            InitializeComponent();
            
            for (double i = 0.25; i < 1; i += 0.25)
            {
                SettingsMenuButton.Children.Add(new BoxView() { Color = Color.Black }, new Rectangle(0.5, i, 0.6, 0.075), AbsoluteLayoutFlags.All);
            }
            SettingsMenuButton.Button.Clicked += (sender, e) => App.Current.ShowSettings();
            FunctionsMenuButton.Clicked += (sender, e) => FunctionsDrawer.ChangeStatus(true);
            
#if DEBUG
            App.Current.SetBinding<bool, bool>(Screenshots.InSampleModeProperty, AdSpace, "IsVisible", convertBack: value => !value, mode: BindingMode.OneWayToSource);
#endif

            //Canvas.Children.Add(new Label { Text = "⚙⛭", FontFamily = CrunchStyle.SYMBOLA_FONT, FontSize = 50 }, new Point(0, 100));
            //canvas.Children.Add(new Label { Text = "˂<‹〈◁❬❰⦉⨞⧼︿＜⏴⯇🞀", FontFamily = CrunchStyle.SYMBOLA_FONT }, new Point(0, 100));
            //canvas.Children.Add(new Label { Text = "🌐\u1F310\u1F30F\u1F311", FontFamily = CrunchStyle.SYMBOLA_FONT }, new Point(0, 200));
            /*canvas.Children.Add(new Expression(Render.Math("log_(4)-9")), new Point(100, 100));
            canvas.Children.Add(new Expression(Render.Math("log_-9(4)")), new Point(200, 200));
            canvas.Children.Add(new Expression(Render.Math("log_-9-4")), new Point(300, 300));*/
            
            //Variables.SetBinding<StackOrientation, StackOrientation>(StackLayout.OrientationProperty, this, "Orientation", StackLayoutExtensions.Invert);

            AbsoluteLayout KeyboardMask = new AbsoluteLayout
            {
                BackgroundColor = Color.Gray,
                Opacity = 0.875,
            };

            /*Color shade = (Color)App.Current.Resources["BackgroundColor"];
            double percent = 0.25;
            Color[] colors = new Color[]
            {
                Color.GhostWhite,
                Color.FromHex("#e7f0ff"),
                Color.FromHex("#fcfafa"),
                Color.FromHex("#ccccff"),
                new Color(1 - (1 - shade.R) * percent, 1 - (1 - shade.G) * percent, 1 - (1 - shade.B) * percent)
            };
            for (int i = 0; i < colors.Length; i++)
            {
                CanvasArea.Children.Add(new BoxView
                {
                    Color = colors[i]
                }, new Rectangle(1.0 / (colors.Length - 1) * i, 1, 1.0 / colors.Length, 1), AbsoluteLayoutFlags.All);
            }
            Color primary = (Color)App.Current.Resources["PrimaryColor"];
            //App.Current.Resources["PrimaryColor"] = new Color(primary.R, primary.G, primary.B, 0.5);
            BackgroundColor = Color.White;
            FunctionsDrawer.FunctionsList.BackgroundColor = Color.FromHex("#e4d9ff");// new Color(shade.R, shade.G, shade.B, 0.1);// new Color((BackgroundColor.R + shade.R) / 2, (BackgroundColor.G + shade.G) / 2, (BackgroundColor.B + shade.B) / 2);*/
            //App.Current.Resources["test"] = Color.Purple;
            //Color primary = (Color)App.Current.Resources["BackgroundColor"];
            //FunctionsDrawer.FunctionsList.BackgroundColor = primary.Shade(0.95);

            SoftKeyboard.Cursor.SetDynamicResource(BackgroundColorProperty, "DetailColor");
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

            CrunchKeyboard.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName != "Collapsed")
                {
                    return;
                }

                Collapsed = CrunchKeyboard.Collapsed;
                OnPropertyChanged("Collapsed");
            };

            //Screen.Children.Add(CrunchKeyboard, new Rectangle(0.5, 1, -1, -1), AbsoluteLayoutFlags.PositionProportional);
            
            BasicAnswer.Remove();

            Answer CurrentAnswer() => SoftKeyboard.Cursor.Parent<Equation>()?.RHS as Answer;

            VisualStateManager.GetVisualStateGroups(this).Add(new VisualStates(nameof(CalculatorMode.Crunch), nameof(CalculatorMode.Basic))
            {
                new TargetedSetters
                {
                    Targets = { CanvasScroll, FunctionsMenuButton },
                    Setters =
                    {
                        new Setters { Property = OpacityProperty, Values = { 1, 0 } }
                    }
                },
                new TargetedSetters
                {
                    Targets = BasicAnswer,
                    Setters =
                    {
                        //new Setters { Property = LabelExtensions.AutoSizeFontProperty, Values = { (BooleanValue<bool>)false, true } },
                        new Setters
                        {
                            Property = Label.FontSizeProperty,
                            Values =
                            {
                                33,
                                80
                                //new Thunk<double>(() => LabelExtensions.MaxFontSize(CanvasArea.Width, Height - SafeAreaInsets.Top - SETTINGS_BUTTON_SIZE - App.PAGE_PADDING - CrunchKeyboard.MeasureBasicMode().Height - App.PAGE_PADDING, BasicAnswer.Text))
                            }
                        },
                        new Setters { Property = Label.PaddingProperty, Values = { new Thickness(0), new Thickness(20, SETTINGS_BUTTON_SIZE + App.PAGE_PADDING, 20, 0) } },
                        new Setters
                        {
                            Property = OpacityProperty,
                            Values =
                            {
                                new Thunk<double>(() => CurrentAnswer()?.RawAnswer == null ? 0 : 1),
                                1
                            }
                        },
                        new Setters
                        {
                            Property = AbsoluteLayout.LayoutBoundsProperty,
                            Values =
                            {
                                new Thunk<Rectangle>(() =>
                                {
                                    View answer = CurrentAnswer();

                                    double fontSize = BasicAnswer.FontSize;
                                    BasicAnswer.FontSize = 33;
                                    Size size = answer?.Bounds.Size ?? BasicAnswer.Measure();
                                    BasicAnswer.FontSize = fontSize;

                                    Point point = answer?.PositionOn(CanvasArea) ?? new Point(CanvasArea.Width - size.Width, CanvasArea.Height - size.Height);
                                    
                                    return new Rectangle(point, new Size(-1, -1));
                                }),
                                new Rectangle(1, 1, -1, -1)
                            }
                        }
                    }
                }
            });
            
            KeyboardManager.Typed += (keystroke) =>
            {
                if (BasicAnswer.Parent == null)
                {
                    return;
                }
                
                BasicAnswer.Input(keystroke);
            };

            KeyboardManager.CursorMoved += (key) =>
            {
                if (BasicAnswer.Parent == null)
                {
                    return;
                }

                if (key == KeyboardManager.CursorKey.Up || key == KeyboardManager.CursorKey.Down)
                {
                    BasicAnswer.Reset();
                }
            };

            CrunchKeyboard.ChangeModeButton.Clicked += (sender, e) =>
            {
                CalculatorMode targetMode = (CalculatorMode)(((int)Mode + 1) % Enum.GetValues(typeof(CalculatorMode)).Length);
                Mode = (CalculatorMode)(-1);

                /*void Grow(object sender1, EventArgs e1) => AbsoluteLayoutExtensions.SetLayout(FullKeyboardView, y: Height - (SoftKeyboardManager.Size.Height + CrunchKeyboard.Padding.VerticalThickness));
                AbsoluteLayout.SetLayoutFlags(FullKeyboardView, AbsoluteLayoutFlags.XProportional);
                SoftKeyboardManager.SizeChanged += Grow;*/

                // value => FunctionsDrawer.HeightRequest = CrunchKeyboard.Size.Height);

                /*string[] modes = new string[] { "Regular", "Basic" };
                if (regular)
                {
                    System.Misc.Swap(ref modes[0], ref modes[1]);
                }*/

                //FunctionsDrawer.IsVisible = false;
                //Screen.Children.Add(CrunchKeyboard, new Point(0.5, 1), AbsoluteLayoutFlags.PositionProportional);

                if (SoftKeyboard.Cursor.Parent != null)
                {
                    //BasicAnswer.Reset();
                    //BasicAnswer.Update((answer?.ToString() ?? "0") + "+0=");
                }

                if (targetMode == CalculatorMode.Basic)
                {
                    //CanvasArea.Children.Add(BasicAnswer, CurrentAnswerBounds(), AbsoluteLayoutFlags.None, CanvasArea.Children.IndexOf(CanvasScroll) + 1);
                    CanvasArea.Children.Insert(CanvasArea.Children.IndexOf(CanvasScroll) + 1, BasicAnswer);
                    BasicAnswer.SetValueFromState(AbsoluteLayout.LayoutBoundsProperty, this.GetVisualStateByName(nameof(CalculatorMode.Crunch)));
                    //AbsoluteLayoutExtensions.SetLayout(BasicAnswer, new Size(-1, -1));

                    BasicAnswer.Reset();

                    if (SoftKeyboard.Cursor.Parent<Equation>() is Equation parentEquation && parentEquation.RHS is Answer answerView)
                    {
                        string answerText = answerView.RawAnswer?.Format(Crunch.Polynomials.Expanded, Crunch.Numbers.Decimal, Crunch.Trigonometry.Degrees)?.ToString();

                        if (parentEquation.LHS.Children.Count > 1)
                        {
                            BasicAnswer.Input((answerText ?? "1/0") + "+0=");
                            BasicAnswer.End(parentEquation);
                        }
                    }
                }
                
                AbsoluteLayoutExtensions.ConvertLayoutBounds(BasicAnswer, targetMode == CalculatorMode.Crunch ? AbsoluteLayoutFlags.None : AbsoluteLayoutFlags.PositionProportional);
                //BasicAnswer.ShowAnswer();

                View answer = SoftKeyboard.Cursor.Parent<Equation>()?.RHS as Answer;
                if (answer != null)
                {
                    answer.Opacity = 0;
                }
                else if (targetMode == CalculatorMode.Basic)
                {
                    BasicAnswer.Opacity = 0;
                }

                this.AnimateVisualState("changeMode", end: targetMode.ToString(), length: ModeTransitionLength, callback: value =>
                {
                    AbsoluteLayoutExtensions.SetLayout(FullKeyboardView, y: value == 1 ? 1 : (Height - SoftKeyboardManager.Size.Height), flags: value == 1 ? AbsoluteLayoutFlags.PositionProportional : AbsoluteLayoutFlags.XProportional);
                },
                finished: (final, cancelled) =>
                {
                    if (targetMode == CalculatorMode.Crunch)
                    {
                        BasicAnswer.Remove();
                    }

                    if (answer != null)
                    {
                        answer.Opacity = 1;
                    }

                    //SoftKeyboardManager.SizeChanged -= Grow;
                    //AbsoluteLayoutExtensions.SetLayout(FullKeyboardView, y: 1, flags: AbsoluteLayoutFlags.PositionProportional);

                    FunctionsDrawer.FunctionsList.ListView.GetSwipeListener().Active = targetMode == CalculatorMode.Crunch;

                    Mode = targetMode;
                });
                CrunchKeyboard.ChangeMode(targetMode == CalculatorMode.Crunch, true);
            };
            //(CrunchKeyboard.Parent<StackLayout>().Parent as ListView).Header = "Test";
            //Screen.Children.Add(CrunchKeyboard.Parent<StackLayout>(), new Point(0.5, 0.25), AbsoluteLayoutFlags.PositionProportional);
            FunctionsDrawerSetup();
            //CrunchKeyboard.BackgroundColor = App.CRUNCH_PURPLE;
            AbsoluteLayout.SetLayoutFlags(FullKeyboardView, AbsoluteLayoutFlags.PositionProportional);
            this.Bind<Display>(DisplayModeProperty, value =>
            {
                Point location = Point.Zero;

                CrunchKeyboard.Padding = new Thickness(0);
                //FunctionsDrawer.Padding = new Thickness(0);
                FullKeyboardView.Margin = new Thickness(0);

                if (value == Display.Expanded)
                {
                    location = App.KeyboardPosition.Value;
                }
                else if (value == Display.CondensedPortrait)
                {
                    CrunchKeyboard.SetBinding<Thickness, Thickness>(Xamarin.Forms.Layout.PaddingProperty, this, "SafeAreaInsets", safeArea => new Thickness(safeArea.Left, 0, safeArea.Right, safeArea.Bottom));
                    //FullKeyboardView.SetBinding<Thickness, Thickness>(View.MarginProperty, this, "SafeAreaInsets", safeArea => new Thickness(-safeArea.Left, 0, -safeArea.Right, -safeArea.Bottom));
                    location = new Point(0.5, 1);
                }
                else if (value == Display.CondensedLandscape)
                {
                    //CrunchKeyboard.Padding = new Thickness(0, SafeAreaInsets.Top, 0, SafeAreaInsets.Bottom);
                    //FullKeyboardView.Padding = new Thickness(0, 0, SafeAreaInsets.Right, 0);
                    CrunchKeyboard.SetBinding<Thickness, Thickness>(Xamarin.Forms.Layout.PaddingProperty, this, "SafeAreaInsets", safeArea => new Thickness(0, safeArea.Top, safeArea.Right, safeArea.Bottom));

                    //CrunchKeyboard.Parent<EditListView>().SetBinding<Thickness, Thickness>(Xamarin.Forms.Layout.PaddingProperty, this, "SafeAreaInsets", safeArea => new Thickness(0, 0, safeArea.Right, 0));
                    //FullKeyboardView.SetBinding<Thickness, Thickness>(View.MarginProperty, this, "SafeAreaInsets", safeArea => new Thickness(0, 0, safeArea.Right, 0));

                    location = new Point(1, 0.5);
                }
                
                AbsoluteLayoutExtensions.SetLayout(FullKeyboardView, location.X, location.Y);//, height: value == Display.CondensedLandscape ? Height : -1);
            });
            
            void SetOpenDrawerHeight() => FunctionsDrawer.SetDrawerHeight(false, Height * (DisplayMode == Display.Expanded ? 0.9 : 1) - (DisplayMode == Display.CondensedPortrait ? SafeAreaInsets.Top + SETTINGS_BUTTON_SIZE + App.PAGE_PADDING : 0));

            this.WhenPropertyChanged(HeightProperty, (sender, e) => SetOpenDrawerHeight());
            //this.Bind<StackOrientation>("Orientation", value => SetClosedDrawerHeight());

            //Xamarin.Forms.PlatformConfiguration.iOSSpecific.Page.SetUseSafeArea(this, true);
            //Screen.SetBinding(Xamarin.Forms.Layout.PaddingProperty, this, "SafeAreaInsets");
            this.Bind<Thickness>(Xamarin.Forms.PlatformConfiguration.iOSSpecific.Page.SafeAreaInsetsProperty, value =>
            {
                SetOpenDrawerHeight();
                //CrunchKeyboard.SafeAreaChanged(SafeAreaInsets);
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

            /*CrunchKeyboard.MeasureInvalidated += (sender, e) =>
            {
                SizeRequest sr = CrunchKeyboard.Measure(App.Current.MainPage.Width, App.Current.MainPage.Height);

                FullKeyboardView.WidthRequest = sr.Request.Width;
                FullKeyboardView.MinimumWidthRequest = sr.Minimum.Width;
            };*/
            
            SoftKeyboardManager.SizeChanged += (sender, e) =>
            {
                OnscreenKeyboardSizeChanged();

                FunctionsDrawer.SetDrawerHeight(true, SoftKeyboardManager.Size.Height);// + (sender == CrunchSystemKeyboard.Instance ? CrunchKeyboard.ButtonSize + CrunchKeyboard.Spacing : 0));
                //FunctionsDrawer.SetDrawerHeight(true, SoftKeyboardManager.Size.Height + (Orientation == StackOrientation.Vertical ? CrunchKeyboard.Variables.Height + CrunchKeyboard.Spacing : 0));

                SetCanvasSafeArea();
            };

            SoftKeyboardManager.KeyboardChanged += (sender, e) =>
            {
                AdjustPadding();

                Grid variables = CrunchKeyboard.Variables;
                Func<Thickness, Thickness> LeftRightPadding = value => new Thickness(value.Left, 0, value.Right, 0);
                Func<Thickness, Thickness> LeftRightBottomPadding = value => new Thickness(value.Left, 0, value.Right, value.Bottom);

                if (sender == CrunchKeyboard)
                {
                    bool transposed = GridExtensions.GetIsTransposed(CrunchKeyboard);
                    GridExtensions.SetIsTransposed(variables, transposed);
                    if (transposed)
                    {
                        Variables.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
                    }
                    else
                    {
                        Variables.RowDefinitions[0].Height = new GridLength(1, GridUnitType.Star);
                    }
                    variables.ClearValue(Xamarin.Forms.Layout.PaddingProperty);

                    CrunchKeyboard.Children.Add(variables, 0, 0);
                    CrunchKeyboard.LowerChild(variables);
                    if (CrunchKeyboard.Orientation == StackOrientation.Horizontal)
                    {
                        Grid.SetColumnSpan(variables, CrunchKeyboard.ColumnDefinitions.Count);
                    }
                    else
                    {
                        Grid.SetRowSpan(variables, CrunchKeyboard.RowDefinitions.Count);
                    }
                    KeyboardView.Content = CrunchKeyboard;
                }
                else if (sender == CrunchSystemKeyboard.Instance)
                {
                    GridExtensions.SetIsTransposed(variables, false);
                    Variables.RowDefinitions[0].Height = 33;
                    variables.SetBinding<Thickness, Thickness>(Xamarin.Forms.Layout.PaddingProperty, this, "SafeAreaInsets", LeftRightPadding);

                    KeyboardView.Content = variables;
                }

                FunctionsDrawer.FunctionsList.EditingToolbar.SetBinding<Thickness, Thickness>(Xamarin.Forms.Layout.PaddingProperty, this, "SafeAreaInsets", sender == CrunchSystemKeyboard.Instance ? LeftRightPadding : LeftRightBottomPadding);
            };

            //Set up for keyboards
            SystemKeyboard.Setup(Screen);
            CrunchSystemKeyboard.Instance.Variables = CrunchKeyboard.Variables;
            WireUpKeyboard(CrunchKeyboard);
            SoftKeyboardManager.AddKeyboard(CrunchSystemKeyboard.Instance, CrunchKeyboard);
            SoftKeyboardManager.SwitchTo(CrunchKeyboard);

            CanvasScroll.Scrolled += AdjustKeyboardPosition;
            //CanvasScroll.Scrolled += AdjustKeyboard;
            Canvas.Touch += AddCalculation;
            FocusChanged += SwitchCalculationFocus;

            Canvas.DescendantAdded += OnDescendantAdded;
            CanvasArea.WhenPropertyChanged(WidthProperty, (sender, e) => LoadAd());
            CanvasArea.SizeChanged += (sender, e) => ExtraPadding = (int)Math.Max(CanvasArea.Width, CanvasArea.Height);

            void FixDynamicLag(object o) => Print.Log(o as dynamic);
            FixDynamicLag("");
        }

        private void AdjustPadding()
        {
            Thickness safeAreaInsets = SafeAreaInsets;

            //FunctionsDrawer.FunctionsList.ListView.Margin = DisplayMode == Display.CondensedPortrait ? new Thickness(safeAreaInsets.Left, 0, safeAreaInsets.Right, 0) : new Thickness(0);
            //FunctionsDrawer.Padding = DisplayMode == Display.CondensedLandscape ? new Thickness(0, safeAreaInsets.Top, safeAreaInsets.Right, safeAreaInsets.Bottom) : new Thickness(0);
            
            //FunctionsDrawer.FunctionsList.EditingToolbar.Padding = new Thickness(0, 0, DisplayMode == Display.CondensedLandscape ? 0 : safeAreaInsets.Right, DisplayMode == Display.CondensedPortrait && SoftKeyboardManager.Current == CrunchKeyboard ? safeAreaInsets.Bottom : 0);
        }

        private void SetCanvasSafeArea()
        {
            Thickness canvasPadding = SafeAreaInsets;

            if (DisplayMode == Display.CondensedPortrait)
            //if (SoftKeyboardManager.Size.Width >= CanvasArea.Width)
            {
                canvasPadding.Bottom = SoftKeyboardManager.Size.Height + App.PAGE_PADDING;
            }
            else if (DisplayMode == Display.CondensedLandscape)
            //else if (SoftKeyboardManager.Size.Height >= CanvasArea.Height)
            {
                canvasPadding.Right = SoftKeyboardManager.Size.Width + App.PAGE_PADDING;
            }

            CanvasArea.Margin = canvasPadding;
        }

        private void FunctionsDrawerSetup()
        {
            //FunctionsDrawer.Content.SetBinding<Color, StackOrientation>(BackgroundColorProperty, CrunchKeyboard, "Orientation", value => value == StackOrientation.Horizontal ? Color.Transparent : Color.White);
            //KeyboardView.SetBinding<Color, StackOrientation>(BackgroundColorProperty, CrunchKeyboard, "Orientation", value => value == StackOrientation.Horizontal ? Color.Transparent : App.BACKGROUND_COLOR);
            
            ContentView portraitAddFunctionLayout = new ContentView();
            portraitAddFunctionLayout.SetBinding<View, Display>(ContentView.ContentProperty, this, "DisplayMode", value => value == Display.Expanded ? AddFunctionLayout : null);

            ContentView landscapeAddFunctionLayout = new ContentView();
            void SetVisible()
            {
                portraitAddFunctionLayout.IsVisible = AddFunctionLayout.IsVisible && DisplayMode == Display.Expanded;
                landscapeAddFunctionLayout.IsVisible = AddFunctionLayout.IsVisible && DisplayMode != Display.Expanded;
            }
            CanvasArea.Children.Add(landscapeAddFunctionLayout, new Rectangle(0.5, 0.5, 1, -1), AbsoluteLayoutFlags.PositionProportional | AbsoluteLayoutFlags.WidthProportional);
            this.Bind<Display>(DisplayModeProperty, value =>
            {
                //landscapeAddFunctionLayout.Margin = value == Display.CondensedPortrait ? new Thickness(0, 0, 0, CrunchKeyboard.ButtonSize) : new Thickness(0);
                landscapeAddFunctionLayout.Content = value == Display.Expanded ? null : AddFunctionLayout;
                SetVisible();

                AbsoluteLayoutExtensions.SetLayout(landscapeAddFunctionLayout, new Point(0.5, value == Display.CondensedPortrait ? 1 : 0.5));
            });

            AddFunctionLayout.Bind<bool>(IsVisibleProperty, value => SetVisible());

            FullKeyboardView.Children.Insert(0, portraitAddFunctionLayout);
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
                //width += KeyboardAndVariables.Spacing + CrunchKeyboard.ButtonSize;
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

            AbsoluteLayoutExtensions.SetLayout(FullKeyboardView, new Size(width, displayMode == Display.CondensedLandscape ? height : -1));
            FunctionsDrawer.FunctionsList.Margin = new Thickness(0, 0, 0, SoftKeyboardManager.Current == CrunchSystemKeyboard.Instance ? SystemKeyboard.Instance.Size.Height : 0);
            
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
                        if (Mode != CalculatorMode.Crunch)
                        {
                            return;
                        }

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
                        if (Mode != CalculatorMode.Crunch || !SoftKeyboard.Cursor.IsDescendantOf(Canvas))
                        {
                            return;
                        }

                        Point start;
                        if (DisplayMode == Display.Expanded && !IsKeyboardDocked)
                        {
                            start = new Point(TouchScreen.FirstTouch.X, FullKeyboardView.PositionOn(Screen).Y - SoftKeyboard.Cursor.Height);
                        }
                        else
                        {
                            start = SoftKeyboard.Cursor.PositionOn(CanvasArea);
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
                if (DisplayMode == Display.Expanded && e.State == TouchState.Moving)
                {
                    DockKeyboard(false);

                    TouchScreen.Dragging += (sender1, e1) =>
                    {
                        App.KeyboardPosition.Value = AbsoluteLayout.GetLayoutBounds(FullKeyboardView).Location;
                    };
                    TouchScreen.BeginDrag(FullKeyboardView, Screen);
                }
            };

            CrunchKeyboard.ClearButton.Clicked += (sender, e) =>
            {
                Expression current = SoftKeyboard.Cursor.Parent<Equation>().LHS;
                //current.Children.Clear();
                SoftKeyboard.MoveCursor(current);
                while (current.Children.Count > 1)
                {
                    current.Children.RemoveAt(1);
                }
                BasicAnswer.Reset();
            };
            
            CrunchKeyboard.PlusMinus.Clicked += (sender, e) => BasicAnswer.Negate();

            KeyboardManager.Typed += (keystroke) =>
            {
                if (BasicAnswer.Parent != null)
                {
                    if (!SoftKeyboard.Cursor.IsDescendantOf(this))
                    {
                        AddCalculation();
                    }

                    return;
                }

                if (keystroke == KeyboardManager.BACKSPACE)
                {
                    //if (CalculationFocus != null)
                    SoftKeyboard.Delete();
                }
                else if (keystroke != '=')
                {
                    //if (CalculationFocus == null)
                    //if (SoftKeyboard.Cursor.Parent<Canvas>() == null)
                    if (!SoftKeyboard.Cursor.IsDescendantOf(this))
                    {
                        AddCalculation();
                    }

                    SoftKeyboard.Type(keystroke.ToString());
                }
            };

            KeyboardManager.CursorMoved += (key) =>
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
            };
        }

        public static int MaxBannerWidth = -1;

        private void LoadAd()
        {
            int width = (int)Math.Max(0, Math.Min(320, CanvasArea.Width - App.PAGE_PADDING * 2 - SETTINGS_BUTTON_SIZE * 2));

            if (width != MaxBannerWidth)
            {
                AbsoluteLayout.SetLayoutBounds(AdSpace, new Rectangle(0.5, 0, MaxBannerWidth = width, -1));
                AdSpace.Content = new BannerAd();
            }
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

            if (DisplayMode == Display.Expanded && IsKeyboardDocked)
            {
                FullKeyboardView.MoveTo(FullKeyboardView.X + delta.X, FullKeyboardView.Y + delta.Y);
            }
        }

        private void AdjustKeyboardPosition(object sender, EventArgs e) => AdjustKeyboardPosition();

        private void AdjustKeyboardPosition()
        {
            if (DisplayMode == Display.Expanded && IsKeyboardDocked && CalculationFocus != null)
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
            Point point = location.HasValue ? location.Value : new Point(CanvasScroll.ScrollX, CanvasScroll.ScrollY + SETTINGS_BUTTON_SIZE);

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
                EnterCursorMode(view.PositionOn(CanvasArea).Add(e.Point));//.Add(new Point(-MainPage.phantomCursor.Width / 2, -Text.MaxTextHeight))));
            }
        }

        private void DragAnswer(object sender, TouchEventArgs e)
        {
            if (sender is Answer answer && e.State == TouchState.Moving)
            {
                Link link = new Link(answer);
                link.MathContent.Touch += DragLink;
                TouchScreen.BeginDrag(link, CanvasArea, answer);
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
                TouchScreen.BeginDrag(link, CanvasArea);
            }
        }

        private void StartDraggingLink(Link link)
        {
            BoxView placeholder = link.StartDrag();

            TouchScreen.Dragging += (sender, e) =>
            {
                if (link.Parent == null)
                {
                    return;
                }

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

#if true
            SoftKeyboard.Cursor.Remove();
#else
            asdf
#endif

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
            if (!new Rectangle(Point.Zero, CanvasArea.Bounds.Size).Contains(new Rectangle(start, PhantomCursor.Bounds.Size)))
            {
                start = new Point((CanvasArea.Width - PhantomCursor.Width) / 2, (CanvasArea.Height - PhantomCursor.Height) / 2);
            }
            TouchScreen.BeginDrag(PhantomCursor, CanvasArea, start, speed);

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
            Point temp = dragging.PositionOn(Screen).Add(new Point(dragging.Width / 2, dragging.Height / 2)).Subtract(Canvas.PositionOn(Screen));// new Point(CanvasScroll.ScrollX + dragging.X + dragging.Width / 2, CanvasScroll.ScrollY + dragging.Y + dragging.Height / 2).Subtract(CanvasScroll.PositionOn(Screen));
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
                //MethodCall("Collapsed");
            }
        }
    }
}