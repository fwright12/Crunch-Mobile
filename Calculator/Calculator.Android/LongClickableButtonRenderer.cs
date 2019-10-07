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

[assembly: ExportRenderer(typeof(Xamarin.Forms.Button), typeof(Calculator.Droid.LongClickableButtonRenderer))]

namespace Calculator.Droid
{
    public class LongClickableButtonRenderer : ButtonRenderer
    {
        public LongClickableButtonRenderer(Context context) : base(context) { }

        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.Button> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                //Control.SetMinWidth(0);
                //Control.SetMinimumWidth(0);
                Control.SetAllCaps(false);
                Control.LongClick += (sender, args) => (Element as LongClickableButton)?.OnLongClick();
                /*Control.Touch += (sender, args) =>
                {
                    Print.Log("button touched", args.Event.Action);
                };*/
            }
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            //Print.Log("touch event", e.Action);
            base.OnTouchEvent(e);
            return this.RelayTouch(e);
        }

        public override bool OnInterceptTouchEvent(MotionEvent e)
        {
            /*if (Element is LongClickableButton && (Element as LongClickableButton).ShouldIntercept)
            {
                return true;
            }
            else
            {
                return base.OnInterceptTouchEvent(e);
            }*/

            //return base.OnInterceptTouchEvent(e);

            if (e.Action == MotionEventActions.Move)
            {
                return OnTouchEvent(e);
            }
            else
            {
                this.RelayTouch(e);
            }

            return false;
        }
    }
}