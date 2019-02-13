using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace Calculator.Graphics
{
    public class Expression : Container
    {
        public Expression(List<Symbol> children)
        {
            foreach (Symbol s in children)
            {
                Add(s);
            }
        }

        public Expression(params Symbol[] children) : this(children.ToList()) { }

        public override View Render()
        {
            var temp = BaseLayout();
            temp.BackgroundColor = Color.White;
            
            /*temp.ChildAdded += delegate
            {
                if (temp.Children.Count == 1)
                {
                    //temp.LayoutParameters.Width = ViewGroup.LayoutParams.WrapContent;
                }
            };
            temp.ChildRemoved += delegate
            {
                if (temp.Children.Count == 0)
                {
                    //temp.LayoutParameters.Width = ViewGroup.LayoutParams.MatchParent;
                }
            };*/

            return temp;
        }

        public static implicit operator Crunch.Term(Expression layout)
        {
            try
            {
                var temp = Crunch.Math.Evaluate(layout);
                if (temp is Crunch.Fraction)
                {
                    return (temp as Crunch.Fraction).Simplify();
                }
                else if (temp is Crunch.Exponent)
                {
                    return new Crunch.Number(temp.value);
                }
                else
                {
                    return temp;
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
