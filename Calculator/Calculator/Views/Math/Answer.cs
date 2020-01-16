using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using System.Extensions;
using Xamarin.Forms;
using Xamarin.Forms.Extensions;
using Xamarin.Forms.MathDisplay;
using Crunch;

namespace Calculator
{
    /*public class FormEnumertor : IEnumerator<Operand.Form>
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
    }*/

    public class Answer : Expression
    {
        public event EventHandler<ChangedEventArgs<Operand>> Changed;
        public event EventHandler<ChangedEventArgs<Expression>> FormChanged;

        public Polynomials PolynomialChoice = App.Polynomials;
        public Numbers NumberChoice = App.Numbers.Value;
        public Trigonometry TrigChoice = App.Trigonometry.Value;
        public Dictionary<char, Operand> Knowns;

        private Polynomials actualPolynomialForm;
        private Numbers actualNumbersForm;

        public Operand answer { get; private set; }
        private Expression[,,] allFormats = new Expression[2, 2, 2];
        private List<Operand> formats = new List<Operand>();

        private TouchableStackLayout DegRadLabel;
        private int clicks;

        public Answer()
        {
            Orientation = StackOrientation.Horizontal;
            VerticalOptions = LayoutOptions.Center;

            DegRadLabel = new TouchableStackLayout { VerticalOptions = LayoutOptions.Fill };
            Label label = new Label
            {
                FontSize = Text.MaxFontSize * 2 / 3,
                Opacity = 0.5,
                VerticalOptions = LayoutOptions.EndAndExpand,
                Margin = new Thickness(3, 0, 0, 0)
            };

            TapGestureRecognizer tgr = new TapGestureRecognizer();
            tgr.Tapped += delegate
            {
                TrigChoice = TrigChoice.Iterate((int)TrigChoice);
                SwitchDegRad(label);
                SwitchToNextValidFormat(actualPolynomialForm, actualNumbersForm, false);
            };
            DegRadLabel.GestureRecognizers.Add(tgr);

            DegRadLabel.Children.Add(label);
            SwitchDegRad(label);

            TapGestureRecognizer tgr1 = new TapGestureRecognizer();
            tgr1.Tapped += delegate
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
            GestureRecognizers.Add(tgr1);

            clicks = Enum.GetNames(typeof(Polynomials)).Length + Enum.GetNames(typeof(Numbers)).Length;
        }

        public IEnumerable<Element> GetListeners()
        {
            if (FormChanged == null)
            {
                yield break;
            }
            
            foreach(Delegate d in FormChanged.GetInvocationList())
            {
                if (d.Target is Element)
                {
                    yield return d.Target as Element;
                }
            }
        }

        /*public void Update(Operand answer, Dictionary<char, Operand> knowns)
        {
            Knowns = knowns;
            Update(answer);
        }*/

        public void Update(Operand answer)
        {
            Children.Clear();

            allFormats = new Expression[2, 2, 2];

            Operand old = this.answer;
            if (answer != null)
            {
                this.answer = answer;
                PolynomialChoice = answer.GetPolynomials;
                SwitchToNextValidFormat(PolynomialChoice, NumberChoice, false);
            }

            Changed?.Invoke(this, new ChangedEventArgs<Operand>(old, answer));
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

                if (e == null)
                {
                    Operand o;

                    try
                    {
                        o = answer.Format(polynomials, numbers, TrigChoice, Knowns);
                    }
                    catch (Exception ex)
                    {
                        if (Children.Count > 0)
                        {
                            Children.RemoveAt(0);
                        }
                        Print.Log("error formatting", ex.Message);
                        return false;
                    }

                    if (o != null)
                    {
                        //print.log("rendering new answer");
                        e = new Expression(Xamarin.Forms.MathDisplay.Reader.Render(o.ToString()));

                        /*PanGestureRecognizer pgr = new PanGestureRecognizer();
                        pgr.PanUpdated += (sender, e1) =>
                        {
                            this.Root<Calculation>().BeginDrag(MainPage.VisiblePage.Canvas.Bounds);
                        };
                        e.GestureRecognizers.Add(pgr);*/

                        

                        allFormats[(int)polynomials, (int)numbers, (int)TrigChoice] = e;
                    }
                }

                if (e != null)
                {
                    Expression old = allFormats[(int)actualPolynomialForm, (int)actualNumbersForm, (int)TrigChoice];

                    actualPolynomialForm = polynomials;
                    actualNumbersForm = numbers;

                    FormChanged?.Invoke(this, new ChangedEventArgs<Expression>(old, e));

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

        public override string ToString()
        {
            string s = answer?.ToString() ?? "";
            return s == "" ? s : "(" + s + ")";
        }

        public string BetterToString()
        {
            if (Children.Count > 0 && Children[0] is Expression)
            {
                return Children[0].ToString(); 
            }
            return "";
        }

        //public override string ToString() => "(" + answer?.ToString() + ")" ?? "";// allFormats[(int)PolynomialChoice, (int)NumberChoice, (int)TrigChoice]?.ToString() ?? "";
    }
}
