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
        public Action<bool> SetVisible;

        private Stepper DecimalPlaces;
        private Selector LogBase;
        private Selector Numbers;
        private Selector Trig;
        private SwitchCell ClearCanvasWarning;

        public void Refresh()
        {
            DecimalPlaces.SetValue(Stepper.ValueProperty, Settings.DecimalPlaces);
            LogBase.Select(Settings.LogarithmBase == 2 ? 0 : 1);

            Numbers.Select((int)Settings.Numbers);
            Trig.Select((int)Settings.Trigonometry);

            ClearCanvasWarning.SetValue(SwitchCell.OnProperty, Settings.ClearCanvasWarning);
        }

        public SettingsPage()
        {
            InitializeComponent();

            Title = "Settings";

            TableView tableView = new TableView()
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                Intent = TableIntent.Settings,
                HasUnevenRows = true
            };

            //Math
            DecimalPlaces = new Stepper() { Minimum = 1, Maximum = 15, Increment = 1 };
            DecimalPlaces.ValueChanged += (sender, e) => Settings.DecimalPlaces = (int)e.NewValue;
            LogBase = new Selector("2", "10");
            LogBase.Selected += (selected) => Settings.LogarithmBase = selected == 0 ? 2 : 10;

            TableSection math = new TableSection() { Title = "Math" };
            math.Add(new LabeledCell("Decimal Precision:", DecimalPlaces.ValueWrappedStepper()));
            math.Add(new LabeledCell("Logarithm Base:", LogBase));

            //Answer Defaults
            Numbers = new Selector(Enum.GetNames(typeof(Crunch.Numbers)));
            Numbers.Selected += (selected) => Settings.Numbers = (Crunch.Numbers)selected;
            Trig = new Selector(Enum.GetNames(typeof(Crunch.Trigonometry)));
            Trig.Selected += (selected) => Settings.Trigonometry = (Crunch.Trigonometry)selected;

            TableSection answerDefaults = new TableSection() { Title = "Answer Defaults" };
            answerDefaults.Add(new LabeledCell("Numerical values:", Numbers));
            answerDefaults.Add(new LabeledCell("Trigonometry:", Trig));

            //Other
            ClearCanvasWarning = new SwitchCell() { Text = "Clear canvas warning" };
            ClearCanvasWarning.OnChanged += (sender, e) => Settings.ClearCanvasWarning = e.Value;

            TableSection other = new TableSection() { Title = "Other" };
            other.Add(ClearCanvasWarning);

            //Info
            TextCell about = new TextCell { Text = "About" };
            about.Tapped += async (sender, e) => await App.Navigation.PushAsync(new AboutPage());

            TextCell tutorial = new TextCell { Text = "Show tutorial" };
            tutorial.Tapped += async (sender, e) =>
            {
                if (Device.Idiom == TargetIdiom.Phone)
                {
                    await Navigation.PopAsync();
                }
                else
                {
                    (App.Current.MainPage as MasterDetailPage).IsPresented = false;
                }

                if (!Settings.Tutorial)
                {
                    App.Main.Tutorial();
                }
            };

            TextCell privacy = new TextCell { Text = "Privacy Policy" };
            privacy.Tapped += async (sender, e) => await App.Navigation.PushAsync(new PrivacyPolicyPage());

            TableSection info = new TableSection { Title = "Info" };
            info.Add(about);
            info.Add(tutorial);
            info.Add(privacy);
            foreach(TextCell cell in info)
            {
                if (Device.RuntimePlatform == Device.Android)
                {
                    cell.TextColor = CrunchStyle.TEXT_COLOR;
                }
            }

            Refresh();

            TableRoot root = new TableRoot();
            root.Add(math);
            root.Add(answerDefaults);
            root.Add(other);
            root.Add(info);

            tableView.Root = root;

            page.Children.Insert(0, tableView);

            reset.Clicked += async (sender, e) =>
            {
                if (await Application.Current.MainPage.DisplayAlert("Wait!", "This will reset all settings to their default values. This cannot be undone. Are you sure you want to continue?", "Yes", "No"))
                {
                    Settings.ResetToDefault();
                    Refresh();
                }
            };
        }
    }

    public class LabeledCell : ViewCell
    {
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
            layout.Children.Add(new Label() { Text = labelText, VerticalOptions = LayoutOptions.Center });
            layout.Children.Add(control);

            View = layout;
        }
    }

    public static class Extend
    {
        public static StackLayout ValueWrappedStepper(this Stepper stepper)
        {
            Label valueText = new Label
            {
                Text = stepper.Value.ToString(),
                HorizontalOptions = LayoutOptions.StartAndExpand,
                VerticalOptions = LayoutOptions.Center
            };
            stepper.VerticalOptions = LayoutOptions.Center;
            stepper.ValueChanged += (sender, e) => valueText.Text = e.NewValue.ToString();

            StackLayout layout = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand
            };
            layout.Children.Add(valueText);
            layout.Children.Add(stepper);

            return layout;
        }
    }
}