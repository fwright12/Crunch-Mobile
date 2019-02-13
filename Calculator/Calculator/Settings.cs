using System;
using System.Collections.Generic;
using System.Text;

using Crunch.Engine;

namespace Calculator
{
    public static class Settings
    {
        public static int DecimalPlaces
        {
            get { return Crunch.Engine.Math.DecimalPlaces; }
            set { Crunch.Engine.Math.DecimalPlaces = value; }
        }

        public static Polynomials Polynomials;
        public static Numbers Numbers = Numbers.Exact;
        public static Trigonometry Trigonometry = Trigonometry.Degrees;

        public static bool ClearCanvasWarning = true;
        public static bool LeftHanded = false;
    }
}
