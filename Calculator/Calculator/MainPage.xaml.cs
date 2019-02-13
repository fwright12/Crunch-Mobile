using System;
using Xamarin.Forms;
using System.Linq;
using System.Collections.ObjectModel;
using System.IO;

using Crunch.GraFX;

namespace Calculator
{
    public class GestureRelativeLayout : RelativeLayout { }
    public class LongClickableButton : Button { }
    public class Mask : StackLayout { }
    public class ScrollSpy : AbsoluteLayout { }
    public class DockButton : Button { }
    public class BannerAd : View { }

    public partial class MainPage : ContentPage
    {
        public static Crunch.GraFX.Equation focus;

        public static bool isTouchingCanvas = false;
        public static bool keyboardDocked = true;

        public static View cursor;

        private BoxView phantomCursor;

        //How much extra space is in the lower right
        private int padding = 100;

        public static readonly double fontSize = 33;
        private static double buttonFontSize = -1;

        //Color #560297

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
                cursor = new Cursor(true);
                page.Children.Remove(l);
            };

            //Button hookup
            left.Clicked += (sender, e) => { if (Cursor.Parent != null) Cursor.Left(); };
            right.Clicked += (sender, e) => { if (Cursor.Parent != null) Cursor.Right(); };
            delete.Clicked += (sender, e) => { if (Cursor.Parent != null) Cursor.Delete(); };
            info.Clicked += (sender, e) => DisplayAlert("About Crunch",
                "Thank you for using Crunch!\n\n" +
                "A few tips about how to navigate the app: " + tips +
                "\nClick the answer to see alternate formats.\n\n" +
                "If you find any bugs, please report them to GreenMountainLabs802@gmail.com. The more information " +
                "you can provide (what you did to cause the error, screenshots, etc.) the easier it will be to fix.\n\n" +
                "Please also email me with any ideas you have about how the app can be improved, or features you " +
                "would like to see in the future. I'm open to suggestions!",
                "Dismiss");

            //Phantom cursor stuff
            phantomCursor = new Cursor();
            phantomCursor.Color = Color.Red;
            phantomCursorField.Children.Add(phantomCursor);
            phantomCursor.IsVisible = false;

