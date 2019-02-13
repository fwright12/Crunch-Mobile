using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Calculator.Crunch
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

namespace Calculator.Graphics
{
    public class Exponent : Expression
    {
        public override View Render()
        {
            var layout = BaseLayout();

            layout.BackgroundColor = Color.Blue;
            layout.Padding = new Thickness(0, 0, 0, Input.TextSize * 1.5);
            
            layout.DescendantAdded += (sender, e) =>
            {
                print.log(sender);
                return;
                Label l = sender as Label;
                if (l != null)
                {
                    print.log("label added");
                }
            };

            return layout;
        }
    }
}