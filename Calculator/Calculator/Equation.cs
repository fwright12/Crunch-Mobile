using System;
using System.Collections.Generic;
using System.Text;

using Xamarin.Forms;
using Xamarin.Forms.Extensions;
using Crunch.Engine;
using Crunch.GraphX;

namespace Calculator
{
    public class Equation : StackLayout
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

            Text equals = new Text(" = ");
            equals.Touch += (point, state) =>
            {
                if (state == TouchState.Down)
                {
                    this.BeginDrag(MainPage.VisiblePage.Canvas.Bounds);
                }
            };

            Children.Add(new Expression(LHS, equals, RHS = new Answer()));
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
                    List<Operand> temp = Crunch.Engine.Math.Evaluate(s);
                    if (temp.Count > 0)
                    {
                        substitutions.Add(((Children[i] as Expression).Children[0] as Text).Text[0], temp[0]);
                    }
                }
            }

            List<Operand> answers = Crunch.Engine.Math.Evaluate(LHS.ToString(), ref substitutions);
            
            if (RecognizeVariables && (answers.Count > 0 || LHS.ChildCount() == 0))
            {
                foreach (char c in unknowns.Keys)
                {
                    if (!substitutions.ContainsKey(c))
                    {
                        unknowns[c].Parent.IsVisible = false;
                    }
                }

                foreach(char c in substitutions.Keys)
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

            /*if (answers.Count == 0)
            {
                RHS.Children.Clear();
            }
            else
            {*/
                RHS.Update(answers);
            //}

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
            return LHS.ToLatex() + "=" + RHS.ToLatex();
        }

        public override string ToString()
        {
            return LHS.ToString() + "=" + RHS.ToString();
        }
    }
}
