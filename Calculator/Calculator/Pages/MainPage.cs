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

    /* To do:
     * v2.4.1
     * Fix ios safe area issues
     * Show full keyboard broken
     * Check that variable row expanded saves
     * 
     * Android Button Touch renderer long press problems
     * Make TouchInterface use variant eventhandler
     * 
     * v3.0
     * Makeover - color scheme, better icons
     * Basic calculator mode
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
     * render (sinx)^2 as sin^2(x)
     * Look for uneccessary hashing
     */

    public delegate void FocusChangedEventHandler(Calculation oldFocus, Calculation newFocus);
    public delegate void RenderedEventHandler();

#if DEBUG
    public class AbsoluteLayoutMeasureProblemsDemo : ContentPage
    {
        public AbsoluteLayoutMeasureProblemsDemo()
        {
            AbsoluteLayout a = new AbsoluteLayout
            {
                BackgroundColor = Color.LightBlue
            };
            Content = a;

            Xamarin.Forms.Slider widthSlider;
            Xamarin.Forms.Slider heightSlider;

            a.Children.Add(new StackLayout
            {
                Orientation = StackOrientation.Vertical,
                Children =
                {
                    (widthSlider = new Slider
                    {
                        Maximum = 1000,
                        Minimum = 0,
                        Value = 300,
                    }),
                    (heightSlider = new Slider
                    {
                        Maximum = 1000,
                        Minimum = 0,
                        Value = 300,
                    })
                },
                BackgroundColor = Color.Red,
                WidthRequest = 300,
                HeightRequest = 50
            }, new Rectangle(0.5, 0.5, -1, -1), AbsoluteLayoutFlags.PositionProportional);

            StackLayout s = new StackLayout
            {
                Orientation = StackOrientation.Vertical,
                Children =
                {
                    new Button { Text = "hi", VerticalOptions = LayoutOptions.Start, HorizontalOptions = LayoutOptions.Start },
                    new Button { Text = "hi", VerticalOptions = LayoutOptions.Start, HorizontalOptions = LayoutOptions.Start }
                },
                BackgroundColor = Color.Purple
            };
            a.Children.Add(s);

            widthSlider.ValueChanged += (sender, e) =>
            {
                AbsoluteLayout.SetLayoutBounds(s, new Rectangle(0, 0, -1, e.NewValue));
            };
            heightSlider.ValueChanged += (sender, e) =>
            {
                AbsoluteLayout.SetLayoutBounds(s, new Rectangle(0, 0, e.NewValue, -1));
            };
        }
    }
