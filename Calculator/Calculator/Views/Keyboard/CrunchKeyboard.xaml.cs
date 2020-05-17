using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Extensions;

namespace Calculator
{
    public class ButtonFeedbackBehavior : Behavior<Button>
    {
        private readonly double Opacity = 0.25;

        protected override void OnAttachedTo(Button bindable)
        {
            base.OnAttachedTo(bindable);

            bindable.Bind<Color>(Button.BorderColorProperty, value => bindable.BackgroundColor = value.WithAlpha(Opacity));
            //bindable.SetBinding<Color, Color>(Button.BackgroundColorProperty, bindable, "BorderColor", value => value.WithAlpha(Opacity));
            bindable.Clicked += Clicked;
        }

        protected override void OnDetachingFrom(Button bindable)
        {
            base.OnDetachingFrom(bindable);
            bindable.Clicked += Clicked;
        }


        private void Clicked(object sender, EventArgs e)
        {
            Button button = (Button)sender;

            button.BackgroundColor = button.BackgroundColor.WithAlpha(1);
            button.Animate("fade", new PropertyAnimation(button, VisualElement.BackgroundColorProperty, button.BackgroundColor.WithAlpha(Opacity)), length: 500);
        }
    }

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CrunchKeyboard : Grid, IEnumerable<Key>, ISoftKeyboard
    {
        public event KeystrokeEventHandler Typed;
        public event EventHandler OnscreenSizeChanged;

        public double Spacing { get; set; } = 6;

        public static readonly BindableProperty OrientationProperty = BindableProperty.Create(nameof(Orientation), typeof(StackOrientation), typeof(CrunchKeyboard), StackOrientation.Horizontal, propertyChanged: (bindable, oldvalue, newvalue) => ((CrunchKeyboard)bindable).InvalidateLayout());

        public static readonly BindableProperty IsCondensedProperty = BindableProperty.Create(nameof(IsCondensed), typeof(bool), typeof(CrunchKeyboard), false, propertyChanged: (bindable, oldvalue, newvalue) => ((CrunchKeyboard)bindable).InvalidateLayout());

        public StackOrientation Orientation
        {
            get => (StackOrientation)GetValue(OrientationProperty);
            protected set => SetValue(OrientationProperty, value);
        }

        public bool IsCondensed
        {
            get => (bool)GetValue(IsCondensedProperty);
            set => SetValue(IsCondensedProperty, value);
        }

        private readonly Key EXP = new Key { Text = "x\u207F", Basic = "^" };
        private readonly Key SQRD = new Key { Text = "x\u00B2", Basic = "^2" };
        private readonly Key DIV = new Key { Text = "\u00F7", Basic = "/" };
        private readonly Key LOG = new Key { Text = "log", Basic = "log_" };
        private readonly Key SQRT = new Key { Text = "\u221A", Basic = "√" };// { FontFamily = CrunchStyle.SYMBOLA_FONT };
        private readonly Key MULT = new Key { Text = "\u00D7", Basic = "*" };
        private readonly Key TEN = new Key { Text = "10\u207F", Basic = "10^" };
        private readonly Key PI = new Key { Text = "\u03C0" };

        private readonly View[][] Keys;

        //private RowDefinition VariablesRow;// = new RowDefinition { Height = new GridLength(0.5, GridUnitType.Star) };
        //private ColumnDefinition PermanentKeysColumn;// = new ColumnDefinition { Width = new GridLength(1.25, GridUnitType.Star) };

        private readonly int MAX_BUTTON_SIZE = 75;
        private double PERMANENT_KEYS_INCREASE = 1.25;
        private double MIN_COLUMNS = 4;
        private readonly int RecentlyUsed = 10;

        //public int Rows => Keys.Length + this.Orient(0, 1);
        //public double MaxColumns => Keys[0].Length + this.Orient(PERMANENT_KEYS_INCREASE, 0);
        //public double Columns => this.Orient((IsCondensed ? MIN_COLUMNS : Keys[0].Length) + PERMANENT_KEYS_INCREASE, MIN_COLUMNS);
        //public bool ShowingFullKeyboard => !App.IsCondensed && Settings.ShouldShowFullKeyboard;

        public Size FullSize(double columns, double rows) => new Size((columns - 1 + PERMANENT_KEYS_INCREASE) * MAX_BUTTON_SIZE + (columns - 1) * ColumnSpacing, (rows - 1) * MAX_BUTTON_SIZE + RowDefinitions[0].Height.Value + (rows - 1) * RowSpacing);
            /*new Size(
            PaddedButtonsWidth(Keys[0].Length + PERMANENT_KEYS_INCREASE, MAX_BUTTON_SIZE),
            PaddedButtonsWidth(Keys.Length, MAX_BUTTON_SIZE)
            );*/

        public Size Size { get; private set; }
        public double ButtonSize { get; set; }

        private GridDefinition RegularGridDefinition = new GridDefinition(5, 5);

        private GridDefinition BasicGridDefinition = new GridDefinition(4, 5);
        private GridDefinition SidewaysGridDefinition = new GridDefinition(5, 5);

        public CrunchKeyboard()
        {
            //Resources = new ResourceDictionary();
            // Make spacing consistent between all buttons
            /*this.WhenDescendantAdded<Grid>((grid) =>
            {
                grid.BindingContext = this;
                grid.SetBinding(Grid.RowSpacingProperty, "RowSpacing");
                grid.SetBinding(Grid.ColumnSpacingProperty, "ColumnSpacing");
            });*/
            
            void InitialButtonSetup(object sender, ElementEventArgs e)
            {
                //this.WhenDescendantAdded<Button>((button) =>
                if (e.Element is Button button)
                {
                    button.PropertyChanged += (sender1, e1) =>
                    {
                        if (!e1.IsProperty(WidthProperty, HeightProperty, Button.FontSizeProperty))// || button.Text == null)
                        {
                            return;
                        }

                        int length = button.Text?.Length ?? 0;
                        button.FontSize = length > 1 ?
                            Math.Floor(33 * button.Width / 75 * 5 / (4 + length)) :
                            Math.Floor(33 * Math.Max(button.Height, button.Width) / 75);
                    };
                }
                if (e.Element is CursorKey cursorKey)
                {
                    cursorKey.Clicked += (sender1, e1) => KeyboardManager.MoveCursor(cursorKey.Key);
                }
                //this.WhenExactDescendantAdded<Key>((key) =>
                else if (e.Element is Key key)
                {
                    key.Clicked += (sender1, e1) => Typed?.Invoke(key.Output);
                }
                //this.WhenExactDescendantAdded<CursorKey>((cursorKey) =>
            }
            DescendantAdded += InitialButtonSetup;
            RowSpacing = 2;
            ColumnSpacing = 2;
            InitializeComponent();

            Grid parentheses = new Grid
            {
                Children =
                {
                    { new Key { Text = "(" }, 0, 0 },
                    { new Key { Text = ")" }, 1, 0 }
                }
            };
            foreach (View view in parentheses.Children)
            {
                view.SetDynamicResource(Button.BorderColorProperty, "PrimaryKeyboardKeyColor");
            }

            double InverseOpacity(double value) => 1 - value;
            parentheses.SetBinding<double, double>(OpacityProperty, EqualsKey, "Opacity", InverseOpacity, InverseOpacity, BindingMode.TwoWay);

            //equals.SetBinding<bool, double>(IsVisibleProperty, equals, "Opacity", value => value > 0);
            Keys = new View[][]
            {
                //new Key[] { null,  null,    null,   null,   null,   null,   null },
                new Key[] { "sin",  TEN,    EXP,    "7",    "8",    "9",    DIV },
                new Key[] { "cos",  LOG,    SQRD,   "4",    "5",    "6",    MULT },
                new Key[] { "tan",  "ln",   SQRT,    "1",    "2",    "3",    "-" },
                new List<View>(new Key[] { PI,     "e",    "x",    "0",    ".",    "()",   "+" }).ToArray()
            };
            Keys[3][5] = parentheses;

            // Rotate the arrow text for the up and down keys
            foreach (View view in new View[] { ArrowKeys.Children[0], ArrowKeys.Children[3] })
            {
                if (!(view is LabelButton labelButton))
                {
                    continue;
                }

                labelButton.Label.Rotation = 90;
                labelButton.Label.SetBinding<Rectangle, double>(AbsoluteLayout.LayoutBoundsProperty, labelButton.Button, "Width", value => new Rectangle(0.5, 0.5, value, value));
                //AbsoluteLayout.SetLayoutBounds(labelButton.Label, new Rectangle(0.5, 0.5, 100, 100));
            }

            BackspaceButton.Basic = KeyboardManager.BACKSPACE.ToString();
            ExpandButton.Clicked += (sender, e) => SoftKeyboardManager.OnDismissed();
            NextKeyboardButton.Clicked += (sender, e) => SoftKeyboardManager.NextKeyboard();

            foreach (char c in App.Variables)
            {
                Button button = new Button
                {
                    Text = c.ToString(),
                };
                button.Clicked += (sender, e) =>
                {
                    int index = App.Variables.IndexOf(c);

                    if (index > RecentlyUsed - 1)
                    {
                        App.Variables.RemoveAt(index);
                        App.Variables.Insert(0, c);

                        View v = VariableLayout.Children[index];
                        VariableLayout.Children.RemoveAt(index);
                        VariableLayout.Children.Insert(0, v);

                        (VariableLayout.Parent as ScrollView)?.MakeVisible(v);
                    }

                    KeyboardManager.Type(c.ToString());
                };
                button.SetBinding(WidthRequestProperty, ExpandButton, "Width");
                button.SetBinding(HeightRequestProperty, ExpandButton, "Height");

#if !(ANDROID && DEBUG)
                VariableLayout.Children.Add(button);
#endif
            }
            //Variables.SetBinding<bool, StackOrientation>(GridExtensions.IsTransposedProperty, VariableLayout, "Orientation", convertBack: value => value ? StackOrientation.Vertical : StackOrientation.Horizontal, mode: BindingMode.OneWayToSource);
            //VariableLayout.SetBinding(StackLayout.OrientationProperty, Variables, "Orientation");

            for (int i = 0; i < Keys.Length; i++)
            {
                Keypad.RowDefinitions.Add(new RowDefinition());
            }
            for (int i = 0; i < Keys[0].Length; i++)
            {
                Keypad.ColumnDefinitions.Add(new ColumnDefinition());
            }

            for (int i = 0; i < Keys.Length; i++)
            {
                for (int j = 0; j < Keys[i].Length; j++)
                {
                    View view = Keys[i][j];

                    if (view == null)
                    {
                        continue;
                    }

                    /*if (Keys[i][j].Text == "()")
                    {
                        
                    }
                    else
                    {
                        if (Keys[i].Length - j > MIN_COLUMNS)
                        {
                            //Keys[i][j].Clicked += (sender, e) => Scroll.ScrollToAsync(Keypad, ScrollToPosition.End, false);
                        }
                    }*/

                    if (view is Button button)
                    {
                        if (j == Keys[i].Length - 1)
                        {
                            button.BorderColor = Color.DarkOrange;
                        }
                        else
                        {
                            button.SetDynamicResource(Button.BorderColorProperty, "PrimaryKeyboardKeyColor");
                        }
                        //button.BorderColor = j == Keys[i].Length - 1 ? Color.DarkOrange : Color.Black;// Color.FromHex("#C4C3D0");
                    }

                    Keypad.Children.Add(view, j, i);
                }
            }
            
            RegularGridDefinition.ColumnDefinitions.Last().Width = new GridLength(1.25, GridUnitType.Star);
            //VariablesRow = RegularGridDefinition.RowDefinitions[0];
            //PermanentKeysColumn = RegularGridDefinition.ColumnDefintions.Last();

            VisualStateManager.GetVisualStateGroups(this).Add(new VisualStates("Regular", "Basic")
            {
                new Setters { Property = RowDefinitionsProperty, Values = { RegularGridDefinition.RowDefinitions, BasicGridDefinition.RowDefinitions } },
                new Setters { Property = ColumnDefinitionsProperty, Values = { RegularGridDefinition.ColumnDefinitions, BasicGridDefinition.ColumnDefinitions } },
                /*new Setters
                {
                    Property = HeightRequestProperty,
                    Values =
                    {
                        //new Thunk<double>(() => MeasureRegularMode().Height - Padding.VerticalThickness),
                        //new Thunk<double>(() => MeasureBasicMode().Height - Padding.VerticalThickness)
                    }
                },*/
                new TargetedSetters
                {
                    Targets = BackspaceButton,
                    Setters =
                    {
                        new Setters
                        {
                            Property = MarginProperty,
                            Values =
                            {
                                new Thickness(0),
                                new Thunk<Thickness>(() =>
                                {
                                    double final = ButtonWidth(Width - Padding.HorizontalThickness, 4);
                                    return new Thickness(-final, -final - RowSpacing, 0, final + RowSpacing + (final + RowSpacing) / 2);
                                })
                            }
                        }
                    }
                },
                new TargetedSetters
                {
                    Targets = BottomRight,
                    Setters =
                    {
                        new Setters
                        {
                            Property = MarginProperty,
                            Values =
                            {
                                new Thickness(0),
                                new Thunk<Thickness>(() =>
                                {
                                    double final = ButtonWidth(Width - Padding.HorizontalThickness, 4);
                                    return new Thickness(-final, (final + RowSpacing) / 2, 0, 0);
                                })
                            }
                        }
                    }
                },
                new TargetedSetters
                {
                    Targets = { EqualsKey, ClearButton, PlusMinus, NewCalculationKey },
                    Setters =
                    {
                        new Setters { Property = VisualElementExtensions.VisibilityProperty, Values = { 0, 1 } },
                    }
                },
                new TargetedSetters
                {
                    Targets = { parentheses, VariableLayout, NextKeyboardButton.Parent, ExpandButton },
                    Setters =
                    {
                        new Setters { Property = VisualElementExtensions.VisibilityProperty, Values = { 1, 0 } },
                    }
                },
                /*new TargetedSetters
                {
                    Targets = VariablesRow,
                    Setters =
                    {
                        new Setters { Property = RowDefinition.HeightProperty, Values = { new GridLength(0.5, GridUnitType.Star), new GridLength(1, GridUnitType.Star) } }
                    }
                },*/
                /*new TargetedSetters
                {
                    Targets = PermanentKeysColumn,
                    Setters =
                    {
                        new Setters { Property = ColumnDefinition.WidthProperty, Values = { new GridLength(1.25, GridUnitType.Star), new GridLength(0, GridUnitType.Star) } }
                    }
                },*/
                new TargetedSetters
                {
                    Targets = Scroll,
                    Setters =
                    {
                        new Setters { Property = IsVisibleProperty, Values = { true, (BooleanValue<bool>)false } }
                    }
                },
                new TargetedSetters
                {
                    Targets = Right,
                    Setters =
                    {
                        //new Setters { Property = IsVisibleProperty, Values = { false, (BooleanValue<bool>)true } },
                        new Setters
                        {
                            Property = MarginProperty,
                            Values =
                            {
                                new Thunk<Thickness>(() => new Thickness(0, VariablesSize, 0, -RowSpacing)),
                                //new Thunk<Thickness>(() => new Thickness(0, (Measure(5, 5, 5.25 / 4.5, true).Height - Padding.VerticalThickness - 4 * RowSpacing) * 0.5 / 4.5, 0, -RowSpacing)),
                                new Thickness(0)
                            }
                        }
                    }
                },
                new TargetedSetters
                {
                    Targets = { Right.RowDefinitions[0], Right.RowDefinitions[9] },
                    Setters =
                    {
                        new Setters { Property = RowDefinition.HeightProperty, Values = { new GridLength(0, GridUnitType.Star), new GridLength(1, GridUnitType.Star) } }
                    }
                },
            });

            this.AddContinuousTouchListener<TouchEventArgs>(Gesture.Pinch, (sender, e) =>
            {
                if (e.State == TouchState.Up)
                {
                    App.ShowFullKeyboard.Value = !App.ShowFullKeyboard.Value;
                }
            });

            Scroll.SetBounces(false);

            //Keypad.SetBinding<double, double>(WidthRequestProperty, Keypad, "Height", value => PaddedButtonsWidth(Keys[0].Length, ButtonWidth(value, Keys.Length)));
            void UpdateKeypadWidth() => Keypad.WidthRequest = (Scroll.Width - (GetColumnSpan(Scroll) - 1) * Keypad.ColumnSpacing) / GetColumnSpan(Scroll) * Keys[0].Length + (Keys[0].Length - 1) * Keypad.ColumnSpacing;
            Scroll.Bind<double>(WidthProperty, value => UpdateKeypadWidth());
            Scroll.WhenPropertyChanged(ColumnSpanProperty, (sender, e) => UpdateKeypadWidth());

            /*Keypad.WhenPropertyChanged(WidthProperty, (sender, e) =>
            {
                ResetScroll();
            });*/
            Keypad.Children[0].Bind<double>(WidthProperty, value => Scroll.SetPagingInterval(value + Keypad.ColumnSpacing));

            LayoutChanged += HandleLayoutChanged;

            //App.Current.MainPage.SizeChanged += (sender, e) => ScreenSizeChanged();
            App.ShowFullKeyboard.Bind<bool>(App.ShowFullKeyboard.ValueProperty, value => IsCondensed = value);

            Variables.Bind<bool>(GridExtensions.IsTransposedProperty, value =>
            {
                VariableLayout.Orientation = value ? StackOrientation.Vertical : StackOrientation.Horizontal;
                (VariableLayout.Parent as ScrollView).Orientation = value ? ScrollOrientation.Vertical : ScrollOrientation.Horizontal;

                OnVariablesSizeChanged();

                for (int i = 0; i < 3; i += 2)
                {
                    if (value)
                    {
                        Variables.RowDefinitions[i].SetBinding<double, double>(RowDefinition.HeightProperty, Variables, "Width", width => Math.Max(0, width));
                    }
                    else
                    {
                        Variables.ColumnDefinitions[i].SetBinding<double, double>(ColumnDefinition.WidthProperty, Variables, "Height", width => Math.Max(0, width));
                    }
                }
            });

            this.Bind<bool>(IsCondensedProperty, value =>
            {
                int target = (IsCondensed ? Keys[0].Length : 4) + 1;
                int diff = target - RegularGridDefinition.ColumnDefinitions.Count;
                int axis = 4;
                while (RegularGridDefinition.ColumnDefinitions.Count < target)
                {
                    RegularGridDefinition.ColumnDefinitions.Insert(axis, new ColumnDefinition());
                }
                while (RegularGridDefinition.ColumnDefinitions.Count > target)
                {
                    RegularGridDefinition.ColumnDefinitions.RemoveAt(axis);
                }
            });

            this.Bind<StackOrientation>(OrientationProperty, value =>
            {
                bool transposed = value == StackOrientation.Vertical;
                GridExtensions.SetIsTransposed(this, transposed);
                if (Variables.Parent == this)
                {
                    GridExtensions.SetIsTransposed(Variables, transposed);
                }

                GridDefinition definition = transposed ? SidewaysGridDefinition : RegularGridDefinition;
                ColumnDefinitions = definition.ColumnDefinitions;
                RowDefinitions = definition.RowDefinitions;

                ChangeModeButton.IsEnabled = value == StackOrientation.Horizontal;
                
                if (value == StackOrientation.Horizontal)
                {
                    GridExtensions.SetPos(ArrowKeys.Children[0], 0, 0); // Up
                    GridExtensions.SetPos(ArrowKeys.Children[1], 1, 0); // Left
                    GridExtensions.SetPos(ArrowKeys.Children[2], 1, 1); // Right
                    GridExtensions.SetPos(ArrowKeys.Children[3], 3, 0); // Down
                }
                else if (value == StackOrientation.Vertical)
                {
                    GridExtensions.SetPos(ArrowKeys.Children[0], 0, 1);
                    GridExtensions.SetPos(ArrowKeys.Children[1], 0, 0);
                    GridExtensions.SetPos(ArrowKeys.Children[2], 0, 3);
                    GridExtensions.SetPos(ArrowKeys.Children[3], 1, 1);
                }
            });

            DockButton.Remove();
            ChangeModeButton.Remove();

            DescendantAdded -= InitialButtonSetup;
            ChangeMode(true, false);

            OnscreenSizeChanged += (sender, e) => this.SizeRequest(Size.Width - Padding.HorizontalThickness - SafePadding.HorizontalThickness, Size.Height - Padding.VerticalThickness - SafePadding.VerticalThickness);
        }

        protected virtual void OnOnscreenSizeChanged(Size size)
        {
            Size = size;
            OnscreenSizeChanged?.Invoke(this, new EventArgs());
        }

        private Size ScreenSize;
        private Thickness SafePadding;

        public void ScreenSizeChanged(Size screenSize, Thickness safePadding)
        {
            ScreenSize = screenSize;
            SafePadding = safePadding;

            OnVariablesSizeChanged();
            HandleLayoutChanged(null, null);
        }

        public double VariablesSize => Math.Max(0, (Math.Min(MAX_BUTTON_SIZE * (4 + 1.25) + (5 - 1) * ColumnSpacing, GridExtensions.GetIsTransposed(Variables) ? ScreenSize.Height - SafePadding.VerticalThickness : ScreenSize.Width - SafePadding.HorizontalThickness) - (Keys[0].Length + 1) * ColumnSpacing) / (Keys[0].Length + 1.25));

        private void OnVariablesSizeChanged()
        {
            bool transposed = GridExtensions.GetIsTransposed(Variables);
            BindableProperty property = transposed ? ColumnDefinition.WidthProperty : RowDefinition.HeightProperty;

            (transposed ? (BindableObject)SidewaysGridDefinition.ColumnDefinitions[0] : RegularGridDefinition.RowDefinitions[0]).SetValue(property, VariablesSize);
            //(transposed ? (BindableObject)Variables.ColumnDefinitions[0] : Variables.RowDefinitions[0]).SetValue(property, Variables.Parent == this ? new GridLength(1, GridUnitType.Star) : size);
        }

        private void HandleLayoutChanged(object sender, EventArgs e)
        {
            LayoutChanged -= HandleLayoutChanged;

            bool transposed = GridExtensions.GetIsTransposed(this);
            SetColumnSpan(Variables, transposed ? 1 : ColumnDefinitions.Count);
            SetRowSpan(Variables, transposed ? RowDefinitions.Count : 1);

            SetColumnSpan(Scroll, ColumnDefinitions.Count - 1);
            SetRowSpan(Scroll, RowDefinitions.Count - 1);

            if (IsRegular)
            {
                SetColumn(BackspaceButton, transposed ? 1 : (ColumnDefinitions.Count - 1));
                SetRow(BackspaceButton, transposed ? (RowDefinitions.Count - 1) : 1);
                SetColumn(ArrowKeys, transposed ? 2 : (ColumnDefinitions.Count - 1));
                SetRow(ArrowKeys, transposed ? (RowDefinitions.Count - 1) : 2);
                SetColumn(BottomRight, transposed ? 4 : (ColumnDefinitions.Count - 1));
                SetRow(BottomRight, transposed ? (RowDefinitions.Count - 1) : 4);
            }

            ResetScroll();

            if (!this.AnimationIsRunning("changeMode"))
            {
                OnOnscreenSizeChanged(Measure(IsRegular));
            }

            bool collapsed = Size.Width == ScreenSize.Width || Size.Height == ScreenSize.Height;
            /*if (Collapsed != collapsed)
            {
                Collapsed = collapsed;
                OnPropertyChanged("Collapsed");
            }*/

            Orientation = collapsed && IsRegular ? (ScreenSize.Height > ScreenSize.Width ? StackOrientation.Horizontal : StackOrientation.Vertical) : StackOrientation.Horizontal;

            BottomRight.Content = collapsed ? ChangeModeButton : DockButton;

            LayoutChanged += HandleLayoutChanged;
        }

        private Size Measure(bool regularMode, GridDefinition grid = null)
        {
            Size constraint = ScreenSize - new Size(SafePadding.HorizontalThickness, SafePadding.VerticalThickness);
            GridExtensions.AutoSize autoSizeDirection = constraint.Height > constraint.Width ? GridExtensions.AutoSize.Height : GridExtensions.AutoSize.Width;

            ColumnDefinitionCollection columns = grid?.ColumnDefinitions ?? ColumnDefinitions;
            RowDefinitionCollection rows = grid?.RowDefinitions ?? RowDefinitions;

            if (regularMode)
            {
                if (constraint.Height > constraint.Width)
                {
                    constraint.Height /= 2;
                }
                else
                {
                    constraint.Width /= 2;
                }
            }

            Size size = (grid == null ? this : new Grid { ColumnDefinitions = columns, ColumnSpacing = ColumnSpacing, RowDefinitions = rows, RowSpacing = RowSpacing, Padding = Padding, Margin = Margin }).Measure(constraint.Width, constraint.Height, autoSizeDirection).Request;
            size += new Size(SafePadding.HorizontalThickness, SafePadding.VerticalThickness);

            Size fullSize = FullSize(Keys[0].Length + 1, Keys.Length + 1);
            return fullSize.Area() < size.Area() && fullSize.Width < App.Current.MainPage.Width && fullSize.Height < App.Current.MainPage.Height ? FullSize(ColumnDefinitions.Count, RowDefinitions.Count) : size;
        }

        private bool IsRegular;

        public void ChangeMode(bool regular, bool animated = true)
        {
            GridDefinition intermediate = new GridDefinition();
            for (int i = 0; i < 5; i++)
            {
                intermediate.ColumnDefinitions.Add(new ColumnDefinition { Width = i >= ColumnDefinitions.Count ? new GridLength(0, GridUnitType.Star) : ColumnDefinitions[i].Width });
                intermediate.RowDefinitions.Add(new RowDefinition { Height = RowDefinitions[i].Height });
            }
            ColumnDefinitions = intermediate.ColumnDefinitions;
            RowDefinitions = intermediate.RowDefinitions;

            if (!Children.Contains(ArrowKeys))
            {
                //ColumnDefinitions.Add(PermanentKeysColumn);
                //SetColumnSpan(Variables, ColumnDefinitions.Count);
                Children.Add(ArrowKeys, ColumnDefinitions.Count - 1, 2);
                SetRowSpan(ArrowKeys, 2);
            }

            if (regular)
            {
                VisualState basic = this.GetVisualStateByName("Basic");

                Children.Add(BackspaceButton, ColumnDefinitions.Count - 1, 1);
                BackspaceButton.SetValueFromState(MarginProperty, basic);

                Children.Add(BottomRight, ColumnDefinitions.Count - 1, 4);
                BottomRight.SetValueFromState(MarginProperty, basic);
            }
            else
            {
                Right.SetValueFromState(MarginProperty, this.GetVisualStateByName("Regular"));
                Right.IsVisible = true;

                for (int i = 0; i < Keys.Length; i++)
                {
                    int j = Keys[i].Length - 4;
                    for (; j < Keys[i].Length - 1; j++)
                    {
                        Children.Add(Keys[i][j], j - (Keys[i].Length - 4), i + 1);
                    }

                    Right.Children.Add(Keys[i][j], 0, 1, i * 2 + 1, i * 2 + 3);
                }
            }

            /*string[] modes = new string[] { "Regular", "Basic" };
            if (regular)
            {
                Misc.Swap(ref modes[0], ref modes[1]);
            }*/
            
            void Finished()
            {
                if (regular)
                {
                    IsRegular = true;

                    for (int i = 0; i < Keys.Length; i++)
                    {
                        int j = Keys[i].Length - 4;
                        for (; j < Keys[i].Length - 1; j++)
                        {
                            Keypad.Children.Add(Keys[i][j], j, i);
                        }

                        Keypad.Children.Add(Keys[i][j], j, i);
                    }

                    Right.IsVisible = false;
                }
                else
                {
                    Right.Children.Add(BottomRight, 0, 9);
                    BottomRight.ClearValue(MarginProperty);

                    Right.Children.Add(BackspaceButton, 0, 0);
                    BackspaceButton.ClearValue(MarginProperty);

                    //ColumnDefinitions.Remove(PermanentKeysColumn);
                    //SetColumnSpan(Variables, ColumnDefinitions.Count);
                    ArrowKeys.Remove();

                    //ScreenSizeChanged();
                }

                if (Parent != null)
                {
                    AbsoluteLayoutExtensions.SetLayout(Parent, new Size(-1, -1));
                }
            }

            IsRegular = false;

            double finalHeight = Measure(regular, regular ? RegularGridDefinition : BasicGridDefinition).Height;
            
            Animation changeHeight = new Animation(value => OnOnscreenSizeChanged(new Size(Size.Width, value)), Size.Height, finalHeight);
            Animation animation = this.AnimationToState(end: regular ? "Regular" : "Basic", callback: value =>
            {
                //PERMANENT_KEYS_INCREASE = (regular ? value : (1 - value)) * 1.25;
                //ScreenSizeChanged();

                //BackspaceButton.TranslationX = value == 1 ? 0 : ArrowKeys.Width + Spacing;
                //DockButton.TranslationX = value == 1 ? 0 : ArrowKeys.Width + Spacing;

                ArrowKeys.Margin = new Thickness(0, -BackspaceButton.Margin.Bottom, 0, -BottomRight.Margin.Top);

                if (Parent != null)
                {
                    AbsoluteLayoutExtensions.SetLayout(Parent, Size - new Size(SafePadding.HorizontalThickness, 0));
                    //AbsoluteLayout.SetLayoutBounds(Parent, new Rectangle(Point.Zero, value == 1 ? new Size(-1, -1) : Size));
                }
            });

            if (animated)
            {
                Animation columns = new Animation(value =>
                {
                    intermediate.ColumnDefinitions.Last().Width = new GridLength(value, GridUnitType.Star);
                }, intermediate.ColumnDefinitions.Last().Width.Value, regular ? 1.25 : 0);
                animation.Add(0, 1, columns);

                //double finalVariables = regular ? VariablesSize / (finalHeight - Padding.VerticalThickness - (RegularGridDefinition.RowDefinitions.Count - 1) * RowSpacing) : (Width - Padding.HorizontalThickness - (BasicGridDefinition.ColumnDefintions.Count - 1) * ColumnSpacing) / BasicGridDefinition.ColumnDefintions.Count;
                double absolute;
                double proportional;
                BasicGridDefinition.RowDefinitions.DeconstructRows(RowSpacing, out absolute, out proportional);
                double basicVariablesHeight = ((regular ? Height : (finalHeight - SafePadding.VerticalThickness)) - absolute) / proportional;

                Animation variableSize = new Animation(value => intermediate.RowDefinitions[0].Height = value, regular ? basicVariablesHeight : VariablesSize, regular ? VariablesSize : basicVariablesHeight);
                animation.Add(0, 1, variableSize);

                animation.Add(0, 1, changeHeight);
                animation.Commit(this, "changeMode", length: MainPage.ModeTransitionLength, easing: Easing.Linear, finished: (final, cancelled) => Finished());
            }
            else
            {
                animation.GetCallback()(1);
                changeHeight.GetCallback()(finalHeight);
                Finished();
            }
        }

        /*private Thickness SafeArea;

        public void SafeAreaChanged(Thickness value)
        {
            SafeArea = value;
            ScreenSizeChanged();
        }*/

        public bool Collapsed { get; private set; }

        /*public Size MeasureRegularMode() => Measure(5, 5, 5.25 / 4.5, true);

        public Size MeasureBasicMode() => Measure(5, 4, 4 / 5.0, false);

        private Size Measure(int rows, int columns, double ratio, bool regularMode)
        {
            double widthConstraint = App.Current.MainPage.Width;
            double heightConstraint = App.Current.MainPage.Height;

            Size extra = new Size(Padding.HorizontalThickness + (columns - 1) * Spacing, Padding.VerticalThickness + (rows - 1) * Spacing);
            Size size = new Size(widthConstraint - extra.Width, heightConstraint - extra.Height);

            if (size.Width / size.Height > ratio)
            {
                size.Width = size.Height * ratio;
            }
            else
            {
                size.Height = size.Width / ratio;
            }

            StackOrientation orientation = widthConstraint > heightConstraint ? StackOrientation.Vertical : StackOrientation.Horizontal;

            if (regularMode)
            {
                if (orientation == StackOrientation.Horizontal)
                {
                    size.Height = Math.Min(size.Height, App.Current.MainPage.Bounds.Height / 2 - extra.Height);
                }
                else if (orientation == StackOrientation.Vertical)
                {
                    size.Width = Math.Min(size.Width, App.Current.MainPage.Bounds.Width / 2 - extra.Width);
                }

                Size full = new Size(columns * MAX_BUTTON_SIZE, rows * MAX_BUTTON_SIZE);
                if (full.Area() < size.Area())
                {
                    size = full;
                    orientation = StackOrientation.Horizontal;
                }
            }

            SetValue(OrientationProperty, orientation);

            return new Size(size.Width + extra.Width, size.Height + extra.Height);
        }*/

        private void ScreenSizeChanged()
        {
            //Size bounds = App.Current.MainPage.Bounds.Size;
            //bounds = new Size(bounds.Width - SafeArea.HorizontalThickness, bounds.Height - SafeArea.VerticalThickness);
            //Print.Log("screen size changed", SafeArea.UsefulToString(), bounds);
            //CondensedOrientation = bounds.Height >= bounds.Width ? StackOrientation.Horizontal : StackOrientation.Vertical;

            //Size size = MeasureOnscreenSize(bounds.Width, bounds.Height);

            //bool isCondensed = !FullSize.Equals(size);
            //bool collapsed = size.Width == bounds.Width || size.Height == bounds.Height;
            //SetValue(OrientationProperty, (collapsed ? CondensedOrientation : StackOrientation.Horizontal));
            //Main.Orientation = collapsed ? CondensedOrientation : StackOrientation.Horizontal;
            //base.Orientation = Main.Orientation.Invert();
            //IsCondensed = isCondensed;

            //AbsoluteLayout root = this.Root<AbsoluteLayout>();

            //Size = !collapsed ? size : (Orientation == StackOrientation.Horizontal ? new Size(size.Width + SafeArea.HorizontalThickness, size.Height + SafeArea.Bottom) : new Size(size.Width + SafeArea.Right, size.Height + SafeArea.VerticalThickness));

            //Print.Log("requesting " + size);
            //this.SizeRequest(new Size(size.Width, size.Height + ButtonSize + Spacing));
            //WidthRequest = size.Width;
            /*if (Orientation == StackOrientation.Horizontal)
            {
                double columns = ColumnDefinitions.Count - (PermanentKeysColumn.Width.Value == 0 ? 1 : 0);
                size.Height = ((size.Width - (ColumnDefinitions.Count - 1) * Spacing) / (4 + PermanentKeysColumn.Width.Value)) * (4 + RowDefinitions[0].Height.Value) + (RowDefinitions.Count - 1) * Spacing;
                //size.Height = PaddedButtonsWidth(Keys.Length + VariablesRow.Height.Value, ButtonWidth(size.Width, MIN_COLUMNS + PermanentKeysColumn.Width.Value));// + Spacing;
            }
            else if (Orientation == StackOrientation.Vertical)
            {
                size.Width = PaddedButtonsWidth(MIN_COLUMNS + VariablesRow.Height.Value, ButtonWidth(size.Height, MIN_COLUMNS + 1)) + Spacing;
            }*/

            //HeightRequest = size.Height + Spacing + (TopRight.Height == -1 ? 33 : TopRight.Height);// RowDefinitions[0].Height.Value;
            //RowDefinitions[1].Height = size.Height;

            //Print.Log("display size changed", App.Current.MainPage.Bounds.Size, size, Orientation);

            //DockButton.Text = collapsed ? "↔" : "\u25BD"; //white down-pointing triangle
            //DockButton.IsEnabled = !collapsed;

            /*if (Collapsed != collapsed)
            {
                Collapsed = collapsed;
                OnPropertyChanged("Collapsed");
            }*/
        }

        /*public Size MeasureOnscreenSize(double width, double height)
        {
            //Orientation = height >= width ? StackOrientation.Horizontal : StackOrientation.Vertical;

            Size size = GetSize(width, height);// OnMeasure(width, height).Request;
            return size;
            bool collapsed = !size.Equals(FullSize);

            if (collapsed)
            {
                if (height >= width)
                {
                    size.Height = Math.Min(size.Height, height / 2);
                }
                else
                {
                    size.Width = Math.Min(size.Width, width / 2);
                }
            }
            /*else
            {
                Orientation = StackOrientation.Horizontal;
            }

            return size;
        }*/

        private void ResetScroll() => Scroll.ScrollToAsync(Keypad, ScrollToPosition.End, false);

        public void Enable(bool animated = false) => IsVisible = true;// SetEnabled(true, animated);

        public void Disable(bool animated = false) => IsVisible = false;// SetEnabled(false, animated);

        private void SetEnabled(bool value, bool animated = false)
        {
            if (animated)
            {
                this.Animate("Enable", new PropertyAnimation(this, VisualElementExtensions.VisibilityProperty, value.ToInt()), length: 500);
            }
            else
            {
                this.SetVisibility(value.ToInt());
            }
        }

        public IEnumerator<Key> GetEnumerator()
        {
            foreach (Key key in this.GetDescendants<Key>())
            {
                yield return key;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /*protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName.IsProperty(WidthProperty) || propertyName.IsProperty(HeightProperty))
            {
                if (this.AnimationIsRunning("changeMode"))
                {
                    //return;
                }
                //IsCondensed = !FullSize.Equals(Bounds.Size);
                //Orientation = Height >= Width ? StackOrientation.Horizontal : StackOrientation.Vertical;

                //PermanentKeys.ColumnDefinitions = new ColumnDefinitionCollection();
                //PermanentKeys.RowDefinitions = new RowDefinitionCollection();

                //PermanentKeys.ColumnDefinitions[0].ClearValue(ColumnDefinition.WidthProperty);
                //PermanentKeys.RowDefinitions[0].ClearValue(RowDefinition.HeightProperty);
                
                double buttonSize = 0;
                if (Orientation == StackOrientation.Horizontal && Width > 0)
                {
                    buttonSize = Math.Max(0, ButtonWidth(Width - Padding.HorizontalThickness, (IsCondensed ? MIN_COLUMNS : Keys[0].Length) + PERMANENT_KEYS_INCREASE));
                    //PermanentKeys.ColumnDefinitions[0].Width = buttonSize * PERMANENT_KEYS_INCREASE;
                    //AbsoluteLayoutExtensions.SetLayoutSize(DockButton, new Size(buttonSize * 1.25, buttonSize));
                    //PermanentKeys.ColumnDefinitions.Add(new ColumnDefinition { Width = buttonSize * PERMANENT_KEYS_INCREASE });
                }
                else if (Orientation == StackOrientation.Vertical && Height > 0)
                {
                    buttonSize = Math.Max(0, ButtonWidth(Height - Padding.VerticalThickness, Keys.Length + 1));
                    //PermanentKeys.RowDefinitions[0].Height = buttonSize;
                    //PermanentKeys.RowDefinitions.Add(new RowDefinition { Height = buttonSize });
                }
            }
        }*/

        /*public Size DesiredSize;

        private Size GetSize(double widthConstraint, double heightConstraint)
        {
            widthConstraint -= Padding.HorizontalThickness;
            heightConstraint -= Padding.VerticalThickness;

            bool idealOrientation = CondensedOrientation == StackOrientation.Horizontal;
            double rows = Keys.Length + (!idealOrientation).ToInt();
            MIN_COLUMNS = App.ShowFullKeyboard.Value && idealOrientation ? Keys[0].Length : 4;
            double cols = MIN_COLUMNS + idealOrientation.ToInt() * PERMANENT_KEYS_INCREASE;

            // Determine if the condensed size is smaller if we make it as wide as possible or a tall as possible. Could probably be more efficient by comparing ratio of width / height to cols / rows
            Size fitWidth = new Size(widthConstraint, Math.Min(heightConstraint, PaddedButtonsWidth(rows, ButtonWidth(widthConstraint, cols))));
            Size fitHeight = new Size(Math.Min(widthConstraint, PaddedButtonsWidth(cols, ButtonWidth(heightConstraint, rows))), heightConstraint);
            Size condensed = fitWidth.Area() < fitHeight.Area() ? fitWidth : fitHeight;

            //Print.Log("measuring", widthConstraint, heightConstraint, condensed, App.ShowFullKeyboard.Value);
            Size fullSize = App.ShowFullKeyboard.Value ? FullSize : new Size(PaddedButtonsWidth(MIN_COLUMNS + PERMANENT_KEYS_INCREASE, MAX_BUTTON_SIZE), PaddedButtonsWidth(Keys.Length, MAX_BUTTON_SIZE));
            return FullSize.Width < widthConstraint && FullSize.Height < heightConstraint && FullSize.Area() < condensed.Area() ? fullSize : condensed;

            //return sr;
        }*/

        public double PaddedButtonsWidth(double numButtons, double buttonSize) => buttonSize * numButtons + Spacing * ((int)numButtons - 1);

        public double ButtonWidth(double spaceConstraint, double numButtons) => (spaceConstraint - Spacing * ((int)numButtons - 1)) / numButtons;
    }
}