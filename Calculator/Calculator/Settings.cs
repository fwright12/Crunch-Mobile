using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

using System.Extensions;
using Xamarin.Forms;
using Crunch;

namespace Calculator
{
    using Tip = Tuple<BindableValue<bool>, string, string, TargetIdiom>;

    public partial class App
    {
        public static readonly BindableValue<int> DecimalPlaces = Settings.Register("decimal places", 3, propertyChanged: (bindable, oldValue, newValue) => Crunch.Math.DecimalPlaces = (int)newValue);
        public static readonly BindableValue<double> LogarithmBase = Settings.Register<double>("implicit logarithm base", 10, propertyChanged: (bindable, oldValue, newValue) => Crunch.Math.ImplicitLogarithmBase = (double)newValue);

        public static Polynomials Polynomials;
        // Serialize enum types as ints (otherwise Application.Current.Properties won't load). Cast back is handled by default deserializer
        public static readonly BindableValue<Numbers> Numbers = Settings.Register("number form", Crunch.Numbers.Decimal, serializer: value => (int)value);
        public static readonly BindableValue<Trigonometry> Trigonometry = Settings.Register("trig form", Crunch.Trigonometry.Degrees, serializer: value => (int)value);

        public static readonly BindableValue<ThemeOptions> ThemeSetting = Settings.Register("theme", ThemeOptions.System, deserializer: value => (ThemeOptions)(int)value, serializer: value => (int)value);
        public static readonly BindableValue<bool> ClearCanvasWarning = Settings.Register("clear canvas warning", true);
        // Needs the deserializer because v2.3.2 set the value for key "tutorial1" to null (so cast to bool fails). Just means the tutorial was already shown, so value should be false. Can probably be removed in a couple versions.
        public static readonly BindableValue<bool> ShouldRunTutorial = Settings.Register("tutorial1", true, deserializer: value => false);

        public static List<char> Variables = DefaultVariables();
        private static List<char> DefaultVariables()
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
        public static readonly BindableValue<bool> VariableRowExpanded = Settings.Register("variables", false, deserializer: value =>
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

        public static readonly BindableValue<bool> ShowFullKeyboard = Settings.Register("show full keyboard1", Device.Idiom == TargetIdiom.Tablet);
        public static readonly BindableValue<Point> KeyboardPosition = Settings.Register("keyboard position", new Point(1, 1), deserializer: value =>
        {
            string position = (string)value;

            int separator = position.IndexOf("|");
            //Print.Log("parsing keyboard position", position, position.Substring(0, separator), position.Substring(separator + 1));
            double x = double.Parse(position.Substring(0, separator));
            double y = double.Parse(position.Substring(separator + 1));

            return new Point(x, y);
        }, serializer: value => value.X.ToString() + "|" + value.Y.ToString());

        public static readonly BindableValue<bool> ShowTips = Settings.Register("should show tips", true);
        public static readonly IReadOnlyList<Tip> Tips = new List<Tip>
        {
            new Tip(Settings.Register<bool>("tap links tip"), "You can tap a link to see the equation it came from", "https://static.wixstatic.com/media/4a4e50_d9c8bdf752ca42c5abec19413cb4fc3b~mv2.gif", TargetIdiom.Phone | TargetIdiom.Tablet),
            new Tip(Settings.Register<bool>("assign variables tip"), "You can assign values to unknown variables", "https://static.wixstatic.com/media/4a4e50_b430cf0e45904a119e932b482b0bc7db~mv2.gif", TargetIdiom.Phone | TargetIdiom.Tablet),
            new Tip(Settings.Register<bool>("cursor mode tip"), "If you long press the keyboard, you can drag the cursor", "https://static.wixstatic.com/media/4a4e50_9c931692184c46e29c82dd2d64ff3c14~mv2.gif", TargetIdiom.Phone | TargetIdiom.Tablet),
            new Tip(Settings.Register<bool>("move keyboard tip"), "You can drag the keyboard by the \u25BD key to move the keyboard around the screen", "https://static.wixstatic.com/media/4a4e50_4f4048250d6e4d6896082ad53c57c384~mv2.gif", TargetIdiom.Tablet),
            new Tip(Settings.Register<bool>("default answers tip1"), "You can choose the default format for answers in settings", "https://static.wixstatic.com/media/4a4e50_afe479ab52874b8aabf1403c7ec3f2ff~mv2.gif", TargetIdiom.Phone | TargetIdiom.Tablet),
            new Tip(Settings.Register<bool>("clear canvas tip"), "You can clear the canvas by long pressing the DEL key", "https://static.wixstatic.com/media/4a4e50_ab41263fc30a4a9bb3949bdcb4179a3b~mv2.gif", TargetIdiom.Phone | TargetIdiom.Tablet),
            new Tip(Settings.Register<bool>("functions drawer tip"), "You can drop stored functions onto the canvas for quicker calcuations", "https://static.wixstatic.com/media/4a4e50_972f1465e4c644dea54b889a9ca1a072~mv2.gif", TargetIdiom.Phone | TargetIdiom.Tablet)
        };
    }
}