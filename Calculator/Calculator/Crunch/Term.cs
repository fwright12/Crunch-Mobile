using System;
using System.Collections.Generic;
using System.Text;

namespace Crunch.Math
{
    public class Term
    {
        public Operand Coefficient;
        public Variable Variable;

        public Term(Operand coefficient, Variable variable)
        {
            Coefficient = coefficient;
            Variable = variable;
        }

        public Term() : this(new Constant(1), null) { }

        public Term(Operand coefficient) : this(coefficient, null) { }

        public static Term operator +(Term t1, Term t2) => new Term(t1.Coefficient + t2.Coefficient, t1.Variable);
        public static Term operator *(Term t, Operand o) => new Term(t.Coefficient * o, t.Variable);
        public static Term operator *(Operand o, Term t) => t * o;

        public static Term operator /(Term t1, Term t2) => new Term(new Fraction(t1.Coefficient, t2.Coefficient), t1.Variable);


        public bool IsLike(Term term)
        {
            return true;
        }

        public override string ToString()
        {
            return Coefficient.ToString();// + Variable.ToString();
        }
    }
}
