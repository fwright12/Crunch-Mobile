using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace Crunch
{
    public class Fraction : Term
    {
        public override double value
        {
            get { return Numerator.value / Denominator.value; }
        }

        public Term Numerator;
        public Term Denominator;

        public Fraction(Term n) : this(n, new Number(1)) { }

        public Fraction(Term numerator, Term denominator)
        {
            Numerator = numerator;
            Denominator = denominator;
        }

        public override Term Simplify()
        {
            //Both Numerator and Denominator are something other than a fraction
            if (Numerator.GetType() != typeof(Fraction) && Denominator.GetType() != typeof(Fraction))
            {
                //The fraction simplifies to a whole number
                if (value == (int)value)
                {
                    return new Number(value);
                }
                //If both the Numerator and Denominator are whole numbers, look for gcd
                else if (Numerator.value == (int)Numerator.value && Denominator.value == (int)Denominator.value)
                {
                    //if (((Numerator.value < 0).ToInt() + (Denominator.value < 0).ToInt()) % 2 == 1)

                    Number temp = new Number(gcd(System.Math.Abs((int)Numerator.value), System.Math.Abs((int)Denominator.value)));
                    return new Fraction(new Number(Numerator.value / temp.value), new Number(Denominator.value / temp.value));
                }
                //There is a decimal in either the Numerator or the Denominator
                else
                {
                    return new Number(value);
                }
            }
            //I have a fraction somewhere; try to simplify
            else
            {
                if (Numerator.GetType() == typeof(Fraction))
                {
                    Fraction temp = Numerator as Fraction;

                    Denominator = (Denominator as dynamic) * (temp.Denominator as dynamic);
                    Numerator = temp.Numerator;
                }

                if (Denominator.GetType() == typeof(Fraction))
                {
                    Fraction temp = Denominator as Fraction;

                    Numerator = (Numerator as dynamic) * (temp.Denominator as dynamic);
                    Denominator = temp.Numerator;
                }

                return Simplify();
            }
        }

        //Problem here when fraction is negative
        public int gcd(int a, int b)
        {
            if (a == 0)
            {
                return b;
            }
            if (b == 0)
            {
                return a;
            }

            if (a > b)
            {
                return gcd(a % b, b);
            }
            return gcd(a, b % a);
        }

        public static Fraction operator +(Number n, Fraction f) { return f + n; }

        public static Fraction operator +(Fraction f, Number n)
        {
            return new Fraction(f.Numerator + n * f.Denominator, f.Denominator);
        }

        public static Fraction operator +(Fraction f1, Fraction f2)
        {
            return new Fraction(f1.Numerator * f2.Denominator + f1.Denominator * f2.Numerator, f1.Denominator * f2.Denominator);
        }

        public static Fraction operator *(Number n, Fraction f) { return f * n; }

        public static Fraction operator *(Fraction f, Number n)
        {
            return new Fraction(n * f.Numerator, f.Denominator);
        }

        public static Fraction operator *(Fraction f1, Fraction f2)
        {
            return new Fraction(f1.Numerator * f2.Numerator, f1.Denominator * f2.Denominator);
        }

        public override string ToString()
        {
            return "(" + (Numerator as dynamic).ToString() + "/" + (Denominator as dynamic).ToString() + ")";
        }
    }
}