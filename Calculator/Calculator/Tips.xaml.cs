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
    public partial class Tips : CarouselPage
    {
        public Tips()
        {
            //InitializeComponent();

            CurrentPageChanged += delegate
            {
                print.log("page changed to " + Children.IndexOf(CurrentPage));
            };
        }

        public async void LoadMain()
        {
            await Navigation.PushModalAsync(new MainPage());
        }
    }
}