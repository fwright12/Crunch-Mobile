using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace Crunch.Math
{
    public class Fraction : Operand
    {
        public Operand Numerator;
        public Operand Denominator;

        public Fraction(Operand n, Operand d)
        {
            Numerator = n;
            Denominator = d;
        }

        protected void Simplify()
        {
            //Both Numerator and Denominator are something other than a fraction
            /*if (Numerator.GetType() != typeof(Fraction) && Denominator.GetType() != typeof(Fraction))
            {
                //The fraction simplifies to a whole number
                if (value == (int)value)
                {
                    Value = new Number(value);
                }
                //If both the Numerator and Denominator are whole numbers, look for gcd
                else if (Numerator.Value.value == (int)Numerator.Value.value && Denominator.Value.value == (int)Denominator.Value.value)
                {
                    //if (((Numerator.value < 0).ToInt() + (Denominator.value < 0).ToInt()) % 2 == 1)

                    Number temp = new Number(gcd(System.Math.Abs((int)Numerator.Value.value), System.Math.Abs((int)Denominator.Value.value)));
                    Value = new Fraction(new Number(Numerator.Value.value / temp.value), new Number(Denominator.Value.value / temp.value));
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
                if (Numerator.Value.GetType() == typeof(Fraction))
                {
                    Fraction temp = Numerator.Value as Fraction;

                    Denominator = (Denominator as dynamic) * (temp.Denominator as dynamic);
                    Numerator = temp.Numerator;
                }

                if (Denominator.Value.GetType() == typeof(Fraction))
                {
                    Fraction temp = Denominator.Value as Fraction;

                    Numerator = (Numerator as dynamic) * (temp.Denominator as dynamic);
                    Denominator = temp.Numerator;
                }

                return Simplify();
            }*/
        }

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

        public override Operand Add(Constant c) => new Fraction(Numerator + Denominator * c, Denominator);

        public override Operand Add(Fraction f) => new Fraction(Numerator * f.Denominator + Denominator * f.Numerator, Denominator * f.Denominator);

        public override Operand Add(Expression e) => e.Add(this);

        public override Operand Multiply(Constant c) => new Fraction(Numerator * c, Denominator);

        public override Operand Multiply(Fraction f) => new Fraction(Numerator * f.Numerator, Denominator * f.Denominator);

        public override Operand Multiply(Expression e) => e.Multiply(this);

        public override string ToString()
        {
            return (Numerator as dynamic).ToString() + "/" + (Denominator as dynamic).ToString();
        }
    }
}