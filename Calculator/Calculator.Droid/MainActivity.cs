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
    [Activity (Label = "CalcuPad - Debug", MainLauncher = true, Icon = "@drawable/icon")]
	public partial class MainActivity : Activity, IGraphicsHandler
	{
        public static string screenSize = "xLarge";

        public static RelativeLayout canvas;
        private TableLayout keyboard;
        private LinearLayout functionalityMenu;
        
        protected override void OnCreate (Bundle bundle)
		{
            //Bogus operation to remove later lag - problem with dynamic?
            new Expression().FixWeirdError();

            base.OnCreate (bundle);
            
            //Set view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

            Format.os = "android";

            //Assign all reference variables
            canvas = FindViewById<RelativeLayout>(Resource.Id.canvas);
            Equation.canvas = canvas;

            keyboard = FindViewById<TableLayout>(Resource.Id.keyboard);
            functionalityMenu = FindViewById<LinearLayout>(Resource.Id.FunctionalityLayout);

            //Assign other necessary variables
            Input.graphicsHandler = this;
            Input.layoutRoot = canvas;
            Input.cursor = AddCursor();

            //Hide functionality menu
            functionalityMenu.Visibility = ViewStates.Gone;

            //Disable system keyboard
            Window.SetSoftInputMode(SoftInput.StateAlwaysHidden);

            //Assign actions on button clicks
            //FindViewById<Button>(Resource.Id.cursorLeft).Click += cursorLeft;
            //FindViewById<Button>(Resource.Id.cursorRight).Click += cursorRight;
            //FindViewById<Button>(Resource.Id.delete).Click += delete;
            //FindViewById<Button>(Resource.Id.enter).Click += enterIsItNecessary;
            FindViewById<Button>(Resource.Id.FunctionalityButton).Click += openFunctionalityMenu;
            //var functionalityList = FindViewById<ListView>(Resource.Id.FunctionalityListView);
            //functionalityList.Adapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleListItem1, MathView.supportedFunctions);

            foreach (string s in MathView.supportedFunctions.Keys)
            {
                TextView temp = new TextView(this);
                temp.Text = s;
                temp.TextSize = 25;

                temp.SetPadding(0, 10, 0, 10);
                temp.Gravity = GravityFlags.Center;

                temp.Touch += ItemTouched;

                FindViewById<LinearLayout>(Resource.Id.scrollingLinearLayout).AddView(temp);
            }

            /*functionalityList.ItemLongClick += (object sender, AdapterView.ItemLongClickEventArgs e) =>
            {
                View temp = sender as View;
                (temp as TextView).Text = "testing";
                print.log("item number " + (int)e.Id + " clicked");
                var data = ClipData.NewPlainText("name", "Element 1");
                temp.StartDrag(data, new View.DragShadowBuilder(temp), null, 0);
            };*/

            canvas.Drag += dropEquation;

            //Assign click listeners to all buttons on the keyboard, and do some formatting
            for (int i = 0; i < keyboard.ChildCount; i++)
            {
                TableRow row = (TableRow)keyboard.GetChildAt(i);
                for (int j = 0; j < row.ChildCount; j++)
                {
                    Button key = (Button)row.GetChildAt(j);
                    key.Touch += keyboardTouch;
                    key.LongClick += Cursor;

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

            //Define what happens when the canvas is touched
            canvas.Touch += canvasTouch;
		}

        private void Cursor(object sender, View.LongClickEventArgs e)
        {
            Vibrator v = (Vibrator)GetSystemService(VibratorService);
            v.Vibrate(1000);
        }

        //Send key strokes to Input class
        private void KeyPress(object sender, EventArgs e)
        {
            Button sent = (Button)sender;

            if (MathText.selected != null)
            {
                Input.Send(sent.Text);
            }
        }


        private void ItemTouched(object sender, View.TouchEventArgs e)
        {
            TextView temp = sender as TextView;

            var data = ClipData.NewPlainText("canvas", temp.Text);
            temp.StartDrag(data, new View.DragShadowBuilder(temp), null, 0);
        }

        private void dropEquation(object sender, View.DragEventArgs e)
        {
            var evt = e.Event;

            switch (evt.Action)
            {
                case DragAction.Started:
                    /* To register your view as a potential drop zone for the current view being dragged
                     * you need to set the event as handled
                     */
                    e.Handled = true;

                    /* An important thing to know is that drop zones need to be visible (i.e. their Visibility)
                     * property set to something other than Gone or Invisible) in order to be considered. A nice workaround
                     * if you need them hidden initially is to have their layout_height set to 1.
                     */

                    break;
                case DragAction.Entered:
                case DragAction.Exited:
                    /* These two states allows you to know when the dragged view is contained atop your drop zone.
                     * Traditionally you will use that tip to display a focus ring or any other similar mechanism
                     * to advertise your view as a drop zone to the user.
                     */

                    break;
                case DragAction.Drop:
                    /* This state is used when the user drops the view on your drop zone. If you want to accept the drop,
                     *  set the Handled value to true like before.
                     */
                    e.Handled = true;
                    /* It's also probably time to get a bit of the data associated with the drag to know what
                     * you want to do with the information.
                     */

                    if (evt.ClipData.Description.Label != "canvas")
                    {
                        return;
                    }

                    var data = e.Event.ClipData.GetItemAt(0).Text;

                    print.log("it worked " + data);

                    LinearLayout layout = new LinearLayout(this);
                    RelativeLayout.LayoutParams param = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
                    param.SetMargins((int)e.Event.GetX(), (int)e.Event.GetY(), 0, 0);
                    layout.LayoutParameters = param;
                    layout.SetGravity(GravityFlags.Center);
                    canvas.AddView(layout);

                    /*Input.selected = new Input(new MathView(layout));
                    Input.selected.text = Input.Wrap(MathView.supportedFunctions[data].ToArray());
                    Input.selected.mathView.SetText(Input.Parse(Input.selected.text));*/

                    functionalityMenu.Visibility = ViewStates.Gone;

                    break;
                case DragAction.Ended:
                    /* This is the final state, where you still have possibility to cancel the drop happened.
                     * You will generally want to set Handled to true.
                     */
                    e.Handled = true;
                    break;
            }
        }

        private void SymbolHover(object sender, View.DragEventArgs e)
        {
            var evt = e.Event;
            switch (evt.Action)
            {
                case DragAction.Started:
                    e.Handled = true;
                    break;
                case DragAction.Location:
                    View child = sender as View;
                    ViewGroup parent = child.Parent as ViewGroup;

                    float location = evt.GetX() / (sender as View).Width;
                    int index = parent.IndexOfChild(child) + (int)Math.Round(location);

                    if ((Input.cursor as View).Parent == parent && parent.IndexOfChild(Input.cursor as View) < parent.IndexOfChild(child))
                    {
                        index--;
                    }

                    if (index != parent.IndexOfChild(Input.cursor as View))
                    {
                        if ((Input.cursor as View).Parent != null)
                        {
                            ((Input.cursor as View).Parent as ViewGroup).RemoveView(Input.cursor as View);
                        }
                        parent.AddView(Input.cursor as View, index);

                        try
                        {
                            Input.selected.SetCursor(parent.GetChildAt(parent.IndexOfChild(Input.cursor as View) + 1));
                        }
                        catch
                        {
                            Input.selected.SetCursor(parent.GetChildAt(parent.IndexOfChild(Input.cursor as View) - 1), 1);
                        }
                    }

                    break;
                case DragAction.Drop:
                    e.Handled = true;
                    break;
                case DragAction.Ended:
                    e.Handled = true;
                    break;
            }
        }

        private void toggleAnswer(object sender, EventArgs e)
        {
            Expression.showDecimal = !Expression.showDecimal;
        }

        private void LongClicked(object sender, View.LongClickEventArgs e)
        {
            Vibrator v = (Vibrator)GetSystemService(VibratorService);
            v.Vibrate(3000);

            ((View)sender).Touch += Touching;
        }

        //Not in use
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

        //Define what happens when canvas is touched
        private void canvasTouch(object sender, View.TouchEventArgs touchArgs) {
            //Leave if touch is not being released
            MotionEvent e = touchArgs.Event;
            if (!touchArgs.Event.Action.Equals(MotionEventActions.Up))
                return;
            Console.WriteLine("canvas touched");

            if (selected != null)
            {
                selected.SetBackgroundColor(Color.Transparent);
            }

            //Hide functionality menu, add show keyboard
            functionalityMenu.Visibility = ViewStates.Gone;
            ShowKeyboard();

            //Try to remove focus from previous expression
            if (Input.selected != null)
                Input.selected.Key("exit edit mode");

            //Create new layout for expression
            /*LinearLayout layout = new LinearLayout(this);
            RelativeLayout.LayoutParams param = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            param.SetMargins((int)e.GetX(), (int)e.GetY(), 0, 0);
            layout.LayoutParameters = param;
            layout.SetGravity(GravityFlags.Center);
            canvas.AddView(layout);

            Input.selected = new Input(new MathView(layout));
            Input.selected.mathView.SetText(Input.selected.text);*/

            LinearLayout layout = new Equation().root as LinearLayout;
            RelativeLayout.LayoutParams param = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            param.SetMargins((int)e.GetX(), (int)e.GetY(), 0, 0);
            layout.LayoutParameters = param;
            layout.SetGravity(GravityFlags.Center);
        }

        private float start, end;
        private static int minDistance = 100;

        //Check for swipe to dismiss keyboard
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
                        HideKeyboard();

                    break;
                default:
                    if (0 == 1)
                    {
                        int[] coords = new int[2];
                        keyboard.GetLocationOnScreen(coords);
                        Input.selected.SetCursor(Math.Max(0, Math.Min(1, (e.Event.RawX - coords[0]) / keyboard.Width)));
                    }

                    break;
            }

            e.Handled = false;
        }

        private void openFunctionalityMenu(object sender, EventArgs e)
        {
            functionalityMenu.Visibility = ViewStates.Visible;
        }

        private void HideKeyboard()
        {
            keyboard.Visibility = ViewStates.Gone;
        }

        private void ShowKeyboard()
        {
            keyboard.Visibility = ViewStates.Visible;
        }

        //Not sure if this will be needed yet
        private void symbolTouch(object sender, View.TouchEventArgs e)
        {
            if (Input.selected != null)
                Input.selected.Key("exit edit mode");

            TextView temp = sender as TextView;

            var data = ClipData.NewPlainText("name", temp.Text);
            temp.StartDrag(data, new View.DragShadowBuilder(Input.cursor as View), null, 0);

            /*switch (e.Event.Action)
            {
                case MotionEventActions.Down:
                    print.log("down");
                    break;
                case MotionEventActions.Up:
                    print.log("up");
                    break;
                case MotionEventActions.HoverEnter:
                    print.log("hover enter");
                    break;
                case MotionEventActions.HoverMove:
                    print.log("hover move");
                    break;
                case MotionEventActions.Outside:
                    print.log("outside");
                    break;
            }

            e.Handled = false;*/

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

        /*private void cursorLeft(object sender, EventArgs e)
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
        }*/
    }
}


