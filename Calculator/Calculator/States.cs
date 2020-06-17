using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Calculator.States
{
    [ContentProperty(nameof(Groups))]
    public class All : IMarkupExtension
    {
        public IList<string> Groups { get; set; } = new List<string>();

        /*public MainPage MainPage { get; set; }
        public CrunchKeyboard CrunchKeyboard { get; set; }
        public FunctionsDrawer FunctionsDrawer { get; set; }*/

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            throw new NotImplementedException();
        }
    }

    [InitialState(nameof(Portrait))]
    public class MainPage : VisualStateValuesExtension
    {
        public object Portrait { get; set; }
        public object Landscape { get; set; }
        public object Basic { get; set; }
    }

    [InitialState(nameof(Portrait))]
    public class CrunchKeyboard : VisualStateValuesExtension // MainPage
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
