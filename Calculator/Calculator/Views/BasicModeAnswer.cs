using System;
using System.Collections.Generic;
using System.Text;

using Xamarin.Forms;
using Xamarin.Forms.MathDisplay;
using Crunch;

namespace Calculator
{
    public class BasicModeAnswerLabel : Label
    {
        //public Func<string, string, string, string> Operate;

        private readonly char NEGATIVE = '-';

        private string Answer;
        private char Operation;
        private string Operand;
        private string LastOperation = "";

        public BasicModeAnswerLabel()
        {
            Reset();
        }

        public void Reset()
        {
            Answer = string.Empty;
            Operation = default;
            Operand = "";
            LastOperation = default;

            Input('0');
        }

        public void End(Equation equation) => SoftKeyboard.MoveCursor(equation.LHS, equation.LHS.Children.Count);

        private string Operate(string answer, string operation, string operand)
        {
            Operand result;

            if (this.Parent<MainPage>().CurrentState()?.Name != MainPage.BASIC_MODE)
            {
                try
                {
                    result = Crunch.Math.Evaluate(answer + operation + operand);
                }
                catch
                {
                    result = null;
                }
            }
            else
            {
                Equation parentEquation = SoftKeyboard.Cursor.Parent<Equation>();
                Answer rhs = parentEquation?.RHS as Answer;

                bool negatedAnswer = rhs?.RawAnswer == null || answer == null ? false : Crunch.Math.Evaluate(answer).IsNegative ^ rhs.RawAnswer.IsNegative;
                if (negatedAnswer || ((LastOperation == "+" || LastOperation == "-") && (operation == "*" || operation == "/")))
                {
                    Expression current = SoftKeyboard.Cursor.Parent<Equation>().LHS;
                    SoftKeyboard.MoveCursor(current);
                    if (negatedAnswer)
                    {
                        SoftKeyboard.Type("-");
                    }
                    SoftKeyboard.Type("(");
                    SoftKeyboard.MoveCursor(current, current.Children.Count - 1);
                    SoftKeyboard.Type(")");
                }

                if (LastOperation == default)
                {
                    SoftKeyboard.Type(answer ?? "");
                }
                SoftKeyboard.Type(operation);
                SoftKeyboard.Type(operand);
                End(parentEquation);

                result = rhs == null ? Crunch.Math.Evaluate("0") : rhs.RawAnswer;
            }

            LastOperation = operation;

            return result?.Format(Crunch.Polynomials.Expanded, Crunch.Numbers.Decimal, Crunch.Trigonometry.Degrees)?.ToString();
        }

        public void Input(string keystrokes) => Input(keystrokes.ToCharArray());

        public void Input(params char[] keystrokes)
        {
            foreach (char c in keystrokes)
            {
                Input(c);
            }
        }

        public void Input(char keystroke)
        {
            if (keystroke == KeyboardManager.BACKSPACE)
            {
                Operand = OperandEquals("", Operand.Length - 1) ? "0" : Operand.Substring(0, System.Math.Max(0, Operand.Length - 1));
            }
            else if (Crunch.Machine.StringClassification.IsNumber(keystroke.ToString()))
            {
                if (keystroke == '.')
                {
                    if (Operand.Length == 0)
                    {
                        Operand = "0";
                    }
                    else if (Operand.Contains('.'))
                    {
                        return;
                    }
                }
                else if (OperandEquals("0"))
                {
                    Operand = Operand.Replace("0", "");
                }

                Operand += keystroke;
            }
            else
            {
                if (Operand.Length > 0)
                {
                    if (Operation == '=')
                    {
                        string operand = Operand;
                        KeyboardManager.MoveCursor(KeyboardManager.CursorKey.Down);
                        Input(operand);
                    }

                    Answer = Operation == default ? Operand : Operate(Answer, Operation.ToString(), Operand);
                    Operand = "";
                }

                Operation = keystroke;
            }

            Text = Operand.Length > 0 ? Operand : (Answer ?? "Error");
        }

        private bool OperandEquals(string strB, int? length = null)
        {
            int start = (Operand.Length > 0 && Operand[0] == NEGATIVE).ToInt();
            return (length ?? Operand.Length) - start == strB.Length && string.Compare(Operand, start, strB, 0, strB.Length) == 0;
        }

        public void Negate()
        {
            if (Text.Length > 0 && Text[0] == NEGATIVE)
            {
                Text = Text.Substring(1, Text.Length - 1);
            }
            else
            {
                Text = NEGATIVE + Text;
            }

            if (Operand.Length > 0)
            {
                Operand = Text;
            }
            else
            {
                Answer = Text;
            }
        }
    }
}
