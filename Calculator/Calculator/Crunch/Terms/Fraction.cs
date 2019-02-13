using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace Calculator.Crunch
{
    public class Fraction : Term
    {
        public override double value
        {
            get
            {
                return numerator.value / denominator.value;
            }
        }

        public Term numerator;
        public Term denominator;

        public Fraction(Term n) : this(n, new Number(1)) { }

        public Fraction(Term Numerator, Term Denominator)
        {
            numerator = Numerator;
            denominator = Denominator;
        }

        public Term Simplify()
        {
            //Both numerator and denominator are whole numbers
            if (numerator.GetType() != typeof(Fraction) && denominator.GetType() != typeof(Fraction))
            {
                //The fraction simplifies to a whole number
                if (value == (int)value)
                {
                    return new Number(value);
                }
                //If the numerator and denominator are whole numbers, look for gcd
                else if (numerator.value == (int)numerator.value && denominator.value == (int)denominator.value)
                {
                    Number temp = new Number(gcd((int)numerator.value, (int)denominator.value));
                    return new Fraction(new Number(numerator.value / temp.value), new Number(denominator.value / temp.value));
                }
                //Can't be further simplified
                else
                {
                    return new Fraction(numerator, denominator);
                }
            }
            else
            {
                //Term top = new Fraction(numerator).Simplify();
                //Term bottom = new Fraction(denominator).Simplify();

                Term[] terms = new Term[] { numerator, denominator };
                Fraction[] fractions = new Fraction[2];

                for (int i = 0; i < 2; i++)
                {
                    var term = terms[i];

                    if (term is Fraction)
                    {
                        var temp = (term as Fraction).Simplify();
                        if (temp is Fraction)
                        {
                            fractions[i] = temp as Fraction;
                        }
                        else
                        {
                            fractions[i] = new Fraction(temp);
                        }
                    }
                    else
                    {
                        fractions[i] = new Fraction(terms[i]);
                    }
                }

                /*if (numerator is Fraction)
                {
                    top = new Fraction((numerator as Fraction).Simplify()); //Wrap((n as Fraction).Simplify());
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
                }*/

                //return new Fraction(Evaluate(top.numerator).Multiply(Evaluate(bottom.denominator)), Evaluate(top.denominator).Multiply(Evaluate(bottom.numerator)));
                return new Fraction(fractions[0].numerator * fractions[1].denominator, fractions[0].denominator * fractions[1].numerator);
            }
        }

        //Problem here when fraction is negative
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

        public static Fraction operator +(Number n, Fraction f) { return f + n; }

        public static Fraction operator +(Fraction f, Number n)
        {
            return new Fraction(f.numerator + n * f.denominator, f.denominator);
        }

        public static Fraction operator +(Fraction f1, Fraction f2)
        {
            return new Fraction(f1.numerator * f2.denominator + f1.denominator * f2.numerator, f1.denominator * f2.denominator);
        }

        public static Fraction operator *(Number n, Fraction f) { return f * n; }

        public static Fraction operator *(Fraction f, Number n)
        {
            return new Fraction(n * f.numerator, f.denominator);
        }

        public static Fraction operator *(Fraction f1, Fraction f2)
        {
            return new Fraction(f1.numerator * f2.numerator, f1.denominator * f2.denominator);
        }
    }
}

namespace Calculator.Graphics
{
    public class Fraction : Container
    {
        public static Queue<Action<Symbol>> Creator = new Queue<Action<Symbol>>();

        public Fraction()
        {
            Creator.Enqueue(delegate (Symbol text)
            {
                Add(new Expression(GetQuantity(text, -1)));
                Add(new Expression(GetQuantity(text, 1)));
            });
        }

        public Fraction(Expression Numerator, Expression Denominator)
        {
            Add(Numerator);
            Add(Denominator);
        }

        public List<Symbol> GetQuantity(Symbol start, int direction)
        {
            List<Symbol> result = new List<Symbol>();
            List<Symbol> list = start.Parent.Children.ToList();

            int index = list.IndexOf(start);
            while ((index + direction).IsBetween(0, list.Count - 1) && list[index + direction] is Number)
            {
                index += direction;
                result.Insert(result.Count * (direction + 1) / -2, list[index]);
            }

            return result;
        }

        public override View Render()
        {
            var temp = BaseLayout();
            temp.Orientation = StackOrientation.Vertical;
            temp.BackgroundColor = Color.Black;
            temp.Spacing = 2;

            /*temp.ChildAdded += delegate
            {
                if (temp.Children.Count == 2)
                {
                    var bar = new StackLayout();
                    bar.Padding = new Thickness(0, 0, 0, 3);
                    bar.BackgroundColor = Color.Black;
                    temp.Children.Insert(1, bar);
                }
            };*/

            return temp;
        }

        public static implicit operator Crunch.Fraction(Fraction f)
        {
            return new Crunch.Fraction(f.Children.First() as dynamic, f.Children.Last() as dynamic);
        }
    }
}