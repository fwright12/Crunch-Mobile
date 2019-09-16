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
    public delegate void KeyPressEventHandler(string output);

    public class Key : LongClickableButton
    {
        //public static readonly string DELETE = "delete";
        public static readonly string DOCK = "dock";

        private string Basic;

        public Key(string display, string basic = "")
        {
            Padding = new Thickness(0);
            Text = display;
            Basic = basic;
        }

        public string Output => Basic == "" ? Text : Basic;

        public static implicit operator Key(string s) => new Key(s);
    }

    public class CursorKey : Key
    {
        /*public static readonly string LEFT = "left";
        public static readonly string RIGHT = "right";
        public static readonly string UP = "up";
        public static readonly string DOWN = "down";*/

        public KeyboardManager.CursorKey Key;

        public CursorKey(string display, KeyboardManager.CursorKey key) : base(display)
        {
            Key = key;
        }
    }

    public class CrunchKeyboard : StackLayout, IEnumerable<Key>, IKeyboard
    {
        public event SpecialKeyEventHandler<Key> LongKeyPress;

        public KeystrokeEventHandler Typed { get; set; }

        private static readonly Key EXP = new Key("x\u207F", "^");
        private static readonly Key SQRD = new Key("x\u00B2", "^2");
        private static readonly Key DIV = new Key("\u00F7", "/");
        private static readonly Key LOG = new Key("log", "log_");
        private static readonly Key SQRT = new Key("\u221A", "√");
        private static readonly Key MULT = new Key("\u00D7", "*");
        private static readonly Key TEN = new Key("10\u207F", "10^");
        private static readonly Key PI = new Key("\u03C0");

        private readonly Key[][] Keys = new Key[][]
        {
            new Key[] { "sin",  TEN,    EXP,    "7",    "8",    "9",    DIV },
            new Key[] { "cos",  LOG,    SQRD,   "4",    "5",    "6",    MULT },
            new Key[] { "tan",  "ln",   SQRT,    "1",    "2",    "3",    "-" },
            new Key[] { PI,     "e",    "x",    "0",    ".",    "()",   "+" }
        };

        private readonly int MAX_BUTTON_SIZE = 75;
        private readonly double PERMANENT_KEYS_INCREASE = 1.25;
        private readonly int MIN_COLUMNS = 4;

        public int Rows => Keys.Length + Orient(0, 1);
        public double Columns => Orient((ShowingFullKeyboard ? Keys[0].Length : MIN_COLUMNS) + PERMANENT_KEYS_INCREASE, MIN_COLUMNS);
        public bool ShowingFullKeyboard => Settings.ShouldShowFullKeyboard && CanShowFullKeyboard;

        private Grid PermanentKeys;
        private Grid ArrowKeys;

        private bool CanShowFullKeyboard = false;

        public CrunchKeyboard()
        {
            Resources = new ResourceDictionary();
            Resources.Add<Grid>(
                (grid) =>
                {
                    grid.BindingContext = this;
                    grid.SetBinding(Grid.RowSpacingProperty, "Spacing");
                    grid.SetBinding(Grid.ColumnSpacingProperty, "Spacing");
                });
            Resources.Add<Key>(
                (key) =>
                {
                    if (key is CursorKey)
                    {
                        key.Clicked += (sender, e) => KeyboardManager.MoveCursor((sender as CursorKey).Key);
                    }
                    else if (key.Output != Key.DOCK)
                    {
                        key.Clicked += (sender, e) => Typed?.Invoke((sender as Key).Output);
                    }

                    key.LongClick += (sender, e) => LongKeyPress?.Invoke(sender as Key);

                    key.PropertyChanged += (sender, e) =>
                    {
                        if (!e.IsProperty(WidthProperty, HeightProperty, Button.FontSizeProperty) || !(sender is Button button) || button.Text == null)
                        {
                            return;
                        }

                        button.FontSize = button.Text.Length > 1 ?
                            Math.Floor(33 * button.Width / 75 * 5 / (4 + button.Text.Length)) :
                            Math.Floor(33 * Math.Max(button.Height, button.Width) / 75);
                    };
                });

            Grid keypad = new Grid();

            ScrollView scroll = new ScrollView()
            {
                Content = keypad,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                Orientation = ScrollOrientation.Horizontal,
            };

            for (int i = 0; i < Keys.Length; i++)
            {
                for (int j = 0; j < Keys[i].Length; j++)
                {
                    View view = Keys[i][j];

                    if (Keys[i][j].Text == "()")
                    {
                        Grid grid = new Grid();

                        grid.Children.Add(new Key("("), 0, 0);
                        grid.Children.Add(new Key(")"), 1, 0);

                        view = grid;
                    }
                    else
                    {
                        if (Keys[i].Length - j > MIN_COLUMNS)
                        {
                            Keys[i][j].Clicked += (sender, e) => scroll.ScrollToAsync(keypad, ScrollToPosition.End, false);
                        }
                    }

                    keypad.Children.Add(view, j, i);
                }
            }

            for (int i = 0; i < Keys[0].Length; i++)
            {
                ColumnDefinition cd = new ColumnDefinition
                {
                    BindingContext = this,
                };
                keypad.ColumnDefinitions.Add(cd);

                scroll.SizeChanged += (sender, e) =>
                {
                    cd.Width = ButtonWidth(Width, Columns);
                };
            }

            Button left = new CursorKey("\u27E8", KeyboardManager.CursorKey.Left);

            AnythingButton up = new AnythingButton { };
            up.Children.Add(
                new Label
                {
                    Text = "\u27E8", //mathematical left angle bracket
                    TextColor = CrunchStyle.BUTTON_TEXT_COLOR,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                    Rotation = 90,
                    BindingContext = left,
                },
                new Rectangle(0.5, 0.5, 100, 100),
                AbsoluteLayoutFlags.PositionProportional
                );
            up.Children[up.Children.Count - 1].SetBinding(Button.FontSizeProperty, "FontSize");
            up.Button.Clicked += (sender, e) => KeyboardManager.MoveCursor(KeyboardManager.CursorKey.Up);

            AnythingButton down = new AnythingButton { };
            down.Children.Add(
                new Label
                {
                    Text = "\u27E9", //mathematical right angle bracket
                    TextColor = CrunchStyle.BUTTON_TEXT_COLOR,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                    Rotation = 90,
                    BindingContext = left
                },
                new Rectangle(0.5, 0.5, 100, 100),
                AbsoluteLayoutFlags.PositionProportional
                );
            down.Children[down.Children.Count - 1].SetBinding(Button.FontSizeProperty, "FontSize");
            down.Button.Clicked += (sender, e) => KeyboardManager.MoveCursor(KeyboardManager.CursorKey.Down);

            ArrowKeys = new Grid();
            ArrowKeys.Children.Add(up);
            ArrowKeys.Children.Add(left); //mathematical left angle bracket
            ArrowKeys.Children.Add(new CursorKey("\u27E9", KeyboardManager.CursorKey.Right)); //mathematical right angle bracket
            ArrowKeys.Children.Add(down);

            Grid.SetColumnSpan(ArrowKeys.Children[0], 2);
            Grid.SetRowSpan(ArrowKeys.Children[1], 2);
            Grid.SetRowSpan(ArrowKeys.Children[2], 2);
            Grid.SetColumnSpan(ArrowKeys.Children[3], 2);

            PermanentKeys = new Grid();
            PermanentKeys.RowDefinitions.Add(new RowDefinition());
            PermanentKeys.ColumnDefinitions.Add(new ColumnDefinition());

            PermanentKeys.Children.Add(new Key("DEL", KeyboardManager.BACKSPACE.ToString()));
            PermanentKeys.Children.Add(ArrowKeys);
            PermanentKeys.Children.Add(Device.Idiom == TargetIdiom.Tablet ? new Key("\u25BD", Key.DOCK) : new LongClickableButton()); //white down-pointing triangle

            Children.Add(scroll);
            Children.Add(PermanentKeys);

            OnPropertyChanged(OrientationProperty.PropertyName);

            Settings.KeyboardChanged += (e) =>
            {
                InvalidateMeasure();
            };

            LayoutChanged += (sender, e) =>
            {
                scroll.ScrollToAsync(keypad, ScrollToPosition.End, false);
            };
        }

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

                if (Orientation == StackOrientation.Horizontal)
                {
                    SetPos(ArrowKeys.Children[0], 0, 0); // Up
                    SetPos(ArrowKeys.Children[1], 1, 0); // Left
                    SetPos(ArrowKeys.Children[2], 1, 1); // Right
                    SetPos(ArrowKeys.Children[3], 3, 0); // Down
                }
                else if (Orientation == StackOrientation.Vertical)
                {
                    SetPos(ArrowKeys.Children[0], 0, 1);
                    SetPos(ArrowKeys.Children[1], 0, 0);
                    SetPos(ArrowKeys.Children[2], 0, 3);
                    SetPos(ArrowKeys.Children[3], 1, 1);
                }
            }

            if (!propertyName.IsProperty(OrientationProperty, WidthProperty, HeightProperty, SpacingProperty) || Height < 0 || Width < 0)
            //if (propertyName != OrientationProperty.PropertyName && propertyName != WidthProperty.PropertyName && propertyName != HeightProperty.PropertyName && propertyName != SpacingProperty.PropertyName)
            {
                return;
            }

            PermanentKeys.ColumnDefinitions[0] = new ColumnDefinition();
            PermanentKeys.RowDefinitions[0] = new RowDefinition();

            if (Orientation == StackOrientation.Horizontal)
            {
                PermanentKeys.ColumnDefinitions[0].Width = ButtonWidth(Width, Columns) * PERMANENT_KEYS_INCREASE;
            }
            else
            {
                PermanentKeys.RowDefinitions[0].Height = ButtonWidth(Height, Rows);
            }
        }

        protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
        {
            Print.Log("measuring");
            SizeRequest sr = base.OnMeasure(widthConstraint, heightConstraint);
            //return sr;

            VisualElement root = this.Parent<TouchScreen>();
            double width = root.Width;
            double height = root.Height;

            CanShowFullKeyboard = Device.Idiom == TargetIdiom.Tablet;// PaddedButtonsWidth((Keys[0].Length + PERMANENT_KEYS_INCREASE), MAX_BUTTON_SIZE) < width;
            //CanShowFullKeyboard = (Keys[0].Length + PERMANENT_KEYS_INCREASE) * MAX_BUTTON_SIZE * Keys.Length * MAX_BUTTON_SIZE < width * height / 2;

            if (Device.Idiom == TargetIdiom.Tablet && !CanShowFullKeyboard)
            {
                Print.Log(width, height, root);
                ;
            } 
            double buttonSize = CanShowFullKeyboard ? MAX_BUTTON_SIZE : ButtonWidth(Orient(width, height), Orient(Columns, Rows));
            Print.Log(new Size(PaddedButtonsWidth(Columns, buttonSize), PaddedButtonsWidth(Rows, buttonSize)
                ));
            return new SizeRequest(new Size(
                Math.Min(widthConstraint, PaddedButtonsWidth(Columns, buttonSize)),
                Math.Min(heightConstraint, PaddedButtonsWidth(Rows, buttonSize)
                )));
        }

        public double PaddedButtonsWidth(double numButtons, double buttonSize) => buttonSize * numButtons + Spacing * ((int)numButtons - 1);

        public double ButtonWidth(double spaceConstraint, double numButtons) => (spaceConstraint - Spacing * ((int)numButtons - 1)) / numButtons;

        /*public void ClearOverlay() => Mask.Children.Clear();
        public void Overlay(View view, Rectangle bounds, AbsoluteLayoutFlags flags = AbsoluteLayoutFlags.None) => Mask.Children.Add(view, bounds, flags);

        public void MaskKeys(bool onOrOff) => Mask.IsVisible = onOrOff;*/

        private T Orient<T>(T ifHorizontal, T ifVertical) => Orientation == StackOrientation.Horizontal ? ifHorizontal : ifVertical;

        private void SetPos(BindableObject bindable, int row, int column)
        {
            Grid.SetRow(bindable, row);
            Grid.SetColumn(bindable, column);
        }
    }

    /*public class CrunchKeyboard : AbsoluteLayout, IEnumerable<Key>, IKeyboard
    {
        //public event Crunch.Mobile.KeystrokeEventHandler KeyPress;
        public KeystrokeEventHandler Typed { get; set; }

        public event SpecialKeyEventHandler<Key> LongKeyPress;

        private static readonly Key EXP = new Key("x\u207F", "^");
        private static readonly Key SQRD = new Key("x\u00B2", "^2");
        private static readonly Key DIV = new Key("\u00F7", "/");
        private static readonly Key LOG = new Key("log", "log_");
        private static readonly Key SQRT = new Key("\u221A", "√");
        private static readonly Key MULT = new Key("\u00D7", "*");
        private static readonly Key TEN = new Key("10\u207F", "10^");
        private static readonly Key PI = new Key("\u03C0");

        private readonly Key[][] Keys = new Key[][]
        {
            new Key[] { "sin",  TEN,    EXP,    "7",    "8",    "9",    DIV },
            new Key[] { "cos",  LOG,    SQRD,   "4",    "5",    "6",    MULT },
            new Key[] { "tan",  "ln",   SQRT,    "1",    "2",    "3",    "-" },
            new Key[] { PI,     "e",    "x",    "0",    ".",    "()",   "+" }
        };

        public View ExampleKey => Keypad.Children[0];

        public readonly double SPACING = 6;
        private readonly int MAX_BUTTON_SIZE = 75;
        private readonly double PERMANENT_KEYS_INCREASE = 1.25;
        private readonly int MIN_COLUMNS = 4;

        public int Rows => Keys.Length + Orient(0, 1);
        public double Columns => Orient((ShowingFullKeyboard ? Keys[0].Length : MIN_COLUMNS) + PERMANENT_KEYS_INCREASE, MIN_COLUMNS);

        private ScrollView Scroll;
        private Grid Keypad;
        private AbsoluteLayout Mask;

        private StackLayout Parentheses;
        private StackLayout PermanentKeys;
        private StackLayout LRArrowKeys;
        private StackLayout UDArrowKeys;

        //white down-pointing triangle
        private LongClickableButton Dock = Device.Idiom == TargetIdiom.Tablet ? new Key("\u25BD", Key.DOCK) : new LongClickableButton();

        public CrunchKeyboard()
        {
            DescendantAdded += (sender, e) =>
            {
                Key key = e.Element as Key;

                if (key != null)
                {
                    if (key is CursorKey)
                    {
                        key.Clicked += (sender1, e1) => KeyboardManager.MoveCursor((sender1 as CursorKey).Key);
                    }
                    else if (key.Output != Key.DOCK)
                    {
                        key.Clicked += (sender1, e1) => Typed?.Invoke((sender1 as Key).Output);
                    }

                    key.LongClick += (sender1, e1) => LongKeyPress?.Invoke(sender1 as Key);
                }
            };

            //StackLayout keyboard = new StackLayout() { HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, Spacing = 0 };

            Scroll = new ScrollView()
            {
                Orientation = ScrollOrientation.Horizontal
            };
            Keypad = new Grid()
            {
                ColumnSpacing = SPACING,
                RowSpacing = SPACING
            };

            for (int i = 0; i < Keys.Length; i++)
            {
                for (int j = 0; j < Keys[i].Length; j++)
                {
                    View view = Keys[i][j];

                    if (Keys[i][j].Text == "()")
                    {
                        view = Parentheses = new StackLayout() { Orientation = StackOrientation.Horizontal, Spacing = SPACING };
                        Parentheses.Children.Add(new Key("("));
                        Parentheses.Children.Add(new Key(")"));
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

            Scroll.Content = Keypad;

            PermanentKeys = new StackLayout()
            {
                Orientation = StackOrientation.Vertical,
                Spacing = SPACING,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand
            };
            //PermanentKeys.Children.Add(new LongClickableButton());
            PermanentKeys.Children.Add(new Key("DEL", KeyboardManager.BACKSPACE.ToString()));
            PermanentKeys.Children.Add(Dock);

            LRArrowKeys = new StackLayout()
            {
                Orientation = StackOrientation.Horizontal,
                Spacing = SPACING
            };
            LRArrowKeys.Children.Add(new CursorKey("\u27E8", KeyboardManager.CursorKey.Left)); //mathematical left angle bracket
            LRArrowKeys.Children.Add(new CursorKey("\u27E9", KeyboardManager.CursorKey.Right)); //mathematical right angle bracket
            LRArrowKeys.Children[0].SizeChanged += (sender, e) =>
            {
                double size = Math.Floor(33 * Math.Max((sender as View).Height, (sender as View).Width) / 75);
                UP.FontSize = size;
                DOWN.FontSize = size;
            };

            UDArrowKeys = new StackLayout()
            {
                Orientation = StackOrientation.Vertical,
                Spacing = SPACING
            };
            UDArrowKeys.Children.Add(new CursorKey("", KeyboardManager.CursorKey.Up) { TextColor = Color.Black });
            UDArrowKeys.Children.Add(new CursorKey("", KeyboardManager.CursorKey.Down) { TextColor = Color.Black });

            Mask = new AbsoluteLayout()
            {
                IsVisible = false,
                BackgroundColor = Color.Gray,
                Opacity = 0.875
            };

            Children.Add(PermanentKeys);
            Children.Add(Scroll);
            Children.Add(UP);
            Children.Add(DOWN);
            Children.Add(Mask);

            SizeChanged += (sender, e) =>
            {
                LRArrowKeys.Remove();
                UDArrowKeys.Remove();

                if (IdealOrientation)
                {
                    PermanentKeys.Children[0].HeightRequest = ButtonWidth(PermanentKeys.Height, 4);
                    PermanentKeys.Children[PermanentKeys.Children.Count - 1].HeightRequest = ButtonWidth(PermanentKeys.Height, 4);
                    UDArrowKeys.Children.Insert(1, LRArrowKeys);
                }
                else
                {
                    PermanentKeys.Children[0].WidthRequest = ButtonWidth(PermanentKeys.Width, 4);
                    PermanentKeys.Children[PermanentKeys.Children.Count - 1].WidthRequest = ButtonWidth(PermanentKeys.Width, 4);
                    LRArrowKeys.Children.Insert(1, UDArrowKeys);
                }
                PermanentKeys.Children.Insert(1, Orient(UDArrowKeys, LRArrowKeys));
            };
            foreach (View v in LRArrowKeys.Children)
            {
                v.SizeChanged += (sender, e) =>
                {
                    v.WidthRequest = ButtonWidth(ButtonSize * Orient(PERMANENT_KEYS_INCREASE, 1), 2);
                    v.HeightRequest = ButtonSize;
                };
            }
            foreach (View v in UDArrowKeys.Children)
            {
                v.SizeChanged += (sender, e) =>
                {
                    v.WidthRequest = ButtonSize;
                    v.HeightRequest = ButtonWidth(ButtonSize, 2);
                };
            }

            Settings.KeyboardChanged += (e) =>
            {
                InvalidateLayout();
            };
        }

        public void ClearOverlay() => Mask.Children.Clear();
        public void Overlay(View view, Rectangle bounds, AbsoluteLayoutFlags flags = AbsoluteLayoutFlags.None) => Mask.Children.Add(view, bounds, flags);

        public void MaskKeys(bool onOrOff) => Mask.IsVisible = onOrOff;

        public IEnumerator<Key> GetEnumerator()
        {
            for (int i = 0; i < Keys.Length; i++)
            {
                for (int j = 0; j < Keys[i].Length; j++)
                {
                    if (Keys[i][j].Text != "()")
                    {
                        yield return Keys[i][j];
                    }
                }
            }
            foreach (Layout<View> layout in new Layout<View>[] { PermanentKeys, LRArrowKeys, UDArrowKeys, Parentheses })
            {
                foreach (View v in layout.Children)
                {
                    if (v is Key)
                    {
                        yield return v as Key;
                    }
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Enable() => IsVisible = true;
        public void Disable() => IsVisible = false;

        public bool ShowingFullKeyboard => Settings.ShouldShowFullKeyboard && CanShowFullKeyboard;

        public double ButtonSize { get; private set; }
        private bool CanShowFullKeyboard = false;

        private bool IdealOrientation;
        private T Orient<T>(T ifHorizontal, T ifVertical) => IdealOrientation ? ifHorizontal : ifVertical;

        protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
        {
            View root = this.Parent<TouchScreen>();
            widthConstraint = root.Width;
            heightConstraint = root.Height;

            CanShowFullKeyboard = (Keys[0].Length + PERMANENT_KEYS_INCREASE) * MAX_BUTTON_SIZE * Keys.Length * MAX_BUTTON_SIZE < widthConstraint * heightConstraint / 2;
            IdealOrientation = CanShowFullKeyboard || heightConstraint > widthConstraint;

            double constraint = Orient(Columns, Rows);
            ButtonSize = CanShowFullKeyboard ? MAX_BUTTON_SIZE : (Orient(widthConstraint, heightConstraint) - SPACING * ((int)constraint - 1)) / constraint;

            return new SizeRequest(new Size(PaddedButtonsWidth(Columns), PaddedButtonsWidth(Rows)));
        }

        private double PaddedButtonsWidth(double numButtons) => ButtonSize * numButtons + SPACING * ((int)numButtons - 1);

        private double ButtonWidth(double spaceConstraint, int numButtons) => (spaceConstraint - SPACING * (numButtons - 1)) / numButtons;

        private readonly Label UP = new Label
        {
            Text = "\u27E8", //mathematical left angle bracket
            Rotation = 90,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center,
            InputTransparent = true,
            TextColor = Color.Black,
        };
        private readonly Label DOWN = new Label
        {
            Text = "\u27E9", //mathematical right angle bracket
            Rotation = 90,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center,
            InputTransparent = true,
            TextColor = Color.Black,
        };

        protected override void LayoutChildren(double x, double y, double width, double height)
        {
            //base.LayoutChildren(x, y, width, height);

            double middle = Orient(PaddedButtonsWidth(Columns - PERMANENT_KEYS_INCREASE), PaddedButtonsWidth(Rows - 1));
            
            LayoutChildIntoBoundingRegion(
                Keypad.Parent as VisualElement,
                new Rectangle(
                    new Point(0, 0),
                    Orient(
                        new Size(middle, height),
                        new Size(width, middle)
                    )
                )
            );
            LayoutChildIntoBoundingRegion(
                PermanentKeys,
                Orient(
                    new Rectangle(middle + SPACING, 0, ButtonSize * PERMANENT_KEYS_INCREASE, height),
                    new Rectangle(0, middle + SPACING, width, ButtonSize)
                )
            );
            LayoutChildIntoBoundingRegion(Mask, new Rectangle(0, 0, width, height));

            // Temporary fix to display text for up and down arrow keys
            double halfButton = ButtonWidth(ButtonSize, 2);
            LayoutChildIntoBoundingRegion(
                UP,
                Orient(
                    new Rectangle(middle + SPACING + ButtonSize * PERMANENT_KEYS_INCREASE / 2 - halfButton / 2, ButtonSize + SPACING + halfButton / 2 - ButtonSize * PERMANENT_KEYS_INCREASE / 2, halfButton, ButtonSize * PERMANENT_KEYS_INCREASE),
                    new Rectangle(ButtonSize * 2 + SPACING * 2 + halfButton / 2 - ButtonSize / 2, middle + SPACING - ButtonSize / 2 + halfButton / 2, halfButton, ButtonSize)
                    ));
            LayoutChildIntoBoundingRegion(
                DOWN,
                Orient(
                    new Rectangle(middle + SPACING + ButtonSize * PERMANENT_KEYS_INCREASE / 2 - halfButton / 2, ButtonSize * 2 + SPACING * 3 + halfButton + halfButton / 2 - ButtonSize * PERMANENT_KEYS_INCREASE / 2, halfButton, ButtonSize * PERMANENT_KEYS_INCREASE),
                    new Rectangle(ButtonSize * 2 + SPACING * 2 + halfButton / 2 - ButtonSize / 2, middle + SPACING * 2 - ButtonSize / 2 + halfButton / 2 + halfButton, halfButton, ButtonSize)
                    ));

            foreach (View v in Parentheses.Children)
            {
                v.WidthRequest = ButtonWidth(ButtonSize, 2);
            }

            Keypad.WidthRequest = PaddedButtonsWidth(Keys[0].Length);
            Scroll.ScrollToAsync(Keypad, ScrollToPosition.End, false);
            PermanentKeys.Orientation = Orient(StackOrientation.Vertical, StackOrientation.Horizontal);
        }
    }*/
}