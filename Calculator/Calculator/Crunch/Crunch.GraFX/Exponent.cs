using System;
using System.Collections.Generic;
using System.Text;

using Xamarin.Forms;

namespace Crunch.GraFX
{
    public class Exponent : Expression
    {
        public static readonly float Superscript = 1f / 2f;

        private double lastHeight = -1;

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

        public Exponent(params View[] children) : this() => AddRange(children);

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
                p.Margin = new Thickness(0, System.Math.Max(0, height - p.Height), 0, 0);
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
}
