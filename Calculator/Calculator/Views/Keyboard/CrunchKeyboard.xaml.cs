using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

using System.Extensions;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Forms.Extensions;

namespace Calculator
{
    public class OrientedGrid : Grid
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

            RowDefinitionCollection rowDefintions = new RowDefinitionCollection();
            foreach (ColumnDefinition cd in ColumnDefinitions)
            {
                RowDefinition rd = new RowDefinition();
                rd.Height = cd.Width;
                rowDefintions.Add(rd);
            }

            ColumnDefinitionCollection columnDefinitions = new ColumnDefinitionCollection();
            foreach (RowDefinition rd in RowDefinitions)
            {
                ColumnDefinition cd = new ColumnDefinition();
                cd.Width = rd.Height;
                columnDefinitions.Add(cd);
            }

            RowDefinitions = rowDefintions;
            ColumnDefinitions = columnDefinitions;

            Tuple<int, int>[] spans = new Tuple<int, int>[Children.Count];
            for (int i = 0; i < Children.Count; i++)
            {
                spans[i] = new Tuple<int, int>(GetRowSpan(Children[i]), GetColumnSpan(Children[i]));
            }
            for (int i = 0; i < Children.Count; i++)
            {
                SetRowSpan(Children[i], spans[i].Item2);
                SetColumnSpan(Children[i], spans[i].Item1);
            }

