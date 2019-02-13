using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace Calculator.Graphics
{
    public class Text : Symbol
    {
        public string text;

        public Text(string str, bool selectable = true)
        {
            text = str;
            this.selectable = selectable;
        }

        public override View Render()
        {
            var temp = new Label();
            //temp.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.MatchParent);
            temp.FontSize = Input.TextSize;
            temp.Text = " " + text + " ";
            temp.VerticalOptions = LayoutOptions.CenterAndExpand;
            temp.HorizontalOptions = LayoutOptions.CenterAndExpand;
            //temp.VerticalTextAlignment = TextAlignment.Center;
            //temp.HorizontalTextAlignment = TextAlignment.Center;
            //temp.BackgroundColor = Color.White;
            return temp;
        }

        public bool IsOperand()
        {
            return text == "+" || text == "*" || text == "-";
        }
    }
}
