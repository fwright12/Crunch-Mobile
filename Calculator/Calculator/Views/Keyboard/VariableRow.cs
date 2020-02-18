using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using System.Extensions;
using Xamarin.Forms;
using Xamarin.Forms.Extensions;

namespace Calculator
{
    public class VariableRow : StackLayout
    {
        public static readonly BindableProperty ButtonSizeProperty = BindableProperty.Create("ButtonSize", typeof(double), typeof(VariableRow));

        public static readonly BindableProperty ExpandedProperty = BindableProperty.Create("Expanded", typeof(bool), typeof(VariableRow));

        public double ButtonSize
        {
            get { return (double)GetValue(ButtonSizeProperty); }
            set { SetValue(ButtonSizeProperty, value); }
        }

        public bool Expanded
        {
            //get => (bool)GetValue(ExpandedProperty);
            //set => SetValue(ExpandedProperty, value);
            get => App.VariableRowExpanded.Value;
            set => App.VariableRowExpanded.Value = value;
        }

        public VisualElement LengthBinding
        {
            get => _LengthBinding;
            set
            {
                if (_LengthBinding != null)
                {
                    //_LengthBinding.MeasureInvalidated -= InvalidateMeasure;
                }
                _LengthBinding = value;
                //_LengthBinding.MeasureInvalidated += InvalidateMeasure;

                InvalidateMeasure();
                //Reload();
            }
        }
        private VisualElement _LengthBinding;

        private double OrientedLength;// => LengthBinding == null ? 0 : (Orientation == StackOrientation.Horizontal ? LengthBinding.Width : LengthBinding.Height);
        private double BarSize;// => OrientedLength - ButtonSize - Spacing;

        private readonly int RecentlyUsed = 10;
        private readonly uint NumRotations = 1;
        private readonly double TransitionSpeed = 0.5;

        private readonly StackLayout Variables;
        private readonly ScrollView Scroll;
        private readonly StackLayout Bar;
        private readonly LabelButton ExpandButton;

