using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Xamarin.Forms.Extensions;

//[assembly: ExportRenderer(typeof(TouchScreen), typeof(Calculator.Droid.TouchScreenRenderer))]
[assembly: ExportRenderer(typeof(Page), typeof(Calculator.Droid.TouchScreenRenderer))]

namespace Calculator.Droid
{
    public class TouchScreenRenderer : PageRenderer
    {
        public TouchScreenRenderer(Context context) : base(context) { }

        //private void DragTouch(MotionEvent e) => Xamarin.Forms.Drag.OnTouch(this.ScaleTouch(Element, e), (TouchState)e.Action);

        public override bool OnTouchEvent(MotionEvent e)
        {
            bool wants = base.OnTouchEvent(e);
            if (Element != TouchScreen.Instance)
            {
                return wants;
            }

            //Xamarin.Forms.Drag.Screen.RelayTouch(this, e);
            if (TouchScreen.Active)
            {
                return this.RelayTouch(e, Relay);
                //return Element.RelayTouch(this, e);
                //DragTouch(e);
            }
            return wants;
        }

        /*public override bool DispatchTouchEvent(MotionEvent e)
        {
            Element.RelayTouch(this, e);
            //(Element as TouchScreen)?.OnInterceptedTouch(this.ScaleTouch(Element, e), (TouchState)e.Action);

            if (TouchScreen.Active)
            {
                OnTouchEvent(e);
                e.Action = MotionEventActions.Cancel;
                base.DispatchTouchEvent(e);
                return true;
            }
            else
            {
                return base.DispatchTouchEvent(e);
            }
        }*/

        public override bool OnInterceptTouchEvent(MotionEvent e)
        {
            bool wants = base.OnInterceptTouchEvent(e);
            if (Element != TouchScreen.Instance)
            {
                return wants;
            }

            TouchScreen.OnInterceptedTouch(this.ScaleTouch(Element, e), (TouchState)e.Action);
            //(Element as TouchScreen)?.OnInterceptedTouch(this.ScaleTouch(Element, e), (TouchState)e.Action);
            //Element.RelayTouch(this, e);
            //Xamarin.Forms.Drag.Screen.OnInterceptedTouch(this.ScaleTouch(Element, e), (TouchState)(int)e.Action);

            /*if (e.Action == MotionEventActions.Down)
            {
                //TouchScreen.LastDownEvent = this.ScaleTouch(Element, e);
                //Xamarin.Forms.Drag.OnTouch(this.ScaleTouch(Element, e), (TouchState)e.Action);
                //DragTouch(e);
                Element.RelayTouch(this, e);
            }*/
            if (TouchScreen.Active && e.Action == MotionEventActions.Up)
            {
                this.RelayTouch(e, Relay);
                //return true;
                //OnTouchEvent(e);
            }
            //return false;
            return TouchScreen.Active;
        }

        public override void RequestDisallowInterceptTouchEvent(bool disallowIntercept)
        {
            if (Element == TouchScreen.Instance)
            {
                Parent?.RequestDisallowInterceptTouchEvent(disallowIntercept);
            }
            else
            {
                base.RequestDisallowInterceptTouchEvent(disallowIntercept);
            }
        }

        private bool Relay(Point point, TouchState state)
        {
            TouchScreen.OnTouch(point, state);
            return true;
        }
    }

#if none
    //public class TouchScreenRenderer : VisualElementRenderer<Xamarin.Forms.AbsoluteLayout>
    public class TouchScreenRenderer1 : PageRenderer
    {
        public TouchScreenRenderer1(Context context) : base(context) { }

        //private void DragTouch(MotionEvent e) => Xamarin.Forms.Drag.OnTouch(this.ScaleTouch(Element, e), (TouchState)e.Action);

        public override bool OnTouchEvent(MotionEvent e)
        {
            bool wants = base.OnTouchEvent(e);

            //Xamarin.Forms.Drag.Screen.RelayTouch(this, e);
            if (TouchScreen.Active)
            {
                return this.RelayTouch(e);
                //return Element.RelayTouch(this, e);
                //DragTouch(e);
            }
            return wants;
        }

        /*public override bool DispatchTouchEvent(MotionEvent e)
        {
            Element.RelayTouch(this, e);
            //(Element as TouchScreen)?.OnInterceptedTouch(this.ScaleTouch(Element, e), (TouchState)e.Action);

            if (TouchScreen.Active)
            {
                OnTouchEvent(e);
                e.Action = MotionEventActions.Cancel;
                base.DispatchTouchEvent(e);
                return true;
            }
            else
            {
                return base.DispatchTouchEvent(e);
            }
        }*/

        public override bool OnInterceptTouchEvent(MotionEvent e)
        {
            base.OnInterceptTouchEvent(e);

            //(Element as TouchScreen)?.OnInterceptedTouch(this.ScaleTouch(Element, e), (TouchState)e.Action);
            //Element.RelayTouch(this, e);
            //Xamarin.Forms.Drag.Screen.OnInterceptedTouch(this.ScaleTouch(Element, e), (TouchState)(int)e.Action);

            /*if (e.Action == MotionEventActions.Down)
            {
                //TouchScreen.LastDownEvent = this.ScaleTouch(Element, e);
                //Xamarin.Forms.Drag.OnTouch(this.ScaleTouch(Element, e), (TouchState)e.Action);
                //DragTouch(e);
                Element.RelayTouch(this, e);
            }*/
            if (TouchScreen.Active && e.Action == MotionEventActions.Up)
            {
                this.RelayTouch(e);
                //return true;
                //OnTouchEvent(e);
            }
            //return false;
            return TouchScreen.Active;
        }

        public override void RequestDisallowInterceptTouchEvent(bool disallowIntercept)
        {
            Parent?.RequestDisallowInterceptTouchEvent(disallowIntercept);
        }
    }
#endif
}