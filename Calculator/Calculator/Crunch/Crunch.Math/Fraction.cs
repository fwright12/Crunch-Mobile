using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace Crunch
{
    public static partial class Math
    {
        public class Fraction : Operand
        {
            public Operand Numerator;
            public Operand Denominator;

            public bool IsConstant;
            private double constantValue;

            public Fraction(Operand numerator, Operand denominator)
            {
                Numerator = numerator;
                Denominator = denominator;

                /*if (IsConstant = (numerator is Constant) && (denominator is Constant))
                {
                    constantValue = (Numerator as Constant).Value / (Denominator as Constant).Value;
                }*/
            }

            protected override Operand Add(Constant c) => (Numerator + Denominator * c) / Denominator;

            protected override Operand Add(Fraction f) => (Numerator * f.Denominator + Denominator * f.Numerator) / (Denominator * f.Denominator);

            protected override Operand Multiply(Constant c) => (Numerator * c) / Denominator;

            protected override Operand Multiply(Fraction f) => (Numerator * f.Numerator) / (Denominator * f.Denominator);

            protected override Operand Divide(Constant c) => Divide(c as Operand);

            protected override Operand Exponentiate(Constant c)
            {
                if (c.Value < 0)
                {
                    return new Fraction(Denominator, Numerator) ^ new Constant((int)System.Math.Abs(c.Value));
                }
                return (Numerator ^ c) / (Denominator ^ c);
            }

            public override string ToString()
            {
                return "(" + (Numerator as dynamic).ToString() + "/" + (Denominator as dynamic).ToString() + ")";
            }
        }
    }
}