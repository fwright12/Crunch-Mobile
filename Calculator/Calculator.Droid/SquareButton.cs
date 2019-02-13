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
using Android.Util;

namespace Calculator.Droid
{
    //[Register("Cacluator.Droid.SquareButton")]
    /*public class SquareButton : Button
    {
        //new public int MeasuredHeight { get { return MeasuredWidth; } }

        /*public SquareButton(Context context) : base(context) { }

        public SquareButton(Context context, IAttributeSet attrs) : base(context, attrs) { }

        public SquareButton(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr) { }

        public SquareButton(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes) { }

        new protected void Measure(int widthMeasureSpec, int heightMeasureSpec)// : base(widthMeasureSpec, widthMeasureSpec)
        {
            int width = MeasureSpec.GetSize(widthMeasureSpec);
            int height = MeasureSpec.GetSize(heightMeasureSpec);
            int size = width > height ? height : width;
            SetMeasuredDimension(size, size); // make it square
        }
    }*/
}