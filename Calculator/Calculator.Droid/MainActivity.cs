using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Util;
using System.Collections.Generic;

namespace Calculator.Droid
{
    [Activity (Label = "Calculator.Droid", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : Activity, IGraphicsHandler
	{
        public static string screenSize = "xLarge";

        private DroidGraphics display;
        public RelativeLayout canvas;
        private TableLayout keyboard;

        public Dictionary<LinearLayout, Expression> expressions = new Dictionary<LinearLayout, Expression>();

        int count = 1;
        EditText focused;

        //Raw input (ie EditText)
        //Parse for numbers, operands, and functions
        //Output for display

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
            
            // Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

            /*Number n = new Number(0);
            TextView temp = new TextView(this);
            temp.Text = "Number";
            n.graphicalObject = temp;

            Number na = new Number(1);
            temp = new TextView(this);
            temp.Text = "Another number";
            na.graphicalObject = temp;

            Console.WriteLine(((TextView)n.graphicalObject).Text + ", " + ((TextView)na.graphicalObject).Text);*/

            //Bogus operation to remove later lag - problem with dynamic?
            new Operation(new Operand("+"), new Number(1)).operate(new Number(1));

            display = new DroidGraphics(this);
            Graphics.graphicsHandler = this;

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
                        key.Click += input;
                    }

                    if (screenSize == "xLarge")
                    {
                        key.TextSize = 40;
                    }
                }
            }

