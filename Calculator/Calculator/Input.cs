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
        public static Symbol lastAdded;
        public static object layoutRoot;

        public static bool editing = false;
        public static object editField;
        public static Edit editor;

        public List<Symbol> text = new List<Symbol>();
        public Expression expression;
        public MathView mathView;

        public static OnReceive state = OnReceive.add;
        public enum OnReceive { add, delete };

        private Text equalSign = new Text("=");

        public Input(MathView view)
        {
            mathView = view;
            pos = 0;
        }

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

        private void Insert(Symbol sender)
        {
            text.Insert(pos, sender);
            lastAdded = sender;
        }

        public void Key(string s)
        {
            if (IsNumber(s))
            {
                if (!editing)
                {
                    editField = graphicsHandler.AddEditField();
                    Insert(editor = new Edit(0));
                }

                editor.Dispatch(double.Parse(graphicsHandler.DispatchKey(s)));
            }
            else
            {
                if (editing)
                {
                    text.Remove(editor);
                    Insert(new Number(editor.value));
                    pos++;
                }

                switch (s)
                {
                    case "exit edit mode":
                        if (text.Count == 0)
                        {
                            graphicsHandler.RemoveChild(layoutRoot, mathView.main);
                        }
                        pos--;
                        break;
                    case "^":
                    case "/":
                    case "*":
                    case "+":
                    case "-":
                        Insert(new Operand(s));
                        break;
                    case "(":
                    case ")":
                        Insert(new Text(s));
                        break;
                    default:
                        Insert(new Function(s));
                        parend(++pos);
                        break;
                }

                pos++;
            }

            editing = IsNumber(s);

            print.log("Input text");
            foreach (Symbol a in text)
                print.log(a +", "+ a.GetHashCode());

            List<Symbol> parsed = Parse(text);
            parsed.Add(new Text("="));
            parsed.Add(new Expression(parsed).answer.Copy());

            mathView.SetText(parsed);
        }

        public void parend(int index)
        {
            text.Insert(Math.Min(index + 1, text.Count), new Text(")"));
            text.Insert(Math.Max(index, 0), new Text("("));
        }

        public List<Symbol> Parse(List<Symbol> sender)
        {
            List<Symbol> list = new List<Symbol>();
            foreach (Symbol s in sender)
                list.Add(s);

            List<Symbol> result = new List<Symbol>();

            //Search for and create expressions
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].text == "(")
                {
                    int count = 0;

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
                    }

                    if (j < list.Count)
                    {
                        List<Symbol> temp = Parse(list.GetRange(i + 1, j - i - 1));
                        temp.Insert(0, list[i]);
                        temp.Add(list[j]);

                        list.RemoveRange(i, j - i + 1);
                        list.Insert(i, new Expression(temp));
                        //result.Add(list[j]);

                        //i = j;
                    }
                }
            }

            print.log("extracted expressions");
            foreach (Symbol s in list)
                print.log(s);

            //Special cases
            for (int i = 0; i < list.Count; i++)
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
                        }
                    }

                    //result[result.Count - 1] = new Exponent(exp[0], exp[1]);
                    exp[1].format = Format.Exponent;
                    result.Add(exp[1]);

                    i++;
                }
                else
                {
                    result.Add(list[i]);
                }

                /*if (list[i].text == "^")
                {
                    try
                    {
                        if (list[i + 1].GetType() == typeof(Expression))
                        {
                            result.Add(list[i + 1]);
                        }
                        else
                        {
                            parend(text.IndexOf(list[i + 1]));
                            result.Add(Expression.Wrap(list[i + 1]));
                        }

                        i++;
                    }
                    catch { }
                }*/
            }

            print.log("new text");
            foreach (Symbol s in text)
                print.log(s + ", " + s.text);
            print.log("----parsed list-----");
            foreach (Symbol s in result)
                print.log(s +", "+ s.GetHashCode());
            print.log("----parsed list-----");

            return result;
        }
    }

    public class Edit : Number
    {
        public Edit(double d) : base(d) { }

        public void Dispatch(double d)
        {
            _value = d;
        }
    }
}
