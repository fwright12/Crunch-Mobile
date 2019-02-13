using System;
using Xamarin.Forms;

using Crunch.Math.Input;

namespace Crunch.GraFX
{
    public class Expression : StackLayout, IMathList
    {
        public Field expression;
        public System.Collections.Generic.IList<object> list => (System.Collections.Generic.IList<object>)Children;

        public Expression()
        {
            Orientation = StackOrientation.Horizontal;
            HorizontalOptions = LayoutOptions.Center;
            Spacing = 0;
        }

        public Expression(string str) : this (new Field())
        {
            expression.Build(str);
        }

        public Expression(Field e) : this()
        {
            if (e != null)
            {
                expression = e;
                /*expression.Inserted += Inserted;
                expression.Removed += Removed;
                expression.Focused += delegate { Calculator.Cursor.MoveTo(this); };*/
            }
        }

        public void Insert(int index, object o)
        {
            print.log(o + " was inserted");
            View view = null;

            if (Calculator.Cursor.RecentlyRemoved.ContainsKey(o))
            {
                Calculator.Cursor.RecentlyRemoved.Remove(o, out view);
            }
            else
            {
                if (o is Field)
                {
                    Field f = (o as Field);
                    print.log("inserting field formatted as " + f.Format);

                    Expression e = new Expression(f);
                    list.InsertAndFill<Expression>(ref index, 0);
                    if (f.Format == FieldFormat.Fraction)
                    {
                        e.Orientation = StackOrientation.Vertical;
                        e.Children.Add(new BoxView { HeightRequest = 2, WidthRequest = 0, BackgroundColor = Color.Black });
                    }

                    view = e;
                }
                else if (o is string)
                {
                    string str = o as string == "*" ? "×" : o as string;
                    string pad = "";
                    if (!str.IsNumber())
                    {
                        pad = " ";
                    }
                    view = new Text(pad + str + pad);
                }
            }

            try
            {
                Children.Insert(index, view);
            }
            catch
            {
                throw new Exception("No known conversion from '" + o.GetType() + "' to 'View'");
            }
        }

        public void RemoveAt(int index)
        {
            print.log("removing at " + index);
            Calculator.Cursor.RecentlyRemoved.Add(o, Children[index]);
            Children.RemoveAt(index);
        }
    }
}