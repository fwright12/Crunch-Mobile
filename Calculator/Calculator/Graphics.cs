using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Crunch.GraFX;

namespace Calculator
{
    public class Number : Text
    {
        public Number(string text) : base()
        {
            Text = text;
        }
    }
}
