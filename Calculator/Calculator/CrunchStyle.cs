﻿using System;
using System.Collections.Generic;
using System.Text;

using Xamarin.Forms;
using Xamarin.Forms.MathDisplay;
using Xamarin.Forms.Extensions;

namespace Calculator
{
    public static class CrunchStyle
    {
        public static readonly int PAGE_PADDING = 10;

        public static readonly Color TEXT_COLOR = Color.Gray;
        public static readonly Color BACKGROUND_COLOR = Color.FromHex("#ebeae8");

        public static readonly Color BUTTON_TEXT_COLOR = Color.Black;
        public static readonly Color BUTTON_BACKGROUND_COLOR = Color.LightGray;
        public static readonly int CORNER_RADIUS = 5;

        private static readonly Thickness BUTTON_PADDING = Device.RuntimePlatform == Device.iOS ? new Thickness(10, 0, 10, 0) : new Thickness(0);

        public static ResourceDictionary Apply()
        {
            ResourceDictionary resources = new ResourceDictionary();

            //Buttons
            resources.Add(new Style(typeof(Button))
            {
                ApplyToDerivedTypes = true,
                Setters =
                {
                    new Setter { Property = Button.TextColorProperty, Value = BUTTON_TEXT_COLOR },
                    new Setter { Property = Button.BackgroundColorProperty, Value = BUTTON_BACKGROUND_COLOR },
                    new Setter { Property = Button.CornerRadiusProperty, Value = CORNER_RADIUS },
                    new Setter { Property = Button.PaddingProperty, Value = BUTTON_PADDING }
                }
            });

            //Text
            resources.Add(new Style(typeof(Text))
            {
                ApplyToDerivedTypes = true,
                Setters =
                {
                    new Setter { Property = Label.TextColorProperty, Value = TEXT_COLOR },
                }
            });

            //Pages
            resources.Add(new Style(typeof(ContentPage))
            {
                ApplyToDerivedTypes = true,
                Behaviors =
                {
                    new PageBehavior()
                },
                Setters =
                {
                    new Setter { Property = VisualElement.BackgroundColorProperty, Value = BACKGROUND_COLOR },
                }
            });

            //Canvas
            resources.Add(new Style(typeof(Canvas))
            {
                Setters =
                {
                    new Setter { Property = VisualElement.BackgroundColorProperty, Value = BACKGROUND_COLOR },
                }
            });

            //Cursor
            resources.Add(new Style(typeof(CursorView))
            {
                Setters =
                {
                    new Setter { Property = VisualElement.BackgroundColorProperty, Value = TEXT_COLOR },
                }
            });

            return resources;
        }

        private class PageBehavior : Behavior<Page>
        {
            protected override void OnAttachedTo(Page bindable)
            {
                base.OnAttachedTo(bindable);

                if (!(bindable is SettingsPage) && !(bindable is PrivacyPolicyPage))
                {
                    bindable.LayoutChanged += (sender, e) => SetPadding(sender as Page);
                    bindable.Appearing += (sender, e) => SetPadding(sender as Page);
                }
            }

            private void SetPadding(Page page)
            {
                var safeInsets = Xamarin.Forms.PlatformConfiguration.iOSSpecific.Page.SafeAreaInsets(page.On<Xamarin.Forms.PlatformConfiguration.iOS>());
                safeInsets = new Thickness(
                    Math.Max(PAGE_PADDING, safeInsets.Left),
                    Math.Max(PAGE_PADDING, safeInsets.Top),
                    Math.Max(PAGE_PADDING, safeInsets.Right),
                    Math.Max(PAGE_PADDING, safeInsets.Bottom)
                    );
                page.Padding = safeInsets;
            }
        }
    }
}