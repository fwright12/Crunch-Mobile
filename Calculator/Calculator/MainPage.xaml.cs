using System;
using Xamarin.Forms;
using System.Linq;
using System.Collections.ObjectModel;
using System.IO;

using Crunch.GraFX;

namespace Calculator
{
    public partial class MainPage : ContentPage
    {
        public static MainPage VisiblePage => visiblePage;
        private static MainPage visiblePage;

        public Layout Canvas => canvas;
        public TouchScreen Page => page;

        public static int cursorWidth;
        public static double textHeight;
        public static int textWidth;
        public static int TextSize = 40;

        public static bool isTouchingCanvas = false;
        public static bool keyboardDocked = true;

        private BoxView phantomCursor;

        //How much extra space is in the lower right
        private int padding = 100;

        public static readonly double fontSize = 33;

        //Color 560297

        private static string tips
        {
            get
            {
                if (Device.Idiom == TargetIdiom.Tablet)
                {
                    return "Press anywhere on the screen to start a new calculation. Long press DEL to clear the " +
                        "canvas. Long press anywhere else on the keyboard to move the cursor. Press the dock button" +
                        "(bottom right corner) to detach the keyboard, then drag to move the keyboard.";
                }
                else
                {
                    return "The top half of the page is the canvas, where you can do calculations. Press anywhere to " +
                        "start a new calculation.\n" +
                        "The bottom half of the page is the keyboard. Long press DEL to clear the canvas. Long press " +
                        "anywhere else on the keyboard to move the cursor. Scroll the keyboard for more operations.";
                }
            }
        }

        public MainPage()
        {
            InitializeComponent();

            //Application.Current.Properties["test"] = 234;

            Button dock;
            if (Device.Idiom == TargetIdiom.Tablet)
            {
                dock = new DockButton();

                page.Children.RemoveAt(1);
                phantomCursorField.Children.Add(keyboardContainer);
                keyboardContainer.MoveTo(-1000, -1000);

                dock.Text = "▽";
                dock.Clicked += delegate
                {
                    print.log("dock clicked");
                    DockKeyboard(!keyboardDocked);
                };
                (dock as DockButton).Touch += (point, state) =>
                {
                    if (state == TouchState.Moving)
                    {
                        DockKeyboard(false);
                        keyboardContainer.BeginDrag(phantomCursorField.Bounds);
                        
                    }
                };

                canvasScroll.Scrolled += delegate
                {
                    Focus_SizeChanged(null, null);
                };
            }
            else
            {
                dock = new Button();
            }
            permanentKeys.Children.Add(dock);

            //ad.IsVisible = false;
            phantomCursorField.SizeChanged += delegate
            {
                ad.TranslationX = (phantomCursorField.Width - ad.Width) / 2;
            };

            canvas.Touch += AddEquation;

            //Keyboard formatting
            buttonFormat(keyboard);

            //Measuring for cursor
            Label l = new Label();
            l.FontSize = fontSize;
            page.Children.Add(l);
            l.SizeChanged += delegate
            {
                textHeight = l.Height;
                page.Children.Remove(l);

                Testing.Test(canvas);
            };

            visiblePage = this;
            Drag.Screen = page;

            //Button hookup
            left.Clicked += (sender, e) => SoftKeyboard.Left();
            right.Clicked += (sender, e) => SoftKeyboard.Right();
            delete.Clicked += (sender, e) => SoftKeyboard.Delete();
            delete.LongClick += ClearCanvas;
            info.Clicked += (sender, e) => DisplayAlert("About Crunch",
                "Thank you for using Crunch!\n\n" +
                "A few tips about how to navigate the app: " + tips +
                "\nClick the answer to see alternate formats. Touch and drag the equals sign to move the equation " +
                "on the canvas.\n\n" +
                "If you find any bugs, please report them to GreenMountainLabs802@gmail.com. The more information " +
                "you can provide (what you did to cause the error, screenshots, etc.) the easier it will be to fix.\n\n" +
                "If you enjoy using Crunch, please rate it on the app store. Ratings help with visibility, so other " +
                "people can find Crunch.\n\n" +
                "Please also email me with any ideas you have about how the app can be improved, or features you " +
                "would like to see in the future. I'm open to suggestions!",
                "Dismiss");

            //Phantom cursor stuff
            phantomCursor = new CursorView() { Color = Color.Red, IsVisible = false };
            phantomCursorField.Children.Add(phantomCursor);

            FixDynamicLag("");
            print.log("main page constructor finished");
        }

