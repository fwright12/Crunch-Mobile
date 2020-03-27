using System;
using System.Collections.Generic;
using System.Text;

using Xamarin.Forms;
using Xamarin.Forms.MathDisplay;
using Xamarin.Forms.Extensions;

namespace Calculator
{
    public class CrunchStyle : ResourceDictionary
    {
        public CrunchStyle()
        {
            //Buttons
            Add(new Style(typeof(Button))
            {
                ApplyToDerivedTypes = true,
                Setters =
                {
                    new Setter { Property = Button.TextColorProperty, Value = App.BUTTON_TEXT_COLOR },
                    new Setter { Property = Button.BackgroundColorProperty, Value = App.BUTTON_BACKGROUND_COLOR },
                    new Setter { Property = Button.CornerRadiusProperty, Value = App.CORNER_RADIUS },
                    new Setter { Property = Button.PaddingProperty, Value = App.BUTTON_PADDING }
                }
            });

            //Text
            Add(new Style(typeof(Text))
            {
                ApplyToDerivedTypes = true,
                Setters =
                {
                    new Setter { Property = Label.TextColorProperty, Value = App.TEXT_COLOR }
                }
            });
            
            //Pages
            Add(new Style(typeof(ContentPage))
            {
                ApplyToDerivedTypes = true,
                Behaviors =
                {
                    new BehaviorFunc<ContentPage>((page) =>
                    {
                        if (!(page is SettingsPage))
                        {
                            //page.LayoutChanged += (sender, e) => SetPadding(sender as Page);
                            //page.Appearing += (sender, e) => SetPadding(sender as Page);
                        }
                    })
                },
                Setters =
                {
                    new Setter { Property = VisualElement.BackgroundColorProperty, Value = App.BACKGROUND_COLOR }
                }
            });

            //Canvas
            Add(new Style(typeof(Canvas))
            {
                ApplyToDerivedTypes = true,
                Setters =
                {
                    new Setter { Property = VisualElement.BackgroundColorProperty, Value = App.BACKGROUND_COLOR }
                }
            });

            //Cursor
            Add(new Style(typeof(CursorView))
            {
                ApplyToDerivedTypes = true,
                Setters =
                {
                    new Setter { Property = VisualElement.BackgroundColorProperty, Value = App.TEXT_COLOR }
                }
            });
        }

        private void SetPadding(Page page)
        {
            var safeInsets = Xamarin.Forms.PlatformConfiguration.iOSSpecific.Page.SafeAreaInsets(page.On<Xamarin.Forms.PlatformConfiguration.iOS>());
            safeInsets = new Thickness(
                Math.Max(App.PAGE_PADDING, safeInsets.Left),
                Math.Max(App.PAGE_PADDING, safeInsets.Top),
                Math.Max(App.PAGE_PADDING, safeInsets.Right),
                Math.Max(App.PAGE_PADDING, safeInsets.Bottom)
                );
            page.Padding = safeInsets;
        }
    }
}
