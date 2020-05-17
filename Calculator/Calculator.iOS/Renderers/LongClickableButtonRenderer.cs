using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using Xamarin.Forms.Extensions;

[assembly: ExportRenderer(typeof(Button), typeof(Calculator.iOS.ButtonRenderer))]
[assembly: ExportRenderer(typeof(LongClickableButton), typeof(Calculator.iOS.LongClickableButtonRenderer))]

namespace Calculator.iOS
{
    public class MyButton : UIButton
    {
        private IVisualElementRenderer Renderer;

        public MyButton(IVisualElementRenderer renderer) => Renderer = renderer;

        public override bool Highlighted
        {
            get => base.Highlighted;
            set
            {
                if (Renderer.Element is Button button)
                {
                    if (value)
                    {
                        BackgroundColor = button.BorderColor.ToUIColor();
                    }
                    else
                    {
                        BackgroundColor = button.BackgroundColor.ToUIColor();
                    }
                }

                base.Highlighted = value;
            }
        }
    }

    public class ButtonRenderer : Xamarin.Forms.Platform.iOS.ButtonRenderer
    {
        public ButtonRenderer() : base()
        {
            //new Touch(this);
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
        {
            /*if (Control == null)
            {
                SetNativeControl(new MyButton(this));
            }*/

            base.OnElementChanged(e);
        }
    }

    public class LongClickableButtonRenderer : ButtonRenderer
    {
        //UIPanGestureRecognizer pan = new UIPanGestureRecognizer();

        public LongClickableButtonRenderer() : base()
        {
            UIPanGestureRecognizer pgr = TouchScreenRenderer.AddDrag(this);
            pgr.CancelsTouchesInView = false;
            pgr.ShouldRecognizeSimultaneously = (a, b) => false;

            UILongPressGestureRecognizer longPress = new UILongPressGestureRecognizer
            {
                
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