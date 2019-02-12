using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculator
{
    public class Equation
    {
        public static Dictionary<object, Equation> findByGraphics = new Dictionary<object, Equation>();
        public static Equation selected;

        public object parent;

        public Expression left;
        public Symbol right;
        public Symbol answer;

        public Equation(object Parent, Expression Left)
        {
            /*parent = Parent;
            left = Left;

            selected = this;

            //new Graphics(left);

            Input.UI = new Graphics(left);
            Input.UI.Update();
            Input.pos = 0;
            Input.editField = null;
            findByGraphics.Add(parent, this);
            //Graphics.findByLayout.Add(Input.UI.graphicalObject, Input.UI);
            Graphics.graphicsHandler.AddChild(parent, Input.UI.graphicalObject, 0);

            //print.log("testing this " + Graphics.Create(new Number(1)));
            //Graphics.graphicsHandler.AddChild(Input.UI.graphicalObject, Graphics.Create(new Number(1)), Input.pos);

            //Input.UI = Graphics.aaAdd(parent, new Graphics(left), 0);
            /*graphicsHandler.AddChild(parent, toAdd.graphicalObject, index);
            toAdd.Update();
            Input.pos = 0;

            return toAdd;

            //Graphics.graphicsHandler.AddChild(parent, Input.UI.graphicalObject, 0);
            //Graphics.Add(parent, new Graphics(left), 0);*/
        }

        public void SetAnswer()
        {
            /*Graphics.clickable = false;

            answer = left.answer;

            try
            {
                Graphics.graphicsHandler.RemoveChild(parent, 2);
            }
            catch
            {
                Graphics.graphicsHandler.AddChild(parent, Graphics.graphicsHandler.AddText(" = "), 1);
            }

            if (answer.GetType() == typeof(Fraction))
                answer = ((Fraction)answer).Simplify();

            Graphics.graphicsHandler.AddChild(parent, Graphics.Create((dynamic)answer), 2);
            Graphics.clickable = true;*/
        }
    }
}
