using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculator
{
    public abstract class Symbol
    {
        public abstract string text
        {
            get;
        }

        /*public string text
        {
            get
            {
                return graphicsHandler.GetText(this);
            }
            set
            {
                graphicsHandler.SetText(this, value);
            }
        }*/
    }

    public class Operand : Symbol
    {
        public override string text
        {
            get { return _text; }
        }

        private string _text;

        public Operand(string s)
        {
            _text = s;
        }
    }

    public abstract class Term : Symbol
    {
        public override string text
        {
            get
            {
                return value.ToString();
            }
        }

        public abstract double value
        {
            get;
        }

        public abstract Term Add(Number other);
        public abstract Term Add(Fraction other);

        public abstract Term Multiply(Number other);
        public abstract Term Multiply(Fraction other);

        public abstract Term Pow(Number other);
        public abstract Term Pow(Fraction other);
    }

    public class Number : Term
    {
        public override double value
        {
            get
            {
                return _value;
            }
        }

        private double _value;

        public Number(double d)
        {
            _value = d;
        }

        public override Term Add(Number other)
        {
            return new Number(value + other.value);
        }

        public override Term Add(Fraction other)
        {
            return other.Add(this);
        }

        public override Term Multiply(Number other)
        {
            return new Number(value * other.value);
        }

        public override Term Multiply(Fraction other)
        {
            return other.Multiply(this);
        }

        public override Term Pow(Number other)
        {
            return new Number(Math.Pow(value, other.value));
        }

        public override Term Pow(Fraction other)
        {
            return other.Pow(this);
        }
    }

    public class Fraction : Term
    {
        public override double value
        {
            get
            {
                return numerator.evaluate().value / denominator.evaluate().value;
            }
        }
        private Expression numerator;
        private Expression denominator;

        public Fraction(Expression n, Expression d)
        {
            numerator = n;
            denominator = d;
        }

        public override Term Add(Number other)
        {
            throw new NotImplementedException();
        }

        public override Term Add(Fraction other)
        {
            throw new NotImplementedException();
        }

        public override Term Multiply(Number other)
        {
            throw new NotImplementedException();
        }

        public override Term Multiply(Fraction other)
        {
            throw new NotImplementedException();
        }

        public override Term Pow(Number other)
        {
            throw new NotImplementedException();
        }

        public override Term Pow(Fraction other)
        {
            throw new NotImplementedException();
        }
    }
}