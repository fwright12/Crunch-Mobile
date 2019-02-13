using System;
using System.Collections.Generic;
using System.Text;

namespace Crunch
{
    public static partial class Math
    {
        public class Expression : Operand
        {
            public IReadOnlyList<Term> Terms => terms;
            private List<Term> terms = new List<Term>();

            public Expression(params Term[] list)
            {
                foreach (Term t in list)
                {
                    add(t);
                }
            }

            public override Operand Simplify()
            {
                Expression ans = new Expression();
                foreach (Term t in terms)
                {
                    ans.Add((Term)t.Simplify());
                }
                return ans;
            }

            public static explicit operator Term(Expression e) => e.terms.Count == 1 ? e.terms[0] : new Term(e);

            /*public static void Divide(ref Expression e1, ref Expression e2)
            {
                Term gcd = e1.terms[0];

                for (int i = 1; i < e1.terms.Count; i++)
                {
                    gcd = Term.GCD(gcd, e1.terms[i]);
                }
                foreach(Term t in e2.terms)
                {
                    print.log(t);
                    gcd = Term.GCD(gcd, t);
                }
                print.log("gcd of " + e1 + " and " + e2 + " is " + gcd);
                e1 = Term.Divide(e1, gcd);
                e2 = Term.Divide(e2, gcd);
            }*/

            //public static Operand Add(Operand o1, Operand o2) => (o1 as dynamic).Add(o2 as dynamic) ?? (o2 as dynamic).Add(o1 as dynamic);

            public Expression Copy()
            {
                Expression e = new Expression();
                foreach (Term t in terms)
                {
                    e.terms.Add(t.Copy());
                }
                return e;
            }

            /******************************* ADDITION *******************************/
            //public Expression Add(Term t) => add(t);
            public Expression Add(Expression e)
            {
                foreach(Term t in e.terms)
                {
                    add(t);
                }
                return this;
            }

            private Expression add(Term t)
            {
                int i;
                for (i = 0; i < terms.Count; i++)
                {
                    //Operand result = (terms[i] as dynamic).Add(o as dynamic) ?? (o as dynamic).Add(terms[i] as dynamic);
                    //Expression result = terms[i].Add(t);
                    if (terms[i].IsLike(t))
                    {
                        terms[i] = terms[i].Add(t).terms[0];
                        if (terms[i].ToString() == "0")
                        {
                            terms.RemoveAt(i--);
                        }
                        break;
                    }
                }

                if (i == terms.Count)
                {
                    terms.Add(t);
                }

                return this;
            }

            /******************************* MULTIPLICATION *******************************/
            //public Operand Multiply(Term t) => multiply(t);
            public Expression Multiply(Expression e)
            {
                if (e.terms.Count < terms.Count)
                {
                    return e.Multiply(this);
                }

                /*Term t = new Term(3);
                t.Multiply(new Term(new Variable('x')));
                t.Exponentiate(2);
                t.Multiply(new Term(new Variable('y')));

                Term copy = t.Copy();
                print.log(t, copy);
                t.Multiply(new Term(6));
                print.log(t, copy);*/

                Expression ans = new Expression();
                print.log("multiplying " + this + " and " + e);
                foreach (Term t1 in terms)
                {
                    foreach (Term t2 in e.terms)
                    //for (int i = 0; i < e.terms.Count; i++)
                    {
                        /*print.log(t1);
                        Term a = t1.Copy();
                        print.log(t1, a, a.Multiply(t2));*/
                        Term t = t1.Copy().Multiply(t2);
                        ans.add(t);
                    }
                }
                return ans;
            }
            //public Expression Multiply(Fraction f) => (f.IsConstant) ? multiply(f) : null;

            private Expression multiply(Term t)
            {
                for (int i = 0; i < terms.Count; i++)
                {
                    terms[i].Multiply(t);
                }

                return this;
            }

            /******************************* EXPONENTIATION *******************************/
            public new Operand Exponentiate(Operand o) => ((Term)this).Exponentiate(o);

            public override int GetHashCode() => ToString().GetHashCode();
            public override string ToString()
            {
                if (terms.Count == 0) return "0";

                string s = "";
                for (int i = 0; i < terms.Count; i++)
                {
                    string temp = terms[i].ToString();
                    s += (i > 0 && temp[0] != '-') ? "+" + temp : temp;
                }
                return terms.Count > 1 ? "(" + s + ")" : s;
            }
        }
    }
}