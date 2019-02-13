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
            var canvas = GraphicsEngine.canvas as View;

            keyboard = FindViewById<TableLayout>(Resource.Id.keyboard);
            keyboard.Measure((canvas.Parent as View).Width, (canvas.Parent as View).Height);

            FindViewById<Button>(Resource.Id.rightArrow).Click += delegate { RightArrow(); };
            FindViewById<Button>(Resource.Id.leftArrow).Click += delegate { LeftArrow(); };

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
            temp.Measure((canvas.Parent as View).Width, (canvas.Parent as View).Height);

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
                            Input.Send((sender as Button).Text);
                        };
                    }
                    key.LongClick += (object sender, View.LongClickEventArgs e) =>
                    {
                        (sender as Button).Pressed = false;
                        KeyboardCursorMode();
                        e.Handled = false;
                    };

                    if (screenSize == "xLarge")
                    {
                        key.TextSize = 40;
                    }
                }
            }
        }

        private float startPosition;
        private DateTime startTime;
        private static int minDistance = 100;
        private float[] lastPos = new float[2];
        float increase = 1f;

        private void KeyTouch(object sender, View.TouchEventArgs e)
        {
            switch (e.Event.Action)
            {
                case MotionEventActions.Down:
                    startPosition = e.Event.GetX();
                    startTime = DateTime.Now;

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
                        View cursor = GraphicsEngine.phantomCursor as View;
                        ViewGroup parent = GraphicsEngine.canvas as ViewGroup;

                        //print.log(e.Event.RawY + ", " + e.Event.GetY());
                        float x = cursor.GetX() + (e.Event.RawX - lastPos[0]) * increase;
                        float y = cursor.GetY() + (e.Event.RawY - lastPos[1]) * increase;

                        cursor.SetX(Math.Max(Math.Min(parent.Width - cursor.Width, x), 0));
                        cursor.SetY(Math.Max(Math.Min(parent.Height - cursor.Height, y), 0));

                        /*int[] window = new int[2];
                        keyboard.GetLocationInWindow(window);

                        float expand = 1.5f;
                        float x = Math.Max(0, Math.Min(1, e.Event.RawX / keyboard.Width * expand - (expand - 1f) / 2f));
                        float y = Math.Max(0, Math.Min(1, (e.Event.RawY - window[1]) / keyboard.Height * expand - (expand - 1f) / 2f));
                        
                        cursor.SetX(Math.Min(x * layout.Width, layout.Width - cursor.Width));
                        cursor.SetY(Math.Min(y * layout.Height, layout.Height - cursor.Height));*/

                        CheckOverlap();
                    }

                    break;
            }

            lastPos = new float[] { e.Event.RawX, e.Event.RawY };

            e.Handled = false;
        }

        public View GetViewAt(ViewGroup parent, float x, float y)
        {
            for (int i = 0; i < parent.ChildCount; i++)
            {
                var child = parent.GetChildAt(i);

                int[] bounds = new int[2];
                child.GetLocationInWindow(bounds);

                if (x >= bounds[0] && x <= bounds[0] + child.Width && y >= bounds[1] && y <= bounds[1] + child.Height)
                {
                    if (child is ViewGroup)
                    {
                        return GetViewAt(child as ViewGroup, x, y);
                    }
                    else
                    {
                        return child;
                    }
                }
            }

            return parent;
        }

        public void CheckOverlap()
        {
            View Cursor = GraphicsEngine.phantomCursor as View;
            int[] bounds = new int[2];
            Cursor.GetLocationInWindow(bounds);
            float x = bounds[0] + Cursor.Width / 2;
            float y = bounds[1] + Cursor.Height / 2;

            View view = GetViewAt(GraphicsEngine.canvas as ViewGroup, x, y);
            view.GetLocationInWindow(bounds);
            var leftOrRight = (int)Math.Round((double)(x - bounds[0]) / view.Width);
            Input.selected.MoveCursor(view, view.PaddingLeft > 0, view.PaddingRight > 0, leftOrRight);
        }

        public void KeyboardCursorMode()
        {
            arrowKeys.Visibility = ViewStates.Visible;

            View cursor = GraphicsEngine.phantomCursor as View;
            cursor.Visibility = ViewStates.Visible;

            int[] pos = new int[2];
            (Input.selected.cursorObject as View).GetLocationInWindow(pos);
            print.log(pos[0] + ", " + pos[1] +", "+ (GraphicsEngine.canvas as View).Top);
            cursor.SetX(pos[0]);
            cursor.SetY(pos[1]);// - (Input.selected.cursorObject as View).Height * 2);
            int[] d = new int[2];
            cursor.GetLocationInWindow(d);
            print.log(pos[1] - d[1]);
            cursor.SetY(pos[1] - (Input.selected.cursorObject as View).Height * 1.5f);

            Vibrator v = (Vibrator)GetSystemService(VibratorService);
            v.Vibrate(100);
        }

        public void RevertFromCursorMode()
        {
            arrowKeys.Visibility = ViewStates.Gone;
            //arrowKeys.SetBackgroundColor(Color.Transparent);
            (GraphicsEngine.phantomCursor as View).Visibility = ViewStates.Gone;
        }

        public void RightArrow()
        {
            print.log("right arrow");
        }

        public void LeftArrow()
        {
            print.log("left arrow");
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