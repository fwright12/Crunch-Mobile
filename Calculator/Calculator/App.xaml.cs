using System;
using System.Linq;
using System.Extensions;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Forms.Extensions;
using Xamarin.Forms.MathDisplay;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]

namespace Calculator
{
    public partial class App : Application
    {
        public enum ThemeOptions { System, Light, Dark };

        new public static App Current => Application.Current as App;

        public MainPage Home;
        public static double TextHeight { get; private set; }
        public static double TextWidth { get; private set; }

        //public static readonly Color CRUNCH_PURPLE = Color.FromHex("#560297");
        public static readonly int PAGE_PADDING = 10;

        //public static readonly Color TEXT_COLOR = Color.Gray;
        //public static readonly Color BACKGROUND_COLOR = Color.FromHex("#ebeae8"); //rgb(244, 236, 215)

        //public static readonly Color BUTTON_TEXT_COLOR = Color.Black;
        //public static readonly Color BUTTON_BACKGROUND_COLOR = Color.LightGray;
        //public static readonly int CORNER_RADIUS = 5;

        //public static readonly string SYMBOLA_FONT = Device.RuntimePlatform == Device.Android ? "Symbola.ttf#Symbola" : "Symbola";

        public static readonly Thickness BUTTON_PADDING = Device.RuntimePlatform == Device.iOS ? new Thickness(10, 0, 10, 0) : new Thickness(0);

        private static bool TutorialRunning = false;

        public void RunTutorial()
        {
            if (TutorialRunning)
            {
                return;
            }
            TutorialRunning = true;

            TutorialDialog tutorial = new TutorialDialog(Device.Idiom == TargetIdiom.Phone)
            {
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                WidthRequest = 400,
                HeightRequest = 400
            };

            tutorial.Completed += () =>
            {
                tutorial.Remove();
                TutorialRunning = false;
            };

            (Home.Content as AbsoluteLayout)?.Children.Add(tutorial, new Rectangle(0.5, 0.5, 0.75, 0.75), AbsoluteLayoutFlags.All);
        }

        public static bool DarkModeSupported = (Device.RuntimePlatform == Device.Android && Xamarin.Essentials.DeviceInfo.Version.Major >= 10) || (Device.RuntimePlatform == Device.iOS && Xamarin.Essentials.DeviceInfo.Version.Major >= 13);

        public static BindableProperty SystemDarkModeEnabledProperty = BindableProperty.Create(nameof(SystemDarkModeEnabled), typeof(bool), typeof(App), false);

        public bool SystemDarkModeEnabled
        {
            get => (bool)GetValue(SystemDarkModeEnabledProperty);
            set => SetValue(SystemDarkModeEnabledProperty, value);
        }

        public ResourceDictionary Theme { get; private set; }

        private void SetTheme(ThemeOptions newTheme)
        {
            if (newTheme == ThemeOptions.System)
            {
                newTheme = SystemDarkModeEnabled ? ThemeOptions.Dark : ThemeOptions.Light;
            }

            ThemeOptions currentTheme = Theme is DarkTheme ? ThemeOptions.Dark : ThemeOptions.Light;
            if (newTheme == currentTheme)
            {
                return;
            }

            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(Theme = (newTheme == ThemeOptions.Dark ? (ResourceDictionary)new DarkTheme() : new LightTheme()));

            OnPropertyChanged(nameof(Theme));
        }

        public App()
        {
            InitializeComponent();

            ThemeSetting.Bind<ThemeOptions>(ThemeSetting.ValueProperty, value => SetTheme(value));
            this.WhenPropertyChanged(SystemDarkModeEnabledProperty, (sender, e) =>
            {
                if (ThemeSetting.Value != ThemeOptions.System)
                {
                    return;
                }

                SetTheme(ThemeOptions.System);
            });

            //Resources.Add(new CrunchStyle());
            //Resources.MergedDictionaries.Clear();
            //Resources.MergedDictionaries.Add(new DarkTheme());
            
            bool success = Settings.Load();
#if DEBUG
            if (!success)
            {
                //Print.Log("Failed to set Setting value from storage (possible type mismatch?). Setting looking for type " + setting.ValueProperty.ReturnType + " and storage value is type " + value.GetType());

                throw new Exception();
            }
#endif

            Label l = new Label
            {
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start,
                Text = "(",
                FontSize = Text.MaxFontSize,
            };
            l.SizeChanged += (sender, e) =>
            {
                //Size size = l.Measure();
                TextHeight = l.Height;
                TextWidth = l.Width;

                MainPage = new NavigationPage(Home = new MainPage());
                NavigationPage.SetHasNavigationBar(Home, false);
                TouchScreen.Instance = Home;

                if (ShouldRunTutorial.Value)
                {
                    RunTutorial();
                }
                else if (ShowTips.Value)
                {
                    var list = new System.Collections.Generic.List<int>();
                    for (int i = 0; i < Tips.Count; i++)
                    {
                        var tip = Tips[i];
                        if (!tip.Item1.Value && tip.Item4.HasFlag(Device.Idiom))
                        {
                            list.Add(i);
                        }
                    }
                    Print.Log(ShowTips.Value, list.Count);

                    if (list.Count > 0)
                    {
                        int index = list[new Random().Next(list.Count)];
                        var tip = Tips[index];

                        TipDialog tips = null;
                        (Home.Content as AbsoluteLayout)?.Children.Add(tips = new TipDialog
                        {
                            Explanation = tip.Item2,
                            URL = tip.Item3,
                            HorizontalOptions = LayoutOptions.Center,
                            VerticalOptions = LayoutOptions.Center,
                            WidthRequest = 400,
                            HeightRequest = 400
                        }, new Rectangle(0.5, 0.5, 0.75, 0.75), AbsoluteLayoutFlags.All);
                        tip.Item1.Value = true;
                    }
                }
            };

            MainPage = new NavigationPage(new ContentPage { Content = new StackLayout { Children = { l } } });
        }

        protected override void OnStart()
        {
            // Handle when your app starts
            System.Diagnostics.Debug.WriteLine("on start");
        }

        protected async override void OnSleep()
        {
            // Handle when your app sleeps
            Settings.Store(true);
            Print.Log("saved", Application.Current.Properties.Count);
            await Application.Current.SavePropertiesAsync();
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}