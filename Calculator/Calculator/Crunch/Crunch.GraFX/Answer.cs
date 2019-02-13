using System;
using System.Collections.Generic;
using System.Text;

using Xamarin.Forms;

namespace Crunch.GraFX
{
    public class Answer : Expression
    {
        public event Calculator.TouchEventArgs Touch;

        private View[] raw;
        private View[] value = null;
        private bool isDecimal = false;

        public void Update(Math.Operand o) 
        {
            Math.Operand simplified = o.Simplify();
            if (!simplified.Equals(o))
            {
                value = Render.Math(simplified.ToString());
            }
            else
            {
                value = null;
            }

            raw = Render.Math(o.ToString());
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
            Trim();
        }

        public void Touched() => Touch(Point.Zero);
    }
}
