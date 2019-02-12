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
        public Symbol text;

        public Graphic(object Parent, object Child, int index)
        {
            child = Child;
            SetParent(index, Parent);
        }

        public Graphic(object Parent, Symbol Text, int index)
        {
            child = Input.selected.mathView.Create((dynamic)Text);
            text = Text;

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
            Input.selected.mathView.main = parent;
        }
    }

    public class MathView
    {
        public object main;

        public Dictionary<Symbol, Graphic> views = new Dictionary<Symbol, Graphic>();

        public MathView(object layout)
        {
            main = layout;// Create(new Expression());
            //Graphics.graphicsHandler.AddChild(canvas, parent, 0);
        }

        private List<Symbol> sent = new List<Symbol>();

        public void SetText(List<Symbol> list)
        {
            sent.Clear();

            SetText(main, list);

            print.log("removing...");
            List<Symbol> temp = views.Keys.ToList();
            foreach (Symbol s in temp)
            {
                if (!sent.Contains(s))
                {
                    print.log("removed " + s);
                    Input.graphicsHandler.RemoveChild(views[s].parent, views[s].child);
                    views.Remove(s);
                }
            }
        }

        private void SetText(object parent, List<Symbol> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                print.log(i +", "+ "look here " + list[i] + ", " + views.ContainsKey(list[i]) + ", "+ list[i].Equals(new Text("=")));
                //print.log(list[i].GetText()[0].text);
                //foreach (Symbol s in list[i].GetText())
                //    print.log(s);

                sent.Add(list[i]);

                if (!views.ContainsKey(list[i]))
                {
                    print.log("adding new");
                    //object temp = Create((dynamic)list[i]); //Graphics.graphicsHandler.AddText(" " + list[i].text + " ");
                    //if (Graphics.clickable)
                    //    Graphics.graphicsHandler.Select(temp);

                    //Input.graphicsHandler.AddChild(adding.parent, adding.child, i);

                    /*if (Input.editing && !views.ContainsKey(Input.EditingPlaceHolder))
                    {
                        Input.selected.text.Add(Input.EditingPlaceHolder);
                        views.Add(Input.EditingPlaceHolder, new Graphic(parent, Input.editField, i));
                    }*/
                    //else if (!Input.editing)
                    //{

                    //}
                    views.Add(list[i], new Graphic(parent, list[i], i));
                }
                else if (views.ContainsKey(list[i]) && views[list[i]].parent != parent)
                {
                    views[list[i]].SetParent(i, parent);
                }

                if (list[i] != list[i].GetText()[0])
                {
                    print.log("going again");
                    SetText(views[list[i]].child, list[i].GetText());
                }
            }
        }

        public object Create(Symbol sender)
        {
            if (sender == Input.EditingPlaceHolder)
            {
                return Input.editField;
            }
            else
            {
                return Input.graphicsHandler.AddText(" " + sender.text + " ");
            }
        }

        public object Create(Expression sender)
        {
            //Graphics temp = new Graphics(sender);

            //temp.Update();

            //lastAdded = temp;

            return Input.graphicsHandler.AddLayout(true);
            //return temp.graphicalObject;
        }

        public object Create(Fraction sender)
        {
            object layout = Input.graphicsHandler.AddLayout(false);

            return layout;
            //print.log("SDFLJSDFJLSDF " + ", " + views.ContainsKey(sender.numerator.GetText()[0]) + ", "+ sender.numerator.GetText()[0]);

            //The numberator already exists as an expression
            if (!views.ContainsKey(sender.numerator))
            {
                views.Add(sender.numerator, new Graphic(layout, sender.numerator, 0));
            }

            if (!views.ContainsKey(sender.denominator))
            {
                views.Add(sender.denominator, new Graphic(layout, sender.denominator, 1));
            }

            return layout;
        }
    }
}
