using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crunch
{
    public abstract class Term
    {
        public abstract double value
        {
            get;
        }

        public virtual Term Simplify()
        {
            return new Number(value);
        }

        public static Number operator +(Term t1, Term t2)
        {
            return new Number(t1.value + t2.value);
        }

        public static Term operator -(Term t1, Term t2)
        {
            return t1 + t2 * new Number(-1);
        }

        public static Number operator *(Term t1, Term t2)
        {
            return new Number(t1.value * t2.value);
        }

        public virtual Term Pow(Term other)
        {
            return new Number(Math.Pow(value, other.value));
        }
    }
}
