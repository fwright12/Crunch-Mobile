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

using Crunch;

//[assembly: Xamarin.Forms.Dependency(typeof(Calculator.Droid.MainActivity))]
namespace Calculator.Droid
{
    [Activity (Label = "CalcuPad - Debug", MainLauncher = true, Icon = "@drawable/icon")]
	public partial class MainActivity : Activity, IUserInterface
    {
        public static string screenSize = "xLarge";

        protected override void OnCreate (Bundle bundle)
		{
            //Bogus operation to remove later lag - problem with dynamic?
            ExtensionMethods.FixDynamicLag("");

            base.OnCreate (bundle);

            SetContentView (Resource.Layout.Main);

            Window.SetSoftInputMode(SoftInput.StateAlwaysHidden);

            var canvas = FindViewById<RelativeLayout>(Resource.Id.canvas);
            canvas.Drag += dropEquation;
            canvas.Touch += canvasTouch;

            //Measure cursor width
            View temp = PhantomCursor() as View; // GraphicsEngine.renderFactory.Create(new Graphics.Text("")) as View;
            temp.Measure((canvas.Parent as View).Width, (canvas.Parent as View).Height);
            GraphicsEngine.cursorWidth = temp.MeasuredWidth;

            //Measure text dimensions
            temp = GraphicsEngine.renderFactory.Create(new Graphics.Number(" ")) as View;
            temp.Measure((canvas.Parent as View).Width, (canvas.Parent as View).Height);
            GraphicsEngine.textHeight = temp.MeasuredHeight;
            GraphicsEngine.textWidth = temp.MeasuredWidth;

            Input.SetUp(this, canvas);
            (GraphicsEngine.phantomCursor as View).Visibility = ViewStates.Gone;
        }

        public object PhantomCursor()
        {
            TextView temp = new TextView(Application.Context);
            temp.TextSize = GraphicsEngine.TextSize;
            temp.SetPadding(1, 0, 1, 0);
            temp.SetBackgroundColor(Color.Red);

            return temp;
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
                    //canvas.AddView(layout);

                    /*Input.selected = new Input(new MathView(layout));
                    Input.selected.text = Input.Wrap(MathView.supportedFunctions[data].ToArray());
                    Input.selected.mathView.SetText(Input.Parse(Input.selected.text));*/

                    HideFunctionsMenu();

                    break;
                case DragAction.Ended:
                    /* This is the final state, where you still have possibility to cancel the drop happened.
                     * You will generally want to set Handled to true.
                     */
                    e.Handled = true;
                    break;
            }
        }

        private void LongClicked(object sender, View.LongClickEventArgs e)
        {
            Vibrator v = (Vibrator)GetSystemService(VibratorService);
            v.Vibrate(3000);

            //((View)sender).Touch += Touching;
        }

        //Define what happens when canvas is touched
        private void canvasTouch(object sender, View.TouchEventArgs touchArgs) {
            MotionEvent e = touchArgs.Event;

            switch (e.Action)
            {
                case MotionEventActions.Up:
                    Console.WriteLine("canvas touched");

                    RelativeLayout layout = new RelativeLayout(this);
                    layout.SetX((int)e.GetX());
                    layout.SetY((int)e.GetY());
                    layout.SetGravity(GravityFlags.Center);

                    new GraphicsEngine(layout);

                    break;
            }
        }

        //Not sure if this will be needed yet
        /*private void symbolTouch(object sender, View.TouchEventArgs e)
        {
            //if (Input.selected != null)
             //   Input.Send("exit edit mode");

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

            e.Handled = false;s

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
            Equation.selected = Equation.findByGraphics[sent];
        }*/

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

        //Deprecated
        /*private void SymbolHover(object sender, View.DragEventArgs e)
        {
            View child = sender as View;
            ViewGroup parent = child.Parent as ViewGroup;
            var evt = e.Event;

            switch (evt.Action)
            {
                case DragAction.Started:
                    e.Handled = true;
                    break;
                case DragAction.Location:
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
                    }

                    break;
                case DragAction.Drop:
                    e.Handled = true;

                    break;
                case DragAction.Ended:
                    e.Handled = true;

                    print.log("ended");

                    try
                    {
                        Input.SetCursor(parent.GetChildAt(parent.IndexOfChild(Input.cursor as View) + 1));
                    }
                    catch
                    {
                        Input.SetCursor(parent.GetChildAt(parent.IndexOfChild(Input.cursor as View) - 1), 1);
                    }

                    break;
            }
        }*/

        //Not in use
        /*private void Touching(object sender, View.TouchEventArgs touchArgs)
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
        }*/
    }
}