        void FixDynamicLag(object o) => print.log(o as dynamic);

        private double width = 0;
        private double height = 0;

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            if (this.width != width || this.height != height)
            {
                this.width = width;
                this.height = height;

                padding = (int)Math.Min(page.Width, page.Height);

                keyboardScroll.ScrollToAsync(keypad, ScrollToPosition.End, false);
                layoutKeyboard();
            }
        }

        private int rows = 4;
        private int columns = 7;
        private readonly int columnsOnScreen = 5;
        private readonly int undockedButtonSize = 75;
        private readonly double permanentKeysIncrease = 1.25;

        private void layoutKeyboard()
        {
            //Buttons are squares with dimensions equal to 1/5 of available space
            double buttonSize = orient(page.Width, page.Height) / columnsOnScreen;
            if (Device.Idiom == TargetIdiom.Tablet)
            {
                buttonSize = undockedButtonSize;
            }

            //Right and left sized individually so both fit in one column
            right.WidthRequest = buttonSize * permanentKeysIncrease / 2;
            left.WidthRequest = buttonSize * permanentKeysIncrease / 2;

            keypad.ColumnDefinitions.Clear();
            keypad.RowDefinitions.Clear();

            double height = buttonSize;
            double width = buttonSize;

            if (Device.Idiom == TargetIdiom.Phone)
            {
                page.Orientation = orient(StackOrientation.Vertical, StackOrientation.Horizontal);
                keyboard.Orientation = orient(StackOrientation.Horizontal, StackOrientation.Vertical);
                permanentKeys.Orientation = orient(StackOrientation.Vertical, StackOrientation.Horizontal);

                if (page.Orientation == StackOrientation.Vertical)
                {
                    //Permanent keys are slightly wider than rest of buttons
                    permanentKeys.WidthRequest = buttonSize * permanentKeysIncrease;

                    //Recalculate button size
                    width = (page.Width - buttonSize * permanentKeysIncrease) / (columnsOnScreen - 1);
                    height = Math.Min(buttonSize, page.Height / 8);

                    phantomCursorField.HeightRequest = page.Height - height * 4 - page.Spacing;
                }
                else if (page.Orientation == StackOrientation.Horizontal)
                {
                    width = Math.Min(buttonSize, page.Width / 8);

                    //Permanent keys equally sized using space no occupied by left and right
                    double temp = (width * (columnsOnScreen - 1) - buttonSize * permanentKeysIncrease) / (rows - 1);
                    foreach (View v in permanentKeys.Children)
                    {
                        if (v is Button)
                        {
                            v.WidthRequest = temp;
                        }
                    }

                    phantomCursorField.WidthRequest = page.Width - width * 4 - page.Spacing;
                    keyboardContainer.WidthRequest = width * 4;
                }
            }

            //Button width
            for (int i = 0; i < columns - 1; i++)
            {
                keypad.ColumnDefinitions.Add(new ColumnDefinition { Width = width });
            }
            foreach(Button b in parentheses.Children)
            {
                b.WidthRequest = width / 2;
            }

            //Button height
            for (int i = 0; i < rows; i++)
            {
                keypad.RowDefinitions.Add(new RowDefinition { Height = height });
            }
            foreach(View v in permanentKeys.Children)
            {
                v.HeightRequest = height;
            }
        }

