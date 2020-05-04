using System;
using System.Collections.Generic;
using System.Text;

using System.Extensions;
using Xamarin.Forms;
using Xamarin.Forms.Extensions;
using Xamarin.Forms.MathDisplay;

namespace Calculator
{
    public class Variable : Link
    {
        public Variable(char name, Answer value)
        {
            VerticalOptions = LayoutOptions.Center;
            HorizontalOptions = LayoutOptions.Center;
            BackgroundColor = Color.Transparent;
            Padding = new Thickness(5);
            BorderColor = Color.Transparent;
            CornerRadius = 0;

            Value = value;
            Value.FormChanged += Update;

            Update(name.ToString());
        }
    }

    public class Link : Frame, IMathView
    {
        public Expression MathContent
        {
            get => Content as Expression;
            set => Content = value;
        }

        public double Middle => MathContent.Middle;
        public double FontSize
        {
            set => MathContent.FontSize = value;
        }

        protected Answer Value;

        protected Link()
        {

        }

        public Link(Answer value)
        {
            VerticalOptions = LayoutOptions.Center;
            HorizontalOptions = LayoutOptions.Center;
            BackgroundColor = Color.Transparent;
            Padding = new Thickness(5);
            Margin = new Thickness(1);
            CornerRadius = 5;
            HasShadow = false;
            SetDynamicResource(BorderColorProperty, "ControlColor");

            Value = value;
            Value.FormChanged += Update;

            Print.Log(";lkjasd;lkf", Value.BetterToString());
            Update(Value.BetterToString());
            
            TapGestureRecognizer tgr = new TapGestureRecognizer();
            tgr.Tapped += (sender, e) =>
            {
                OnTap();
            };
            GestureRecognizers.Add(tgr);
        }

        private async void OnTap()
        {
            Print.Log("link tapped");

            ScrollView scroll = this.Parent<ScrollView>();
            if (scroll != null)
            {
                await scroll.ScrollToAsync(Value.Parent<Equation>() ?? (View)Value, ScrollToPosition.MakeVisible, true);
                //await scroll.MakeVisible(Value.Parent<Equation>() ?? (View)Value);
            }

            Color temp = BorderColor;
            uint length = 1000;
            Animation animation = new Animation(v => Value.BackgroundColor = new Color(temp.R, temp.G, temp.B, 1 - v), 0, 1);
            animation.Commit(this, "FadeBackground", length / 255, length, Easing.Linear, (v, c) => Value.BackgroundColor = Color.Transparent, () => false);
        }

        public BoxView StartDrag()
        {
            BoxView placeholder = new BoxView
            {
                Color = Color.Transparent,
                WidthRequest = Value.Width,
                HeightRequest = Value.Height
            };
            //MainPage.VisiblePage.MakeDraggable(this, view);

            TouchScreen.Dragging += (sender, e) =>
            {
                if (e.Value == DragState.Ended)
                {
                    this.Remove();

                    if (placeholder.Parent != null)
                    {
                        Tuple<Expression, int> loc = new Tuple<Expression, int>(placeholder.Parent as Expression, placeholder.Index());
                        placeholder.Remove();

                        Equation root = loc.Item1.Parent<Equation>();
                        if (root != null && !CanAddDependency(Value, root))
                        {
                            loc.Item1.Insert(loc.Item2, this);
                            //root.Update();
                        }
                        else
                        {
                            Destroy();
                        }
                    }
                }
            };

            return placeholder;
        }

        public void Destroy()
        {
            this.Remove();
            Value.FormChanged -= Update;
        }

        private static bool CanAddDependency(Answer source, Element dependent)
        {
            Answer next = (dependent as Equation ?? dependent?.Parent<Equation>())?.RHS as Answer;

            if (next == null)
            {
                return false;
            }
            else if (next == source)
            {
                return true;
            }
            
            foreach (Element e in next.GetListeners())
            {
                if (CanAddDependency(source, e))
                {
                    return true;
                }
            }

            VariableAssignment va = (next as Element).Parent?.Parent as VariableAssignment;
            if (va != null)
            {
                foreach(Equation e in va.GetDependencies())
                {
                    if (CanAddDependency(source, e))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private Equation ParentEquation;

        protected override void OnParentSet()
        {
            base.OnParentSet();
            ParentEquation = this.Parent<Equation>();
        }

        protected void Update(object sender, ChangedEventArgs<Expression> e)
        {
            Update(e.NewValue.ToString());
            //ParentEquation?.Update();
        }

        protected void Update(string text)
        {
            MathContent = new Expression(Reader.Render(text));

            MathContent.Tapped += (point) =>
            {
                OnTap();
            };

            /*MathContent.Touch += (sender, e) =>
            {
                if (e.State == TouchState.Moving)
                {
                    TouchScreen.BeginDrag(this, MainPage.CanvasArea);
                    this.StartDrag();
                }
            };*/
        }

        public override string ToString() => MathContent.ToString();

        public string ToLatex() => MathContent.ToLatex();
    }
}
