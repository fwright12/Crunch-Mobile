using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculator
{
    public class Format
    {
        public static readonly Format Exponent = new Format(padding: 75);
        public static readonly Format Fraction = new Format(orientation: "vertical");
        public static string os;

        public string Orientation;
        public int Padding;
        public bool IsAnswer;
        public string Gravity;

        public Format(string orientation = "horizontal", int padding = 0, bool isAnswer = false, string gravity = "center")
        {
            Orientation = orientation;
            Padding = padding;
            IsAnswer = isAnswer;
            Gravity = gravity;
        }
    }

    public class Orientation
    {
        public static dynamic Horizontal
        {
            get
            {
                if (Format.os == "android")
                {
                    return Android.Widget.Orientation.Horizontal;
                }
                else
                {
                    throw new PlatformNotSupportedException();
                }
            }
        }
    }
}
