using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Calculator;

namespace Crunch
{
    public class UnrecognizedSymbolException : Exception
    {
        public UnrecognizedSymbolException(Type t) : base("Could not recognize symbol of type" + t) { }
    }

    public static class Math
    {
        public static Term Evaluate(Expression expression)
        {
            List<View> calculate = (expression as StackLayout).Children.ToList();

            //Get rid of the cursor
            if (Cursor.Parent == expression)
            {
                calculate.RemoveAt(Cursor.Index);
            }

            int i = 0;
            return Evaluate(calculate, ref i);
        }

        public static Term Evaluate(List<View> calculate, ref int i)
        {
            List<Term> terms = new List<Term>();
            List<string> operands = new List<string>();
            
            //Parse
            for ( ; i < calculate.Count; i++)
            {
                Type type = calculate[i].GetType();

                //Numbers need to be combined
                if (type == typeof(Calculator.Number))
                {
                    string number = "";

                    int j = i;
                    while (j < calculate.Count && calculate[j].GetType() == typeof(Calculator.Number))
                    {
                        number += (calculate[j] as Calculator.Number).Text;
                        j++;
                    }

                    terms.Add(new Number(double.Parse(number)));
                    i = j - 1; //One less because i will be incremented by the loop
                }
                else if (type == typeof(Calculator.Exponent))
                {
                    terms[terms.Count - 1] = new Number(new Exponent(terms[terms.Count - 1], Evaluate(calculate[i] as Calculator.Exponent)).value);
                }
                else if (type == typeof(Text) || type == typeof(Minus))
                {
                    string text = (calculate[i] as Text).Text.Trim();

                    if (text == "(")
                    {
                        i++;
                        terms.Add(Evaluate(calculate, ref i));
                    }
                    else if (text == ")")
                    {
                        break;
                    }
                    else if ((calculate[i] as Text).Text == "-")
                    {
                        operands.Add("*");
                        terms.Add(new Number(-1));
                    }
                    else
                    {
                        if (text == "×")
                        {
                            text = "*";
                        }
                        operands.Add(text);
                    }
                }
                //These types have implicit operators defined
                else if (type == typeof(Calculator.Fraction))
                {
                    Calculator.Fraction f = calculate[i] as Calculator.Fraction;
                    terms.Add(new Fraction(Evaluate(f.Numerator), Evaluate(f.Denominator)).Simplify());
                }
                else
                {
                    throw new UnrecognizedSymbolException(type);
                }
            }

            return evaluate(terms, operands);
        }

        private static Term evaluate(List<Term> terms, List<string> operands)
        {
            //Multiplication
            for (int i = 0; i < operands.Count; i++)
            {
                if (operands[i] == "*")
                {
                    operate("*", ref terms, ref operands, ref i);
                }
            }

            //Addition and Subtraction
            for (int i = 0; i < operands.Count; i++)
            {
                operate(operands[i], ref terms, ref operands, ref i);
            }

            return terms[0].Simplify();
        }

        private static void operate(string operation, ref List<Term> terms, ref List<string> operands, ref int i)
        {
            if (operation == "+")
            {
                terms[i] = (terms[i] as dynamic) + (terms[i + 1] as dynamic);
            }
            else if (operation == "-")
            {
                terms[i] = terms[i] - terms[i + 1];
            }
            else if (operation == "*")
            {
                terms[i] = (terms[i] as dynamic) * (terms[i + 1] as dynamic);
            }

            terms.RemoveAt(i + 1);
            operands.RemoveAt(i--);
        }

        //Alternate way to parse operations
        //PEMDAS
        //First pass multiplication, second addition/subtraction
        /*for (int i = 0; i< 2; i++)
        {
            for (int j = 0; j<operands.Count; j++)
            {
                if (i == 0 && operands[j].text == "*")
                {
                    terms[j] = (terms[j] as dynamic) * (terms[j + 1] as dynamic);
                }
                else if (i == 1)
                {
                    print.log("here " + operands[j]);
                    if (operands[j].text == "+")
                    {
                        terms[j] = (terms[j] as dynamic) + (terms[j + 1] as dynamic);
                    }
                    else if (operands[j].text == "-")
                    {
                        terms[j] = terms[j] - terms[j + 1];
                    }
                }
                else
                {
                    continue;
                }

                terms.RemoveAt(j + 1);
                operands.RemoveAt(j--);
            }
        }*/
    }
}