            canvas.Touch += canvasTouch;
            //keyboard.Touch += keyboardTouch;
		}

        private EditText editField;

        private void input(object sender, EventArgs e)
        {
            Button sent = (Button)sender;

            //Input.Key(sent.Text);

            //Console.WriteLine(((LinearLayout)Input.UI.graphicalObject).ChildCount);
            if (((LinearLayout)Input.UI.graphicalObject).ChildCount == 1)
                ((LinearLayout)Input.UI.graphicalObject).AddView((View)Create(new Operand(" = ")), 0);

            if (Input.IsNumber(sent.Text))
            {
                if (editField == null)
                {
                    editField = new EditText(this);
                    editField.InputType = Android.Text.InputTypes.NumberFlagDecimal | Android.Text.InputTypes.NumberFlagSigned;
                    editField.TextSize = 35;
                    //Insert()
                    //((LinearLayout)Input.selected.graphicalObject).AddView(editText);

                    //editText.TextChanged += textChanged;
                    //editText.FocusChange += focusChanged;

                    editField.RequestFocus();

                    ((LinearLayout)Input.UI.graphicalObject).AddView(editField, Input.pos);

                    //Input.editing = new Number(sent.Text);
                    //Input.selected.Insert(editing);
                }

                editField.DispatchKeyEvent(new KeyEvent(0, sent.Text, 0, new KeyEventFlags()));
                //Input.editing = new Number(editing.Text);
                Input.Key(editField.Text);
            }
            else
            {
                tryEditTextExit();

                Input.Key(sent.Text);
            }

            /*Console.WriteLine(sent.Text);
            switch (inputType(sent.Text))
            {
                case "number":
                    if (Input.selected != null)
                    {
                        if (enter == null)
                        {
                            Console.WriteLine("in");
                            EditText editText = new EditText(this);
                            editText.TextSize = 35;
                            ((LinearLayout)Input.selected.graphicalObject).AddView(editText);

                            //editText.TextChanged += textChanged;
                            //editText.FocusChange += focusChanged;

                            enter = editText;
                            enter.RequestFocus();
                        }

                        enter.DispatchKeyEvent(new KeyEvent(0, sent.Text, 0, new KeyEventFlags()));

                        //Console.WriteLine(display.answer);
                    }
                    break;
                default:
                    tryEditTextExit();

                    TextView text = new TextView(this);
                    text.TextSize = 40;
                    text.Text = sent.Text;
                    display.Add(text);

                    break;
            }

            display.Evaluate(display.selected);*/
        }

        private void tryEditTextExit()
        {
            if (Input.UI != null && editField != null)
            {
                //int index = ((LinearLayout)editing.parent.graphicalObject).IndexOfChild((View)editing.graphicalObject);
                //Remove(editing);
                ((LinearLayout)Input.UI.graphicalObject).RemoveView(editField);
                Input.Key("exit edit mode");

                //string text = ((EditText)editing.graphicalObject).Text;
                //print.log("SDFSDSDF " + editing.value);
                //editing.graphicalObject = Create(editing);
                //editing.value = text;

                editField = null;
            }
        }

        public object Create(Symbol sender)
        {
            TextView text = new TextView(this);
            text.TextSize = 40;
            text.Text = sender.text;

            return text;
        }

        public object Create(Fraction sender)
        {

            throw new NotImplementedException();
        }

        public object Create(Expression sender)
        {
            LinearLayout layout = new LinearLayout(this);
            layout.Orientation = Orientation.Horizontal;
            //canvas.AddView(layout);

            return layout;
        }

        public void AddChild(object parent, object child, int index)
        {
            Console.WriteLine(parent.GetType() + ", " + child.GetType() + ", " + index);
            ((ViewGroup)parent).AddView((View)child, index);
        }

        public void RemoveChild(object parent, int index)
        {
            ((ViewGroup)parent).RemoveViewAt(index);
            //((LinearLayout)sender.parent.graphicalObject).RemoveView((View)sender.graphicalObject);
        }

        public void SendText(Symbol sender, string s)
        {
            //((EditText)sender.graphicalObject).DispatchKeyEvent(new KeyEvent(0, s, 0, new KeyEventFlags()));
        }

        public string GetText(Symbol sender)
        {
            return "";// ((TextView)sender.graphicalObject).Text;
        }

        public void SetText(object parent, int index, string s)
        {
            ((TextView)((ViewGroup)parent).GetChildAt(index)).Text = s;
            //((TextView)sender.graphicalObject).Text = s;
        }

        private void canvasTouch(object sender, View.TouchEventArgs touchArgs) {
            //Console.WriteLine("touch" + touchArgs.Event.Action.Equals(MotionEventActions.Up) +", "+ FindViewById<Button>(Resource.Id.button1).Height);

            //if (focused != null)
            //  focused.ClearFocus();

            tryEditTextExit();

            MotionEvent e = touchArgs.Event;
            if (!touchArgs.Event.Action.Equals(MotionEventActions.Up))
                return;
            Console.WriteLine("touched");

            /*if (focused != null && focused.HasFocus)
            {
                Console.WriteLine("click off");
                focused.ClearFocus();
                return;
            }*/

            //Expression temp = Input.Touch(canvas);

            LinearLayout layout = Input.Touch(canvas).graphicalObject as LinearLayout;
            RelativeLayout.LayoutParams param = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            param.SetMargins((int)e.GetX(), (int)e.GetY(), 0, 0);
            layout.LayoutParameters = param;
            layout.Touch += blockTouch;

            //temp.graphicalObject = layout;

            /*EditText editText = new EditText(this);
            editText.TextSize = 40;
            editText.TextChanged += textChanged;
            editText.FocusChange += focusChanged;
            //editText.Click += delegate
            //{
            //    requestFocus(editText);
            //};
            requestFocus(editText);*/

            /*TextView text = new TextView(this);
            text.TextSize = 40;
            text.Text = " = error";
            //layout.AddView(editText);
            layout.AddView(text);*/

            //display.Create(layout);
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
                        //Console.WriteLine("swiped left");
                        keyboard.Visibility = ViewStates.Gone;
                        
                    break;
            }

            e.Handled = false;
        }

        private void textChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            LinearLayout parent = (LinearLayout)((EditText)sender).Parent;

            //((TextView)parent.GetChildAt(1)).Text = "= " + Expression.evaluate(((EditText)sender).Text);
            Console.WriteLine("Text is different");
        }

        private void focusChanged(object sender, View.FocusChangeEventArgs e)
        {
            EditText sent = (EditText)sender;

            if (e.HasFocus)
            {
                Console.WriteLine("focused on " + sent.Text);

                focused = sent;

                //if (sent.GetX() < 400)
                  //  canvas.RemoveView((LinearLayout)sent.Parent);
            }
            else
            {
                //if (sent.Text == "")
                 //   canvas.RemoveView((LinearLayout)sent.Parent);

                //focused = null;

                Console.WriteLine("lost focus on " + sent.Text);
            }
        }

        private void blockTouch(object sender, View.TouchEventArgs e)
        {
            Console.WriteLine("linear layout touched");

            e.Handled = true;
        }

        private void cursorLeft(object sender, EventArgs e)
        {
            Console.WriteLine("left");
            focused.DispatchKeyEvent(new KeyEvent(new KeyEventActions(), Keycode.DpadLeft));
        }

        private void cursorRight(object sender, EventArgs e)
        {
            Console.WriteLine("right");
            focused.DispatchKeyEvent(new KeyEvent(new KeyEventActions(), Keycode.DpadRight));
        }

        private void delete(object sender, EventArgs e)
        {
            focused.DispatchKeyEvent(new KeyEvent(new KeyEventActions(), Keycode.Del));
            Console.WriteLine("delete");
        }

        private void enterIsItNecessary(object sender, EventArgs e)
        {
            Console.WriteLine("enter");
        }
    }
}


