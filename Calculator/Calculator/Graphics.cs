using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculator
{
    public interface IGraphicsHandler
    {
        object AddLayout(bool isHorizontal);
        object AddText(string text);
        object AddEditField();
        string DispatchKey(string key);

        void Select(object sender);

        void AddChild(object parent, object child, int index);
        void RemoveChild(object parent, int index);
        void RemoveChild(object parent, object sender);
        bool hasParent(object sender);
    }

    /*public class Graphics
    {
        public static IGraphicsHandler graphicsHandler;
        public static Dictionary<object, Graphics> findByLayout = new Dictionary<object, Graphics>();
        public static Graphics lastAdded;
        public static bool clickable = true;

        public Graphics parent;
        public object graphicalObject;
        public Dictionary<Symbol, object> views = new Dictionary<Symbol, object>();

        public Expression expression;

        public Graphics(Expression child)
        {
            expression = child;

            graphicalObject = graphicsHandler.AddLayout(true);

            findByLayout.Add(graphicalObject, this);
        }

        public void StartEditing()
        {
            Input.editField = graphicsHandler.AddEditField();

            if (views.ContainsKey(Input.text[Input.pos]))
            {
                graphicsHandler.RemoveChild(graphicalObject, Input.pos);
                views.Remove(Input.text[Input.pos]);
                //views[Input.text[Input.pos]] = Input.editField;
            }
            else
            {
                //views.Add(Input.text[Input.pos], Input.editField);
            }

            //graphicsHandler.AddChild(graphicalObject, Input.editField, Input.pos);
        }

        public Graphics Update()
        {
            return null;

            lastAdded = null;

            List<Symbol> updated = Expression.Parse(Input.text);

            print.log("updating...");
            foreach (Symbol s in updated)
                print.log(s);

            print.log("------------");
            foreach (Symbol s in views.Keys)
                print.log(s.text);
            print.log("------------");

            for (int i = 0; i < updated.Count; i++)
            {
                //If this part of the expression does not yet have a view associated with it
                if (!views.ContainsKey(updated[i]))
                {
                    views.Add(updated[i], Create((dynamic)updated[i]));
                }

                //If the object associated with this part of the expression is not on the current display
                if (!graphicsHandler.hasParent(views[updated[i]]))
                {
                    graphicsHandler.AddChild(graphicalObject, views[updated[i]], i);
                }
            }

            Input.UI.expression.parts = updated;

            /*for (int i = 0; i < Input.text.Count; i++)
            {
                Symbol toAdd = null;

                if (toAdd.text == "/")
                {
                    dynamic denominator;// = new Expression();
                    try
                    {
                        denominator = Input.text[i + 1];
                    }
                    catch
                    {
                        denominator = new Expression();
                    }

                    toAdd = new Fraction(updated[updated.Count - 1], denominator);

                    updated.Insert(i - 1, toAdd);
                    i++;
                }
                else
                {
                    updated.Add(toAdd);
                }

                if (!views.Contains((dynamic)toAdd))
                {
                    graphicsHandler.AddChild(graphicalObject, Create((dynamic)toAdd), updated.Count - 1);
                }
            }*

            /*int correctForParentheses = 0;
            if (expression.parend && !(expression.parts[0].GetType() == typeof(Text) && ((Text)expression.parts[0]).text == "("))
            {
                expression.Insert(0, new Text("("));
                correctForParentheses = 1;
            }
            if (expression.parend && !(expression.parts[expression.parts.Count - 1].GetType() == typeof(Text) && ((Text)expression.parts[expression.parts.Count - 1]).text == "("))
            {
                expression.Insert(expression.parts.Count - 1, new Text(")"));
            }

            //graphicsHandler.Parentheses(graphicalObject, true);

            /*for (int i = 0; i < views.Count; i++)
            {
                //The expression no longer has this, remove it
                if (!expression.parts.Contains(views[i]))
                {
                    views.RemoveAt(i);
                    graphicsHandler.RemoveChild(graphicalObject, i);
                }
            }

            for (int i = 0; i < expression.parts.Count; i++)
            {
                //This has been added to the expression, so add it to UI
                if (!views.Contains(expression.parts[i]))
                {
                    object temp = Create((dynamic)expression.parts[i]);

                    //Insert at i + 1 to account for parentheses at beginning
                    graphicsHandler.AddChild(graphicalObject, temp, i);
                    
                    views.Insert(i, expression.parts[i]);
                }
            }

            return lastAdded;
        }

        public static object Create(Symbol sender)
        {
            object temp = graphicsHandler.AddText(" " + sender.text + " ");
            if (clickable)
                graphicsHandler.Select(temp);

            return temp;
        }

        public static object Create(Number sender)
        {
            print.log("creating number");
            if (Input.editing && Input.editField == null)
            {
                print.log("new edit field");
                object temp = graphicsHandler.AddEditField();
                graphicsHandler.Select(temp);
                return Input.editField = temp;
                //return Input.editing;
            }
            else
            {
                return Create((Symbol)sender);
            }
        }

        public static object Create(Expression sender)
        {
            Graphics temp = new Graphics(sender);

            temp.Update();

            lastAdded = temp;
            return temp.graphicalObject;
        }

        public static object Create(Fraction sender)
        {
            object layout = graphicsHandler.AddLayout(false);

            graphicsHandler.AddChild(layout, Create((dynamic)sender.numerator), 0);
            graphicsHandler.AddChild(layout, Create((dynamic)sender.denominator), 1);

            return layout;
        }
    }

    public class PairedList<TKey, TValue>
    {
        public List<Tuple<TKey, TValue>> list = new List<Tuple<TKey, TValue>>();

        public void Add(TKey key, TValue value)
        {
            list.Add(new Tuple<TKey, TValue>(key, value));
        }

        public void SetKey(TValue value)
        {
        }

        public TValue GetKey(TKey key)
        {
            foreach (Tuple<TKey, TValue> t in list)
                if (t.Item1.Equals(key))
                    return t.Item2;

            throw new Exception();
        }

        public TKey GetValue(TValue value)
        {
            foreach (Tuple<TKey, TValue> t in list)
                if (t.Item2.Equals(value))
                    return t.Item1;

            throw new Exception();
        }

        public bool ContainsKey(TKey key)
        {
            foreach (Tuple<TKey, TValue> t in list)
                if (t.Item1.Equals(key))
                    return true;

            return false;
        }

        public bool ContainsValue(TValue value)
        {
            foreach (Tuple<TKey, TValue> t in list)
                if (t.Item2.Equals(value))
                    return true;

            return false;
        }
    }*/
}
