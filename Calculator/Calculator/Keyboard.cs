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
        public static readonly string DELETE = "delete";
        public static readonly string DOCK = "dock";

        private string Basic;

        public Key(string display, string basic = "")
        {
            Text = display;
            Basic = basic;
        }

        public string Output => Basic == "" ? Text : Basic;

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            if (Text?.Length > 1)
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

        private readonly int MaxButtonSize = 75;
        private readonly double PermanentKeysIncrease = 1.25;
        private readonly int MinColumns = 4;

        public int Rows => Keys.Length + Orient(0, 1);
        public double Columns => Orient((ShowingFullKeyboard ? Keys[0].Length : MinColumns) + PermanentKeysIncrease, MinColumns);

        private ScrollView Scroll;
        private Grid Keypad;
        private AbsoluteLayout Mask;

        private StackLayout Parentheses;
        private StackLayout PermanentKeys;
        private StackLayout ArrowKeys;

        //white down-pointing triangle
        private LongClickableButton Dock => Device.Idiom == TargetIdiom.Tablet ? new Key("\u25BD", Key.DOCK) : new LongClickableButton();

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

            Scroll = new ScrollView() { Orientation = ScrollOrientation.Horizontal };
            Keypad = new Grid() { ColumnSpacing = 0, RowSpacing = 0 };

            for (int i = 0; i < Keys.Length; i++)
            {
                for (int j = 0; j < Keys[i].Length; j++)
                {
                    View view = Keys[i][j];

                    if (Keys[i][j].Text == "()")
                    {
                        view = Parentheses = new StackLayout() { Orientation = StackOrientation.Horizontal, Spacing = 0 };
                        Parentheses.Children.Add(new Key("("));
                        Parentheses.Children.Add(new Key(")"));
                    }
                    else
                    {
                        if (Keys[i].Length - j > MinColumns)
                        {
                            Keys[i][j].Clicked += delegate { Scroll.ScrollToAsync(Keypad, ScrollToPosition.End, false); };
                        }
                    }

                    Keypad.Children.Add(view, j, i);
                }
            }

            Scroll.Content = Keypad;

            PermanentKeys = new StackLayout() { Orientation = StackOrientation.Vertical, Spacing = 0, HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.FillAndExpand };
            PermanentKeys.Children.Add(new LongClickableButton());
            PermanentKeys.Children.Add(new Key("DEL", Key.DELETE));
            ArrowKeys = new StackLayout() { Orientation = StackOrientation.Horizontal, Spacing = 0, HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.FillAndExpand };
            ArrowKeys.Children.Add(new Key("\u27E8", Key.LEFT)); //mathematical left angle bracket
            ArrowKeys.Children.Add(new Key("\u27E9", Key.RIGHT)); //mathematical right angle bracket
            PermanentKeys.Children.Add(ArrowKeys);
            PermanentKeys.Children.Add(Dock);

            Mask = new AbsoluteLayout() { IsVisible = false, BackgroundColor = Color.Gray, Opacity = 0.875 };

            Children.Add(PermanentKeys);
            Children.Add(Scroll);
            Children.Add(Mask);
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
            foreach (Layout<View> layout in new Layout<View>[] { PermanentKeys, ArrowKeys, Parentheses })
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
            View root = this.Root<View>();
            widthConstraint = root.Width;
            heightConstraint = root.Height;

            CanShowFullKeyboard = (Keys[0].Length + PermanentKeysIncrease) * MaxButtonSize * Keys.Length * MaxButtonSize < widthConstraint * heightConstraint / 2;
            IdealOrientation = ShowingFullKeyboard || heightConstraint > widthConstraint;

            double constraint = Orient(Columns, Rows);
            ButtonSize = ShowingFullKeyboard ? MaxButtonSize : Orient(widthConstraint, heightConstraint) / constraint;
            return new SizeRequest(new Size(ButtonSize * Columns, ButtonSize * Rows));
        }

        protected override void LayoutChildren(double x, double y, double width, double height)
        {
            double middle = ButtonSize * Orient(Columns - PermanentKeysIncrease, Rows - 1);

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
                    new Rectangle(middle, 0, ButtonSize * PermanentKeysIncrease, height),
                    new Rectangle(0, middle, width, ButtonSize)
                )
            );
            LayoutChildIntoBoundingRegion(Mask, new Rectangle(0, 0, width, height));

            foreach (View v in PermanentKeys.Children)
            {
                if (v == ArrowKeys)
                {
                    continue;
                }

                if (IdealOrientation)
                {
                    v.HeightRequest = ButtonSize;
                }
                else
                {
                    v.WidthRequest = (ButtonSize * Columns - (ButtonSize * PermanentKeysIncrease)) / (PermanentKeys.Children.Count - 1);
                }
            }
            foreach (View v in ArrowKeys.Children)
            {
                v.WidthRequest = ButtonSize * PermanentKeysIncrease / 2;
            }
            foreach (View v in Parentheses.Children)
            {
                v.WidthRequest = ButtonSize / 2;
            }

            Keypad.WidthRequest = Keys[0].Length * ButtonSize;
            Scroll.ScrollToAsync(Keypad, ScrollToPosition.End, false);
            PermanentKeys.Orientation = Orient(StackOrientation.Vertical, StackOrientation.Horizontal);
        }
    }
}
