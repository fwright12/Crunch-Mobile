using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Calculator.States
{
    [InitialState(nameof(Portrait))]
    public class MainPage : VisualStateValuesExtension
    {
        public object Portrait { get; set; }
        public object Landscape { get; set; }
        public object Basic { get; set; }
    }

    [InitialState(nameof(Portrait))]
    public class CrunchKeyboard : VisualStateValuesExtension
    {
        public object Portrait { get; set; }
        public object Landscape { get; set; }
        public object Basic { get; set; }
        public object Expanded { get; set; }
    }

    [InitialState(nameof(Closed))]
    public class FunctionsDrawer : VisualStateValuesExtension
    {
        public object Open { get; set; }
        public object Closed { get; set; }
    }
}
