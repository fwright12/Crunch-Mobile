using System;
using System.Collections.Generic;
using System.Text;

using Xamarin.Forms;
 
namespace Crunch.GraFX
{
    public class Text : Label, ITouchable
    {
        public TouchEventHandler Touch;

        public new Expression Parent
        {
            get { return base.Parent as Expression; }
        }

        public Text(bool isVisible = true)
        {
            VerticalOptions = LayoutOptions.Center;
            HorizontalOptions = LayoutOptions.Center;
            IsVisible = isVisible;
        }

        public Text(string text) : this()
        {
            Text = text;
        }

        public void OnTouch(Point point, TouchState state) => Touch?.Invoke(point, state);

        public override string ToString()
        {
            return Text.Simple();
        }
    }
}
