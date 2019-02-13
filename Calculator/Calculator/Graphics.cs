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
            //Text = " " + text + " ";
            Text = text;
        }

        public bool IsOperand()
        {
            return Text == " + " || Text == " × " || Text == " - ";
        }

        public override string ToString()
        {
            return Text;
        }
    }

    public class Number : Text
    {
        public Number(string text) : base()
        {
            Text = text;
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

            if (Parent != null)
            {
                Parent.ChildAdded += Parent_ChildAdded;
                parent = Parent;
            }
        }

        private void Parent_ChildAdded(object sender, ElementEventArgs e) => change();

        private void change()
        {
            if (Parent != null)
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
            else if (parent != null)
            {
                parent.ChildAdded -= Parent_ChildAdded;
            }
        }
    }

    public class Expression : StackLayout
    {
        public static Stack<Action> Populate = new Stack<Action>();
        public bool Selectable = false;

        protected Action build;

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

        public Expression()
        {
            Orientation = StackOrientation.Horizontal;
            HorizontalOptions = LayoutOptions.Center;
            VerticalOptions = LayoutOptions.Center;
            Spacing = 0;

            build = delegate { };
        }

        public Expression(params View[] list) : this() => setBuild(list);
        public Expression(Expression adding, int direction) : this() => setBuild(adding, direction);

        protected void setBuild(params View[] list)
        {
            build = delegate
            {
                foreach (View v in list)
                {
                    Add(v);
                }
            };
        }

        protected void setBuild(Expression adding, int direction)
        {
            build = delegate
            {
                Layout<View> temp = Cursor.Parent;
                IList<View> list = temp.Children;
                int index = temp.Children.IndexOf(MainPage.cursor) - 1;

                //Grab stuff while there is stuff to grab until we hit an operand, or we added the cursor last time
                int imbalance = 0;
                while ((index + direction).IsBetween(0, list.Count - 1) && !(list[index] is Cursor) && !(list[index + direction] is Text && (list[index + direction] as Text).IsOperand() && imbalance == 0))
                {
                    index += direction;
                    if (list[index] is Text)
                    {
                        string s = (list[index] as Text).Text;
                        if (s == "(" || s == ")")
                        {
                            if (s == "(") imbalance++;
                            if (s == ")") imbalance--;
                        }
                    }
                    Insert(ChildCount * (direction + 1) / -2, list[index]);
                }

                if (ChildCount > 0 && ChildAt(ChildCount - 1) is Text && (ChildAt(ChildCount - 1) as Text).Text == ")" && ChildAt(0) is Text && (ChildAt(0) as Text).Text == "(")
                {
                    RemoveAt(children.Count - 1);
                    RemoveAt(0);
                }

                /*if ((index + direction).IsBetween(0, list.Count - 1))
                {
                    //Peek at what's next without actually moving anywhere
                    View view = list[index + direction];


                    if (view is Number)
                    {
                        while ((index + direction).IsBetween(0, list.Count - 1) && list[index + direction] is Number)
                        {
                            index += direction;
                            Insert(ChildCount * (direction + 1) / -2, list[index]);
                        }
                    }
                    else if (view is BoxView || view is Fraction)
                    {
                        Add(view);
                    }

                    /*int i;
                    Func<bool> canGrab = () => { return true; } ;
                    for (i = 1; (index + i * direction).IsBetween(0, list.Count - 1); i += direction)
                    {
                        view = list[index + i * direction];

                        if (canGrab())
                        {
                            Insert(ChildCount * (direction + 1) / -2, view);
                        }
                        else
                        {
                            break;
                        }

                        if (view is Exponent)
                        {
                            if (direction > 0)
                            {
                                break;
                            }
                            else if (direction < 0)
                            {
                                canGrab = () => { return i < 3 && view is Number; };
                            }
                        }
                        else if (view is Number)
                        {
                            if (i == 1)
                            {
                                canGrab = () => { return view is Exponent || view is Number; };
                            }
                            else
                            {
                                canGrab = () => { return view is Number; };
                            }
                        }
                    }
                }*/
            };
        }

        public Expression Build()
        {
            build();
            foreach(View v in children)
            {
                if (v is Expression)
                {
                    (v as Expression).Build();
                }
            }
            return this;
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

            if (view == MainPage.cursor && index != Cursor.Index)
            {
                print.log("adding cursor", Input.textHeight, FontSize / MainPage.fontSize, index, Cursor.Index);
                Cursor.UpdateIndex();
            }
        }

        protected virtual double determineFontSize() => Parent.FontSize;

        protected override void OnRemoved(View view)
        {
            base.OnRemoved(view);
            CheckPadding();
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
                e.MinimumHeightRequest = Input.textHeight * e.FontSize / MainPage.fontSize;
                foreach(View v in e.children)
                {
                    e.OnAdded(v);
                }
            }
            else if (view == MainPage.cursor)
            {
                MainPage.cursor.HeightRequest = Input.TextSize * FontSize / MainPage.fontSize;
            }

            CheckPadding();
        }

        private readonly int extraSpaceForCursor = 5;
        private readonly int nestedFractionPadding = 5;

        protected void CheckPadding()
        {
            if (ChildCount > 0)
            {
                int last = ChildCount - 1;
                Point isNoSpace = new Point((ChildAt(0) is Fraction).ToInt(), (ChildAt(last) is Fraction || ChildAt(last) is Exponent).ToInt());
                int isOnlyChildFraction = (Parent is Fraction && ChildCount == 1 && ChildAt(0) is Fraction).ToInt();

                Padding = new Thickness(
                    isNoSpace.X * extraSpaceForCursor + isOnlyChildFraction * nestedFractionPadding,
                    Padding.Top,
                    isNoSpace.Y * extraSpaceForCursor + isOnlyChildFraction * nestedFractionPadding,
                    Padding.Bottom);
            }
        }

        public virtual string ToLatex()
        {
            return ToString();
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

        public Exponent(params View[] children) : this() => setBuild(children);
        public Exponent(Expression parent, int direction) : this() => setBuild(parent, direction);

        public Exponent() : base()
        {
            VerticalOptions = LayoutOptions.End;

            SizeChanged += delegate
            {
                if (lastHeight != Height)
                {
                    lastHeight = Height;
                    checkMargins();
                }
            };
        }

        private void checkMargins()
        {
            if (!needsMargins)
            {
                return;
            }

            Expression p = this;
            do
            {
                //Calculate p's total height
                double height = p.Height - p.TranslationY + p.Margin.Top;
                p = p.Parent;

                //print.log("before", p, height, p.Height, p.Margin.Top);
                //If I'm smaller than the space I'm in, check to make sure there's not someone bigger
                //that needs that space before shrinking it
                if (height < p.Height + p.Margin.Top)
                {
                    foreach (View v in (p as StackLayout).Children)
                    {
                        //print.log(v);
                        //print.log("has an exponent of height ", height, (-v.TranslationY + v.Height + v.Margin.Top));
                        //Found someone bigger - make theirs the new height
                        if (v.Height - v.TranslationY + v.Margin.Top > height)
                        {
                            //height = (v as Exponent).actualHeight;
                            height = v.Height - v.TranslationY + v.Margin.Top;
                        }
                    }
                    //print.log("*******");
                }
                //Set the margin
                p.Margin = new Thickness(0, Math.Max(0, height - p.Height), 0, 0);
                //print.log("after", height, p.Height, -p.TranslationY, p.Margin.Top, p.GetType());

                /*if ((p.Parent as Exponent) == null)
                {
                    break;
                }*/

                //p = p.Parent as Exponent;
            } while (p is Exponent);
        }

        private double getTranslation() => -Parent.Height * Superscript;

        protected override double determineFontSize() => Parent.FontSize * fontSizeDecrease;

        private Expression parent;
        private bool needsMargins;

        protected override void OnParentSet()
        {
            base.OnParentSet();
            print.log("exponent parent set");

            if (Parent == null)
            {
                if (parent != null)
                {
                    parent.SizeChanged -= Parent_SizeChanged;
                }
                return;
            }

            TranslationY = getTranslation();

            Expression temp = this;
            while (temp.Parent != null && !(temp.Parent is Fraction && (temp.Parent as Fraction).Denominator == temp))
            {
                temp = temp.Parent;
            }
            needsMargins = temp.Parent != null;

            if (Parent.GetType() == typeof(Expression))
            {
                if (parent != null)
                {
                    parent.SizeChanged -= Parent_SizeChanged;
                }
                Parent.SizeChanged += Parent_SizeChanged;
                parent = Parent;
            }
        }

        private void Parent_SizeChanged(object sender, EventArgs e)
        {
            checkMargins();
            //height += TranslationY - getTranslation();
            TranslationY = getTranslation();
            //setMargin(this);
        }

        public override string ToLatex()
        {
            return "^{" + base.ToString() + "}";
        }

        public override string ToString()
        {
            return "^(" + base.ToString() + ")";
        }
    }

    public class Answer : Expression
    {
        public bool isDecimal = false;

        private readonly int decimalPlaces = 3;
        private View raw;
        private View value;

        public Answer(Crunch.Term term) : base()
        {
            raw = term.ToView();
            value = new Number(Math.Round(term.value, decimalPlaces).ToString());
            if (term is Crunch.Number)
            {
                raw = value;
            }

            setBuild(raw);
        }

        public void SwitchFormat()
        {
            isDecimal = !isDecimal;
            children.Clear();

            if (isDecimal)
            {
                Add(value);
            }
            else
            {
                Add(raw);
            }
        }
    }

    public class Fraction : Expression
    {
        public Expression Numerator;
        public Expression Denominator;

        private readonly BoxView bar = new BoxView { HeightRequest = 2, WidthRequest = 0, BackgroundColor = Color.Black };

        public Fraction(Expression numerator, Expression denominator) : base()
        {
            Orientation = StackOrientation.Vertical;

            Numerator = numerator;
            Denominator = denominator;

            setBuild(numerator, bar, denominator);
        }

        protected override double determineFontSize()
        {
            if (Parent != null && Parent.Parent != null && Parent.Parent is Fraction)
            {
                return Parent.FontSize * fontSizeDecrease;
            }
            return base.determineFontSize();
        }

        public override string ToLatex()
        {
            return "\frac{" + Numerator.ToString() + "}{" + Denominator.ToString() + "}";
        }

        public override string ToString()
        {
            return "(" + Numerator.ToString() + ")/(" + Denominator.ToString() + ")";
        }
    }
}
