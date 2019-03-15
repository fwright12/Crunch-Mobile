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
	public partial class AboutPage : ContentPage
	{
		public AboutPage ()
		{
			InitializeComponent ();

            foreach (Label label in main.Children)
            {
                label.HorizontalOptions = LayoutOptions.CenterAndExpand;
                label.FontSize = MainPage.FontSize;
            }
		}
    }
}