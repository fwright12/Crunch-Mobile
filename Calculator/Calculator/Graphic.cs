using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculator
{
    public class Graphic
    {
        public object parent;
        public object child;
        public string format;

        public Graphic(object Parent, object Child, int index)
        {
            child = Child;
            SetParent(index, Parent);
        }

        public Graphic(object Parent, Symbol Text, int index)
        {
            child = Input.selected.mathView.Create((dynamic)Text);

            SetParent(index, Parent);
        }

        public void SetParent(int index, object newParent)
        {
            try
            {
                Input.graphicsHandler.RemoveChild(parent, child);
            }
            catch { }

            Input.graphicsHandler.AddChild(newParent, child, index);
            parent = newParent;
        }
    }
}