#endif

    public class MainPage : ContentPage
    {
        public static readonly BindableProperty CollapsedProperty = BindableProperty.Create("Collapsed", typeof(bool), typeof(MainPage));

        public bool Collapsed
        {
            get { return (bool)GetValue(CollapsedProperty); }
            private set { SetValue(CollapsedProperty, value); }
        }

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
        public readonly CrunchKeyboard CrunchKeyboard;
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
                        }
                    }),
                    (PhantomCursor = new CursorView
                    {
                        BindingContext = SoftKeyboard.Cursor,
                        Color = Color.Red,
                        IsVisible = false,
                    })
                }
            };
            PhantomCursor.SetBinding(HeightRequestProperty, "Height");

            //Canvas.Children.Add(new Label { Text = "⚙⛭", FontFamily = CrunchStyle.SYMBOLA_FONT, FontSize = 50 }, new Point(0, 100));
            //canvas.Children.Add(new Label { Text = "˂<‹〈◁❬❰⦉⨞⧼︿＜⏴⯇🞀", FontFamily = CrunchStyle.SYMBOLA_FONT }, new Point(0, 100));
            //canvas.Children.Add(new Label { Text = "🌐\u1F310\u1F30F\u1F311", FontFamily = CrunchStyle.SYMBOLA_FONT }, new Point(0, 200));
            /*canvas.Children.Add(new Expression(Render.Math("log_(4)-9")), new Point(100, 100));
            canvas.Children.Add(new Expression(Render.Math("log_-9(4)")), new Point(200, 200));
            canvas.Children.Add(new Expression(Render.Math("log_-9-4")), new Point(300, 300));*/

            //AbsoluteLayout maskedKeys;
            CrunchKeyboard = new CrunchKeyboard();
            
            KeyboardAndVariables = new TouchableStackLayout
            {
                BindingContext = CrunchKeyboard,
                Orientation = StackOrientation.Vertical,
                Children =
                {
                    CrunchKeyboard,
                }
            };

            /*Grid grid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition(),
                    new ColumnDefinition()
                },
                RowDefinitions = new RowDefinitionCollection
                {
                    new RowDefinition(),
                    new RowDefinition()
                },
                Children =
                {
                    Variables,
                    CrunchKeyboard
                }
            };
            grid.ColumnDefinitions[1].SetBinding(ColumnDefinition.WidthProperty, CrunchKeyboard, "Width");
            grid.RowDefinitions[1].SetBinding(RowDefinition.HeightProperty, CrunchKeyboard, "Height");*/

            //maskedKeys.SetBinding(IsVisibleProperty, CrunchKeyboard, "IsVisible");
            //KeyboardMask.SetBinding(IsVisibleProperty, PhantomCursor, "IsVisible");
            KeyboardAndVariables.SetBinding(StackLayout.OrientationProperty, "Orientation", converter: new ValueConverter<StackOrientation>(StackLayoutExtensions.Invert));

            KeyboardMask = new AbsoluteLayout
            {
                BackgroundColor = Color.Gray,
                Opacity = 0.875,
            };

            PhantomCursor.WhenPropertyChanged(IsVisibleProperty, (sender, e) =>
            {
                if (PhantomCursor.IsVisible)
                {
                    Screen.Children.Add(KeyboardMask, new Rectangle(CrunchKeyboard.PositionOn(Screen), CrunchKeyboard.Bounds.Size), AbsoluteLayoutFlags.None);
                }
                else
                {
                    KeyboardMask.Remove();
                }
            });

            App.ShowFullKeyboard.WhenPropertyChanged(App.ShowFullKeyboard.ValueProperty, (sender, e) =>
            //Settings.KeyboardChanged += (e) =>
            {
                //CrunchKeyboard.Remeasure();
                //ResizeKeyboard();
            });

            AddFunction addFunctionLayout = new AddFunction
            {
                IsVisible = false,
                CornerRadius = 20,
                BackgroundColor = Color.White,
                HasShadow = false,
                Padding = new Thickness(0, 0, 0, 5),
            };
            FullKeyboardView = FunctionsDrawer = new FunctionsDrawer(KeyboardAndVariables, addFunctionLayout)
            {
                DropArea = Canvas,
            };

            FunctionsDrawer.Content.SetBinding<Color, StackOrientation>(BackgroundColorProperty, CrunchKeyboard, "Orientation", value => value == StackOrientation.Horizontal ? Color.Transparent : Color.White);
            KeyboardAndVariables.SetBinding<Color, StackOrientation>(BackgroundColorProperty, CrunchKeyboard, "Orientation", value => value == StackOrientation.Horizontal ? Color.Transparent : CrunchStyle.BACKGROUND_COLOR);

            ContentView portraitAddFunctionLayout = new ContentView();
            portraitAddFunctionLayout.SetBinding(IsVisibleProperty, addFunctionLayout, "IsVisible");
            portraitAddFunctionLayout.SetBinding<View, StackOrientation>(ContentView.ContentProperty, CrunchKeyboard, "Orientation", value => value == StackOrientation.Horizontal ? addFunctionLayout : null);
            (FunctionsDrawer.Content as StackLayout)?.Children.Insert(0, portraitAddFunctionLayout);

            ContentView landscapeAddFunctionLayout = new ContentView();
            landscapeAddFunctionLayout.SetBinding(IsVisibleProperty, addFunctionLayout, "IsVisible");
            landscapeAddFunctionLayout.SetBinding<View, StackOrientation>(ContentView.ContentProperty, CrunchKeyboard, "Orientation", value => value == StackOrientation.Horizontal ? null : addFunctionLayout);
            CanvasArea.Children.Add(landscapeAddFunctionLayout, new Rectangle(0.5, 0.5, 1, -1), AbsoluteLayoutFlags.PositionProportional | AbsoluteLayoutFlags.WidthProportional);

            Screen.Children.Add(FullKeyboardView);
            AbsoluteLayout.SetLayoutFlags(FullKeyboardView, AbsoluteLayoutFlags.PositionProportional);

            void SafeInsetsChanged()
            {
                Thickness safeInsets = Xamarin.Forms.PlatformConfiguration.iOSSpecific.Page.SafeAreaInsets(On<Xamarin.Forms.PlatformConfiguration.iOS>());

                CanvasArea.Padding = new Thickness(
                    Math.Max(CrunchStyle.PAGE_PADDING, safeInsets.Left),
                    Math.Max(CrunchStyle.PAGE_PADDING, safeInsets.Top),
                    Math.Max(CrunchStyle.PAGE_PADDING, safeInsets.Right),
                    0);

                Thickness keyboardPadding = new Thickness(CanvasArea.Padding.Left, 0, CanvasArea.Padding.Right, Math.Max(CrunchStyle.PAGE_PADDING, safeInsets.Bottom));
                if (CrunchKeyboard.Orientation == StackOrientation.Horizontal)
                {
                    keyboardPadding.Top = 0;
                }
                else
                {
                    //keyboardPadding.Left = 0;
                }
                Print.Log("padding", keyboardPadding.Left, CanvasArea.Padding.Left);
                KeyboardAndVariables.Padding = keyboardPadding;

                //FunctionsDrawer.FunctionsList.ListView.Margin.Bottom = 0;

                //OnSizeChanged();
            };

            this.WhenPropertyChanged(Xamarin.Forms.PlatformConfiguration.iOSSpecific.Page.SafeAreaInsetsProperty, (sender, e) => SafeInsetsChanged());
            SafeInsetsChanged();

            Variables = new VariableRow
            {
                BindingContext = CrunchKeyboard,
                //HorizontalOptions = LayoutOptions.End,
                //VerticalOptions = LayoutOptions.End,
                Orientation = StackOrientation.Horizontal,
                ButtonSize = 33
            };
            Variables.SetBinding(StackLayout.SpacingProperty, "Spacing");
            KeyboardAndVariables.Children.Insert(0, Variables);
            //variables.SetBinding(StackLayout.OrientationProperty, "Orientation");

            KeyboardAndVariables.WhenPropertyChanged(WidthProperty, (sender, e) =>
            {
                if (KeyboardManager.Current == CrunchKeyboard && CrunchKeyboard.Orientation == StackOrientation.Vertical)
                {
                    OnscreenKeyboardSizeChanged(KeyboardAndVariables.Width, KeyboardAndVariables.Height);
                }
            });
            Crunch.Mobile.SoftKeyboard.SizeChanged += (sender, e) => OnscreenKeyboardSizeChanged(e.Value.Width, e.Value.Height);

            //Set up for keyboards
            SystemKeyboard.Setup(Screen);
            WireUpKeyboard(CrunchKeyboard);
            KeyboardManager.AddKeyboard(SystemKeyboard.Instance, CrunchKeyboard);
            KeyboardManager.SwitchTo(CrunchKeyboard);

            SettingsMenuSetUp();

            CanvasScroll.Scrolled += AdjustKeyboardPosition;
            //CanvasScroll.Scrolled += AdjustKeyboard;
            Canvas.Touch += AddCalculation;
            FocusChanged += SwitchCalculationFocus;

            Canvas.DescendantAdded += OnDescendantAdded;
            SizeChanged += (sender, e) => OnSizeChanged();
            CanvasArea.WhenPropertyChanged(PaddingProperty, (sender, e) => OnSizeChanged());
            
            void FixDynamicLag(object o) => Print.Log(o as dynamic);
            FixDynamicLag("");
        }

        private void OnscreenKeyboardSizeChanged(double width, double height)
        {
            Print.Log("onscreen keyboard size changed", width, height);

            Size screenSize = App.Current.MainPage.Bounds.Size;
            Print.Log("\t" + screenSize);
            
            Collapsed = true;
            bool portrait = screenSize.Height >= screenSize.Width;

            if (KeyboardManager.Current == CrunchKeyboard && CrunchKeyboard.Orientation == StackOrientation.Vertical)
            {
                Variables.Orientation = StackOrientation.Vertical;
                //Variables.SizeRequest(-1, height);
            }
            else
            {
                Variables.Orientation = StackOrientation.Horizontal;
                //Variables.SizeRequest(width, -1);
            }
            //FullKeyboardView.WidthRequest = width;

            if (Collapsed)
            {
                if (portrait)
                {
                    Print.Log("canvas area height is " + (screenSize.Height - height));
                    AbsoluteLayout.SetLayoutBounds(CanvasArea, new Rectangle(0, 0, 1, screenSize.Height - height));
                    AbsoluteLayout.SetLayoutFlags(CanvasArea, AbsoluteLayoutFlags.WidthProportional);
                }
                else
                {
                    AbsoluteLayout.SetLayoutBounds(CanvasArea, new Rectangle(0, 0, screenSize.Width - width, 1));
                    AbsoluteLayout.SetLayoutFlags(CanvasArea, AbsoluteLayoutFlags.HeightProportional);
                }
            }
            else
            {
                AbsoluteLayout.SetLayoutBounds(CanvasArea, new Rectangle(0, 0, 1, 1));
                AbsoluteLayout.SetLayoutFlags(CanvasArea, AbsoluteLayoutFlags.SizeProportional);
            }

            Point keyboardPosition = Collapsed ? new Point(1, 1) : App.KeyboardPosition.Value;
            AbsoluteLayout.SetLayoutBounds(FullKeyboardView, new Rectangle(keyboardPosition, new Size(-1, -1)));
        }

        public void Tutorial()
        {
            Tutorial tutorial = new Tutorial(CrunchKeyboard.IsCondensed);

            ModalView popup = new ModalView
            {
                Content = tutorial,
                Padding = new Thickness(20, 20, 20, 0),
                BackgroundColor = Color.White
            };

            tutorial.Completed += () =>
            {
                popup.Remove();
                App.TutorialRunning = false;
            };

            Screen.Children.Add(popup, new Rectangle(0.5, 0.5, -1, -1), AbsoluteLayoutFlags.PositionProportional);
        }

        public void ShowTip(string explanation, string url)
        {
            WebImage gif = new Image
            {
                Source = new UriImageSource { CachingEnabled = false, Uri = new Uri(url) },
                IsAnimationPlaying = true,
            };
            gif.ErrorText.Text += "\n\n(all tips can also be viewed in settings)";

            CheckBox showTips = new CheckBox
            {
                IsChecked = true,
                Color = CrunchStyle.CRUNCH_PURPLE,
                BindingContext = App.ShowTips
            };
            showTips.SetBinding(CheckBox.IsCheckedProperty, "Value", BindingMode.TwoWay);
            
            Label dismiss = new Label
            {
                HorizontalOptions = LayoutOptions.Center,
                Text = "Dismiss",
            };

            ModalView popup = new ModalView { };
            popup.WhenDescendantAdded<View>((view) =>
            {
                view.VerticalOptions = LayoutOptions.Center;
            });
            popup.Content = new StackLayout
            {
                Orientation = StackOrientation.Vertical,
                Children =
                {
                    new StackLayout
                    {
                        Orientation = StackOrientation.Horizontal,
                        Children =
                        {
                            new Label
                            {
                                HorizontalOptions = LayoutOptions.Start,
                                Text = "Did you know?",
                                FontSize = NamedSize.Large.On<Label>(),
                            }
                        }
                    },
                    new Label
                    {
                        HorizontalOptions = LayoutOptions.Center,
                        HorizontalTextAlignment = TextAlignment.Center,
                        Text = explanation,
                        FontSize = NamedSize.Body.On<Label>(),
                        Margin = new Thickness(0, 10, 0, 10)
                    },
                    gif,
                    new StackLayout
                    {
                        Orientation = StackOrientation.Horizontal,
                        HorizontalOptions = LayoutOptions.Center,
                        Spacing = 0,
                        Children =
                        {
                            new Label
                            {
                                HorizontalTextAlignment = TextAlignment.Center,
                                Text = "Show new tips at startup",
                                FontSize = NamedSize.Caption.On<Label>()
                            },
                            showTips,
                        }
                    },
                    dismiss
                }
            };
            
            TapGestureRecognizer tgr = new TapGestureRecognizer();
            tgr.Tapped += (sender, e) =>
            {
                popup.Remove();
            };
            dismiss.GestureRecognizers.Add(tgr);

            Screen.Children.Add(popup, new Rectangle(0.5, 0.5, -1, -1), AbsoluteLayoutFlags.PositionProportional);
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

            /*FunctionsDrawer = new FunctionsDrawer
            {
                DropArea = Canvas
            };
            PhantomCursorField.Children.Add(FunctionsDrawer);
            AbsoluteLayout.SetLayoutBounds(FunctionsDrawer, new Rectangle(0, 0, App.Current.Collapsed ? 1 : 400, 0.9));
            AbsoluteLayout.SetLayoutFlags(FunctionsDrawer, App.Current.Collapsed ? AbsoluteLayoutFlags.SizeProportional : AbsoluteLayoutFlags.HeightProportional);
            App.Current.WhenPropertyChanged(App.CollapsedProperty, (sender, e) =>
            {
                
            });*/

            Button functionsMenuButton = new Button
            {
                Text = "f(x)",
                FontSize = NamedSize.Large.On<Button>(),
            };
            functionsMenuButton.Clicked += (sender, e) =>
            {
                FunctionsDrawer.ChangeStatus();

                return;
                if (CrunchKeyboard.Orientation == StackOrientation.Vertical)
                {
                    int y = (int)((FunctionsDrawer.FunctionsList.ListView.Header as View)?.Height ?? (CrunchKeyboard.Height + 25) - CrunchKeyboard.Height);
                    Print.Log("here", y, (FunctionsDrawer.FunctionsList.ListView.Header as View)?.Height, CrunchKeyboard.Height);
                    FunctionsDrawer.FunctionsList.ListView.ScrollToPosition(0, y, true);
                }
            };

            SettingsMenuButton.WidthRequest = SettingsButtonSize;
            functionsMenuButton.WidthRequest = SettingsButtonSize;
            TouchableStackLayout header = new TouchableStackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Children =
                {
                    SettingsMenuButton,
                    new ContentView
                    {
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                    },
                    functionsMenuButton
                }
            };

            CanvasArea.Children.Add(header, new Rectangle(0, 0, 1, SettingsButtonSize), AbsoluteLayoutFlags.WidthProportional);
            CanvasArea.Children.Add(AdSpace = new ContentView
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
            });
