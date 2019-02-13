using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Calculator;

using Xamarin.Forms;

namespace Calculator
{
    public abstract class tester
    {
        public abstract void testing();
    }

    public interface IRenderer<TView, TLayout> where TLayout : TView
    {
        void Add(TLayout parent, TView child, int index);
        void Remove(TView sender);

        TView Create(Symbol sender);
        TView Create(Number sender);
        TLayout Create(Expression sender);
        TLayout Create(Fraction sender);
        TLayout Create(Exponent sender);
        TLayout Create(Bar sender);

        TLayout GetParent(TView sender);

        //TLayout Create(Fraction sender);
    }

    public interface IGraphicsHandler
    {
        event CursorListener acursor;
        event OverlapListener cursorMoved;

        object AddLayout(Format f);
        object AddText(string text);
        object AddEditField();
        object AddCursor();
        object AddBar();
        string DispatchKey(string key);
        object GetParent(object child);

        void Select(object sender);
        void Reorder(object child, int pos);

        void AddChild(object parent, object child, int index);
        void RemoveChild(object parent, int index);
        void RemoveChild(object parent, object child);

        bool IsOverlapping(object first, object second);
    }
}
