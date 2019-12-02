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

[assembly: ExportRenderer(typeof(TouchableStackLayout), typeof(Calculator.Droid.TouchableStackLayoutRenderer))]

namespace Calculator.Droid
{
    public class TouchableStackLayoutRenderer : VisualElementRenderer<StackLayout>
    {
        public TouchableStackLayoutRenderer(Context context) : base(context) { }

        public override bool OnTouchEvent(MotionEvent e)
        {
            bool wants = base.OnTouchEvent(e);
            //Print.Log("touch", wants, e.Action, "intercepting: " + (Element is TouchableStackLayout && (Element as TouchableStackLayout).ShouldIntercept), Element, Element?.GetType());
            
            //print.log("touch event", e.Action, Element is TouchableStacLayout && (Element as TouchableStackLayout).ShouldIntercept, test);
            if (Element is TouchableStackLayout && (Element as TouchableStackLayout).ShouldIntercept)
            {
                return this.RelayTouch(e);
                //return Element.RelayTouch(this, e);
            }
            else
            {
                return wants;
            }
        }

        public override bool OnInterceptTouchEvent(MotionEvent ev)
        {
            bool wants = base.OnInterceptTouchEvent(ev);

            if (Element is TouchableStackLayout && (Element as TouchableStackLayout).ShouldIntercept)
            {
                if (ev.Action == MotionEventActions.Move)
                {
                    return true;
                }
                else
                {
                    Parent?.RequestDisallowInterceptTouchEvent(ev.Action == MotionEventActions.Down);
                }
            }

            return wants;
        }
    }
}