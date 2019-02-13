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
using System.Reflection;

using Graphics;

namespace Calculator
{
    /*public class Texts : Playground.View<Android.Views.View>
    {
        public override int Index
        {
            get
            {
                return (view.Parent as ViewGroup).IndexOfChild(view);
            }
            set
            {
            }
        }

        public override object Parent
        {
            get
            {
                return view.Parent;
            }
            set
            {

            }
        }
    }*/

    public partial class RenderFactory : IRenderFactory<View, ViewGroup>
    {
        public void Add(ViewGroup parent, View child, int index = 0)
        {
            if (child.Parent != null)
            {
                (child.Parent as ViewGroup).RemoveView(child);
            }
            parent.AddView(child, index);
        }

        public void Remove(View sender)
        {
            (sender.Parent as ViewGroup).RemoveView(sender);
        }

        public int GetIndex(View sender)
        {
            return (sender.Parent as ViewGroup).IndexOfChild(sender);
        }

        public View Create(Text sender)
        {
            var temp = new TextView(Application.Context);
            temp.TextSize = 40;
            temp.Text = " " + sender.text + " ";
            return temp;
        }

        public View Create(Number sender)
        {
            var temp = Create(sender as Text) as TextView;
            temp.Text = sender.text;
            return temp;
        }

        public ViewGroup Create(Graphics.Layout sender)
        {
            return baseLayout();
        }

        public ViewGroup Create(Fraction sender)
        {
            var temp = baseLayout();
            temp.Orientation = Android.Widget.Orientation.Vertical;
            return temp;
        }

        public ViewGroup Create(int sender)
        {
            var temp = baseLayout();
            temp.SetPadding(0, 0, 0, 50);
            return temp;
        }

        public ViewGroup Create(Bar sender)
        {
            var temp = new LinearLayout(Application.Context);
            temp.SetPadding(0, 0, 0, 3);
            temp.SetBackgroundColor(Color.White);
            return temp;
        }

        public ViewGroup Create(Equation sender)
        {
            return baseLayout();

            //var layout = new RelativeLayout(this);
            //layout.SetGravity(GravityFlags.Center);
            //canvas.AddView(layout);

            var root = baseLayout();
            //(layout.GetChildAt(0) as ViewGroup).AddView((View)(Input.cursor = Cursor()));
            root.AddView((View)(Input.cursor = Cursor()));
            Input.selected.root = root;
            return root;
        }

        public ViewGroup Create(string sender)
        {
            var temp = baseLayout();
            //temp.BringToFront();
            temp.Click += AnswerToggle;
            return temp;
        }

        private void AnswerToggle(object sender, EventArgs e)
        {
            Input.selected.displayAnswerAsFraction = !Input.selected.displayAnswerAsFraction;
            //Input.selected.graphicsEngine.SetText(Input.selected.GetText());
        }

        public ViewGroup Create(Graphics.Space sender)
        {
            var temp = baseLayout();
            temp.SetMinimumHeight((Input.cursor as View).Height);
            temp.SetMinimumWidth((Input.cursor as View).Width);
            return temp;
        }

        public ViewGroup CreateRoot()
        {
            return new RelativeLayout(Application.Context);
        }

        public ViewGroup GetParent(View sender)
        {
            return sender.Parent as ViewGroup;
        }

        public ViewGroup BaseLayout()
        {
            return baseLayout();
        }

        public LinearLayout baseLayout()
        {
            var temp = new LinearLayout(Application.Context);
            temp.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            temp.SetGravity(GravityFlags.Center);
            //temp.SetPadding(50, 50, 50, 50);
            return temp;
        }

        public void IsOverlapping(View first, View second, Symbol sender)
        {
            ViewGroup root = Input.selected.root as ViewGroup;

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
                if ((Input.cursor as View).Parent != null)
                {
                    ((Input.cursor as View).Parent as ViewGroup).RemoveView(Input.cursor as View);
                }

                if (first is LinearLayout)
                {
                    (first as ViewGroup).AddView(Input.cursor as View);
                }
                else
                {
                    int leftOrRight = (int)Math.Round((double)(x - loc[0]) / first.Width);
                    ViewGroup parent = first.Parent as ViewGroup;
                    parent.AddView(Input.cursor as View, parent.IndexOfChild(first) + leftOrRight);

                    Input.selected.pos = Input.selected.text.IndexOf(sender) + leftOrRight;
                }
            }
        }

        public void SetPadding(ViewGroup sender, int size)
        {
            sender.SetPadding(0, 0, size, 0);
        }

        public View Create(Cursor sender)
        {
            return Cursor();
        }

        public View Cursor()
        {
            TextView temp = new TextView(Application.Context);
            //temp.SetPadding(3, 0, 0, 0);
            temp.Text = "'";
            temp.TextSize = 40;
            temp.SetBackgroundColor(Color.White);
            temp.SetTextColor(Color.White);

            return temp;
        }
    }
}