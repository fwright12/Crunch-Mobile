using System;
using System.Collections.Generic;
using System.Text;

using Xamarin.Forms;
using Calculator;

namespace Crunch.GraFX
{
    public class Expression : StackLayout
    {
        public bool Selectable = false;

        public new Expression Parent => base.Parent as Expression;
        public int ChildCount => Children.Count - this.HideCursor().ToInt();
        //public View ChildAt(int index) => Children[index + this.HideCursor(index).ToInt()];
        public int IndexOf(View child)
        {
            int index = Children.IndexOf(child);
            return index - this.HideCursor(index).ToInt();
        }

        public double FontSize = MainPage.fontSize;

        public readonly float fontSizeDecrease = 4f / 5f;

        public Expression()
        {
            //BackgroundColor = Color.SeaGreen;
            //Padding = new Thickness(10, 10, 10, 10);
            Orientation = StackOrientation.Horizontal;
            HorizontalOptions = LayoutOptions.Center;
            VerticalOptions = LayoutOptions.Center;
            Spacing = 0;

            if (GetType() == typeof(Expression))
            {
                ChildAdded += delegate { this.CheckPadding(); };
                ChildRemoved += delegate { this.CheckPadding(); };
            }
        }

        public Expression(params View[] list) : this() => AddRange(list);

        public Expression Trim()
        {
            while (ChildCount > 0 && this.ChildBefore(ChildCount) is Text && (this.ChildBefore(ChildCount) as Text).Text == ")" && this.ChildAfter(-1) is Text && (this.ChildAfter(-1) as Text).Text == "(")
            {
                RemoveAt(ChildCount - 1);
                RemoveAt(0);
            }

            return this;
        }

        public void AddRange(params View[] list) => InsertRange(Children.Count, list);

        public void InsertRange(int index, params View[] list)
        {
            //int removeParends = (list.Length > 0 && (list[0] as Text)?.Text == "(" && (list[list.Length - 1] as Text)?.Text == ")").ToInt();

            for (int i = 0; i < list.Length; i++)
            {
                Children.Insert(index + i, list[i]);
            }
        }

        public void RemoveAt(int index)
        {
            Children.RemoveAt(index);
        }

        public void Add(View view) => Insert(Children.Count, view);

        public void Insert(int index, View view)
        {
            print.log("set text", view, view.GetType(), index, FontSize);
            (view.Parent as Expression)?.Children.Remove(view);
            Children.Insert(index, view);
        }

        protected virtual double determineFontSize() => Parent.FontSize;

        protected override void OnRemoved(View view)
        {
            base.OnRemoved(view);
            //CheckPadding();
        }

        protected override void OnAdded(View view)
        {
            base.OnAdded(view);

            print.log("child added", view);
            print.log("view is selectable: " + Selectable, FontSize);//, this == MainPage.focus, MainPage.focus.Selectable);
            view.SetSelectable(Selectable);

            if (view is Text)
            {
                (view as Text).FontSize = FontSize;
            }
            else if (view is Expression)
            {
                Expression e = view as Expression;
                e.FontSize = e.Parent != null ? e.determineFontSize() : MainPage.fontSize;
                foreach (View v in e.Children)
                {
                    e.OnAdded(v);
                }
            }
            else if (view == SoftKeyboard.Cursor)
            {
                SoftKeyboard.Cursor.HeightRequest = Input.TextSize * FontSize / MainPage.fontSize;
            }

            //CheckPadding();
        }

        public virtual string ToLatex()
        {
            string s = "";
            foreach (View v in Children)
            {
                if (v is Expression)
                {
                    s += (v as Expression).ToLatex();
                }
                else
                {
                    s += v.ToString().Simple().Trim();
                }
            }
            return s;
        }

        public override string ToString()
        {
            string s = "";
            foreach (View v in Children)
            {
                s += v.ToString().Simple().Trim();
            }
            return s;
        }
    }
}
