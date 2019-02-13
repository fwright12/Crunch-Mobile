using System;
using Xamarin.Forms;
using System.Linq;
using System.Collections.ObjectModel;
using System.IO;

using Crunch.GraFX;

namespace Calculator
{
    public delegate void LongClickEventArgs();
    public delegate void TouchEventArgs(Point point);
    public delegate void DoubleClick();

    public class TouchSpy : StackLayout
    {
        public event TouchEventArgs Touch;
        public void Touched(Point point) => Touch(point);
    }
    public class Canvas : RelativeLayout
    {
        public event TouchEventArgs Touch;
        public void Touched(Point point) => Touch(point);
    }
    public class LongClickableButton : Button
    {

    }
    public class Mask : StackLayout { }
    public class ScrollSpy : AbsoluteLayout
    {
        public Point TouchPos;
    }
    public class DockButton : Button { }
    public class BannerAd : View { }

    public partial class MainPage : ContentPage
    {
        public static MainPage VisiblePage => visiblePage;
        private static MainPage visiblePage;

        public Layout Canvas => canvas;
        public Layout Page => page;

        public static bool isTouchingCanvas = false;
        public static bool keyboardDocked = true;

        private BoxView phantomCursor;

        //How much extra space is in the lower right
        private int padding = 100;

        public static readonly double fontSize = 33;
        private static double buttonFontSize = -1;

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
                keyboardContainer.TranslationX = -1000;
                keyboardContainer.TranslationY = -1000;

                dock.Text = "▽";
                dock.Clicked += delegate
                {
                    DockKeyboard(!keyboardDocked);
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

            //Keyboard formatting
            buttonFormat(keyboard);

            //Measuring for cursor
            Label l = new Label();
            l.FontSize = fontSize;
            page.Children.Add(l);
            l.SizeChanged += delegate
            {
                Input.textHeight = l.Height;
                //cursor = new Cursor(true);
                page.Children.Remove(l);

                //test();
            };

            visiblePage = this;
            page.Touch += (point) => Drag.UpdatePosition(point);

            //print.log(";lakjsdflk;jasld;kfj;alskdfj", ((string)new Text()) == null);
            /*print.log(Crunch.Parse.Math("^3234+4^(6-85^"));
            print.log(Crunch.Parse.Math("(3)+(9^(4*(8)+1^5"));
            print.log(Crunch.Parse.Math("((1^3)-1^9)+(43^2*(6-9))-8+(234^4*(7))"));
            print.log(Crunch.Parse.Math("3^1^2^3^4-95)+7)+(4*(6-9))-8+(4*17)"));
            print.log(Crunch.Parse.Math("(4+3()2+1^8^0)(4)+(6)"));
            print.log(Crunch.Parse.Math("()4+()*8()"));
            throw new Exception();*/

            //Button hookup
            left.Clicked += (sender, e) => { SoftKeyboard.Left(); };
            right.Clicked += (sender, e) => { SoftKeyboard.Right(); };
            delete.Clicked += (sender, e) => { SoftKeyboard.Delete(); };
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

            //ℹ - &#8505;
            //🛈 - &#128712;

            //Phantom cursor stuff
            phantomCursor = new CursorView();
            phantomCursor.Color = Color.Red;
            phantomCursorField.Children.Add(phantomCursor);
            phantomCursor.IsVisible = false;

            FixDynamicLag("");
            Input.Started(this);
            print.log("main page constructor finished");
        }

        public static Point ScaleToPage(Point point) => new Point(point.X * VisiblePage.page.Width, point.Y * VisiblePage.page.Height);

