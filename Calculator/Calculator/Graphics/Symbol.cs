using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace Calculator.Graphics
{
    public abstract class Symbol
    {
        public object view;
        public Container Parent;
        public virtual bool selectable
        {
            get { return _selectable; }
            set { _selectable = value; }
        }
        protected bool _selectable;

        public abstract View Render();

        public StackLayout BaseLayout()
        {
            var temp = new StackLayout();
            temp.Orientation = StackOrientation.Horizontal;
            temp.Spacing = 0;
            temp.VerticalOptions = LayoutOptions.Center;
            //temp.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            //temp.SetGravity(GravityFlags.Center);
            //temp.SetMinimumHeight(Control.textHeight);
            //temp.SetPadding(5, 5, 5, 5);

            return temp;
        }

        public bool HasParent
        {
            get { return Parent != null; }
        }

        public virtual int Index
        {
            get { return Parent.Children.IndexOf(this); }
        }

        public Symbol Next
        {
            get { return Parent.Children[Parent.Children.IndexOf(this) + 1]; }
        }

        public Symbol Previous
        {
            get { return Parent.Children[Parent.Children.IndexOf(this) - 1]; }
        }

        public void Add()
        {
            //Cursor.Parent.Add(this, Cursor.Index);
            //Cursor.Set(Cursor.Parent, Cursor.Index + 1);
            //Cursor.Right();
        }

        public void AddAfter(Symbol symbol)
        {
            symbol.Parent.Add(this, symbol.Index + 1);
        }

        public void AddBefore(Symbol symbol)
        {
            symbol.Parent.Add(this, symbol.Index);
        }

        public void Remove()
        {
            Parent.Remove(this);
        }

        /*public static implicit operator Symbol(Crunch.Number n)
        {
            return new Number(n.value.ToString());
        }

        public static implicit operator Symbol(Crunch.Exponent e)
        {
            return new Number(e.value.ToString());
        }

        public static implicit operator Symbol(Crunch.Fraction f)
        {
            return new Fraction(new Expression(f.numerator as dynamic), new Expression(f.denominator as dynamic));
        }
        
        public static implicit operator Crunch.Number(Number n)
        {
            return new Crunch.Number(double.Parse(n.text));
        }

        public static implicit operator Crunch.Fraction(Fraction f)
        {
            return new Crunch.Fraction(f.Children.First() as dynamic, f.Children.Last() as dynamic);
        }*/
    }
}