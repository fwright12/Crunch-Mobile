using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculator
{
    public abstract class Number<T>
    {

    }

    public class Quantity
    {
        public object UIobj;
        public double value;

        public static Quantity Parse(string s)
        {
            return new Quantity(double.Parse(s));
        }

        public Quantity(double s) { }

        public virtual double Add(Quantity other)
        {
            return value + other.value;
        }

        public virtual double Subtract(Quantity other)
        {
            return value - other.value;
        }

        public virtual double Multiply(Quantity other)
        {
            return value * other.value;
        }

        public virtual double Divide(Quantity other)
        {
            return value / other.value;
        }

        public virtual double exp(Quantity other)
        {
            return Math.Pow(value, other.value);
        }
    }
}
