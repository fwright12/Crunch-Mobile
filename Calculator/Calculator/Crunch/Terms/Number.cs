using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace Calculator.Crunch
{
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

        public Number(double value)
        {
            _value = value;
        }
    }
}

namespace Calculator.Graphics
{
    public class Number : Text
    {
        public Number(string text, bool selectable = true) : base(text, selectable) { }

        public override View Render()
        {
            Label temp = base.Render() as Label;
            temp.Text = text;
            return temp;
        }

        public static implicit operator Crunch.Number(Number n)
        {
            return new Crunch.Number(double.Parse(n.text));
        }
    }
}