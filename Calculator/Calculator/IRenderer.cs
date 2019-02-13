using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Calculator;

using Graphics;

namespace Calculator
{
    public interface IView
    {

    }

    public interface ILayout : IView
    {
        void Add(IView child, int index = 0);
    }

    public class AndroidView : IView
    {
        public Android.Views.View obj;
    }

    public class AndroidLayout : ILayout
    {
        public Android.Views.ViewGroup obj;

        public AndroidLayout(Android.Views.ViewGroup Obj)
        {
            obj = Obj;
        }

        public void Add(IView child, int index = 0)
        {
            obj.AddView(child as dynamic, index);
        }
    }

    public interface IRenderFactory<TView, TLayout>
    {
        void Add(TLayout parent, TView child, int index = 0);
        void Remove(TView sender);

        TLayout BaseLayout();
        TView Create(Text sender);
        TView Create(Number sender);
        TLayout Create(Expression sender);
        TLayout Create(Fraction sender);
        TView Create(Cursor sender);

        void SetPadding(TLayout sender, int left, int right);
    }
}
