using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculator
{
    public class MathView
    {
        public static Dictionary<string, List<string>> supportedFunctions = new Dictionary<string, List<string>>()
        {
            { "Pythagorean Theorem", new List<string> {"(", "(", "a", ")", "^", "(", "2", ")", ")", "+", "(", "(", "b", ")", "^", "(", "2", ")", ")", "=", "(", "(", "c", ")", "^", "(", "2", ")", ")"} }
        };

        public object root;

        public Dictionary<Symbol, Graphic> views = new Dictionary<Symbol, Graphic>();
        public Dictionary<object, Symbol> shown = new Dictionary<object, Symbol>();

        //private bool parendFlag = true;

        private List<Symbol> all;

        public void SetText(List<Symbol> list)
        {
            all = expand(list);

            print.log("removing...");
            //Remove all views that are still being shown, but no longer should be
            List<Symbol> temp = views.Keys.ToList();
            foreach (Symbol s in temp)
            {
                if (!all.Contains(s))
                {
                    print.log("removed " + s + ", " + s.text);// +", "+ views[s]);
                    Input.graphicsHandler.RemoveChild(views[s].parent, views[s].child);
                    shown.Remove(views[s].child);
                    views.Remove(s);
                }
            }

            shown.Clear();
            SetText(root, new Expression(list), true);
        }

        public static List<Symbol> expand(List<Symbol> sender)
        {
            List<Symbol> list = new List<Symbol>();
            foreach (Symbol s in sender)
                list.Add(s);

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].GetText().Count > 0 && list[i] != list[i].GetText()[0])
                {
                    list.InsertRange(i + 1, list[i].GetText());
                }
            }

            return list;
        }
                       
        private void SetText(object parent, Symbol sender, bool parend)
        {
            List<Symbol> list = sender.GetText();

            for (int i = 0; i < list.Count; i++)
            {
                print.log(i +", "+ "look here " + list[i] + ", " + views.ContainsKey(list[i]) + ", "+ list[i].GetHashCode());

                if ((list[i].text == "(" || list[i].text == ")") && !parend)
                {
                    all.Remove(list[i]);
                    list.RemoveAt(i--);
                    continue;
                }
                else if (!views.ContainsKey(list[i]))
                {
                    views.Add(list[i], new Graphic(parent, list[i], i));
                    print.log("adding new " + list[i]);

                    //if (list[i] == Input.lastAdded)
                    if (Input.adding.Contains(list[i]))
                    {
                        //Input.lastAdded = null;
                        Input.adding.Remove(list[i]);
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

                    bool canParend = list[i].GetType() == typeof(Expression) && list[i].format == null && sender.GetType() == typeof(Expression);

                    SetText(temp.child, list[i], canParend);

                    views.Add(list[i], temp);
                }
                else if (!shown.Keys.Contains(views[list[i]].child))
                {
                    shown.Add(views[list[i]].child, list[i]);
                }
            }
        }

        public object Create(Symbol sender)
        {
            if (sender == Symbol.Cursor)
            {
                return Input.cursor;
            }
            else
            {
                return Input.graphicsHandler.AddText(" " + sender.text + " ");
            }
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
            sender.format.Orientation = "vertical";
            return Input.graphicsHandler.AddLayout(sender.format);
        }

        public object Create(Exponent sender)
        {
            return Input.graphicsHandler.AddLayout(new Format(gravity: "abottom"));
        }

        public object Create(Bar sender)
        {
            return Input.graphicsHandler.AddBar();
        }
    }
}
