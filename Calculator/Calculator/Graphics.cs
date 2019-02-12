using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculator
{
    public class Graphics
    {
        public static IGraphicsHandler graphicsHandler;

        public Expression expression;
        public List<Symbol> views = new List<Symbol>();
        public object graphicalObject;

        public int pos;

        public Graphics(object parent)
        {
            //Input.selected = this;
            expression = new Expression();
            graphicalObject = graphicsHandler.Create(expression);
            graphicsHandler.AddChild(parent, graphicalObject, 0);
            graphicsHandler.AddChild(graphicalObject, graphicsHandler.Create(new Operand("")), views.Count);
        }

        public void Update()
        {
            List<Symbol> updated = new List<Symbol>();

            print.log("updating...");
            foreach (Symbol s in expression.parts)
                print.log(s.text);

            for (int i = 0; i < expression.parts.Count; i++)
            {
                //if (expression.parts[i] == views[i])
                //    continue;

                //This has been added to the expression, so add it to UI
                if (!Input.editing && !views.Contains(expression.parts[i]))
                {
                    views.Insert(i, expression.parts[i]);
                    graphicsHandler.AddChild(graphicalObject, graphicsHandler.Create(expression.parts[i]), i);
                }
                //The expression no longer has this, remove it
                else if (!expression.parts.Contains(views[i]))
                {
                    views.RemoveAt(i);
                    graphicsHandler.RemoveChild(graphicalObject, i);
                }
            }

            SetAnswer();
            //graphicsHandler.SetText(graphicalObject, views.Count, expression.answer.value);
        }

        public void SetAnswer()
        {
            graphicsHandler.RemoveChild(graphicalObject, expression.parts.Count + 1);
            graphicsHandler.AddChild(graphicalObject, graphicsHandler.Create(expression.answer), expression.parts.Count + 1);
        }
    }

    public interface IGraphicsHandler
    {
        object Create(Expression sender);
        object Create(Symbol sender);
        object Create(Fraction sender);

        void AddChild(object parent, object child, int index);
        void RemoveChild(object parent, int index);

        void SetText(object parent, int index, string s);
        string GetText(Symbol sender);
    }

    public abstract class Graphics<T>
    {
        public List<T> expressions = new List<T>();
        public T selected;

        /*public abstract string answer
        {
            get;
            set;
        }*/

        public abstract void Create(T sender);
        public abstract void Delete(T sender);
        public abstract void Add<X>(X sender) where X : Android.Widget.TextView;
        public abstract void Add<X>(int index, X sender) where X : Android.Widget.TextView;
        public abstract void Remove<X>(X sender) where X : Android.Widget.TextView;

        public void Evaluate(T sender)
        {
            //answer = new Expression(GetText(sender)).evaluated();
        }

        //public abstract List<string> GetText(T sender);

        /*public List<string> extract(T t)
        {
            List<string> list = new List<string>();

            foreach (U u in expressions[t])
                list.Add(GetText(u));

            return list;
        }*/
    }
}
