using System;
using System.Collections.Generic;
using System.Text;

using System.Extensions;
using Xamarin.Forms;
using Crunch.Engine;

namespace Calculator
{
    public static class Settings
    {
        public static int DecimalPlaces
        {
            get { return Crunch.Engine.Math.DecimalPlaces; }
            set { Crunch.Engine.Math.DecimalPlaces = value; }
        }

        public static Polynomials Polynomials;
        public static Numbers Numbers = Numbers.Exact;
        public static Trigonometry Trigonometry = Trigonometry.Degrees;

        public static bool ClearCanvasWarning = true;
        public static bool LeftHanded = false;
    }

    public partial class MainPage
    {
        private void SetDecimalPrecision(object sender, ValueChangedEventArgs e)
        {
            Settings.DecimalPlaces = (int)e.NewValue;
            decimalPrecision.Text = e.NewValue.ToString();
        }

        private void SetMenuVisibility(bool visible)
        {
            settingsMenu.TranslateTo((settingsMenuWidth + Padding.Left) * (visible.ToInt() - 1), 0);
            settingsMenuButton.TranslateTo((settingsMenuWidth + Padding.Left) * visible.ToInt(), 0);

            //AbsoluteLayout.SetLayoutBounds(settingsMenuButton.Parent, new Rectangle((settingsMenuWidth + Padding.Left) * visible.ToInt(), 0, 50, 50));
        }

        private void ClearCanvasWarning(object sender, ToggledEventArgs e) => Settings.ClearCanvasWarning = e.Value;

        private double settingsMenuWidth;
        private readonly double maxSettingsMenuWidth = 400;
        private bool smallerSettingsMenu => settingsMenuWidth < maxSettingsMenuWidth;

        private static string canvasLocation
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

        private static string additionalKeyboardFunctionality
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

        private void SettingsMenuSetup()
        {
            App.LoadSettings();
            //Resources = CrunchStyle.Apply();

            AbsoluteLayout.SetLayoutBounds(settingsMenuButton, new Rectangle(0, 0, 50, 50));
            settingsMenuButton.SetButtonBorderWidth(0);
            settingsMenuButton.Clicked += (sender, e) => SetMenuVisibility(settingsMenu.TranslationX < 0);

            decimalPrecisionStepper.Value = Settings.DecimalPlaces;
            SetDecimalPrecision(null, new ValueChangedEventArgs(0, Settings.DecimalPlaces));

            fractiondecimal.View = new Toggle("Numerical values:", (int)Settings.Numbers, Enum.GetNames(typeof(Numbers)));
            (fractiondecimal.View as Toggle).Toggled += (selected) => Settings.Numbers = (Numbers)selected;

            //factoredexpanded.View = new Toggle("Polynomials:", Enum.GetNames(typeof(Crunch.Engine.Polynomials)));

            degrad.View = new Toggle("Trigonometry:", (int)Settings.Trigonometry, Enum.GetNames(typeof(Trigonometry)));
            (degrad.View as Toggle).Toggled += (selected) => Settings.Trigonometry = (Trigonometry)selected;

            clearCanvasWarningSwitch.On = Settings.ClearCanvasWarning;

            tips.Clicked += (sender, e) => DisplayAlert("Tips",
                "A few tips about how to navigate the app:\n\n" + canvasLocation +
                "\n\nCrunch allows you to view your answer in multiple forms, when possible. Tap the answer to cycle through them, or in the case of degrees and radians, tap the label. The answer can also be moved on the canvas by simply touching and dragging the equals sign.\n\n" +
                "There is also additional functionality attached to the keyboard keys. Long pressing DEL will clear the canvas, and long pressing any other button gives you the ability to move the cursor.\n" + additionalKeyboardFunctionality,
                "Dismiss");

            about.Clicked += (sender, e) => DisplayAlert("About Crunch",
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
}
