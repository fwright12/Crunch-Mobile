using System;
using Xamarin.Forms;
using System.Linq;
using System.Collections.ObjectModel;

using Calculator.Graphics;

namespace Calculator
{
    public class GestureRelativeLayout : RelativeLayout { }
    public class SoftKeyboardDisabledEntry : Entry { }
    public class LiveButton : Button { }

    public partial class MainPage : ContentPage
    {
        public static Expression focus;

        private static BiDictionary<Symbol, View> viewLookup = new BiDictionary<Symbol, View>();
        private static bool isDecimal = false;

        private View[] keyboards;
        private Entry cursor;
        private BoxView phantomCursor;
        private int keyboardPage = 0;

        private int padding = 100;

        //For laying out keyboard
        private int rows = 4;
        private int columns = 6;
        private int buttonSize = 80;
        private int columnsShown;

        private int fontSize
        {
            get { return 40; }
        }

        public MainPage()
        {
            InitializeComponent();

            page.LayoutChanged += OnRender;

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
                return;
                if ((sender as ListView).SelectedItem.ToString() == "<- back"){
                    ChangeKeyboard(-1);
                }
            };

            //canvas.HeightRequest = 1000;
            //canvas.WidthRequest = 1000;

            //keyboards = new View[] { extended, keypad, formulas };

            //Cursor stuff
            cursor = CreateCursor();
            Cursor.Initialize(UpdateCursor);
            left.Clicked += (sender, e) => Cursor.Left();
            right.Clicked += (sender, e) => Cursor.Right();
            delete.Clicked += (sender, e) => Input.Delete();

            //Phantom cursor stuff
            phantomCursor = CreatePhantomCursor();
            (canvas as Layout<View>).Children.Add(phantomCursor);
            phantomCursor.IsVisible = false;

            ExtensionMethods.FixDynamicLag("");
            Input.Started(this);
        }

