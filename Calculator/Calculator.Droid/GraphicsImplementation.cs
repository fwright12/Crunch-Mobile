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
    public partial class MainActivity : Activity, IGraphicsHandler
    {
        public object AddLayout(Format f)
        {
            if (f == null)
                f = new Format();

            LinearLayout layout = new LinearLayout(this);
            layout.SetGravity(GravityFlags.Center);

            if (f.Orientation == "horizontal")
                layout.Orientation = Orientation.Horizontal;
            else
                layout.Orientation = Orientation.Vertical;

            layout.SetPadding(0, 0, 0, f.Padding);
            //layout.Click += symbolTouch;

            return layout;
        }

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

        public object AddEditField()
        {
            EditText temp = new EditText(this);
            temp.InputType = InputTypes.NumberFlagDecimal | InputTypes.NumberFlagSigned;
            temp.TextSize = 35;
            temp.RequestFocus();
            temp.Click += symbolTouch;

            return temp;
        }

        public void Superscript(object sender)
        {
            ((TextView)sender).Text = Html.FromHtml("<p>" + ((TextView)sender).Text.ToString() + "</p>").ToString();
        }

        public string DispatchKey(string text)
        {
            ((EditText)Input.editField).DispatchKeyEvent(new KeyEvent(0, text, 0, new KeyEventFlags()));

            return ((EditText)Input.editField).Text;
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