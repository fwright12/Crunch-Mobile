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

        public VisualElement LengthBinding
        {
            get => _LengthBinding;
            set
            {
                if (_LengthBinding != null)
                {
                    _LengthBinding.MeasureInvalidated -= InvalidateMeasure;
                }
                _LengthBinding = value;
                _LengthBinding.MeasureInvalidated += InvalidateMeasure;

                InvalidateMeasure();
                //Reload();
            }
        }
        private VisualElement _LengthBinding;

        private double OrientedLength;// => LengthBinding == null ? 0 : (Orientation == StackOrientation.Horizontal ? LengthBinding.Width : LengthBinding.Height);
        private double BarSize => OrientedLength - ButtonSize - Spacing;

        private double ExpandButtonRotation => (Orientation == StackOrientation.Horizontal ? 0 : 90) + (Expanded ? 180 + NumRotations * 360 : 0);

        private readonly int RecentlyUsed = 10;
        private readonly double NumRotations = 1;
        private readonly uint TransitionTime = 500;

        private readonly StackLayout Variables;
        private readonly ScrollView Scroll;
        private readonly StackLayout Bar;
        private readonly LabelButton ExpandButton;

        public VariableRow()
        {
            this.WhenDescendantAdded<Button>((button) =>
            {
                button.BindingContext = this;
                button.FontSize = 12;

                button.SetBinding(Button.WidthRequestProperty, "ButtonSize");
                button.SetBinding(Button.HeightRequestProperty, "ButtonSize");
            });
            this.WhenDescendantAdded<StackLayout>((stacklayout) =>
            {
                stacklayout.BindingContext = this;
                stacklayout.SetBinding(SpacingProperty, "Spacing");
                stacklayout.SetBinding(OrientationProperty, "Orientation");
            });

#if DEBUG
            Expanded = true;
#endif

            LabelButton keyboard = new Button
            {
                Text = "🌐",
                FontSize = 15,
                FontFamily = CrunchStyle.SYMBOLA_FONT,
            };
            keyboard.Button.Clicked += (sender, e) =>
            {
                KeyboardManager.NextKeyboard();
            };

            Bar = new StackLayout
            {
                Children =
                {
                    keyboard,
                    (Scroll = new ScrollView
                    {
                        Content = Variables = new StackLayout { },
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                        VerticalOptions = LayoutOptions.FillAndExpand,
                        HorizontalScrollBarVisibility = ScrollBarVisibility.Never,
                    })
                }
            };

            ExpandButton = new Button
            {
                Text = "◁",
                FontFamily = CrunchStyle.SYMBOLA_FONT,
                FontSize = 15
            };
            ExpandButton.Button.Clicked += Change;

            Children.Add(Bar);
            Children.Add(ExpandButton);

            foreach (char c in Settings.Variables)
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

                        View v = Variables.Children[index];
                        Variables.Children.RemoveAt(index);
                        Variables.Children.Insert(0, v);

                        Scroll.MakeVisible(v);
                    }

                    KeyboardManager.Type(c.ToString());
                };

                Variables.Children.Add(button);
            }

            OnPropertyChanged(OrientationProperty.PropertyName);
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

            Reload();
        }

        private void Change(object sender, EventArgs e) => Change();

        private void Change()
        {
            Expanded = !Expanded;

            ExpandButton.RotateTo(ExpandButtonRotation, TransitionTime, Easing.SinInOut);
            //Print.Log(OrientedLength, Width, Height);
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

        private void Reload()
        {
            /*Bar.WidthRequest = -1;
            Bar.HeightRequest = -1;
            Resize(Bar, BarSize);*/

            if (!Expanded)
            {
                Bar.IsVisible = true;
                Bar.SizeChanged += Load;
            }
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

        private void InvalidateMeasure(object sender, EventArgs e) => InvalidateMeasure();

        protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
        {
            //Print.Log("measuring variables", widthConstraint, heightConstraint, LengthBinding, LengthBinding.Bounds.Size);
            //return base.OnMeasure(widthConstraint, heightConstraint);

            SizeRequest sr = base.OnMeasure(widthConstraint, heightConstraint);
            /*if (!double.IsInfinity(heightConstraint))
                sr.Request = new Size(sr.Request.Width, heightConstraint);
            return sr;
            if (Orientation == StackOrientation.Horizontal)
            {

            }*/

            Size request = sr.Request;

            if (KeyboardManager.Current == SystemKeyboard.Instance)
            {
                Page root = this.Parent<Page>();
                OrientedLength = root.Width - root.Padding.Left - root.Padding.Right;
            }
            else if (Orientation == StackOrientation.Horizontal)
            {
                OrientedLength = LengthBinding.Width;
            }
            else
            {
                OrientedLength = LengthBinding.Height;
            }

            if (Orientation == StackOrientation.Horizontal || KeyboardManager.Current == SystemKeyboard.Instance)
            {
                request.Width = Math.Min(sr.Request.Width, OrientedLength);
            }
            else if (Orientation == StackOrientation.Vertical)
            {
                request.Height = Math.Min(sr.Request.Height, OrientedLength);
            }

            return new SizeRequest(request, sr.Minimum);
        }
    }
}