#if DEBUG
            App.Current.SetBinding<bool, bool>(Screenshots.InSampleModeProperty, AdSpace, "IsVisible", convertBack: value => !value, mode: BindingMode.OneWayToSource);
#endif
            AbsoluteLayout.SetLayoutFlags(AdSpace, AbsoluteLayoutFlags.XProportional);
            //Screen.Children.Add(SettingsMenuButton, new Rectangle(0, 0, SettingsButtonSize, SettingsButtonSize), AbsoluteLayoutFlags.PositionProportional);
            //Screen.Children.Add(functionsMenuButton, new Rectangle(1, 0, SettingsButtonSize, SettingsButtonSize), AbsoluteLayoutFlags.PositionProportional);
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
                        if (!CrunchKeyboard.IsCondensed && !IsKeyboardDocked)
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
            if (Width < 0)
            {
                return;
            }
            
            bool isCondensedLandscape = KeyboardManager.Current == CrunchKeyboard && CrunchKeyboard.Orientation == StackOrientation.Vertical && CrunchKeyboard.IsCondensed;
            int width = (int)Math.Min(320, AdSpace.Width);
            
            if (width != MaxBannerWidth)
            {
                MaxBannerWidth = width;

                /*if (ad != null)
                {
                    ad.Remove();
                }*/

                AdSpace.Content = ad = new BannerAd();
                AbsoluteLayout.SetLayoutBounds(AdSpace, new Rectangle(0.5, 0, width, -1));

                /*if (isCondensedLandscape)
                {
                    Screen.Children.Add(ad, new Rectangle(SettingsButtonSize + Padding.Left, 0, width, -1), AbsoluteLayoutFlags.None);
                }
                else
                {
                    Screen.Children.Add(ad, new Rectangle(0.5, 0, width, -1), AbsoluteLayoutFlags.XProportional);
                }*/
            }
        }

        private bool Loaded = false;

        private void OnSizeChanged()
        {
            //Print.Log("page size changed", CanvasArea.Width, CanvasArea.Height, Padding.Left, Padding.Top);

            if (!Loaded)
            {
                CrunchKeyboard.SizeChanged += (sender, e) => LoadAd();
                Loaded = true;
            }
            LoadAd();

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
                FullKeyboardView.MoveTo(CalculationFocus.X - CanvasScroll.ScrollX, CalculationFocus.Y + CalculationFocus.Height - CanvasScroll.ScrollY);
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
            Point temp = new Point(CanvasScroll.ScrollX + dragging.X + dragging.Width / 2, CanvasScroll.ScrollY + dragging.Y + dragging.Height / 2);
            View view = GetViewAt(Canvas, temp, out loc);

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
    }
}
 