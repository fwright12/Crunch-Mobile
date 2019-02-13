using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculator
{
    public class GraphicsEngine
    {
        public GraphicsEngine<Android.Views.View, Android.Views.ViewGroup> AndroidGraphics = new GraphicsEngine<Android.Views.View, Android.Views.ViewGroup>();

        public void Create(Symbol sender, Symbol parent, int index)
        {
            //AndroidGraphics.Create(sender as dynamic);
        }

        public void SetText(List<Symbol> list)
        {
            AndroidGraphics.Start(list);
            //SetText(root, new Expression(list), true);
        }
    }

    public class GraphicsEngine<TView, TLayout> where TLayout : TView
    {
        public static IRenderer<TView, TLayout> RenderFactory;

        public BiDictionary<Symbol, TView> viewLookup = new BiDictionary<Symbol, TView>();
        public BiDictionary<Symbol, TView> textLookup = new BiDictionary<Symbol, TView>();
        public BiDictionary<Symbol, TLayout> layoutLookup = new BiDictionary<Symbol, TLayout>();

        public void Start(List<Symbol> list)
        {
            var all = expand(list);

            print.log("removing...");
            //Remove all views that are still being shown, but no longer should be
            var temp = new Symbol[viewLookup.value1List.Count];
            viewLookup.value1List.CopyTo(temp);
            foreach (Symbol s in temp)
            {
                if (!all.Contains(s))
                {
                    print.log("removed " + s + ", " + s.text);// +", "+ views[s]);
                    //Input.graphicsHandler.RemoveChild(views[s].parent, views[s].child);
                    RenderFactory.Remove(viewLookup[s]);
                    //shown.Remove(views[s].child);
                    viewLookup.Remove(s);
                }
            }

            SetText((TLayout)Input.selected.root, new Expression(list), false);
        }

        private void SetText(TLayout parent, Symbol sender, bool parend)
        {
            List<Symbol> list = sender.GetText();

            for (int i = 0; i < list.Count; i++)
            {
                print.log(i + ", " + "look here " + list[i].text + ", " + viewLookup.Contains(list[i]) + ", " + list[i].GetHashCode());

                Symbol symbol = list[i];

                /*if ((list[i].text == "(" || list[i].text == ")") && !parend)
                {
                    all.Remove(list[i]);
                    list.RemoveAt(i--);
                    continue;
                }
                else*/ if (!viewLookup.Contains(symbol))
                {
                    //numberFlag = i > 0 && i < list.Count - 1 && list[i - 1] is Number && list[i + 1] is Number;

                    var view = RenderFactory.Create(symbol as dynamic);
                    RenderFactory.Add(parent, view, i);
                    viewLookup.Add(symbol, view); //new Graphic(parent, list[i], i)
                    print.log("adding new " + list[i]);

                    if (Input.adding.Contains(symbol))
                    {
                        Input.adding.Remove(symbol);
                    }
                }
                else if (!RenderFactory.GetParent(viewLookup[symbol]).Equals(parent))
                {
                    //views[list[i]].SetParent(i, parent);
                    RenderFactory.Add(parent, viewLookup[symbol], i);
                }

                if (list[i].GetText().Count > 0 && list[i] != list[i].GetText()[0])
                {
                    var temp = viewLookup[symbol];
                    viewLookup.Remove(symbol);

                    bool canParend = list[i].GetType() == typeof(Expression) && list[i].format == null && sender.GetType() == typeof(Expression);

                    SetText((TLayout)temp, list[i], canParend);

                    viewLookup.Add(symbol, temp);
                }
                /*else if (!shown.Keys.Contains(views[list[i]].child))
                {
                    shown.Add(views[list[i]].child, list[i]);
                }*/
            }
        }

        public List<Symbol> expand(List<Symbol> sender)
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
    }
}
