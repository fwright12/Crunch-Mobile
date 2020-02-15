#if DEBUG
using System;
using System.Collections.Generic;
using System.Text;

using Xamarin.Forms;
using Xamarin.Forms.MathDisplay;
using Xamarin.Forms.Extensions;
using Crunch;

namespace Calculator
{
    public static class Screenshots
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

        public static BindableProperty InSampleModeProperty = BindableProperty.CreateAttached("InSampleMode", typeof(bool), typeof(Screenshots), false);

        public static bool GetInSampleMode(this App app) => (bool)app.GetValue(InSampleModeProperty);

        public static void SetInSampleMode(this App app, bool value) => app.SetValue(InSampleModeProperty, value);

        private static MainPage MainPage;
        private static StackLayout Options;

        public static void ScreenShot()
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
            tipsAndTricks.Clicked += (sender, e) => { DragDropAnswers(); };

            /*Button custom = new Button
            {
                Text = "Custom",
            };
            custom.Clicked += (sender, e) => ClearCanvas();*/

            Options = new StackLayout
            {
                Orientation = StackOrientation.Vertical,
            };
            Options.Children.Add(screenshots);
            Options.Children.Add(tipsAndTricks);
            //Options.Children.Add(custom);

            //ShowOptions();

            /*Canvas.SizeChanged += (sender, e) =>
            {
                if (Canvas.Children.Count == 0)
                {
                    ShowOptions();
                }
            };*/
        }

        //private void ShowOptions() => Canvas.Children.Add(Options, new Rectangle(0.5, 0.5, -1, -1), AbsoluteLayoutFlags.PositionProportional);

        private static Queue<Action> Screens = new Queue<Action>();

        private static void SlideShow(params Action[] slides)
        {
            foreach(Action action in slides)
            {
                Screens.Enqueue(action);
            }

            NextSlide(null, new TouchEventArgs(Point.Zero, TouchState.Up));
            //Canvas.Touch += NextSlide;
        }

        private static void NextSlide(object sender, TouchEventArgs e)
        {
            if (e.State != TouchState.Up)
            {
                return;
            }

            if (Screens.Count == 0)
            {
                //Canvas.Touch -= NextSlide;
                //ShowOptions();
                //AddCalculation(e.Point, e.State);
                return;
            }

            Screens.Dequeue()?.Invoke();
        }

        private static void ExplainUI()
        {
            /*AddCalculation(new Point(CanvasScroll.Width * .31, CanvasScroll.Height * .64));
            SoftKeyboard.Type("log100");
            CalculationFocus.Down();
            SoftKeyboard.Type("9*6.3");

            AddCalculation(new Point(CanvasScroll.Width * .08, CanvasScroll.Height * .06));
            SoftKeyboard.Type("sqrt(x)");
            CalculationFocus.Down();
            SoftKeyboard.Type("5");
            CalculationFocus.Up();
            CalculationFocus.Up();
            SoftKeyboard.Type("x^2+5x+3");*/
        }

        private static void DragDropAnswers()
        {
            /*AddCalculation(Point.Zero);
            SoftKeyboard.Type("9*3.47+4");
            
            //AddCalculation(Point.Zero, TouchState.Up);
            Equation e2 = new Equation();
            e2.LHS.Children.Add(new Link(e1.RHS as Answer));
            e2.Update();
            canvas.Children.Add(e2, Point.Zero);// new Rectangle(.61, .49, -1, -1), AbsoluteLayoutFlags.PositionProportional);
            */
            /*Equation e3 = new Equation("sin");
            e3.LHS.Children.Add(new Link(e1.RHS as Answer));
            e3.Update();
            canvas.Children.Add(e3, new Rectangle(.42, .89, -1, -1), AbsoluteLayoutFlags.PositionProportional);*/
        }

        /*private void AssignVariables()
        {
            AddCalculation(new Point(CanvasScroll.Width * .07, CanvasScroll.Height * .4));
            SoftKeyboard.Type("x^2+5x+3");
        }

        private void MoveCursor()
        {
            AddCalculation(new Point(CanvasScroll.Width * .07, CanvasScroll.Height * .6));
            SoftKeyboard.Type("9*6/8");

            AddCalculation(new Point(CanvasScroll.Width * .37, CanvasScroll.Height * .03));
            SoftKeyboard.Type("3+sqrt(4)");
        }

        private void AnswerForms()
        {
            Canvas.Children.Add(new Equation("9*6/8"), new Rectangle(.19, .15, -1, -1), AbsoluteLayoutFlags.PositionProportional);

            Equation e = new Equation("3^2+5/4");
            Operand o = Crunch.Math.Evaluate(e.LHS.ToString());
            o = o.Format(Polynomials.Expanded, Numbers.Decimal, Trigonometry.Degrees);
            e.Children.RemoveAt(2);
            e.Children.Add(new Expression(Xamarin.Forms.MathDisplay.Reader.Render(o.ToString())));

            Canvas.Children.Add(e, new Rectangle(.61, .78, -1, -1), AbsoluteLayoutFlags.PositionProportional);
        }

        private void CustomVariables()
        {
            AddCalculation(new Point(CanvasScroll.Width * .07, CanvasScroll.Height * .6));
            SoftKeyboard.Type("sqrt(x)+2");
            CalculationFocus.Down();
            SoftKeyboard.Type("4*6.3");

            AddCalculation(new Point(CanvasScroll.Width * .37, CanvasScroll.Height * .03));
            SoftKeyboard.Type("x^2+5x+3");
            CalculationFocus.Down();
            SoftKeyboard.Type("2");
        }

        private void InfiniteCanvas()
        {
            Canvas.Children.Add(new Equation("9*6-3"), Point.Zero);
            Canvas.Children.Add(new Equation("9/8+5*6"), new Point(CanvasScroll.Width - 186, CanvasScroll.Height - 52));
            Canvas.WidthRequest = Canvas.Width + 100;
            Canvas.HeightRequest = Canvas.Height + 100;

            CanvasScroll.HorizontalScrollBarVisibility = ScrollBarVisibility.Always;
            CanvasScroll.VerticalScrollBarVisibility = ScrollBarVisibility.Always;
        }*/
    }
}
#endif
