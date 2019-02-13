using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace Calculator
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new MainPage();
        }

        private static T TryGet<T>(string key, T defaultValue)
        {
            object o;
            if (Current.Properties.TryGetValue(key, out o))
            {
                return (T)o;
            }
            else
            {
                return defaultValue;
            }
        }

        private static string decimalPlaces = "decimal places";
        private static string numberForm = "number form";
        private static string trigForm = "trig form";
        private static string clearCanvasWarning = "clear canvas warning";

        protected override void OnStart()
        {
            // Handle when your app starts
            System.Diagnostics.Debug.WriteLine("on start");
        }

        public static void LoadSettings()
        {
            Settings.DecimalPlaces = TryGet(decimalPlaces, 3);

            Settings.Numbers = TryGet(numberForm, Crunch.Engine.Numbers.Exact);
            Settings.Trigonometry = TryGet(trigForm, Crunch.Engine.Trigonometry.Degrees);

            Settings.ClearCanvasWarning = TryGet(clearCanvasWarning, true);
        }

        public static void SaveSettings()
        {
            Current.Properties.Clear();

            Current.Properties[decimalPlaces] = Settings.DecimalPlaces;
            Current.Properties[numberForm] = (int)Settings.Numbers;
            Current.Properties[trigForm] = (int)Settings.Trigonometry;
            Current.Properties[clearCanvasWarning] = Settings.ClearCanvasWarning;

            Current.SavePropertiesAsync();
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
            SaveSettings();
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}