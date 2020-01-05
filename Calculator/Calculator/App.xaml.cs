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
    //public enum UILayout { Condensed, Full };

    public partial class App : Application
    {
        new public static App Current => Application.Current as App;

        public static MainPage Home;
        public static double TextHeight { get; private set; }
        public static double TextWidth { get; private set; }

        private readonly MasterDetailPage Root;
        private readonly NavigationPage SideNavigation;
        private NavigationPage FullNavigation;

        public static readonly BindableProperty CollapsedProperty = BindableProperty.Create("Collapsed", typeof(bool), typeof(App));
        
        public bool Collapsed
        {
            get { return (bool)GetValue(CollapsedProperty); }
            private set { SetValue(CollapsedProperty, value); }
        }

        //public static bool IsCondensed => Home.CrunchKeyboard.IsCondensed;

        //public event EventHandler<ToggledEventArgs> UILayoutChanged;
        //public bool IsLayoutCondensed { get; private set; }

        private bool SettingsInStack()
        {
            foreach(Page page in FullNavigation.Navigation.NavigationStack)
            {
                if (page is SettingsPage)
                {
                    return true;
                }
            }

            return false;
        }

        public void CollapsedChanged(bool collapsed)
        {
            if (collapsed == Collapsed)
            {
                return;
            }
            Collapsed = collapsed;

            Print.Log("app layout changed", Collapsed, Root.IsPresented);

            if (collapsed && Root.IsPresented)
            {
                Root.IsPresented = false;
                //FullNavigation.PushAsync(new SettingsPage());
            }
            else if (!collapsed && SettingsInStack())
            {
                FullNavigation.PopToRootAsync(false);
            }
            else
            {
                //Root.IsPresented = true;
                return;
            }

            ShowSettings(false);

            //Print.Log("set collapsed to " + collapsed, Collapsed);
            //IsLayoutCondensed = condensed;
            //UILayoutChanged?.Invoke(this, new ToggledEventArgs(condensed));
        }

        public void ShowSettings(bool animated = true)
        {
            //FullNavigation.PushAsync(new SettingsPage());
            //return;
            Print.Log("showing settings");
            if (Collapsed)
            {
                FullNavigation.PushAsync(new SettingsPage(), animated);
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
                await ReplaceCurrentPage(tutorial);
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

                    await ReplaceCurrentPage(Home);
                    TutorialRunning = false;
                };
            }
            else
            {
                Home.Tutorial();
            }
        }

        private System.Threading.Tasks.Task ReplaceCurrentPage(Page replacement)
        {
            FullNavigation.Navigation.InsertPageBefore(replacement, FullNavigation.CurrentPage);
            return FullNavigation.PopToRootAsync();
        }

        public App()
        {
            InitializeComponent();
            
            Resources = new CrunchStyle();

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
                TextHeight = l.Height;
                TextWidth = l.Width;

                Root.Detail = FullNavigation = new NavigationPage(Home = new MainPage());
                NavigationPage.SetHasNavigationBar(Home, false);
                TouchScreen.Instance = Home;

                if (ShouldRunTutorial.Value)
                {
                    RunTutorial();
                }
#if __IOS__
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
                        Home.ShowTip(tip.Item2, tip.Item3);
                        tip.Item1.Value = true;
                    }
                }
#endif
            };

            Collapsed = Device.Idiom != TargetIdiom.Tablet;
            MainPage = Root = new MasterDetailPage
            {
                Title = "Home",
                Detail = new ContentPage { Content = new StackLayout { Children = { l } } },
                Master = SideNavigation = new NavigationPage(new SettingsPage())
                {
                    Title = "Settings"
                },
                MasterBehavior = MasterBehavior.Popover,
#if __IOS__
                IsGestureEnabled = false
#endif
            };
        }

#if __IOS__
        // Fix for iOS problems with split view on iPad
        public class MasterDetailPage : Xamarin.Forms.MasterDetailPage
        {
            new public bool IsPresented
            {
                get => base.IsPresented;
                set
                {
                    base.IsPresented = !value;
                    base.IsPresented = value;
                }
            }
        }
#endif

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