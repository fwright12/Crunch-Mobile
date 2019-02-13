using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Graphics;

namespace Calculator.Droid
{
    public partial class MainActivity
    {
        private TableLayout keyboard;
        private LinearLayout arrowKeys;

        public void KeyboardSetup()
        {
            keyboard = FindViewById<TableLayout>(Resource.Id.keyboard);
            //keyboard.Measure((canvas.Parent as View).Width, (canvas.Parent as View).Height);

            FindViewById<Button>(Resource.Id.rightArrow).Click += delegate { Control<View, ViewGroup>.Right(); };
            FindViewById<Button>(Resource.Id.leftArrow).Click += delegate { Control<View, ViewGroup>.Left(); };
            FindViewById<Button>(Resource.Id.delete).Click += delegate { Control<View, ViewGroup>.Delete(); };

            arrowKeys = FindViewById<LinearLayout>(Resource.Id.arrowKeysLayout);
            //arrowKeys.LayoutParameters = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, keyboard.meas);
            arrowKeys.BringToFront();
            arrowKeys.SetBackgroundColor(new Color(72, 72, 72, 225));
            arrowKeys.Visibility = ViewStates.Gone;
            //arrowKeys.Touch += delegate { RevertFromCursorMode(); };
            //arrowKeys.SetBackgroundColor(Color.Transparent);
            //arrowKeys.Touch += KeyTouch;
            //arrowKeys.Clickable = false;
            /*arrowKeys.LongClick += (object sender, View.LongClickEventArgs e) =>
            {
                //KeyboardCursorMode();
                print.log("long click");
                e.Handled = false;
            };*/

            var temp = (keyboard.GetChildAt(0) as TableRow).GetChildAt(0);
            //temp.Measure((canvas.Parent as View).Width, (canvas.Parent as View).Height);

            //Assign click listeners to all buttons on the keyboard, and do some formatting
            for (int i = 0; i < keyboard.ChildCount; i++)
            {
                TableRow row = (TableRow)keyboard.GetChildAt(i);
                for (int j = 0; j < row.ChildCount; j++)
                {
                    Button key = (Button)row.GetChildAt(j);

                    /*key.Touch += (object sender, View.TouchEventArgs e) =>
                    {
                        print.log("key touched");
                        //(sender as Button).PerformClick();
                        //(sender as Button).Pressed = true;
                        e.Handled = false;
                    };*/
                    key.Touch += KeyTouch;
                    if (!key.HasOnClickListeners)
                    {
                        key.Click += (object sender, EventArgs e) =>
                        {
                            Control<View, ViewGroup>.KeyboardInput((sender as Button).Text);
                        };
                    }
                    key.LongClick += (object sender, View.LongClickEventArgs e) =>
                    {
                        (sender as Button).Pressed = false;
                        KeyboardCursorMode();
                        e.Handled = false;
                    };

                    key.TextSize = 40;
                }
            }
        }

        private float startPosition;
        private DateTime startTime;
        private static int minDistance = 100;

        private void KeyTouch(object sender, View.TouchEventArgs e)
        {
            switch (e.Event.Action)
            {
                case MotionEventActions.Down:
                    startPosition = e.Event.GetX();
                    startTime = DateTime.Now;

                    View cursor = Control<View, ViewGroup>.cursor;
                    int[] pos = new int[2];
                    cursor.GetLocationInWindow(pos);
                    Control<View, ViewGroup>.lastPos = new Point(pos[0], pos[1] + 500);// - cursor.Height * 1.5f);

                    break;
                case MotionEventActions.Up:
                    float endPosition = e.Event.GetX();

                    if (arrowKeys.IsShown)
                    {
                        RevertFromCursorMode();
                    }
                    //Check for swipe to dismiss keyboard
                    else if (endPosition < startPosition && Math.Abs(endPosition - startPosition) > minDistance && (DateTime.Now - startTime).TotalMilliseconds < 500)
                    {
                        HideKeyboard();
                    }

                    break;
                case MotionEventActions.Move:
                    if (arrowKeys.IsShown)
                    {
                        Control<View, ViewGroup>.MovePhantomCursor(new Point(e.Event.RawX, e.Event.RawY));
                    }

                    break;
            }

            e.Handled = false;
        }

        public void KeyboardCursorMode()
        {
            arrowKeys.Visibility = ViewStates.Visible;

            View phantomCursor = Control<View, ViewGroup>.phantomCursor;
            phantomCursor.Visibility = ViewStates.Visible;

            View cursor = Control<View, ViewGroup>.cursor;
            int[] pos = new int[2];
            cursor.GetLocationInWindow(pos);
            //print.log(pos[0] + ", " + pos[1] +", "+ (GraphicsEngine.canvas as View).Top);
            phantomCursor.SetX(pos[0]);
            phantomCursor.SetY(pos[1]);// - (Input.selected.cursorObject as View).Height * 2);

            int[] d = new int[2];
            phantomCursor.GetLocationInWindow(d);
            //print.log(pos[1] - d[1]);
            phantomCursor.SetY(pos[1] - cursor.Height * 1.5f);

            Vibrator v = (Vibrator)GetSystemService(VibratorService);
            v.Vibrate(100);
        }

        public void RevertFromCursorMode()
        {
            arrowKeys.Visibility = ViewStates.Gone;
            //arrowKeys.SetBackgroundColor(Color.Transparent);
            Control<View, ViewGroup>.phantomCursor.Visibility = ViewStates.Gone;
        }

        public void HideKeyboard()
        {
            keyboard.Visibility = ViewStates.Gone;
        }

        public void ShowKeyboard()
        {
            keyboard.Visibility = ViewStates.Visible;
        }
    }
}