using System;
using Xamarin.Forms;
using System.Linq;
using System.Collections.ObjectModel;
using System.IO;

namespace Calculator
{
    public class GestureRelativeLayout : RelativeLayout { }
    public class SoftKeyboardDisabledEntry : Entry { }
    public class LongClickableButton : Button { }
    public class Mask : StackLayout { }
    public class ScrollSpy : AbsoluteLayout { }

    public partial class MainPage : ContentPage
    {
        public static Expression focus;

        public static bool isTouchingCanvas = false;
        private static bool isDecimal = false;

        public static View cursor;

        private BoxView phantomCursor;

        //How much extra space is in the lower right
        private int padding = 100;
        private int decimalPlaces = 3;

        //Keyboard
        private bool isDocked = true;
        private int rows = 4;
        private int columns = 5;
        private int buttonSize;
        private readonly int undockedButtonSize = 60;
        private int columnsShown = 4;

        public static readonly int fontSize = 33;

        public MainPage()
        {
            InitializeComponent();

            formulas.ItemSelected += (sender, e) =>
            {
                (sender as ListView).SelectedItem = null;
            };

            //formulas.BindingContext = new string[] { "test" };
            ObservableCollection<string> labels = new ObservableCollection<string>();
            labels.Add("<- back");
            formulas.ItemsSource = labels;
            //formulas.ItemTemplate = new DataTemplate(typeof(Button));
            formulas.ItemAppearing += (object sender, ItemVisibilityEventArgs e) =>
            {
                print.log(sender.GetType());
            };

            formulas.ItemTapped += (sender, e) =>
            {
                print.log(sender.GetType());
            };

            dock.Clicked += (sender, e) => {
                keyboardScroll.MinimumWidthRequest = 50;
                keyboardScroll.WidthRequest = 50;
            };

            //Application.Current.Properties["test"] = 234;

            //Measuring for cursor
            Text t = new Text("");
            page.Children.Add(t);
            t.SizeChanged += delegate
            {
                Input.textHeight = t.Height;
                cursor = new Cursor(true);
                page.Children.Remove(t);
            };

            buttons(permanentKeys, (button) => { button.FontSize = fontSize; });
            foreach(View v in permanentKeys.Children)
            {
                v.VerticalOptions = LayoutOptions.FillAndExpand;
                v.HorizontalOptions = LayoutOptions.FillAndExpand;
            }
            buttons(keypad, (button) =>
            {
                button.FontSize = fontSize;
                if (button != left && button != right && button != delete)
                {
                    button.Clicked += (b, args) =>
                    {
                        Input.Key((b as Button).Text);
                    };
                }
            });

            //Cursor stuff
            //cursor.Focus();
            left.Clicked += (sender, e) => Cursor.Left();
            right.Clicked += (sender, e) => Cursor.Right();
            delete.Clicked += (sender, e) => Input.Delete();
            left.WidthRequest = fontSize * 1.5;
            right.WidthRequest = fontSize * 1.5;

            //Phantom cursor stuff
            phantomCursor = new Cursor();
            phantomCursor.Color = Color.Red;
            //(canvas as Layout<View>).Children.Add(phantomCursor);
            phantomCursorField.Children.Add(phantomCursor);
            phantomCursor.IsVisible = false;

            ExtensionMethods.FixDynamicLag("");
            Input.Started(this);
        }

        private void showGraph()
        {
            var htmlSource = new HtmlWebViewSource();
            htmlSource.Html = @"
<html>
<body>
<script src = ""https://www.desmos.com/api/v1.0/calculator.js?apiKey=dcb31709b452b1cf9dc26972add0fda6""></script>
 <div id = ""calculator"" style = ""width: 600px; height: 400px;""></div>
    <script>
      var elt = document.getElementById('calculator');
            var calculator = Desmos.GraphingCalculator(elt, { expressions: false, settingsMenu: false, zoomButtons: false});
            calculator.setExpression({ id: 'graph1', latex: 'y=x^2'});
</script>
</body>
</html> ";
            //graph.Source = htmlSource;
        }

        private void buttons(Layout<View> parent, Action<Button> action)
        {
            for (int i = 0; i < parent.Children.Count; i++)
            {
                View v = parent.Children[i];
                if (v is Button)
                {
                    action(v as Button);
                }
                else if (v is Layout <View>)
                {
                    buttons(v as Layout<View>, action);
                }
            }
        }

        private double width = 0;
        private double height = 0;

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height); //must be called

