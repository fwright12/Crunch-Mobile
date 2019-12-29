using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

using System.Extensions;
using Xamarin.Forms;
using Crunch;

namespace Calculator
{
    using Tip = Tuple<Settings.BindableSetting<bool>, string, string, TargetIdiom>;

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

        /*public static int DecimalPlaces
        {
            get { return Crunch.Math.DecimalPlaces; }
            set { Crunch.Math.DecimalPlaces = value; }
        }
        public static double LogarithmBase
        {
            get { return Crunch.Math.ImplicitLogarithmBase; }
            set { Crunch.Math.ImplicitLogarithmBase = value; }
        }*/

        public static Polynomials Polynomials;
        //public static Numbers Numbers = Numbers.Decimal;
        //public static Trigonometry Trigonometry = Trigonometry.Degrees;

        /*public static bool ShouldRunTutorial
        {
            get
            {
                bool keyExists = Storage.ContainsKey(TUTORIAL);
                Print.Log("key exists", keyExists, Storage.Count);
                Storage[TUTORIAL] = null;
                return !keyExists;
            }
        }*/
        //public static bool ClearCanvasWarning = true;
        public static bool LeftHanded = false;
        public static List<char> Variables = DefaultVariables();
        public static List<char> DefaultVariables()
        {
            List<char> result = new List<char>();

            // Lowercase english letters
            for (int i = 0; i < 26; i++)
            {
                result.Add((char)(i + 97));
            }
            // Lowercase greek letters
            for (int i = 0; i < 25; i++)
            {
                result.Add((char)(i + 945));
            }

            return result;
        }
        //public static bool VariableRowExpanded = false;
        //public static Point KeyboardPosition = new Point(1, 1);
        //public static bool ShouldShowTips = false;
        //public static Dictionary<string, string> Tips = new Dictionary<string, string>();

        //public static event StaticEventHandler<ToggledEventArgs> KeyboardChanged;

        /*public static bool ShouldShowFullKeyboard
        {
            get => ShowFullKeyboard;
            set
            {
                ShowFullKeyboard = value;
                KeyboardChanged?.Invoke(new ToggledEventArgs(value));
            }
        }
        private static bool ShowFullKeyboard = true;*/

        public static BindableSetting<int> DecimalPlaces = new BindableSetting<int>("decimal places", 3, propertyChanged: (bindable, oldValue, newValue) => Crunch.Math.DecimalPlaces = (int)newValue);
        public static BindableSetting<double> LogarithmBase = new BindableSetting<double>("implicit logarithm base", 10, propertyChanged: (bindable, oldValue, newValue) => Crunch.Math.ImplicitLogarithmBase = (double)newValue);

        public static BindableSetting<Numbers> Numbers = new BindableSetting<Numbers>("number form", Crunch.Numbers.Decimal, serializer: value => (int)value);
        public static BindableSetting<Trigonometry> Trigonometry = new BindableSetting<Trigonometry>("trig form", Crunch.Trigonometry.Degrees, serializer: value => (int)value);

        public static BindableSetting<bool> ClearCanvasWarning = new BindableSetting<bool>("clear canvas warning", true);
        // Needs the deserializer because v2.3.2 set the value for key "tutorial1" to null, which doesn't cast to bool. Just means the tutorial was already shown, so value should be false. Can probably be removed in a couple versions.
        public static BindableSetting<bool> ShouldRunTutorial = new BindableSetting<bool>("tutorial1", true, deserializer: value => false);

        public static BindableSetting<bool> VariableRowExpanded = new BindableSetting<bool>("variables", false, deserializer: value =>
        {
            string varStr = (string)value;

            Variables = new List<char>();
            for (int i = 1; i < varStr.Length; i++)
            {
                Variables.Add(varStr[i]);
            }

            return varStr[0] == '1';
        }, serializer: value =>
        {
            string varStr = value ? "1" : "0";
            foreach (char c in Variables)
            {
                varStr += c;
            }
            return varStr;
        });
        public static BindableSetting<bool> ShowFullKeyboard = new BindableSetting<bool>("show full keyboard", true);
        public static BindableSetting<Point> KeyboardPosition = new BindableSetting<Point>("keyboard position", new Point(1, 1), deserializer: value =>
        {
            string position = (string)value;

            int separator = position.IndexOf("|");
            //Print.Log("parsing keyboard position", position, position.Substring(0, separator), position.Substring(separator + 1));
            double x = double.Parse(position.Substring(0, separator));
            double y = double.Parse(position.Substring(separator + 1));

            return new Point(x, y);
        }, serializer: value => value.X.ToString() + "|" + value.Y.ToString());

