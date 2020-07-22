﻿using System;
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

    public class DragLinkBehavior : Behavior<View>
    {
        protected override void OnAttachedTo(View bindable)
        {
            base.OnAttachedTo(bindable);

            if (bindable is Link link)
            {
                link.PropertyChanged += GetTouchFromLinkContent;
                link.MathContent.Touch += DragLink;
            }
            else if (bindable is Answer answer)
            {
                answer.Touch += DragLink;
            }
        }

        protected override void OnDetachingFrom(View bindable)
        {
            base.OnDetachingFrom(bindable);

            if (bindable is Link link)
            {
                link.PropertyChanged -= GetTouchFromLinkContent;
                link.MathContent.Touch -= DragLink;
            }
            else if (bindable is Answer answer)
            {
                answer.Touch -= DragLink;
            }
        }

        private void DragLink(object sender, TouchEventArgs e)
        {
            if (e.State != TouchState.Moving)
            {
                return;
            }

            Link link = sender is Answer answer ? new Link(answer) : (sender as View)?.Parent as Link;

            if (link != null)
            {
                Layout<View> dropArea = (sender as View).Parent<AbsoluteLayout>();

                if (!TouchScreen.Active)
                {
                    link.StartDrag(dropArea);
                }
                TouchScreen.BeginDrag(link, dropArea, sender as Answer ?? (View)link);
            }
        }

        private void GetTouchFromLinkContent(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != ContentView.ContentProperty.PropertyName)
            {
                return;
            }

            Link link = (Link)sender;
            link.MathContent.Touch += DragLink;
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

            Update(Value.BetterToString());
        }

        /*public void StartDraggingLinkOn(Layout<View> dropArea)
        {
            BoxView placeholder = StartDrag();

            TouchScreen.Dragging += (sender, e) =>
            {
                
            };
        }*/

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

        public BoxView StartDrag(Layout<View> dropArea)
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
                if (e.Value == DragState.Moving)
                {
                    if (Parent == null)
                    {
                        return;
                    }

                    Tuple<Expression, int> target = MainPage.ExampleDrop(this, dropArea, Parent as AbsoluteLayout);

                    target?.Item1.Insert(target.Item2, placeholder);
                }
                else if (e.Value == DragState.Ended)
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
            MathContent?.GestureRecognizers.Clear();
            MathContent = new Expression(Reader.Render(text));

            TapGestureRecognizer tgr = new TapGestureRecognizer();
            tgr.Tapped += (sender, e) =>
            {
                OnTap();
            };
            MathContent.GestureRecognizers.Add(tgr);

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
