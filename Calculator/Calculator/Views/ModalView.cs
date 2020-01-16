using System;
using System.Collections.Generic;
using System.Text;

using Xamarin.Forms;
using Xamarin.Forms.Extensions;

namespace Calculator
{
    public class ModalView : Frame
    {
        private readonly int MAX_TUTORIAL_SIZE_ABSOLUTE = 400;
        private readonly double MAX_TUTORIAL_SIZE_PERCENT = 0.75;

        private Color Background = Color.White;

        public ModalView()
        {
            //BorderColor = Background;
            //BackgroundColor = Background;
            CornerRadius = 10;
            //VerticalOptions = LayoutOptions.FillAndExpand;

            /*PopUp.WhenPropertyChanged(Frame.IsVisibleProperty, (sender, e) =>
            {
                if (IsVisible)
                {
                    PhantomCursorField.Children.Add(PopUp, new Rectangle(0.5, 0.5, -1, -1), AbsoluteLayoutFlags.PositionProportional);
                }
                else
                {
                    PopUp.Remove();
                    PhantomCursorField.SizeChanged -= sizing;
                }
            });*/
        }

        private Layout<View> ParentLayout;

        protected override void OnParentSet()
        {
            base.OnParentSet();

            if (ParentLayout != null)
            {
                EnableCurrentChildren(ParentLayout, true);
                ParentLayout.ChildAdded -= DisableFutureChildren;
            }

            ParentLayout = Parent as Layout<View>;

            if (ParentLayout != null)
            {
                EnableCurrentChildren(ParentLayout, false);
                ParentLayout.ChildAdded += DisableFutureChildren;
            }
        }

        private void DisableFutureChildren(object sender, ElementEventArgs e)
        {
            if (e.Element == this || !(e.Element is View view))
            {
                return;
            }

            view.IsEnabled = false;
        }

        private void EnableCurrentChildren(Layout<View> parent, bool enabled)
        {
            foreach (View view in parent.Children)
            {
                if (view == this)
                {
                    continue;
                }

                view.IsEnabled = enabled;
            }
        }

        protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
        {
            //return base.OnMeasure(widthConstraint, heightConstraint);

            //widthConstraint = (Parent as View)?.Width ?? widthConstraint;
            //heightConstraint = (Parent as View)?.Height ?? heightConstraint;
            
            widthConstraint = Math.Min(MAX_TUTORIAL_SIZE_ABSOLUTE, MAX_TUTORIAL_SIZE_PERCENT * widthConstraint);
            heightConstraint = Math.Min(MAX_TUTORIAL_SIZE_ABSOLUTE, MAX_TUTORIAL_SIZE_PERCENT * heightConstraint);
            Print.Log("measuring modal view", widthConstraint, heightConstraint);
            SizeRequest sr = base.OnMeasure(widthConstraint, heightConstraint);
            //SizeRequest sr = Content.Measure(widthConstraint, heightConstraint);
            sr.Request = new Size(Math.Min(widthConstraint, sr.Request.Width), Math.Min(heightConstraint, sr.Request.Height));
            //sr.Request = new Size(Math.Min(sr.Request.Width, maxWidth), Math.Min(sr.Request.Height, maxHeight));
            return sr;
        }
    }
}
