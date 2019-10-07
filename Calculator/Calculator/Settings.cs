using System;
using System.Collections.Generic;
using System.Text;

using System.Extensions;
using Xamarin.Forms;
using Crunch;

namespace Calculator
{
    public static class Settings
    {
        public static IDictionary<string, object> Storage = Application.Current.Properties;

        public static int DecimalPlaces
        {
            get { return Crunch.Math.DecimalPlaces; }
            set { Crunch.Math.DecimalPlaces = value; }
        }
        public static double LogarithmBase
        {
            get { return Crunch.Math.ImplicitLogarithmBase; }
            set { Crunch.Math.ImplicitLogarithmBase = value; }
        }

        public static Polynomials Polynomials;
        public static Numbers Numbers = Numbers.Decimal;
        public static Trigonometry Trigonometry = Trigonometry.Degrees;

        public static bool Tutorial = true;
        public static bool ClearCanvasWarning = true;
        public static bool LeftHanded = false;
        public static List<char> Variables = new List<char>(51);
        public static bool VariableRowExpanded = false;
        public static Point KeyboardPosition = new Point(1, 1);

        public static event StaticEventHandler<ToggledEventArgs> KeyboardChanged;
        public static bool ShouldShowFullKeyboard
        {
            get => ShowFullKeyboard;
            set
            {
                KeyboardChanged?.Invoke(new ToggledEventArgs(value));
                ShowFullKeyboard = value;
            }
        }
        private static bool ShowFullKeyboard = true;

        private static readonly string DECIMAL_PLACES = "decimal places";
        private static readonly string LOG_BASE = "implicit logarithm base";
        private static readonly string NUMBER_FORM = "number form";
        private static readonly string TRIG_FORM = "trig form";
        private static readonly string CLEAR_CANVAS_WARNING = "clear canvas warning";
        private static readonly string TUTORIAL = "tutorial1";
        private static readonly string VARIABLES = "variables";
        private static readonly string KEYBOARD_FULL = "show full keyboard";
        private static readonly string KEYBOARD_POSITION = "keyboard position";

        public static void Load()
        {
            DecimalPlaces = Storage.TryGet(DECIMAL_PLACES, 3);
            LogarithmBase = Storage.TryGet(LOG_BASE, 10.0);

            Numbers = Storage.TryGet(NUMBER_FORM, Numbers.Decimal);
            Trigonometry = Storage.TryGet(TRIG_FORM, Trigonometry.Degrees);

            ClearCanvasWarning = Storage.TryGet(CLEAR_CANVAS_WARNING, true);
            Tutorial = Storage.TryGet(TUTORIAL, true);
            ShouldShowFullKeyboard = Storage.TryGet(KEYBOARD_FULL, true);
            
            if (Storage.ContainsKey(KEYBOARD_POSITION))
            {
                string position = (string)Storage[KEYBOARD_POSITION];

                int separator = position.IndexOf("|");
                //Print.Log("parsing keyboard position", position, position.Substring(0, separator), position.Substring(separator + 1));
                double x = double.Parse(position.Substring(0, separator));
                double y = double.Parse(position.Substring(separator + 1));

                KeyboardPosition = new Point(x, y);
            }
            else
            {
                KeyboardPosition = new Point(1, 1);
            }

            if (Storage.ContainsKey(VARIABLES))
            {
                string varStr = (string)Storage[VARIABLES];
                VariableRowExpanded = varStr[0] == '1';

                //foreach(char c in (string)Storage[VARIABLES])
                for (int i = 1; i < varStr.Length; i++)
                {
                    Variables.Add(varStr[i]);
                }
            }
            else
            {
                // Lowercase english letters
                for (int i = 0; i < 26; i++)
                {
                    Variables.Add((char)(i + 97));
                }
                // Lowercase greek letters
                for (int i = 0; i < 25; i++)
                {
                    Variables.Add((char)(i + 945));
                }
            }
        }

        public static async void Save()
        {
            Storage[DECIMAL_PLACES] = DecimalPlaces;
            Storage[LOG_BASE] = LogarithmBase;
            Storage[NUMBER_FORM] = (int)Numbers;
            Storage[TRIG_FORM] = (int)Trigonometry;
            Storage[CLEAR_CANVAS_WARNING] = ClearCanvasWarning;
            Storage[TUTORIAL] = false;
            Storage[KEYBOARD_FULL] = ShouldShowFullKeyboard;
            Storage[KEYBOARD_POSITION] = KeyboardPosition.X.ToString() + "|" + KeyboardPosition.Y.ToString();

            string varStr = VariableRowExpanded ? "1" : "0";
            foreach(char c in Variables)
            {
                varStr += c;
            }
            Storage[VARIABLES] = varStr;

            await Application.Current.SavePropertiesAsync();
        }

        public static void ResetToDefault()
        {
            Storage.Clear();
            Load();
            Tutorial = false;
        }

        private static T TryGet<T>(this IDictionary<string, object> dict, string key, T defaultValue)
        {
            try
            {
                return (T)dict[key];
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}