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
        new public static Equation Focus;

        public Expression LHS;
        public Answer RHS;
        public bool RecognizeVariables = false;

        private Dictionary<char, Expression> unknowns = new Dictionary<char, Expression>();

        public Equation(string text = "")
        {
            Orientation = StackOrientation.Vertical;

            LHS = text == "" ? new Expression() : new Expression(Render.Math(text));
            LHS.Editable = true;

            Text equals = new Text(" = ") { FontSize = Text.MaxFontSize };
            Touch += (point, state) =>
            {
                if (state == TouchState.Down)
                {
                    this.BeginDrag(MainPage.VisiblePage.Canvas.Bounds);
                }
            };

            StackLayout layout = new StackLayout { Orientation = StackOrientation.Horizontal, Spacing = 0, VerticalOptions = LayoutOptions.Center };
            layout.Children.AddRange(LHS, equals, RHS = new Answer());

            Children.Add(layout);
            Children[0].HorizontalOptions = LayoutOptions.Start;
            
            SetAnswer();
        }

        public void SetAnswer()
        {
            print.log("Entered: " + LHS.ToString());

            Dictionary<char, Operand> substitutions = new Dictionary<char, Operand>();
            for (int i = 1; i < Children.Count; i++)
            {
                string s = (Children[i] as Expression).Children[1].ToString();
                if (s != "()")
                {
                    Operand temp = Crunch.Engine.Math.Evaluate(s);
                    if (temp != null)
                    {
                        substitutions.Add(((Children[i] as Expression).Children[0] as Text).Text[0], temp);
                    }
                }
            }

            Dictionary<char, Operand> updatedUnknowns = new Dictionary<char, Operand>(); 
            Operand answer = Crunch.Engine.Math.Evaluate(LHS.ToString(), ref updatedUnknowns);

            if (answer != null)
            {
                answer.Knowns = substitutions;
            }
            RHS.Update(answer);

            if (RecognizeVariables)
            {
                foreach (char c in unknowns.Keys)
                {
                    if (answer == null || !answer.Unknowns.Contains(c))
                    {
                        unknowns[c].Parent.IsVisible = false;
                    }
                }

                if (answer != null)
                {
                    foreach (char c in answer.Unknowns)
                    {
                        if (!unknowns.ContainsKey(c))
                        {
                            unknowns.Add(c, AddVariable(c));
                        }
                        else
                        {
                            unknowns[c].Parent.IsVisible = true;
                        }
                    }
                }
            }

            print.log("*************************");
        }

        private Expression AddVariable(char c)
        {
            Expression e = new Expression(new Text(c + " = "), new Expression() { Editable = true });
            e.HorizontalOptions = LayoutOptions.Start;
            Children.Add(e);
            return e.Children[1] as Expression;
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