        public static BindableSetting<bool> ShowTips = new BindableSetting<bool>("should show tips", true);
        
        public static Tip[] Tips = new Tip[]
        {
            new Tip("tap links tip", "You can tap a link to see the equation it came from", "https://static.wixstatic.com/media/4a4e50_d9c8bdf752ca42c5abec19413cb4fc3b~mv2.gif", TargetIdiom.Phone | TargetIdiom.Tablet),
            new Tip("assign variables tip", "You can assign values to unknown variables", "https://static.wixstatic.com/media/4a4e50_b430cf0e45904a119e932b482b0bc7db~mv2.gif", TargetIdiom.Phone | TargetIdiom.Tablet),
            /*new Tip("cursor mode tip", "If you long press the keyboard, you can drag the cursor", "https://static.wixstatic.com/media/4a4e50_9c931692184c46e29c82dd2d64ff3c14~mv2.gif", TargetIdiom.Phone | TargetIdiom.Tablet),
            new Tip("move keyboard tip", "You can drag the keyboard by the \u25BD key to move the keyboard around the screen", "https://static.wixstatic.com/media/4a4e50_4f4048250d6e4d6896082ad53c57c384~mv2.gif", TargetIdiom.Tablet),
            new Tip("default answers tip", "You can choose the default format for answers in settings", "https://static.wixstatic.com/media/4a4e50_afe479ab52874b8aabf1403c7ec3f2ff~mv2.gif", TargetIdiom.Phone | TargetIdiom.Tablet),
            new Tip("clear canvas tip", "You can clear the canvas by long pressing the DEL key", "https://static.wixstatic.com/media/4a4e50_ab41263fc30a4a9bb3949bdcb4179a3b~mv2.gif", TargetIdiom.Phone | TargetIdiom.Tablet)*/
        };

        //private static readonly string DECIMAL_PLACES = "decimal places";
        //private static readonly string LOG_BASE = "implicit logarithm base";
        //private static readonly string NUMBER_FORM = "number form";
        //private static readonly string TRIG_FORM = "trig form";
        //private static readonly string CLEAR_CANVAS_WARNING = "clear canvas warning";
        //private static readonly string TUTORIAL = "tutorial1";
        //private static readonly string VARIABLES = "variables";
        //private static readonly string KEYBOARD_FULL = "show full keyboard";
        //private static readonly string KEYBOARD_POSITION = "keyboard position";
        //private static readonly string SHOW_TIPS = "should show tips";
        /*private static readonly Tip[] TIPS = new Tip[]
        {
            new Tip("links", "You can tap a link to see the equation it came from", "https://static.wixstatic.com/media/4a4e50_d9c8bdf752ca42c5abec19413cb4fc3b~mv2.gif", TargetIdiom.Phone | TargetIdiom.Tablet),
            new Tip("assign variables", "You can assign values to unknown variables", "https://static.wixstatic.com/media/4a4e50_b430cf0e45904a119e932b482b0bc7db~mv2.gif", TargetIdiom.Phone | TargetIdiom.Tablet),

        };*/

        public abstract class Setting : BindableObject
        {
            private static Dictionary<string, Setting> AllInstances = new Dictionary<string, Setting>();

            protected abstract object SerializedValue { get; }

            public readonly BindableProperty ValueProperty;

            private readonly string Identifier;

            public Setting(string identifier, Type returnType, Type declaringType, object defaultValue = null, BindingMode defaultBindingMode = BindingMode.OneWay, BindableProperty.ValidateValueDelegate validateValue = null, BindableProperty.BindingPropertyChangedDelegate propertyChanged = null, BindableProperty.BindingPropertyChangingDelegate propertyChanging = null, BindableProperty.CoerceValueDelegate coerceValue = null, BindableProperty.CreateDefaultValueDelegate defaultValueCreator = null)
            {
                ValueProperty = BindableProperty.Create("Value", returnType, declaringType, defaultValue, defaultBindingMode, validateValue, propertyChanged, propertyChanging, coerceValue, defaultValueCreator);
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

            public static bool LoadFrom(IDictionary<string, object> storage)
            {
                bool successfullyLoaded = true;

                foreach (KeyValuePair<string, object> kvp in storage)
                {
                    Setting setting;
                    if (AllInstances.TryGetValue(kvp.Key, out setting))
                    {
                        object value = setting.Deserialize(kvp.Value);
                        setting.SetValue(setting.ValueProperty, value);
#if DEBUG
                        if (value.GetType() != setting.ValueProperty.ReturnType)
                        {
                            Print.Log("Failed to set Setting value from storage (possible type mismatch?). Setting looking for type " + setting.ValueProperty.ReturnType + " and storage value is type " + value.GetType());
                            successfullyLoaded = false;

                            throw new Exception();
                        }
#endif
                        /*try
                        {
                            //((dynamic)setting).Value = (dynamic)setting.Deserialize(kvp.Value);// setting.Deserialize == null ? (dynamic)kvp.Value : setting.Deserialize;
                        }
                        catch
                        {
                            Print.Log("Failed to set Setting value from storage (possible type mismatch?). Setting is type " + setting.GetType() + " and storage value is type " + kvp.Value.GetType());
                            successfullyLoaded = false;
#if DEBUG
                            throw new Exception();
#endif
                        }*/
                    }
                }

                return successfullyLoaded;
            }

            public static void SaveTo(IDictionary<string, object> storage)
            {
                foreach (KeyValuePair<string, Setting> kvp in AllInstances)
                {
                    storage[kvp.Key] = kvp.Value.SerializedValue;
                }
            }

            public static Dictionary<string, object> Save()
            {
                Dictionary<string, object> result = new Dictionary<string, object>();
                SaveTo(result);
                return result;
            }

            public virtual object Deserialize(object value) => value;
        }

