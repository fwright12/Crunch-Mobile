using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculator
{
    public static class Input
    {
        public static Graphics UI;
        public static int pos;

        public static bool editing = false;

        public static bool IsNumber(string s)
        {
            try
            {
                double.Parse(s);
                return true;
            }
            catch
            {
                return false;
            }

            char charAt = s[0];

            if (charAt == 46 || (charAt >= 48 && charAt <= 57))
                return true;
            //else //if (charAt == 42 || charAt == 43 || charAt == 45 || charAt == 47 || charAt == 94)

            return false;
        }

        public static void Key(string s)
        {
            switch (s)
            {
                case "exit edit mode":
                    if (!editing)
                        return;
                    
                    editing = false;
                    break;
                case "/":
                    UI.expression.Insert(new Fraction(UI.expression.prev("+", "-", "*", "/"), new Expression()));
                    break;
                case "+":
                case "-":
                case "*":
                case "^":
                    UI.expression.Insert(new Operand(s));
                    break;
                default:
                    //UI.expression.Insert(new Number(s));
                    if (!editing)
                    {
                        UI.expression.Insert(new Number(double.Parse(s)));
                        //UI.views.Insert(pos, editing);
                        editing = true;
                    }
                    else
                    {
                        UI.expression.parts[pos] = new Number(double.Parse(s));

                        //UI.views[UI.views.IndexOf(editing)] = editing;
                    }
                    print.log("SDFSDFSDF  " + s);
                    //boxedInput = new Number(double.Parse(s));
                    UI.SetAnswer();

                    return;
            }

            pos++;
            UI.Update();
        }

        public static Graphics Touch(object layout)
        {
            pos = 0;
            UI = new Graphics(layout);
            return UI;
        }
    }
}
