using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Calculator;

using Graphics;

namespace Calculator
{
    public interface IRenderFactory<TView, TLayout> where TLayout : TView
    {
        void Initialize(TLayout canvas, TView phantomCursor);
        TLayout AddEquation(Point pos);
        Point MovePhantomCursor(TView phantomCursor, Point increase);
        TView GetViewAt(TLayout canvas, Point pos);
        int LeftOrRight(TView view, float x);

        void Add(TLayout parent, TView child, int index = 0);
        void Remove(TView sender);
        TLayout GetParent(TView sender);
        int GetIndex(TView sender);
        void CheckPadding(TLayout view, bool padRight, bool padLeft);
        void AttachClickListener(TView view, Action action);

        TView CreateCursor();
        TView CreatePhantomCursor();
        TLayout CreateBaseLayout();

        TView Create(Text sender);
        TView Create(Number sender);
        TLayout Create(Expression sender);
        TLayout Create(Fraction sender);
    }
}
