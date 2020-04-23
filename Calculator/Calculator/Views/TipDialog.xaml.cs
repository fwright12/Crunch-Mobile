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
            get => (GIF.Image.Source as UriImageSource)?.Uri.ToString() ?? GIF.Image.Source.ToString();
            set => GIF.Image.Source = new UriImageSource { CachingEnabled = false, Uri = new Uri(value) };
        }

        public TipDialog()
        {
            InitializeComponent();

            GIF.ErrorLabel.Text = "Hmm...having trouble loading this image\n\nTap to try again,\nor head over to settings to view this tip and many others!";
            //GIF.ErrorLabel.Text += "\n\n(all tips can also be viewed in settings)";
            DismissButton.Clicked += (sender, e) =>
            {
                //ContentPage page = this.Root<ContentPage>();
                //View content = page.Content;
                this.Remove();
                //page.Content = null;
                //page.Content = content;
            };
        }
    }

    public class FFImageLoadingImage : FFImageLoading.Forms.CachedImage, IImage
    {
        public event EventHandler<EventArgs<bool>> Loaded;

        public View View => this;

        public FFImageLoadingImage()
        {
            Success += (sender, e) => Device.BeginInvokeOnMainThread(() => Loaded?.Invoke(this, new EventArgs<bool>(true)));
            Error += (sender, e) => Device.BeginInvokeOnMainThread(() => Loaded?.Invoke(this, new EventArgs<bool>(false)));
        }
    }
}