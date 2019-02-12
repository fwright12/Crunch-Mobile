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

namespace Calculator.Droid
{
    public partial class MainActivity : Activity, IGraphicsHandler
    {
        public object AddLayout(Format f)
        {
            LinearLayout layout = new LinearLayout(this);
            layout.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);

            if (f.Orientation == "horizontal")
                layout.Orientation = Android.Widget.Orientation.Horizontal;
            else
                layout.Orientation = Android.Widget.Orientation.Vertical;

            if (f.Gravity == "aabottom")
                layout.SetGravity(GravityFlags.Bottom);
            else
                layout.SetGravity(GravityFlags.Center);

            //layout.SetPadding(10, 10, 10, 10);
            layout.SetPadding(0, 0, 0, f.Padding);
            //layout.Click += symbolTouch;

            if (f.IsAnswer)
                layout.Click += toggleAnswer;

            return layout;
        }

        public object AddText(string text)
        {
            TextView temp = new TextView(this);
            temp.TextSize = 40;

            temp.Text = text;

            temp.Touch += symbolTouch;
            temp.Drag += SymbolHover;

            /*if (Input.clickable)
            {
                temp.Click += symbolTouch;
            }*/

            temp.LongClick += LongClicked;

            return temp;
        }

        public object AddEditField()
        {
            EditText temp = new EditText(this);
            temp.InputType = InputTypes.NumberFlagDecimal | InputTypes.NumberFlagSigned;
            temp.TextSize = 35;
            temp.RequestFocus();
            //temp.Click += symbolTouch;

            return temp;
        }

        public object AddCursor()
        {
            TextView temp = new TextView(this);
            temp.Text = "'";
            temp.TextSize = 40;
            temp.SetBackgroundColor(Color.White);
            temp.SetTextColor(Color.White);
            
            /*//try
            //{
                FieldInfo f = temp.GetType().GetField("mCursorDrawableRes");
                //f.a(true);
                f.SetValue(temp, Resource.Drawable.Cursor);
            //} 
            //catch (Exception ignored)
            //{
            //}*/

            //temp.SetBackgroundColor(Color.Transparent);

            return temp;
        }

        public object AddBar()
        {
            LinearLayout layout = new LinearLayout(this);
            layout.SetPadding(0, 0, 0, 3);
            layout.SetBackgroundColor(Color.White);
            return layout;
        }

        public string DispatchKey(string text)
        {
            ((EditText)Input.editField).DispatchKeyEvent(new KeyEvent(0, text, 0, new KeyEventFlags()));

            return ((EditText)Input.editField).Text;
        }

        private View selected;

        //Not in use
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

        public void Reorder(object child, int pos)
        {
            ViewGroup parent = ((ViewGroup)((View)child).Parent);

            parent.RemoveView(child as View);
            parent.AddView(child as View, pos);
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
    }
}