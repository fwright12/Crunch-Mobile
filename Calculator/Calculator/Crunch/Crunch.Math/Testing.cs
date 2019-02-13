using System;
using System.Collections.Generic;
using System.Text;

using Xamarin.Forms;

namespace Calculator
{
    public static class Testing
    {
        public static bool ShouldTest = false;

        static Testing()
        {
            //ShouldTest = true;
        }

        public static void Test(Layout<View> canvas)
        {
            List<string> testcases = new List<string>();

            bool other = false;
            bool trig = false;
            bool negative = false;
            bool simplify = false;
            bool division = false;
            bool multiplication = false;
            bool addition = false;
            bool cancel = false;
            bool advanced = false;

            //other = true;
            //trig = true;
            //negative = true;
            //simplify = true;
            //division = true;
            //multiplication = true;
            //addition = true;
            //cancel = true;
            //advanced = true;

            /*testcases.Add("1/(2^2+5/3)");
            testcases.Add("1/(1^(2^2^2+1^2))");
            testcases.Add("5/(4+e^2)");*/

            if (other)
            {
                testcases.Add("3+476576/9878-56^2876");
                testcases.Add("6-(3/2+4^(7-5))/(8^2*4)+2^(2+3)");
                testcases.Add("8/8/8+2^2^2");
                testcases.Add("6+(8-3)*3/(9/5)+2^2");
            }

            if (trig)
            {
                testcases.Add("sin(30)");
                testcases.Add("sin30");
                testcases.Add("sin5/4");
                testcases.Add("sin(30)");
                testcases.Add("si(30)");
                testcases.Add("s(30)");
                testcases.Add("in(30)");
                testcases.Add("sin(30)+cos(30)");
                testcases.Add("sin(cos(60))");
                testcases.Add("cossin60");
                testcases.Add("5(x+1)");
                testcases.Add("(x+1)5");
                testcases.Add("6sin30");
                testcases.Add("5/4sin30");
                testcases.Add("sin30cos30");
                testcases.Add("esin30");
                testcases.Add("e^2sin30+cos30e^2");
            }

            if (negative)
            {
                testcases.Add("x-5");
                testcases.Add("-9*6");
                testcases.Add("-6*x");
                testcases.Add("-1*x");
                testcases.Add("-1");
                testcases.Add("-7");
                testcases.Add("6+-9");
                testcases.Add("6*-(1+2)");
                testcases.Add("-(1+2)");
                testcases.Add("6/-9");
                testcases.Add("(6+6)-9");
                testcases.Add("2*(6+6)-9");
                testcases.Add("(-5)");
                testcases.Add("2^-3");
                testcases.Add("2^-1^2");
                testcases.Add("-1--4/5+2^-2+4/-5+5*-6");
                testcases.Add("x^2+-6*x^2");
                testcases.Add("x^2+-1*x");
                testcases.Add("x^2--1*x");
            }

            if (simplify)
            {
                testcases.Add("2^2");
                testcases.Add("7^24x*8");
                testcases.Add("e");
                testcases.Add("e*e");
                testcases.Add("e*π");
                testcases.Add("e+e");
                testcases.Add("e+π");
                testcases.Add("e^2");
                testcases.Add("e^2+π");
                testcases.Add("e^π+3");
                testcases.Add("e^(2+π)");
                testcases.Add("eπ+2");
                testcases.Add("5.3e");
                testcases.Add("5xe^2+4x^3e");
                testcases.Add("5x^3e^2+4x^3e");
            }

            if (division)
            {
                testcases.Add("5/8");
                testcases.Add("6/8");
                testcases.Add("8/2");
                testcases.Add("(-5)/8");
                testcases.Add("(-6)/8");
                testcases.Add("(-8)/2");
                testcases.Add("5/(-8)");
                testcases.Add("6/(-8)");
                testcases.Add("8/(-2)");
                testcases.Add("(-5)/(-8)");
                testcases.Add("(-6)/(-8)");
                testcases.Add("(-8)/(-2)");
                testcases.Add("8/3/7");
                testcases.Add("8/(3/7)");
                testcases.Add("9/2/7/5");
                testcases.Add("(9/2)/(7/5)");

                testcases.Add("5*8/3");
                testcases.Add("5*8/3.5");
                testcases.Add("5.5*8/3");
                testcases.Add("5/e");
                testcases.Add("e/3");
                testcases.Add("e/e");
                testcases.Add("e/π");
            }

            if (multiplication)
            {
                testcases.Add("6*8");
                testcases.Add("6*8/5");
                testcases.Add("x*6");
                testcases.Add("x*x");
                testcases.Add("x*y");
                testcases.Add("yyy");
                testcases.Add("6x*3");
                testcases.Add("6x*3x");
                testcases.Add("6x*y");
                testcases.Add("6x*3y");
                testcases.Add("6x*1/2y*z^2");
                testcases.Add("x^2*x");
                testcases.Add("6x^2*y*5x");
                testcases.Add("6x^2*3x^5*y^7");
                testcases.Add("6x*y/z");
                testcases.Add("x*1/z");
                testcases.Add("(x+1)5");
                testcases.Add("5(x+1)");
                testcases.Add("(x+1)*x");
                testcases.Add("(x+1)*6x");
                testcases.Add("(x+1)*(x+2)");
                testcases.Add("(x+1)^2*(x+1)");
                testcases.Add("(x+1)^y*(x+1)^2y");
                testcases.Add("(x+1)*5/6");
                testcases.Add("(x+1)*y/z");
            }

            if (addition)
            {
                testcases.Add("5+8");
                testcases.Add("5+8/3");
                testcases.Add("5/3+8/9");
                testcases.Add("5/3+8/7");
                testcases.Add("5+x/y");
                testcases.Add("5+x");
                testcases.Add("5/2+x");
                testcases.Add("5+x^2");
                testcases.Add("5+6x^2");
                testcases.Add("5+(x+6)");
                testcases.Add("5+(7/3+x^2)");
                testcases.Add("5+(x^2+x)");
                testcases.Add("x+x");
                testcases.Add("x+y");
                testcases.Add("x+(x^2+x)");
                testcases.Add("x+y/z");
                testcases.Add("5x+x");
                testcases.Add("5x+8x");
                testcases.Add("5x+8y");
                testcases.Add("5xy^2+8/3y^2x");
                testcases.Add("(x+1)+(y^2+4x+2)");
                testcases.Add("x/y+y/z");
                testcases.Add("x/y+z/y");
                testcases.Add("(x+1)/2+(x+1)/2");
                testcases.Add("(x+1)/2+(1-x)/2");
                testcases.Add("(x+1)/2+(-x-1)/2");
                testcases.Add("(x+1)/2+(-x)/2");
            }

            if (cancel)
            {
                testcases.Add("6/x");
                testcases.Add("6/(6x)");
                testcases.Add("6/(5x^2)");
                testcases.Add("6/(5x+3y)");
                testcases.Add("x/6");
                testcases.Add("x/y");
                testcases.Add("x/x");
                testcases.Add("x/(6x)");
                testcases.Add("3/(6x)");
                testcases.Add("x/(5x+3x^2)");
                testcases.Add("x/(6x+3x^2)");
                testcases.Add("(6x)/6");
                testcases.Add("(6x)/x");
                testcases.Add("(6x)/(6x)");
                testcases.Add("(6x)/(5x)");
                testcases.Add("(6x)/(6y)");
                testcases.Add("(6x)/(5y)");
                testcases.Add("(6x)/(5x+3y)");
                testcases.Add("(6x)/(5x+3x^2)");
                testcases.Add("(6x+3)/6");
                testcases.Add("(6x+3)/x");
                testcases.Add("(6x+3)/(3x)");
                testcases.Add("(6x+3)/(6x+3)");
                testcases.Add("(6x+3)/(6x+2)");
                testcases.Add("(x^3+x+2)*(x^2+x+3)");
            }

            if (advanced)
            {
                testcases.Add("(e+π)/(π+e)");
                testcases.Add("(x+y)/(y+x)");
                testcases.Add("(5^(x+y))/(5^x+6*5^(x+y))");
                testcases.Add("(5^(x+1))/(5^x+9*5^(x+1))");
                testcases.Add("(x^(3+x))/(x^(2+x)+6x^4)");
            }

            //print.log(";lakjsdflk;jasld;kfj;alskdfj", ((string)new Text()) == null);
            /*print.log(Crunch.Parse.Math("^3234+4^(6-85^"));
            print.log(Crunch.Parse.Math("(3)+(9^(4*(8)+1^5"));
            print.log(Crunch.Parse.Math("((1^3)-1^9)+(43^2*(6-9))-8+(234^4*(7))"));
            print.log(Crunch.Parse.Math("3^1^2^3^4-95)+7)+(4*(6-9))-8+(4*17)"));
            print.log(Crunch.Parse.Math("(4+3()2+1^8^0)(4)+(6)"));
            print.log(Crunch.Parse.Math("()4+()*8()"));
            throw new Exception();*/

            int num = testcases.Count;
            int cutoff = 10;
            for (int i = 0; i < num; i++)
            {
                string s = testcases[i];
                if (s != "")
                {
                    var temp = new Crunch.GraFX.Equation(s);
                    canvas.Children.Add(temp);
                    temp.TranslationY = 50 + 100 * (i - cutoff * (i / cutoff));
                    temp.TranslationX = 400 * (i / cutoff);
                }
            }
            canvas.HeightRequest = 50 + num * 100;
            canvas.WidthRequest = 1500;
        }
    }
}
