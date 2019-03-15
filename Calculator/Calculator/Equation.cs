using System;
using System.Collections.Generic;
using System.Text;

using System.Extensions;
using Xamarin.Forms;
using Xamarin.Forms.MathDisplay;
using Xamarin.Forms.Extensions;
using Crunch;

namespace Calculator
{
    public class Equation : MathLayout
    {
        public Expression LHS { get; private set; }
        public Answer RHS { get; private set; }

        public override double Middle => 0.5;
        public override Expression InputContinuation => LHS;
        public override double MinimumHeight => System.Math.Max(LHS.MinimumHeight, RHS.MinimumHeight);

        public Equation(string text = "")
        {
            //BackgroundColor = Color.PowderBlue;
            Orientation = StackOrientation.Horizontal;
            Spacing = 0;
            VerticalOptions = LayoutOptions.Center;
            
            LHS = text == "" ? new Expression() : new Expression(Xamarin.Forms.MathDisplay.Reader.Render(text));
            LHS.Editable = true;

            LHS.Touch += (point, state) =>
            {
                if (state == TouchState.Moving)
                {
                    MainPage.VisiblePage.EnterCursorMode(point.Add(LHS.PositionOn(MainPage.VisiblePage.Canvas).Add(new Point(-MainPage.phantomCursor.Width / 2, -Text.MaxTextHeight))));
                }
            };

            Text equals = new Text(" = ") { FontSize = Text.MaxFontSize };

            Children.AddRange(LHS, equals, RHS = new Answer());

            SetAnswer();
        }

        public HashSet<char> SetAnswer() => SetAnswer(new Dictionary<char, Operand>());

        public HashSet<char> SetAnswer(Dictionary<char, Operand> substitutions)
        {
            print.log("Entered: " + LHS.ToString());

            Operand answer = Crunch.Reader.Evaluate(LHS.ToString());

            /*if (answer != null)
            {
                answer.Knowns = substitutions;
            }*/
            RHS.Update(answer, substitutions);

            print.log("*************************");

            return answer == null ? new HashSet<char>() : answer.Unknowns;
        }

        public override void Lyse() => Lyse(LHS);

        public override string ToLatex()
        {
            return LHS.ToLatex() + "=";// + RHS.ToLatex();
        }

        public override string ToString()
        {
            return LHS.ToString() + "=" + RHS.ToString();
        }
    }
}
