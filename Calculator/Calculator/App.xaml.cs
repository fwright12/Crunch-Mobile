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

            Settings.Load();

            if (Device.Idiom == TargetIdiom.Phone)
            {
                MainPage = new NavigationPage(new MainPage());
            }
            else if (Device.Idiom == TargetIdiom.Tablet)
            {
                MainPage = new MainPage();
            }
        }

        protected override void OnStart()
        {
            // Handle when your app starts
            System.Diagnostics.Debug.WriteLine("on start");
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
            Settings.Save();
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}