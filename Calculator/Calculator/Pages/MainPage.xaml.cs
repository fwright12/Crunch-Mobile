using System;
using System.Extensions;
using System.Runtime.CompilerServices;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Forms.Extensions;
using Xamarin.Forms.MathDisplay;
using System.Collections.Generic;

namespace Calculator
{
    /* TODO:
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

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage, IStateTransitionManager
    {
        public enum Display { CondensedPortrait, CondensedLandscape, Expanded };

        public static readonly string PORTRAIT_MODE = "Portrait";
        public static readonly string LANDSCAPE_MODE = "Landscape";
        public static readonly string BASIC_MODE = "Basic";

        private static readonly BindableProperty DisplayModeProperty = BindableProperty.Create("DisplayMode", typeof(Display), typeof(MainPage),
            propertyChanging: (bindable, oldValue, newValue) => HandleDisplayModePropertyChange(bindable, oldValue, newValue, true),
            propertyChanged: (bindable, oldValue, newValue) => HandleDisplayModePropertyChange(bindable, oldValue, newValue, false));

        public Thickness SafeAreaInsets
        {
            get
            {
                Thickness value = (Thickness)GetValue(Xamarin.Forms.PlatformConfiguration.iOSSpecific.Page.SafeAreaInsetsProperty);

#if DEBUG
                if (App.Current.GetInSampleMode())
                {
                    value.Top = 0;
                }
#endif

                return new Thickness(
                    Math.Max(App.PAGE_PADDING, value.Left),
                    Math.Max(App.PAGE_PADDING, value.Top),
                    Math.Max(App.PAGE_PADDING, value.Right),
                    Math.Max(App.PAGE_PADDING, value.Bottom));
            }
        }

        public Display DisplayMode
        {
            get => (Display)GetValue(DisplayModeProperty);
            private set => SetValue(DisplayModeProperty, value);
        }

        public StackOrientation Orientation => DisplayMode == Display.CondensedLandscape ? StackOrientation.Horizontal : StackOrientation.Vertical;

        private bool IsKeyboardDocked => App.KeyboardPosition.Value.Equals(KeyboardHidden);

        public static readonly double SETTINGS_BUTTON_SIZE = 50;
        private readonly Point KeyboardHidden = new Point(-1000, -1000);

        //How much extra space is in the lower right
        private int ExtraPadding = 100;

        public static double ParenthesesWidth;
        public event FocusChangedEventHandler FocusChanged;
        private Calculation CalculationFocus;

        public MainPage()
        {
            Text.MaxTextHeight = App.TextHeight;
            ParenthesesWidth = App.TextWidth;
            
            Text.CreateLeftParenthesis = () => new ImageText { Text = "(", HeightRequest = 0, WidthRequest = App.TextWidth, Aspect = Aspect.Fill };
            Text.CreateRightParenthesis = () => new ImageText { Text = ")", HeightRequest = 0, WidthRequest = App.TextWidth, Aspect = Aspect.Fill };
            Text.CreateRadical = () => new Image() { Source = new FontImageSource { Glyph = "⎷", FontFamily = (OnPlatform<string>)App.Current.Resources["SymbolaFont"] }, HeightRequest = 0, WidthRequest = App.TextWidth * 2, Aspect = Aspect.Fill };

            InitializeComponent();

            SettingsMenuButton.Clicked += (sender, e) => Navigation.PushAsync(new SettingsPage());
            FunctionsMenuButton.Clicked += (sender, e) => FunctionsDrawer.ChangeStatus(true);
            
#if DEBUG
            App.Current.SetBinding<bool, bool>(Screenshots.InSampleModeProperty, AdSpace, "IsVisible", convertBack: value => !value, mode: BindingMode.OneWayToSource);
            App.Current.WhenPropertyChanged(Screenshots.InSampleModeProperty, (sender, e) => OnPropertyChanged(nameof(SafeAreaInsets)));
#endif

            AbsoluteLayout KeyboardMask = new AbsoluteLayout
            {
                BackgroundColor = Color.Gray,
                Opacity = 0.875,
            };

            SoftKeyboard.Cursor.SetDynamicResource(BackgroundColorProperty, "DetailColor");
            PhantomCursor.Bind<bool>(IsVisibleProperty, value =>
            {
                if (value && SoftKeyboardManager.Current == CrunchKeyboard)
                {
                    Screen.Children.Add(KeyboardMask, new Rectangle(FunctionsDrawer.PositionOn(Screen), FunctionsDrawer.Bounds.Size), AbsoluteLayoutFlags.None);
                }
                else
                {
                    KeyboardMask.Remove();
                }
            });

            BasicAnswer.Remove();

            /*KeyboardManager.Typed += (keystroke) =>
            {
                if (BasicAnswer.Parent == null)
                {
                    return;
                }
                
                BasicAnswer.Input(keystroke);
            };*/

