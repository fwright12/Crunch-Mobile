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
    public partial class MainActivity : IRenderer<View, ViewGroup>
    {
        public void Add(ViewGroup parent, View child, int index)
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

        public View Create(Symbol sender)
        {
            var temp = new TextView(this);
            temp.TextSize = 40;
            temp.Text = " " + sender.text + " ";
            return temp;
        }

        public View Create(Number sender)
        {
            var temp = Create(sender as Symbol) as TextView;
            temp.Text = sender.text;
            return temp;
        }

        public ViewGroup Create(Expression sender)
        {
            return linearLayout();
        }

        public ViewGroup Create(Fraction sender)
        {
            var temp = linearLayout();
            temp.Orientation = Android.Widget.Orientation.Vertical;
            return temp;
        }

        public ViewGroup Create(Exponent sender)
        {
            var temp = linearLayout();
            temp.SetPadding(0, 0, 0, 50);
            return temp;
        }

        public ViewGroup Create(Bar sender)
        {
            var temp = new LinearLayout(this);
            temp.SetPadding(0, 0, 0, 3);
            temp.SetBackgroundColor(Color.White);
            return temp;
        }

        public ViewGroup GetParent(View sender)
        {
            return sender.Parent as ViewGroup;
        }

        public LinearLayout linearLayout()
        {
            var temp = new LinearLayout(this);
            temp.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            temp.SetGravity(GravityFlags.Center);
            return temp;
        }

        public BiDictionary<Symbol, View> textLookup = new BiDictionary<Symbol, View>();
        public BiDictionary<Symbol, ViewGroup> layoutLookup = new BiDictionary<Symbol, ViewGroup>();

        public void Add(Symbol parent, Symbol child, int index)
        {
            layoutLookup[parent].AddView(textLookup[child], index);
        }

        public View Render(Xamarin.Forms.Label sender)
        {
            return new TextView(this);
        }

        public object GetParent(object child)
        {
            return (child as View).Parent;
        }

        public bool IsOverlapping(object first, object second)
        {
            ViewGroup root = Input.selected.layout as ViewGroup;

            Xamarin.Forms.View test = first as Xamarin.Forms.View;
            View renderer = Xamarin.Forms.Platform.Android.Platform.CreateRenderer(test) as View;
            //Xamarin.Forms.Platform.Android.Platform.SetRenderer(test, renderer);
            (renderer as View).SetX(50);

            return true;
            /*if (v is LinearLayout && (v as ViewGroup).ChildCount > 1 && (v as ViewGroup).GetChildAt(0) != Input.cursor)
            {
                MainActivity.changed -= movingCursor;
                return;
            }

            int[] loc = new int[2], rootLoc = new int[2];
            v.GetLocationInWindow(loc);
            root.GetLocationInWindow(rootLoc);

            float x = xper * root.Width + rootLoc[0];
            float y = yper * root.Height + rootLoc[1];

            return x > loc[0] && x < loc[0] + v.Width && y > loc[1] && y < loc[1] + v.Height;

            if (x > loc[0] && x < loc[0] + v.Width && y > loc[1] && y < loc[1] + v.Height)
            {
                if ((Input.cursor as View).Parent != null)
                {
                    ((Input.cursor as View).Parent as ViewGroup).RemoveView(Input.cursor as View);
                }

                if (v is LinearLayout)
                {
                    (v as ViewGroup).AddView(Input.cursor as View);
                }
                else
                {
                    int leftOrRight = (int)Math.Round((x - loc[0]) / v.Width);
                    ViewGroup parent = v.Parent as ViewGroup;
                    parent.AddView(Input.cursor as View, parent.IndexOfChild(v) + leftOrRight);
                }
            }*/
        }

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

            layout.SetMinimumHeight(50);
            layout.SetMinimumWidth(50);
            //new OverlapListener(layout);

            //layout.SetPadding(10, 10, 10, 10);
            layout.SetPadding(0, 0, 0, f.Padding);
            //layout.Click += symbolTouch;

            //if (f.IsAnswer)
              //  layout.Click += toggleAnswer;

            return layout;
        }

        public object AddText(string text)
        {
            TextView temp = new TextView(this);
            temp.TextSize = 40;

            temp.Text = text;

            //tester.Add(new test(temp));
            //new OverlapListener(temp);

            //temp.Drag += SymbolHover;

            /*if (Input.clickable)
            {
                temp.Click += symbolTouch;
            }*/

            //temp.LongClick += LongClicked;

            return temp;
        }

        public object AddEditField()
        {
            return new RelativeLayout(this);

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
            //temp.SetPadding(3, 0, 0, 0);
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
            //((EditText)Input.editField).DispatchKeyEvent(new KeyEvent(0, text, 0, new KeyEventFlags()));

            return "";// ((EditText)Input.editField).Text;
        }

        private View selected;

        //Not in use
        public void Select(object sender)
        {
            /*if (sender.Equals(selected))
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
            }*/
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