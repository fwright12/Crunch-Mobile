using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace Calculator
{
    public class Cursor : BoxView
    {
        public static new Expression Parent => (instance as BoxView).Parent as Expression;
        public static int Index { get => index; }

        public static void Right() => move(1);
        public static void Left() => move(-1);

        private static Cursor instance;
        private static int index;

        public Cursor(bool isMain = false)
        {
            Color = Color.Gray;
            HeightRequest = Input.textHeight;
            WidthRequest = 1;
            VerticalOptions = LayoutOptions.Center;

            if (isMain)
            {
                instance = this;
            }
        }

        public static bool Delete()
        {
            if (index == 0)
            {
                int loc;
                if (Parent == MainPage.focus)
                {
                    return false;
                }
                else if (Parent.Parent is Fraction)
                {
                    Fraction f = Parent.Parent as Fraction;
                    loc = f.Index() + 1;
                    lyse(f.Denominator, f.Parent, loc);
                    lyse(f.Numerator, f.Parent, loc);
                }
                else if (Parent is Expression)
                {
                    loc = Parent.Index() + 1;
                    lyse(Parent, Parent.Parent, loc);
                }
                else
                {
                    return false;
                }

                Parent.RemoveAt(loc - 1);
                UpdateIndex();
            }
            else
            {
                index--;
                Parent.RemoveAt(index);
            }

            MainPage.SetAnswer();

            return true;
        }

        private static void lyse(Layout<View> target, Expression destination, int index)
        {
            for (int i = target.Children.Count - 1; i >= 0 ; i--)
            {
                destination.Insert(index, target.Children[i]);
            }
        }

        public static bool Move(Expression parent, int _index = 0)
        {
            if (parent == Parent && index == _index)
            {
                return false;
            }

            index = _index;
            parent.Insert(index, instance);

            return true;
        }

        public static void Add(View view)
        {
            Parent.Insert(index++, view);
        }

        public static void UpdateIndex() => index = (Parent as Layout<View>).Children.IndexOf(instance);

        private static void move(int direction)
        {
            checkIndex(direction, Parent).Insert(index, instance);
        }

        private static Expression checkIndex(int direction, Expression parent)
        {
            //Stepping once in this direction will keep me in my current expression
            if ((index + direction).IsBetween(0, parent.ChildCount))
            {
                //Step into a new parent
                if (parent.ChildAt(index + (direction - 1) / 2) is Expression)
                {
                    parent = parent.ChildAt(index + (direction - 1) / 2) as Expression;
                    //I either want to be at the very beginning or the very end of the new expression,
                    //depending on which direction I'm going
                    index = parent.ChildCount * (direction - 1) / -2;
                }
                else
                {
                    index += direction;
                }
            }
            //I'm stepping out of this parent
            else
            {
                //Try to go up
                if (parent.HasParent() && parent != MainPage.focus)
                {
                    index = parent.Index() + (direction + 1) / 2;
                    parent = parent.Parent;
                }
            }

            //I'm somewhere I shouldn't be (like a fraction); keep going
            if (parent.GetType() != typeof(Expression) && parent.GetType() != typeof(Exponent))
            {
                parent = checkIndex(direction, parent);
            }

            return parent;
        }

        public override string ToString()
        {
            return "|";
        }
    }
}
