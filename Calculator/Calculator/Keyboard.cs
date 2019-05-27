using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using Xamarin.Forms;
using Xamarin.Forms.Extensions;

namespace Calculator
{
    public delegate void KeyPressEventHandler(string output);

    public class Key : LongClickableButton
    {
        public static readonly string LEFT = "left";
        public static readonly string RIGHT = "right";
        public static readonly string UP = "up";
        public static readonly string DOWN = "down";
        public static readonly string DELETE = "delete";
        public static readonly string DOCK = "dock";

        private string Basic;

        public Key(string display, string basic = "")
        {
            Padding = new Thickness(0);
            Text = display;
            Basic = basic;
        }

        public string Output => Basic == "" ? Text : Basic;

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            if (Text.Length > 1)
            {
                FontSize = Math.Floor(33 * Width / 75 * 5 / (4 + Text.Length));
            }
            else
            {
                FontSize = Math.Floor(33 * Math.Max(Height, Width) / 75);
            }
        }

        public static implicit operator Key(string s) => new Key(s);
    }

    public class Keyboard : AbsoluteLayout, IEnumerable<Key>
    {
        public event KeyPressEventHandler KeyPress;

        private static readonly Key EXP = new Key("x\u207F", "^");
        private static readonly Key SQR = new Key("x\u00B2", "^2");
        private static readonly Key DIV = new Key("\u00F7", "/");
        private static readonly Key LOG = new Key("log", "log_");
        private static readonly Key SQRT = new Key("\u221A", "sqrt");
        private static readonly Key MULT = new Key("\u00D7", "*");
        private static readonly Key TEN = new Key("10\u207F", "10^");
        private static readonly Key PI = new Key("\u03C0");

        private readonly Key[][] Keys = new Key[][]
        {
            new Key[] { "sin",  EXP,    SQR,    "7",    "8",    "9",    DIV },
            new Key[] { "cos",  LOG,    SQRT,   "4",    "5",    "6",    MULT },
            new Key[] { "tan",  "ln",   TEN,    "1",    "2",    "3",    "-" },
            new Key[] { PI,     "e",    "x",    "0",    ".",    "()",   "+" }
        };

        private readonly int MAX_BUTTON_SIZE = 75;
        private readonly double PERMANENT_KEYS_INCREASE = 1.25;
        private readonly int MIN_COLUMNS = 4;
        private readonly double BUTTON_SPACING = 6;

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

        public Keyboard()
        {
            DescendantAdded += (sender, e) =>
            {
                if (e.Element is Key)
                {
                    (e.Element as Key).Clicked += (sender1, e1) => KeyPress?.Invoke((sender1 as Key).Output);
                }
            };

            //StackLayout keyboard = new StackLayout() { HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, Spacing = 0 };

            Scroll = new ScrollView()
            {
                Orientation = ScrollOrientation.Horizontal
            };
            Keypad = new Grid()
            {
                ColumnSpacing = BUTTON_SPACING,
                RowSpacing = BUTTON_SPACING
            };

            for (int i = 0; i < Keys.Length; i++)
            {
                for (int j = 0; j < Keys[i].Length; j++)
                {
                    View view = Keys[i][j];

                    if (Keys[i][j].Text == "()")
                    {
                        view = Parentheses = new StackLayout() { Orientation = StackOrientation.Horizontal, Spacing = BUTTON_SPACING };
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
                Spacing = BUTTON_SPACING,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand
            };
            //PermanentKeys.Children.Add(new LongClickableButton());
            PermanentKeys.Children.Add(new Key("DEL", Key.DELETE));
            PermanentKeys.Children.Add(Dock);

            LRArrowKeys = new StackLayout()
            {
                Orientation = StackOrientation.Horizontal,
                Spacing = BUTTON_SPACING
            };
            LRArrowKeys.Children.Add(new Key("\u27E8", Key.LEFT)); //mathematical left angle bracket
            LRArrowKeys.Children.Add(new Key("\u27E9", Key.RIGHT)); //mathematical right angle bracket
            LRArrowKeys.Children[0].SizeChanged += (sender, e) =>
            {
                double size = Math.Floor(33 * Math.Max((sender as View).Height, (sender as View).Width) / 75);
                UP.FontSize = size;
                DOWN.FontSize = size;
            };

            UDArrowKeys = new StackLayout()
            {
                Orientation = StackOrientation.Vertical,
                Spacing = BUTTON_SPACING
            };
            UDArrowKeys.Children.Add(new Key("", Key.UP) { TextColor = Color.Black });
            UDArrowKeys.Children.Add(new Key("", Key.DOWN) { TextColor = Color.Black });

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

        public bool ShowingFullKeyboard => ShouldShowFullKeyboard && CanShowFullKeyboard;
        public bool ShouldShowFullKeyboard = true;

        private double ButtonSize;
        private bool CanShowFullKeyboard = false;

        private bool IdealOrientation;
        private T Orient<T>(T ifHorizontal, T ifVertical) => IdealOrientation ? ifHorizontal : ifVertical;

        protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
        {
            View root = this.Parent<View>();
            widthConstraint = root.Width;
            heightConstraint = root.Height;

            CanShowFullKeyboard = (Keys[0].Length + PERMANENT_KEYS_INCREASE) * MAX_BUTTON_SIZE * Keys.Length * MAX_BUTTON_SIZE < widthConstraint * heightConstraint / 2;
            IdealOrientation = CanShowFullKeyboard || heightConstraint > widthConstraint;

            double constraint = Orient(Columns, Rows);
            ButtonSize = ShowingFullKeyboard ? MAX_BUTTON_SIZE : (Orient(widthConstraint, heightConstraint) - BUTTON_SPACING * ((int)constraint - 1)) / constraint;

            return new SizeRequest(new Size(PaddedButtonsWidth(Columns), PaddedButtonsWidth(Rows)));
        }

        private double PaddedButtonsWidth(double numButtons) => ButtonSize * numButtons + BUTTON_SPACING * ((int)numButtons - 1);

        private double ButtonWidth(double spaceConstraint, int numButtons) => (spaceConstraint - BUTTON_SPACING * (numButtons - 1)) / numButtons;

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
                    new Rectangle(middle + BUTTON_SPACING, 0, ButtonSize * PERMANENT_KEYS_INCREASE, height),
                    new Rectangle(0, middle + BUTTON_SPACING, width, ButtonSize)
                )
            );
            LayoutChildIntoBoundingRegion(Mask, new Rectangle(0, 0, width, height));

            // Temporary fix to display text for up and down arrow keys
            double halfButton = ButtonWidth(ButtonSize, 2);
            LayoutChildIntoBoundingRegion(
                UP,
                Orient(
                    new Rectangle(middle + BUTTON_SPACING + ButtonSize * PERMANENT_KEYS_INCREASE / 2 - halfButton / 2, ButtonSize + BUTTON_SPACING + halfButton / 2 - ButtonSize * PERMANENT_KEYS_INCREASE / 2, halfButton, ButtonSize * PERMANENT_KEYS_INCREASE),
                    new Rectangle(ButtonSize * 2 + BUTTON_SPACING * 2 + halfButton / 2 - ButtonSize / 2, middle + BUTTON_SPACING - ButtonSize / 2 + halfButton / 2, halfButton, ButtonSize)
                    ));
            LayoutChildIntoBoundingRegion(
                DOWN,
                Orient(
                    new Rectangle(middle + BUTTON_SPACING + ButtonSize * PERMANENT_KEYS_INCREASE / 2 - halfButton / 2, ButtonSize * 2 + BUTTON_SPACING * 3 + halfButton + halfButton / 2 - ButtonSize * PERMANENT_KEYS_INCREASE / 2, halfButton, ButtonSize * PERMANENT_KEYS_INCREASE),
                    new Rectangle(ButtonSize * 2 + BUTTON_SPACING * 2 + halfButton / 2 - ButtonSize / 2, middle + BUTTON_SPACING * 2 - ButtonSize / 2 + halfButton / 2 + halfButton, halfButton, ButtonSize)
                    ));

            foreach (View v in Parentheses.Children)
            {
                v.WidthRequest = ButtonWidth(ButtonSize, 2);
            }

            Keypad.WidthRequest = PaddedButtonsWidth(Keys[0].Length);
            Scroll.ScrollToAsync(Keypad, ScrollToPosition.End, false);
            PermanentKeys.Orientation = Orient(StackOrientation.Vertical, StackOrientation.Horizontal);
        }
    }
}
