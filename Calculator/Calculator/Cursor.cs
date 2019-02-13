using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Calculator.Graphics;

namespace Calculator
{
    public static class Cursor
    {
        public static Container Parent
        {
            get { return parent; }
        }

        public static int Index
        {
            get { return index; }
        }

        private static Action updateGraphics;

        private static Container parent;
        private static int index;

        public static void Initialize(Action action) => updateGraphics = action;

        public static void Right() => move(1);
        public static void Left() => move(-1);
        public static bool Delete()
        {
            if (index == 0)
                return false;

            index--;
            return true;
        }

        public static bool Set(Container parent, int index = 0)
        {
            if (Cursor.parent == parent && Cursor.index == index)
            {
                return false;
            }

            Cursor.parent = parent;
            Cursor.index = index;

            return true;
        }

        private static void setParent(Container newParent)
        {
            parent = parent.Children[index] as Container;

            if (!(parent is Expression))
            {
                Right();
            }
        }

        private static void move(int direction)
        {
            checkIndex(direction);
            updateGraphics();
        }

        private static void checkIndex(int direction)
        {
            if ((index + direction).IsBetween(0, parent.Children.Count))
            {
                if (parent.Children[index + (direction - 1) / 2] is Container)
                {
                    parent = parent.Children[index + (direction - 1) / 2] as Container;
                    index = parent.Children.Count * (direction - 1) / -2;
                }
                else
                {
                    index += direction;
                }
            }
            else
            {
                if (parent.HasParent && parent != MainPage.focus)
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
