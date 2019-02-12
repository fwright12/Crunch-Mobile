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

        //public static Graphics UI;
        public List<Symbol> text = new List<Symbol>();
        public static int pos;
        public static OnReceive state = OnReceive.add;

        public enum OnReceive { add, delete };

        public static bool editing = false;
        public static object editField;

        public static readonly Text EditingPlaceHolder = new Text("for debugging purposes");
        public Expression expression;
        public MathView mathView;
        public Number changing;

        private Text equalSign = new Text("=");

        public Input(MathView view)
        {
            mathView = view;
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

        public void ExitEditMode()
        {
            text[pos] = new Number(changing.value);
            mathView.SetText(text);
            //graphicsHandler.RemoveChild(mathView.main, pos);
            //Graphics.graphicsHandler.AddChild(mathView.parent, Graphics.Create((Number)text[pos]), pos);
            //editField = null;
            pos++;
        }

        public void Key(string s)
        {
            //If already editing and another number is sent
            /*if (IsNumber(s) && !editing)
            {
                print.log("started");
                text.Insert(pos, new Text("number place holder"));
                UI.StartEditing();
            }
            else if (!editing)
            {*/

            Symbol adding = null;

            if (IsNumber(s))
            {
                if (!editing)
                {
                    editField = graphicsHandler.AddEditField();
                    //graphicsHandler.AddChild(mathView.main, editField, pos);
                    text.Insert(pos, EditingPlaceHolder);
                    //text.Add(EditingPlaceHolder);
                }
                else
                {
                    //text[pos] = new Number(double.Parse(graphicsHandler.DispatchKey(s)));
                    //Equation.selected.SetAnswer();
                    //return;
                }

                changing = new Number(double.Parse(graphicsHandler.DispatchKey(s)));

                //Input.UI.views.Remove(Input.text[Input.pos]);
                //Input.text[Input.pos] = new Number(double.Parse(editField.Text));
                //Input.UI.views.Add(Input.text[Input.pos], Input.editField);

                //Input.UI.expression.parts = Expression.Parse(Input.text);
            }
            else
            {
                if (editing)
                {
                    ExitEditMode();
                    //mathView.SetText(Expression.Parse(text));
                }

                switch (s)
                {
                    case "/":
                    case "+":
                    case "-":
                    case "*":
                        text.Insert(pos, new Operand(s));
                        break;
                    case "(":
                        Expression temp = new Expression();
                        temp.Parend = true;
                        expression.Insert(temp);
                        break;
                    default:
                        text.Insert(pos, new Function(s, new Expression()));
                        break;
                }

                //Graphics.graphicsHandler.AddChild(UI.graphicalObject, Graphics.Create((dynamic)text[pos]), pos);
                pos++;
            }

            editing = IsNumber(s);

            print.log("Input text");
            foreach (Symbol a in text)
                print.log(a);

            //}

            //editing = IsNumber(s);

            //UI.Update();
            List<Symbol> idk = Expression.Parse(text);

            idk.Add(equalSign);
            //idk.Add(new Text("="));
            //idk.Add(new Number(0));

            mathView.SetText(idk);

            //Equation.selected.SetAnswer();

            /*editing = IsNumber(s);

            print.log(state +", "+ (state == OnReceive.delete));
            if (state == OnReceive.delete && !editing)
            {
                print.log("in");
                UI.expression.parts.RemoveAt(--pos);
            }

            //print.log("SDFSDFSD " + pos);

            //If no longer editing but the object still exists, then
            //the last thing sent must have been a number
            if (!editing && editField != null)
            {
                //UI.expression.parts[pos - 1] = new Number(double.Parse(UI.expression.parts[pos - 1].text));
                pos++;
                editField = null;
            }

            switch (s)
            {
                case "/":
                    //List<Symbol> temp = Expression.next(UI.expression.parts, -1, "+", "-", "*", "/");
                    //print.log(UI.expression.parts[pos - 1]);
                    //UI.expression.parts.RemoveRange(pos - temp.Count, temp.Count);

                    dynamic denominator;// = new Expression();
                    try
                    {
                        denominator = UI.expression.parts[pos + 1];
                    }
                    catch
                    {
                        denominator = new Expression();
                    }

                    UI.expression.parts[pos - 1] = new Fraction((dynamic)UI.expression.parts[pos - 1], denominator);

                    break;
                case "+":
                case "-":
                case "*":
                case "^":
                    UI.expression.Insert(new Operand(s));
                    break;
                case "(":
                    Expression temp = new Expression();
                    temp.Parend = true;
                    UI.expression.Insert(temp);
                    break;
                default:
                    try
                    {
                        double.Parse(s);

                        //If the object hasn't been created, then
                        //this is the first number
                        if (editField == null)
                        {
                            UI.expression.Insert(new Number(0));
                            UI.Update();
                            UI.expression.parts[pos] = new Number(double.Parse(s));
                            //UI.Update();
                        }
                        else
                        {
                            UI.expression.parts[pos] = new Number(double.Parse(UI.expression.parts[pos].text + s));
                        }
                    }
                    catch
                    {
                        UI.expression.Insert(new Function(s, new Expression()));
                    }

                    //Equation.selected.SetAnswer();
                    //return;
                    break;
            }

            if (!editing)
                pos++;

            //If there's no edit object, then
            //it's either not editing or one needs to be created
            if (editField == null)
            {
                Graphics updatedInputUI = UI.Update();

                //if (!editing)
                 //   pos++;

                //print.log("thi sk k k  " + updatedInputUI + "DSF DS");
                if (updatedInputUI != null)
                {
                    UI = updatedInputUI;
                    pos = 0;
                }
            }

            Equation.selected.SetAnswer();*/
        }
    }
}
