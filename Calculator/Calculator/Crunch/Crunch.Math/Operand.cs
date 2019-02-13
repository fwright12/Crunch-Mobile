using System;
using System.Collections.Generic;
using System.Text;

namespace Crunch
{
    /*
     *  constant + constant = constant          
     *  constant + variable = expression    variable + variable = term/expression
     *  constant + fraction = fraction      variable + fraction = expression        fraction + fraction = fraction
     *  constant + exponent = expression    variable + exponent = expression        fraction + exponent = expression
     *  constant + term = expression        variable + term = term/expression       fraction + term = expression
     *  constant + expression = expression  variable + expression = expression      fraction + expression = expression
     *  
     *  constant * constant = constant
     *  constant * variable = term          variable * variable = term/exponent
     *  constant * fraction = fraction      variable * fraction = ?                 fraction * fraction = fraction
     *  constant * exponent = term          variable * exponent = ?                 fraction * exponent = term
     *  constant * term = term              variable * term = term                  fraction * term = term
     *  constant * expression = expression  variable * expression = expression      fraction * expression = expression
     *  
     *  constant / constant = fraction
     *  constant / variable = fraction      variable / variable = fraction
     *  constant / fraction = fraction      variable / fraction = fraction
     *  constant / exponent = fraction      variable / exponent = fraction
     *  constant / term = fraction
     *  constant / expression = fraction
     *  
     *  constant ^ constant = exponent/constant     
     *  constant ^ variable = exponent              variable ^ constant = exponent
     *  constant ^ fraction = exponent              fraction ^ constant = fraction
     *  constant ^ exponent = exponent              exponent ^ constant = exponent
     *  constant ^ term = exponent                  term ^ constant = term
     *  constant ^ expression = exponent            expression ^ constant = expression
     */

    public static partial class Math
    {
        public interface IOperand<T>
        {
            Operand Multiply(T t);
        }

        public interface IConstant : IOperand<Constant> { }
        public interface IVariable : IOperand<Variable> { }


        public interface IHashable { int Hash(); }

        public abstract class Operand
        {
            public virtual Operand Simplify() => this;

            public virtual bool IsNegative() => false;

            public Operand Add(Operand o) => null;
            public Operand Multiply(Operand o) => null;
            public Operand Divide(Operand o) => null;
            public Operand Exponentiate(Operand o) => null;
            public bool RemoveNegative(ref Operand o)
            {
                bool b = o.IsNegative();
                if (b)
                {
                    o *= -1;
                }

                return b;
            }

            public static Operand operator +(Operand o1, Operand o2) => (o1 as dynamic).Add(o2 as dynamic) ?? (o2 as dynamic).Add(o1 as dynamic);
            public static Operand operator -(Operand o1, Operand o2) => o1 + (-1 * o2);
            public static Operand operator *(Operand o1, Operand o2) => (o1 as dynamic).Multiply(o2 as dynamic) ?? (o2 as dynamic).Multiply(o1 as dynamic);
            public static Operand operator /(Operand o1, Operand o2) => o1 * (o2 ^ -1);
            public static Operand operator ^(Operand o1, Operand o2) => (o1 as dynamic).Exponentiate(o2 as dynamic);

            public static implicit operator Operand(Variable v) => new Term(v);
            public static implicit operator Operand(double d) => new Term(d);

            public override bool Equals(object obj) => obj.GetType() == GetType() && obj.GetHashCode() == GetHashCode();
            public override int GetHashCode() => ToString().GetHashCode();
        }
    }
}