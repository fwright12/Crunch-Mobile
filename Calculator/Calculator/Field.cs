using System;
using System.Collections.Generic;
using System.Text;

namespace System
{
    public interface IMathList
    {
        IList<object> list { get; }

        void Insert(int index, object o);
        object RemoveAt(int index);
    }

    public static class Input
    {
        public static string Screen<T>(this IMathList stuff, int index, string str) where T : IMathList, new()
        {
            if (str == "/" || str == "^")
            {
                stuff.list.InsertAndFill<T>(ref index, 0);
            }
            if (str == "/")
            {
                stuff.list.InsertAndFill<T>(ref index, -1);
            }

            return str;
        }

        public static object InsertAndFill(this IList<object> input, ref int index, int direction) // where T : IMathList, new()
        {
            object a = new object();
            input.Insert(index, a);

            int imbalance = 0;
            object o = null;
            index += direction * 2 + 1;

            print.log("starting", index);
            //Grab stuff until we hit an operand
            while ((index).IsBetween(0, input.Count - 1) && !(input[index] is string && (input[index] as string).IsOperand() && imbalance == 0))
            {
                o = input[index];

                if (o is string)
                {
                    string s = o as string;
                    if (s == "(" || s == ")")
                    {
                        if (s == "(") imbalance++;
                        if (s == ")") imbalance--;
                    }
                }

                input.RemoveAt(index);
                a.list.Insert(a.list.Count * (direction + 1), o);

                index += direction;
            }
            print.log("done");

            /*if (f.Size == 0)
            {
                f.Focused?.Invoke();
            }
            else
            {
                f.Update();
            }*/

            index -= direction * 2 + 1;

            return a;
            /*if (ChildCount > 0 && ChildAt(ChildCount - 1) is Text && (ChildAt(ChildCount - 1) as Text).Text == ")" && ChildAt(0) is Text && (ChildAt(0) as Text).Text == "(")
            {
                RemoveAt(children.Count - 1);
                RemoveAt(0);
            }*/
        }
    }
}

namespace Crunch.Math.Input
{
    public delegate void InsertEventHandler(int index, object o);
    public delegate void RemovedEventHandler(int index, object o);
    public delegate void FocusedEventHandler();
    public enum FieldFormat { None, Fraction, Exponent };

    public class Field : IMathList
    {
        public event FocusedEventHandler Focused;
        //public event InsertEventHandler Inserted;
        //public event RemovedEventHandler Removed;

        public Field Parent;
        public Operand Value;
        public FieldFormat Format;

        public int Size => input.Count;

        public IList<object> list => input;
        private List<object> input = new List<object>();

        public Field(string expression = "") : this(FieldFormat.None, expression) { }
        public Field(FieldFormat format) : this(format, "") { }
        public Field(FieldFormat format, string expression)
        {
            Build(expression);
            Format = format;
        }

        public void Build(string expression)
        {
            for (int i = 0; i < expression.Length; i++)
            {
                input.Insert(i, expression[i].ToString());
            }
            for (int i = 0; i < expression.Length; i++)
            {
                Insert(i, input[i]);
            }
        }

        public void Add(object o) => Insert(input.Count, o);

        public void Insert(int index, object o)
        {
            if (o as string == "/")
            {
                /*Field numerator, denominator;
                input.Insert(index, denominator = new Field());
                input.Insert(index, numerator = new Field());
                denominator.input.Fill(input, ref index, 0);
                numerator.input.Fill(input, ref index, -1);
                index++;*/

                /*Field numerator = makeField(ref index, 0);
                input.Insert(index, numerator);
                Field denominator = makeField(ref index, -1);
                input.Insert(index, denominator);
                index++;*/

                /*Field fraction = new Field(FieldFormat.Fraction);
                Field denominator = makeField(ref index, 0);
                denominator.Parent = this;
                Field numerator = makeField(ref index, -1);
                numerator.Parent = this;

                Inserted?.Invoke(index, fraction);
                Removed?.Invoke(index - 1, numerator);
                Removed?.Invoke(index, denominator);
                fraction.Inserted?.Invoke(0, numerator);
                fraction.Inserted?.Invoke(2, denominator);*/
            }
            else if (o as string == "^")
            {
                makeField(ref index, 0, FieldFormat.Exponent);
            }
            else
            {
                //Inserted?.Invoke(index, o);

                if (o is Field)
                {
                    (o as Field).Parent = this;
                }
            }
        }

        public object RemoveAt(int index)
        {
            var temp = input[index];
            input.RemoveAt(index);
            //Removed?.Invoke(index, temp);
            return temp;
        }

        public void Update()
        {
            print.log("evaluating " + ToString() + "...");
            
            try
            {
                if (input.Count == 0)
                {
                    throw new Exception();
                }
                Value = parse();
                print.log("evaluated to " + Value.ToString());
            }
            catch (Exception e)
            {
                Value = null;
                print.log(e);
            }
        }

        private Operand parse()
        {
            Expression answer = new Expression();

            Term term = new Term();
            for (int i = 0; i < input.Count; i++)
            {
                do
                {
                    Type type = input[i].GetType();

                    //Numbers need to be combined
                    if (type == typeof(string) && (input[i] as string).IsNumber())
                    {
                        string number = "";

                        int j = i;
                        while (j < input.Count && (bool)(input[j] as string)?.IsNumber())
                        {
                            number += input[j].ToString();
                            j++;
                        }

                        term *= new Constant(double.Parse(number));
                        i = j - 1; //One less because i will be incremented by the loop
                    }
                    else if (type == typeof(Field))
                    {
                        term *= (input[i] as Field).Value;
                    }
                    else if (type == typeof(Operand))
                    {
                        term *= input[i] as Operand;
                    }
                    else if (input[i] as string == "/")
                    {
                        term.Coefficient = new Fraction(term.Coefficient, (input[++i] as Field).Value);
                    }

                    i++;
                } while (i < input.Count && input[i] as string != "+" && input[i] as string != "-");
                
                answer = answer.Add(term);

                term = new Term();
                if (i < input.Count && input[i] as string == "-")
                {
                    term = new Term(new Constant(-1));
                }
            }

            return answer;
        }

        private Field makeField(ref int index, int direction, FieldFormat format = FieldFormat.None)
        {
            Field f = new Field(format);
            Insert(index, f);

            int imbalance = 0;
            object o = null;
            index += direction * 2 + 1;

            print.log("starting", index);
            //Grab stuff until we hit an operand
            while ((index).IsBetween(0, input.Count - 1) && !(input[index] is string && (input[index] as string).IsOperand() && imbalance == 0))
            {
                o = input[index];

                if (o is string)
                {
                    string s = o as string;
                    if (s == "(" || s == ")")
                    {
                        if (s == "(") imbalance++;
                        if (s == ")") imbalance--;
                    }
                }

                RemoveAt(index);
                f.Insert(f.input.Count * (direction + 1), o);

                index += direction;
            }
            print.log("done");

            if (f.Size == 0)
            {
                f.Focused?.Invoke();
            }
            else
            {
                f.Update();
            }

            index -= direction * 2 + 1;

            /*if (ChildCount > 0 && ChildAt(ChildCount - 1) is Text && (ChildAt(ChildCount - 1) as Text).Text == ")" && ChildAt(0) is Text && (ChildAt(0) as Text).Text == "(")
            {
                RemoveAt(children.Count - 1);
                RemoveAt(0);
            }*/

            return f;
        }

        public override string ToString()
        {
            string s = "(";
            foreach (object o in input)
            {
                s += o.ToString();
            }
            return s + ")";
        }
    }
}