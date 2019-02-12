using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Util;
using System.Collections.Generic;
using Android.Graphics;
using Android.Graphics.Drawables;

using System.Collections.Specialized;
using Android.Text;

namespace Calculator.Droid
{
    [Activity (Label = "Calculator.Droid", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : Activity, IGraphicsHandler
	{
        public static string screenSize = "xLarge";
        public static OrderedDictionary text = new OrderedDictionary();

        public RelativeLayout canvas;
        private TableLayout keyboard;

        protected override void OnCreate (Bundle bundle)
		{
            //Bogus operation to remove later lag - problem with dynamic?
            //new Operation(new Operand("+"), new Number(1)).operate(new Number(1));
            new Expression().FixWeirdError();

            base.OnCreate (bundle);
            
            // Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

            Input.graphicsHandler = this;

            // Get our button from the layout resource,
            // and attach an event to it
            canvas = FindViewById<RelativeLayout>(Resource.Id.canvas);
            Button button = FindViewById<Button> (Resource.Id.myButton);
            keyboard = FindViewById<TableLayout>(Resource.Id.keyboard);
            
            Window.SetSoftInputMode(SoftInput.StateAlwaysHidden);

            FindViewById<Button>(Resource.Id.cursorLeft).Click += cursorLeft;
            FindViewById<Button>(Resource.Id.cursorRight).Click += cursorRight;
            FindViewById<Button>(Resource.Id.delete).Click += delete;
            FindViewById<Button>(Resource.Id.enter).Click += enterIsItNecessary;

            Console.WriteLine(keyboard.ChildCount);
            for (int i = 0; i < keyboard.ChildCount; i++)
            {
                TableRow row = (TableRow)keyboard.GetChildAt(i);
                for (int j = 0; j < row.ChildCount; j++)
                {
                    Button key = (Button)row.GetChildAt(j);
                    key.Touch += keyboardTouch;

                    if (!key.HasOnClickListeners)
                    {
                        key.Click += KeyPress;
                    }

                    if (screenSize == "xLarge")
                    {
                        key.TextSize = 40;
                    }
                }
            }

            canvas.Touch += canvasTouch;
		}

        private EditText editField;

        private void KeyPress(object sender, EventArgs e)
        {
            Button sent = (Button)sender;

            Input.selected.Key(sent.Text);

            //Equation.selected.SetAnswer();
        }

        public string DispatchKey(string text)
        {
            ((EditText)Input.editField).DispatchKeyEvent(new KeyEvent(0, text, 0, new KeyEventFlags()));

            return ((EditText)Input.editField).Text;
        }

        /*public void StartEditing()
        {
            editField = (EditText)AddEditField();

            if (Input.UI.views.ContainsKey(Input.text[Input.pos]))
            {
                RemoveChild(Input.UI.graphicalObject, Input.pos);
                Input.UI.views.Remove(Input.text[Input.pos]);

                //views[Input.text[Input.pos]] = Input.editField;
            }
            else
            {
                //views.Add(Input.text[Input.pos], Input.editField);
            }

            AddChild(Input.UI.graphicalObject, editField, Input.pos);

            //graphicsHandler.AddChild(graphicalObject, Input.editField, Input.pos);
        }*/

        public object AddText(string text)
        {
            TextView temp = new TextView(this);
            temp.TextSize = 40;

            temp.Text = text;

            /*if (Input.clickable)
            {
                temp.Click += symbolTouch;
            }*/

            temp.LongClick += LongClicked;

            return temp;
        }

        private object moving;

        private void LongClicked(object sender, View.LongClickEventArgs e)
        {
            Vibrator v = (Vibrator)GetSystemService(VibratorService);
            v.Vibrate(3000);

            ((View)sender).Touch += Touching;
        }

        private void aTouching(object sender, View.TouchEventArgs touchArgs)
        {
            MotionEvent e = touchArgs.Event;

            print.log(e.Action);
        }

        private void Touching(object sender, View.TouchEventArgs touchArgs)
        {
            MotionEvent e = touchArgs.Event;
            LinearLayout sent = (LinearLayout)((View)sender).Parent;
            
            if (e.Action.Equals(MotionEventActions.Move))
            {
                print.log("moving");
                print.log(e.GetX() + ", " + e.GetY());
                print.log(((LinearLayout)sent.Parent).Left + " ," + ((LinearLayout)sent.Parent).Top);

                ViewGroup parent = (ViewGroup)sent.Parent;
                parent.RemoveView(sent);

                RelativeLayout.LayoutParams param = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
                //param.SetMargins((int)e.GetX(), (int)e.GetY(), 0, 0);
                param.SetMargins(parent.Left + (int)e.GetX() + sent.Width / 4, parent.Top + (int)e.GetY() - sent.Height / 2, 0, 0);
                sent.LayoutParameters = param;
                
                canvas.AddView(sent);
                sent.Touch += Touching;
            }
            else if (e.Action.Equals(MotionEventActions.Up))
            {
                //((View)sender).Touch -= Touching;
            }
        }

        private View selected;

        public void Select(object sender)
        {
            if (sender.Equals(selected))
            {
                if (Input.state == Input.OnReceive.add)
                {
                    selected.SetBackgroundColor(Color.Red);
                    Input.state = Input.OnReceive.delete;
                }
                else
                {
                    selected.SetBackgroundColor(Color.Green);
                    Input.state = Input.OnReceive.add;
                }
            }
            else
            {
                if (selected != null)
                {
                    selected.SetBackgroundColor(Color.Transparent);
                }

                ((View)sender).SetBackgroundColor(Color.Green);
                Input.state = Input.OnReceive.add;

                selected = (View)sender;
            }
        }

        private void symbolTouch(object sender, EventArgs e)
        {
            /*return;
            print.log("clicked");
            Input.UI.Update();
            Input.editField = null;

            View sent = (View)sender;
            
            Select(sender);

            Input.pos = ((ViewGroup)sent.Parent).IndexOfChild(sent);
            Input.UI = Graphics.findByLayout[sent.Parent];

            if (Input.UI.expression.parts[Input.pos].GetType() == typeof(Number))
            {
                string temp = Input.UI.expression.parts[Input.pos].text;

                Input.editing = true;
                Input.UI.expression.parts[Input.pos] = new Number(0);
                Input.UI.Update();
                Input.UI.expression.parts[Input.pos] = new Number(double.Parse(temp));

                sent = (View)Input.editField;
                ((EditText)sent).Text = temp;
                ((EditText)sent).SetSelection(temp.Length);
            }
            else
                Input.pos++;

            /*    ((LinearLayout)Input.UI.graphicalObject).RemoveViewAt(Input.pos);
                Input.UI.views.RemoveAt(Input.pos);

                editField = new EditText(this);
                editField.InputType = Android.Text.InputTypes.NumberFlagDecimal | Android.Text.InputTypes.NumberFlagSigned;
                editField.TextSize = 35;
                editField.RequestFocus();
                editField.Click += symbolTouch;

                editField.Text = Input.UI.expression.parts[Input.pos].text;
                editField.SetSelection(editField.Text.Length);
                Input.editing = true;

                Select(editField);

                ((LinearLayout)Input.UI.graphicalObject).AddView(editField, Input.pos);

                sent = editField;
            }
            else
            {
                Input.pos++;
                tryEditTextExit();
            }

            //View temp = Input.UI;
            while (!Equation.findByGraphics.ContainsKey(sent))
            {
                //temp = Graphics.findByLayout[((View)temp.graphicalObject).Parent];
                sent = (View)sent.Parent;
            }
            Equation.selected = Equation.findByGraphics[sent];*/
        }

        public object AddEditField()
        {
            EditText temp = new EditText(this);
            temp.InputType = Android.Text.InputTypes.NumberFlagDecimal | Android.Text.InputTypes.NumberFlagSigned;
            temp.TextSize = 35;
            temp.RequestFocus();
            temp.Click += symbolTouch;

            return temp;
        }

        public void Parentheses(object parent, bool isVisible)
        {
            if (isVisible)
            {
                ((ViewGroup)parent).GetChildAt(0).Visibility = ViewStates.Visible;
                ((ViewGroup)parent).GetChildAt(((ViewGroup)parent).ChildCount - 1).Visibility = ViewStates.Visible;
            }
            else
            {
                ((ViewGroup)parent).GetChildAt(0).Visibility = ViewStates.Gone;
                ((ViewGroup)parent).GetChildAt(((ViewGroup)parent).ChildCount - 1).Visibility = ViewStates.Gone;
            }
        }

        public object AddLayout(bool isHorizontal)
        {
            LinearLayout layout = new LinearLayout(this);

            if (isHorizontal)
            {
                layout.Orientation = Orientation.Horizontal;

                //layout.AddView((View)Graphics.Create(new Text("(")), 0);
                //layout.AddView((View)Graphics.Create(new Text(")")), 1);
            }
            else
            {
                layout.Orientation = Orientation.Vertical;
            }

            layout.SetGravity(GravityFlags.Center);
            //layout.Click += symbolTouch;

            return layout;
        }

        public bool hasParent(object sender)
        {
            return ((View)sender).Parent != null;
        }

        public void AddChild(object parent, object child, int index)
        {
            ((ViewGroup)parent).AddView((View)child, index);
        }

        public void RemoveChild(object parent, int index)
        {
            ((ViewGroup)parent).RemoveViewAt(index);
        }

        public void RemoveChild(object parent, object child)
        {
            ((ViewGroup)parent).RemoveView((View)child);
        }

        private void canvasTouch(object sender, View.TouchEventArgs touchArgs) {
            if (selected != null)
            {
                selected.SetBackgroundColor(Color.Transparent);
                //Input.ExitEditMode();
                //Input.UI.Update();
                //Input.editField = null;
            }

            MotionEvent e = touchArgs.Event;
            if (!touchArgs.Event.Action.Equals(MotionEventActions.Up))
                return;
            Console.WriteLine("canvas touched");

            LinearLayout layout = new LinearLayout(this);
            RelativeLayout.LayoutParams param = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            param.SetMargins((int)e.GetX(), (int)e.GetY(), 0, 0);
            layout.LayoutParameters = param;
            layout.SetGravity(GravityFlags.Center);
            canvas.AddView(layout);

            Input.selected = new Input(new MathView(layout));

            //Equation.selected = new Equation(layout, new Expression());
        }

        private float start, end;
        private static int minDistance = 100;

        private void keyboardTouch(object sender, View.TouchEventArgs e)
        {
            switch (e.Event.Action)
            {
                case MotionEventActions.Down:
                    start = e.Event.GetX();
                    break;
                case MotionEventActions.Up:
                    end = e.Event.GetX();

                    if (end < start && Math.Abs(end - start) > minDistance)
                        keyboard.Visibility = ViewStates.Gone;
                        
                    break;
            }

            e.Handled = false;
        }

        private void cursorLeft(object sender, EventArgs e)
        {
            Console.WriteLine("left");
            //focused.DispatchKeyEvent(new KeyEvent(new KeyEventActions(), Keycode.DpadLeft));
        }

        private void cursorRight(object sender, EventArgs e)
        {
            Console.WriteLine("right");
            //focused.DispatchKeyEvent(new KeyEvent(new KeyEventActions(), Keycode.DpadRight));
        }

        private void delete(object sender, EventArgs e)
        {
            //focused.DispatchKeyEvent(new KeyEvent(new KeyEventActions(), Keycode.Del));
            Console.WriteLine("delete");
        }

        private void enterIsItNecessary(object sender, EventArgs e)
        {
            Console.WriteLine("enter");
        }
    }
}


