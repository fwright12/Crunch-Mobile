using System;
using System.Collections.Generic;
using System.Text;

namespace Crunch
{
    public static partial class Math
    {
        public class Exponenta : Operand
        {
            /*public Operand Base;
            public Operand Power;

            public override bool IsConstant => Base.IsConstant && Power.IsConstant;

            public Exponent(Operand bse, Operand power)
            {
                Base = bse;
                Power = power;
            }

            public override Operand Simplify()
            {
                Operand b = Base.Simplify();
                Operand p = Power.Simplify();

                if (b is Constant && p is Constant)
                {
                    return System.Math.Pow((b as Constant).Value, (p as Constant).Value);
                }
                else if ((p as Constant)?.Value == 1)
                {
                    return Base;
                }
                return base.Simplify();
            }

            /*public Operand Multiply(Exponent e)
            {
                if (Base == e.Base)
                {
                    return new Exponent(Base, Power + e.Power);
                }
                rturn null;
            }

            //public Operand Multiply(Variable v) => Multiply(new Exponent(v, 1));

            public Operand Divide(Exponent e) => Multiply(e.Exponentiate(-1));

            public Exponent Exponentiate(Constant c) => new Exponent(Base, Power * c);

            public override bool Equals(object obj)
            {
                return (obj as Exponent)?.Base == Base && (obj as Exponent).Power == Power;
            }

            public override string ToString()
            {
                return Base.ToString() + "^" + Power.ToString();
            }*/
        }
    }
}