        void test()
        {
            System.Collections.Generic.List<string> testcases = new System.Collections.Generic.List<string>();

            testcases.Add("sin(30)");
            testcases.Add("sin30");
            testcases.Add("sin5/4");
            testcases.Add("sin(30)");
            testcases.Add("si(30)");
            testcases.Add("s(30)");
            testcases.Add("in(30)");
            testcases.Add("sin(30)+cos(30)");
            testcases.Add("sin(cos(60))");
            testcases.Add("cossin60");
            testcases.Add("5(x+1)");
            testcases.Add("(x+1)5");
            testcases.Add("6sin30");
            testcases.Add("5/4sin30");
            testcases.Add("sin30cos30");
            testcases.Add("esin30");
            testcases.Add("e^2sin30+cos30e^2");

            /*testcases.Add("6-(3/2+4^(7-5))/(8^2*4)+2^(2+3)");
            testcases.Add("8/8/8+2^2^2");
            testcases.Add("6+(8-3)*3/(9/5)+2^2");
            testcases.Add("-1--4/5+2^-2+4/-5+5*-6");
            testcases.Add("-9*6");
            testcases.Add("6+-9");
            testcases.Add("6*-(1+2)");
            testcases.Add("-(1+2)");
            testcases.Add("6/-9");
            testcases.Add("(6+6)-9");
            testcases.Add("2*(6+6)-9");
            testcases.Add("(-5)");
            testcases.Add("2^-3");
            testcases.Add("2^-1^2");*/

            //testcases.Add("3+476576/9878-56^2876");

            /*testcases.Add("2^2");
            testcases.Add("7^24x*8");
            testcases.Add("e");
            testcases.Add("e*e");
            testcases.Add("e*π");
            testcases.Add("e+e");
            testcases.Add("e+π");
            testcases.Add("e^2");
            testcases.Add("e^2+π");
            testcases.Add("e^π+3");
            testcases.Add("e^(2+π)");
            testcases.Add("eπ+2");
            testcases.Add("5.3e");
            testcases.Add("5xe^2+4x^3e");
            testcases.Add("5x^3e^2+4x^3e");*/

            /*testcases.Add("5/3");
            testcases.Add("8/3/7");
            testcases.Add("8/(3/7)");
            testcases.Add("9/2/7/5");
            testcases.Add("(9/2)/(7/5)");
            testcases.Add("5/e");
            testcases.Add("e/3");
            testcases.Add("e/e");
            testcases.Add("e/π");
            testcases.Add("(e+π)/(π+e)");
            testcases.Add("(x+y)/(y+x)");*/

            /*testcases.Add("6*8");
            testcases.Add("6*8/5");
            testcases.Add("x*6");
            testcases.Add("x*x");
            testcases.Add("x*y");
            testcases.Add("yyy");
            testcases.Add("6x*3");
            testcases.Add("6x*3x");
            testcases.Add("6x*y");
            testcases.Add("6x*3y");
            testcases.Add("6x*1/2y*z^2");
            testcases.Add("x^2*x");
            testcases.Add("6x^2*y*5x");
            testcases.Add("6x^2*3x^5*y^7");
            testcases.Add("6x*y/z");
            testcases.Add("x*1/z");
            testcases.Add("(x+1)5");
            testcases.Add("5(x+1)");
            testcases.Add("(x+1)*x");
            testcases.Add("(x+1)*6x");
            testcases.Add("(x+1)*(x+2)");
            testcases.Add("(x+1)^2*(x+1)");
            testcases.Add("(x+1)^y*(x+1)^2y");
            testcases.Add("(x+1)*5/6");
            testcases.Add("(x+1)*y/z");*/

            /*testcases.Add("5+8");
            testcases.Add("5+8/3");
            testcases.Add("5/3+8/9");
            testcases.Add("5/3+8/7");
            testcases.Add("5+x/y");
            testcases.Add("5+x");
            testcases.Add("5/2+x");
            testcases.Add("5+x^2");
            testcases.Add("5+6x^2");
            testcases.Add("5+(x+6)");
            testcases.Add("5+(7/3+x^2)");
            testcases.Add("5+(x^2+x)");
            testcases.Add("x+x");
            testcases.Add("x+y");
            testcases.Add("x+(x^2+x)");
            testcases.Add("x+y/z");
            testcases.Add("5x+x");
            testcases.Add("5x+8x");
            testcases.Add("5x+8y");
            testcases.Add("5xy^2+8/3y^2x");
            testcases.Add("(x+1)+(y^2+4x+2)");
            testcases.Add("x/y+y/z");
            testcases.Add("x/y+z/y");*/

            /*testcases.Add("5");
            testcases.Add("5x");
            testcases.Add("1");
            testcases.Add("x");
            testcases.Add("-6*x");
            testcases.Add("-1*x");
            testcases.Add("-1");
            testcases.Add("-7");*/

            /*testcases.Add("x-5");
            testcases.Add("x^2+-6*x^2");
            testcases.Add("x^2+-1*x");
            testcases.Add("x^2--1*x");*/

            /*testcases.Add("5/8");
            testcases.Add("6/8");
            testcases.Add("8/2");
            testcases.Add("(-5)/8");
            testcases.Add("(-6)/8");
            testcases.Add("(-8)/2");
            testcases.Add("5/(-8)");
            testcases.Add("6/(-8)");
            testcases.Add("8/(-2)");
            testcases.Add("(-5)/(-8)");
            testcases.Add("(-6)/(-8)");
            testcases.Add("(-8)/(-2)");
            testcases.Add("5*8/3");
            testcases.Add("5*8/3.5");
            testcases.Add("5.5*8/3");
            testcases.Add("(-8)/(-2)");*/

            /*testcases.Add("6/x");
            testcases.Add("6/(6x)");
            testcases.Add("6/(5x^2)");
            testcases.Add("6/(5x+3y)");
            testcases.Add("x/6");
            testcases.Add("x/y");
            testcases.Add("x/x");
            testcases.Add("x/(6x)");
            testcases.Add("3/(6x)");
            testcases.Add("x/(5x+3x^2)");
            testcases.Add("x/(6x+3x^2)");
            testcases.Add("(6x)/6");
            testcases.Add("(6x)/x");
            testcases.Add("(6x)/(6x)");
            testcases.Add("(6x)/(5x)");
            testcases.Add("(6x)/(6y)");
            testcases.Add("(6x)/(5y)");
            testcases.Add("(6x)/(5x+3y)");
            testcases.Add("(6x)/(5x+3x^2)");
            testcases.Add("(6x+3)/6");
            testcases.Add("(6x+3)/x");
            testcases.Add("(6x+3)/(3x)");
            testcases.Add("(6x+3)/(6x+3)");
            testcases.Add("(6x+3)/(6x+2)");
            testcases.Add("(x^3+x+2)*(x^2+x+3)");*/

            /*testcases.Add("(x+1)/2+(x+1)/2");
            testcases.Add("(x+1)/2+(1-x)/2");
            testcases.Add("(x+1)/2+(-x-1)/2");
            testcases.Add("(x+1)/2+(-x)/2");*/

            //testcases.Add("(5^(x+y))/(5^x+6*5^(x+y))");
            //testcases.Add("(5^(x+1))/(5^x+9*5^(x+1))");
            //testcases.Add("(x^(3+x))/(x^(2+x)+6x^4)");

            int num = testcases.Count;
            int cutoff = 10;
            for (int i = 0; i < num; i++)
            {
                string s = testcases[i];
                if (s != "")
                {
                    var temp = new Equation(s);
                    (canvas as Layout<View>).Children.Add(temp);
                    temp.TranslationY = 50 + 100 * (i - cutoff * (i / cutoff));
                    temp.TranslationX = 400 * (i / cutoff);
                }
            }
            canvas.HeightRequest = 50 + num * 100;
            canvas.WidthRequest = 1500;
        }

