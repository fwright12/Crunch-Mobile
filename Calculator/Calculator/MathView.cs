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

    public class MathView
    {
        public static List<string> supportedFunctions = new List<string>() { "Pythagorean Theorem", "Law of Sines", "Law of Cosines" };

        public object main;

        public Dictionary<Symbol, Graphic> views = new Dictionary<Symbol, Graphic>();

        public MathView(object layout)
        {
            main = layout;// Create(new Expression());
        }

        private List<Symbol> sent = new List<Symbol>();
        public static List<Symbol> different = new List<Symbol>();

        private List<Type> unparend = new List<Type>() { typeof(Fraction) };
        private bool parendFlag = false;

        public void SetText(List<Symbol> list)
        {
            sent.Clear();

            //different.Clear();
            //findDifferent(list);

            SetText(main, list);

            print.log("removing...");
            foreach (Symbol s in sent)
                print.log(s);
            print.log("SDFSDFF");
            List<Symbol> temp = views.Keys.ToList();
            foreach (Symbol s in temp)
            {
                if (!sent.Contains(s))
                {
                    print.log("removed " + s);// +", "+ views[s]);
                    Input.graphicsHandler.RemoveChild(views[s].parent, views[s].child);
                    views.Remove(s);
                }
            }
        }

        private void findDifferent(List<Symbol> list)
        {
            foreach (Symbol s in list)
            {
                if (s.GetText().Count > 0 && s != s.GetText()[0])
                {
                    findDifferent(s.GetText());
                }
                else if (!views.Keys.Contains(s))
                {
                    different.Add(s);
                }
            }
        }
                       
        private void SetText(object parent, List<Symbol> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                print.log(i +", "+ "look here " + list[i] + ", " + views.ContainsKey(list[i]) + ", "+ list[i].GetHashCode());

                if (!views.ContainsKey(list[i]))
                {
                    views.Add(list[i], new Graphic(parent, list[i], i));
                    print.log("adding new " + list[i]);

                    if (list[i] == Input.lastAdded)
                    {
                        Input.lastAdded = null;
                    }
                }
                else if (views[list[i]].parent != parent)
                {
                    views[list[i]].SetParent(i, parent);
                }

                if (list[i].GetText().Count > 0 && list[i] != list[i].GetText()[0])
                {
                    Graphic temp = views[list[i]];
                    views.Remove(list[i]);

                    if (unparend.Contains(list[i].GetType()))
                        parendFlag = true;

                    SetText(temp.child, list[i].GetText());

                    if (unparend.Contains(list[i].GetType()))
                        parendFlag = false;

                    views.Add(list[i], temp);
                }

                //if (!(parendFlag && (list[i].text == "(" || list[i].text == ")")))
                    sent.Add(list[i]);
            }
        }

        public object Create(Symbol sender)
        {
            return Input.graphicsHandler.AddText(" " + sender.text + " ");
        }

        public object Create(Edit sender)
        {
            return Input.editField;
        }

        public object Create(Expression sender)
        {
            return Input.graphicsHandler.AddLayout(sender.format);
        }

        public object Create(Fraction sender)
        {
            object layout = Input.graphicsHandler.AddLayout(new Format(orientation: "vertical"));

            return layout;
        }
    }
}