            if (this.width != width || this.height != height)
            {
                this.width = width;
                this.height = height;

                padding = (int)Math.Min(page.Width, page.Height);

                page.Orientation = orient(StackOrientation.Vertical, StackOrientation.Horizontal);
                keyboard.Orientation = orient(StackOrientation.Horizontal, StackOrientation.Vertical);
                permanentKeys.Orientation = orient(StackOrientation.Vertical, StackOrientation.Horizontal);

                keyboardScroll.ScrollToAsync(keypad, ScrollToPosition.End, false);
                LayoutKeyboard(isDocked);
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

        private Point lastKeyboardPos;

        public void MoveKeyboard(Point pos)
        {
            if (isDocked)
            {
                LayoutKeyboard(isDocked = false);
                if (lastKeyboardPos == null)
                {
                    lastKeyboardPos = pos;// + new Point((float)-keyboardContainer.TranslationX, (float)-keyboardContainer.TranslationY);
                }
            }

            keyboardContainer.TranslationX += (lastKeyboardPos.X - pos.X);
            keyboardContainer.TranslationY += (lastKeyboardPos.Y - pos.Y);

            lastKeyboardPos = pos;
        }

        public void LayoutKeyboard(bool docked)
        {
            print.log("layout keyboard", docked);

            keypad.ColumnDefinitions.Clear();
            keypad.RowDefinitions.Clear();

            if (docked)
            {
                buttonSize = (int)orient((page.Width - fontSize * 3) / columnsShown, page.Height / (columnsShown + 1));

                if (keyboardContainer.Parent == canvas)
                {
                    keyboardContainer.Remove();
                    page.Children.Add(keyboardContainer);
                    keyboardContainer.TranslationX = 0;
                    keyboardContainer.TranslationY = 0;
                }
            }
            else if (undockedButtonSize < buttonSize)
            {
                buttonSize = undockedButtonSize;

                keyboardContainer.Remove();
                (canvas as Layout<View>).Children.Add(keyboardContainer);

                keyboardContainer.TranslationX = page.Width - buttonSize * columnsShown - permanentKeys.Width;
                keyboardContainer.TranslationY = page.Height - buttonSize * columnsShown;
            }

            //keyboard.WidthRequest = orient(page.Width, buttonSize * columnsShown);
            //permanentKeys.WidthRequest = orient(fontSize * 3, buttonSize * columnsShown);
            keyboardScroll.WidthRequest = buttonSize * columnsShown;
            
            //keyboard.HeightRequest = buttonSize * columnsShown;
            //keyboard.WidthRequest = buttonSize * columnsShown + permanentKeys.Width;

            //keyboardContainer.WidthRequest = buttonSize * columnsShown + permanentKeys.Width;

            for (int i = 0; i < columns; i++)
            {
                keypad.ColumnDefinitions.Add(new ColumnDefinition { Width = buttonSize });
            }
            for (int i = 0; i < rows; i++)
            {
                keypad.RowDefinitions.Add(new RowDefinition { Height = buttonSize });
            }
            foreach (View v in permanentKeys.Children)
            {
                if (!(v is StackLayout))
                {
                    v.WidthRequest = orient(fontSize * 3, (buttonSize * columnsShown - fontSize * 3) / (permanentKeys.Children.Count - 1));
                }
            }
            permanentKeys.HeightRequest = orient(buttonSize * columnsShown, buttonSize);
        }

        public void AddEquation(Point touchPercent)
        {
            //Get rid of the old focus
            if (focus != null && focus.ChildCount == 0)
            {
                focus.Remove();
            }
            
            //Make a generic equation
            Expression e = new Expression(focus = new Expression(cursor), new Text("="));
            e.Selectable = true;
            focus.Selectable = true;

            //Do this last because the '=' needs to be added for it to be centered properly
            Point position = new Point((float)(touchPercent.X * canvas.Width), (float)(touchPercent.Y * canvas.Height));
            canvas.Children.Add(e, Constraint.Constant(position.X), Constraint.Constant(position.Y));

            e.SizeChanged += delegate
            {
                Point p = position.Add(new Point(e.Width, e.Height));

                if (canvas.Width - p.X < padding)
                {
                    canvas.WidthRequest = p.X + padding;
                }
                if (canvas.Height - p.Y < padding)
                {
                    canvas.HeightRequest = p.Y + padding;
                }
            };
        }

        public async void clearCanvas()
        {
            if (await DisplayAlert("Wait!", "Are you sure you want to clear the canvas?", "Yes", "No"))
            {
                canvas.Children.Clear();
                canvas.WidthRequest = page.Width;
                canvas.HeightRequest = (canvas.Parent as View).Height;
            }
        }

        public static bool IsInCursorMode = false;

        public void CursorMode(bool isVisible)
        {
            if (isVisible)
            {
                Point temp = PositionOnCanvas(cursor).Add(new Point(-canvasScroll.ScrollX, -canvasScroll.ScrollY));
                phantomCursor.TranslationX = temp.X;
                phantomCursor.TranslationY = temp.Y;
            }
            else
            {
                lastPos = new Point(-1, -1);
            }

            phantomCursor.IsVisible = isVisible;
            keyboardMask.IsVisible = isVisible;
            IsInCursorMode = isVisible;
        }

        public static Point lastPos = new Point(-1, -1);
        private static float speed = 1f;

        private Point PositionOnCanvas(View view)
        {
            if (view == canvas)
            {
                return Point.Zero;
            }

            return PositionOnCanvas(view.Parent as View).Add(new Point(view.X, view.Y));
        }

        //Physically move the phantom cursor
        public void MovePhantomCursor(Point pos)
        {
            if (lastPos.X == -1 && lastPos.Y == -1)
            {
                lastPos = new Point(pos.X, pos.Y);
            }

            Point increase = new Point((pos.X - lastPos.X) * speed, (pos.Y - lastPos.Y) * speed);

            phantomCursor.TranslationX = Math.Max(Math.Min(canvasScroll.Width - phantomCursor.Width, phantomCursor.TranslationX + increase.X), 0);
            phantomCursor.TranslationY = Math.Max(Math.Min(canvasScroll.Height - phantomCursor.Height, phantomCursor.TranslationY + increase.Y), 0);

            lastPos = new Point(pos.X, pos.Y);

            //Get the coordinates of the cursor relative to the entire screen
            Point loc = new Point(canvasScroll.ScrollX + phantomCursor.TranslationX + phantomCursor.Width / 2, canvasScroll.ScrollY + phantomCursor.TranslationY + phantomCursor.Height / 2);
            int leftOrRight = 0;
            View view = GetViewAt(canvas, loc, ref leftOrRight);

            //if (viewLookup.Contains(view) && viewLookup[view].selectable && (viewLookup[view] is Text || viewLookup[view] is Expression))
            if (view is Text || view is Expression)
            {
                phantomCursor.HeightRequest = cursor.Height;

                //Climb up to the top of the tree structure
                Element root = view;
                while (root.HasParent() && root.Parent.HasParent() && root.Parent.Parent is Expression)
                {
                    root = root.Parent;
                }

                //Focus has changed
                if (root != focus)
                {
                    print.log(root, focus);
                    //if (focus.Children.Count == 0)
                    //{
                    //viewLookup[focus.Parent].Remove();
                    //viewLookup.Remove(focus.Parent);
                    //}
                    focus = root as Expression;
                }

                bool changed = false;
                if (view.GetType() == typeof(Expression))
                {
                    Expression e = view as Expression;
                    changed = Cursor.Move(e, e.ChildCount * leftOrRight);
                }
                else if (view is Text)
                {
                    print.log(view.Index());
                    changed = Cursor.Move((view as Text).Parent, view.Index() + leftOrRight);
                }
            }
        }

        private View GetViewAt(Layout<View> parent, Point pos, ref int leftOrRight)
        {
            View ans = parent;

            for (int i = 0; i < parent.Children.Count; i++)
            {
                View child = parent.Children[i];
                //print.log("hovering over " + child + " which has selectability set to: " + child.Selectable());
                print.log("test", child is Exponent, pos.Y, child.Y, child.Height, child.TranslationY);
                if (child.Selectable() && pos.X >= child.X && pos.X <= child.X + child.Width && pos.Y >= child.Y && pos.Y <= child.Y + child.Height)
                {
                    if (child is Layout<View>)
                    {
                        return GetViewAt(child as Layout<View>, pos.Add(new Point(-child.X, -child.Y)), ref leftOrRight);
                    }
                    else
                    {
                        ans = child;
                        break;
                    }
                }
            }

            leftOrRight = (int)Math.Round((pos.X - ans.X) / ans.Width);
            return ans;
        }

        public void SetAnswer()
        {
            Expression root = focus.Parent;
            
            //Remove the old answer if it exists
            if (root.ChildCount == 3)
            {
                root.RemoveAt(2);
            }

            //Try to add the new answer
            try
            {
                var answer = Crunch.Math.Evaluate(focus);
                if (isDecimal || answer.GetType() == typeof(Crunch.Number))
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
                print.log(e.Message);
            }
        }
    }
}