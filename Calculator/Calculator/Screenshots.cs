#if SAMPLE
using System;
using System.Collections.Generic;
using System.Text;

using Xamarin.Forms;
using Xamarin.Forms.MathDisplay;
using Xamarin.Forms.Extensions;
using Crunch;

namespace Calculator
{
    public partial class MainPage
    {
        /* Store video / screenshots:
         *      Canvas - enter 9*6/8 (top), 5pi+5 (bottom), and sin30 (middle)
         *      Different answer forms - tap through all forms on both
         *      Drag and drop answers - enter + (middle), drag top answer
         *      Flexibility - move cursor back to end of top equation, add .5 on end, go back and change .5 to .25 and 9 to 3^2
         *      Scroll the canvas for more room
         *      Custom variables - enter x^2+5x-2, x = 16^(1/4)
         */

        /* Tips and tricks:
         *      Link source - Type in link equation, tap link
         *      Move calculations - Switch link and source equation
         *      Keyboard cursor mode - Scroll link off screen, 9*6+3
         *      Clear the canvas
         *      Default answer formats - Enter sin30*2/3, change settings, enter again
         */

        private StackLayout Options;

        public void ScreenShot()
        {
            Button screenshots = new Button
            {
                Text = "Screenshots",
            };
            screenshots.Clicked += (sender, e) => SlideShow(ExplainUI);//, AnswerForms, CustomVariables, DragDropAnswers, InfiniteCanvas);

            Button tipsAndTricks = new Button
            {
                Text = "Tips & Tricks",
            };
            tipsAndTricks.Clicked += (sender, e) => { ClearCanvas(); DragDropAnswers(); };

            Button custom = new Button
            {
                Text = "Custom",
            };
            custom.Clicked += (sender, e) => ClearCanvas();

            Options = new StackLayout
            {
                Orientation = StackOrientation.Vertical,
            };
            Options.Children.Add(screenshots);
            Options.Children.Add(tipsAndTricks);
            Options.Children.Add(custom);

            ShowOptions();

            canvas.SizeChanged += (sender, e) =>
            {
                if (canvas.Children.Count == 0)
                {
                    ShowOptions();
                }
            };
        }

        private void ShowOptions() => canvas.Children.Add(Options, new Rectangle(0.5, 0.5, -1, -1), AbsoluteLayoutFlags.PositionProportional);

        private Queue<Action> Screenshots = new Queue<Action>();

        private void SlideShow(params Action[] slides)
        {
            foreach(Action action in slides)
            {
                Screenshots.Enqueue(action);
            }

            NextSlide(null, new TouchEventArgs(Point.Zero, TouchState.Up));
            canvas.Touch += NextSlide;
        }

        private void NextSlide(object sender, TouchEventArgs e)
        {
            if (e.State != TouchState.Up)
            {
                return;
            }

            ClearCanvas();

            if (Screenshots.Count == 0)
            {
                canvas.Touch -= NextSlide;
                ShowOptions();
                //AddCalculation(e.Point, e.State);
                return;
            }

            Screenshots.Dequeue()?.Invoke();
        }

        private void AssignVariables()
        {
            AddCalculation(new Point(canvasScroll.Width * .07, canvasScroll.Height * .4), TouchState.Up);
            SoftKeyboard.Type("x^2+5x+3");
        }

        private void MoveCursor()
        {
            AddCalculation(new Point(canvasScroll.Width * .07, canvasScroll.Height * .6), TouchState.Up);
            SoftKeyboard.Type("9*6/8");

            AddCalculation(new Point(canvasScroll.Width * .37, canvasScroll.Height * .03), TouchState.Up);
            SoftKeyboard.Type("3+sqrt(4)");
        }

        private void AnswerForms()
        {
            canvas.Children.Add(new Equation("9*6/8"), new Rectangle(.19, .15, -1, -1), AbsoluteLayoutFlags.PositionProportional);

            Equation e = new Equation("3^2+5/4");
            Operand o = Crunch.Reader.Evaluate(e.LHS.ToString());
            o = o.Format(Polynomials.Expanded, Numbers.Decimal, Trigonometry.Degrees);
            e.Children.RemoveAt(2);
            e.Children.Add(new Expression(Xamarin.Forms.MathDisplay.Reader.Render(o.ToString())));

            canvas.Children.Add(e, new Rectangle(.61, .78, -1, -1), AbsoluteLayoutFlags.PositionProportional);
        }

        private void CustomVariables()
        {
            AddCalculation(new Point(canvasScroll.Width * .07, canvasScroll.Height * .6), TouchState.Up);
            SoftKeyboard.Type("sqrt(x)+2");
            CalculationFocus.Down();
            SoftKeyboard.Type("4*6.3");

            AddCalculation(new Point(canvasScroll.Width * .37, canvasScroll.Height * .03), TouchState.Up);
            SoftKeyboard.Type("x^2+5x+3");
            CalculationFocus.Down();
            SoftKeyboard.Type("2");
        }

        private void DragDropAnswers()
        {
            AddCalculation(Point.Zero, TouchState.Up);
            SoftKeyboard.Type("9*3.47+4");

            //AddCalculation(Point.Zero, TouchState.Up);
            /*Equation e2 = new Equation();
            e2.LHS.Children.Add(new Link(e1.RHS as Answer));
            e2.Update();
            canvas.Children.Add(e2, Point.Zero);// new Rectangle(.61, .49, -1, -1), AbsoluteLayoutFlags.PositionProportional);
            */
            /*Equation e3 = new Equation("sin");
            e3.LHS.Children.Add(new Link(e1.RHS as Answer));
            e3.Update();
            canvas.Children.Add(e3, new Rectangle(.42, .89, -1, -1), AbsoluteLayoutFlags.PositionProportional);*/
        }

        private void InfiniteCanvas()
        {
            canvas.Children.Add(new Equation("9*6-3"), Point.Zero);
            canvas.Children.Add(new Equation("9/8+5*6"), new Point(canvasScroll.Width - 186, canvasScroll.Height - 52));
            canvas.WidthRequest = canvas.Width + 100;
            canvas.HeightRequest = canvas.Height + 100;

            canvasScroll.HorizontalScrollBarVisibility = ScrollBarVisibility.Always;
            canvasScroll.VerticalScrollBarVisibility = ScrollBarVisibility.Always;
        }

        private void ExplainUI()
        {
            AddCalculation(new Point(canvasScroll.Width * .31, canvasScroll.Height * .64), TouchState.Up);
            SoftKeyboard.Type("log100");
            CalculationFocus.Down();
            SoftKeyboard.Type("9*6.3");

            AddCalculation(new Point(canvasScroll.Width * .08, canvasScroll.Height * .06), TouchState.Up);
            SoftKeyboard.Type("sqrt(x)");
            CalculationFocus.Down();
            SoftKeyboard.Type("5");
            CalculationFocus.Up();
            CalculationFocus.Up();
            SoftKeyboard.Type("x^2+5x+3");
        }
    }
}
#endif
