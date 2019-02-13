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

        private void KeyboardSetup()
        {
            keyboard = FindViewById<TableLayout>(Resource.Id.keyboard);
            arrowKeys = FindViewById<LinearLayout>(Resource.Id.arrowKeysLayout);

            arrowKeys.BringToFront();
            arrowKeys.Visibility = ViewStates.Gone;
            arrowKeys.Touch += RevertFromCursorMode;

            //Assign click listeners to all buttons on the keyboard, and do some formatting
            for (int i = 0; i < keyboard.ChildCount; i++)
            {
                TableRow row = (TableRow)keyboard.GetChildAt(i);
                for (int j = 0; j < row.ChildCount; j++)
                {
                    Button key = (Button)row.GetChildAt(j);
                    /*key.Touch += (object sender, View.TouchEventArgs e) =>
                    {
                        e.Handled = false;
                    };
                    key.LongClick += (object sender, View.LongClickEventArgs e) =>
                    {
                        e.Handled = false;
                    };*/

                    key.Touch += KeyTouch;
                    key.LongClick += KeyboardCursorMode;

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
        }

        private float startPosition;
        private DateTime startTime;
        private static int minDistance = 100;

        public event OverlapListener checkOverlap;

        private void KeyTouch(object sender, View.TouchEventArgs e)
        {
            Button key = sender as Button;

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
                        (Input.selected.graphicsEngine.AndroidGraphics.root.Parent as ViewGroup).RemoveView(Input.phantomCursor as View);

                        arrowKeys.Visibility = ViewStates.Gone;
                    }
                    //Check for swipe to dismiss keyboard
                    else if (endPosition < startPosition && Math.Abs(endPosition - startPosition) > minDistance)// && (DateTime.Now - startTime).TotalMilliseconds < 500)
                    {
                        HideKeyboard();
                    }

                    break;
                case MotionEventActions.Move:
                    if (arrowKeys.IsShown)
                    {
                        int[] window = new int[2];
                        keyboard.GetLocationInWindow(window);

                        float expand = 1.5f;
                        float x = Math.Max(0, Math.Min(1, e.Event.RawX / keyboard.Width * expand - (expand - 1f) / 2f));
                        float y = Math.Max(0, Math.Min(1, (e.Event.RawY - window[1]) / keyboard.Height * expand - (expand - 1f) / 2f));

                        View s = sender as View;
                        ViewGroup v = Input.selected.graphicsEngine.AndroidGraphics.root.GetChildAt(0) as ViewGroup;
                        View c = Input.phantomCursor as View;

                        c.SetX(Math.Min(x * v.Width, v.Width - c.Width));
                        c.SetY(Math.Min(y * v.Height, v.Height - c.Height));

                        Input.selected.graphicsEngine.AndroidGraphics.CheckOverlap();

                        //checkOverlap();

                        /*View child;
                        try
                        {
                            child = temp.views[temp.shown[index]].child as View;
                        }
                        catch
                        {
                            child = temp.views[temp.shown[temp.shown.Count - 1]].child as View;
                        }

                        ViewGroup parent = child.Parent as ViewGroup;
                        if ((Input.cursor as View).Parent != null)
                        {
                            ((Input.cursor as View).Parent as ViewGroup).RemoveView(Input.cursor as View);
                        }
                        parent.AddView(Input.cursor as View, parent.IndexOfChild(child));*/

                        /*if (index != parent.IndexOfChild(Input.cursor as View))
                        {
                            if ((Input.cursor as View).Parent != null)
                            {
                                ((Input.cursor as View).Parent as ViewGroup).RemoveView(Input.cursor as View);
                            }
                            parent.AddView(Input.cursor as View, index);
                        }*/

                        //Input.SetCursora(Math.Max(0, Math.Min(1, e.Event.RawX / keyboard.Width)));
                    }

                    break;
            }

            e.Handled = false;
        }

        private void KeyboardCursorMode(object sender, View.LongClickEventArgs e)
        {
            arrowKeys.SetBackgroundColor(new Color(72, 72, 72, 225));
            arrowKeys.LayoutParameters = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, keyboard.Height);
            arrowKeys.Visibility = ViewStates.Visible;

            (Input.selected.graphicsEngine.AndroidGraphics.root.Parent as ViewGroup).AddView(Input.phantomCursor as View);

            Vibrator v = (Vibrator)GetSystemService(VibratorService);
            v.Vibrate(100);

            (sender as Button).Pressed = false;
            e.Handled = false;
        }

        private void RevertFromCursorMode(object sender, View.TouchEventArgs e)
        {
            arrowKeys.Visibility = ViewStates.Gone;

            e.Handled = false;
        }

        private void HideKeyboard()
        {
            keyboard.Visibility = ViewStates.Gone;
        }

        private void ShowKeyboard()
        {
            keyboard.Visibility = ViewStates.Visible;
        }
    }
}