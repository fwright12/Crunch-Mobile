using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(ScrollView), typeof(Calculator.iOS.ScrollViewRenderer))]

namespace Calculator.iOS
{
    public class ScrollViewRenderer : Xamarin.Forms.Platform.iOS.ScrollViewRenderer
    {
        public ScrollViewRenderer()
        {
            new Touch(this);
            
            DraggingEnded += (sender, e) =>
            {
                if (!e.Decelerate)
                {
                    Snap();
                }
            };
            DecelerationEnded += (sender, e) => Snap();
        }

        protected override void OnElementChanged(VisualElementChangedEventArgs e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null)
            {
                e.NewElement.PropertyChanged -= HandlePropertyChanged;
                e.OldElement.RemoveBinding(ScrollViewExtensions.BouncesProperty);
            }
            if (e.NewElement != null)
            {
                e.NewElement.PropertyChanged += HandlePropertyChanged;
                e.NewElement.SetBinding(ScrollViewExtensions.BouncesProperty, this, "Bounces", mode: BindingMode.OneWayToSource);
            }
        }

        private void HandlePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {

        }

        private void Snap()
        {
            ScrollView scrollView = (ScrollView)Element;

            if (scrollView.IsPagingEnabled())
            {
                ScrollViewExtensions.GetPagingBehavior(scrollView).Snap();
            }
        }
    }
}