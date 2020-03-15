using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using Xamarin.Forms.Extensions;

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
                new LabeledCell("Numerical values:", Numbers = new Selector(Enum.GetNames(typeof(Crunch.Numbers)))),
                new LabeledCell("Trigonometry:", Trig = new Selector(Enum.GetNames(typeof(Crunch.Trigonometry))))
            });

#if DEBUG
            SwitchCell sampleMode;

            (Content as TableView).Root.Insert((Content as TableView).Root.Count - 1, new TableSection("Developer Options")
            {
                (sampleMode = new SwitchCell
                {
                    Text = "Sampling",
                    On = App.Current.GetInSampleMode()
                })
            });

            sampleMode.OnChanged += (sender, e) => App.Current.SetInSampleMode(e.Value);
#endif

#if ANDROID
            foreach (TableSection section in (Content as TableView).Root)
            {
                foreach(TextCell textCell in section.OfType<TextCell>())
                {
                    textCell.TextColor = CrunchStyle.TEXT_COLOR;
                }
            }
#endif

            LogBase.Selected += (selected) => App.LogarithmBase.Value = selected == 0 ? 2 : 10;

            Numbers.Selected += (selected) => App.Numbers.Value = (Crunch.Numbers)selected;
            Trig.Selected += (selected) => App.Trigonometry.Value = (Crunch.Trigonometry)selected;

            /*DecimalStepper.RemoveBinding(Stepper.ValueProperty);
            DecimalStepper.Bind<double>(Stepper.ValueProperty, value => App.DecimalPlaces.Value = (int)value);*/
#if ANDROID
            ShowFullKeyboard.RemoveBinding(SwitchCell.OnProperty);
            ShowFullKeyboard.Bind<bool>(SwitchCell.OnProperty, value => App.ShowFullKeyboard.Value = value);
            App.ShowFullKeyboard.Bind<bool>(App.ShowFullKeyboard.ValueProperty, value => ShowFullKeyboard.On = value);
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
                if (App.Current.MainPage is MasterDetailPage masterDetail && masterDetail.IsPresented)
                {
                    masterDetail.IsPresented = false;
                }
                else
                {
                    await Navigation.PopAsync();
                }

                (App.Current as App).RunTutorial();
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
        }

        public void Refresh()
        {
            LogBase.Select(App.LogarithmBase.Value == 2 ? 0 : 1);

            Numbers.Select((int)App.Numbers.Value);
            Trig.Select((int)App.Trigonometry.Value);

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

    public class LabeledCell : ViewCell
    {
        public LabeledCell() { }

        public LabeledCell(string labelText, View control)
        {
            control.VerticalOptions = LayoutOptions.Center;
            //control.HorizontalOptions = LayoutOptions.EndAndExpand;

            StackLayout layout = new StackLayout
            {
                Padding = new Thickness(15, 5, 10, 5),
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand
            };
            layout.Children.Add(new Label()
            {
                Text = labelText,
                VerticalOptions = LayoutOptions.Center
            });
            layout.Children.Add(control);

            View = layout;
        }
    }
}