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

        public virtual List<Symbol> GetText()
        {
            return new List<Symbol>() { this };
        }
    }

    public class Space : Symbol
    {
        public override string text
        {
            get
            {
                return " ";
            }
        }
    }

    public class Text : Symbol
    {
        public override string text
        {
            get
            {
                return _text;
            }
        }

        private string _text;

        public Text(string s)
        {
            _text = s;
        }
    }

    public class Operand : Text
    {
        public Operand(string s) : base(s) { }
    }

    public class Function : Symbol
    {
        public override string text
        {
            get
            {
                return value.ToString();
            }
        }

        public double value
        {
            get
            {
                return findFunction();
            }
        }

        private string Func;
        private Expression Exp;

        public Function(string func, Expression exp)
        {
            Func = func;
            Exp = exp;
        }

        private double findFunction()
        {
            double input = Exp.evaluate().value;

            switch (Func)
            {
                case "sin":
                    return Math.Sin(input);
                case "cos":
                    return Math.Cos(input);
                case "tan":
                    return Math.Tan(input);
                default:
                    throw new NotSupportedException();
            }
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

        public Expression numerator;
        public Expression denominator;

        public Fraction(Symbol n, Symbol d)
        {
            if (n.GetType() == typeof(Expression))
                numerator = n as Expression;
            else
                numerator = new Expression(new List<Symbol>() { n });

            if (d.GetType() == typeof(Expression))
                denominator = d as Expression;
            else
                denominator = new Expression(new List<Symbol>() { d });

            numerator.Parend = false;
            denominator.Parend = false;
        }

        public Fraction Simplify()
        {
            return new Fraction(numerator.evaluate(), denominator.evaluate());
        }

        public override Term Add(Number other)
        {
            return new Fraction(numerator.evaluate().Add(other.Multiply((dynamic)denominator.evaluate())), denominator);
        }

        public override Term Add(Fraction other)
        {
            throw new NotImplementedException();
        }

        public override Term Multiply(Number other)
        {
            return new Fraction(other.Multiply((dynamic)numerator.evaluate()), denominator);
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

        public override List<Symbol> GetText()
        {
            return new List<Symbol>() { numerator, denominator };
        }

        /*public override bool Equals(object obj)
        {
            return (obj as Fraction).numerator.Equals(numerator) && (obj as Fraction).denominator.Equals(denominator);
        }*/
    }
}