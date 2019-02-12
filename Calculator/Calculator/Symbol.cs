using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculator
{
    public class Symbol
    {
        public static readonly Symbol Cursor = new Symbol();

        public virtual string text
        {
            get
            {
                return _text;
            }
        }
        private string _text;

        public Format format = new Format();

        public Symbol() : this("") { }

        public Symbol (string Text)
        {
            _text = Text;
        }

        public virtual Symbol Copy()
        {
            return new Symbol(text);
        }

        public virtual List<Symbol> GetText()
        {
            return new List<Symbol>() { this };
        }
    }

    public class Bar : Symbol { }

    public class Answer : Symbol
    {
        public static bool isFraction = true;

        private Term value;

        public Answer(Term Value)
        {
            value = Value;
        }

        public Term Get()
        {
            if (isFraction)
            {
                return value;
            }
            else
            {
                return new Number(value.value);
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

        public override Symbol Copy()
        {
            return new Text(text);
        }
    }

    public class Operand : Text
    {
        public Operand(string s) : base(s) { }
    }

    public class Space : Symbol { }

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
                try
                {
                    return value.ToString();
                }
                catch
                {
                    return "null";
                }
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

    public class Edit : Number
    {
        public Edit(double d) : base(d) { }

        public void Dispatch(double d)
        {
            _value = d;
        }
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

        public Exponent(Symbol Num, Symbol Power)
        {
            num = Expression.Wrap(Num);
            power = Expression.Wrap(Power);
        }

        public override List<Symbol> GetText()
        {
            return new List<Symbol>() { num, power };
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
        private Bar bar = new Bar();
        private Expression denominator;

        public Fraction(Symbol n) : this(n, new Number(1)) { }

        public Fraction(Symbol n, Symbol d)
        {
            numerator = Expression.Wrap(n);
            denominator = Expression.Wrap(d);
        }

        public override Term Simplify()
        {
            Term n = Evaluate(numerator);
            Term d = Evaluate(denominator);

            if (n.GetType() != typeof(Fraction) && d.GetType() != typeof(Fraction))
            {
                if (value == (int)value)
                {
                    return new Number(value);
                }
                else if (n.value == (int)n.value && d.value == (int)d.value)
                {
                    Number temp = new Number(gcd((int)n.value, (int)d.value));
                    return new Fraction(new Number(n.value / temp.value), new Number(d.value / temp.value));
                }
                else
                {
                    return new Fraction(n, d);
                }
            }
            else
            {
                Fraction top, bottom;

                if (n.GetType() == typeof(Fraction))
                {
                    top = Wrap((n as Fraction).Simplify());
                }
                else
                {
                    top = new Fraction(n);
                }

                if (d.GetType() == typeof(Fraction))
                {
                    bottom = Wrap(((Fraction)(d)).Simplify());
                }
                else
                {
                    bottom = new Fraction(d);
                }

                return new Fraction(Evaluate(top.numerator).Multiply(Evaluate(bottom.denominator)), Evaluate(top.denominator).Multiply(Evaluate(bottom.numerator)));
            }
        }

        public Fraction Wrap(Term t)
        {
            if (t.GetType() == typeof(Fraction))
            {
                return t as Fraction;
            }
            else
            {
                return new Fraction(t);
            }
        }

        public int gcd(int a, int b)
        {
            if (a == 0)
                return b;
            if (b == 0)
                return a;

            if (a > b)
                return gcd(a % b, b);
            else
                return gcd(a, b % a);
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
            return new Fraction(Evaluate(numerator).Multiply(Evaluate(other.numerator)), Evaluate(denominator).Multiply(Evaluate(other.denominator)));
        }

        public override List<Symbol> GetText()
        {
            return new List<Symbol>() { numerator, new Bar(), denominator };
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