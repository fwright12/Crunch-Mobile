using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using Xamarin.Forms.Extensions;
using Calculator;
using CoreMidi;

[assembly: ExportRenderer(typeof(Canvas), typeof(Calculator.iOS.CanvasRenderer))]

namespace Calculator.iOS
{
    public class CanvasRenderer : VisualElementRenderer<AbsoluteLayout>
    {
        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);
            this.RelayTouch(touches, TouchState.Up);
        }
    }
}