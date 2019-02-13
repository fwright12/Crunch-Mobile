using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Calculator.Graphics;
using static Java.Util.ResourceBundle;

namespace Calculator
{
    public partial class MainPage
    {
        public Entry CreateCursor()
        {
            Entry temp = new Entry();
            temp.FontSize = Input.TextSize;
            //temp.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.MatchParent);
            //temp.FontSize = Input.TextSize;
            //temp.Text = "|";
            //temp.Margin = new Thickness(0, 0, 0, 0);
            //temp.SetPadding(1, 0, 1, 0);
            //temp.Background = null;
            //temp.SetCursorVisible(true);
            //temp.Gravity = GravityFlags.Center;

            //temp.ViewAttachedToWindow += delegate { temp.RequestFocus(); };

            return temp;
        }

        public BoxView CreatePhantomCursor()
        {
            BoxView temp = new BoxView();
            temp.WidthRequest = 2;
            temp.HeightRequest = 50;
            temp.BackgroundColor = Color.Red;

            return temp;
        }

        public void CheckPadding(Layout parent, bool padLeft, bool padRight)
        {
            parent.Padding = new Thickness(padLeft.ToInt() * Input.cursorWidth, parent.Padding.Top, padRight.ToInt() * Input.cursorWidth, parent.Padding.Bottom);
        }
    }
}
