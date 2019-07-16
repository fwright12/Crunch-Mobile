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
        public static Numbers Numbers = Numbers.Exact;
        public static Trigonometry Trigonometry = Trigonometry.Degrees;

        public static bool Tutorial = true;
        public static bool ClearCanvasWarning = true;
        public static bool LeftHanded = false;

        private static readonly string DECIMAL_PLACES = "decimal places";
        private static readonly string LOG_BASE = "implicit logarithm base";
        private static readonly string NUMBER_FORM = "number form";
        private static readonly string TRIG_FORM = "trig form";
        private static readonly string CLEAR_CANVAS_WARNING = "clear canvas warning";
        private static readonly string TUTORIAL = "tutorial1";

        public static void Load()
        {
            DecimalPlaces = Storage.TryGet(DECIMAL_PLACES, 3);
            LogarithmBase = Storage.TryGet(LOG_BASE, 10.0);

            Numbers = Storage.TryGet(NUMBER_FORM, Numbers.Exact);
            Trigonometry = Storage.TryGet(TRIG_FORM, Trigonometry.Degrees);

            ClearCanvasWarning = Storage.TryGet(CLEAR_CANVAS_WARNING, true);
            Tutorial = Storage.TryGet(TUTORIAL, true);
        }

        public static async void Save()
        {
            Storage[DECIMAL_PLACES] = DecimalPlaces;
            Storage[LOG_BASE] = LogarithmBase;
            Storage[NUMBER_FORM] = (int)Numbers;
            Storage[TRIG_FORM] = (int)Trigonometry;
            Storage[CLEAR_CANVAS_WARNING] = ClearCanvasWarning;
            Storage[TUTORIAL] = false;

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
