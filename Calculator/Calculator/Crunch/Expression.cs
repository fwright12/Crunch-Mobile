using System;
using System.Collections.Generic;
using System.Text;

namespace Crunch.Math
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

            foreach(Term term in Terms)
            {
                answer.Add(term * o);
            }

            return new Expression(answer.ToArray());
        } 

        public override Operand Add(Constant c) => Add(new Term(c));

        public override Operand Add(Fraction f) => Add(new Term(f));

        public override Operand Add(Expression e) => Add(e.Terms.ToArray());

        public override Operand Multiply(Constant c) => multiply(c);

        public override Operand Multiply(Fraction f) => multiply(f);

        public override Operand Multiply(Expression e)
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