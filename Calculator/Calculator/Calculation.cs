using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Extensions;
using Crunch.Engine;
using Crunch.GraphX;

namespace Calculator
{
    public class Calculation : TouchableStackLayout
    {
        public Equation Main { get; private set; }
        public bool RecognizeVariables = false;

        private Dictionary<char, Equation> Substitutions = new Dictionary<char, Equation>();

        public Calculation(Equation main)
        {
            Orientation = StackOrientation.Vertical;
            Spacing = 0;

            Children.Add(Main = main);
        }

        public void SetAnswer()
        {
            Dictionary<char, Operand> substitutions = new Dictionary<char, Operand>();
            //for (int i = 1; i < Children.Count; i++)
            foreach (KeyValuePair<char, Equation> pair in Substitutions)
            {
                //string s = (Children[i] as Expression).Children[1].ToString();
                string s = pair.Value.LHS.ToString();
                if (s != "()")
                {
                    Operand temp = Crunch.Engine.Math.Evaluate(s);
                    if (temp != null)
                    {
                        substitutions.Add(pair.Key, temp);
                    }
                }
            }

            RestrictedHashSet<char> unknowns = Main.SetAnswer(substitutions);

            if (RecognizeVariables)
            {
                foreach (char c in Substitutions.Keys)
                {
                    if (!unknowns.Contains(c))
                    {
                        (Substitutions[c].Parent as View).IsVisible = false;
                    }
                }

                if (unknowns.Count > 0)
                {
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
