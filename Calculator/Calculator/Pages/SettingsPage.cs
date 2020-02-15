using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Extensions;
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
            //DecimalPlaces.SetValue(Stepper.ValueProperty, Settings.DecimalPlaces.Value);
            LogBase.Select(App.LogarithmBase.Value == 2 ? 0 : 1);

            Numbers.Select((int)App.Numbers.Value);
            Trig.Select((int)App.Trigonometry.Value);

            //ClearCanvasWarning.SetValue(SwitchCell.OnProperty, Settings.ClearCanvasWarning.Value);
            RefreshShowFullKeyboard();
            //ShowFullKeyboard.SetValue(SwitchCell.OnProperty, Settings.ShouldShowFullKeyboard);
        }

        private void RefreshShowFullKeyboard()
        {
            ShowFullKeyboard.On = App.Current.Home.Collapsed ? false : App.ShowFullKeyboard.Value;
        }

        public SettingsPage()
        {
            Title = "Settings";

            //Math
            DecimalPlaces = new Stepper()
            {
                BindingContext = App.DecimalPlaces,
                Minimum = 1,
                Maximum = 15,
                Increment = 1
            };
            DecimalPlaces.SetBinding(Stepper.ValueProperty, "Value", BindingMode.TwoWay);
            /*Settings.DecimalPlaces.PropertyChanged += (sender, e) =>
            //Settings.DecimalPlaces.WhenPropertyChanged(Settings.DecimalPlaces.ValueProperty, (sender, e) =>
            {
                Print.Log(e.PropertyName);
            });*/
            //DecimalPlaces.ValueChanged += (sender, e) => Settings.DecimalPlaces.Value = (int)e.NewValue;
            LogBase = new Selector("2", "10");
            LogBase.Selected += (selected) => App.LogarithmBase.Value = selected == 0 ? 2 : 10;

            TableSection math = new TableSection()
            {
                Title = "Math"
            };
            math.Add(new LabeledCell("Decimal Precision:", DecimalPlaces.ValueWrappedStepper()));
            math.Add(new LabeledCell("Logarithm Base:", LogBase));

            //Answer Defaults
            Numbers = new Selector(Enum.GetNames(typeof(Crunch.Numbers)));
            Numbers.Selected += (selected) => App.Numbers.Value = (Crunch.Numbers)selected;
            Trig = new Selector(Enum.GetNames(typeof(Crunch.Trigonometry)));
            Trig.Selected += (selected) => App.Trigonometry.Value = (Crunch.Trigonometry)selected;

            TableSection answerDefaults = new TableSection()
            {
                Title = "Answer Defaults"
            };
            answerDefaults.Add(new LabeledCell("Numerical values:", Numbers));
            answerDefaults.Add(new LabeledCell("Trigonometry:", Trig));

            //Other
            ClearCanvasWarning = new SwitchCell
            {
                BindingContext = App.ClearCanvasWarning,
                Text = "Clear canvas warning",
            };
            ClearCanvasWarning.SetBinding(SwitchCell.OnProperty, "Value", BindingMode.TwoWay);
            //ClearCanvasWarning.OnChanged += (sender, e) => Settings.ClearCanvasWarning = e.Value;
            ShowFullKeyboard = new SwitchCell
            {
                BindingContext = App.Current,
                Text = "Show full keyboard",
            };
            ShowFullKeyboard.OnChanged += (sender, e) =>
            {
                if (ShowFullKeyboard.IsEnabled)
                {
                    App.ShowFullKeyboard.Value = e.Value;
                }
            };
            ShowFullKeyboard.SetBinding(Cell.IsEnabledProperty, "Collapsed", converter: new ValueConverter<bool>((b) => !b));
            //App.Current.WhenPropertyChanged(DrawerPage.CollapsedProperty, (sender, e) => RefreshShowFullKeyboard());

            TableSection other = new TableSection()
            {
                Title = "Other"
            };
            other.Add(ClearCanvasWarning);
            other.Add(ShowFullKeyboard);

            TextCell tutorial = new TextCell
            {
                Text = "Tutorial"
            };
            tutorial.Tapped += async (sender, e) =>
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

            // Help
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

            SwitchCell showTips = new SwitchCell
            {
                Text = "Show new tips at startup",
                BindingContext = App.ShowTips
            };
            showTips.SetBinding(SwitchCell.OnProperty, "Value", BindingMode.TwoWay);

            TableSection help = new TableSection("Help")
            {
                tutorial,
                support,
                tips
            };
#if __IOS__
            help.Add(showTips);
#endif

            //Info
            TextCell about = new TextCell
            {
                Text = "About"
            };
            about.Tapped += (sender, e) => Navigation.PushAsync(new AboutPage());

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
            info.Add(privacy);
            if (Device.RuntimePlatform == Device.Android)
            {
                foreach (TableSection section in new TableSection[] { help, info })
                {
                    foreach (TextCell cell in section)
                    {
                        cell.TextColor = CrunchStyle.TEXT_COLOR;
                    }
                }
            }

            Refresh();

            TableRoot root = new TableRoot
            {
                math, 
                answerDefaults,
                other,
                help,
                info,
                Danger()
            };

#if DEBUG
            SwitchCell sampleMode;

            TableSection sample = new TableSection("Developer Options")
            {
                (sampleMode = new SwitchCell
                {
                    Text = "Sampling",
                    On = App.Current.GetInSampleMode()
                })
            };

            sampleMode.OnChanged += (sender, e) => App.Current.SetInSampleMode(e.Value);

            root.Insert(root.Count - 1, sample);
#endif
            Content = new TableView()
            {
                Root = root,
                VerticalOptions = LayoutOptions.FillAndExpand,
                Intent = TableIntent.Settings,
#if __IOS__
                BackgroundColor = Color.Transparent,
#endif
                HasUnevenRows = true
            };

#if __IOS__
            BackgroundColor = Color.LightGray;
#endif
            Xamarin.Forms.PlatformConfiguration.iOSSpecific.Page.SetUseSafeArea(this, true);
        }

        private TableSection Danger()
        {
            LabelCell reset = new LabelCell
            {
                Label = new Label
                {
                    Text = "Reset to Default",
                    TextColor = Color.Red,
                    HorizontalTextAlignment = TextAlignment.Center,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                },
            };

            reset.Tapped += async (sender, e) =>
            {
                if (await Application.Current.MainPage.DisplayAlert("Wait!", "This will reset all settings to their default values. This cannot be undone. Are you sure you want to continue?", "Yes", "No"))
                {
                    ResetToDefault();
                    Refresh();
                }
            };
            TableSection danger = new TableSection
            {
                Title = " ",
            };
            danger.Add(reset);

            return danger;
        }

        private void ResetToDefault()
        {
            foreach(BindableValue setting in new BindableValue[] { App.ClearCanvasWarning, App.DecimalPlaces, App.LogarithmBase, App.Numbers, App.ShowFullKeyboard, App.ShowTips, App.Trigonometry })
            {
                setting.ClearValue(setting.ValueProperty);
            }
        }
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
            layout.Children.Add(new Label()
            {
                Text = labelText,
                VerticalOptions = LayoutOptions.Center
            });
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