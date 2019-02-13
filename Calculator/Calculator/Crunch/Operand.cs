using System;
using System.Collections.Generic;
using System.Text;

namespace Crunch.Math
{
    public abstract class Operand
    {
        public abstract Operand Add(Constant c);
        public abstract Operand Add(Fraction f);
        public abstract Operand Add(Expression e);
        public abstract Operand Multiply(Constant c);
        public abstract Operand Multiply(Fraction f);
        public abstract Operand Multiply(Expression e);

        public static Operand operator +(Operand o1, Operand o2)
        {
            return o1.Add(o2 as dynamic);
        }

        public static Operand operator -(Operand o1, Operand o2)
        {
            return o1.Add(o2.Multiply(new Constant(-1)) as dynamic);
        }

        public static Operand operator *(Operand o1, Operand o2)
        {
            return o1.Multiply(o2 as dynamic);
        }
    }
}