        public class BindableSetting<T> : Setting
        {
            public delegate T DeserializeDelegate(object value);
            public delegate object SerializeDelegate(T value);

            private readonly DeserializeDelegate Deserializer;
            private readonly SerializeDelegate Serializer;

            public T Value
            {
                get => (T)GetValue(ValueProperty);
                set => SetValue(ValueProperty, value);
            }

            /*public T Value
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

            private object _Value;*/

            protected override object SerializedValue => Serializer != null ? Serializer(Value) : Value;

            public BindableSetting(string identifier, T defaultValue = default, BindingMode defaultBindingMode = BindingMode.TwoWay, DeserializeDelegate deserializer = null, SerializeDelegate serializer = null, BindableProperty.ValidateValueDelegate validateValue = null, BindableProperty.BindingPropertyChangedDelegate propertyChanged = null, BindableProperty.BindingPropertyChangingDelegate propertyChanging = null, BindableProperty.CoerceValueDelegate coerceValue = null, BindableProperty.CreateDefaultValueDelegate defaultValueCreator = null) : base(identifier, typeof(T), typeof(BindableSetting<T>), defaultValue, defaultBindingMode, validateValue, propertyChanged, propertyChanging, coerceValue, defaultValueCreator)
            {
                //ValueProperty = BindableProperty.Create("Value", typeof(T), typeof(BindableSetting<T>), defaultValue, defaultBindingMode, validateValue, propertyChanged, propertyChanging, coerceValue, defaultValueCreator);

                Deserializer = deserializer;
                Serializer = serializer;
            }

            public static implicit operator BindableSetting<T>(string identifier) => new BindableSetting<T>(identifier);

            public override object Deserialize(object value) => Deserializer != null ? Deserializer(value) : (T)value;
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

            //DecimalPlaces = Storage.TryGet(DECIMAL_PLACES, 3);
            //LogarithmBase = Storage.TryGet(LOG_BASE, 10.0);
            
            //Numbers = Storage.TryGet(NUMBER_FORM, Numbers.Decimal);
            //Trigonometry = Storage.TryGet(TRIG_FORM, Trigonometry.Degrees);

            //ClearCanvasWarning = Storage.TryGet(CLEAR_CANVAS_WARNING, true);
            //Tutorial = !Storage.ContainsKey(TUTORIAL);
            //ShouldShowFullKeyboard = Storage.TryGet(KEYBOARD_FULL, true);

            //ShouldShowTips = Storage.TryGet(SHOW_TIPS, true);
            /*foreach(Tip tip in TIPS)
            {
                // false means the tip hasn't been shown yet
                if (tip.Item4.HasFlag(Device.Idiom))// && !Storage.TryGet(tip.Item1, false))
                {
                    Tips.Add(tip.Item2, tip.Item3);
                }
            }*/
        }

        public static async void Save()
        {
            Storage.Clear();

            //Storage[DECIMAL_PLACES] = DecimalPlaces;
            //Storage[LOG_BASE] = LogarithmBase;
            //Storage[NUMBER_FORM] = (int)Numbers;
            //Storage[TRIG_FORM] = (int)Trigonometry;
            //Storage[CLEAR_CANVAS_WARNING] = ClearCanvasWarning;
            //Storage[KEYBOARD_FULL] = ShouldShowFullKeyboard;

            //Storage[SHOW_TIPS] = ShouldShowTips;
            /*foreach(Tip tip in TIPS)
            {
                Storage[tip.Item1] = !Tips.ContainsKey(tip.Item2);
            }*/

            //Storage[TUTORIAL] = null;

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