            /*KeyboardManager.CursorMoved += (key) =>
            {
                if (BasicAnswer.Parent == null)
                {
                    return;
                }

                
            };*/
            
            FunctionsDrawerSetup();

            void ScreenSizeChanged()
            {
                Thickness safeArea = SafeAreaInsets;
                
                if (DisplayMode == Display.CondensedPortrait)
                {
                    safeArea.Top = App.PAGE_PADDING;
                }
                else if (DisplayMode == Display.CondensedLandscape)
                {
                    safeArea.Left = App.PAGE_PADDING;
                }
                else
                {
                    safeArea = new Thickness(10);
                }

                double borderWidth = 0;
                //safeArea = new Thickness(safeArea.Left - borderWidth, safeArea.Top - borderWidth, safeArea.Right - borderWidth, safeArea.Bottom - borderWidth);

                //FunctionsDrawer.Padding = new Thickness(borderWidth);
                FunctionsDrawer.FunctionsList.ListView.Margin = new Thickness(safeArea.Left - borderWidth, 0, safeArea.Right - borderWidth, 0);
                KeyboardView.Margin = new Thickness(0, safeArea.Top - borderWidth, 0, safeArea.Bottom - borderWidth);

                CrunchKeyboard.ScreenSizeChanged(Bounds.Size, safeArea);
            }
            SizeChanged += (sender, e) => ScreenSizeChanged();
            
            FunctionsDrawer.SetBinding<Color, Color>(BackgroundColorProperty, this, "BackgroundColor", value => value.Tint(1 - ((1 / 7.0) / (1 - Math.Max(value.R, Math.Max(value.G, value.B)))).Bound(0, 1)));

            AbsoluteLayout.SetLayoutFlags(FullKeyboardView, AbsoluteLayoutFlags.PositionProportional);
            this.WhenPropertyChanged(nameof(SafeAreaInsets), (sender, e) => ScreenSizeChanged());
            this.Bind<Display>(DisplayModeProperty, value =>
            {
                ScreenSizeChanged();

                Point location = Point.Zero;

                if (value == Display.CondensedPortrait)
                {
                    location = new Point(0.5, 1);
                }
                else if (value == Display.CondensedLandscape)
                {
                    location = new Point(1, 0.5);
                }
                else
                {
                    location = App.KeyboardPosition.Value;
                }
                
                AbsoluteLayoutExtensions.SetLayout(FullKeyboardView, location.X, location.Y);//, height: value == Display.CondensedLandscape ? Height : -1);
            });
            
            void SetOpenDrawerHeight() => FunctionsDrawer.SetDrawerHeight(false, Height * (DisplayMode == Display.Expanded ? 0.9 : 1) - (DisplayMode == Display.CondensedPortrait ? SafeAreaInsets.Top + SETTINGS_BUTTON_SIZE + App.PAGE_PADDING : 0));

            this.WhenPropertyChanged(HeightProperty, (sender, e) => SetOpenDrawerHeight());

