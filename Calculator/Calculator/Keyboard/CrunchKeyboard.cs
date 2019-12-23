using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

using System.Extensions;
using Xamarin.Forms;
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
            foreach(ColumnDefinition cd in ColumnDefinitions)
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

            foreach(View child in Children)
            {
                Tuple<int, int> pos = new Tuple<int, int>(Grid.GetRow(child), Grid.GetColumn(child));
                Grid.SetRow(child, pos.Item2);
                Grid.SetColumn(child, pos.Item1);
            }
        }
    }

    public class CrunchKeyboard : StackLayout, IEnumerable<Key>, IKeyboard
    {
        //public event SpecialKeyEventHandler<Key> LongKeyPress;

        public KeystrokeEventHandler Typed { get; set; }

        //public event EventHandler<ToggledEventArgs> CondensedChanged;
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
        private readonly double PERMANENT_KEYS_INCREASE = 1.25;
        private readonly int MIN_COLUMNS = 4;

        //public int Rows => Keys.Length + this.Orient(0, 1);
        //public double MaxColumns => Keys[0].Length + this.Orient(PERMANENT_KEYS_INCREASE, 0);
        public double Columns => this.Orient((IsCondensed ? MIN_COLUMNS : Keys[0].Length) + PERMANENT_KEYS_INCREASE, MIN_COLUMNS);
        //public bool ShowingFullKeyboard => !App.IsCondensed && Settings.ShouldShowFullKeyboard;

        private readonly ScrollView Scroll;
        private readonly Grid Keypad;
        private readonly Grid PermanentKeys;
        private readonly ArrowKeysGrid ArrowKeys;
        public readonly LongClickableButton DockButton;

        private Size FullSize => new Size(
            PaddedButtonsWidth(Keys[0].Length + PERMANENT_KEYS_INCREASE, MAX_BUTTON_SIZE),
            PaddedButtonsWidth(Keys.Length, MAX_BUTTON_SIZE)
            );

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

            Keys = new Key[][]
            {
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
                    (DockButton = new LongClickableButton//("", Key.DOCK)
                    {
                        FontFamily = CrunchStyle.SYMBOLA_FONT
                    })
                }
            });

            for (int i = 0; i < Keys.Length; i++)
            {
                for (int j = 0; j < Keys[i].Length; j++)
                {
                    View view = Keys[i][j];

                    if (Keys[i][j].Text == "()")
                    {
                        Grid grid = new Grid { };

                        grid.Children.Add(new Key { Text = "(" }, 0, 0);
                        grid.Children.Add(new Key { Text = ")" }, 1, 0);

                        view = grid;
                    }
                    else
                    {
                        if (Keys[i].Length - j > MIN_COLUMNS)
                        {
                            Keys[i][j].Clicked += (sender, e) => Scroll.ScrollToAsync(Keypad, ScrollToPosition.End, false);
                        }
                    }

                    Keypad.Children.Add(view, j, i);
                }
            }

            SizeChanged += (sender, e) => ResetScroll();

            Orientation = StackOrientation.Horizontal;
            OnPropertyChanged(OrientationProperty.PropertyName);
        }

        private void ResetScroll() => Scroll.ScrollToAsync(Keypad, ScrollToPosition.End, false);
        public void Remeasure() => InvalidateMeasure();

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
            try
            {
                base.OnPropertyChanged(propertyName);
            }
            catch { }

            if (propertyName.IsProperty(WidthProperty) || propertyName.IsProperty(HeightProperty))
            {
                IsCondensed = !FullSize.Equals(Bounds.Size);

                base.Orientation = IsCondensed ? CondensedOrientation : StackOrientation.Horizontal;

                PermanentKeys.ColumnDefinitions = new ColumnDefinitionCollection();
                PermanentKeys.RowDefinitions = new RowDefinitionCollection();

                double buttonSize = 0;
                if (Orientation == StackOrientation.Horizontal && Width > 0)
                {
                    buttonSize = ButtonWidth(Width, (IsCondensed ? MIN_COLUMNS : Keys[0].Length) + PERMANENT_KEYS_INCREASE);
                    PermanentKeys.ColumnDefinitions.Add(new ColumnDefinition { Width = buttonSize * PERMANENT_KEYS_INCREASE });
                }
                else if (Orientation == StackOrientation.Vertical && Height > 0)
                {
                    buttonSize = ButtonWidth(Height, Keys.Length + 1);
                    PermanentKeys.RowDefinitions.Add(new RowDefinition { Height = buttonSize });
                }

                Keypad.WidthRequest = PaddedButtonsWidth(Keys[0].Length, buttonSize);
            }
            
            if (PermanentKeys != null && propertyName.IsProperty(OrientationProperty))
            {
                for (int i = 0; i < 3; i++)
                {
                    int pos = i + (i > 1).ToInt();

                    if (Orientation == StackOrientation.Horizontal)
                    {
                        SetPos(PermanentKeys.Children[i], pos, 0);
                    }
                    else
                    {
                        SetPos(PermanentKeys.Children[i], 0, pos);
                    }
                }

                int span = (Orientation == StackOrientation.Horizontal).ToInt();
                Grid.SetRowSpan(PermanentKeys.Children[1], 1 + span);
                Grid.SetColumnSpan(PermanentKeys.Children[1], 2 - span);
                ArrowKeys.ChangeOrientation(Orientation);
            }
        }

        protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
        {
            SizeRequest sr = base.OnMeasure(widthConstraint, heightConstraint);
            
            bool idealOrientation = CondensedOrientation == StackOrientation.Horizontal;
            double rows = Keys.Length + (!idealOrientation).ToInt();
            double cols = MIN_COLUMNS + idealOrientation.ToInt() * PERMANENT_KEYS_INCREASE;

            Size fitWidth = new Size(widthConstraint, Math.Min(heightConstraint, PaddedButtonsWidth(rows, ButtonWidth(widthConstraint, cols))));
            Size fitHeight = new Size(Math.Min(widthConstraint, PaddedButtonsWidth(cols, ButtonWidth(heightConstraint, rows))), heightConstraint);
            Size condensed = fitWidth.Area() < fitHeight.Area() ? fitWidth : fitHeight;

            //Size full = Settings.ShouldShowFullKeyboard ? FullSize : new Size(PaddedButtonsWidth())
            /*double buttonSize = CondensedOrientation == StackOrientation.Horizontal ? ButtonWidth(widthConstraint, MIN_COLUMNS + PERMANENT_KEYS_INCREASE) : ButtonWidth(heightConstraint, Keys.Length + 1);
            Size condensedSize = new Size(
                PaddedButtonsWidth(cols, buttonSize),
                PaddedButtonsWidth(rows, buttonSize)
                );*/

            Print.Log("measuring", widthConstraint, heightConstraint, Settings.ShouldShowFullKeyboard);
            Size fullSize = Settings.ShouldShowFullKeyboard ? FullSize : new Size(PaddedButtonsWidth(MIN_COLUMNS + PERMANENT_KEYS_INCREASE, MAX_BUTTON_SIZE), PaddedButtonsWidth(Keys.Length, MAX_BUTTON_SIZE));
            sr.Request = FullSize.Width < widthConstraint && FullSize.Height < heightConstraint && FullSize.Area() < condensed.Area() ? fullSize : condensed;

            return sr;

            /*Size condensed = CrunchKeyboard.CondensedSize(Width / (Page.Orientation == StackOrientation.Horizontal ? 2 : 1), Height / (Page.Orientation == StackOrientation.Vertical ? 2 : 1), Page.Orientation);

            double rows = Keys.Length + this.Orient(0, 1);
            double columns = Keys[0].Length + this.Orient(PERMANENT_KEYS_INCREASE, 0);
            double buttonSize = CanExpand(widthConstraint) ? MAX_BUTTON_SIZE : ButtonWidth(widthConstraint, columns - (Keys[0].Length - MIN_COLUMNS));

            sr.Request = new Size(PaddedButtonsWidth(columns, buttonSize), PaddedButtonsWidth(rows, buttonSize));
            return sr;*/
        }

        //private bool CanExpand(double widthConstraint) => PaddedButtonsWidth(Columns, MAX_BUTTON_SIZE) > widthConstraint;

        /*protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
        {
            SizeRequest sr = base.OnMeasure(widthConstraint, heightConstraint);
            Size request = sr.Request;
            Print.Log("measuring", request, widthConstraint, heightConstraint);
            //return sr;

            if (!double.IsInfinity(widthConstraint))
            {
                ////return sr;
                request.Width = widthConstraint;
            }

            //Print.Log("height request", request.Height, heightConstraint);
            double buttonSize = ButtonWidth(request.Width, Columns);
            //request.Width = widthConstraint;
            request.Height = Math.Min(heightConstraint, PaddedButtonsWidth(Keys.Length, buttonSize));

            sr.Request = request;
            return sr;
        }*/

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
                /*AnythingButton up = new AnythingButton { };
                up.Children.Add(
                    new Label
                    {
                        
                    },
                    new Rectangle(0.5, 0.5, 100, 100),
                    AbsoluteLayoutFlags.PositionProportional
                    );
                up.Children[up.Children.Count - 1].SetBinding(Button.FontSizeProperty, "FontSize");
                up.Button.Clicked += (sender, e) => KeyboardManager.MoveCursor(KeyboardManager.CursorKey.Up);*/

                /*AnythingButton down = new AnythingButton { };
                down.Children.Add(
                    new Label
                    {
                        Text = "〉",
                        TextColor = CrunchStyle.BUTTON_TEXT_COLOR,
                        HorizontalTextAlignment = TextAlignment.Center,
                        VerticalTextAlignment = TextAlignment.Center,
                        Rotation = 90,
                        BindingContext = left,
                        FontFamily = CrunchStyle.SYMBOLA_FONT,
                    },
                    new Rectangle(0.5, 0.5, 100, 100),
                    AbsoluteLayoutFlags.PositionProportional
                    );
                down.Children[down.Children.Count - 1].SetBinding(Button.FontSizeProperty, "FontSize");
                down.Button.Clicked += (sender, e) => KeyboardManager.MoveCursor(KeyboardManager.CursorKey.Down);*/

                Children.Add((LabelButton)new CursorKey
                {
                    Text = "〈",
                    Key = KeyboardManager.CursorKey.Up,
                    FontFamily = CrunchStyle.SYMBOLA_FONT,
                });
                Children.Add(new CursorKey
                {
                    Text = "〈",
                    Key = KeyboardManager.CursorKey.Left,
                    FontFamily = CrunchStyle.SYMBOLA_FONT,
                });
                Children.Add(new CursorKey
                {
                    Text = "〉",
                    Key = KeyboardManager.CursorKey.Right,
                    FontFamily = CrunchStyle.SYMBOLA_FONT,
                });
                Children.Add((LabelButton)new CursorKey
                {
                    Text = "〉",
                    Key = KeyboardManager.CursorKey.Down,
                    FontFamily = CrunchStyle.SYMBOLA_FONT,
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