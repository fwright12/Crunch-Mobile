using System;
using System.Collections.Generic;
using System.Text;

using Xamarin.Forms;

namespace Crunch.GraFX
{
    public class Text : Label
    {
        public bool Selectable = false;

        public new Expression Parent
        {
            get { return base.Parent as Expression; }
        }

        public Text()
        {
            VerticalOptions = LayoutOptions.Center;
            HorizontalOptions = LayoutOptions.Center;
        }

        public Text(string text) : this()
        {
            //Text = " " + text + " ";
            FontSize = Calculator.MainPage.fontSize;
            Text = text;
        }

        public bool IsOperand()
        {
            return Text == " + " || Text == " × " || Text == " - ";
        }

        public override string ToString()
        {
            return Text;
        }
    }
}
