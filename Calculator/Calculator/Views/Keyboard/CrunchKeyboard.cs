﻿using System;
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

    public class CrunchKeyboard : StackLayout, IEnumerable<Key>, ISoftKeyboard
    {
        //public event SpecialKeyEventHandler<Key> LongKeyPress;

        public event KeystrokeEventHandler Typed;
        public event EventHandler OnscreenSizeChanged;

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

        public Size FullSize => new Size(
            PaddedButtonsWidth(Keys[0].Length + PERMANENT_KEYS_INCREASE, MAX_BUTTON_SIZE),
            PaddedButtonsWidth(Keys.Length, MAX_BUTTON_SIZE)
            );

        public Size Size { get; private set; }

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
                            //Keys[i][j].Clicked += (sender, e) => Scroll.ScrollToAsync(Keypad, ScrollToPosition.End, false);
                        }
                    }

                    Keypad.Children.Add(view, j, i);
                }
            }

            Keypad.SetBinding<double, double>(WidthRequestProperty, Keypad, "Height", value => PaddedButtonsWidth(Keys[0].Length, ButtonWidth(value, Keys.Length)));
            Keypad.WhenPropertyChanged(WidthProperty, (sender, e) => ResetScroll());

            Scroll.SizeChanged += (sender, e) =>
            {
                //Keypad.WidthRequest = Keys[0].Length / Keys.Length * Scroll.Height;
                return;
                //Keypad.WidthRequest = PaddedButtonsWidth(Keys[0].Length, )

                bool isCondensed = !FullSize.Equals(Bounds.Size);

                double buttonSize = 0;
                if (Orientation == StackOrientation.Horizontal && Scroll.Width > 0)
                {
                    buttonSize = Math.Max(0, ButtonWidth(Scroll.Width, (isCondensed ? MIN_COLUMNS : Keys[0].Length) + PERMANENT_KEYS_INCREASE));
                }
                else if (Orientation == StackOrientation.Vertical && Scroll.Height > 0)
                {
                    buttonSize = Math.Max(0, ButtonWidth(Scroll.Height, Keys.Length + 1));
                }

                Keypad.WidthRequest = PaddedButtonsWidth(Keys[0].Length, buttonSize);
            };
            SizeChanged += (sender, e) =>
            {
                //ResetScroll();
                //SoftKeyboardManager.SizeChangedHandler(this, new EventArgs<Size>(Bounds.Size));
            };
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

        private Thickness SafeArea;

        public void SafeAreaChanged(Thickness value)
        {
            SafeArea = value;
            ScreenSizeChanged();
        }

        private void ScreenSizeChanged()
        {
            Size bounds = App.Current.MainPage.Bounds.Size;
            bounds = new Size(bounds.Width - SafeArea.HorizontalThickness, bounds.Height - SafeArea.VerticalThickness);
            Print.Log("screen size changed", SafeArea.UsefulToString(), bounds);
            CondensedOrientation = bounds.Height >= bounds.Width ? StackOrientation.Horizontal : StackOrientation.Vertical;

            Size size = MeasureOnscreenSize(bounds.Width, bounds.Height);

            bool isCondensed = !FullSize.Equals(size);
            bool collapsed = size.Width == bounds.Width || size.Height == bounds.Height;
            base.Orientation = collapsed ? CondensedOrientation : StackOrientation.Horizontal;
            IsCondensed = isCondensed;

            Print.Log("requesting " + size);
            this.SizeRequest(size);
            Size = !collapsed ? size : (Orientation == StackOrientation.Horizontal ? new Size(size.Width + SafeArea.HorizontalThickness, size.Height + SafeArea.Bottom) : new Size(size.Width + SafeArea.Right, size.Height + SafeArea.VerticalThickness));

            OnscreenSizeChanged?.Invoke(this, new EventArgs());
            //Print.Log("display size changed", App.Current.MainPage.Bounds.Size, size, Orientation);

            DockButton.Text = collapsed ? "" : "\u25BD"; //white down-pointing triangle
            DockButton.IsEnabled = !collapsed;
        }

        public Size MeasureOnscreenSize()
        {
            double width = App.Current.MainPage.Width;// - Margin.HorizontalThickness;
            double height = App.Current.MainPage.Height;// - Margin.VerticalThickness;
            return MeasureOnscreenSize(width, height);
        }

        public Size MeasureOnscreenSize(double width, double height)
        {
            //Orientation = height >= width ? StackOrientation.Horizontal : StackOrientation.Vertical;

            Size size = GetSize(width, height);// OnMeasure(width, height).Request;

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
                //IsCondensed = !FullSize.Equals(Bounds.Size);
                //Orientation = Height >= Width ? StackOrientation.Horizontal : StackOrientation.Vertical;
                
                PermanentKeys.ColumnDefinitions = new ColumnDefinitionCollection();
                PermanentKeys.RowDefinitions = new RowDefinitionCollection();
                
                double buttonSize = 0;
                if (Orientation == StackOrientation.Horizontal && Width > 0)
                {
                    buttonSize = Math.Max(0, ButtonWidth(Width - Padding.HorizontalThickness, (IsCondensed ? MIN_COLUMNS : Keys[0].Length) + PERMANENT_KEYS_INCREASE));
                    PermanentKeys.ColumnDefinitions.Add(new ColumnDefinition { Width = buttonSize * PERMANENT_KEYS_INCREASE });
                }
                else if (Orientation == StackOrientation.Vertical && Height > 0)
                {
                    buttonSize = Math.Max(0, ButtonWidth(Height - Padding.VerticalThickness, Keys.Length + 1));
                    PermanentKeys.RowDefinitions.Add(new RowDefinition { Height = buttonSize });
                }
            }
        }

        //public SizeRequest ForceMeasure(double widthConstraint, double heightConstraint) => OnMeasure(widthConstraint, heightConstraint);

        public Size DesiredSize;

        private Size GetSize(double widthConstraint, double heightConstraint)
        {
            widthConstraint -= Padding.HorizontalThickness;
            heightConstraint -= Padding.VerticalThickness;

            bool idealOrientation = CondensedOrientation == StackOrientation.Horizontal;
            double rows = Keys.Length + (!idealOrientation).ToInt();
            double cols = MIN_COLUMNS + idealOrientation.ToInt() * PERMANENT_KEYS_INCREASE;

            // Determine if the condensed size is smaller if we make it as wide as possible or a tall as possible. Could probably be more efficient by comparing ratio of width / height to cols / rows
            Size fitWidth = new Size(widthConstraint, Math.Min(heightConstraint, PaddedButtonsWidth(rows, ButtonWidth(widthConstraint, cols))));
            Size fitHeight = new Size(Math.Min(widthConstraint, PaddedButtonsWidth(cols, ButtonWidth(heightConstraint, rows))), heightConstraint);
            Size condensed = fitWidth.Area() < fitHeight.Area() ? fitWidth : fitHeight;

            Print.Log("measuring", widthConstraint, heightConstraint, condensed, App.ShowFullKeyboard.Value);
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