            this.Bind<Thickness>(Xamarin.Forms.PlatformConfiguration.iOSSpecific.Page.SafeAreaInsetsProperty, value =>
            {
                SetOpenDrawerHeight();
                //CrunchKeyboard.SafeAreaChanged(SafeAreaInsets);
                SetCanvasSafeArea();
            });
            this.WhenPropertyChanged(DisplayModeProperty, (sender, e) =>
            {
                SetOpenDrawerHeight();
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

                FunctionsDrawer.SetDrawerHeight(true, SoftKeyboardManager.Size.Height);

                SetCanvasSafeArea();
            };

            CrunchKeyboard.Resources.Remove("Xamarin.Forms.Button");
            KeyboardView.Resources.Add(CrunchKeyboard.KeyboardKeyStyle);
            SoftKeyboardManager.KeyboardChanged += (sender, e) =>
            {
                Grid variables = CrunchKeyboard.Variables;

                if (sender == CrunchKeyboard)
                {
                    // CrunchKeyboard.Children.Insert(0, variables);
                    // ^ should be equivalent but throws error (cannot change observable collection)
                    CrunchKeyboard.Children.Add(variables);
                    CrunchKeyboard.LowerChild(variables);

                    variables.RemoveBinding(HeightRequestProperty);
                    KeyboardView.Content = CrunchKeyboard;
                }
                else if (sender == CrunchSystemKeyboard.Instance)
                {
                    variables.SetBinding(HeightRequestProperty, CrunchKeyboard, nameof(CrunchKeyboard.VariablesSize));
                    KeyboardView.Content = variables;
                }

                Func<Thickness, Thickness> LeftRightPadding = value => new Thickness(value.Left, 0, value.Right, 0);
                Func<Thickness, Thickness> LeftRightBottomPadding = value => new Thickness(value.Left, 0, value.Right, value.Bottom);

                FunctionsDrawer.FunctionsList.EditingToolbar.SetBinding<Thickness, Thickness>(Xamarin.Forms.Layout.PaddingProperty, this, "SafeAreaInsets", sender == CrunchSystemKeyboard.Instance ? LeftRightPadding : LeftRightBottomPadding);

                UpdateState();
            };

            SoftKeyboardManager.Dismissed += (sender, e) =>
            {
                SoftKeyboard.Cursor.Remove();

                if (sender != CrunchKeyboard)
                {
                    SoftKeyboardManager.Current.Disable();
                }
                this.Animate("HideKeyboard", PropertyAnimation.Create(FullKeyboardView, VisualElementAdditions.VisibilityProperty, 0), length: 500);
            };

            FullKeyboardView.WhenPropertyChanged(IsVisibleProperty, (sender, e) =>
            {
                Size size = SoftKeyboardManager.Current.Size;

                if (!FullKeyboardView.IsVisible)
                {
                    if (Orientation == StackOrientation.Horizontal)
                    {
                        size.Width = 0;
                    }
                    else if (Orientation == StackOrientation.Vertical)
                    {
                        size.Height = 0;
                    }
                }

                SoftKeyboardManager.OnSizeChanged(size);
            });

            //Set up for keyboards
            KeyboardContext.Focused = new MathFieldLayout().BindingContext as MathFieldViewModel;
            //Canvas.Children.Add(FocusedMathField, new Point(100, 100));

            Screen.Children.Add(new KeyboardEntry { BindingContext = CrunchKeyboard.BindingContext }, new Point(-1000, -1000));
            new CrunchSystemKeyboard();
            CrunchSystemKeyboard.Instance.Variables = CrunchKeyboard.Variables;
            WireUpKeyboard(CrunchKeyboard);
            SoftKeyboardManager.AddKeyboard(CrunchSystemKeyboard.Instance, CrunchKeyboard);
            SoftKeyboardManager.SwitchTo(CrunchKeyboard);
            
            App.BasicMode.WhenPropertyChanged(App.BasicMode.ValueProperty, (sender, e) => UpdateState());
            App.ShowFullKeyboard.WhenPropertyChanged(App.ShowFullKeyboard.ValueProperty, (sender, e) => UpdateState());
            CrunchKeyboard.WhenPropertyChanged(CrunchKeyboard.OrientationProperty, (sender, e) => UpdateState());
            CrunchKeyboard.WhenPropertyChanged(nameof(CrunchKeyboard.Collapsed), (sender, e) => UpdateState(false));

            CanvasScroll.Scrolled += AdjustKeyboardPosition;
            //CanvasScroll.Scrolled += AdjustKeyboard;
            Canvas.Touch += AddCalculation;
            FocusChanged += SwitchCalculationFocus;

            Canvas.DescendantAdded += (sender, e) =>
            {
                if (e.Element is CursorView cursor && FullKeyboardView.GetVisibility() != 1)
                {
                    SoftKeyboardManager.Current.Enable(false);
                    this.Animate("ShowKeyboard", PropertyAnimation.Create(FullKeyboardView, VisualElementAdditions.VisibilityProperty, 1), length: 500);
                }
            };
            CanvasArea.WhenPropertyChanged(WidthProperty, (sender, e) => LoadAd());
            CanvasArea.SizeChanged += (sender, e) => ExtraPadding = (int)Math.Max(CanvasArea.Width, CanvasArea.Height);
            
            void FixDynamicLag(object o) => Print.Log(o as dynamic);
            FixDynamicLag("");
        }

