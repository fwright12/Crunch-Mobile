using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Windows.Input;
using Crunch;
using Xamarin.Forms;
using Xamarin.Forms.MathDisplay;

namespace Calculator
{
    public class CrunchCalculator : BasicCalcViewModel, ICalculator
    {
        public MainPage Interface { get; set; }
        public ICommand NewCommand { get; protected set; }

        private readonly ICalculator ScientificCalculator = new ScientificCalculator();

        private MathEntryViewModel MathField => Interface?.FocusedMathField.BindingContext as MathEntryViewModel;
        private Equation MathEntry => SoftKeyboard.Cursor.Parent<Equation>();
        private string LastOperation;

        public CrunchCalculator() : base()
        {
            Calculator = this;

            ICommand baseClearCommand = ClearCommand;
            ClearCommand = new Command(value =>
            {
                baseClearCommand?.Execute(value);

                Expression root = MathEntry?.LHS;

                if (root != null)
                {
                    SoftKeyboard.MoveCursor(root);
                    while (root.Children.Count > 1)
                    {
                        root.Children.RemoveAt(1);
                    }
                }

                LastOperation = default;
            });

            NewCommand = new Command(() =>
            {
                baseClearCommand?.Execute(null);
                MathField?.MoveCursorCommand?.Execute(MathEntryViewModel.CursorKey.Down);
            });
        }

        public string Compute(string operand1, string operation, string operand2)
        {
            string result;

            if (Interface?.CurrentState().Name != MainPage.BASIC_MODE)
            {
                result = ScientificCalculator.Compute(operand1, operation, operand2);
            }
            else
            {
                Equation parentEquation = MathEntry;
                Answer answer = parentEquation?.RHS as Answer;

                bool negatedAnswer = false;
                bool sameAnswer = operand1 == null && answer?.RawAnswer == null;
                if (operand1 != null && answer?.RawAnswer != null)
                {
                    Operand ans = Format(answer.RawAnswer);
                    Operand op1 = Format(Crunch.Math.Evaluate(operand1));
                    bool Compare(Operand a, Operand b) => a.ToString() == b.ToString();

                    sameAnswer = Compare(ans, op1);

                    if (!sameAnswer && (op1.IsNegative ^ ans.IsNegative))
                    {
                        op1 = Format(Crunch.Math.Evaluate("(" + operand1 + ")*-1"));
                        negatedAnswer = sameAnswer = Compare(ans, op1);
                    }
                }
                
                //if (LastOperation == default)
                if (!sameAnswer)
                //if (Answer?.RawAnswer == null)
                {
                    NewCommand?.Execute(null);
                    Input(operand1 ?? string.Empty);
                }
                else if (negatedAnswer || ((LastOperation == "+" || LastOperation == "-") && (operation == "*" || operation == "/")))
                {
                    SoftKeyboard.MoveCursor(parentEquation.LHS);
                    if (negatedAnswer)
                    {
                        Input("-");
                    }
                    Input("(");
                    parentEquation.LHS.End();
                    Input(")");
                }

                Input(operation);
                Input(operand2);

                parentEquation = MathEntry;
                answer = parentEquation?.RHS as Answer;
                parentEquation.LHS.End();

                result = Format(answer == null ? Crunch.Math.Evaluate("0") : answer.RawAnswer)?.ToString();
            }

            LastOperation = operation;

            return result;
        }

        public Operand Format(Operand operand) => operand?.Format(Polynomials.Expanded, Numbers.Decimal, Trigonometry.Degrees);

        private void Input(string input) => MathField?.InputCommand?.Execute(input);
    }

    public class ScientificCalculator : ICalculator
    {
        public string Compute(string operand1, string operation, string operand2)
        {
            try
            {
                return Crunch.Math.Evaluate(operand1 + operation + operand2)?.Format(Polynomials.Expanded, Numbers.Decimal, Trigonometry.Degrees)?.ToString();
            }
            catch
            {
                return null;
            }
        }
    }
}
