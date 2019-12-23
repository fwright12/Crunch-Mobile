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

[assembly: ExportRenderer(typeof(Canvas), typeof(Calculator.iOS.CanvasRenderer))]
//[assembly: ExportRenderer(typeof(MainPage.TestImage), typeof(Calculator.iOS.GIFRenderer))]

namespace Calculator.iOS
{
    public class GIFRenderer : UIImageView
    {
        public GIFRenderer()
        {
            Image = new UIImage("https://static.wixstatic.com/media/4a4e50_d9c8bdf752ca42c5abec19413cb4fc3b~mv2.gif");
            AnimationsEnabled = true;
        }
    }

    public class CanvasRenderer : VisualElementRenderer<AbsoluteLayout>
    {
        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);
            this.RelayTouch(touches, TouchState.Up);
        }
    }
}