using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Calculator;

namespace Calculator
{
    public interface IGraphicsHandler
    {
        object AddLayout(Format f);
        object AddText(string text);
        object AddEditField();
        object AddCursor();
        object AddBar();
        string DispatchKey(string key);

        void Select(object sender);
        void Reorder(object child, int pos);

        void AddChild(object parent, object child, int index);
        void RemoveChild(object parent, int index);
        void RemoveChild(object parent, object child);
    }
}
