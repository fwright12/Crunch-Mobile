using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Extensions;

namespace Calculator
{
	public class SettingsPage : ContentPage
	{
        private Stepper DecimalPlaces;
        private Selector LogBase;
        private Selector Numbers;
        private Selector Trig;
        private SwitchCell ClearCanvasWarning;
        private SwitchCell ShowFullKeyboard;

        public void Refresh()
        {
            DecimalPlaces.SetValue(Stepper.ValueProperty, Settings.DecimalPlaces);
            LogBase.Select(Settings.LogarithmBase == 2 ? 0 : 1);

            Numbers.Select((int)Settings.Numbers);
            Trig.Select((int)Settings.Trigonometry);

            ClearCanvasWarning.SetValue(SwitchCell.OnProperty, Settings.ClearCanvasWarning);
            ShowFullKeyboard.SetValue(SwitchCell.OnProperty, Settings.ShouldShowFullKeyboard);
        }

        public SettingsPage()
        {
            Title = "Settings";

            //Math
            DecimalPlaces = new Stepper()
            {
                Minimum = 1,
                Maximum = 15,
                Increment = 1
            };
            DecimalPlaces.ValueChanged += (sender, e) => Settings.DecimalPlaces = (int)e.NewValue;
            LogBase = new Selector("2", "10");
            LogBase.Selected += (selected) => Settings.LogarithmBase = selected == 0 ? 2 : 10;

            TableSection math = new TableSection()
            {
                Title = "Math"
            };
            math.Add(new LabeledCell("Decimal Precision:", DecimalPlaces.ValueWrappedStepper()));
            math.Add(new LabeledCell("Logarithm Base:", LogBase));

            //Answer Defaults
            Numbers = new Selector(Enum.GetNames(typeof(Crunch.Numbers)));
            Numbers.Selected += (selected) => Settings.Numbers = (Crunch.Numbers)selected;
            Trig = new Selector(Enum.GetNames(typeof(Crunch.Trigonometry)));
            Trig.Selected += (selected) => Settings.Trigonometry = (Crunch.Trigonometry)selected;

            TableSection answerDefaults = new TableSection()
            {
                Title = "Answer Defaults"
            };
            answerDefaults.Add(new LabeledCell("Numerical values:", Numbers));
            answerDefaults.Add(new LabeledCell("Trigonometry:", Trig));

            //Other
            ClearCanvasWarning = new SwitchCell()
            {
                Text = "Clear canvas warning"
            };
            ClearCanvasWarning.OnChanged += (sender, e) => Settings.ClearCanvasWarning = e.Value;
            ShowFullKeyboard = new SwitchCell()
            {
                Text = "Show full keyboard"
            };
            ShowFullKeyboard.OnChanged += (sender, e) =>
            {
                Settings.ShouldShowFullKeyboard = e.Value;
            };

            TableSection other = new TableSection()
            {
                Title = "Other"
            };
            other.Add(ClearCanvasWarning);
            if (Device.Idiom == TargetIdiom.Tablet)
            {
                other.Add(ShowFullKeyboard);
            }

            //Info
            TextCell about = new TextCell
            {
                Text = "About"
            };
            about.Tapped += async (sender, e) => await Navigation.PushAsync(new AboutPage());

            TextCell tutorial = new TextCell
            {
                Text = "Tutorial"
            };
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

            TextCell support = new TextCell
            {
                Text = "Support \u2197"
            };
            //ExternalLinkCell support = new ExternalLinkCell { Text = "Support" };
            support.Tapped += (sender, e) => Device.OpenUri(new Uri(@"https://gml802.wixsite.com/apps/support"));

            TextCell tips = new TextCell
            {
                Text = "Tips & Tricks \u2197"
            };
            tips.Tapped += (sender, e) => Device.OpenUri(new Uri(@"https://gml802.wixsite.com/apps/support"));

            TextCell privacy = new TextCell
            {
                Text = "Privacy Policy \u2197"
            };
            privacy.Tapped += (sender, e) => Device.OpenUri(new Uri(@"https://gml802.wixsite.com/apps/privacy"));

            TableSection info = new TableSection
            {
                Title = "Info"
            };
            info.Add(about);
            info.Add(tutorial);
            info.Add(support);
            info.Add(tips);
            info.Add(privacy);
            foreach(TextCell cell in info)
            {
                if (Device.RuntimePlatform == Device.Android)
                {
                    cell.TextColor = CrunchStyle.TEXT_COLOR;
                }
            }

            //TextCell test = new TextCell
            LabelCell test = new LabelCell
            {
                Label = new Label
                {
                    Text = "Reset to Default",
                    TextColor = Color.Red,
                    HorizontalTextAlignment = TextAlignment.Center,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                },
            };
            TableSection danger = new TableSection
            {
                Title = " ",
            };
            danger.Add(test);

            Refresh();

            TableRoot root = new TableRoot();
            root.Add(math);
            root.Add(answerDefaults);
            root.Add(other);
            root.Add(info);
            root.Add(danger);

            test.Tapped += async (sender, e) =>
            {
                if (await Application.Current.MainPage.DisplayAlert("Wait!", "This will reset all settings to their default values. This cannot be undone. Are you sure you want to continue?", "Yes", "No"))
                {
                    Settings.ResetToDefault();
                    Refresh();
                }
            };

            Content = new TableView()
            {
                Root = root,
                VerticalOptions = LayoutOptions.FillAndExpand,
                Intent = TableIntent.Settings,
                HasUnevenRows = true
            };

            /*Content = new StackLayout
            {
                Orientation = StackOrientation.Vertical,
                Children =
                {
                    tableView,
                    new Button
                    {
                        VerticalOptions = LayoutOptions.End,
                        HorizontalOptions = LayoutOptions.Center,
                        Text = "Reset to Default",
                        IsVisible = false
                    }
                }
            };*/
        }

        private ContentPage FullPageWebView(string url) => new ContentPage
        {
            Content = new WebView
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                Source = url
            }
        };
    }

    public class LabelCell : ViewCell
    {
        public Label Label
        {
            get => label;
            set
            {
                label?.Remove();
                Layout.Children.Add(label = value);
            }
        }

        private StackLayout Layout;
        private Label label;

        public LabelCell()
        {
            Layout = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                Padding = new Thickness(15),
            };

            View = Layout;
        }
    }

    public class ExternalLinkCell : ViewCell
    {
        public static BindableProperty TextProperty = TextCell.TextProperty;

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public ExternalLinkCell()
        {
            StackLayout layout = new StackLayout
            {
                Orientation = StackOrientation.Vertical,
            };

            Label symbol = new Label
            {
                Text = "\u2197"
            };

            Label text = new Label
            {
                BindingContext = this,
            };
            text.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == Label.FontSizeProperty.PropertyName)
                {
                    symbol.FontSize = 1.2 * text.FontSize;
                }
            };

            layout.Children.Add(text);
            layout.Children.Add(symbol);

            View = layout;
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