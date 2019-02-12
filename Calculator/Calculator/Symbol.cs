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

        public abstract Symbol Copy();
        public Format format;

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

        public override Symbol Copy()
        {
            throw new NotImplementedException();
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

        public override Symbol Copy()
        {
            return new Text(text);
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
                return func;
            }
        }

        private string func;

        public Function(string Func)
        {
            func = Func;
        }

        public Number evaluate(Term t)
        {
            return new Number(search(t));
        }

        private double search(Term t)
        {
            double input = t.value;

            switch (func)
            {
                case "sin":
                    return Math.Sin(input);
                case "cos":
                    return Math.Cos(input);
                case "tan":
                    return Math.Tan(input);
                case "sqrt":
                    return Math.Sqrt(input);
                case "ln":
                    return Math.Log(input);
                case "log":
                    return Math.Log10(input);
                default:
                    throw new NotSupportedException();
            }
        }

        public override Symbol Copy()
        {
            throw new NotImplementedException();
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

        public virtual Term Simplify()
        {
            return new Number(value);
        }

        public virtual Term Evaluate(Expression other)
        {
            return other.answer as dynamic;
        }

        public virtual Term Add(Term other)
        {
            return new Number(value + other.value);
        }

        public virtual Term Multiply(Term other)
        {
            return new Number(value * other.value);
        }

        public virtual Term Pow(Term other)
        {
            return new Number(Math.Pow(value, other.value));
        }
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

        protected double _value;

        public Number(double d)
        {
            _value = d;
        }

        public Term Add(Fraction other)
        {
            return other.Add(this);
        }

        public Term Multiply(Fraction other)
        {
            return other.Multiply(this);
        }

        public Term Pow(Fraction other)
        {
            return other.Pow(this);
        }

        public override Symbol Copy()
        {
            return new Number(value);
        }
    }

    public class Power : Expression
    {

    }

    public class Exponent : Term
    {
        public override double value
        {
            get
            {
                return Math.Pow(Evaluate(num).value, Evaluate(power).value);
            }
        }

        private Expression num;
        private Expression power;

        public Exponent(Expression Num, Expression Power)
        {
            num = Num;
            power = Power;
            power.format = new Format(padding: 75);
        }

        public override List<Symbol> GetText()
        {
            return new List<Symbol>() { num, power };
        }

        public override Symbol Copy()
        {
            throw new NotImplementedException();
        }
    }

    public class Fraction : Term
    {
        public override double value
        {
            get
            {
                return Evaluate(numerator).value / Evaluate(denominator).value;
            }
        }

        private Expression numerator;
        private Expression denominator;

        public Fraction(Symbol n, Symbol d)
        {
            numerator = Expression.Wrap(n);
            denominator = Expression.Wrap(d);

            /*if (n.GetType() == typeof(Expression))
                numerator = n as Expression;
            else
                numerator = new Expression(new List<Symbol>() { n });

            if (d.GetType() == typeof(Expression))
                denominator = d as Expression;
            else
                denominator = new Expression(new List<Symbol>() { d });*/
        }

        public override Term Simplify()
        {
            return new Fraction(Evaluate(numerator), Evaluate(denominator));
        }

        public Term Add(Number other)
        {
            return new Fraction(Evaluate(numerator).Add(other.Multiply(Evaluate(denominator))), denominator);
        }

        public Term Add(Fraction other)
        {
            throw new NotImplementedException();
        }

        public Term Multiply(Number other)
        {
            return new Fraction(other.Multiply(Evaluate(numerator)), denominator);
        }

        public Term Multiply(Fraction other)
        {
            throw new NotImplementedException();
        }

        public override List<Symbol> GetText()
        {
            return new List<Symbol>() { numerator, denominator };
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(Fraction))
                return false;

            return GetHashCode() == (obj as Fraction).GetHashCode();
        }

        public override Symbol Copy()
        {
            return new Fraction(numerator.Copy(), denominator.Copy());
        }

        public override int GetHashCode()
        {
            return (numerator.GetHashCode() + denominator.GetHashCode()) % int.MaxValue;
        }
    }
}