        public VariableRow()
        {
            Resources.Add(new Style(typeof(Button))
            {
                Setters =
                {
                    new Setter { Property = Button.FontSizeProperty, Value = 12 }
                }
            });
            /*this.WhenDescendantAdded<Button>((button) =>
            {
                button.BindingContext = this;
                //button.FontSize = 12;

                button.SetBinding(Button.WidthRequestProperty, "ButtonSize");
                button.SetBinding(Button.HeightRequestProperty, "ButtonSize");
            });*/
            this.WhenDescendantAdded<StackLayout>((stacklayout) =>
            {
                stacklayout.BindingContext = this;
                stacklayout.SetBinding(SpacingProperty, "Spacing");
                stacklayout.SetBinding(OrientationProperty, "Orientation");
            });

#if DEBUG
            if (Device.RuntimePlatform == Device.Android)
            {
                Expanded = false;
            }
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
                        WidthRequest = 0,
                        HeightRequest = 0
                    })
                }
            };

            App.VariableRowExpanded.Bind<bool>(App.VariableRowExpanded.ValueProperty, value =>
            {
                this.SetValues(value ? LayoutOptions.Fill : LayoutOptions.End, HorizontalOptionsProperty, VerticalOptionsProperty);
                Bar.SetValues(value ? LayoutOptions.FillAndExpand : LayoutOptions.Fill, HorizontalOptionsProperty, VerticalOptionsProperty);
                Bar.SetValues(value ? -1 : 1, WidthRequestProperty, HeightRequestProperty);
            });
            Bar.SizeChanged += (sender, e) =>
            {
                Bar.Opacity = ((Orientation == StackOrientation.Horizontal && Bar.Width > 1) || (Orientation == StackOrientation.Vertical && Bar.Height > 1)).ToInt();
            };
            //Bar.SetBinding<double, double>(OpacityProperty, Bar, "Width", value => value > 1 ? 1 : 0);
            ExpandButton = new Button
            {
                Text = "◁",
                FontFamily = CrunchStyle.SYMBOLA_FONT,
                FontSize = 15,
            };
            ExpandButton.Button.Clicked += Change;

            Children.Add(Bar);
            Children.Add(ExpandButton);

            foreach (char c in App.Variables)
            {
                Button button = new Button
                {
                    VerticalOptions = LayoutOptions.Start,
                    Text = c.ToString(),
                };
                button.Clicked += (sender, e) =>
                {
                    int index = App.Variables.IndexOf(c);

                    if (index > RecentlyUsed - 1)
                    {
                        App.Variables.RemoveAt(index);
                        App.Variables.Insert(0, c);

                        View v = Variables.Children[index];
                        Variables.Children.RemoveAt(index);
                        Variables.Children.Insert(0, v);

                        Scroll.MakeVisible(v);
                    }

                    KeyboardManager.Type(c.ToString());
                };

                Variables.Children.Add(button);
            }

            this.Bind<double>(ButtonSizeProperty, value =>
            {
                keyboard.SizeRequest(ButtonSize);

                foreach(View child in Variables.Children)
                {
                    child.SizeRequest(ButtonSize);
                }

                ExpandButton.SizeRequest(ButtonSize);
            });

            Scroll.SetBinding<ScrollOrientation, StackOrientation>(ScrollView.OrientationProperty, this, "Orientation", value => value == StackOrientation.Horizontal ? ScrollOrientation.Horizontal : ScrollOrientation.Vertical);

            this.Bind<StackOrientation>(OrientationProperty, value =>
            {
                ExpandButton.Rotation = GetExpandButtonRotation(Expanded, value);
            });
            //OnPropertyChanged(OrientationProperty.PropertyName);
        }

        private double GetExpandButtonRotation(bool expanded, StackOrientation orientation) => (orientation == StackOrientation.Horizontal ? 0 : 90) + (expanded ? 180 + NumRotations * 360 : 0);

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if (!propertyName.IsProperty(OrientationProperty))
            {
                return;
            }
            
            //ExpandButton.Rotation = ExpandButtonRotation;
            //Scroll.Orientation = Orientation == StackOrientation.Horizontal ? ScrollOrientation.Horizontal : ScrollOrientation.Vertical;
            
            //Reload();
        }

        private Size LastAllocatedSize;

        private void Change(object sender, EventArgs e) => Change();

        private void Change()
        {            
            bool horizontal = Orientation == StackOrientation.Horizontal;
            BindableProperty dimension = horizontal ? WidthProperty : HeightProperty;
            BindableProperty dimensionRequest = horizontal ? WidthRequestProperty : HeightRequestProperty;

            VisualElement parent = this.Parent<VisualElement>();
            //(horizontal ? LastAllocatedSize.Width : LastAllocatedSize.Height)
            double start = Bar.GetValue<double>(dimension);
            double end = Expanded ? 0 : (horizontal ? AvailableSpace.Width : AvailableSpace.Height) - ButtonSize - Spacing;
            //double end = Expanded ? 0 : parent.GetValue<double>(dimension) - ButtonSize - Spacing - 20;

            // Get the rotation for the new Expanded value (which will be the opposite of what it is now)
            double rotation = GetExpandButtonRotation(!Expanded, Orientation);
            ExpandButton.RotateTo(rotation, (uint)(Math.Abs(start - end) / TransitionSpeed), Easing.SinInOut);

            if (end == 0)
            {
                Expanded = false;
            }

            Bar.AnimateAtSpeed("Transition", dimensionRequest, start, end, 16, TransitionSpeed, Easing.SinInOut, (final, cancelled) =>
            {
                if (end > 0)
                {
                    Expanded = true;
                }
            });

            return;

            //Print.Log(OrientedLength, Width, Height);
            start = 0;
            end = BarSize - ButtonSize - Spacing;
            if (!Expanded)
            {
                System.Extensions.Misc.Swap(ref start, ref end);
            }

            Resize(Scroll, 0);

            Bar.IsVisible = true;
            Animation collapse = new Animation(size => Resize(Bar, size), start, end);
            collapse.Commit(this, "Collapse", 1, 80, Easing.SinInOut, (a, b) =>
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

        //private void InvalidateMeasure(object sender, EventArgs e) => InvalidateMeasure();

        private Size AvailableSpace;

        protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
        {
            //Print.Log("measuring variables", widthConstraint, heightConstraint, LengthBinding, LengthBinding.Bounds.Size);
            //LastAllocatedSize = new Size(widthConstraint, heightConstraint);

            AvailableSpace = new Size(widthConstraint, heightConstraint);

            //Print.Log("measuring variable row", widthConstraint, heightConstraint);
            BarSize = Orientation == StackOrientation.Horizontal ? widthConstraint : heightConstraint;// - ButtonSize - Spacing;
            return base.OnMeasure(widthConstraint, heightConstraint);

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