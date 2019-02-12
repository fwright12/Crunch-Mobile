using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculator
{
    public class Equation
    {
        public static Dictionary<object, Equation> all = new Dictionary<object, Equation>();
        public static object canvas;

        public MathText left = new MathText();
        public MathText right = new MathText();

        public object root;

        public Equation()
        {
            root = Input.graphicsHandler.AddLayout(new Format());

            MathText.selected = left;

            Input.graphicsHandler.AddChild(canvas, root, 0);
            Input.graphicsHandler.AddChild(root, left.mathView.root = Input.graphicsHandler.AddLayout(new Format()), 0);
            Input.graphicsHandler.AddChild(root, Input.graphicsHandler.AddText("="), 0);
            Input.graphicsHandler.AddChild(root, right.mathView.root = Input.graphicsHandler.AddLayout(new Format()), 0);
        }
    }
}
