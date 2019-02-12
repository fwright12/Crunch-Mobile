using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculator
{
    public class Input
    {
        public static Input selected;
        public static IGraphicsHandler graphicsHandler;
        public static int pos;
        public static List<Symbol> adding = new List<Symbol>();
        public static object layoutRoot;

        public static bool editing = false;
        public static object editField;
        public static object cursor;
        public static Edit editor;

        public List<Symbol> text = new List<Symbol>() { Symbol.Cursor };
        public MathView mathView;

        public static OnReceive state = OnReceive.add;
        public enum OnReceive { add, delete };

        private Text equalSign = new Text("=");

        public Input(MathView view)
        {
            mathView = view;
            pos = 0;

            //Parse(text);

            //SetCursor(0);

            //text.Add(Symbol.Cursor);
        }

        public static bool IsNumber(string s)
        {
            char chr = s[0];

            return (chr >= 48 && chr <= 57) || chr == 46;

            try
            {
                double.Parse(s);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsVariable(string s)
        {
            return s.Length == 1 && s[0] >= 97 && s[0] <= 122;
        }

        //Insert symbol where cursor is, indicate that it was the last added symbol
        private void Insert(Symbol sender)
        {
            Insert(pos, sender);
        }

        private void Insert(int index, Symbol sender)
        {
            text.Insert(index, sender);
            adding.Add(sender);
        }

        public static void Send(string s)
        {
            MathText.selected.Insert(Wrapper(s));
        }

        //For handling text input
        public void Key(string s)
        {
            //Check for number
            if (IsNumber(s))
            {
                //Create new edit field if one doesn't already exist
                if (!editing)
                {
                    editField = graphicsHandler.AddEditField();
                    Insert(editor = new Edit(0));
                }

                //Send keystroke
                editor.Dispatch(double.Parse(graphicsHandler.DispatchKey(s)));
            }
            else
            {
                //If the last text sent was a number, add that new number
                if (editing)
                {
                    text.Remove(editor);
                    Insert(new Number(editor.value));
                    pos++;
                }

                //Indicates no longer editing
                if (s == "exit edit mode")
                {
                    if (text.Count == 0)
                    {
                        graphicsHandler.RemoveChild(layoutRoot, mathView.root);
                    }
                    pos--;
                }
                else if (s != ")")
                {
                    print.log(pos);
                    Insert(Wrapper(s));

                    //For different possible inputs - add each using corresponding symbol
                    switch (s)
                    {
                        case "^":
                        case "/":
                            if (pos - 1 < 0 || text[pos - 1].text != ")")
                            {
                                parend(pos - 1);
                                pos += 2;
                            }
                            if (pos + 1 > text.Count - 1 || text[pos + 1].text != "(")
                            {
                                parend(pos + 1);
                            }

                            text.Insert(findMatching(text[pos + 1] as Text, text) + 1, new Text(")"));
                            text.Insert(findMatching(text[pos - 1] as Text, text), new Text("("));
                            pos++;

                            break;
                        case "(":
                            Insert(pos + 1, new Text(")"));
                            pos--;
                            break;
                        /*case "*":
                        case "+":
                        case "-":
                            Insert(new Operand(s));
                            break;
                        case "(":
                            parend(pos, 0);
                            break;
                        case ")":
                            if (text[pos].text == ")")
                                break;
                            else
                                return;*/
                        default:
                            //It's a function
                            if (s.Length > 1)
                            {
                                parend(pos, 0);
                            }
                            /*if (IsNumber(s))
                            {
                                Insert(new Number(double.Parse(s)));
                            }
                            else if (IsVariable(s))
                            {

                            }
                            if (!IsVariable(s))
                            {
                                //Insert(new Function(s));
                                parend(pos + 1, 0);
                            }*/
                            break;
                    }

                }

                do
                {
                    pos++;
                }
                while (pos < text.Count && text[pos].text == "(");

                //SetCursor(pos);
            }

            editing = IsNumber(s);

            print.log("Input text");
            foreach (Symbol a in text)
                print.log(a + ", " + a.text);

            List<Symbol> parsed = Parse(text);

            Symbol answer = new Expression(parsed).answer.Copy();
            answer.format = new Format(isAnswer: true);
            if (Expression.showDecimal)
            {
                answer = new Number((answer as Term).value);
            }

            parsed.Add(new Text("="));
            parsed.Add(answer);

            mathView.SetText(parsed);

            adding.Clear();
        }

        public void SetCursor(object view, int isToRight = 0)
        {
            pos = text.IndexOf(mathView.shown[view]) + isToRight;
        }

        public static List<Symbol> Wrap(params string[] list)
        {
            List<Symbol> result = new List<Symbol>();

            foreach (string s in list)
                result.Add(Wrapper(s));

            return result;
        }

        private static Symbol Wrapper(string s)
        {
            switch (s)
            {
                case "^":
                case "*":
                case "/":
                case "-":
                case "+":
                    return new Operand(s);
                case "(":
                case ")":
                    return new Text(s);
                default:
                    if (IsNumber(s))
                    {
                        return new Number(double.Parse(s));
                    }
                    else if (IsVariable(s))
                    {
                        return new Text(s);
                    }
                    else
                    {
                        return new Function(s);
                    }
            }
        }

        //Add parentheses around symbol at 'index'
        private void parend(int index, int isNull = 1)
        {
            text.Insert(Math.Min(index + isNull, text.Count), new Text(")"));
            text.Insert(Math.Max(index, 0), new Text("("));
        }

        private static int findMatching(Text first, List<Symbol> inList)
        {
            int dir = 1;
            if (first.text == ")")
                dir = -1;

            int count = 0;

            int index;
            for (index = inList.IndexOf(first) + dir; index < inList.Count && index > -1; index += dir)
            {
                if ((dir == 1 && inList[index].text == "(") || (dir == -1 && inList[index].text == ")"))
                {
                    count++;
                }
                if ((dir == -1 && inList[index].text == "(") || (dir == 1 && inList[index].text == ")"))
                {
                    if (count == 0)
                        break;

                    count--;
                }
            }

            return index;
        }

        //Parse inputed list of symbols for expressions, fractions, and exponents
        public static List<Symbol> Parse(List<Symbol> sender)
        {
            List<Symbol> list = new List<Symbol>();
            foreach (Symbol s in sender)
                list.Add(s);

            List<Symbol> result = new List<Symbol>();

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].GetType() == typeof(Number))
                {
                    string number = "";

                    int j = i;
                    while (list[j].GetType() == typeof(Number))
                    {
                        number += list[j].text;
                        j++;
                    }

                    print.log("testing here" + number);
                    list.RemoveRange(i, j - i);
                    list.Insert(i, new Number(double.Parse(number)));
                }
            }

            //Search for and create expressions
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].text == "(")
                {
                    /*int count = 0;

                    int j;
                    for(j = i + 1; j < list.Count; j++)
                    {
                        if (list[j].text == "(")
                        {
                            count++;
                        }
                        else if (list[j].text == ")")
                        {
                            if (count == 0)
                                break;

                            count--;
                        }
                    }*/

                    int index = findMatching(list[i] as Text, list);

                    if (index < list.Count)
                    {
                        List<Symbol> temp = Parse(list.GetRange(i + 1, index - i - 1));
                        temp.Insert(0, list[i]);
                        temp.Add(list[index]);

                        list.RemoveRange(i, index - i + 1);

                        if (temp.Count == 3 && (temp[1].GetType() == typeof(Fraction) || temp[1].GetType() == typeof(Exponent)))
                        {
                            list.Insert(i, temp[1]);
                        }
                        else
                        {
                            list.Insert(i, new Expression(temp));
                        }
                    }
                }
            }

            for (int i = 0; i < list.Count; i++)
            {
                if (i + 1 < list.Count && list[i + 1].text == "/")
                {
                    result.Add(new Fraction(list[i], list[i + 2]));
                    i += 2;
                }
                else if (i + 1 < list.Count && list[i + 1].text == "^")
                {
                    Exponent temp = new Exponent(list[i], list[i + 2]);
                    //temp.format = new Format(gravity: "bottom");
                    list[i + 2].format = new Format(padding: 50, gravity: "bottom");

                    result.Add(temp);
                    i += 2;
                }
                else
                {
                    result.Add(list[i]);
                }
            }

            //Special cases
            /*for (int i = 0; i < list.Count; i++)
            {
                if (list[i].text == "/")
                {
                    Expression[] frac = new Expression[2];

                    for (int j = 0; j < 2; j++)
                    {
                        print.log("j is " + j);
                        try
                        {
                            Symbol temp = list[i + (j * 2 - 1)];

                            if (temp.GetType() == typeof(Expression))
                            {
                                frac[j] = temp as Expression;
                            }
                            else
                            {
                                frac[j] = new Expression(temp);

                                parend(text.IndexOf(list[i]) + (j * 2 - 1));
                                if (temp.GetType() == typeof(Edit))
                                    pos--;
                                pos += 2;
                            }
                        }
                        catch
                        {
                            frac[j] = new Expression();
                            parend(pos++);
                        }
                    }

                    result[result.Count - 1] = new Fraction(frac[0], frac[1]);

                    i++;
                }
                else if (list[i].text == "^")
                {
                    Expression[] exp = new Expression[2];

                    for (int j = 1; j < 2; j++)
                    {
                        try
                        {
                            Symbol temp = list[i + (j * 2 - 1)];

                            if (temp is Expression)
                            {
                                exp[j] = temp as Expression;
                            }
                            else
                            {
                                exp[j] = new Expression(temp);

                                parend(text.IndexOf(list[i]) + (j * 2 - 1));
                                if (temp.GetType() == typeof(Edit))
                                    pos--;
                                pos += 2;
                            }
                        }
                        catch
                        {
                            exp[j] = new Expression();
                            parend(pos++);
                        }
                    }

                    exp[1].format = Format.Exponent;
                    result.Add(exp[1]);

                    i++;
                }
                else
                {
                    result.Add(list[i]);
                }
            }*/

            print.log("----parsed list-----");
            foreach (Symbol s in result)
                print.log(s +", "+ s.GetHashCode());
            print.log("----parsed list-----");

            return result;
        }
    }
}