        public KeyboardContext<MathFieldViewModel> KeyboardContext { get; } = new KeyboardContext<MathFieldViewModel>();

        private void UpdateState(bool? animated = null)
        {
            string state = PORTRAIT_MODE;

            if (SoftKeyboardManager.Current == CrunchKeyboard)
            {
                if (CrunchKeyboard.Orientation == StackOrientation.Vertical)
                {
                    state = LANDSCAPE_MODE;
                }
                else if (App.BasicMode.Value && CrunchKeyboard.Collapsed)
                {
                    state = BASIC_MODE;
                }
            }

            void GoToState(VisualElement visualElement)
            {
                string current = visualElement.CurrentState()?.Name;

                if (!animated.HasValue)
                {
                    animated = ((current == PORTRAIT_MODE || current == CrunchKeyboard.EXPANDED_MODE) && state == BASIC_MODE) || (current == BASIC_MODE && (state == PORTRAIT_MODE || state == CrunchKeyboard.EXPANDED_MODE));
                }

                if (animated.Value)
                {
                    uint ModeTransitionLength = 2000;
                    Easing ModeEasing = Easing.SinInOut;

                    visualElement.GoToState(state, length: ModeTransitionLength, easing: ModeEasing);
                }
                else
                {
                    visualElement.GoToState(state);
                }
            }
            
            GoToState(this);

            if (state == PORTRAIT_MODE && App.ShowFullKeyboard.Value)
            {
                state = CrunchKeyboard.EXPANDED_MODE;
            }

            GoToState(CrunchKeyboard);
        }

        private Answer CurrentAnswer() => SoftKeyboard.Cursor.Parent<Equation>()?.RHS as Answer;

