using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculator.Crunch
{
    public abstract class Term
    {
        public abstract double value
        {
            get;
        }

        public static Number operator +(Term t1, Term t2)
        {
            return new Number(t1.value + t2.value);
        }

        public static Term operator -(Term t1, Term t2)
        {
            return (t2 as dynamic) * new Number(-1) + (t1 as dynamic);
        }

        public static Number operator *(Term t1, Term t2)
        {
            return new Number(t1.value * t2.value);
        }

        public virtual Term Pow(Term other)
        {
            return new Number(System.Math.Pow(value, other.value));
        }
    }
}
