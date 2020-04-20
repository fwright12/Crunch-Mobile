using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Calculator
{
    /*public class OrientedGrid : Grid
    {
        public static readonly BindableProperty OrientationProperty = BindableProperty.Create(StackLayout.OrientationProperty.PropertyName, StackLayout.OrientationProperty.ReturnType, typeof(OrientedGrid), StackLayout.OrientationProperty.DefaultValue, StackLayout.OrientationProperty.DefaultBindingMode);

        public bool Orientation
        {
            get { return (bool)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName != OrientationProperty.PropertyName)
            {
                return;
            }
        }
    }*/

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CrunchKeyboard : Grid, IEnumerable<Key>, ISoftKeyboard
    {
        public event KeystrokeEventHandler Typed;
        public event EventHandler OnscreenSizeChanged;

        public bool IsCondensed { get; private set; }

        public double Spacing { get; set; } = 6;

        public static readonly BindableProperty OrientationProperty = BindableProperty.Create(nameof(Orientation), typeof(StackOrientation), typeof(CrunchKeyboard), StackOrientation.Horizontal,
            propertyChanged: (bindable, oldvalue, newvalue) => ((CrunchKeyboard)bindable).InvalidateLayout());

        public StackOrientation Orientation
        {
            get => (StackOrientation)GetValue(OrientationProperty);
            set
            {
                CondensedOrientation = value;
                if (IsCondensed)
                {
                    InvalidateMeasure();
                }
            }
        }
        private StackOrientation CondensedOrientation;

        private readonly Key EXP = new Key { Text = "x\u207F", Basic = "^" };
        private readonly Key SQRD = new Key { Text = "x\u00B2", Basic = "^2" };
        private readonly Key DIV = new Key { Text = "\u00F7", Basic = "/" };
        private readonly Key LOG = new Key { Text = "log", Basic = "log_" };
        private readonly Key SQRT = new Key { Text = "\u221A", Basic = "√" };// { FontFamily = CrunchStyle.SYMBOLA_FONT };
        private readonly Key MULT = new Key { Text = "\u00D7", Basic = "*" };
        private readonly Key TEN = new Key { Text = "10\u207F", Basic = "10^" };
        private readonly Key PI = new Key { Text = "\u03C0" };

        private readonly View[][] Keys;

        private RowDefinition VariablesRow = new RowDefinition { Height = new GridLength(0.5, GridUnitType.Star) };
        private ColumnDefinition PermanentKeysColumn = new ColumnDefinition { Width = new GridLength(1.25, GridUnitType.Star) };

        private readonly int MAX_BUTTON_SIZE = 75;
        private double PERMANENT_KEYS_INCREASE = 1.25;
        private double MIN_COLUMNS = 4;
        private readonly int RecentlyUsed = 10;

        //public int Rows => Keys.Length + this.Orient(0, 1);
        //public double MaxColumns => Keys[0].Length + this.Orient(PERMANENT_KEYS_INCREASE, 0);
        //public double Columns => this.Orient((IsCondensed ? MIN_COLUMNS : Keys[0].Length) + PERMANENT_KEYS_INCREASE, MIN_COLUMNS);
        //public bool ShowingFullKeyboard => !App.IsCondensed && Settings.ShouldShowFullKeyboard;

        public Size FullSize => new Size(
            PaddedButtonsWidth(Keys[0].Length + PERMANENT_KEYS_INCREASE, MAX_BUTTON_SIZE),
            PaddedButtonsWidth(Keys.Length, MAX_BUTTON_SIZE)
            );

        public Size Size { get; private set; }
        public double ButtonSize { get; set; }

        public CrunchKeyboard()
        {
            Resources = new ResourceDictionary();
            // Make spacing consistent between all buttons
            this.WhenDescendantAdded<Grid>((grid) =>
            {
                grid.BindingContext = this;
                grid.SetBinding(Grid.RowSpacingProperty, "RowSpacing");
                grid.SetBinding(Grid.ColumnSpacingProperty, "ColumnSpacing");
            });

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

            InitializeComponent();

            Grid parentheses = new Grid
            {
                Children =
                {
                    { new Key { Text = "(" }, 0, 0 },
                    { new Key { Text = ")" }, 1, 0 }
                }
            };

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
            VariableLayout.SetBinding(StackLayout.OrientationProperty, this, "Orientation");
            
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

                    Keypad.Children.Add(view, j, i);
                }
            }
            
            VisualStateManager.GetVisualStateGroups(this).Add(new VisualStates("Regular", "Basic")
            {
                new Setters
                {
                    Property = HeightRequestProperty,
                    Values =
                    {
                        new Thunk<double>(() => MeasureRegularMode().Height - Padding.VerticalThickness),
                        new Thunk<double>(() => MeasureBasicMode().Height - Padding.VerticalThickness)
                    }
                },
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
                    Targets = ChangeModeButton,
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
                    Targets = { parentheses, VariableLayout, NextKeyboardButton, ExpandButton },
                    Setters =
                    {
                        new Setters { Property = VisualElementExtensions.VisibilityProperty, Values = { 1, 0 } },
                    }
                },
                new TargetedSetters
                {
                    Targets = VariablesRow,
                    Setters =
                    {
                        new Setters { Property = RowDefinition.HeightProperty, Values = { new GridLength(0.5, GridUnitType.Star), new GridLength(1, GridUnitType.Star) } }
                    }
                },
                new TargetedSetters
                {
                    Targets = PermanentKeysColumn,
                    Setters =
                    {
                        new Setters { Property = ColumnDefinition.WidthProperty, Values = { new GridLength(1.25, GridUnitType.Star), new GridLength(0, GridUnitType.Star) } }
                    }
                },
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
                                new Thunk<Thickness>(() => new Thickness(0, (Measure(5, 5, 5.25 / 4.5, true).Height - Padding.VerticalThickness - 4 * RowSpacing) * 0.5 / 4.5, 0, -RowSpacing)),
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

            DockButton.Remove();
            Scroll.SetBounces(false);

            //Keypad.SetBinding<double, double>(WidthRequestProperty, Keypad, "Height", value => PaddedButtonsWidth(Keys[0].Length, ButtonWidth(value, Keys.Length)));
            void UpdateKeypadWidth() => Keypad.WidthRequest = (Scroll.Width - (GetColumnSpan(Scroll) - 1) * Keypad.ColumnSpacing) / GetColumnSpan(Scroll) * Keys[0].Length + (Keys[0].Length - 1) * Keypad.ColumnSpacing;
            Scroll.Bind<double>(WidthProperty, value => UpdateKeypadWidth());
            /*Keypad.WhenPropertyChanged(WidthProperty, (sender, e) =>
            {
                ResetScroll();
            });*/
            Keypad.Children[0].Bind<double>(WidthProperty, value => Scroll.SetPagingInterval(value + Keypad.ColumnSpacing));

            LayoutChanged += HandleLayoutChanged;

            //App.Current.MainPage.SizeChanged += (sender, e) => ScreenSizeChanged();
            App.ShowFullKeyboard.WhenPropertyChanged(App.ShowFullKeyboard.ValueProperty, (sender, e) =>
            {
                int target = (App.ShowFullKeyboard.Value ? Keys[0].Length : 4) + 1;
                int diff = target - ColumnDefinitions.Count;
                int axis = 4;
                while (ColumnDefinitions.Count < target)
                {
                    ColumnDefinitions.Insert(axis, new ColumnDefinition());
                }
                while (ColumnDefinitions.Count > target)
                {
                    ColumnDefinitions.RemoveAt(axis);
                }

                foreach (View view in Children)
                {
                    if (view == Scroll)
                    {
                        SetColumnSpan(view, GetColumnSpan(view) + diff);
                    }
                    else if (view != Variables)
                    {
                        SetColumn(view, GetColumn(view) + diff);
                    }
                }

                UpdateKeypadWidth();
            });

            /*this.WhenPropertyChanged(OrientationProperty, (sender, e) =>
            {
                this.Transpose();
                Variables.Transpose();
            });*/
            
            this.Bind<StackOrientation>(OrientationProperty, value =>
            {
                bool transposed = value == StackOrientation.Vertical;
                GridExtensions.SetIsTransposed(this, transposed);
                if (Variables.Parent == this)
                {
                    GridExtensions.SetIsTransposed(Variables, transposed);
                }

                if (value == StackOrientation.Horizontal)
                {
                    RowDefinitions[0] = VariablesRow;
                    ColumnDefinitions[4] = PermanentKeysColumn;
                }
                else if (value == StackOrientation.Vertical)
                {
                    RowDefinitions[4].Height = new GridLength(1, GridUnitType.Star);
                }

                /*foreach (View child in Variables.Children)
                {
                    child.RemoveBinding(WidthRequestProperty);
                    child.RemoveBinding(HeightRequestProperty);
                    if (value == StackOrientation.Horizontal)
                    {
                        child.SetBinding(WidthRequestProperty,)
                    }
                }*/

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
            
            Variables.Bind<bool>(GridExtensions.IsTransposedProperty, value =>
            {
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
                    //Right.RowDefinitions[i].SetBinding<double, double>(RowDefinition.HeightProperty, DIV, "Height", value => Math.Max(0, (value - Spacing) / 2));
                }
            });

            DescendantAdded -= InitialButtonSetup;
            ChangeMode(true, false);
        }

        private void HandleLayoutChanged(object sender, EventArgs e)
        {
            SetColumnSpan(Variables, ColumnDefinitions.Count);

            Size size = Measure(RowDefinitions.Count, ColumnDefinitions.Count, ColumnDefinitions.Sum(column => column.Width.Value) / RowDefinitions.Sum(row => row.Height.Value), PermanentKeysColumn.Width.Value == 1.25);

            WidthRequest = size.Width - Padding.HorizontalThickness;
            if (!this.AnimationIsRunning("changeMode"))
            {
                HeightRequest = size.Height - Padding.VerticalThickness;
            }
            //this.SizeRequest(new Size(size.Width - Padding.HorizontalThickness, size.Height - Padding.VerticalThickness));

            ResetScroll();

            Size = new Size(WidthRequest + Padding.HorizontalThickness, HeightRequest + Padding.VerticalThickness);
            OnscreenSizeChanged?.Invoke(this, new EventArgs());

            bool collapsed = size.Width == App.Current.MainPage.Width || size.Height == App.Current.MainPage.Height;
            if (Collapsed != collapsed)
            {
                Collapsed = collapsed;
                OnPropertyChanged("Collapsed");
            }
        }

        public void ChangeMode(bool regular, bool animated = true)
        {
            if (!ColumnDefinitions.Contains(PermanentKeysColumn))
            {
                ColumnDefinitions.Add(PermanentKeysColumn);
                //SetColumnSpan(Variables, ColumnDefinitions.Count);
                Children.Add(ArrowKeys, ColumnDefinitions.Count - 1, 2);
                SetRowSpan(ArrowKeys, 2);
            }

            if (regular)
            {
                VisualState basic = this.GetVisualStateByName("Basic");

                Children.Add(BackspaceButton, ColumnDefinitions.Count - 1, 1);
                BackspaceButton.SetValueFromState(MarginProperty, basic);

                Children.Add(ChangeModeButton, ColumnDefinitions.Count - 1, 4);
                ChangeModeButton.SetValueFromState(MarginProperty, basic);
            }
            else
            {
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
                    Right.Children.Add(ChangeModeButton, 0, 9);
                    ChangeModeButton.ClearValue(MarginProperty);

                    Right.Children.Add(BackspaceButton, 0, 0);
                    BackspaceButton.ClearValue(MarginProperty);

                    ColumnDefinitions.Remove(PermanentKeysColumn);
                    //SetColumnSpan(Variables, ColumnDefinitions.Count);
                    ArrowKeys.Remove();

                    //ScreenSizeChanged();
                }

                if (Parent != null)
                {
                    AbsoluteLayoutExtensions.SetLayout(Parent, new Size(-1, -1));
                }
            }
            
            Animation animation = this.AnimationToState(end: regular ? "Regular" : "Basic", callback: value =>
            {
                //PERMANENT_KEYS_INCREASE = (regular ? value : (1 - value)) * 1.25;
                //ScreenSizeChanged();

                //BackspaceButton.TranslationX = value == 1 ? 0 : ArrowKeys.Width + Spacing;
                //DockButton.TranslationX = value == 1 ? 0 : ArrowKeys.Width + Spacing;

                ArrowKeys.Margin = new Thickness(0, -BackspaceButton.Margin.Bottom, 0, -ChangeModeButton.Margin.Top);

                if (Parent != null)
                {
                    AbsoluteLayoutExtensions.SetLayout(Parent, Size);
                    //AbsoluteLayout.SetLayoutBounds(Parent, new Rectangle(Point.Zero, value == 1 ? new Size(-1, -1) : Size));
                }
            });

            if (animated)
            {
                animation.Commit(this, "changeMode", length: MainPage.ModeTransitionLength, easing: Easing.Linear, finished: (final, cancelled) => Finished());
            }
            else
            {
                animation.GetCallback()(1);
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

        public Size MeasureRegularMode() => Measure(5, 5, 5.25 / 4.5, true);

        public Size MeasureBasicMode() => Measure(5, 4, 4 / 5.0, false);

        private Size Measure(int rows, int columns, double ratio, bool regularMode)
        {
            double widthConstraint = App.Current.MainPage.Width - 0;
            double heightConstraint = App.Current.MainPage.Height - 0;

            SetValue(OrientationProperty, widthConstraint > heightConstraint ? StackOrientation.Vertical : StackOrientation.Horizontal);

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

            size = new Size(size.Width + extra.Width, size.Height + extra.Height);

            if (regularMode)
            {
                if (FullSize.Area() < size.Area())
                {
                    size = FullSize;
                }
                else if (Orientation == StackOrientation.Horizontal)
                {
                    size.Height = Math.Min(size.Height, App.Current.MainPage.Bounds.Height / 2);
                }
                else if (Orientation == StackOrientation.Vertical)
                {
                    size.Width = Math.Min(size.Width, App.Current.MainPage.Bounds.Width / 2);
                }
            }

            return size;
        }

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

        public void Enable() => IsVisible = true;

        public void Disable() => IsVisible = false;

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