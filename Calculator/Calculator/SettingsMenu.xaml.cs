using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Calculator
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsMenu : ContentView
    {
        private static string CanvasLocation
        {
            get
            {
                if (Device.Idiom == TargetIdiom.Tablet)
                {
                    return "The entire screen is the canvas - tap anywhere to start a new calculation";
                }
                else
                {
                    return "The top half of the page is the canvas - tap it to start a new calculation";
                }
            }
        }

        private static string AdditionalKeyboardFunctionality
        {
            get
            {
                if (Device.Idiom == TargetIdiom.Tablet)
                {
                    return "The dock button (bottom right key) can be used to change the location of the keyboard. Tap it to toggle between having the keyboard follow your calculations, or float in one position. Tap and drag to change the floating position.";
                }
                else
                {
                    return "Additional operations can be accessed by scrolling the keyboard to the right";
                }
            }
        }

        private Stepper DecimalPlaces;
        private Toggle LogBase;
        private Toggle Numbers;
        private Toggle Trig;
        private SwitchCell ClearCanvasWarning;

        public void Refresh()
        {
            DecimalPlaces.SetValue(Stepper.ValueProperty, Settings.DecimalPlaces);
            LogBase.Select(Settings.LogarithmBase == 2 ? 0 : 1);

            Numbers.Select((int)Settings.Numbers);
            Trig.Select((int)Settings.Trigonometry);

            ClearCanvasWarning.SetValue(SwitchCell.OnProperty, Settings.ClearCanvasWarning);
        }

        public SettingsMenu()
        {
            InitializeComponent();

            TableView tableView = new TableView() { VerticalOptions = LayoutOptions.FillAndExpand, Intent = TableIntent.Settings, HasUnevenRows = true };

            //Math
            DecimalPlaces = new Stepper() { Minimum = 1, Maximum = 15, Increment = 1 };
            DecimalPlaces.ValueChanged += (sender, e) => Settings.DecimalPlaces = (int)e.NewValue;
            LogBase = new Toggle("2", "10");
            LogBase.Toggled += (selected) => Settings.LogarithmBase = selected == 0 ? 2 : 10;

            TableSection math = new TableSection() { Title = "Math" };
            math.Add(new LabeledCell("Decimal Precision:", DecimalPlaces.ValueWrappedStepper()));
            math.Add(new LabeledCell("Logarithm Base:", LogBase));

            //Answer Defaults
            Numbers = new Toggle(Enum.GetNames(typeof(Crunch.Engine.Numbers)));
            Numbers.Toggled += (selected) => Settings.Numbers = (Crunch.Engine.Numbers)selected;
            Trig = new Toggle(Enum.GetNames(typeof(Crunch.Engine.Trigonometry)));
            Trig.Toggled += (selected) => Settings.Trigonometry = (Crunch.Engine.Trigonometry)selected;

            TableSection answerDefaults = new TableSection() { Title = "Answer Defaults" };
            answerDefaults.Add(new LabeledCell("Numerical values:", Numbers));
            answerDefaults.Add(new LabeledCell("Trigonometry:", Trig));

            //Other
            ClearCanvasWarning = new SwitchCell() { Text = "Clear canvas warning" };
            ClearCanvasWarning.OnChanged += (sender, e) => Settings.ClearCanvasWarning = e.Value;

            TableSection other = new TableSection() { Title = "Other" };
            other.Add(ClearCanvasWarning);

            Refresh();

            TableRoot root = new TableRoot();
            root.Add(math);
            root.Add(answerDefaults);
            root.Add(other);

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

            tips.Clicked += (sender, e) => Application.Current.MainPage.DisplayAlert("Tips",
                    "A few tips about how to navigate the app:\n\n" + CanvasLocation +
                    "\n\nCrunch allows you to view your answer in multiple forms, when possible. Tap the answer to cycle through them, or in the case of degrees and radians, tap the label. The answer can also be moved on the canvas by simply touching and dragging the equals sign.\n\n" +
                    "There is also additional functionality attached to the keyboard keys. Long pressing DEL will clear the canvas, and long pressing any other button gives you the ability to move the cursor.\n" + AdditionalKeyboardFunctionality,
                    "Dismiss");

            about.Clicked += (sender, e) => Application.Current.MainPage.DisplayAlert("About Crunch",
                "Thank you for using Crunch!\n\n" +
                "If you find any bugs, please report them to GreenMountainLabs802@gmail.com. The more information " +
                "you can provide (what you did to cause the error, screenshots, etc.) the easier it will be to fix.\n\n" +
                "If you enjoy using Crunch, please rate it on the app store. Ratings help with visibility, so other " +
                "people can find Crunch.\n\n" +
                "Please also email me with any ideas you have about how the app can be improved, or features you " +
                "would like to see in the future. I'm open to suggestions!",
                "Dismiss");
        }
    }

    public delegate void ToggledEventHandler(int selected);

    public class Toggle : StackLayout
    {
        public static Color NoColor = new Color(-1, -1, -1, -1);
        public event ToggledEventHandler Toggled;

        private Button selected;

        public void Select(int index) => Select(Children[index] as Button);

        private void Select(Button b)
        {
            if (selected != null)
            {
                selected.BackgroundColor = NoColor;
            }

            b.BackgroundColor = Color.MediumPurple;
            selected = b;

            Toggled?.Invoke(Children.IndexOf(b));
        }

        public Toggle(params string[] list)
        {
            HorizontalOptions = LayoutOptions.EndAndExpand;
            Orientation = StackOrientation.Horizontal;
            Spacing = 0;

            for (int i = 0; i < list.Length; i++)
            {
                Button b = new Button { Text = list[i], BorderColor = Color.Black, BorderWidth = 1, CornerRadius = 5 };
                b.Clicked += (o, e) => Select(o as Button);

                Children.Add(b);
            }
        }
    }

    public class LabeledCell : ViewCell
    {
        public LabeledCell(string labelText, View control)
        {
            control.VerticalOptions = LayoutOptions.Center;
            //control.HorizontalOptions = LayoutOptions.EndAndExpand;

            StackLayout layout = new StackLayout() { Padding = new Thickness(15, 5, 10, 5), Orientation = StackOrientation.Horizontal, HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.FillAndExpand };
            layout.Children.Add(new Label() { Text = labelText, VerticalOptions = LayoutOptions.Center });
            layout.Children.Add(control);

            View = layout;
        }
    }

    public static class Extend
    {
        public static StackLayout ValueWrappedStepper(this Stepper stepper)
        {
            Label valueText = new Label() { Text = stepper.Value.ToString(), HorizontalOptions = LayoutOptions.StartAndExpand, VerticalOptions = LayoutOptions.Center };
            stepper.VerticalOptions = LayoutOptions.Center;
            stepper.ValueChanged += (sender, e) => valueText.Text = e.NewValue.ToString();

            StackLayout layout = new StackLayout() { Orientation = StackOrientation.Horizontal, HorizontalOptions = LayoutOptions.FillAndExpand };
            layout.Children.Add(valueText);
            layout.Children.Add(stepper);

            return layout;
        }
    }

    public class SettingsTableView : TableView
    {
        public SettingsTableView()
        {

        }
    }
}