using System;
using System.Collections.Generic;
using System.Text;

namespace Crunch.Math
{
    public class Constant : Operand
    {
        public double Value;

        public Constant(double value)
        {
            Value = value;
        }

        public override Operand Add(Constant c) => new Constant(Value + c.Value);
        public override Operand Add(Fraction f) => f.Add(this);
        public override Operand Add(Expression e) => e.Add(this);
        public override Operand Multiply(Constant c) => new Constant(Value * c.Value);
        public override Operand Multiply(Fraction f) => f.Multiply(this);
        public override Operand Multiply(Expression e) => e.Multiply(this);

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
