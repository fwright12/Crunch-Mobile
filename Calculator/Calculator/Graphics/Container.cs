using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace Calculator.Graphics
{
    public abstract class Container : Symbol
    {
        public ReadOnlyCollection<Symbol> Children
        {
            get
            {
                return new ReadOnlyCollection<Symbol>(children);
            }
        }

        public override bool selectable
        {
            get
            {
                return _selectable;
            }
            set
            {
                foreach (Symbol s in children)
                {
                    s.selectable = value;
                }
                _selectable = value;
            }
        }

        private List<Symbol> children = new List<Symbol>();

        public void Add(Symbol symbol)
        {
            Add(symbol, children.Count);
        }

        public virtual void Add(Symbol symbol, int index)
        {
            //Remove from old list
            if (symbol.Parent != null)
            {
                symbol.Parent.children.Remove(symbol);
            }
            symbol.Parent = this;

            //if (symbol != Control.cursor)
            //{
            children.Insert(index, symbol);

            /*if (Control.cursor.Parent == symbol.Parent && index <= Control.cursor.Index)
            {
                Control.cursor.Right();
            }*/
            //}
        }

        public void Remove(Symbol symbol)
        {
            children.Remove(symbol);
        }
    }
}
