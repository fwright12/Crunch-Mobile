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

        public override string ToString()
        {
            return Text.Simple();
        }
    }

    public class EqualSign : Text
    {
        public event Calculator.TouchEventArgs Touch;
        public static EqualSign selected;

        public EqualSign()
        {
            Text = " = ";
            Touch += (point) =>
            {
                print.log(point);
            };
        }

        public void Touched(Point point)
        {
            Calculator.Drag.Touch = point;

            Calculator.Drag.Move = (pos) =>
            {
                Action<View, Point> move = (v, p) =>
                {
                    RelativeLayout.SetXConstraint(v, Constraint.Constant(p.X));
                    RelativeLayout.SetYConstraint(v, Constraint.Constant(p.Y));
                };

                Parent.Parent.Move(move, Calculator.MainPage.VisiblePage.Canvas.Bounds, Calculator.MainPage.ScaleToPage(pos));

                //RelativeLayout.SetXConstraint(Parent, Constraint.Constant(Parent.X + pos.X * Calculator.MainPage.Canvas.Width));
                //RelativeLayout.SetYConstraint(Parent, Constraint.Constant(Parent.Y + pos.Y * Calculator.MainPage.Canvas.Height));
            };
        }
    }
}
