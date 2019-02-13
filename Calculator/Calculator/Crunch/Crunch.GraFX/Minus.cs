using System;
using System.Collections.Generic;
using System.Text;

using Xamarin.Forms;

namespace Crunch.GraFX
{
    public class Minus : Text
    {
        private View parent;

        public Minus() : base()
        {
            Text = " - ";
        }

        protected override void OnParentSet()
        {
            base.OnParentSet();

            if (parent != null)
            {
                parent.ChildAdded -= Parent_ChildAdded;
                parent.ChildRemoved -= Parent_ChildAdded;
            }

            change();

            if (Parent != null)
            {
                Parent.ChildAdded += Parent_ChildAdded;
                Parent.ChildRemoved += Parent_ChildAdded;
                parent = Parent;
            }
        }

        private void Parent_ChildAdded(object sender, ElementEventArgs e) => change();

        private void change()
        {
            if (Parent != null)
            {
                int index = this.Index();
                if (//(Parent.ChildAfter(index) is Text) && (Parent.ChildAfter(index) as Text).Text.Trim().IsNumber() &&
                    (index == 0 || (Parent.ChildBefore(index) is Text && (Parent.ChildBefore(index) as Text).Text.Trim().IsOperand())))
                {
                    Text = "-";
                }
                else
                {
                    Text = " - ";
                }
            }
            else if (parent != null)
            {
                parent.ChildAdded -= Parent_ChildAdded;
                parent.ChildRemoved -= Parent_ChildAdded;
            }
        }
    }
}
