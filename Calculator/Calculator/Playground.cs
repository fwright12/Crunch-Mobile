using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Calculator;

namespace Playground
{
    public class Test
    {
        public Test()
        {
            double a = 1;
            Constant b = 2;
            Term c = 3;
            Expression d = 4;
            Operand e = 5;

            print.log(d + e);
        }
    }

    public class Operand
    {
        public static dynamic operator +(Operand o1, Operand o2) => (o1 as dynamic) + (o2 as dynamic);
        public static implicit operator Operand(double d) => new Constant(d);
    }

    public class Expression : Operand
    {
        public Expression(params Term[] t) { }
        public static implicit operator Expression(Term t) => new Expression(t);
        public static implicit operator Expression(Constant c) => new Expression(c);
        public static implicit operator Expression(double d) => new Expression(d);

        public static Expression operator +(Expression e1, Expression e2) => new Expression();
    }

    public class Term : Operand
    {
        public Term(Constant c) { }
        public static implicit operator Term(Constant c) => new Term(c);
        public static implicit operator Term(double d) => new Term(d);

        public static Term operator +(Term e1, Term e2) => new Term(1);
    }

    public class Constant : Operand
    {
        public Constant(double d) { }
        public static implicit operator Constant(double d) => new Constant(d);

        public static Constant operator +(Constant e1, Constant e2) => new Constant(0);
    }
}
