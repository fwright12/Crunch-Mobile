using System;
using System.Collections.Generic;
using System.Text;

using System.Extensions;
using Xamarin.Forms;
using Xamarin.Forms.Extensions;
using Crunch.Engine;
using Crunch.GraphX;

namespace Calculator
{
    public class Equation : TouchableStackLayout
    {
        public Expression LHS { get; private set; }
        public Answer RHS { get; private set; }

        public Equation(string text = "")
        {
            Orientation = StackOrientation.Horizontal;
            Spacing = 0;
            VerticalOptions = LayoutOptions.Center;
            
            LHS = text == "" ? new Expression() : new Expression(Render.Math(text));
            LHS.Editable = true;

            //LHS.Children.Add(new BoxView { Color = Color.Blue, WidthRequest = 5, HeightRequest = 0 });
            /*LHS.Children.Add(new Image { Source = "leftParenthesis.png", HeightRequest = 0, WidthRequest = MainPage.parenthesesWidth, Aspect = Aspect.Fill });
            LHS.Children.Add(new Image { Source = "rightParenthesis.png", HeightRequest = 0, WidthRequest = MainPage.parenthesesWidth, Aspect = Aspect.Fill });
            LHS.Children.Add(new Image { Source = "radical.png", HeightRequest = 0, WidthRequest = MainPage.parenthesesWidth, Aspect = Aspect.Fill });*/

            Text equals = new Text(" = ") { FontSize = Text.MaxFontSize };
            Touch += (point, state) =>
            {
                if (state == TouchState.Down)
                {
                    this.BeginDrag(MainPage.VisiblePage.Canvas.Bounds);
                }
            };

            Children.AddRange(LHS, equals, RHS = new Answer());

            SetAnswer();
        }

        public RestrictedHashSet<char> SetAnswer() => SetAnswer(new Dictionary<char, Operand>());

        public RestrictedHashSet<char> SetAnswer(Dictionary<char, Operand> substitutions)
        {
            print.log("Entered: " + LHS.ToString());

            Dictionary<char, Operand> updatedUnknowns = new Dictionary<char, Operand>();
            Operand answer = Crunch.Engine.Math.Evaluate(LHS.ToString(), ref updatedUnknowns);

            if (answer != null)
            {
                answer.Knowns = substitutions;
            }
            RHS.Update(answer);

            print.log("*************************");

            return answer == null ? new RestrictedHashSet<char>() : answer.Unknowns;
        }

        public string ToLatex()
        {
            return LHS.ToLatex() + "=";// + RHS.ToLatex();
        }

        public override string ToString()
        {
            return LHS.ToString() + "=" + RHS.ToString();
        }
    }
}
