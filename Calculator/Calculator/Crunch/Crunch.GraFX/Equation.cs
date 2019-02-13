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

        private EqualSign equals;

        private bool isDecimal = true;

        public Equation(string text = "")
        {
            AddRange(LHS = new Expression(Render.Math(text)), new EqualSign(), RHS = new Answer());
            SetAnswer();
            RHS.Touch += delegate { RHS.SwitchFormat(); };
        }

        public void SetAnswer()
        {
            print.log("Entered: " + LHS.ToString());

            Math.Operand o = Math.Evaluate(LHS.ToString());
            if (o == null)
            {
                RHS.Children.Clear();
            }
            else
            {
                RHS.Update(o);
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
