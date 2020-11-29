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
        public enum CursorKey { Left, Right, Up, Down, End, Home };

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
                MainPage page = SoftKeyboard.Cursor.Parent<MainPage>();
                if (page == null)
                {
                    App.Current.Home.AddCalculation();
                }

                SoftKeyboard.Type(value);
            });

            BackspaceCommand = new Command(() =>
            {
                SoftKeyboard.Delete();
            });

            MoveCursorCommand = new Command<CursorKey>(value =>
            {
                if (value == CursorKey.Up)
                {
                    if (!SoftKeyboard.Up())
                    {
                        SoftKeyboard.Cursor.Parent<Calculation>()?.Up();
                    }
                }
                else if (value == CursorKey.Down)
                {
                    if (!SoftKeyboard.Down())
                    {
                        SoftKeyboard.Cursor.Parent<Calculation>()?.Down();
                    }
                }
                else if (value == CursorKey.Right)
                {
                    SoftKeyboard.Right();
                    //MathField.CursorPosition++;
                }
                else if (value == CursorKey.Left)
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


    public class CrunchCalculator : BasicCalcViewModel, ICalculator
    {
        public MainPage Interface { get; set; }
        public ICommand NewCommand { get; protected set; }

        private readonly ICalculator ScientificCalculator = new ScientificCalculator();

        private MathFieldViewModel MathField => Interface?.FocusedMathField.BindingContext as MathFieldViewModel;
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
                MathField?.MoveCursorCommand?.Execute(MathFieldViewModel.CursorKey.Down);
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
