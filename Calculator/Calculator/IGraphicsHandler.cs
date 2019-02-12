using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Calculator;

namespace Calculator
{
    //public interface IGraphicsHandler : IGraphicsHandler<object, object> { }

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
