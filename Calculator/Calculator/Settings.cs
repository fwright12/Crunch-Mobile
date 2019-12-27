using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

using System.Extensions;
using Xamarin.Forms;
using Crunch;

namespace Calculator
{
    using Tip = Tuple<string, string, string, TargetIdiom>;

    public static class Extensions
    {
        public static T TryGet<T>(this IDictionary<string, object> dict, string key, T defaultValue)
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

        public static bool ShouldRunTutorial
        {
            get
            {
                bool keyExists = Storage.ContainsKey(TUTORIAL);
                Storage[TUTORIAL] = null;
                return !keyExists;
            }
        }
        public static bool ClearCanvasWarning = true;
        public static bool LeftHanded = false;
        public static List<char> Variables = new List<char>(51);
        public static bool VariableRowExpanded = false;
        public static Point KeyboardPosition = new Point(1, 1);
        public static bool ShouldShowTips = false;
        public static Dictionary<string, string> Tips = new Dictionary<string, string>();

        public static event StaticEventHandler<ToggledEventArgs> KeyboardChanged;

        public static bool ShouldShowFullKeyboard
        {
            get => ShowFullKeyboard;
            set
            {
                ShowFullKeyboard = value;
                KeyboardChanged?.Invoke(new ToggledEventArgs(value));
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
        private static readonly string SHOW_TIPS = "should show tips";
        private static readonly Tip[] TIPS = new Tip[]
        {
            new Tip("links", "You can tap a link to see the equation it came from", "https://static.wixstatic.com/media/4a4e50_d9c8bdf752ca42c5abec19413cb4fc3b~mv2.gif", TargetIdiom.Phone | TargetIdiom.Tablet),
            new Tip("assign variables", "You can assign values to unknown variables", "https://static.wixstatic.com/media/4a4e50_b430cf0e45904a119e932b482b0bc7db~mv2.gif", TargetIdiom.Phone | TargetIdiom.Tablet),
            /*new Tip("cursor mode", "If you long press the keyboard, you can drag the cursor", "https://static.wixstatic.com/media/4a4e50_9c931692184c46e29c82dd2d64ff3c14~mv2.gif", TargetIdiom.Phone | TargetIdiom.Tablet),
            new Tip("move keyboard", "You can drag the keyboard by the \u25BD key to move the keyboard around the screen", "https://static.wixstatic.com/media/4a4e50_4f4048250d6e4d6896082ad53c57c384~mv2.gif", TargetIdiom.Tablet),
            new Tip("default answers", "You can choose the default format for answers in settings", "https://static.wixstatic.com/media/4a4e50_afe479ab52874b8aabf1403c7ec3f2ff~mv2.gif", TargetIdiom.Phone | TargetIdiom.Tablet),
            new Tip("clear canvas", "You can clear the canvas by long pressing the DEL key", "https://static.wixstatic.com/media/4a4e50_ab41263fc30a4a9bb3949bdcb4179a3b~mv2.gif", TargetIdiom.Phone | TargetIdiom.Tablet)*/
        };

        public static Setting<bool> ShowTips = new Setting<bool>("should show tips", false);

        public abstract class Setting
        {
            private static Dictionary<string, Setting> AllInstances = new Dictionary<string, Setting>();

            private readonly string Identifier;

            public Setting(string identifier)
            {
                Identifier = identifier;

                try
                {
                    AllInstances.Add(Identifier, this);
                }
                catch (ArgumentException ae)
                {
                    throw new ArgumentException("A Setting with identifier " + Identifier + " already exists; Setting identifiers must be unique", ae);
                }
            }

            public static void LoadFrom(IDictionary<string, object> storage)
            {
                foreach (KeyValuePair<string, object> kvp in storage)
                {
                    Setting setting;
                    if (AllInstances.TryGetValue(kvp.Key, out setting))
                    {
                        try
                        {
                            ((dynamic)setting).Value = (dynamic)kvp.Value;
                        }
                        catch
                        {
                            throw new Exception("Failed to set Setting value from storage (possible type mismatch?). Setting is type " + setting.GetType() + " and storage value is type " + kvp.Value.GetType());
                        }
                    }
                }
            }

            public static void SaveTo(IDictionary<string, object> storage)
            {
                foreach (KeyValuePair<string, Setting> kvp in AllInstances)
                {
                    //storage[kvp.Key] = kvp.Value.GetValue();
                }
            }

            public abstract object GetValue();
        }

        public class Setting<T> : Setting, INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            public readonly BindableProperty ValueProperty;

            public T Value
            {
                get => (T)(_Value ?? ValueProperty.DefaultValue);
                set
                {
                    if ((dynamic)_Value != value)
                    {
                        _Value = value;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(ValueProperty.PropertyName));
                    }
                }
            }

            private object _Value;

            public Setting(string identifier, T defaultValue = default, BindingMode defaultBindingMode = BindingMode.TwoWay, BindableProperty.ValidateValueDelegate validateValue = null, BindableProperty.BindingPropertyChangedDelegate propertyChanged = null, BindableProperty.BindingPropertyChangingDelegate propertyChanging = null, BindableProperty.CoerceValueDelegate coerceValue = null, BindableProperty.CreateDefaultValueDelegate defaultValueCreator = null) : base(identifier)
            {
                ValueProperty = BindableProperty.Create("Value", typeof(T), typeof(Setting<T>), defaultValue, defaultBindingMode, validateValue, propertyChanged, propertyChanging, coerceValue, defaultValueCreator);
            }

            public override object GetValue() => Value;
        }

