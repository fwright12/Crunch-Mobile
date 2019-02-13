using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Graphics;

namespace Calculator
{
    public static class Cursor
    {
        public static Layout Parent
        {
            get
            {
                return parent;
            }
        }
        public static int Index
        {
            get
            {
                return index;
            }
        }

        public static Layout root;

        private static Layout parent;
        private static int index;

        public static bool Set(Layout parent, int index = 0)
        {
            if (Cursor.parent == parent && Cursor.index == index)
            {
                return false;
            }

            Cursor.parent = parent;
            Cursor.index = index;

            return true;
        }

        private static void setParent(Layout newParent)
        {
            parent = parent.Children[index] as Layout;

            if (!(parent is Expression))
            {
                Right();
            }
        }

        public static void Right() => checkIndex(1);
        public static void Left() => checkIndex(-1);
        public static void Delete() => index--;

        private static void checkIndex(int direction)
        {
            if ((index + direction).IsBetween(0, parent.Children.Count))
            {
                if (parent.Children[index + (direction - 1) / 2] is Layout)
                {
                    parent = parent.Children[index + (direction - 1) / 2] as Layout;
                    index = parent.Children.Count * (direction - 1) / -2;
                }
                else
                {
                    index += direction;
                }
            }
            else
            {
                if (parent.HasParent && parent != root)
                {
                    index = parent.Parent.Children.IndexOf(parent) + (direction + 1) / 2;
                    parent = parent.Parent;
                }
            }

            if (!(parent is Expression))
            {
                checkIndex(direction);
            }
        }
    }
}
