using System;
using System.Collections.Generic;
using System.Text;

namespace Crunch
{
    public static partial class Math
    {
        /*public class Constant : Operand
        {
            private double numerator;
            private double denominator;

            public Constant(double n)
            {
                numerator = n;
                denominator = 1;
            }

            public Constant(double n, double d)
            {
                numerator = n;
                denominator = d;
            }

            public Constant Add(Constant c) => 
        }*/

        public class Constant
        {
            public double Value;
            public bool IsInteger;

            private int hash;

            public Constant(double value)
            {
                Value = value;
                hash = value.ToString().GetHashCode();
                IsInteger = (value == (int)value);
            }

            //public override bool IsNegative() => Value < 0;

            public Constant Add(Constant c) => new Constant(Value + c.Value);
            public Constant Multiply(Constant c) => new Constant(Value * c.Value);
            //public Constant Divide(Constant c) => new Constant(Value / c.Value);
            public Operand Exponentiate(Constant c)
            {
                double d = System.Math.Pow(Value, System.Math.Abs(c.Value));
                if (c.Value > 0 && d >= 1 && d < 100000000)
                {
                    //return new Constant(d);
                    //return Fraction.Construct(1, new Constant(Value) ^ System.Math.Abs(c.Value));
                }
                return null;
            }
            
            /*public Operand Divide(Constant c)
            {
                int gcd = 1;
                if (IsInteger && c.IsInteger)
                {
                    gcd = GCD((int)System.Math.Abs(Value), (int)System.Math.Abs(c.Value));
                    //return Fraction.Divide(System.Math.Sign(Value / c.Value) * System.Math.Abs(Value) / gcd, System.Math.Abs(c.Value) / gcd);
                    //if (gcd == 1) return null;
                    return Fraction.Construct(new Constant(Value / gcd), new Constant(c.Value / gcd));
                }
                else
                {
                    return Value / c.Value;
                }
            }*/

            /*public Operand Exponentiate(Constant c)
            {
                double d = System.Math.Pow(Value, System.Math.Abs(c.Value));
                if (c.Value < 0)
                {
                    return Fraction.Divide(1, d);
                }
                else
                {
                    return d;
                }

                if (d < 100000000 && d >= 1)//d.ToString().Length < System.Math.Round(d, 3).ToString().Length + 2 + Value.ToString().Length + c.Value.ToString().Length)
                {
                    return d;
                }
                else if (d > 0.00000001 && d < 100000000)
                {
                    return Fraction.Divide(1, System.Math.Pow(Value, System.Math.Abs(c.Value)));
                }
                return null;
            }*/

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

            public override int GetHashCode() => hash;
            public override string ToString() => Value.ToString();
        }
    }
}
