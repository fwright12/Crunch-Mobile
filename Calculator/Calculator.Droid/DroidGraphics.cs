using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Calculator.Droid
{
    public class DroidGraphics : Graphics<LinearLayout>
    {
        private MainActivity main;

        /*public override string answer
        {
            get
            {
                return ((TextView)selected.GetChildAt(selected.ChildCount - 1)).Text;
            }

            set
            {
                ((TextView)selected.GetChildAt(selected.ChildCount - 1)).Text = value;
            }
        }*/

        public DroidGraphics(MainActivity _main)
        {
            main = _main;
        }

        public override void Create(LinearLayout sender)
        {
            main.canvas.AddView(sender);
            expressions.Add(sender);
            selected = sender;
        }

        public override void Add<X>(X sender)
        {
            Console.WriteLine(selected.ChildCount);
            selected.AddView(sender, selected.ChildCount - 1);
        }

        public override void Add<X>(int index, X sender)
        {
            selected.AddView(sender, index);
        }

        public override void Remove<X>(X sender)
        {
            selected.RemoveView(sender);
        }

        public override void Delete(LinearLayout sender)
        {
            expressions.Remove(sender);
            main.canvas.RemoveView(sender);
        }

        public List<string> GetaText(LinearLayout sender)
        {
            List<string> list = new List<string>();

            for (int i = 0; i < sender.ChildCount - 1; i++)
                list.Add(((TextView)sender.GetChildAt(i)).Text);

            return list;
        }
    }
}