        private void buttonFormat(Layout parent)
        {
            for (int i = 0; i < parent.Children.Count; i++)
            {
                View v = parent.Children[i] as View;

                if (v is Button)
                {
                    Button b = v as Button;

                    //Dynamic sizing for some buttons
                    if (b.Parent == arrowKeys)
                    {
                        b.SizeChanged += delegate { b.FontSize = Math.Floor(33 * b.Height / 75); };
                    }
                    else if (b.Text?.Length > 1) //b == info || b == delete)
                    {
                        b.SizeChanged += delegate { b.FontSize = Math.Floor(33 * b.Width / 75 * 5 / (4 + b.Text.Length)); };
                    }
                    else
                    {
                        b.SizeChanged += delegate { b.FontSize = Math.Floor(33 * Math.Max(b.Height, b.Width) / 75); };
                    }

                    if (b.Parent == keypad || b.Parent == parentheses)
                    {
                        b.Clicked += delegate
                        {
                            SoftKeyboard.Type(b.StyleId ?? b.Text);
                            if (keypad.Children.IndexOf(b) % (columns - 1) <= columns - columnsOnScreen)
                            {
                                keyboardScroll.ScrollToAsync(keypad, ScrollToPosition.End, false);
                            }
                        };
                        (b as LongClickableButton).LongClick += EnterCursorMode;
                    }
                }
                else if (v is Layout)
                {
                    buttonFormat(v as Layout);
                }
            }
        }

        private T orient<T>(T verticalOption, T horizontalOption)
        {
            if (height > width)
            {
                return verticalOption;
            }
            return horizontalOption;
        }

        private void DockKeyboard(bool isDocked)
        {
            keyboardDocked = isDocked;

            print.log("dock clicked", keyboardDocked);
            if (keyboardDocked)
            {
                Focus_SizeChanged(null, null);
            }
        }

        private Equation setFocus(Equation e)
        {
            if (Equation.Focus != null)
            {
                Equation.Focus.LayoutChanged -= Focus_SizeChanged;
                if (Equation.Focus.LHS.ChildCount == 0)
                {
                    Equation.Focus.Remove();
                }
            }

            if (e != null)
            {
                Equation.Focus = e;
                Equation.Focus.LayoutChanged += Focus_SizeChanged;
            }

            return Equation.Focus;
        }

        private void Focus_SizeChanged(object sender, EventArgs e)
        {
            if (Device.Idiom == TargetIdiom.Tablet && keyboardDocked)
            {
                StackLayout temp = Equation.Focus as StackLayout;
                keyboardContainer.MoveTo(temp.X - canvasScroll.ScrollX, temp.Y + Equation.Focus.Height - canvasScroll.ScrollY);
            }
        }

        private void AddEquation(Point point, TouchState state)
        {
            if (state != TouchState.Up)
            {
                return;
            }

            Equation equation = setFocus(new Equation());
            SoftKeyboard.MoveCursor(equation.LHS);

            equation.SizeChanged += delegate
            {
                Point p = point.Add(new Point(equation.Width, equation.Height));


                if (canvas.Width - p.X < padding)
                {
                    canvas.WidthRequest = p.X + padding;
                }
                if (canvas.Height - p.Y < padding)
                {
                    canvas.HeightRequest = p.Y + padding;
                }
            };

            canvas.Children.Add(equation, point);
        }

        private async void ClearCanvas()
        {
            if (await DisplayAlert("Wait!", "Are you sure you want to clear the canvas?", "Yes", "No"))
            {
                canvas.Children.Clear();
                canvas.WidthRequest = (canvas.Parent as View).Width;
                canvas.HeightRequest = (canvas.Parent as View).Height;
                if (Device.Idiom == TargetIdiom.Tablet)
                {
                    keyboardContainer.MoveTo(-1000, -1000);
                    keyboardDocked = true;
                }
            }
        }

        private void SetCursorMode(bool onOrOff)
        {
            phantomCursor.IsVisible = onOrOff;
            keyboardMask.IsVisible = onOrOff;
        }

        private void EnterCursorMode()
        {
            if (Device.Idiom == TargetIdiom.Tablet && !keyboardDocked)
            {
                phantomCursor.MoveTo(TouchScreen.LastDownEvent.X, keyboardContainer.Y - SoftKeyboard.Cursor.Height);
            }
            else
            {
                Point temp = SoftKeyboard.Cursor.PositionOn(canvas).Add(new Point(-canvasScroll.ScrollX, -canvasScroll.ScrollY));
                if (temp.X >= 0 && temp.X <= canvasScroll.Width && temp.Y >= 0 && temp.Y <= canvas.Height)
                {
                    phantomCursor.MoveTo(temp);
                }
            }

            phantomCursor.HeightRequest = SoftKeyboard.Cursor.Height;

            SetCursorMode(true);
            phantomCursor.BeginDrag(phantomCursorField.Bounds, 2);
            page.Touch += MoveCursor;
        }

