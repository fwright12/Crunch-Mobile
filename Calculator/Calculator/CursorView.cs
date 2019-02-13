using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Calculator;

namespace Crunch.GraFX
{
    public class CursorView : BoxView
    {
        public new Expression Parent => base.Parent as Expression;

        public CursorView()
        {
            WidthRequest = 1;
            VerticalOptions = LayoutOptions.Center;
        }

        public override string ToString()
        {
            return "";
        }
    }
}
