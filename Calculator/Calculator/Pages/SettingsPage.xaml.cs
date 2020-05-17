using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Extensions;
using Xamarin.Forms.Xaml;

namespace Calculator
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPage : ContentPage
    {
        private readonly Selector Numbers;
        private readonly Selector Trig;
        
        public SettingsPage()
        {
            InitializeComponent();
            
            (Content as TableView).Root.Insert(1, new TableSection("Answer Defaults")
            {
                new LabeledCell("Numerical values:", Numbers = new Selector(Enum.GetNames(typeof(Crunch.Numbers))) { HorizontalOptions = LayoutOptions.EndAndExpand }),
                new LabeledCell("Trigonometry:", Trig = new Selector(Enum.GetNames(typeof(Crunch.Trigonometry))) { HorizontalOptions = LayoutOptions.EndAndExpand })
            });
            
#if DEBUG
            Xamarin.Forms.SwitchCell sampleMode;

            (Content as TableView).Root.Insert((Content as TableView).Root.Count - 1, new TableSection("Developer Options")
            {
                (sampleMode = new Xamarin.Forms.SwitchCell { Text = "Sampling", On = App.Current.GetInSampleMode() })
            });

            sampleMode.OnChanged += (sender, e) => App.Current.SetInSampleMode(e.Value);
#endif
            
            LogBase.Selected += (selected) => App.LogarithmBase.Value = selected == 0 ? 2 : 10;

            Numbers.Selected += (selected) => App.Numbers.Value = (Crunch.Numbers)selected;
            Trig.Selected += (selected) => App.Trigonometry.Value = (Crunch.Trigonometry)selected;

            App.ThemeOptions SelectedTheme() => ThemeSelector.SelectedIndex == 1 ? App.ThemeOptions.Dark : App.ThemeOptions.Light;
            SystemThemeSwitch.SetBinding<bool, App.ThemeOptions>(Switch.IsToggledProperty, App.ThemeSetting, "Value", value => value == App.ThemeOptions.System, value => value ? App.ThemeOptions.System : SelectedTheme(), BindingMode.TwoWay);
            ThemeSelector.Selected += (selected) =>
            {
                if (SystemThemeSwitch.IsToggled)
                {
                    return;
                }

                App.ThemeSetting.Value = SelectedTheme();
            };

            ThemeSelectorCell.View.Margin = LabeledCell.ViewCellMargin;
            SystemThemeSwitch.Bind<bool>(Switch.IsToggledProperty, value =>
            {
                if (value)
                {
                    ThemeTableSection.Remove(ThemeSelectorCell);
                }
                else if (!ThemeTableSection.Contains(ThemeSelectorCell))
                {
                    ThemeTableSection.Insert(1, ThemeSelectorCell);
                }
            });

            if (!App.DarkModeSupported)
            {
                SystemThemeSwitch.IsToggled = false;
                ThemeTableSection.RemoveAt(0);
            }

            App.Current.Bind<ResourceDictionary>(nameof(App.Theme), theme =>
            {
                foreach (TableSection section in ((TableView)Content).Root)
                {
                    //new DynamicResource("SecondaryColor")
                    //section.TextColor = FindResource("SecondaryColor");// (Color)App.Current.Resources["SecondaryColor"];
                    //section.SetValue(TableSectionBase.TextColorProperty, "SecondaryColor");

                    object value;
                    if (Application.Current.Resources.TryGetValue("PrimaryKeyboardKeyColor", out value) && value is Color color)
                    {
                        section.TextColor = color;
                    }
                }
            });
            
            /*DecimalStepper.RemoveBinding(Stepper.ValueProperty);
            DecimalStepper.Bind<double>(Stepper.ValueProperty, value => App.DecimalPlaces.Value = (int)value);*/
#if ANDROID
            ShowFullKeyboard.SwitchView.RemoveBinding(Switch.IsToggledProperty);
            App.ShowFullKeyboard.Bind<bool>(App.ShowFullKeyboard.ValueProperty, value => ShowFullKeyboard.SwitchView.IsToggled = value);
            ShowFullKeyboard.SwitchView.Bind<bool>(Switch.IsToggledProperty, value => App.ShowFullKeyboard.Value = value);
#endif
            //clearc.RemoveBinding(SwitchCell.OnProperty);
            //ShowFullKeyboard.Bind<bool>(SwitchCell.OnProperty, value => App.ShowFullKeyboard.Value = value);

            /*ShowFullKeyboard.OnChanged += (sender, e) =>
            {
                App.ShowFullKeyboard.Value = e.Value;
                if (ShowFullKeyboard.IsEnabled)
                {
                    
                }
            };*/
            //ShowFullKeyboard.BindingContext = App.Current.Home;
            //ShowFullKeyboard.SetBinding(Cell.IsEnabledProperty, "Collapsed", converter: new ValueConverter<bool>((b) => !b));
            //App.Current.WhenPropertyChanged(DrawerPage.CollapsedProperty, (sender, e) => RefreshShowFullKeyboard());

            TutorialTextCell.Tapped += async (sender, e) =>
            {
                await Navigation.PopAsync();
                App.Current.RunTutorial();
            };

            //ExternalLinkCell support = new ExternalLinkCell { Text = "Support" };
            SupportTextCell.Tapped += (sender, e) => Device.OpenUri(new Uri(@"https://gml802.wixsite.com/apps/support"));
            TipsTextCell.Tapped += (sender, e) => Device.OpenUri(new Uri(@"https://gml802.wixsite.com/apps/support"));

            AboutTextCell.Tapped += (sender, e) => Navigation.PushAsync(new AboutPage());
            PrivacyTextCell.Tapped += (sender, e) => Device.OpenUri(new Uri(@"https://gml802.wixsite.com/apps/privacy"));

            ResetViewCell.Tapped += async (sender, e) =>
            {
                if (await Application.Current.MainPage.DisplayAlert("Wait!", "This will reset all settings to their default values. Are you sure you want to continue?", "Yes", "No"))
                {
                    ResetToDefault();
                    Refresh();
                }
            };
            
            Refresh();

            foreach (TableSection section in ((TableView)Content).Root)
            {   
                foreach (Cell cell in section)
                {
                    if (cell is TextCell textCell)
                    {
                        textCell.SetDynamicResource(TextCell.TextColorProperty, "DetailColor");
                    }
                    if (cell is SwitchCell switchCell)
                    {
                        //switchCell.SwitchView.ThumbColor = (Color)App.Current.Resources["ControlColor"];
                    }
                }
            }
        }

        public void Refresh()
        {
            LogBase.Select(App.LogarithmBase.Value == 2 ? 0 : 1);

            Numbers.Select((int)App.Numbers.Value);
            Trig.Select((int)App.Trigonometry.Value);

            ThemeSelector.Select((App.ThemeSetting.Value == App.ThemeOptions.Dark).ToInt());
            //ShowFullKeyboard.On = App.Current.Home.Collapsed ? false : App.ShowFullKeyboard.Value;
        }

        private void ResetToDefault()
        {
            foreach (BindableValue setting in new BindableValue[] { App.ClearCanvasWarning, App.DecimalPlaces, App.LogarithmBase, App.Numbers, App.ShowFullKeyboard, App.ShowTips, App.Trigonometry })
            {
                setting.ClearValue(setting.ValueProperty);
            }
        }
    }

    public class SwitchCell : LabeledCell
    {
        public Switch SwitchView => Control as Switch;

        public SwitchCell() { }

        public SwitchCell(string text, Switch switchView = null) : base(text, switchView ?? new Switch()) { }

        /*public static readonly BindableProperty OnProperty = BindableProperty.Create("On", typeof(bool), typeof(SwitchCell), false, propertyChanged: (obj, oldValue, newValue) =>
        {
            var switchCell = (SwitchCell)obj;
            switchCell.OnChanged?.Invoke(obj, new ToggledEventArgs((bool)newValue));
        }, defaultBindingMode: BindingMode.TwoWay);

        public static readonly BindableProperty TextProperty = BindableProperty.Create("Text", typeof(string), typeof(SwitchCell), default(string));

        public static readonly BindableProperty OnColorProperty = BindableProperty.Create(nameof(OnColor), typeof(Color), typeof(SwitchCell), Color.Default);

        public Color OnColor
        {
            get => Switch.OnColor;
            set => Switch.OnColor = value;
        }

        public Color ThumbColor
        {
            get => Switch.ThumbColor;
            set => Switch.ThumbColor = value;
        }

        public bool On
        {
            get => Switch.IsToggled;
            set => Switch.IsToggled = value;
        }

        public string Text
        {
            get => Label.Text;
            set => Label.Text = value;
        }

        public event EventHandler<ToggledEventArgs> OnChanged;

        public readonly Label LabelView;
        public readonly Switch SwitchView;

        public SwitchCell() : this(null, null) { }

        public SwitchCell(Label labelView = null, Switch switchView = null)
        {
            LabelView = labelView ?? new Label { HorizontalOptions = LayoutOptions.StartAndExpand, VerticalOptions = LayoutOptions.Center };
            SwitchView = switchView ?? new Switch { VerticalOptions = LayoutOptions.Center };

            View = new StackLayout
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                Padding = new Thickness(20, 5, 10, 5),
                Orientation = StackOrientation.Horizontal,
                Children = { LabelView, SwitchView }
            };
        }*/
    }

    public class LabeledCell : ViewCell
    {
        public static Thickness ViewCellMargin = Device.RuntimePlatform == Device.Android ? new Thickness(16.5, 10, 12.5, 10) : new Thickness(20, 5, 20, 5);

        public readonly Label LabelView;
        public readonly View Control;

        public LabeledCell() { }

        public LabeledCell(string labelText, View control)
        {
            Control = control;
            Control.HorizontalOptions = new LayoutOptions(Control.IsSet(View.HorizontalOptionsProperty) ? control.HorizontalOptions.Alignment : LayoutAlignment.End, true);
            Control.VerticalOptions = LayoutOptions.Center;
            
            View = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                Children =
                {
                    (LabelView = new Label()
                    {
                        Text = labelText,
                        FontSize = NamedSize.Body.On<Label>(),
                        VerticalOptions = LayoutOptions.Center
                    }),
                    Control
                }
            };
            View.Margin = ViewCellMargin;
        }
    }
}