        void FixDynamicLag(object o) => print.log(o as dynamic);

        public void DockKeyboard(bool isDocked)
        {
            keyboardDocked = isDocked;

            print.log("dock clicked", keyboardDocked);
            if (keyboardDocked)
            {
                Focus_SizeChanged(null, null);
            }
            else
            {
                MoveKeyboard(lastKeyboardPos);
            }
        }

        public void LongClickDown(Element element, Point pos, bool isDown)
        {
            if (element == delete)
            {
                clearCanvas();
            }
            else
            {
                CursorMode(pos, isDown);
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
                    }
                }
                else if (v is Layout)
                {
                    buttonFormat(v as Layout);
                }
            }
        }

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

        private T orient<T>(T verticalOption, T horizontalOption)
        {
            if (height > width)
            {
                return verticalOption;
            }
            return horizontalOption;
        }

        public Point lastKeyboardPos;
        
        private Point keyboardPos;

        public void InitMoveKeyboard(Point pos)
        {
            lastKeyboardPos = pos;
            keyboardPos = new Point(keyboardContainer.TranslationX, keyboardContainer.TranslationY);
            DockKeyboard(false);
        }

        public void MoveKeyboard(Point pos)
        {
            keyboardPos.X += (pos.X - lastKeyboardPos.X) * page.Width;
            keyboardPos.Y += (pos.Y - lastKeyboardPos.Y) * page.Height;

            keyboardContainer.TranslationX = Math.Max(0, Math.Min(page.Width - keyboardContainer.Width, keyboardPos.X));
            keyboardContainer.TranslationY = Math.Max(0, Math.Min(page.Height - keyboardContainer.Height, keyboardPos.Y));

            lastKeyboardPos = pos;
        }

        public Equation setFocus(Equation e)
        {
            if (Equation.Focus != null)
            {
                Equation.Focus.SizeChanged -= Focus_SizeChanged;
                if (Equation.Focus.LHS.ChildCount == 0)
                {
                    Equation.Focus.Remove();
                }
            }

            if (e != null)
            {
                Equation.Focus = e;
                Equation.Focus.SizeChanged += Focus_SizeChanged;
            }

            return Equation.Focus;
        }

        private void Focus_SizeChanged(object sender, EventArgs e)
        {
            if (Device.Idiom == TargetIdiom.Tablet && keyboardDocked)
            {
                StackLayout temp = Equation.Focus as StackLayout;
                keyboardContainer.TranslationX = temp.X - canvasScroll.ScrollX;
                keyboardContainer.TranslationY = temp.Y + Equation.Focus.Height - canvasScroll.ScrollY;
            }
        }

        public void AddEquation(Point touchPercent)
        {
            Point position = new Point((float)(touchPercent.X * canvas.Width), (float)(touchPercent.Y * canvas.Height));

            Equation equation = setFocus(new Equation());
            SoftKeyboard.MoveCursor(equation.LHS);
            
            //Make a generic equation
            //Expression e = new Expression(setFocus(new Expression(cursor)), new Text(" = "));
            //e.Selectable = true;
            //e.Children[1].SetSelectable(false);
            equation.SizeChanged += delegate
            {
                Point p = position.Add(new Point(equation.Width, equation.Height));

                if (canvas.Width - p.X < padding)
                {
                    canvas.WidthRequest = p.X + padding;
                }
                if (canvas.Height - p.Y < padding)
                {
                    canvas.HeightRequest = p.Y + padding;
                }
            };

            /*StackLayout layout = e;
            if (Device.Idiom == TargetIdiom.Tablet)
            {
                layout = new StackLayout { Orientation = StackOrientation.Vertical };
                layout.Children.Add(e);
                e.HorizontalOptions = LayoutOptions.Start;
            }*/

            canvas.Children.Add(equation, Constraint.Constant(position.X), Constraint.Constant(position.Y));
        }

        public async void clearCanvas()
        {
            if (await DisplayAlert("Wait!", "Are you sure you want to clear the canvas?", "Yes", "No"))
            {
                canvas.Children.Clear();
                canvas.WidthRequest = (canvas.Parent as View).Width;
                canvas.HeightRequest = (canvas.Parent as View).Height;
                if (Device.Idiom == TargetIdiom.Tablet)
                {
                    keyboardContainer.TranslationX = -1000;
                    keyboardContainer.TranslationY = -1000;
                    keyboardDocked = true;
                }
            }
        }

        public static bool IsInCursorMode = false;

        public void CursorMode(Point pos, bool isVisible)
        {
            if (isVisible)
            {
                if (Device.Idiom == TargetIdiom.Tablet && !keyboardDocked)
                {
                    phantomCursor.TranslationX = pos.X * page.Width;
                    phantomCursor.TranslationY = keyboardContainer.TranslationY - SoftKeyboard.Cursor.Height;
                }
                else
                {
                    Point temp = SoftKeyboard.Cursor.PositionOn(canvas).Add(new Point(-canvasScroll.ScrollX, -canvasScroll.ScrollY));
                    if (temp.X >= 0 && temp.X <= canvasScroll.Width && temp.Y >= 0 && temp.Y <= canvas.Height)
                    {
                        phantomCursor.TranslationX = temp.X;
                        phantomCursor.TranslationY = temp.Y;
                    }
                }

                lastPos = pos;
                phantomCursor.HeightRequest = SoftKeyboard.Cursor.Height;
            }
            else
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
            }

            phantomCursor.IsVisible = isVisible;
            keyboardMask.IsVisible = isVisible;
            IsInCursorMode = isVisible;
        }

        private System.Threading.Thread thread;
        private int shouldScrollX => (int)Math.Truncate(phantomCursor.TranslationX / (canvasScroll.Width - phantomCursor.Width) * 2 - 1);
        private int shouldScrollY => (int)Math.Truncate(phantomCursor.TranslationY / (canvasScroll.Height - phantomCursor.Height) * 2 - 1);
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

        public static Point lastPos = new Point(-1, -1);
        private static float speed = 2f;

        //Physically move the phantom cursor
        public void MovePhantomCursor(Point pos)
        {
            Point increase = new Point((pos.X - lastPos.X) * speed, (pos.Y - lastPos.Y) * speed);

            phantomCursor.TranslationX = Math.Max(Math.Min(canvasScroll.Width - phantomCursor.Width, phantomCursor.TranslationX + increase.X * page.Width), 0);
            phantomCursor.TranslationY = Math.Max(Math.Min(canvasScroll.Height - phantomCursor.Height, phantomCursor.TranslationY + increase.Y * page.Height), 0);

            if ((thread == null || !thread.IsAlive) && shouldScrollX + shouldScrollY != 0)
            {
                //canvasScroll.ScrollToAsync(canvasScroll.ScrollX + shouldScrollX, canvasScroll.ScrollY + shouldScrollY, false);
                thread = new System.Threading.Thread(scrollCanvas);
                thread.Start();
            }

            lastPos = new Point(pos.X, pos.Y);

            //Get the coordinates of the cursor relative to the entire screen
            Point loc = new Point(canvasScroll.ScrollX + phantomCursor.TranslationX + phantomCursor.Width / 2, canvasScroll.ScrollY + phantomCursor.TranslationY + phantomCursor.Height / 2);
            int leftOrRight = 0;
            View view = GetViewAt(canvas, loc, ref leftOrRight);

            if (view is Text || view is Expression)
            {
                phantomCursor.HeightRequest = SoftKeyboard.Cursor.Height;

                bool changed = false;
                if (view.GetType() == typeof(Expression))
                {
                    Expression e = view as Expression;
                    changed = SoftKeyboard.MoveCursor(e, Math.Min(e.Children.Count, e.Children.Count * leftOrRight));
                }
                else if (view is Text)
                {
                    changed = SoftKeyboard.MoveCursor((view as Text).Parent, (view.Parent as Expression).IndexOf(view) + leftOrRight);
                }
            }
        }

        private View GetViewAt(Layout<View> parent, Point pos, ref int leftOrRight)
        {
            View ans = null;

            for (int i = 0; i < parent.Children.Count; i++)
            {
                View child = parent.Children[i];
                print.log(child.GetType(), parent.IsEditable());
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