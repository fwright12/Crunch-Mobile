using System;
using System.Collections.Generic;
using System.Text;

namespace Crunch.GraFX
{
    public class Equation : Expression
    {
        new public static Equation Focus;

        public Expression LHS;
        public Answer RHS;

        private bool isDecimal = true;

        public Equation()
        {
            AddRange(LHS = new Expression(), new Text(" = "), RHS = new Answer());
        }

        public static void SetAnswer()
        {
            print.log("Entered: " + Focus.LHS);

            Math.Operand o = Math.Evaluate(Focus.LHS.ToString());
            if (o == null)
            {
                Focus.RHS.Children.Clear();
            }
            else
            {
                Focus.RHS.Update(o);
            }
        }

        public override string ToLatex()
        {
            return LHS.ToLatex() + "=" + RHS.ToLatex();
        }

        public override string ToString()
        {
            return LHS.ToString() + "=" + RHS.ToString();
        }
    }
}
