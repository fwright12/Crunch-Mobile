using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Forms.Extensions;
using Xamarin.Forms.MathDisplay;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace Calculator
{
    public partial class App : Application
    {
        private static NavigationPage Navigation;
        public static MainPage Main;

        public App()
        {
            InitializeComponent();
            
            MainPageSetup();
            
            return;

            Label l = new Label()
            {
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start,
                Text = "(",
                FontSize = Text.MaxFontSize,
            };
            l.SizeChanged += delegate
            {
                Text.MaxTextHeight = l.Height;
                double parenthesesWidth = l.Width;
                Calculator.MainPage.ParenthesesWidth = parenthesesWidth;

                Text.CreateLeftParenthesis = () => new TextImage(new Image() { Source = "leftParenthesis.png", HeightRequest = 0, WidthRequest = parenthesesWidth, Aspect = Aspect.Fill }, "(");
                Text.CreateRightParenthesis = () => new TextImage(new Image() { Source = "rightParenthesis.png", HeightRequest = 0, WidthRequest = parenthesesWidth, Aspect = Aspect.Fill }, ")");
                Text.CreateRadical = () => new Image() { Source = "radical.png", HeightRequest = 0, WidthRequest = parenthesesWidth * 2, Aspect = Aspect.Fill };

                MainPageSetup();
            };

            StackLayout layout = new StackLayout { };
            layout.Children.Add(l);
            MainPage = new ContentPage
            {
                Content = layout
            };
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

        private void MainPageSetup()
        {
            Resources = new CrunchStyle();

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
                /*(MainPage as MasterDetailPage).IsPresentedChanged += (sender, e) =>
                {
                    (sender as MasterDetailPage).Master.IsVisible = (sender as MasterDetailPage).IsPresented;
                };*/
            }
        }
    }
}