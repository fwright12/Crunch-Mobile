using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Extensions;
using Xamarin.Forms.MathDisplay;
using Crunch;

namespace Calculator
{
    public class Calculation : TouchableStackLayout
    {
        public Equation Main { get; private set; }
        public bool RecognizeVariables = false;

        private Dictionary<char, Equation> Substitutions = new Dictionary<char, Equation>();

        public Calculation(Equation main)
        {
            //BackgroundColor = Color.Aquamarine;
            Orientation = StackOrientation.Vertical;
            Spacing = 0;

            Children.Add(Main = main);

            //(Main.Children[1] as Text)
            Touch += (point, state) =>
            {
                if (state == TouchState.Moving)
                {
                    this.BeginDrag(MainPage.VisiblePage.Canvas.Bounds);
                }
            };
        }

        public void SetAnswer()
        {
            Dictionary<char, Operand> substitutions = new Dictionary<char, Operand>();
            
            foreach (KeyValuePair<char, Equation> pair in Substitutions)
            {
                string s = pair.Value.LHS.ToString();

                if (s != "")
                {
                    Operand temp = Crunch.Reader.Evaluate(s);
                    if (temp != null && !s.Contains(pair.Key))
                    {
                        substitutions.Add(pair.Key, temp);
                    }
                }
            }

            HashSet<char> unknowns = Main.SetAnswer(substitutions);

            if (RecognizeVariables)
            {
                foreach (char c in Substitutions.Keys)
                {
                    if (!unknowns.Contains(c))
                    {
                        (Substitutions[c].Parent as View).IsVisible = false;
                    }
                }

                foreach (char c in unknowns)
                {
                    if (!Substitutions.ContainsKey(c))
                    {
                        Substitutions.Add(c, AddUnknown(c));
                    }
                    else
                    {
                        (Substitutions[c].Parent as View).IsVisible = true;
                    }
                }
            }

            print.log("*************************");
        }

        private Equation AddUnknown(char c)
        {
            Equation e = new Equation();
            e.Children[1].IsVisible = false;
            e.Children[2].IsVisible = false;

            //Expression e = new Expression() { Editable = true };

            Children.Add(new Expression(new Text(c + " = "), e) { HorizontalOptions = LayoutOptions.Start });

            return e;

            //Expression e = new Expression(new Text(c + " = "), new Expression() { Editable = true });
            //e.HorizontalOptions = LayoutOptions.Start;
            //Children.Add(e);
            //return e.Children[1] as Expression;
        }
    }
}
