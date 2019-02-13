using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace Calculator
{
    public static class TermConverter
    {
        public static View ToView(this Crunch.Term term)
        {
            try
            {
                return toView(term as dynamic);
            }
            catch
            {
                throw new InvalidCastException("No known conversion from type '" + term.GetType() + "' to type 'View'");
            }
        }

        private static View toView(Crunch.Number n)
        {
            return new Number(n.value.ToString());
        }

        private static View toView(Crunch.Exponent e)
        {
            return new Number(e.value.ToString());
        }

        private static View toView(Crunch.Fraction f)
        {
            var a = new Fraction(new Expression(new Crunch.Number(Math.Abs(f.Numerator.value)).ToView()), new Expression(new Crunch.Number(Math.Abs(f.Denominator.value)).ToView()));
            Text b = new Text();
            b.Text = "";
            if (f.value < 0)
            {
                b.Text = "-";
            }
            return new Expression(b, a);
        }
    }
}
