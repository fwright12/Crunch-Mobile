using System;
using System.Collections.Generic;
using System.Text;

using Xamarin.Forms;

namespace Crunch.GraFX
{
    public static class SoftKeyboard
    {
        public static CursorView Cursor => cursor;

        private static CursorView cursor;
        private static int index;

        static SoftKeyboard()
        {
            cursor = new CursorView();
        }

        public static void Type(string str)
        {
            View[] list;
            if (str == "(" || str == ")")
            {
                list = new View[] { new Text(str) };
            }
            else
            {
                list = Render.Math(str.Simple());
            }

            if (list[0] is Fraction && (list[0] as Fraction).Numerator.Children.Count == 0)
            {
                (list[0] as Fraction).Numerator.Fill(cursor.Parent.Children, index - 1);
                index -= (list[0] as Fraction).Numerator.Children.Count;
            }

            cursor.Parent.InsertRange(index++, list);

            if (list[list.Length - 1] is Fraction && (list[list.Length - 1] as Fraction).Denominator.Children.Count == 0)
            {
                (list[list.Length - 1] as Fraction).Denominator.Add(cursor);
                index = 0;
            }
            if (list[list.Length - 1] is Exponent && (list[list.Length - 1] as Exponent).Children.Count == 0)
            {
                (list[list.Length - 1] as Exponent).Add(cursor);
                index = 0;
            }

            Equation.SetAnswer();
        }

        public static bool Delete()
        {
            if (index == 0)
            {
                int loc;
                if (cursor.Parent == Equation.Focus.LHS)
                {
                    return false;
                }
                else if (cursor.Parent.Parent is Fraction)
                {
                    Fraction f = cursor.Parent.Parent as Fraction;
                    loc = f.Index() + 1;
                    lyse(f.Denominator, f.Parent, loc);
                    lyse(f.Numerator, f.Parent, loc);
                }
                else if (cursor.Parent is Expression)
                {
                    loc = cursor.Parent.Index() + 1;
                    lyse(cursor.Parent, cursor.Parent.Parent, loc);
                }
                else
                {
                    return false;
                }

                cursor.Parent.RemoveAt(loc - 1);
                index = cursor.Index();
            }
            else
            {
                index--;
                cursor.Parent.RemoveAt(index);
            }

            Equation.SetAnswer();

            return true;
        }

        private static void lyse(Layout<View> target, Expression destination, int index)
        {
            for (int i = target.Children.Count - 1; i >= 0; i--)
            {
                destination.Insert(index, target.Children[i]);
            }
        }

        public static void Right() => move(1);
        public static void Left() => move(-1);

        public static bool MoveCursor(Expression parent, int i = 0)
        {
            if (parent == cursor.Parent && index == i)
            {
                return false;
            }

            index = i;
            parent.Insert(index, cursor);

            return true;
        }

        private static void move(int direction)
        {
            if (cursor.Parent != null)
            {
                int oldIndex = index;
                var parent = checkIndex(direction, cursor.Parent);

                if (parent != cursor.Parent || index != oldIndex)
                {
                    parent.Insert(index, cursor);
                }
            }
        }

        private static Expression checkIndex(int direction, Expression parent)
        {
            //Stepping once in this direction will keep me in my current expression
            if ((index + direction).IsBetween(0, parent.ChildCount))
            {
                //Step into a new parent
                if (parent.ChildInDirection(index - (direction + 1) / 2, direction) is Expression)
                {
                    parent = parent.ChildInDirection(index - (direction + 1) / 2, direction) as Expression;
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
                if (parent.HasParent() && !(parent.Parent is Equation))
                {
                    index = parent.Index() + (direction + 1) / 2;
                    parent = parent.Parent;
                }
            }

            //I'm somewhere I shouldn't be (like a fraction); keep going
            if (parent.GetType() == typeof(Fraction))
            {
                //index -= (direction + 1) / 2;
                parent = checkIndex(direction, parent);
            }

            return parent;
        }

        private static readonly int extraSpaceForCursor = 2;
        private static readonly int nestedFractionPadding = 10;

        public static void CheckPadding(this Expression e)
        {
            if (e.Children.Count > 0)
            {
                Point extraSpaceNeeded = new Point((e.Children[0] is Fraction).ToInt(), (e.Children[e.Children.Count - 1] is Expression).ToInt());
                int isOnlyChildFraction = (e.Parent is Fraction && e.ChildCount == 1 && e.ChildAfter(-1) is Fraction).ToInt();

                e.Padding = new Thickness(
                    System.Math.Min(10, extraSpaceNeeded.X * extraSpaceForCursor + isOnlyChildFraction * nestedFractionPadding),
                    e.Padding.Top,
                    System.Math.Min(10, extraSpaceNeeded.Y * extraSpaceForCursor + isOnlyChildFraction * nestedFractionPadding),
                    e.Padding.Bottom);
            }
        }

        public static void Fill(this Expression e, IList<View> input, int index) // where T : IMathList, new()
        {
            int imbalance = 0;
            View view = default(View);

            print.log("starting", index);
            //Grab stuff until we hit an operand
            while ((index).IsBetween(0, input.Count - 1) && !(input[index] is Text && (input[index] as Text).Text.Trim().IsOperand() && (input[index] as Text).Text.Length > 1 && imbalance == 0))
            {
                view = input[index];

                input.RemoveAt(index);
                e.Children.Insert(0, view);

                if (view is Text)
                {
                    string s = (view as Text).Text;
                    if (s == "(" || s == ")")
                    {
                        if (s == "(") imbalance++;
                        if (s == ")") imbalance--;
                    }
                }

                index--;
            }

            if (e.ChildCount > 0 && e.ChildBefore(e.ChildCount) is Text && (e.ChildBefore(e.ChildCount) as Text).Text == ")" && e.ChildAfter(-1) is Text && (e.ChildAfter(-1) as Text).Text == "(")
            {
                e.RemoveAt(e.Children.Count - 1);
                e.RemoveAt(0);
            }
        }

        public static View ChildInDirection(this Expression parent, int index, int direction)
        {
            if ((index + direction).IsBetween(0, parent.Children.Count - 1))
            {
                index += direction;
                if (parent.Children[index] is CursorView)
                {
                    return parent.ChildInDirection(index, direction);
                }
                return parent.Children[index];
            }
            return null;
        }

        public static View ChildBefore(this Expression parent, int index) => parent.ChildInDirection(index, -1);

        public static View ChildAfter(this Expression parent, int index) => parent.ChildInDirection(index, 1);

        public static bool HideCursor(this Expression parent, int index)
        {
            return cursor.Parent == parent && index >= SoftKeyboard.index;
        }

        public static bool HideCursor(this Expression parent)
        {
            return cursor.Parent == parent;
        }
    }
}
