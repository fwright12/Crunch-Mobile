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
    public class MathFieldViewModel : BindableObject
    {
        public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(MathFieldViewModel), string.Empty, propertyChanged: TextPropertyChanged);

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public ObservableCollection<char> Children => MathField.Children;

        //public static readonly BindableProperty MathFieldProperty = BindableProperty.Create(nameof(MathField), typeof(MathField), typeof(MathFieldViewModel));

        public MathField MathField { get; private set; }
        /*{
            get => (MathField)GetValue(MathFieldProperty);
            set => SetValue(MathFieldProperty, value);
        }*/

        public ICommand InputCommand { get; set; }
        public ICommand BackspaceCommand { get; set; }
        public ICommand MoveCursorCommand { get; set; }

        public MathFieldViewModel()
        {
            MathField = new MathField();

            InputCommand = new Command<string>(value =>
            {
                SoftKeyboard.Type(value);
            });

            BackspaceCommand = new Command(() =>
            {
                SoftKeyboard.Delete();
            });

            MoveCursorCommand = new Command<KeyboardManager.CursorKey>(value =>
            {
                if (value == KeyboardManager.CursorKey.Up)
                {
                    if (!SoftKeyboard.Up())
                    {
                        SoftKeyboard.Cursor.Parent<Calculation>()?.Up();
                    }
                }
                else if (value == KeyboardManager.CursorKey.Down)
                {
                    if (!SoftKeyboard.Down())
                    {
                        SoftKeyboard.Cursor.Parent<Calculation>()?.Down();
                    }
                }
                else if (value == KeyboardManager.CursorKey.Right)
                {
                    SoftKeyboard.Right();
                    //MathField.CursorPosition++;
                }
                else if (value == KeyboardManager.CursorKey.Left)
                {
                    SoftKeyboard.Left();
                    //MathField.CursorPosition--;
                }
            });
        }

        private static void TextPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            MathFieldViewModel model = (MathFieldViewModel)bindable;
            string old = (string)oldValue;
            string value = (string)newValue;

            for (int i = 0; i < value.Length; i++)
            {
                if (old[i] == value[i])
                {
                    continue;
                }


            }
        }

        // Below taken from https://www.geeksforgeeks.org/edit-distance-dp-5/
        static int min(int x, int y, int z)
        {
            if (x <= y && x <= z)
                return x;
            if (y <= x && y <= z)
                return y;
            else
                return z;
        }

        static int editDist(string str1, string str2, int m, int n)
        {
            // If first string is empty, the only option is to 
            // insert all characters of second string into first 
            if (m == 0)
                return n;

            // If second string is empty, the only option is to 
            // remove all characters of first string 
            if (n == 0)
                return m;

            // If last characters of two strings are same, nothing 
            // much to do. Ignore last characters and get count for 
            // remaining strings. 
            if (str1[m - 1] == str2[n - 1])
                return editDist(str1, str2, m - 1, n - 1);

            // If last characters are not same, consider all three 
            // operations on last character of first string, recursively 
            // compute minimum cost for all three operations and take 
            // minimum of three values. 
            return 1 + min(editDist(str1, str2, m, n - 1), // Insert 
                           editDist(str1, str2, m - 1, n), // Remove 
                           editDist(str1, str2, m - 1, n - 1) // Replace 
                           );
        }

        /*private static void MathFieldPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            MathFieldViewModel viewModel = (MathFieldViewModel)bindable;

            if (oldValue is MathField oldMathField)
            {
                oldMathField.CursorMoved -= viewModel.CursorMoved;
                oldMathField.Children.CollectionChanged -= viewModel.ChildrenChanged;
            }

            if (newValue is MathField newMathField)
            {
                newMathField.CursorMoved += viewModel.CursorMoved;
                newMathField.Children.CollectionChanged += viewModel.ChildrenChanged;
            }
        }*/
    }

    public class StringToListConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /*public class KeyboardViewModel : BindableObject
    {
        public static readonly BindableProperty MathFieldProperty = BindableProperty.Create(nameof(MathField), typeof(MathFieldViewModel), typeof(CrunchCalculator));

        public MathFieldViewModel MathField
        {
            get => (MathFieldViewModel)GetValue(MathFieldProperty);
            set => SetValue(MathFieldProperty, value);
        }

        public ICommand InputCommand { get; set; }
        public ICommand BackspaceCommand { get; set; }
        public ICommand MoveCursorCommand { get; set; }

        public KeyboardViewModel()
        {
            InputCommand = new Command(value => MathField?.InputCommand?.Execute(value));
            BackspaceCommand = new Command(value => MathField?.BackspaceCommand?.Execute(value));
            MoveCursorCommand = new Command(value => MathField?.MoveCursorCommand?.Execute(value));
        }
    }*/
    
    public class KeyboardContext<T>
    {
        public T Focused { get; set; }
    }

    public class CrunchCalculator : ICalculator
    {
        private readonly ICalculator ScientificCalculator = new ScientificCalculator();
        private string LastOperation;

        public KeyboardContext<MathFieldViewModel> Keyboard { get; set; }

        public string Compute(string operand1, string operation, string operand2)
        {
            if (Keyboard == null)
            {
                return ScientificCalculator.Compute(operand1, operation, operand2);
            }

            Equation parentEquation = SoftKeyboard.Cursor.Parent<Equation>();
            Answer answer = parentEquation?.RHS as Answer;
            Operand op1 = Crunch.Math.Evaluate(operand1);
            bool negatedAnswer = answer?.RawAnswer == null || operand1 == null ? false : op1.IsNegative ^ answer.RawAnswer.IsNegative;

            //if (LastOperation == default)
            if (!negatedAnswer && answer?.RawAnswer?.Equals(op1) != true)
            {
                Keyboard.Focused?.MoveCursorCommand?.Execute(KeyboardManager.CursorKey.Down);

                if (SoftKeyboard.Cursor.Parent<Equation>().LHS is Expression current)
                {
                    //current.Children.Clear();
                    SoftKeyboard.MoveCursor(current);
                    while (current.Children.Count > 1)
                    {
                        current.Children.RemoveAt(1);
                    }
                }

                //LastOperation = default;

                Input(operand1);
            }
            else if (negatedAnswer || ((LastOperation == "+" || LastOperation == "-") && (operation == "*" || operation == "/")))
            {
                Expression current = SoftKeyboard.Cursor.Parent<Equation>().LHS;
                SoftKeyboard.MoveCursor(current);
                if (negatedAnswer)
                {
                    Input("-");
                }
                Input("(");
                SoftKeyboard.MoveCursor(current, current.Children.Count - 1);
                Input(")");
            }

            Input(operation);
            Input(operand2);
            parentEquation.LHS.End();

            LastOperation = operation;

            return (answer == null ? Crunch.Math.Evaluate("0") : answer.RawAnswer)?.Format(Polynomials.Expanded, Numbers.Decimal, Trigonometry.Degrees)?.ToString();
        }

        private void Input(string input) => Keyboard.Focused?.InputCommand?.Execute(input);
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
