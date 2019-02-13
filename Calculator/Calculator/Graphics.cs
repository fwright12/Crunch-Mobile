using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace Calculator
{
    public class Text : Label
    {
        public bool Selectable = false;

        public new Expression Parent
        {
            get { return base.Parent as Expression; }
        }

        public Text()
        {
            VerticalOptions = LayoutOptions.Center;
            HorizontalOptions = LayoutOptions.Center;
        }

        public Text(string text) : this()
        {
            Text = " " + text + " ";
        }

        public bool IsOperand()
        {
            return Text.Trim() == "+" || Text.Trim() == "*" || Text.Trim() == "-";
        }

        public override string ToString()
        {
            return Text;
        }
    }

    public class Minus : Text
    {
        private View parent;

        public Minus() : base() { }

        protected override void OnParentSet()
        {
            base.OnParentSet();

            if (parent != null)
            {
                parent.ChildAdded -= Parent_ChildAdded;
            }

            change();
            Parent.ChildAdded += Parent_ChildAdded; ;
            parent = Parent;
        }

        private void Parent_ChildAdded(object sender, ElementEventArgs e) => change();

        private void change()
        {
            if (this.Index() + 1 < Parent.ChildCount && Parent.ChildAt(this.Index() + 1) is Number &&
                (this.Index() == 0 || (Parent.ChildAt(this.Index() - 1) is Text && (Parent.ChildAt(this.Index() - 1) as Text).IsOperand())))
            {
                Text = "-";
            }
            else
            {
                Text = " - ";
            }
        }
    }


    public class Number : Text
    {
        public Number(string text) : base()
        {
            try
            {
                double.Parse(text);
            }
            catch
            {
                throw new Exception("Could not parse text as double");
            }
            Text = text;
        }
    }

    public class Expression : StackLayout
    {
        public static Stack<Action> Populate = new Stack<Action>();
        public Action Build;
        public double PaddedHeight;
        public bool Selectable = false;

        protected Action addChildren;
        
        /// <summary>
        /// Not intended for use; use methods on Expression object
        /// </summary>
        public new IList<View> Children
        {
            private get;
            set;
        }

        protected IList<View> children
        {
            get { return base.Children; }
        }

        public new Expression Parent
        {
            get { return base.Parent as Expression; }
        }

        public int ChildCount
        {
            get { return children.Count - (Cursor.Parent == this).ToInt(); }
        }

        public double FontSize = MainPage.fontSize;

        public readonly float fontSizeDecrease = 4f / 5f;
        
        public Expression(params View[] list)
        {
            //Build = delegate
            //{
                foreach (View v in list)
                {
                    Add(v);
                }
            //};
            
            Spacing = 0;
            VerticalOptions = LayoutOptions.Center;
            HorizontalOptions = LayoutOptions.Center;

            format();
        }

        protected virtual void format()
        {
            Orientation = StackOrientation.Horizontal;
        }

        public View ChildAt(int index)
        {
            return children[index + (Cursor.Parent == this && index >= Cursor.Index).ToInt()];
        }

        public int IndexOf(View child)
        {
            int index = children.IndexOf(child);
            return index - (Cursor.Parent == this && index >= Cursor.Index).ToInt();
        }

        public void RemoveAt(int index)
        {
            children.RemoveAt(index);
        }

        public void Add(View symbol)
        {
            Insert(children.Count, symbol);
        }

        public virtual void Insert(int index, View view)
        {
            print.log("set text", view, view.GetType(), index, FontSize);

            Element currentParent = view.Parent;
            if (currentParent == null || currentParent != this || (view.Parent as Layout<View>).Children.IndexOf(view) != index)
            {
                if (currentParent != null)
                {
                    view.Remove();
                }
                children.Insert(index, view);
            }

            if (view.HasParent() && view.Parent.GetType() != typeof(Expression))
            {
                Expression e = view.Parent as Expression;
                CheckPadding(this, e.children.First() is Fraction, e.children.Last() is Fraction);
            }

            if (view == MainPage.cursor && index != Cursor.Index)
            {
                print.log("adding cursor", Input.textHeight, FontSize / MainPage.fontSize, index, Cursor.Index);
                Cursor.UpdateIndex();
            }
        }

        protected virtual double determineFontSize() => Parent.FontSize;

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
                foreach(View v in e.children)
                {
                    e.OnAdded(v);
                }
            }
            else if (view == MainPage.cursor)
            {
                MainPage.cursor.HeightRequest = Input.TextSize * FontSize / MainPage.fontSize;
            }
        }

        public void CheckPadding(Layout parent, bool padLeft, bool padRight)
        {
            parent.Padding = new Thickness(padLeft.ToInt() * Input.cursorWidth, parent.Padding.Top, padRight.ToInt() * Input.cursorWidth, parent.Padding.Bottom);
        }

        public override string ToString()
        {
            string s = "";
            foreach(View v in children)
            {
                s += v.ToString();
            }
            return s;
        }
    }

    public class Exponent : Expression
    {
        public static readonly float Superscript = 1f / 2f;

        private double lastHeight = -1;

        public Exponent(params View[] children) : base(children)
        {
            VerticalOptions = LayoutOptions.End;
            
            SizeChanged += delegate
            {
                if (Cursor.Parent == this && lastHeight != Height)
                {
                    lastHeight = Height;

                    Exponent p = this;
                    double top = Height;
                    do
                    {
                        top += -p.TranslationY;
                        p.PaddedHeight = top;

                        if (p.Parent == null || !(p.Parent is Exponent))
                        {
                            break;
                        }

                        p = p.Parent as Exponent;
                    } while (true);

                    setMargin(p);
                }
            };
        }

        private double getTranslation() => -Parent.Height * Superscript;

        private void setMargin(Exponent p)
        {
            double top = p.PaddedHeight;
            print.log(top, p.Parent.Height, p.Parent.Margin.Top);
            if (top - p.Parent.Height < p.Parent.Margin.Top)
            {
                foreach (View v in (p.Parent as StackLayout).Children)
                {
                    if (v is Exponent && (v as Exponent).PaddedHeight > top)
                    {
                        top = (v as Exponent).PaddedHeight;
                    }
                }
            }

            p.Parent.Margin = new Thickness(0, Math.Max(0, top - p.Parent.Height), 0, 0);
        }

        protected override double determineFontSize() => Parent.FontSize * fontSizeDecrease;

        protected override void OnParentSet()
        {
            base.OnParentSet();
            print.log("exponent parent set");

            TranslationY = getTranslation();

            if (Parent.GetType() == typeof(Expression))
            {
                Parent.SizeChanged += delegate
                {
                    PaddedHeight += TranslationY - getTranslation();
                    TranslationY = getTranslation();
                    setMargin(this);
                };
            }
        }
    }

    public class Fraction : Expression
    {
        public Expression Numerator;
        public Expression Denominator;

        private readonly BoxView bar = new BoxView { HeightRequest = 2, BackgroundColor = Color.Black };

        public Fraction(Expression numerator, Expression denominator) : base (numerator, denominator)
        {
            Numerator = numerator;
            Denominator = denominator;
            
            Insert(1, bar);
        }

        protected override void format()
        {
            Orientation = StackOrientation.Vertical;
        }

        protected override double determineFontSize()
        {
            if (Parent != null && Parent.Parent != null && Parent.Parent is Fraction)
            {
                return Parent.FontSize * fontSizeDecrease;
            }
            return base.determineFontSize();
        }

        protected override void OnParentSet()
        {
            base.OnParentSet();
            bar.WidthRequest = Math.Max(Numerator.Width, Denominator.Width);
        }

        protected override void LayoutChildren(double x, double y, double width, double height)
        {
            base.LayoutChildren(x, y, width, height);

            double w = 0;

            if (Numerator is Fraction)
            {
                w = Math.Max(w, (Numerator as Fraction).bar.Width);
            }
            if (Denominator is Fraction)
            {
                w = Math.Max(w, (Denominator as Fraction).bar.Width);
            }

            if (w > bar.Width)
            {
                bar.MinimumWidthRequest = w + MainPage.fontSize;
            }
        }
    }
}
