using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace Crunch
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

        public override string ToString()
        {
            return value.ToString();
        }
    }
}