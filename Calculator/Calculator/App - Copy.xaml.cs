#if false
using System;
using System.Extensions;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Forms.Extensions;
using Xamarin.Forms.MathDisplay;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]

namespace Calculator
{
    //public enum UILayout { Condensed, Full };

    public partial class App : Application
    {
        new public static App Current => Application.Current as App;
        
        public static MainPage Home;
        public static double TextHeight { get; private set; }
        public static double TextWidth { get; private set; }


        public readonly DrawerPage Root;
        //private readonly NavigationPage SideNavigation;
        private NavigationPage FullNavigation;

        public App()
        {
            InitializeComponent();
            
            Resources = new CrunchStyle();
            
            Settings.Load();

            Label l = new Label
            {
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start,
                Text = "(",
                FontSize = Text.MaxFontSize,
            };
            l.SizeChanged += (sender, e) =>
            {
                TextHeight = l.Height;
                TextWidth = l.Width;
                
                Root.Detail = FullNavigation = new NavigationPage(Home = new MainPage());
                NavigationPage.SetHasNavigationBar(Home, false);
                TouchScreen.Instance = Home;

                /*FullNavigation.PushAsync(new ContentPage { Title = "test" });
                for (int i = 0; i < 5; i++)
                {
                    Page page = await FullNavigation.PopAsync();
                    Print.Log("popping", page);
                }*/

                if (Settings.ShouldRunTutorial)
                {
                    RunTutorial();
                }
            };

            MainPage = Root = new DrawerPage
            {
                Title = "Home",
                Detail = new ContentPage { Content = new StackLayout { Children = { l } } },
                Master = new NavigationPage(new SettingsPage())
                {
                    Title = "Settings"
                },
                MasterBehavior = MasterBehavior.Popover,
#if __IOS__
                IsGestureEnabled = false
#endif
            };
        }

        public static bool TutorialRunning = false;

        public async void RunTutorial()
        {
            if (TutorialRunning)
            {
                return;
            }
            TutorialRunning = true;

            if (Device.RuntimePlatform == Device.Android)
            {
                var keyboards = new System.Collections.Generic.List<IKeyboard>(KeyboardManager.Connected()).ToArray();
                IKeyboard current = KeyboardManager.Current;
                KeyboardManager.ClearKeyboards();

                MainPageTutorial tutorial = new MainPageTutorial();
                await FullNavigation.ReplaceCurrentPageAsync(tutorial);
                NavigationPage.SetHasNavigationBar(tutorial, false);

                /*await System.Threading.Tasks.Task.Delay(3000);

                KeyboardManager.ClearKeyboards();
                KeyboardManager.AddKeyboard(keyboards);
                KeyboardManager.SwitchTo(current);
                ReplaceCurrentPage(Home);*/

                tutorial.Completed += async () =>
                {
                    KeyboardManager.ClearKeyboards();
                    KeyboardManager.AddKeyboard(keyboards);
                    KeyboardManager.SwitchTo(current);

                    await FullNavigation.ReplaceCurrentPageAsync(Home);
                    TutorialRunning = false;
                };
            }
            else
            {
                Home.Tutorial();
            }
        }
        
        /*private void ShowSettings()
        {
            //FullNavigation.PushAsync(new SettingsPage());
            //return;
            Print.Log("showing settings");
            if (Collapsed)
            {
                FullNavigation.PushAsync(new SettingsPage());
            }
            else
            {
                Root.IsPresented = true;
            }
        }

        public async void HideSettings()
        {
            if (Root.IsPresented)
            {
                Root.IsPresented = false;
            }
            else
            {
                await FullNavigation.PopAsync();
            }
        }*/

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
#endif