            //ExtensionMethods.FixDynamicLag("");
            print.log(("") as dynamic);
            Input.Started(this);
            print.log("main page constructor finished");
        }

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

                    //Dynamic sizing for specific buttons
                    if (b.Parent == arrowKeys)
                    {
                        b.SizeChanged += delegate { b.FontSize = Math.Floor(33 * b.Height / 75); };
                    }
                    else if (b == info || b == delete)
                    {
                        b.SizeChanged += delegate { b.FontSize = Math.Floor(33 * b.Width / 75 * 5 / (4 + b.Text.Length)); };
                    }
                    else
                    {
                        b.SizeChanged += delegate { b.FontSize = Math.Floor(33 * Math.Min(b.Height, b.Width) / 75); };
                    }

                    if (b.Parent == keypad)
                    {
                        b.Clicked += delegate
                        {
                            //if (Cursor.Parent != null)
                            Input.Key(b.StyleId ?? b.Text);
                            if (keypad.Children.IndexOf(b) % columns <= 0)
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
        private int columns = 5;
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
            for (int i = 0; i < columns; i++)
            {
                keypad.ColumnDefinitions.Add(new ColumnDefinition { Width = width });
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

        public Crunch.GraFX.Equation setFocus(Crunch.GraFX.Equation e)
        {
            if (focus != null)
            {
                focus.SizeChanged -= Focus_SizeChanged;
                if (focus.Children.Count == 0)
                {
                    focus.Remove();
                }
            }

            if (e != null)
            {
                focus = e;
                focus.SizeChanged += Focus_SizeChanged;
            }

            return focus;
        }

        private void Focus_SizeChanged(object sender, EventArgs e)
        {
            if (Device.Idiom == TargetIdiom.Tablet && keyboardDocked)
            {
                StackLayout temp = focus.Parent as StackLayout;
                keyboardContainer.TranslationX = temp.X - canvasScroll.ScrollX;
                keyboardContainer.TranslationY = temp.Y + focus.Height - canvasScroll.ScrollY;
            }
        }

        public void AddEquation(Point touchPercent)
        {
            Point position = new Point((float)(touchPercent.X * canvas.Width), (float)(touchPercent.Y * canvas.Height));

            //Make a generic equation
            //Expression e = new Expression(setFocus(new Expression(cursor)), new Text(" = "));
            /*e.Selectable = true;
            e.Build();
            e.ChildAt(1).SetSelectable(false);*/

            Equation layout = setFocus(new Equation());
            layout.SizeChanged += delegate
            {
                Point p = position.Add(new Point(layout.Width, layout.Height));

                if (canvas.Width - p.X < padding)
                {
                    canvas.WidthRequest = p.X + padding;
                }
                if (canvas.Height - p.Y < padding)
                {
                    canvas.HeightRequest = p.Y + padding;
                }
            };

            Cursor.MoveTo(layout.LHS);
            //Cursor.parent = layout.LHS;
            //(layout.Children[0] as StackLayout).Children.Add(cursor);

            /*StackLayout layout = e;
            if (Device.Idiom == TargetIdiom.Tablet)
            {
                layout = new StackLayout { Orientation = StackOrientation.Vertical };
                layout.Children.Add(e);
                e.HorizontalOptions = LayoutOptions.Start;
            }*/

            canvas.Children.Add(layout, Constraint.Constant(position.X), Constraint.Constant(position.Y));
        }

        public async void clearCanvas()
        {
            if (await DisplayAlert("Wait!", "Are you sure you want to clear the canvas?", "Yes", "No"))
            {
                canvas.Children.Clear();
                canvas.WidthRequest = (canvas.Parent as View).Width;
                canvas.HeightRequest = (canvas.Parent as View).Height;
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
                    phantomCursor.TranslationY = keyboardContainer.TranslationY - cursor.Height;
                }
                else
                {
                    Point temp = PositionOnCanvas(cursor).Add(new Point(-canvasScroll.ScrollX, -canvasScroll.ScrollY));
                    if (temp.X >= 0 && temp.X <= canvasScroll.Width && temp.Y >= 0 && temp.Y <= canvas.Height)
                    {
                        phantomCursor.TranslationX = temp.X;
                        phantomCursor.TranslationY = temp.Y;
                    }
                }

                lastPos = pos;
                phantomCursor.HeightRequest = cursor.Height;
            }
            else
            {
                //Climb up to the top of the tree structure
                Element root = cursor;
                while (root.HasParent() && root.Parent.HasParent() && root.Parent.Parent is Crunch.GraFX.Expression)
                {
                    root = root.Parent;
                }

                //Focus has changed
                if (root != focus)
                {
                    //setFocus(root as CrunchGraFX.Expression);
                }
            }

            phantomCursor.IsVisible = isVisible;
            keyboardMask.IsVisible = isVisible;
            IsInCursorMode = isVisible;
        }

        private Point PositionOnCanvas(View view)
        {
            if (view == canvas || view is null)
            {
                return Point.Zero;
            }

            return PositionOnCanvas(view.Parent as View).Add(new Point(view.X, view.Y + view.TranslationY));
        }

        private Java.Lang.Thread thread;
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
                thread = new Java.Lang.Thread(scrollCanvas);
                thread.Start();
            }

            lastPos = new Point(pos.X, pos.Y);

            //Get the coordinates of the cursor relative to the entire screen
            Point loc = new Point(canvasScroll.ScrollX + phantomCursor.TranslationX + phantomCursor.Width / 2, canvasScroll.ScrollY + phantomCursor.TranslationY + phantomCursor.Height / 2);
            int leftOrRight = 0;
            View view = GetViewAt(canvas, loc, ref leftOrRight);
            
            //if (viewLookup.Contains(view) && viewLookup[view].selectable && (viewLookup[view] is Text || viewLookup[view] is Expression))
            if (view is Text || view is Expression)
            {
                phantomCursor.HeightRequest = cursor.Height;

                bool changed = false;
                if (view.GetType() == typeof(Expression))
                {
                    Expression e = view as Expression;
                    //changed = Cursor.Move(e, e.ChildCount * leftOrRight);
                }
                else if (view is Text)
                {
                    //changed = Cursor.Move((view as Text).Parent, view.Index() + leftOrRight);
                }
            }
        }

        //Bad logic - rewrite
        private View GetViewAt(Layout<View> parent, Point pos, ref int leftOrRight)
        {
            View ans = null;

            for (int i = 0; i < parent.Children.Count; i++)
            {
                View child = parent.Children[i];

                if (child.Selectable() && pos.X >= child.X && pos.X <= child.X + child.Width && pos.Y >= child.Y - child.Margin.Top + child.TranslationY && pos.Y <= child.Y + child.Height + child.TranslationY)
                {
                    if (child is Layout<View>)
                    {
                        //See if I'm over one of this layout's children
                        ans = GetViewAt(child as Layout<View>, pos.Add(new Point(-child.X, -child.Y - child.TranslationY)), ref leftOrRight);

                        var temp = child as Layout<View>;
                        //I'm not over any of this layout's children, but maybe I'm on one of the ends
                        if (ans == null && ((temp.Padding.Left > 0 && pos.X <= temp.Padding.Left) || (temp.Padding.Right > 0 && pos.X >= temp.Width - temp.Padding.Right)))
                        {
                            ans = child;
                            break;
                        }

                        return ans;
                    }
                    else
                    {
                        ans = child;
                        break;
                    }
                }
            }

            if (ans != null)
            {
                leftOrRight = (int)Math.Round((pos.X - ans.X) / ans.Width);
            }
            return ans;
        }

        public static void SetAnswer()
        {
            /*Expression root = focus.Parent;

            Answer answer = null;
            //Remove the old answer if it exists
            if (root.ChildCount == 2)
            {
                answer = new Answer(new Crunch.Number(0));
                root.Add(answer);
                answer.SetSelectable(false);
                answer.Build();
            }
            else
            {
                answer = root.ChildAt(2) as Answer;
            }

            //Try to add the new answer
            try
            {
                answer.IsVisible = true;
                //answer.SwitchFormat(Crunch.Math.Evaluate(focus));
                /*var answer = new Answer(Crunch.Math.Evaluate(focus));
                root.Insert(2, answer);
                answer.SetSelectable(false);
                answer.Build();*/

                /*if (isDecimal || answer.GetType() == typeof(Crunch.Number))
                {
                    answer = new Crunch.Number(Math.Round(answer.value, decimalPlaces));
                }

                //answer.selectable = false;

                //answer.AddAfter(root.Children[1]);
                //SetText(answer);
                //viewLookup[answer] delegate { isDecimal = !isDecimal; SetAnswer(focus as dynamic); });

                print.log("adding answer");
                root.Insert(2, answer.ToView());
            }
            catch (Exception e)
            {
                answer.IsVisible = false;
                print.log(e.Message);
            }*/
        }
    }
}