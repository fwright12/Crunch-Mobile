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
    public partial class TipDialog : ModalView
    {
        public string Explanation
        {
            get => ExplanationLabel.Text;
            set => ExplanationLabel.Text = value;
        }

        public string URL
        {
            get => (GIF.Source as UriImageSource)?.Uri.ToString() ?? GIF.Source.ToString();
            set => GIF.Source = new UriImageSource { CachingEnabled = false, Uri = new Uri(value) };
        }

        public TipDialog()
        {
            InitializeComponent();

            TapDismiss.Tapped += (sender, e) => this.Remove();
        }
    }
}