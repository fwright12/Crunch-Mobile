using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using Xamarin.Forms;
using Xamarin.Forms.Extensions;

namespace Calculator
{
    public class VariableRow : StackLayout
    {
        public static readonly BindableProperty ButtonSizeProperty = BindableProperty.Create("ButtonSize", typeof(double), typeof(VariableRow));

        public double ButtonSize
        {
            get { return (double)GetValue(ButtonSizeProperty); }
            set { SetValue(ButtonSizeProperty, value); }
        }

        public bool Expanded
        {
            get => Settings.VariableRowExpanded;
            set => Settings.VariableRowExpanded = value;
        }

        //private double BarSize => (Orientation == StackOrientation.Horizontal ? RequestedSize.Width : RequestedSize.Height) - ButtonSize - Spacing;
        private double BarSize => OrientedLength - ButtonSize - Spacing;

        private double ExpandButtonRotation => (Orientation == StackOrientation.Horizontal ? 0 : 90) + (Expanded ? 180 + NumRotations * 360 : 0);

        private readonly int RecentlyUsed = 10;
        private readonly double NumRotations = 1;
        private readonly uint TransitionTime = 500;

        private ScrollView Scroll;
        private StackLayout Bar;
        private Button ExpandButton;

        public VariableRow()
        {
            Resources = new ResourceDictionary();
            Resources.Add<Button>(
                (bindable) =>
                {
                    bindable.BindingContext = this;
                    bindable.SetBinding(Button.WidthRequestProperty, "ButtonSize");
                    bindable.SetBinding(Button.HeightRequestProperty, "ButtonSize");
                },
                new Setter { Property = Button.FontSizeProperty, Value = 12 }
                );
            Resources.Add<StackLayout>((bindable) =>
            {
                bindable.BindingContext = this;
                bindable.SetBinding(SpacingProperty, "Spacing");
                bindable.SetBinding(OrientationProperty, "Orientation");
            });
            
#if DEBUG
            //Expanded = true;
#endif

            Bar = new StackLayout();

            Button keyboard = new Button
            {
                Text = "...",
            };
            keyboard.Clicked += (sender, e) =>
            {
                KeyboardManager.NextKeyboard();
            };

            StackLayout variables = new StackLayout();

            Scroll = new ScrollView
            {
                Content = variables,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Never,
            };

            foreach(char c in Settings.Variables)
            {
                Button button = new Button
                {
                    VerticalOptions = LayoutOptions.Start,
                    Text = c.ToString(),
                };
                button.Clicked += (sender, e) =>
                {
                    int index = Settings.Variables.IndexOf(c);

                    if (index > RecentlyUsed - 1)
                    {
                        Settings.Variables.RemoveAt(index);
                        Settings.Variables.Insert(0, c);

                        View v = variables.Children[index];
                        variables.Children.RemoveAt(index);
                        variables.Children.Insert(0, v);

                        Scroll.MakeVisible(v);
                    }

                    KeyboardManager.Type(c.ToString());
                };

                variables.Children.Add(button);
            }

            Bar.Children.Add(keyboard);
            Bar.Children.Add(Scroll);
            
            ExpandButton = new Button
            {
                Text = "<",
            };
            ExpandButton.Clicked += Change;
            
            Children.Add(Bar);
            Children.Add(ExpandButton);

            PropertyChanged += (sender, e) =>
            {
                if (e.IsProperty(BindingContextProperty) && BindingContext is View view)
                {
                    view.MeasureInvalidated += (sender1, e1) => InvalidateMeasure();

                }
            };
            OnPropertyChanged(OrientationProperty.PropertyName);

            //Expanded = !Expanded;
            //Change();
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if (!propertyName.IsProperty(OrientationProperty))
            {
                return;
            }
            
            ExpandButton.Rotation = ExpandButtonRotation;
            Scroll.Orientation = Orientation == StackOrientation.Horizontal ? ScrollOrientation.Horizontal : ScrollOrientation.Vertical;
            
            if (!Expanded)
            {
                Bar.IsVisible = true;
                Bar.SizeChanged += Load;
            }
        }

        private void Change(object sender, EventArgs e) => Change();

        private void Change()
        {
            Expanded = !Expanded;

            ExpandButton.RotateTo(ExpandButtonRotation, TransitionTime, Easing.SinInOut);
            Print.Log(OrientedLength, Width, Height);
            double start = 0;
            double end = BarSize;
            if (!Expanded)
            {
                System.Extensions.Misc.Swap(ref start, ref end);
            }

            Resize(Scroll, 0);

            Bar.IsVisible = true;
            Animation collapse = new Animation(size => Resize(Bar, size), start, end);
            collapse.Commit(this, "Collapse", 1, TransitionTime, Easing.SinInOut, (a, b) =>
            {
                Bar.IsVisible = Expanded;
                Resize(Bar, -1);
                Resize(Scroll, -1);
            });

            /*System.Threading.Tasks.Task<bool> completion = Bar.TranslateTo(Expanded ? 0 : Bar.Width + Spacing, 0, TransitionTime, Easing.SinInOut);
            if (!Expanded)
            {
                Bar.TranslationX = Bar.Width + Spacing;
                //await completion;
            }

            Bar.IsVisible = Expanded;*/
        }

        private void Load(object sender, EventArgs e)
        {
            //if (!(((int)Orientation).ToBool() ^ bar.Width > bar.Height))
            if (Orientation == StackOrientation.Horizontal && Bar.Width > Bar.Height || Orientation == StackOrientation.Vertical && Bar.Height > Bar.Width)
            {
                Bar.IsVisible = Expanded;
                Bar.SizeChanged -= Load;
            }
        }

        private void Resize(View view, double size)
        {
            if (Orientation == StackOrientation.Horizontal)
            {
                view.WidthRequest = size;
            }
            else
            {
                view.HeightRequest = size;
            }
        }

        private double OrientedLength;

        protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
        {
            //return base.OnMeasure(widthConstraint, heightConstraint);

            SizeRequest sr = base.OnMeasure(widthConstraint, heightConstraint);
            Size request = sr.Request;

            if (BindingContext is View bindable)
            {
                if (KeyboardManager.Current == SystemKeyboard.Instance)
                {
                    Page root = this.Parent<Page>();
                    request.Width = OrientedLength = root.Width - root.Padding.Left - root.Padding.Right;
                }
                else if (Orientation == StackOrientation.Horizontal)
                {
                    request.Width = OrientedLength = bindable.Width;
                }
                else
                {
                    request.Height = OrientedLength = bindable.Height;
                }
                
                request = new Size(Math.Min(sr.Request.Width, request.Width), Math.Min(sr.Request.Height, request.Height));
            }

            return new SizeRequest(Expanded ? request : sr.Request, sr.Minimum);
        }
    }
}