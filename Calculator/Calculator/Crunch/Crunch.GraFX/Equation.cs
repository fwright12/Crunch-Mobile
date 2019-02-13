using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Crunch.GraFX
{
    public class Equation : Expression
    {
        new public static Equation Focus;

        public Expression LHS;
        public Answer RHS;

        private Expression main;
        private Dictionary<char, Expression> unknowns = new Dictionary<char, Expression>();

        public Equation(string text = "") : base()
        {
            Orientation = StackOrientation.Vertical;

            LHS = text == "" ? new Expression() : new Expression(Render.Math(text));
            LHS.Editable = true;

            Text equals = new Text(" = ");
            equals.Touch += (point, state) =>
            {
                if (state == TouchState.Down)
                {
                    this.BeginDrag(Calculator.MainPage.VisiblePage.Canvas.Bounds);
                }
            };

            main = new Expression(LHS, equals, RHS = new Answer());
            main.HorizontalOptions = LayoutOptions.Start;
            
            Children.Add(main);

            SetAnswer();
        }

        public void SetAnswer()
        {
            print.log("Entered: " + LHS.ToString());

            Math.Operand o = Math.Evaluate(LHS.ToString());

            Dictionary<char, Expression> temp = new Dictionary<char, Expression>();
            foreach (char c in unknowns.Keys)
            {
                if (Parse.Variables.Contains(c))
                {
                    temp.Add(c, unknowns[c]);
                }
                else
                {
                    unknowns[c].Parent.Remove();
                }
            }
            foreach (char c in Parse.Variables)
            {
                if (!temp.ContainsKey(c))
                {
                    temp.Add(c, AddVariable(c));
                }
            }
            unknowns = temp;

            if (o == null)
            {
                RHS.Children.Clear();
            }
            else
            {
                substitutions.Clear();
                foreach(char c in unknowns.Keys)
                {
                    string s = unknowns[c].ToString();
                    if (s != "()")
                    {
                        Math.Operand sub = Math.Evaluate(s);
                        if (sub != null)
                        {
                            substitutions.Add(c, sub);
                        }
                    }
                }

                RHS.Update(o);
                substitutions.Clear();
            }

            print.log("*************************");
        }

        public static Dictionary<char, Math.Operand> substitutions = new Dictionary<char, Math.Operand>();

        private Expression AddVariable(char c)
        {
            Expression e = new Expression(new Text(c + " = "), new Expression() { Editable = true });
            e.HorizontalOptions = LayoutOptions.Start;
            Add(e);
            return e.Children[1] as Expression;
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
