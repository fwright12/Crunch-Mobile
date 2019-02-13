using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crunch
{
    using Pair = KeyValuePair<object, Math.Operand>;

    public static partial class Math
    {
        public class Term : Operand
        {
            public double Coefficient => coefficient;
            //private Dictionary<Expression, Operand> expressions = new Dictionary<Expression, Operand>();
            //private Dictionary<object, Operand> members = new Dictionary<object, Operand>();
            private GroupedDictionary<Operand> members = new GroupedDictionary<Operand>();
            private double coefficient = 1;

            public bool IsConstant => members.Count == 0;

            public Term(double d) => coefficient = d;
            public Term(Variable v) => multiply(v);
            public Term(Expression e)
            {
                Term gcd = e.Terms[0];

                for (int i = 1; i < e.Terms.Count; i++)
                {
                    gcd = GCD(gcd, e.Terms[i]);
                }

                Multiply(gcd);

                //Cancel the gcd from the expression - not necessary if the gcd is 1 term (because then the gcd is the expressions only term)
                print.log("making term from expression", e, e.Terms.Count, gcd);
                if (e.Terms.Count > 1)
                {
                    if (!(gcd.IsConstant && gcd.coefficient == 1))
                    {
                        gcd.exponentiate(-1);
                        e = e.Multiply(gcd);
                        gcd.exponentiate(-1);
                    }
                    multiply(e);
                }
            }

            public static implicit operator Expression(Term t)
            {
                if (t.members.TypeCount(typeof(Expression)) > 0)
                {
                    Expression ans = new Expression(new Term(1));
                    foreach (Expression e in t.members.EnumerateKeys<Expression>())
                    {
                        //if (!((Term)t.members[e]).IsConstant)
                        if (t.members[e].ToString() != "1")
                        {
                            return new Expression(t);
                        }
                        ans = ans.Multiply(e);
                    }
                    t.members.RemoveType(typeof(Expression));
                    ans = ans.Multiply(t);
                    
                    return ans;
                }
                else
                {
                    return new Expression(t);
                }
            }

            public override bool IsNegative() => coefficient < 0;

            public override Operand Simplify()
            {
                Operand answer = new Term(1);
                Term ans = new Term(coefficient);
                foreach(Pair pair in members.KeyValuePairs())
                {
                    Variable v = pair.Key as Variable;
                    Operand o = pair.Value.Simplify();
                    Term t = !(o is Fraction) ? (Term)(o as dynamic) : null;

                    if (v != null && t != null && t.IsConstant && (v.Value != null || GraFX.Equation.substitutions.ContainsKey(v.Name)))
                    {
                        if (v.Value != null)
                        {
                            ans.Multiply(new Term(System.Math.Pow(v.Value.Coefficient, t.Coefficient)));
                        }
                        else if (GraFX.Equation.substitutions.ContainsKey(v.Name))
                        {
                            answer *= GraFX.Equation.substitutions[v.Name] ^ pair.Value;
                        }
                    }
                    else
                    {
                        ans.multiply(pair.Key, pair.Value);
                    }
                }
                return ans * answer;
            }

            public Term Copy()
            {
                Term t = new Term(coefficient);
                foreach (Pair pair in members.KeyValuePairs())
                {
                    t.members.Add((pair.Key as Expression)?.Copy() ?? pair.Key, (pair.Value as dynamic).Copy());
                }
                return t;
            }

            //public static Operand Multiply(Operand o1, Operand o2) => (o1 as dynamic).Multiply(o2 as dynamic) ?? (o2 as dynamic).Multiply(o1 as dynamic);

            public bool IsLike(Term t)
            {
                if (members.Count != t.members.Count)
                {
                    return false;
                }

                foreach (KeyValuePair<dynamic, Operand> pair in members.KeyValuePairs())
                {
                    if (!t.members.ContainsKey(pair.Key) || !pair.Value.Equals(t.members[pair.Key]))
                    {
                        return false;
                    }
                }

                return true;
            }

            /*public static Expression Divide(Expression e, Term t)
            {
                t.exponentiate(-1);
                Expression ans = e.Multiply(t);
                t.exponentiate(-1);
                return ans;
            }*/

            public static void Divide(ref Term t1, ref Term t2)
            {
                if (t1.IsConstant && t2.IsConstant && (!t1.coefficient.IsWhole() || !t2.coefficient.IsWhole()))
                {
                    t1 = new Term(t1.coefficient / t2.coefficient);
                    t2 = new Term(1);
                    return;
                }

                Term gcd = GCD(t1, t2);
                gcd.exponentiate(-1);
                print.log("gcd of " + t1 + " and " + t2 + " is " + gcd, gcd.coefficient);

                t1.Multiply(gcd);
                t2.Multiply(gcd);
            }

            public static Term GCD(Term t1, Term t2)
            {
                bool areBothWhole = t1.coefficient.IsWhole() && t2.coefficient.IsWhole();
                Term t = new Term(areBothWhole ? GCD((int)System.Math.Abs(t1.coefficient), (int)System.Math.Abs(t2.coefficient)): 1);
                if (t2.coefficient < 0)
                {
                    t.coefficient *= -1;
                }

                foreach (Pair pair in t1.members.KeyValuePairs())
                {
                    if (t2.members.ContainsKey(pair.Key))
                    {
                        Term a = (pair.Value is Expression) ? (Term)(pair.Value as Expression) : null;
                        Term b = (t2.members[pair.Key] is Expression) ? (Term)(t2.members[pair.Key] as Expression) : null;
                        if (a == null || b == null) continue;

                        t.multiply(pair.Key, System.Math.Min(a.coefficient, b.coefficient));
                    }
                }

                return t;
            }

            private static int GCD(int a, int b)
            {
                if (a == 0)
                {
                    return b;
                }
                if (b == 0)
                {
                    return a;
                }

                if (a > b)
                {
                    return GCD(a % b, b);
                }
                return GCD(a, b % a);
            }

            /******************************* ADDITION *******************************/
            public Expression Add(Term t)
            {
                if (IsLike(t))
                {
                    coefficient += t.coefficient;
                    return this;
                }
                else if (members.TypeCount(typeof(Expression)) > 0 || t.members.TypeCount(typeof(Expression)) > 0)
                {
                    Expression e1 = this;
                    Expression e2 = t;
                    print.log("adding terms with expressions", e1, e2);
                    return e1.Add(e2);
                }
                else
                {
                    return new Expression(this, t);
                }
            }

            /******************************* MULTIPLICATION *******************************/
            public Term Multiply(Term t)
            {
                print.log("multiplying " + this + " and " + t);
                coefficient *= t.coefficient;
                foreach (Pair pair in t.members.KeyValuePairs())
                {
                    multiply(pair.Key, pair.Value);
                }
                return this;
            }
            //public Term Multiply(Expression e) => multiply(e);// members.ContainsKey(e) ? multiply(e) : null;
            //public Term Multiply(Fraction f) => (f.IsConstant) ? multiplyCoefficient(f) : null;

            //private Term multiplyCoefficient(Operand o) { coefficient *= o; return this; }
            private Term multiply(object key) => multiply(key, 1);
            private Term multiply(object key, Operand value)
            {
                /*if (key is double && value.Equals(1))
                {
                    coefficient *= (double)key;
                }*/

                if (!members.ContainsKey(key))
                {
                    members.Add(key, 0);
                }

                members[key] += value;
                if (members[key].ToString() == "0")
                {
                    members.Remove(key);
                }
                return this;
            }

            /******************************* EXPONENTIATION *******************************/
            public new Operand Exponentiate(Operand o)
            {
                print.log("exponentiating " + o, this, coefficient);
                bool b = RemoveNegative(ref o);

                exponentiate(o);

                if (b)
                {
                    return new Fraction(new Term(1), this);
                }
                else
                {
                    return this;
                }
            }

            private void exponentiate(Operand o)
            {
                Term t = !(o is Fraction) ? (Term)(o as dynamic) : null;
                if (t != null && t.IsConstant)
                {
                    coefficient = System.Math.Pow(coefficient, t.coefficient);
                }

                var temp = new List<object>(members.Count);
                foreach (Pair pair in members.KeyValuePairs())
                {
                    temp.Add(pair.Key);
                }
                foreach(object obj in temp)
                {
                    members[obj] *= o;
                }
            }

            /*public override Operand Simplify()
            {
                return base.Simplify();
                Operand ans = null;// coefficient;
                foreach(Operand o in members.Keys)
                {
                    //ans = ans * o.Simplify();
                    if (members[o] is Constant && (members[o] as Constant).Value < 0)
                    {
                        //ans *= Fraction.Construct(1, members[o] * -1);
                    }
                    else
                    {
                        ans *= members[o];
                    }
                }
                return ans;
            }*/

            public override string ToString()
            {
                if (coefficient == 0) return "0";

                string s =  "";// HideOnes(coefficient);

                foreach(Pair pair in members.KeyValuePairs())
                {
                    string exp = pair.Value.ToString();
                    s += pair.Key.ToString() + (exp == "1" ? "" : ("^" + exp));
                }

                string c = coefficient.ToString();
                if (c.Contains("E+"))
                {
                    s = "*10^" + c.Substring(c.IndexOf("E+") + 2) + s;
                    c = c.Substring(0, c.IndexOf("E+"));
                }
                c = System.Math.Round(double.Parse(c), 3).ToString();
                return s != "" && System.Math.Abs(coefficient) == 1 ? c.TrimEnd('1') + s : c + s;
            }
        }
    }
}
 