            foreach (View child in Children)
            {
                Tuple<int, int> pos = new Tuple<int, int>(Grid.GetRow(child), Grid.GetColumn(child));
                Grid.SetRow(child, pos.Item2);
                Grid.SetColumn(child, pos.Item1);
            }
        }
    }

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CrunchKeyboard : TouchableStackLayout, IEnumerable<Key>, ISoftKeyboard
    {
        public event KeystrokeEventHandler Typed;
        public event EventHandler OnscreenSizeChanged;

        public bool IsCondensed { get; private set; }

        new public StackOrientation Orientation
        {
            get => base.Orientation;
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

        private readonly int MAX_BUTTON_SIZE = 75;
        private double PERMANENT_KEYS_INCREASE = 1.25;
        private double MIN_COLUMNS = 4;

        //public int Rows => Keys.Length + this.Orient(0, 1);
        //public double MaxColumns => Keys[0].Length + this.Orient(PERMANENT_KEYS_INCREASE, 0);
        //public double Columns => this.Orient((IsCondensed ? MIN_COLUMNS : Keys[0].Length) + PERMANENT_KEYS_INCREASE, MIN_COLUMNS);
        //public bool ShowingFullKeyboard => !App.IsCondensed && Settings.ShouldShowFullKeyboard;

        private readonly ScrollView Scroll;
        private readonly Grid Keypad;
        private readonly Grid PermanentKeys;
        private readonly ArrowKeysGrid ArrowKeys;
        public readonly LongClickableButton DockButton;

        public Size FullSize => new Size(
            PaddedButtonsWidth(Keys[0].Length + PERMANENT_KEYS_INCREASE, MAX_BUTTON_SIZE),
            PaddedButtonsWidth(Keys.Length, MAX_BUTTON_SIZE)
            );

        public Size Size { get; private set; }

        public CrunchKeyboard()
        {
            InitializeComponent();

            Resources = new ResourceDictionary();
            // Make spacing consistent between all buttons
            this.WhenDescendantAdded<Grid>((grid) =>
            {
                grid.BindingContext = this;
                grid.SetBinding(Grid.RowSpacingProperty, "Spacing");
                grid.SetBinding(Grid.ColumnSpacingProperty, "Spacing");
            });
            this.WhenDescendantAdded<Button>((button) =>
            {
                button.PropertyChanged += (sender, e) =>
                {
                    if (!e.IsProperty(WidthProperty, HeightProperty, Button.FontSizeProperty))// || button.Text == null)
                    {
                        return;
                    }

                    int length = button.Text?.Length ?? 0;
                    button.FontSize = length > 1 ?
                        Math.Floor(33 * button.Width / 75 * 5 / (4 + length)) :
                        Math.Floor(33 * Math.Max(button.Height, button.Width) / 75);
                };
            });
            this.WhenExactDescendantAdded<Key>((key) =>
            {
                key.Clicked += (sender1, e1) => Typed?.Invoke(key.Output);
            });
            this.WhenExactDescendantAdded<CursorKey>((cursorKey) =>
            {
                cursorKey.Clicked += (sender1, e1) => KeyboardManager.MoveCursor(cursorKey.Key);
            });

            View parentheses = null;
            Key equals = "=";
            //equals.SetBinding<bool, double>(IsVisibleProperty, equals, "Opacity", value => value > 0);
            Keys = new Key[][]
            {
                //new Key[] { null,  null,    null,   null,   null,   null,   null },
                new Key[] { "sin",  TEN,    EXP,    "7",    "8",    "9",    DIV },
                new Key[] { "cos",  LOG,    SQRD,   "4",    "5",    "6",    MULT },
                new Key[] { "tan",  "ln",   SQRT,    "1",    "2",    "3",    "-" },
                new Key[] { PI,     "e",    "x",    "0",    ".",    "()",   "+" }
            };

            Children.Add(Scroll = new ScrollView()
            {
                Content = Keypad = new Grid { },
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                Orientation = ScrollOrientation.Horizontal,
            });
            Children.Add(PermanentKeys = new Grid
            {
                Children =
                {
                    new Key
                    {
                        Text = "DEL",
                        Basic = KeyboardManager.BACKSPACE.ToString()
                    },
                    (ArrowKeys = new ArrowKeysGrid { }),
                    new AbsoluteLayout
                    {
                        Children =
                        {
                            {
                                (DockButton = new LongClickableButton//("", Key.DOCK)
                                {
                                    BackgroundColor = Color.Blue,
                                    FontFamily = App.SYMBOLA_FONT
                                }),
                                new Rectangle(0, 0, 1, 1),
                                AbsoluteLayoutFlags.SizeProportional
                            }
                        }
                    }
                },
                ColumnDefinitions = new ColumnDefinitionCollection { new ColumnDefinition() },
                RowDefinitions = new RowDefinitionCollection { new RowDefinition() }
            });

            bool regular = true;
            DockButton.Clicked += (sender, e) => HandleModeChanged(regular = !regular);

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
                        view.SetBinding<double, double>(OpacityProperty, equals, "Opacity", InverseOpacity, InverseOpacity, BindingMode.TwoWay);
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
            Keypad.Children.Add(equals, Grid.GetColumn(parentheses), Grid.GetRow(parentheses));

            VisualStateManager.GetVisualStateGroups(this).Add(new VisualStates("Regular", "Basic")
            {
                new TargetedSetters
                {
                    Target = PermanentKeys,
                    Setters =
                    {
                        //new Setters { Property = IsVisibleProperty, Values = { true, false } },
                    }
                },
                new TargetedSetters
                {
                    Target = equals,
                    Setters =
                    {
                        new Setters { Property = OpacityProperty, Values = { 0, 1 } },
                        new Setters { Property = InputTransparentProperty, Values = { true, false } }
                    }
                },
                new TargetedSetters
                {
                    Target = DockButton,
                    Setters =
                    {
                        new Setters
                        {
                            Property = AbsoluteLayout.LayoutBoundsProperty,
                            Values =
                            {
                                new Thunk<Rectangle>(() =>
                                {
                                    double finalWidth = ButtonWidth(Width, MIN_COLUMNS + 1.25);
                                    return new Rectangle(0, 0, finalWidth * 1.25, finalWidth);
                                }),
                                new Thunk<Rectangle>(() =>
                                {
                                    double finalWidth = ButtonWidth(Width, MIN_COLUMNS);
                                    return new Rectangle(0, -PaddedButtonsWidth(MIN_COLUMNS - 1, finalWidth), finalWidth, finalWidth);
                                })
                            }
                        }
                    }
                }
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

            App.Current.MainPage.SizeChanged += (sender, e) => ScreenSizeChanged();
            App.ShowFullKeyboard.WhenPropertyChanged(App.ShowFullKeyboard.ValueProperty, (sender, e) => ScreenSizeChanged());

            this.Bind<StackOrientation>(OrientationProperty, value =>
            {
                for (int i = 0; i < 3; i++)
                {
                    int pos = i + (i > 1).ToInt();

                    if (value == StackOrientation.Horizontal)
                    {
                        SetPos(PermanentKeys.Children[i], pos, 0);
                    }
                    else
                    {
                        SetPos(PermanentKeys.Children[i], 0, pos);
                    }
                }

                int span = (value == StackOrientation.Horizontal).ToInt();
                Grid.SetRowSpan(PermanentKeys.Children[1], 1 + span);
                Grid.SetColumnSpan(PermanentKeys.Children[1], 2 - span);
                ArrowKeys.ChangeOrientation(value);
            });
        }

        private void HandleModeChanged(bool regular = true)
        {
            //DockButton.TranslationX = DockButton.Width + Spacing;
            //Keypad.Children.Add(DockButton, Keys[0].Length - 1, Keys.Length - 1);

            Scroll.ScrollToAsync(Keypad, ScrollToPosition.End, false);
            foreach (View view in Keypad.Children)
            {
                if (Grid.GetColumn(view) < Keys[0].Length - MIN_COLUMNS)
                {
                    //view.Remove();
                    //view.IsVisible = false;
                }
            }

            (PermanentKeys.Children[2] as AbsoluteLayout).Children.Add(DockButton);
            AbsoluteLayoutExtensions.SetLayout(DockButton, DockButton.Bounds.Size, AbsoluteLayoutFlags.None);
            PermanentKeys.IsVisible = true;

            string[] modes = new string[] { "Regular", "Basic" };
            if (regular)
            {
                System.Misc.Swap(ref modes[0], ref modes[1]);
            }

            int startingColumns = App.ShowFullKeyboard.Value ? Keys[0].Length : 4;
            this.AnimateVisualState("changeMode", modes[0], modes[1], length: 5000, easing: Easing.Linear, callback: value =>
            {
                PERMANENT_KEYS_INCREASE = (regular ? value : (1 - value)) * 1.25;
                //MIN_COLUMNS = 4 + value * (startingColumns - 4);
                //MIN_COLUMNS = 4 + (regular ? value : (1 - value)) * (Keys[0].Length - 4);
                ScreenSizeChanged();

                double buttonSize = ButtonWidth(Width - Padding.HorizontalThickness, (IsCondensed ? MIN_COLUMNS : Keys[0].Length) + PERMANENT_KEYS_INCREASE) * PERMANENT_KEYS_INCREASE;
                DockButton.TranslationX = value == 1 ? 0 : -(DockButton.Width - buttonSize);
                //DockButton.TranslationX = value == 1 ? 0 : (-(DockButton.Width - PermanentKeys.Width));
            }, finished: (final, cancelled) =>
            {
                if (regular)
                {
                    AbsoluteLayoutExtensions.SetLayout(DockButton, new Size(1, 1), AbsoluteLayoutFlags.SizeProportional);
                }
                else
                {
                    Keypad.Children.Add(DockButton, Keys[0].Length - 1, 0);
                }

                PermanentKeys.IsVisible = regular;
            });
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
            Size bounds = App.Current.MainPage.Bounds.Size;
            bounds = new Size(bounds.Width - SafeArea.HorizontalThickness, bounds.Height - SafeArea.VerticalThickness);
            //Print.Log("screen size changed", SafeArea.UsefulToString(), bounds);
            CondensedOrientation = bounds.Height >= bounds.Width ? StackOrientation.Horizontal : StackOrientation.Vertical;

            Size size = MeasureOnscreenSize(bounds.Width, bounds.Height);

            bool isCondensed = !FullSize.Equals(size);
            bool collapsed = size.Width == bounds.Width || size.Height == bounds.Height;
            base.Orientation = collapsed ? CondensedOrientation : StackOrientation.Horizontal;
            IsCondensed = isCondensed;

            //Print.Log("requesting " + size);
            this.SizeRequest(size);
            Size = !collapsed ? size : (Orientation == StackOrientation.Horizontal ? new Size(size.Width + SafeArea.HorizontalThickness, size.Height + SafeArea.Bottom) : new Size(size.Width + SafeArea.Right, size.Height + SafeArea.VerticalThickness));

            OnscreenSizeChanged?.Invoke(this, new EventArgs());
            //Print.Log("display size changed", App.Current.MainPage.Bounds.Size, size, Orientation);

            DockButton.Text = collapsed ? "" : "\u25BD"; //white down-pointing triangle
            //DockButton.IsEnabled = !collapsed;

            if (Collapsed != collapsed)
            {
                Collapsed = collapsed;
                OnPropertyChanged("Collapsed");
            }
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

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
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

                PermanentKeys.ColumnDefinitions[0].ClearValue(ColumnDefinition.WidthProperty);
                PermanentKeys.RowDefinitions[0].ClearValue(RowDefinition.HeightProperty);

                double buttonSize = 0;
                if (Orientation == StackOrientation.Horizontal && Width > 0)
                {
                    buttonSize = Math.Max(0, ButtonWidth(Width - Padding.HorizontalThickness, (IsCondensed ? MIN_COLUMNS : Keys[0].Length) + PERMANENT_KEYS_INCREASE));
                    PermanentKeys.ColumnDefinitions[0].Width = buttonSize * PERMANENT_KEYS_INCREASE;
                    //AbsoluteLayoutExtensions.SetLayoutSize(DockButton, new Size(buttonSize * 1.25, buttonSize));
                    //PermanentKeys.ColumnDefinitions.Add(new ColumnDefinition { Width = buttonSize * PERMANENT_KEYS_INCREASE });
                }
                else if (Orientation == StackOrientation.Vertical && Height > 0)
                {
                    buttonSize = Math.Max(0, ButtonWidth(Height - Padding.VerticalThickness, Keys.Length + 1));
                    PermanentKeys.RowDefinitions[0].Height = buttonSize;
                    //PermanentKeys.RowDefinitions.Add(new RowDefinition { Height = buttonSize });
                }
            }
        }

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

        private static void SetPos(BindableObject bindable, int row, int column)
        {
            Grid.SetRow(bindable, row);
            Grid.SetColumn(bindable, column);
        }

        private class ArrowKeysGrid : Grid
        {
            public ArrowKeysGrid()
            {
                Children.Add((LabelButton)new CursorKey
                {
                    Text = "〈",
                    Key = KeyboardManager.CursorKey.Up,
                    FontFamily = App.SYMBOLA_FONT,
                });
                Children.Add(new CursorKey
                {
                    Text = "〈",
                    Key = KeyboardManager.CursorKey.Left,
                    FontFamily = App.SYMBOLA_FONT,
                });
                Children.Add(new CursorKey
                {
                    Text = "〉",
                    Key = KeyboardManager.CursorKey.Right,
                    FontFamily = App.SYMBOLA_FONT,
                });
                Children.Add((LabelButton)new CursorKey
                {
                    Text = "〉",
                    Key = KeyboardManager.CursorKey.Down,
                    FontFamily = App.SYMBOLA_FONT,
                });

                // Rotate the arrow text for the up and down keys
                foreach (View view in new View[] { Children[0], Children[3] })
                {
                    if (!(view is LabelButton labelButton))
                    {
                        continue;
                    }

                    labelButton.Label.Rotation = 90;
                    AbsoluteLayout.SetLayoutBounds(labelButton.Label, new Rectangle(0.5, 0.5, 100, 100));
                }

                Grid.SetColumnSpan(Children[0], 2);
                Grid.SetRowSpan(Children[1], 2);
                Grid.SetRowSpan(Children[2], 2);
                Grid.SetColumnSpan(Children[3], 2);
            }

            public void ChangeOrientation(StackOrientation orientation)
            {
                if (orientation == StackOrientation.Horizontal)
                {
                    SetPos(Children[0], 0, 0); // Up
                    SetPos(Children[1], 1, 0); // Left
                    SetPos(Children[2], 1, 1); // Right
                    SetPos(Children[3], 3, 0); // Down
                }
                else if (orientation == StackOrientation.Vertical)
                {
                    SetPos(Children[0], 0, 1);
                    SetPos(Children[1], 0, 0);
                    SetPos(Children[2], 0, 3);
                    SetPos(Children[3], 1, 1);
                }
            }
        }
    }
}