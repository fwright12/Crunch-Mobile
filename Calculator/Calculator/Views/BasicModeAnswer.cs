using System;
using System.ComponentModel;
using System.Windows.Input;
using Xamarin.Forms;

namespace Calculator
{
    public interface ICalculator
    {
        string Compute(string operand1, string operation, string operand2);
    }

    public class BasicCalculator : ICalculator
    {
        public string Compute(string operand1, string operation, string operand2)
        {
            double op1, op2;

            if (double.TryParse(operand1, out op1) && double.TryParse(operand2, out op2))
            {
                return OperationLookup(op1, operation, op2)?.ToString();
            }

            return null;
        }

        private double? OperationLookup(double operand1, string operation, double operand2)
        {
            switch (operation)
            {
                case "+": return operand1 + operand2;
                case "-": return operand1 - operand2;
                case "*": return operand1 * operand2;
                case "/": return operand1 / operand2;
                default: return null;
            }
        }
    }

    public class BasicCalcViewModel : BindableObject
    {
        public static readonly BindableProperty CalculatorProperty = BindableProperty.Create(nameof(Calculator), typeof(ICalculator), typeof(BasicCalcViewModel), new BasicCalculator());

        public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(BasicCalcViewModel), "0");

        public ICalculator Calculator
        {
            get => (ICalculator)GetValue(CalculatorProperty);
            set => SetValue(CalculatorProperty, value);
        }

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public ICommand DigitCommand { get; protected set; }
        public ICommand OperationCommand { get; protected set; }
        public ICommand BackspaceCommand { get; protected set; }
        public ICommand EnterCommand { get; protected set; }
        public ICommand ClearCommand { get; protected set; }
        public ICommand PlusMinusCommand { get; protected set; }

        private readonly char NEGATIVE = '-';

        private string Answer;
        private char Operation;
        private string Operand;

        public BasicCalcViewModel()
        {
            DigitCommand = new Command<string>(value => Digit(value[0]));
            OperationCommand = new Command<string>(value => Operate(value[0]));
            BackspaceCommand = new Command(Backspace);
            EnterCommand = new Command(() => Operate('='));
            ClearCommand = new Command(Clear);
            PlusMinusCommand = new Command(Negate);

            Clear();
        }

        public void Backspace()
        {
            Operand = OperandEquals("", Operand.Length - 1) ? "0" : Operand.Substring(0, Math.Max(0, Operand.Length - 1));
            UpdateText();
        }

        public void Clear()
        {
            Answer = string.Empty;
            Operation = default;
            Operand = string.Empty;

            Digit('0');
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

        public void Digit(char keystroke)
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

            UpdateText();
        }

        public void Operate(char operation)
        {
            if (Operand.Length > 0)
            {
                if (Operation == '=')
                {
                    string operand = Operand;
                    Clear();
                    Operand = operand;
                    //Input(operand);
                }

                Answer = Operation == default ? Operand : (Calculator ?? (ICalculator)CalculatorProperty.DefaultValue).Compute(Answer, Operation.ToString(), Operand);
                Operand = "";
            }

            Operation = operation;

            UpdateText();
        }

        private void UpdateText() => Text = Operand.Length > 0 ? Operand : (Answer ?? "Error");

        private bool OperandEquals(string strB, int? length = null)
        {
            int start = (Operand.Length > 0 && Operand[0] == NEGATIVE).ToInt();
            return (length ?? Operand.Length) - start == strB.Length && string.Compare(Operand, start, strB, 0, strB.Length) == 0;
        }
    }
}
