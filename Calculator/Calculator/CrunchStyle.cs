using System;
using System.Collections.Generic;
using System.Text;

using Xamarin.Forms;
using Xamarin.Forms.Math;

namespace Calculator
{
    public static class CrunchStyle
    {
        private static readonly Color TextColor = Color.Red;

        public static ResourceDictionary Apply()
        {
            ResourceDictionary resources = new ResourceDictionary();

            /*resources.Add(new Style(typeof(Text))
            {
                Setters =
                {
                    new Setter { Property = Label.TextColorProperty, Value = TextColor }
                }
            });*/

            return resources;
        }
    }
}