        private double width = 0;
        private double height = 1;

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height); //must be called

            if (this.width != width || this.height != height)
            {
                this.width = width;
                this.height = height;

                //page.LayoutChanged += OnRender;
            }
        }

        private T orient<T>(T first, T second)
        {
            if (height > width)
                return first;
            else
                return second;
        }

        public void OnRender(object sender, EventArgs e)
        {
            padding = (int)Math.Min(page.Width, page.Height);

            //Set button sizes
            columnsShown = Math.Min((int)(orient(page.Width, page.Height) / buttonSize), orient(columns, rows));
            buttonSize = (int)(orient(page.Width, page.Height) / columnsShown);
            for (int i = 0; i < columns; i++)
            {
                keypad.ColumnDefinitions.Add(new ColumnDefinition { Width = buttonSize });
            }
            for (int i = 0; i < rows; i++)
            {
                keypad.RowDefinitions.Add(new RowDefinition { Height = buttonSize });
            }
            print.log("on render");
            //print.log("keypad height " + keypad.Height);

            keypad.SizeChanged += delegate
            {
                if (height > width)
                    keyboard.HeightRequest = keypad.Height;
                else
                    keyboard.WidthRequest = keypad.Width;

                //return;
                //print.log(keypad.Height, keypad.Height / page.Height, keypad.Children[0].Width, keypad.Children[0].Height);
                //AbsoluteLayout.SetLayoutBounds(keyboard, new Rectangle(0, 1, 1, keypad.Height / page.Height));
                //AbsoluteLayout.SetLayoutFlags(keyboard, AbsoluteLayoutFlags.All);
                //keyboard.HeightRequest = keypad.Height;
                setKeyboardPage(0);
            };

            foreach (Button child in keypad.Children)
            {
                child.FontSize = fontSize;
                if (child != left && child != right && child != delete)
                {
                    child.Clicked += (button, args) => 
                    {
                        Input.Key((button as Button).Text);
                    };
                }
            }

            page.LayoutChanged -= OnRender;
        }

        public void AddEquation(Point touchPercent)
        {
            StackLayout layout = new StackLayout();

            //Get rid of the old focus
            if (focus != null && focus.Children.Count == 0)
            {
                viewLookup[focus.Parent].Remove();
                viewLookup.Remove(focus.Parent);
            }

            Expression e = new Expression(focus = new Expression(), new Text("="));
            e.selectable = false;
            focus.selectable = true;
            SetText(layout, e);
            UpdateCursor();

            //Do this last because the '=' needs to be added for it to be centered properly
            //layout.LayoutChanged += delegate
            // {
            print.log(touchPercent.x * canvas.Width, touchPercent.y * canvas.Height);
            print.log(layout.Width, layout.Height);
            canvas.Children.Remove(layout);
            Point position = new Point((float)(touchPercent.x * canvas.Width), (float)(touchPercent.y * canvas.Height));
            canvas.Children.Add(layout, Constraint.Constant(position.x), Constraint.Constant(position.y));

            layout.SizeChanged += delegate
            {
                Point p = position + new Point((float)layout.Width, (float)layout.Height);

                if (canvas.Width - p.x < padding)
                {
                    canvas.WidthRequest = p.x + padding;
                }
                if (canvas.Height - p.y < padding)
                {
                    canvas.HeightRequest = p.y + padding;
                }
            };

            //Constraint.RelativeToParent((parent) => { return 0.5 * parent.Width - 100; }),
            //Constraint.RelativeToParent((parent) => { return 0.5 * parent.Height - 100; }));

            /*AbsoluteLayout.SetLayoutBounds(layout, new Rectangle(
                touchPercent.x,// * canvas.Width - layout.Width / 2,
                touchPercent.y,// * canvas.Height - layout.Height / 2,
                AbsoluteLayout.AutoSize,
                AbsoluteLayout.AutoSize));
            AbsoluteLayout.SetLayoutFlags(layout, AbsoluteLayoutFlags.PositionProportional);*/
            //};
        }

        public void ChangeKeyboard(int direction)
        {
            int updated = (keyboardPage + direction).Bound(0, 1);

            if (updated != keyboardPage)
            {
                keyboardPage = updated;
                setKeyboardPage(keyboardPage);
            }
            else if (keyboardPage == 0 && direction == -1)
            {
                keypad.IsVisible = false;
                formulas.IsVisible = true;
            }
        }

        private void setKeyboardPage(int page)
        {
            AbsoluteLayout.SetLayoutBounds(keypad, new Rectangle(-(columns - columnsShown) * buttonSize * (1 - page), 1, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
            AbsoluteLayout.SetLayoutFlags(keypad, AbsoluteLayoutFlags.YProportional);
        }

        public void CursorMode(bool isVisible)
        {
            keyboardMask.IsVisible = isVisible;
            phantomCursor.Remove();
            (canvas as Layout<View>).Children.Add(phantomCursor);
            //canvas.RaiseChild(cursor);

            if (isVisible)
            {
                Point temp = PositionOnCanvas(cursor);
                phantomCursor.TranslationX = temp.x;
                phantomCursor.TranslationY = temp.y;
            }
            else
            {
                lastPos = null;
            }
            phantomCursor.IsVisible = isVisible;
        }

        public static Point lastPos = null;
        private static float speed = 1f;

        private Point PositionOnCanvas(View view)
        {
            if (view == canvas)
                return new Point(0, 0);

            return new Point((float)view.X, (float)view.Y) + PositionOnCanvas(view.Parent as View);
        }

        //Physically move the phantom cursor
        public void MovePhantomCursor(Point pos)
        {
            if (!keyboardMask.IsVisible)
                return;

            if (lastPos == null)
            {                
                lastPos = new Point(pos);
            }

            Point increase = new Point((pos.x - lastPos.x) * speed, (pos.y - lastPos.y) * speed);

            phantomCursor.TranslationX = Math.Max(Math.Min(canvas.Width - phantomCursor.Width, phantomCursor.TranslationX + increase.x), 0);
            phantomCursor.TranslationY = Math.Max(Math.Min(canvas.Height - phantomCursor.Height, phantomCursor.TranslationY + increase.y), 0);

            lastPos = new Point(pos);

            //Get the coordinates of the cursor relative to the entire screen
            //int[] bounds = new int[2];
            //phantomCursor.GetLocationInWindow(bounds);
            //Point globalPos = new Point(bounds[0] + phantomCursor.Width / 2, bounds[1] + phantomCursor.Height / 2);

            //Point globalPos = renderFactory.MovePhantomCursor(phantomCursor, new Point((pos.x - lastPos.x) * increase, (pos.y - lastPos.y) * increase));
            //Point globalPos = new Point((float)phantomCursor.X, (float)phantomCursor.Y);

            Point loc = new Point((float)(phantomCursor.TranslationX + phantomCursor.Width / 2), (float)(phantomCursor.TranslationY + phantomCursor.Height / 2));
            int leftOrRight = 0;
            View view = GetViewAt(canvas, loc, ref leftOrRight);

            if (viewLookup.Contains(view) && viewLookup[view].selectable && (viewLookup[view] is Text || viewLookup[view] is Expression))
            {
                var symbol = viewLookup[view];

                //Climb up to the top of the tree structure
                Symbol root = symbol;
                while (root.HasParent && root.Parent.HasParent)
                {
                    root = root.Parent;
                }

                //Focus has changed
                if (root != focus)
                {
                    if (focus.Children.Count == 0)
                    {
                        viewLookup[focus.Parent].Remove();
                        viewLookup.Remove(focus.Parent);
                    }
                    focus = root as Expression;
                }

                bool changed;
                if (symbol is Expression)
                {
                    Expression e = symbol as Expression;
                    changed = Cursor.Set(e, e.Children.Count * leftOrRight);
                }
                else
                {
                    changed = Cursor.Set(symbol.Parent, symbol.Index + leftOrRight);
                }

                if (changed)
                {
                    UpdateCursor();
                }
            }
        }

        private View GetViewAt(Layout<View> parent, Point pos, ref int leftOrRight)
        {
            View ans = parent;

            for (int i = 0; i < parent.Children.Count; i++)
            {
                View child = parent.Children[i];

                if (pos.x >= child.X && pos.x <= child.X + child.Width && pos.y >= child.Y && pos.y <= child.Y + child.Height)
                {
                    if (child is Layout<View>)
                    {
                        return GetViewAt(child as Layout<View>, pos - new Point((float)child.X, (float)child.Y), ref leftOrRight);
                    }
                    else
                    {
                        ans = child;
                        break;
                    }
                }
            }

            leftOrRight = (int)Math.Round((pos.x - ans.X) / ans.Width);
            return ans;
        }

        public void UpdateCursor()
        {
            if (cursor.Parent != viewLookup[Cursor.Parent] || (cursor.Parent != null && (cursor.Parent as Layout<View>).Children.IndexOf(cursor) != Cursor.Index))
            {
                cursor.Remove();
                (viewLookup[Cursor.Parent] as Layout<View>).Children.Insert(Cursor.Index, cursor);
            }
            
            //cursor.Focus();
        }

        /// <summary>
        /// Assumes that the symbol being passed has a parent symbol and that the parent symbol has an associated view
        /// </summary>
        public void SetText(Symbol symbol)
        {
            if (symbol.HasParent && viewLookup.Contains(symbol.Parent))
            {
                SetText((Layout<View>)viewLookup[symbol.Parent], symbol, symbol.Parent.Children.IndexOf(symbol));
            }
            else
            {
                throw new Exception("The given symbol either does not have a parent symbol, or the parent does not have an associated view. Alternatively, pass a parent and index");
            }
        }

        public void SetText(Layout<View> parent, Symbol symbol, int index = 0, bool firstCall = true)
        {
            print.log("set text " + symbol);
            var view = default(View);

            if (viewLookup.Contains(symbol))
            {
                view = viewLookup[symbol];
            }
            else
            {
                view = symbol.Render();
                viewLookup.Add(symbol, view);
            }

            Element currentParent = view.Parent;
            if (currentParent == null || !currentParent.Equals(parent) || (view.Parent as Layout<View>).Children.IndexOf(view) != index)
            {
                parent.Children.Insert(index, view);
            }

            if (symbol.HasParent && symbol.Parent is Expression)
            {
                CheckPadding(parent, symbol.Parent.Children.First() is Fraction, symbol.Parent.Children.Last() is Fraction);
            }

            if (symbol is Container)
            {
                Container layout = symbol as Container;

                //Focus on an empty layout
                if (layout is Expression && layout.Children.Count == 0)
                {
                    Cursor.Set(layout);
                }

                for (int i = 0; i < layout.Children.Count; i++)
                {
                    SetText((Layout<View>)view, layout.Children[i], i, false);
                }
            }
        }

        public void Remove(Symbol symbol)
        {
            symbol.Remove();
            viewLookup[symbol].Remove();
            viewLookup.Remove(symbol);
        }

        public void SetAnswer()
        {
            Crunch.Term calculated = focus;
            Container root = focus.Parent;
            
            //Remove the old answer if necessary
            if (root.Children.Count == 3)
            {
                Remove(root.Children[2]);
            }

            //Try to add the new answer
            if (calculated != null)
            {
                Symbol answer = calculated as dynamic;
                if (isDecimal)
                {
                    answer = new Crunch.Number(Math.Round(calculated.value, 3)) as dynamic;
                }

                answer.selectable = false;

                answer.AddAfter(root.Children[1]);
                SetText(answer);
                //viewLookup[answer] delegate { isDecimal = !isDecimal; SetAnswer(focus as dynamic); });
            }
        }
    }
}