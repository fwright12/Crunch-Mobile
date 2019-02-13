using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Crunch
{
    public class Exponent : Term
    {
        public override double value
        {
            get { return System.Math.Pow(Base.value, Power.value); }
        }

        private Term Base;
        private Term Power;

        public Exponent(Term _base, Term _power)
        {
            Base = _base;
            Power = _power;
        }

        public override Term Simplify()
        {
            return new Number(value);
        }
    }
}