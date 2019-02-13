using System;
using System.Collections.Generic;
using System.Text;

namespace Crunch
{
    public static partial class Math
    {
        public class Constant : Operand
        {
            public double Value;
            public bool IsInteger;

            public Constant(double value)
            {
                Value = value;
                IsInteger = (value == (int)value);
            }

            public override int IntegerPart() => IsInteger ? (int)Value : 1;

            protected override Operand Add(Constant c) => new Constant(Value + c.Value);

            protected override Operand Multiply(Constant c) => new Constant(Value * c.Value);

            protected override Operand Divide(Constant c)
            {
                int gcd = 1;
                if (IsInteger && c.IsInteger)
                {
                    gcd = GCD((int)System.Math.Abs(Value), (int)System.Math.Abs(c.Value));
                    return new Fraction(System.Math.Sign(Value / c.Value) * System.Math.Abs(Value) / gcd, System.Math.Abs(c.Value) / gcd);
                }
                return new Fraction(Value / c.Value, 1);
            }

            protected override Operand Exponentiate(Constant c) => new Constant(System.Math.Pow(Value, c.Value));

            private static int GCD(int a, int b)
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
                    return GCD(a % b, b);
                }
                return GCD(a, b % a);
            }

            public static implicit operator Constant(double num) => new Constant(num);

            public override string ToString()
            {
                return Value.ToString();
            }
        }
    }
}
