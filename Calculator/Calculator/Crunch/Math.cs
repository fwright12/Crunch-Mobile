using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculator.Crunch
{
    public static class Math
    {
        public static Term Evaluate(Graphics.Expression expression)
        {
            List<Graphics.Symbol> calculate = expression.Children.ToList();

            List<Term> terms = new List<Term>();
            List<Graphics.Text> operands = new List<Graphics.Text>();

            //Parse Graphics stuff to Crunch stuff
            for (int i = 0; i < calculate.Count; i++)
            {
                Type type = calculate[i].GetType();

                //Numbers need to be combined
                if (type == typeof(Graphics.Number))
                {
                    string number = "";

                    int j = i;
                    while (j < calculate.Count && calculate[j].GetType() == typeof(Graphics.Number))
                    {
                        number += (calculate[j] as Graphics.Number).text;
                        j++;
                    }

                    terms.Add(new Graphics.Number(number));
                    i = j - 1; //One less because i will be incremented by the loop
                }
                else if (type == typeof(Graphics.Exponent))
                {
                    terms[terms.Count - 1] = new Exponent(terms[terms.Count - 1], calculate[i] as dynamic);
                }
                else if (type == typeof(Graphics.Text))
                {
                    operands.Add(calculate[i] as Graphics.Text);
                }
                //These types have implicit operators defined
                else if (type == typeof(Graphics.Number) || type == typeof(Graphics.Fraction))
                {
                    terms.Add(calculate[i] as dynamic);
                }
                else
                {
                    throw new Exception("unrecognized symbol");
                }
            }

            //Multiplication
            for (int i = 0; i < operands.Count; i++)
            {
                if (operands[i].text == "*")
                {
                    operate("*", ref terms, ref operands, ref i);
                }
            }

            //Addition and Subtraction
            for (int i = 0; i < operands.Count; i++)
            {
                operate(operands[i].text, ref terms, ref operands, ref i);
            }

            return terms[0];
        }

        private static void operate(string operation, ref List<Term> terms, ref List<Graphics.Text> operands, ref int i)
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
