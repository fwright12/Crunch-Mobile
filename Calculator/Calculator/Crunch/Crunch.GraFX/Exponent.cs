using System;
using System.Collections.Generic;
using System.Text;

using Xamarin.Forms;

namespace Crunch.GraFX
{
    public class Exponent : Expression
    {
        public static readonly double Superscript = 1 - 0.5 * fontSizeDecrease;// 1f / 2f;

        private Expression lastParent;
        private bool needsMargins = true;

        public Exponent() : base()
        {
            VerticalOptions = LayoutOptions.End;
            HeightChanged += (e) => parentSizeChanged(e.Parent);
        }

        public Exponent(params View[] children) : this()
        {
            AddRange(children);
            Trim();
        }

        private double getTranslation() => -Parent.Height * Superscript;

        protected override double determineFontSize() => Parent.FontSize * fontSizeDecrease;

        protected override void OnParentSet()
        {
            base.OnParentSet();

            //Get rid of the listener on the old parent (if there was one)
            if (lastParent != null)
            {
                lastParent.HeightChanged -= parentSizeChanged;
            }

            //If Parent is null this was just removed from its parent
            if (Parent != null)
            {
                /*Expression temp = this;
                while (temp.Parent != null && !(temp.Parent is Fraction && (temp.Parent as Fraction).Denominator == temp))
                {
                    temp = temp.Parent;
                }
                needsMargins = temp.Parent != null;*/

                Parent.HeightChanged += parentSizeChanged;
                lastParent = Parent;
            }

            checkMargins(Parent ?? lastParent);
        }

        private void parentSizeChanged(Expression e)
        {
            TranslationY = getTranslation();
            checkMargins(e);
        }

        private void checkMargins(Expression e)
        {
            if (!needsMargins)
            {
                return;
            }

            do
            {
                //Calculate p's total height
                double height = -1;// p.Height - p.TranslationY + p.Margin.Top;
                                   //p = p.Parent;

                //If I'm smaller than the space I'm in, check to make sure there's not someone bigger
                //that needs that space before shrinking it
                //if (height < p.Height + p.Margin.Top)
                //{
                e.lastHeight = e.Height;

                foreach (View v in e.Children)
                {
                    //Found someone bigger - make theirs the new height
                    if (v.Height - v.TranslationY + v.Margin.Top > height)
                    {
                        height = v.Height - v.TranslationY + v.Margin.Top;
                    }
                }

                //Set the margin
                e.PaddingForExponents(System.Math.Max(0, height - e.Height));
            }
            while (e is Exponent && (e = e.Parent) != null);
        }

        public override string ToLatex()
        {
            return "^{" + base.ToString() + "}";
        }

        public override string ToString()
        {
            return "^" + base.ToString();
        }
    }
}
