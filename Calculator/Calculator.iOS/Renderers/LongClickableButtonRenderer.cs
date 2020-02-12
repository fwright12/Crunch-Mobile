using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using Xamarin.Forms.Extensions;

[assembly: ExportRenderer(typeof(LongClickableButton), typeof(Calculator.iOS.LongClickableButtonRenderer))]

namespace Calculator.iOS
{
    public class LongClickableButtonRenderer : ButtonRenderer
    {
        //UIPanGestureRecognizer pan = new UIPanGestureRecognizer();

        public LongClickableButtonRenderer()
        {
            UIPanGestureRecognizer pgr = TouchScreenRenderer.AddDrag(this);
            pgr.CancelsTouchesInView = false;
            pgr.ShouldRecognizeSimultaneously = (a, b) => false;

            UILongPressGestureRecognizer longPress = new UILongPressGestureRecognizer
            {
                //CancelsTouchesInView = false,
            };
            longPress.AddTarget(() =>
            {
                if (longPress.State == UIGestureRecognizerState.Began && Element is LongClickableButton button)
                {
                    button.IsLongPressed = true;
                    button.OnLongClick();
                }
                else if (longPress.State == UIGestureRecognizerState.Ended)
                {
                    if (Element is LongClickableButton button1)
                    {
                        button1.IsLongPressed = false;
                    }
                    //(Element as LongClickableButton)?.OnTouch()
                    this.RelayTouch(longPress);
                }
                /*else if (longPress.State == UIGestureRecognizerState.Ended)
                {
                    //TouchScreenRenderer.RelayDrag(longPress);
                }*/
            });
            //longPress.ShouldRecognizeSimultaneously = (gesture1, gesture2) => gesture2 == TouchScreenRenderer.Pan || gesture2 == pan;
            AddGestureRecognizer(longPress);
        }

        /*protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
        {
            base.OnElementChanged(e);

            if (Element != null)
            {
                Element parent = Element;
                while (parent != null && !((parent = parent.Parent) is ScrollView)) { }

                //if (Element is LongClickableButton && (Element as LongClickableButton).Draggable)
                if (parent == null)
                {
                    //TouchScreenRenderer.AddDrag(this);
                    //pan.ShouldRecognizeSimultaneously = (a, b) => !Drag.Active;
                    /*pan.AddTarget(() =>
                    {
                        if (TouchScreen.Active)
                        {
                            TouchScreenRenderer.RelayDrag(pan);
                        }
                        else if (pan.State == UIGestureRecognizerState.Began)
                        {
                            Element.RelayTouch(this, pan.LocationInView(this), TouchState.Moving);
                        }
                    });
                    AddGestureRecognizer(pan);
                }
            }
        }*/
    }
}