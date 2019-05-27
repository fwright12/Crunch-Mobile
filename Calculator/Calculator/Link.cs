using System;
using System.Collections.Generic;
using System.Text;

using System.Extensions;
using Xamarin.Forms;
using Xamarin.Forms.Extensions;
using Xamarin.Forms.MathDisplay;

namespace Calculator
{
    public class Link : Frame
    {
        private Answer Value;

        public Link(Answer value)
        {
            VerticalOptions = LayoutOptions.Center;
            HorizontalOptions = LayoutOptions.Center;
            BackgroundColor = Color.Transparent;
            Padding = new Thickness(5);
            BorderColor = CrunchStyle.CRUNCH_PURPLE;
            CornerRadius = 5;
            HasShadow = false;

            Value = value;

            Value.FormChanged += Update;
            Print.Log(";lkjasd;lkf", Value.BetterToString());
            Update(new Expression(Reader.Render(Value.BetterToString())));
            
            TapGestureRecognizer tgr = new TapGestureRecognizer();
            tgr.Tapped += (sender, e) =>
            {
                OnTap();
            };
            GestureRecognizers.Add(tgr);
        }

        ~Link()
        {
            Value.FormChanged -= Update;
        }

        private async void OnTap()
        {
            Print.Log("link tapped");
            await MainPage.VisiblePage.Scroll.MakeVisible(Value.Parent<Equation>() ?? (View)Value);

            Color temp = CrunchStyle.CRUNCH_PURPLE;
            uint length = 1000;
            Animation animation = new Animation(v => Value.BackgroundColor = new Color(temp.R, temp.G, temp.B, 1 - v), 0, 1);
            animation.Commit(this, "FadeBackground", length / 255, length, Easing.Linear, (v, c) => Value.BackgroundColor = Color.Transparent, () => false);
        }

        public void StartDrag()
        {
            //Expression e = new Expression(Xamarin.Forms.MathDisplay.Reader.Render(ToString()));
            BoxView placeholder = new BoxView { Color = Color.Transparent, WidthRequest = Value.Width, HeightRequest = Value.Height };
            //MainPage.VisiblePage.MakeDraggable(this, view);

            TouchScreen.Dragging += (state1) =>
            {
                Tuple<Expression, int> target = MainPage.VisiblePage.ExampleDrop(this);

                if (state1 == DragState.Moving && target != null)
                {
                    target.Item1.Insert(target.Item2, placeholder);
                }
                else if (state1 == DragState.Ended)
                {
                    this.Remove();

                    if (placeholder.Parent != null)
                    {
                        Tuple<Expression, int> loc = new Tuple<Expression, int>(placeholder.Parent as Expression, placeholder.Index());
                        placeholder.Remove();

                        Equation root = loc.Item1.Parent<Equation>();
                        if (root != null && root != Value.Parent<Equation>())
                        {
                            loc.Item1.Insert(loc.Item2, this);
                            root.Update();
                        }
                    }
                }
            };
        }

        private Equation ParentEquation;

        protected override void OnParentSet()
        {
            base.OnParentSet();
            ParentEquation = this.Parent<Equation>();
            ParentEquation?.Update();
        }

        private void Update(object sender, ChangedEventArgs<Expression> e) => Update(e.NewValue);

        private void Update(Expression e)
        {
            Content = new Expression(Reader.Render(e.ToString()));

            ((Expression)Content).Touch += (point, state) =>
            {
                if (state == TouchState.Moving)
                {
                    TouchScreen.BeginDrag(this, MainPage.VisiblePage.PhantomCursorField);
                    StartDrag();
                }
            };
            ((Expression)Content).Tapped += (point) =>
            {
                OnTap();
            };
            
            ParentEquation?.Update();
        }

        public override string ToString() => ((Expression)Content).ToString();
    }
}
