using System;
using System.Collections.Generic;
using System.Text;

namespace Crunch
{
    using Resolver = Func<object, object, object>;

    public static partial class Math
    {
        public static Operand Evaluate(string str)
        {
            Resolver exponent = (o1, o2) => o1.Parse() ^ o2.Parse();
            Resolver divide = (o1, o2) =>
            {
                print.log("making fraction with " + o1.Parse(), o2.Parse());
                print.log(new Fraction(o1.Parse(), o2.Parse()));
                return o1.Parse() / o2.Parse();
            };
            Resolver multiply = (o1, o2) =>
            {
                print.log("multiplying " + o1.Parse(), o2.Parse());
                Operand o = o1.Parse() * o2.Parse();
                print.log("here");
                return o;
            };
            Resolver subtract = (o1, o2) =>
            {
                Operand o = o1.Parse();
                if (o is null)
                {
                    return new Quantity(o2.Parse() * -1);
                }
                return o - o2.Parse();
            };
            Resolver add = (o1, o2) => o1.Parse() + o2.Parse();

            try
            {
                Quantity q = Crunch.Parse.Math(str, exponent, divide, multiply, subtract, add);
                print.log("q is " + q);
                return q.Parse();
            }
            catch (Exception e)
            {
                print.log("error evaluating", e.Message);
                return null;
            }
        }

        private static Operand Parse(this object str)
        {
            while (str is Quantity)
            {
                str = (str as Quantity).First.Value;
            }
            if (str is Operand)
            {
                return str as Operand;
            }

            return double.Parse(str.ToString());
        }
    }
}