        private void ExitCursorMode()
        {
            //Climb up to the top of the tree structure
            Element root = SoftKeyboard.Cursor;
            while (!(root is Equation))
            {
                root = root.Parent;
            }

            //Focus has changed
            if (root != Equation.Focus)
            {
                setFocus(root as Equation);
            }

            SetCursorMode(false);
            page.Touch -= MoveCursor;
        }

        private System.Threading.Thread thread;
        private int shouldScrollX => (int)Math.Truncate(phantomCursor.X / (canvasScroll.Width - phantomCursor.Width) * 2 - 1);
        private int shouldScrollY => (int)Math.Truncate(phantomCursor.Y / (canvasScroll.Height - phantomCursor.Height) * 2 - 1);
        private readonly double scrollSpeed = 0.025;
        private double preciseScrollX, preciseScrollY;

        private void scrollCanvas()
        {
            preciseScrollX = canvasScroll.ScrollX;
            preciseScrollY = canvasScroll.ScrollY;
            while (shouldScrollX + shouldScrollY != 0 && phantomCursor.IsVisible)
            {
                preciseScrollX += shouldScrollX * scrollSpeed;
                preciseScrollY += shouldScrollY * scrollSpeed;
                canvasScroll.ScrollToAsync(preciseScrollX, preciseScrollY, false);
            }
        }

        private void MoveCursor(Point point, TouchState state)
        {
            if (state == TouchState.Up)
            {
                ExitCursorMode();
            }
            else
            {

                if ((thread == null || !thread.IsAlive) && shouldScrollX + shouldScrollY != 0)
                {
                    thread = new System.Threading.Thread(scrollCanvas);
                    thread.Start();
                }

                //Get the coordinates of the cursor relative to the entire screen
                Point loc = new Point(canvasScroll.ScrollX + phantomCursor.X + phantomCursor.Width / 2, canvasScroll.ScrollY + phantomCursor.Y + phantomCursor.Height / 2);
                int leftOrRight = 0;
                View view = GetViewAt(canvas, loc, ref leftOrRight);

                if (view is Text || view is Expression)
                {
                    phantomCursor.HeightRequest = SoftKeyboard.Cursor.Height;

                    if (view.GetType() == typeof(Expression))
                    {
                        Expression e = view as Expression;
                        SoftKeyboard.MoveCursor(e, Math.Min(e.Children.Count, e.Children.Count * leftOrRight));
                    }
                    else if (view is Text)
                    {
                        SoftKeyboard.MoveCursor((view as Text).Parent, (view.Parent as Expression).IndexOf(view) + leftOrRight);
                    }
                }
            }
        }

        private View GetViewAt(Layout<View> parent, Point pos, ref int leftOrRight)
        {
            View ans = null;

            for (int i = 0; i < parent.Children.Count; i++)
            {
                View child = parent.Children[i];

                //Is the point inside the bounds that this child occupies?
                if (pos.X >= child.X && pos.X <= child.X + child.Width && pos.Y >= child.Y - child.Margin.Top + child.TranslationY && pos.Y <= child.Y + child.Height + child.TranslationY)
                {
                    //The child is a layout
                    if (child is Layout<View>)
                    {
                        Layout<View> layout = child as Layout<View>;
                        //bool isEditable = layout is Expression && (layout as Expression).Editable;

                        //First check if I'm on one of the ends of an Expression
                        if (layout.IsEditable() && ((layout.Padding.Left > 0 && pos.X <= layout.Padding.Left) || (layout.Padding.Right > 0 && pos.X >= layout.Width - layout.Padding.Right)))
                        {
                            ans = child;
                        }
                        //If not, see if I'm over one of this layout's children
                        else
                        {
                            ans = GetViewAt(layout, pos.Add(new Point(-child.X, -(child.Y + child.TranslationY))), ref leftOrRight);
                        }
                    }
                    else if (parent.IsEditable())
                    {
                        ans = child;
                    }

                    if (ans == child)
                    {
                        leftOrRight = (int)Math.Round((pos.X - ans.X) / ans.Width);
                    }
                    break;
                }
            }

            return ans;
        }
    }
}
 