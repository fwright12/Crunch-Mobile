using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using System.Extensions;
using Xamarin.Forms;
using Xamarin.Forms.Extensions;
using Crunch.GraphX;
using Crunch.Engine;

namespace Calculator
{
    public class FormEnumertor : IEnumerator<Operand.Form>
    {
        private Operand.Form form;

        private List<Polynomials> polynomials = new List<Polynomials>() { Polynomials.Expanded, Polynomials.Factored };
        private List<Numbers> numbers = new List<Numbers>() { Numbers.Exact, Numbers.Decimal };
        private int position = -1;

        public FormEnumertor(Operand.Form form)
        {
            this.form = form;
        }

        public Operand.Form Current => null;
        object IEnumerator.Current => Current;

        public void Dispose() { }

        public bool MoveNext()
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
    }

    public class Answer : StackLayout // Expression, ITouchable
    {
        public Polynomials PolynomialChoice = Settings.Polynomials;
        public Numbers NumberChoice = Settings.Numbers;
        public Trigonometry TrigChoice = Settings.Trigonometry;

        private Polynomials actualPolynomialForm;
        private Numbers actualNumbersForm;

        private Operand answer;
        private Expression[,,] allFormats = new Expression[2,2,2];
        private List<Operand> formats = new List<Operand>();
        
        private int displayed = 0;
        private TouchableStackLayout DegRadLabel;
        private int clicks;

        public Answer()
        {
            Orientation = StackOrientation.Horizontal;
            VerticalOptions = LayoutOptions.Center;

            DegRadLabel = new TouchableStackLayout { VerticalOptions = LayoutOptions.Fill };
            Label label = new Label { FontSize = Text.MaxFontSize * 2 / 3, Opacity = 0.5, VerticalOptions = LayoutOptions.EndAndExpand, Margin = new Thickness(3, 0, 0, 0) };

            DegRadLabel.Click += delegate
            {
                TrigChoice = TrigChoice.Iterate((int)TrigChoice);
                SwitchDegRad(label);
                SwitchToNextValidFormat(actualPolynomialForm, actualNumbersForm, false);
            };

            DegRadLabel.Children.Add(label);
            SwitchDegRad(label);

            clicks = Enum.GetNames(typeof(Polynomials)).Length + Enum.GetNames(typeof(Numbers)).Length;
        }

        public void Update(Operand answer)
        {
            Children.Clear();

            this.answer = answer;
            allFormats = new Expression[2, 2, 2];

            if (answer != null)
            {
                SwitchToNextValidFormat(PolynomialChoice, NumberChoice, false);
            }
        }

        private void NextFormat(ref Polynomials polynomials, ref Numbers numbers)
        {
            numbers = numbers.Iterate((int)numbers);

            if (numbers == Numbers.Exact)
            {
                polynomials = polynomials.Iterate((int)polynomials);
            }
        }

        private bool SwitchToNextValidFormat(Polynomials polynomials, Numbers numbers, bool existingFormat = true)
        {
            Expression e;
            int i = 0;
            do
            {
                if (i > 0 || existingFormat)
                {
                    NextFormat(ref polynomials, ref numbers);
                }

                //print.log("checking format ", polynomials, numbers, TrigChoice);
                e = allFormats[(int)polynomials, (int)numbers, (int)TrigChoice];

                if (e == null) {
                    Operand o;

                    try
                    {
                        o = answer.Format(polynomials, numbers, TrigChoice);
                    }
                    catch
                    {
                        if (Children.Count > 0)
                        {
                            Children.RemoveAt(0);
                        }
                        return false;
                    }

                    if (o != null)
                    {
                        //print.log("rendering new answer");
                        e = new Expression(Render.Math(o.ToString()));
                        e.Click += delegate
                        {
                        //print.log("current format is ", actualPolynomialForm, actualNumbersForm, TrigChoice);

                        if (SwitchToNextValidFormat(actualPolynomialForm, actualNumbersForm))
                            {
                                if (answer.HasForm(PolynomialChoice))
                                {
                                    PolynomialChoice = actualPolynomialForm;
                                }
                                if (answer.HasForm(NumberChoice))
                                {
                                    NumberChoice = actualNumbersForm;
                                }
                            }

                            //print.log("ended on " + actualPolynomialForm, actualNumbersForm, TrigChoice);
                        };

                        allFormats[(int)polynomials, (int)numbers, (int)TrigChoice] = e;
                    }
                }

                if (e != null)
                {
                    actualPolynomialForm = polynomials;
                    actualNumbersForm = numbers;

                    break;
                }

                if (++i == clicks - existingFormat.ToInt())
                {
                    return false;
                }
            } while (true);

            //print.log("updating answer");

            Children.Clear();
            Children.Add(e);
            if (answer.HasTrig)
            {
                Children.Add(DegRadLabel);
            }

            return true;
        }

        private void SwitchDegRad(Label label)
        {
            if (TrigChoice == Trigonometry.Degrees)
            {
                label.Text = "deg";
            }
            else
            {
                label.Text = "rad";
            }
        }
    }
}
