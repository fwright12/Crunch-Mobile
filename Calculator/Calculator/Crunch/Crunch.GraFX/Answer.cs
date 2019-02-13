using System;
using System.Collections.Generic;
using System.Text;

using Xamarin.Forms;

namespace Crunch.GraFX
{
    public class Answer : Expression
    {
        private View[] raw;
        private View[] value;
        private bool isDecimal = false;

        public void Update(Math.Operand o) 
        {
            if (o is Math.Fraction && (o as Math.Fraction).Numerator is Math.Constant && (o as Math.Fraction).Denominator is Math.Constant)
            {
                Math.Constant n = (o as Math.Fraction).Numerator as Math.Constant;
                Math.Constant d = (o as Math.Fraction).Denominator as Math.Constant;
                value = Render.Math((n.Value / d.Value).ToString());
                raw = Render.Math(o.ToString());
            }
            else
            {
                raw = Render.Math(o.ToString());
            }

            isDecimal = !isDecimal;
            SwitchFormat();
        }

        public void SwitchFormat()
        {
            isDecimal = !isDecimal;
            Children.Clear();

            if (isDecimal && value != null)
            {
                AddRange(value);
            }
            else
            {
                AddRange(raw);
            }
        }
    }
}
