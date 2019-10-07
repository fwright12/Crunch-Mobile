using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Extensions;

namespace Calculator
{
	public class AboutPage : ContentPage
	{
        public AboutPage()
        {
            Title = "About Crunch";

            Content = new ScrollView
            {
                Content = new StackLayout
                {
                    Spacing = 20,
                    Resources = new ResourceDictionary().Populate<Label>(
                        new Setter { Property = Label.HorizontalOptionsProperty, Value = LayoutOptions.CenterAndExpand },
                        new Setter { Property = Label.FontSizeProperty, Value = MainPage.FontSize }
                        ),
                    Children =
                    {
                        new Label
                        {
                            Text = "Thank you for using Crunch!",
                            HorizontalTextAlignment = TextAlignment.Center
                        },
                        new Label
                        {
                            Text = "If you find any bugs, please report them to GreenMountainLabs802@gmail.com. The more information you can provide (what you did to cause the error, screenshots, etc.) the easier it will be to fix."
                        },
                        new Label
                        {
                            Text = "If you enjoy using Crunch, please rate it on the app store. Ratings help with visibility, so other people can find Crunch."
                        },
                        new Label
                        {
                            Text = "Please also email me with any ideas you have about how the app can be improved, or features you would like to see in the future. I'm open to suggestions!"
                        }
                    }
                },
                Orientation = ScrollOrientation.Vertical
            };
        }
    }
}