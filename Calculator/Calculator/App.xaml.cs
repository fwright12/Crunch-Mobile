using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Forms.Extensions;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace Calculator
{
    public partial class App : Application
    {
        public static NavigationPage Navigation;
        public static MainPage Main;

        public App()
        {
            InitializeComponent();
            Resources = CrunchStyle.Apply();

            Settings.Load();

            if (Device.Idiom == TargetIdiom.Phone)
            {
                MainPage = Navigation = new NavigationPage(Main = new MainPage());
                NavigationPage.SetHasNavigationBar(Main, false);
            }
            else if (Device.Idiom == TargetIdiom.Tablet)
            {
                Navigation = new NavigationPage(new SettingsPage()) { Title = "Settings" };

                Main = new MainPage();
                MainPage = new MasterDetailPage
                {
                    Title = "Home",
                    Detail = new NavigationPage(Main),
                    Master = Navigation,
                    MasterBehavior = MasterBehavior.Popover,
#if __IOS__
                    IsGestureEnabled = false
#endif
                };
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