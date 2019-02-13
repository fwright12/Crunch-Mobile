using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crunch
{
    public class Fraction : Term
    {
        public override double value
        {
            get
            {
                return numerator.value / denominator.value;
            }
        }

        public Term numerator;
        public Term denominator;

        public Fraction (Term n) : this (n, new Number(1)) { }

        public Fraction(Term Numerator, Term Denominator)
        {
            numerator = Numerator;
            denominator = Denominator;
        }

        /*public override Term Simplify()
        {
            Term n = Evaluate(numerator);
            Term d = Evaluate(denominator);

            if (n.GetType() != typeof(Fraction) && d.GetType() != typeof(Fraction))
            {
                if (value == (int)value)
                {
                    return new Number(value);
                }
                else if (n.value == (int)n.value && d.value == (int)d.value)
                {
                    Number temp = new Number(gcd((int)n.value, (int)d.value));
                    return new Fraction(new Number(n.value / temp.value), new Number(d.value / temp.value));
                }
                else
                {
                    return new Fraction(n, d);
                }
            }
            else
            {
                Fraction top, bottom;

                if (n.GetType() == typeof(Fraction))
                {
                    top = Wrap((n as Fraction).Simplify());
                }
                else
                {
                    top = new Fraction(n);
                }

                if (d.GetType() == typeof(Fraction))
                {
                    bottom = Wrap(((Fraction)(d)).Simplify());
                }
                else
                {
                    bottom = new Fraction(d);
                }

                return new Fraction(Evaluate(top.numerator).Multiply(Evaluate(bottom.denominator)), Evaluate(top.denominator).Multiply(Evaluate(bottom.numerator)));
            }
        }*/

        public Fraction Wrap(Term t)
        {
            if (t.GetType() == typeof(Fraction))
            {
                return t as Fraction;
            }
            else
            {
                return new Fraction(t);
            }
        }

        //Problem here when fraction is negative
        public int gcd(int a, int b)
        {
            if (a == 0)
                return b;
            if (b == 0)
                return a;

            if (a > b)
                return gcd(a % b, b);
            else
                return gcd(a, b % a);
        }

        public static Fraction operator +(Number n, Fraction f) { return f + n; }

        public static Fraction operator +(Fraction f, Number n)
        {
            return new Fraction(f.numerator + n * f.denominator, f.denominator);
        }

        public static Fraction operator +(Fraction f1, Fraction f2)
        {
            return new Fraction(f1.numerator * f2.denominator + f1.denominator * f2.numerator, f1.denominator * f2.denominator);
        }

        public static Fraction operator *(Number n, Fraction f) { return f * n; }

        public static Fraction operator *(Fraction f, Number n)
        {
            return new Fraction(n * f.numerator, f.denominator);
        }
        
        public static Fraction operator *(Fraction f1, Fraction f2)
        {
            return new Fraction(f1.numerator * f2.numerator, f1.denominator * f2.denominator);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(Fraction))
                return false;

            return GetHashCode() == (obj as Fraction).GetHashCode();
        }

        public override int GetHashCode()
        {
            return (numerator.GetHashCode() + denominator.GetHashCode()) % int.MaxValue;
        }
    }
}
