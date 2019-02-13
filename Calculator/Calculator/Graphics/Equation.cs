using System;
using System.Collections.Generic;
using System.Text;

using Crunch.Math.Input;

namespace Crunch.GraFX
{
    public class Equation : Expression
    {
        public Expression LHS;
        public Expression RHS;

        public Equation() : this(new Field()) { }

        public Equation(Field e) : base(e)
        {
            LHS = new Expression(new Field());

            Children.Add(LHS);
            Children.Add(new Text(" = "));
        }
    }
}
