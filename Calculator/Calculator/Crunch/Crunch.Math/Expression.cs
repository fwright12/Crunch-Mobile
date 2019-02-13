using System;
using System.Collections.Generic;
using System.Text;

namespace Crunch
{
    public static partial class Math
    {
        /// <summary>
        /// A sequence of terms (quantities separated by + signs)
        /// </summary>
        public class Expression : Operand
        {
            public List<Term> Terms = new List<Term>();

            public Expression(params Term[] list)
            {
                foreach (Term t in list)
                {
                    Terms.Add(t);
                }
            }

            public Expression Add(params Term[] list)
            {
                List<Term> answer = new List<Term>();

                foreach (Term term in list)
                {
                    int i = 0;
                    for (; i < Terms.Count; i++)
                    {
                        if (Terms[i].IsLike(term))
                        {
                            answer.Add(Terms[i] + (term));
                            break;
                        }
                    }

                    if (i == Terms.Count)
                    {
                        answer.Add(term);
                    }
                }

                return new Expression(answer.ToArray());
            }

            private Operand multiply(Operand o)
            {
                List<Term> answer = new List<Term>();

                foreach (Term term in Terms)
                {
                    answer.Add(term * o);
                }

                return new Expression(answer.ToArray());
            }

            protected override Operand Add(Constant c) => Add(new Term(c));

            protected override Operand Add(Fraction f) => Add(new Term(f));

            protected override Operand Add(Expression e) => Add(e.Terms.ToArray());

            protected override Operand Multiply(Constant c) => multiply(c);

            protected override Operand Multiply(Fraction f)
            {
                return base.Multiply(f);
            }

            protected override Operand Multiply(Expression e)
            {
                throw new NotImplementedException();
            }

            protected override Operand Divide(Constant c)
            {
                throw new NotImplementedException();
            }

            protected override Operand Exponentiate(Constant c)
            {
                throw new NotImplementedException();
            }

            public override string ToString()
            {
                string s = "";
                foreach (Term t in Terms)
                {
                    s += t.ToString();
                }
                return s;
            }
        }
    }
}