        void IStateTransitionManager.StateWillChange(string oldState, string newState, Animation animation)
        {
            Answer answer = CurrentAnswer();

            FunctionsDrawer.SetStatus(true, false);

            if (animation != null)
            {
                if (answer == null)
                {
                    BasicAnswer.Opacity = 0;
                    animation.WithConcurrent(PropertyAnimation.Create(BasicAnswer, OpacityProperty, (oldState != BASIC_MODE).ToInt(), (oldState == BASIC_MODE).ToInt()));
                }
                else
                {
                    BasicAnswer.Opacity = 1;
                    answer.Opacity = 0;
                    animation.WithConcurrent(new Animation(value => { }, finished: () => answer.Opacity = 1));
                }

                void StabilizeKeyboard(object sender, EventArgs e)
                {
                    AbsoluteLayoutExtensions.SetLayout(FullKeyboardView, y: Height - SoftKeyboardManager.Size.Height, flags: AbsoluteLayoutFlags.XProportional);
                }
                SoftKeyboardManager.SizeChanged += StabilizeKeyboard;
                animation.WithConcurrent(new Animation(value => { }, finished: () =>
                {
                    SoftKeyboardManager.SizeChanged -= StabilizeKeyboard;
                    AbsoluteLayoutExtensions.SetLayout(FullKeyboardView, y: 1, flags: AbsoluteLayoutFlags.PositionProportional);
                }));
            }

            if (newState == BASIC_MODE)
            {
                CanvasArea.Children.Insert(CanvasArea.Children.IndexOf(CanvasScroll) + 1, BasicAnswer);

                BasicCalcViewModel calcViewModel = (BasicCalcViewModel)CrunchKeyboard.GetValue(BindingContextProperty, BASIC_MODE);
                calcViewModel.Clear();

                if (SoftKeyboard.Cursor.Parent<Equation>() is Equation parentEquation && parentEquation.RHS is Answer answerView)
                {
                    string answerText = answerView.RawAnswer?.Format(Crunch.Polynomials.Expanded, Crunch.Numbers.Decimal, Crunch.Trigonometry.Degrees)?.ToString();

                    if (!(parentEquation.LHS.Children.Count == 1 && parentEquation.LHS.Children[0] == SoftKeyboard.Cursor))
                    {
                        (calcViewModel.Calculator as CrunchCalculator).Keyboard = null;
                        foreach (char keystroke in (answerText ?? "1/0") + "+0")
                        {
                            if (Crunch.Machine.StringClassification.IsNumber(keystroke.ToString()))
                            {
                                calcViewModel.Digit(keystroke);
                            }
                            else
                            {
                                calcViewModel.Operate(keystroke);
                            }
                        }
                        calcViewModel.EnterCommand?.Execute(null);
                        (calcViewModel.Calculator as CrunchCalculator).Keyboard = KeyboardContext;

                        parentEquation.LHS.End();
                    }
                }

                if (animation != null)
                {
                    Size size = BasicAnswer.Measure(33).Request;
                    if (answer == null)
                    {
                        size.Width = CanvasArea.Width;
                    }
                    Point point = answer?.PositionOn(CanvasArea) ?? new Point(CanvasArea.Bounds.Size - size);

                    AbsoluteLayoutExtensions.SetLayout(BasicAnswer, new Rectangle(point, size), AbsoluteLayoutFlags.None);
                    AbsoluteLayoutExtensions.ConvertLayoutBounds(BasicAnswer, AbsoluteLayoutFlags.All);
                    BasicAnswer.HeightRequest = size.Height;
                    
                    animation.WithConcurrent(value =>
                    {
                        Rectangle bounds = AbsoluteLayout.GetLayoutBounds(BasicAnswer);
                    BasicAnswer.FontSize = BasicAnswer.AutoSizeFont(bounds.Width * CanvasArea.Width, Math.Min(BasicAnswer.HeightRequest, bounds.Height * CanvasArea.Height));
                    });
                }
            }

            if (newState == PORTRAIT_MODE && animation != null)
            {
                BasicAnswer.ClearValue(LabelExtensions.AutoSizeFontProperty);
                BasicAnswer.ClearValue(HeightRequestProperty);

                if (answer == null)
                {
                    AbsoluteLayoutExtensions.SetLayout(BasicAnswer, new Rectangle(1, 1, -1, -1), AbsoluteLayoutFlags.PositionProportional);
                }
                else
                {
                    Size size = BasicAnswer.Measure() + new Size(BasicAnswer.Margin.HorizontalThickness, BasicAnswer.Margin.VerticalThickness);
                    AbsoluteLayoutExtensions.SetLayout(BasicAnswer, new Rectangle(new Point(CanvasArea.Bounds.Size - size), new Size(-1, -1)), AbsoluteLayoutFlags.None);

                    CanvasScroll.IsVisible = true;
                    animation?.WithConcurrent(PropertyAnimation.Create(BasicAnswer, AbsoluteLayout.LayoutBoundsProperty, new Rectangle(answer.PositionOn(CanvasArea), new Size(-1, -1))));
                }
            }
        }

