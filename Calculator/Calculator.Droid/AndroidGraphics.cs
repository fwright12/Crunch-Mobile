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
using System.Linq;
using Android.Text;
using System.Reflection;

using Graphics;

namespace Calculator.Droid
{
    public partial class MainActivity
    {
        public void Add(ViewGroup parent, View child, int index = 0)
        {
            Remove(child);
            parent.AddView(child, index);
        }

        public void Remove(View sender)
        {
            if (sender.Parent != null)
            {
                (sender.Parent as ViewGroup).RemoveView(sender);
            }
        }

        public ViewGroup CreateBaseLayout()
        {
            var temp = new LinearLayout(Application.Context);
            temp.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            temp.SetGravity(GravityFlags.Center);
            temp.SetMinimumHeight(Input.textHeight);
            //temp.SetPadding(5, 5, 5, 5);

            return temp;
        }

        public View Create(Text sender)
        {
            var temp = new TextView(Application.Context);
            temp.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.MatchParent);
            temp.TextSize = Input.TextSize;
            temp.Text = " " + sender.text + " ";
            temp.Gravity = GravityFlags.Center;
            return temp;
        }

        public View Create(Number sender)
        {
            var temp = Create(sender as Text) as TextView;
            temp.Text = sender.text;
            return temp;
        }

        public ViewGroup Create(Expression sender)
        {
            var temp = CreateBaseLayout();

            temp.ChildViewAdded += delegate
            {
                if (temp.ChildCount == 1)
                {
                    temp.LayoutParameters.Width = ViewGroup.LayoutParams.WrapContent;
                }
            };
            temp.ChildViewRemoved += delegate
            {
                if (temp.ChildCount == 0)
                {
                    temp.LayoutParameters.Width = ViewGroup.LayoutParams.MatchParent;
                }
            };

            return temp;
        }

        public ViewGroup Create(Fraction sender)
        {
            var temp = CreateBaseLayout() as LinearLayout;
            temp.Orientation = Orientation.Vertical;

            temp.ChildViewAdded += delegate
            {
                if (temp.ChildCount == 2)
                {
                    var bar = new LinearLayout(Application.Context);
                    bar.SetPadding(0, 0, 0, 3);
                    bar.SetBackgroundColor(Color.White);
                    temp.AddView(bar, 1);
                }
            };

            return temp;
        }

        public View CreateCursor()
        {
            EditText temp = new EditText(Application.Context);
            temp.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.MatchParent);
            temp.TextSize = Input.TextSize;
            //temp.SetPadding(temp.PaddingLeft, 0, temp.PaddingRight, 0);
            temp.SetPadding(1, 0, 1, 0);
            temp.Background = null;
            temp.SetCursorVisible(true);
            temp.Gravity = GravityFlags.Center;

            temp.ViewAttachedToWindow += delegate { temp.RequestFocus(); };

            return temp;
        }

        public View CreatePhantomCursor()
        {
            TextView temp = new TextView(Application.Context)
            {
                TextSize = Input.TextSize
            };
            temp.SetPadding(1, 0, 1, 0);
            temp.SetBackgroundColor(Color.Red);

            return temp;
        }

        private void AnswerToggle(object sender, EventArgs e)
        {
            //Input.selected.displayAnswerAsFraction = !Input.selected.displayAnswerAsFraction;
            //Input.selected.graphicsEngine.SetText(Input.selected.GetText());
        }

        public ViewGroup GetParent(View sender)
        {
            return sender.Parent as ViewGroup;
        }

        public int GetIndex(View sender)
        {
            return GetParent(sender).IndexOfChild(sender);
        }

        public void CheckPadding(ViewGroup parent, bool padLeft, bool padRight)
        {
            parent.SetPadding(padLeft.ToInt() * Input.cursorWidth, 0, padRight.ToInt() * Input.cursorWidth, 0);
                /*(temp.Children.Count > 0 && temp.Children.First() is Graphics.Layout && temp.Children.First().GetType() != typeof(Expression)).ToInt() * Input.cursorWidth,
                0,
                (temp.Children.Count > 0 && temp.Children.Last() is Graphics.Layout && temp.Children.Last().GetType() != typeof(Expression)).ToInt() * Input.cursorWidth,
                0
                );*/
        }

        public void AttachClickListener(View view, Action action)
        {
            view.Click += delegate { action(); };
        }

        public int[] GetDimensions(View sender)
        {
            return new int[] { sender.Width, sender.Height };
        }

        public void SetPosition(View view, float x, float y)
        {
            view.SetX(x);
            view.SetY(y);
        }

        /*public void IsOverlapping(View first, View second, Symbol sender)
        {
            ViewGroup root = null;// Input.selected.root as ViewGroup;

            if (first is ViewGroup && !(sender is Graphics.Space)) //if (first is LinearLayout && (first as ViewGroup).ChildCount > 1 && (first as ViewGroup).GetChildAt(0) != Input.cursor)
            {
                return;
            }

            int[] loc = new int[2], loc2 = new int[2];
            first.GetLocationInWindow(loc);
            second.GetLocationInWindow(loc2);

            float x = loc2[0] + second.Width / 2;
            float y = loc2[1] + second.Height / 2;

            if (x > loc[0] && x < loc[0] + first.Width && y > loc[1] && y < loc[1] + first.Height) //if (x > loc[0] && x < loc[0] + v.Width && y > loc[1] && y < loc[1] + v.Height)
            {
                /*if ((Input.cursor as View).Parent != null)
                {
                    ((Input.cursor as View).Parent as ViewGroup).RemoveView(Input.cursor as View);
                }

                if (first is LinearLayout)
                {
                    //(first as ViewGroup).AddView(Input.cursor as View);
                }
                else
                {
                    int leftOrRight = (int)Math.Round((double)(x - loc[0]) / first.Width);
                    ViewGroup parent = first.Parent as ViewGroup;
                    //parent.AddView(Input.cursor as View, parent.IndexOfChild(first) + leftOrRight);

                    //Input.selected.pos = Input.selected.text.IndexOf(sender) + leftOrRight;
                }
            }
        }*/
    }
}