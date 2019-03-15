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
	public partial class PrivacyPolicyPage : ContentPage
	{
		public PrivacyPolicyPage ()
		{
			InitializeComponent ();

            /*foreach (Label label in main.Children)
            {
                label.HorizontalOptions = LayoutOptions.CenterAndExpand;
                label.FontSize = MainPage.FontSize;
            }*/

            TapGestureRecognizer a = new TapGestureRecognizer();
            googleAds.Tapped += (sender, e) => Device.OpenUri(new Uri("https://support.google.com/ads/troubleshooter/1631343?visit_id=636863027008175393-2869351973&rd=1"));
            //googleAds.GestureRecognizers.Add(a);
            
            TapGestureRecognizer b = new TapGestureRecognizer();
            googlePrivacy.Tapped += (sender, e) => Device.OpenUri(new Uri("https://safety.google/privacy/ads-and-data/?hl=en"));
            //googlePrivacy.GestureRecognizers.Add(b);
        }
	}

    public class ParagraphCell : ViewCell
    {
        public static readonly BindableProperty TextProperty = BindableProperty.Create("Text", typeof(string), typeof(ParagraphCell), propertyChanged: (bindable, old, value) => (bindable as ParagraphCell).Paragraph.Text = value.ToString());

        public string Text { get; set; }

        private Label Paragraph;

        public ParagraphCell()
        {
            View = Paragraph = new Label { };
            Paragraph.Margin = new Thickness(10);
        }
    }

}