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

        private readonly Key[][] Keys;

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
                grid.SetBinding(Grid.RowSpacingProperty, "Spacing");
                grid.SetBinding(Grid.ColumnSpacingProperty, "Spacing");
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

            View parentheses = null;
            //equals.SetBinding<bool, double>(IsVisibleProperty, equals, "Opacity", value => value > 0);
            Keys = new Key[][]
            {
                //new Key[] { null,  null,    null,   null,   null,   null,   null },
                new Key[] { "sin",  TEN,    EXP,    "7",    "8",    "9",    DIV },
                new Key[] { "cos",  LOG,    SQRD,   "4",    "5",    "6",    MULT },
                new Key[] { "tan",  "ln",   SQRT,    "1",    "2",    "3",    "-" },
                new Key[] { PI,     "e",    "x",    "0",    ".",    "()",   "+" }
            };

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

            bool regular = true;
            DockButton.Clicked += (sender, e) => HandleModeChanged(regular = !regular);

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

                //VariableLayout.Children.Add(button);
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

                    if (Keys[i][j].Text == "()")
                    {
                        Grid grid = new Grid { };

                        grid.Children.Add(new Key { Text = "(" }, 0, 0);
                        grid.Children.Add(new Key { Text = ")" }, 1, 0);

                        view = parentheses = grid;
                        double InverseOpacity(double value) => 1 - value;
                        view.SetBinding<double, double>(OpacityProperty, EqualsKey, "Opacity", InverseOpacity, InverseOpacity, BindingMode.TwoWay);
                    }
                    else
                    {
                        if (Keys[i].Length - j > MIN_COLUMNS)
                        {
                            //Keys[i][j].Clicked += (sender, e) => Scroll.ScrollToAsync(Keypad, ScrollToPosition.End, false);
                        }
                    }

                    Keypad.Children.Add(view, j, i);
                }
            }
            Keypad.Children.Add(EqualsKey, Grid.GetColumn(parentheses), Grid.GetRow(parentheses));
            
            VisualStateManager.GetVisualStateGroups(this).Add(new VisualStates("Regular", "Basic")
            {
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
                                new Thunk<Thickness>(() => new Thickness((1 - 1.25) * ButtonWidth(Width, 5.25), Spacing, 0, -ButtonWidth(Width, 5.25) - Spacing)),
                                /*new Thunk<Thickness>(() =>
                                {
                                    double size = ButtonWidth(Width, 4 + PermanentKeysColumn.Width.Value);
                                    return new Thickness((1 - PermanentKeysColumn.Width.Value) * size, Spacing, 0, -size - Spacing);
                                }),*/
                                new Thickness(0),
                            }
                        }
                    }
                },
                new TargetedSetters
                {
                    Targets = ArrowKeys,
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
                                    double finalSize = ButtonWidth(Width, 8);
                                    return new Thickness(0, -3 * finalSize - 3 * Spacing, 0, -finalSize - Spacing);
                                }),
                            }
                        }
                    }
                },
                new TargetedSetters
                {
                    Targets = DockButton,
                    Setters =
                    {
                        new Setters
                        {
                            Property = MarginProperty,
                            Values =
                            {
                                new Thunk<Thickness>(() => new Thickness((1 - 1.25) * ButtonWidth(Width, 5.25), -ButtonWidth(Width, 5.25) - Spacing, 0, Spacing)),
                                new Thickness(0),
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
                    Targets =
                    {
                        Right.RowDefinitions[0],
                        Right.RowDefinitions[9],
                    },
                    Setters =
                    {
                        new Setters { Property = RowDefinition.HeightProperty, Values = { new GridLength(0, GridUnitType.Star), new GridLength(1, GridUnitType.Star) } }
                    }
                },
                new TargetedSetters
                {
                    Targets = Right,
                    Setters =
                    {
                        new Setters
                        {
                            Property = MarginProperty,
                            Values =
                            {
                                new Thunk<Thickness>(() => new Thickness(0, (1 - 0.5) * ButtonWidth(Width, 4 + 1.25) + (RowDefinitions.Count - ColumnDefinitions.Count) * Spacing, 0, -Spacing)),
                                new Thickness(0)
                            }
                        }
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

            Keypad.SetBinding<double, double>(WidthRequestProperty, Keypad, "Height", value => PaddedButtonsWidth(Keys[0].Length, ButtonWidth(value, Keys.Length)));
            Keypad.WhenPropertyChanged(WidthProperty, (sender, e) =>
            {
                ResetScroll();
            });
            Keypad.Children[0].Bind<double>(WidthProperty, value => Scroll.SetPagingInterval(value + Keypad.ColumnSpacing));

            LayoutChanged += (sender, e) => ScreenSizeChanged();
            //App.Current.MainPage.SizeChanged += (sender, e) => ScreenSizeChanged();
            //App.ShowFullKeyboard.WhenPropertyChanged(App.ShowFullKeyboard.ValueProperty, (sender, e) => ScreenSizeChanged());

            this.WhenPropertyChanged(OrientationProperty, (sender, e) =>
            {
                this.Transpose();
                Variables.Transpose();
            });

            this.Bind<StackOrientation>(OrientationProperty, value =>
            {
                //value = value.Invert();

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

                for (int i = 0; i < 3; i += 2)
                {
                    if (value == StackOrientation.Horizontal)
                    {
                        Variables.ColumnDefinitions[i].SetBinding<double, double>(ColumnDefinition.WidthProperty, Variables, "Height", width => Math.Max(0, width));
                    }
                    else if (value == StackOrientation.Vertical)
                    {
                        Variables.RowDefinitions[i].SetBinding<double, double>(RowDefinition.HeightProperty, Variables, "Width", width => Math.Max(0, width));
                    }
                    //Right.RowDefinitions[i].SetBinding<double, double>(RowDefinition.HeightProperty, DIV, "Height", value => Math.Max(0, (value - Spacing) / 2));
                }

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

            DescendantAdded -= InitialButtonSetup;
            HandleModeChanged(true, false);
        }

        private void HandleModeChanged(bool regular = true, bool animated = true)
        {
            Scroll.ScrollToAsync(Keypad, ScrollToPosition.End, false);
            foreach (View view in Keypad.Children)
            {
                if (Grid.GetColumn(view) < Keys[0].Length - MIN_COLUMNS)
                {
                    //view.Remove();
                    //view.IsVisible = false;
                }
            }

            //ClearValue(MarginProperty);

            if (ColumnDefinitions.Count < 5)
            {
                ColumnDefinitions.Add(PermanentKeysColumn);
                SetColumnSpan(Variables, 5);
                Children.Add(ArrowKeys, 4, 5, 2, 4);
            }

            if (!regular)
            {
                for (int i = 0; i < 4; i++)
                {
                    Right.Children.Add(Keys[i][Keys[0].Length - 1], 0, 1, i * 2 + 1, i * 2 + 3);
                }
                Right.Children.Add(BackspaceButton, 0, 0);
                Right.Children.Add(DockButton, 0, 9);
                Right.IsVisible = true;
            }

            string[] modes = new string[] { "Regular", "Basic" };
            if (regular)
            {
                Misc.Swap(ref modes[0], ref modes[1]);
            }
            
            void Finished()
            {
                if (regular)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        Keypad.Children.Add(Keys[i][Keys[0].Length - 1], Keys[0].Length - 1, i);
                    }
                    Children.Add(BackspaceButton, 4, 1);
                    Children.Add(DockButton, 4, 4);
                    Right.IsVisible = false;

                    BackspaceButton.ClearValue(MarginProperty);
                    DockButton.ClearValue(MarginProperty);
                }
                else
                {
                    Grid.SetColumnSpan(Variables, 4);
                    ColumnDefinitions.Remove(PermanentKeysColumn);
                    ArrowKeys.Remove();

                    //ScreenSizeChanged();
                }
            }

            Animation animation = this.AnimationToState(modes[0], modes[1], callback: value =>
            {
                //PERMANENT_KEYS_INCREASE = (regular ? value : (1 - value)) * 1.25;
                //ScreenSizeChanged();

                BackspaceButton.TranslationX = value == 1 ? 0 : ArrowKeys.Width + Spacing;
                DockButton.TranslationX = value == 1 ? 0 : ArrowKeys.Width + Spacing;
            });

            if (animated)
            {
                animation.Commit(this, "changeMode", length: 5000, easing: Easing.Linear, finished: (final, cancelled) => Finished());
            }
            else
            {
                animation.GetCallback()(1);
                Finished();
            }
        }

        private Thickness SafeArea;

        public void SafeAreaChanged(Thickness value)
        {
            SafeArea = value;
            ScreenSizeChanged();
        }

        public bool Collapsed { get; private set; }

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

            SetValue(OrientationProperty, App.Current.MainPage.Width > App.Current.MainPage.Height ? StackOrientation.Vertical : StackOrientation.Horizontal);
            
            Size extra = new Size(Margin.HorizontalThickness + Padding.HorizontalThickness + (ColumnDefinitions.Count - 1) * Spacing, Margin.VerticalThickness + Padding.VerticalThickness + (RowDefinitions.Count - 1) * Spacing);
            Size size = new Size(App.Current.MainPage.Width - extra.Width, App.Current.MainPage.Height - extra.Height);
            double ratio = ColumnDefinitions.Sum(column => column.Width.Value) / RowDefinitions.Sum(row => row.Height.Value);
            
            if (size.Width / size.Height > ratio)
            {
                size.Width = size.Height * ratio;
            }
            else
            {
                size.Height = size.Width / ratio;
            }
            
            Size = new Size(size.Width + extra.Width, size.Height + extra.Height);
            this.SizeRequest(new Size(Size.Width - Padding.HorizontalThickness, Size.Height - Padding.VerticalThickness));

            OnscreenSizeChanged?.Invoke(this, new EventArgs());

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

        public Size MeasureOnscreenSize(double width, double height)
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
            }*/

            return size;
        }

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

        public Size DesiredSize;

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
        }

        public double PaddedButtonsWidth(double numButtons, double buttonSize) => buttonSize * numButtons + Spacing * ((int)numButtons - 1);

        public double ButtonWidth(double spaceConstraint, double numButtons) => (spaceConstraint - Spacing * ((int)numButtons - 1)) / numButtons;
    }
}