        /*{
            public event PropertyChangedEventHandler PropertyChanged;

            public readonly BindableProperty ValueProperty;

            private readonly string Identifier;

            public object Value
            {
                get => _Value ?? ValueProperty.DefaultValue; // (T)base.Value; // Stored.TryGet(Identifier, ValueProperty.DefaultValue);
                set
                {
                    //object temp;
                    //if (!Stored.TryGetValue(Identifier, out temp) || (dynamic)temp != value)
                    if ((dynamic)_Value != value)
                    {
                        _Value = value;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(ValueProperty.PropertyName));
                    }
                }
            }

            private object _Value;

            public Setting(string identifier, Type returnType, Type declaringType, object defaultValue = null, BindingMode defaultBindingMode = BindingMode.OneWay, BindableProperty.ValidateValueDelegate validateValue = null, BindableProperty.BindingPropertyChangedDelegate propertyChanged = null, BindableProperty.BindingPropertyChangingDelegate propertyChanging = null, BindableProperty.CoerceValueDelegate coerceValue = null, BindableProperty.CreateDefaultValueDelegate defaultValueCreator = null)
            {
                ValueProperty = BindableProperty.Create("Value", returnType, declaringType, defaultValue, defaultBindingMode, validateValue, propertyChanged, propertyChanging, coerceValue, defaultValueCreator);
                Identifier = identifier;

                object o = null;
                Print.Log("here", (dynamic)o);

                try
                {
                    Stored.Add(Identifier, this);
                }
                catch (ArgumentException ae)
                {
                    throw new ArgumentException("Setting identifiers must be unique", ae);
                }
            }
        }

        public class Setting<T> : Setting
        {
            new public T Value
            {
                get => (T)base.Value;
                set => base.Value = value;
            }

            public Setting(string identifier, T defaultValue = default, BindingMode defaultBindingMode = BindingMode.OneWay, BindableProperty.ValidateValueDelegate validateValue = null, BindableProperty.BindingPropertyChangedDelegate propertyChanged = null, BindableProperty.BindingPropertyChangingDelegate propertyChanging = null, BindableProperty.CoerceValueDelegate coerceValue = null, BindableProperty.CreateDefaultValueDelegate defaultValueCreator = null) : base(identifier, typeof(T), typeof(Setting<T>), defaultValue, defaultBindingMode, validateValue, propertyChanged, propertyChanging, coerceValue, defaultValueCreator) { }
        }*/

        public static void Load()
        {
            Setting.LoadFrom(Storage);

            DecimalPlaces = Storage.TryGet(DECIMAL_PLACES, 3);
            LogarithmBase = Storage.TryGet(LOG_BASE, 10.0);
            
            Numbers = Storage.TryGet(NUMBER_FORM, Numbers.Decimal);
            Trigonometry = Storage.TryGet(TRIG_FORM, Trigonometry.Degrees);

            ClearCanvasWarning = Storage.TryGet(CLEAR_CANVAS_WARNING, true);
            //Tutorial = !Storage.ContainsKey(TUTORIAL);
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

            ShouldShowTips = Storage.TryGet(SHOW_TIPS, true);
            foreach(Tip tip in TIPS)
            {
                // false means the tip hasn't been shown yet
                if (tip.Item4.HasFlag(Device.Idiom))// && !Storage.TryGet(tip.Item1, false))
                {
                    Tips.Add(tip.Item2, tip.Item3);
                }
            }
            Print.Log(Settings.ShouldShowTips, Settings.Tips.Count);
        }

        public static async void Save()
        {
            //Storage.Clear();

            Storage[DECIMAL_PLACES] = DecimalPlaces;
            Storage[LOG_BASE] = LogarithmBase;
            Storage[NUMBER_FORM] = (int)Numbers;
            Storage[TRIG_FORM] = (int)Trigonometry;
            Storage[CLEAR_CANVAS_WARNING] = ClearCanvasWarning;
            Storage[KEYBOARD_FULL] = ShouldShowFullKeyboard;
            Storage[KEYBOARD_POSITION] = KeyboardPosition.X.ToString() + "|" + KeyboardPosition.Y.ToString();

            string varStr = VariableRowExpanded ? "1" : "0";
            foreach(char c in Variables)
            {
                varStr += c;
            }
            Storage[VARIABLES] = varStr;

            Storage[SHOW_TIPS] = ShouldShowTips;
            foreach(Tip tip in TIPS)
            {
                Storage[tip.Item1] = !Tips.ContainsKey(tip.Item2);
            }

            Setting.SaveTo(Storage);

            await Application.Current.SavePropertiesAsync();
        }

        public static void ResetToDefault()
        {
            Storage.Clear();
            Load();
            //Tutorial = false;
        }
    }
}