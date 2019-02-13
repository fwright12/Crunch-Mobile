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
        public static int Index { get => index; }

        private static int index;

        public CursorView()
        {
            Color = Color.Gray;
            WidthRequest = 1;
            VerticalOptions = LayoutOptions.Center;
        }

        public override string ToString()
        {
            return "";
        }
    }
}
