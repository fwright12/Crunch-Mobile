using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crunch
{
    public class Exponent : Term
    {
        public override double value
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /*public override double value
        {
            get
            {
                return Math.Pow(Evaluate(num).value, Evaluate(power).value);
            }
        }

        private Expression num;
        private Expression power;

        public Exponent(Symbol Num, Symbol Power)
        {
            num = Expression.Wrap(Num);
            power = Expression.Wrap(Power);
        }

        public override int GetHashCode()
        {
            return (num.GetHashCode() + power.GetHashCode()) % int.MaxValue;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(Exponent))
                return false;

            return GetHashCode() == (obj as Exponent).GetHashCode();
        }*/
    }
}