        void IStateTransitionManager.StateDidChange(string oldState, string newState)
        {
            if (oldState == BASIC_MODE)
            {
                BasicAnswer.Remove();
            }

            FunctionsDrawer.FunctionsList.ListView.GetSwipeListener().Active = newState != BASIC_MODE;
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
            //Print.Log("onscreen keyboard size changed", width, height, SoftKeyboardManager.Current);
            //Print.Log("\t" + Bounds.Size);

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

        private bool BasicMode => App.BasicMode.Value && CrunchKeyboard.Orientation == StackOrientation.Horizontal;

        private void WireUpKeyboard(CrunchKeyboard keyboard)
        {
            foreach (Key key in keyboard)
            {
                // Long press DEL to clear the canvas
                //if (text == KeyboardManager.BACKSPACE.ToString())
                if (key.Text != "DEL")
                {
                    key.LongClick += async (sender, e) =>
                    {
                        if (BasicMode)
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
                //else if (!(key is CursorKey))
                else if (!(key.CommandParameter is KeyboardManager.CursorKey))
                {
                    key.LongClick += (sender, e) =>
                    {
                        if (BasicMode || !SoftKeyboard.Cursor.IsDescendantOf(Canvas))
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

            /*KeyboardManager.Typed += (keystroke) =>
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
                    //SoftKeyboard.Delete();
                }
                else if (keystroke != '=')
                {
                    //if (CalculationFocus == null)
                    //if (SoftKeyboard.Cursor.Parent<Canvas>() == null)
                    

                    //SoftKeyboard.Type(keystroke.ToString());
                }
            };*/
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

        public void AdjustKeyboardPosition()
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

        /*private void OnDescendantAdded(object sender, ElementEventArgs e)
        {
            if (e.Element is Calculation)
            {
                //(e.Element as Calculation).Touch += DragOnCanvas;
            }
            else if (e.Element is Equation equation)
            {
                if (e.Element.GetType() == typeof(Equation))
                {
                    //equation.LHS.Touch += EquationMoveCursor;
                }
                if (equation.RHS is Answer)
                {
                    //equation.RHS.Touch += DragAnswer;
                }

                //equation.Touch += MoveCalculation;
            }
            /*else if (e.Element is Link link)
            {
                link.Bind<Expression>(ContentView.ContentProperty, value =>
                {
                    value.Touch += DragLink;
                });
                /*link.PropertyChanged += (sender1, e1) =>
                {
                    if (e1.PropertyName == ContentView.ContentProperty.PropertyName)
                    {
                        link.MathContent.Touch += DragLink;
                    }
                };
            }
        }*/

        /*private void DragAnswer(object sender, TouchEventArgs e)
        {
            if (sender is Answer answer && e.State == TouchState.Moving)
            {
                Link link = new Link(answer);
                //link.MathContent.Touch += DragLink;
                TouchScreen.BeginDrag(link, CanvasArea, answer);
                link.StartDrag(Canvas);
            }
        }*/

        /*private void DragLink(object sender, TouchEventArgs e)
        {
            if ((sender as View)?.Parent is Link link && e.State == TouchState.Moving)
            {
                if (!TouchScreen.Active)
                {
                    link.StartDrag(Canvas);
                }
                TouchScreen.BeginDrag(link, CanvasArea);
            }
        }*/

        private void ClearCanvas()
        {
            Canvas.Children.Clear();
            Canvas.WidthRequest = (Canvas.Parent as View).Width;
            Canvas.HeightRequest = (Canvas.Parent as View).Height;

            SoftKeyboard.Cursor.Remove();

            if (IsKeyboardDocked)
            {
                AbsoluteLayoutExtensions.SetLayout(FullKeyboardView, KeyboardHidden);
                //AbsoluteLayout.SetLayoutBounds(FullKeyboardView, new Rectangle(KeyboardHidden, new Size(-1, -1)));
            }

            FocusOnCalculation(null);
        }

        public void DragCursor(View view, Point offset) => EnterCursorMode(view.PositionOn(CanvasArea).Add(offset));//.Add(new Point(-MainPage.phantomCursor.Width / 2, -Text.MaxTextHeight))));

        private void EnterCursorMode(Point start, double speed = 1)
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
                    Tuple<Expression, int> target = ExampleDrop(PhantomCursor, Canvas);
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

        public static Tuple<Expression, int> ExampleDrop(View dragging, Layout<View> dropArea = null, Layout<View> dragArea = null)
        {
            /*if (Device.RuntimePlatform != Device.iOS && (thread == null || !thread.IsAlive) && shouldScrollX + shouldScrollY != 0)
            {
                thread = new Thread(scrollCanvas);
                thread.Start();
            }*/
            //ScrollCanvas.Commit(this, "ScrollCanvas", length / 255, length, Easing.Linear, (v, c) => Value.BackgroundColor = Color.Transparent, () => false);

            //Get the coordinates of the cursor relative to the entire screen
            Point loc;
            Point temp = dragging.PositionOn(dragArea).Add(new Point(dragging.Width / 2, dragging.Height / 2));

            if (dropArea != null)
            {
                temp = temp.Subtract(dropArea.PositionOn(dragArea));
            }

            View view = GetViewAt(dropArea, temp, out loc);
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

        private static View GetViewAt(Layout<View> layout, Point point, out Point scaled)
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