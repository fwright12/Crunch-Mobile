using System;
using System.Collections.Generic;
using System.Text;

using Xamarin.Forms;

namespace Crunch.GraFX
{
    public class Fraction : Expression
    {
        public Expression Numerator;
        public Expression Denominator;

        private readonly BoxView bar = new BoxView { HeightRequest = 2, WidthRequest = 0, BackgroundColor = Color.Black };

        public Fraction(View numerator, View denominator) : base()
        {
            Orientation = StackOrientation.Vertical;

            Numerator = (numerator is Expression) ? (numerator as Expression).Trim() : new Expression(numerator);
            Denominator = (denominator is Expression) ? (denominator as Expression).Trim() : new Expression(denominator);

            AddRange(Numerator, bar, Denominator);
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
