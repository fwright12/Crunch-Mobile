using System;
using System.Collections.Generic;
using System.Text;

using Xamarin.Forms;
using Xamarin.Forms.Extensions;
using Crunch.GraphX;

namespace Calculator
{
    public class Answer : Expression, ITouchable
    {
        public event TouchEventHandler Touch;

        private int displayed = 0;

        public void Update(List<Crunch.Engine.Operand> answers) 
        {
            Children.Clear();
            
            if (displayed > answers.Count - 1)
            {
                displayed = 0;
            }

            for (int i = 0; i < answers.Count; i++)
            {
                Children.Add(new Expression(Render.Math(answers[i].ToString())) { IsVisible = i == displayed }.Trim());
            }
        }

        public void NextFormat() => SetFormat(displayed + 1);

        private void SetFormat(int selection)
        {
            if (selection != displayed)
            {
                Children[displayed].IsVisible = false;
                displayed = selection % Children.Count;
                Children[displayed].IsVisible = true;
            }
        }

        public void OnTouch(Point point, TouchState state)
        {
            if (state == TouchState.Up)
            {
                NextFormat();
            }

            Touch?.Invoke(point, state);
        }
    }
}
