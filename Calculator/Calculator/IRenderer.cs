using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Calculator;

using Graphics;

namespace Calculator
{
    public delegate void OverlapListener();

    public interface IRenderer
    {
        //void Add(Layout parent, Symbol child, int index = 0);
        void test();
    }

    public interface IRenderFactory<TView, TLayout>
    {
        void Add(TLayout parent, TView child, int index = 0);
        void Remove(TView sender);
        
        TView Create(Text sender);
        TView Create(Number sender);
        TLayout Create(Layout sender);
        TLayout Create(Fraction sender);
        //TLayout Create(Exponent sender);
        TLayout Create(Bar sender);
        TLayout Create(Equation sender);
        //TLayout Create(Answer sender);
        TLayout Create(Space sender);

        TLayout CreateRoot();

        TLayout BaseLayout();

        TView Create(Cursor sender);
        void IsOverlapping(TView first, TView second, Symbol sender);
        void SetPadding(TLayout sender, int size);

        TLayout GetParent(TView sender);
        int GetIndex(TView sender);
    }
}
