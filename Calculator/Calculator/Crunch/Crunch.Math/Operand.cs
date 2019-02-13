using System;
using System.Collections.Generic;
using System.Text;

namespace Crunch
{
    public static partial class Math
    {
        public static Operand Simplify(this Fraction f)
        {
            return f;
        }

        public abstract class Operand
        {
            public virtual int IntegerPart() => 1;

            protected abstract Operand Add(Constant c);
            protected virtual Operand Add(Fraction f) => f + this;
            protected virtual Operand Add(Term t) => t + this;
            protected virtual Operand Add(Expression e) => e + this;

            protected abstract Operand Multiply(Constant c);
            protected virtual Operand Multiply(Fraction f) => f * this;
            protected virtual Operand Multiply(Term t) => t * this;
            protected virtual Operand Multiply(Expression e) => e * this;

            protected abstract Operand Divide(Constant c);
            protected virtual Operand Divide(Operand o)
            {
                Fraction f;
                if (o is Fraction)
                {
                    f = ((o as Fraction) ^ -1) as Fraction;
                }
                else
                {
                    f = new Fraction(1, o);
                }
                return Multiply(f);
            }

            protected abstract Operand Exponentiate(Constant c);
            protected virtual Operand Exponentiate(Operand o)
            {
                return new Term();
            }

            public static Operand operator +(Operand o1, Operand o2) => o1.Add(o2 as dynamic);
            public static Operand operator -(Operand o1, Operand o2) => o1.Add(o2.Multiply(new Constant(-1)) as dynamic);
            public static Operand operator *(Operand o1, Operand o2) => o1.Multiply(o2 as dynamic);
            public static Operand operator /(Operand o1, Operand o2)
            {
                Operand o = o1.Divide(o2 as dynamic);
                if (((o as Fraction)?.Denominator as Constant)?.Value == 1)
                {
                    return (o as Fraction).Numerator; 
                }
                return o;
            }
            public static Operand operator ^(Operand o1, Operand o2) => o1.Exponentiate(o2 as dynamic);

            public static implicit operator Operand(double d) => new Constant(d);
        }
    }
}
