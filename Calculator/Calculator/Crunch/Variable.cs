using System;
using System.Collections.Generic;
using System.Text;

namespace Crunch.Math
{
    public class Variable : Operand
    {
        public override Operand Add(Constant c)
        {
            throw new NotImplementedException();
        }

        public override Operand Add(Fraction f)
        {
            throw new NotImplementedException();
        }

        public override Operand Add(Expression e)
        {
            throw new NotImplementedException();
        }

        public override Operand Multiply(Constant c)
        {
            throw new NotImplementedException();
        }

        public override Operand Multiply(Fraction f)
        {
            throw new NotImplementedException();
        }

        public override Operand Multiply(Expression e)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return "";
        }
    }
}
