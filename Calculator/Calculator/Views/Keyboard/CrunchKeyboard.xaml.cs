using System;
using System.Collections;
using System.Collections.Generic;
using System.Extensions;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Calculator
{
    [ContentProperty(nameof(Key))]
    public class BindingResourceExtension : IMarkupExtension<BindingBase>
    {
        public string Key { get; set; }

        public BindingBase ProvideValue(IServiceProvider serviceProvider)
        {
            Binding template = new StaticResourceExtension { Key = Key }.ProvideValue(serviceProvider) as Binding;

            return new Binding(template.Path, template.Mode, template.Converter, template.ConverterParameter, template.StringFormat, template.Source)
            {
                UpdateSourceEventName = template.UpdateSourceEventName,
                FallbackValue = template.FallbackValue,
                TargetNullValue = template.TargetNullValue,
            };

        }

        object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider) => ProvideValue(serviceProvider);
    }

    public class IndexFromEndConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => (int)value - int.Parse(parameter.ToString());

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;
    }

    public class ButtonFeedbackBehavior : Behavior<Button>
    {
        private readonly double Opacity = 0.25;

        protected override void OnAttachedTo(Button bindable)
        {
            base.OnAttachedTo(bindable);

            bindable.Bind<Color>(Button.BorderColorProperty, value => bindable.BackgroundColor = value.WithAlpha(Opacity));
            //bindable.SetBinding<Color, Color>(Button.BackgroundColorProperty, bindable, "BorderColor", value => value.WithAlpha(Opacity));
            bindable.Clicked += Clicked;
        }

        protected override void OnDetachingFrom(Button bindable)
        {
            base.OnDetachingFrom(bindable);
            bindable.Clicked += Clicked;
        }


        private void Clicked(object sender, EventArgs e)
        {
            Button button = (Button)sender;

            button.BackgroundColor = button.BackgroundColor.WithAlpha(1);
            button.Animate("fade", PropertyAnimation.Create(button, VisualElement.BackgroundColorProperty, button.BackgroundColor.WithAlpha(Opacity)), length: 500);
        }
    }

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CrunchKeyboard : Grid, IEnumerable<Key>, ISoftKeyboard, IStateTransitionManager
    {
        //public event KeystrokeEventHandler Typed;
        public event EventHandler OnscreenSizeChanged;

        //public double Spacing { get; set; } = 6;

        public static readonly BindableProperty OrientationProperty = BindableProperty.Create(nameof(Orientation), typeof(StackOrientation), typeof(CrunchKeyboard), StackOrientation.Horizontal, propertyChanged: (bindable, oldvalue, newvalue) => ((CrunchKeyboard)bindable).InvalidateLayout());

        public StackOrientation Orientation
        {
            get => (StackOrientation)GetValue(OrientationProperty);
            protected set => SetValue(OrientationProperty, value);
        }

        public static readonly IndexFromEndConverter IndexFromEnd = new IndexFromEndConverter();
        public static readonly string EXPANDED_MODE = "Expanded";

        public int Columns => Keypad.ColumnDefinitions.Count;
        public int Rows => Keypad.RowDefinitions.Count;

        private readonly int MAX_BUTTON_SIZE = 75;
        private double PERMANENT_KEYS_INCREASE = 1.25;
        private double MIN_COLUMNS = 4;
        private readonly int RecentlyUsed = 10;

        public Size FullSize(double columns, double rows) => new Size((columns - 1 + PERMANENT_KEYS_INCREASE) * MAX_BUTTON_SIZE + (columns - 1) * ColumnSpacing, (rows - 1) * MAX_BUTTON_SIZE + RowDefinitions[0].Height.Value + (rows - 1) * RowSpacing);

        public Size Size { get; private set; }
        //public double ButtonSize { get; set; }

        public CrunchKeyboard()
        {
            void InitialButtonSetup(object sender, ElementEventArgs e)
            {
                if (e.Element is Button button)
                {
                    button.PropertyChanged += (sender1, e1) =>
                    {
                        if (!e1.IsProperty(WidthProperty, HeightProperty, Button.FontSizeProperty))// || button.Text == null)
                        {
                            return;
                        }

                        int length = button.Text?.Length ?? 0;
                        button.FontSize = length > 1 ?
                            Math.Floor(33 * button.Width / 75 * 5 / (4 + length)) :
                            Math.Floor(33 * Math.Max(button.Height, button.Width) / 75);
                    };
                }
            }

            DescendantAdded += InitialButtonSetup;

            RowSpacing = 2;
            ColumnSpacing = 2;

            InitializeComponent();

            // Rotate the arrow text for the up and down keys
            foreach (LabelButton labelButton in ArrowKeys.Children.OfType<LabelButton>())
            {
                labelButton.Label.Rotation = 90;
                labelButton.Label.SetBinding<Rectangle, double>(AbsoluteLayout.LayoutBoundsProperty, labelButton.Button, "Width", value => new Rectangle(0.5, 0.5, value, value));
            }

            foreach (char c in App.Variables)
            {
                Button button = new Button
                {
                    Text = c.ToString(),
                };
                button.Clicked += (sender, e) =>
                {
                    int index = App.Variables.IndexOf(c);

                    if (index > RecentlyUsed - 1)
                    {
                        App.Variables.RemoveAt(index);
                        App.Variables.Insert(0, c);

                        View v = VariableLayout.Children[index];
                        VariableLayout.Children.RemoveAt(index);
                        VariableLayout.Children.Insert(0, v);

                        (VariableLayout.Parent as ScrollView)?.ScrollToAsync(VariableLayout, ScrollToPosition.Start, true);
                    }
                };

                button.SetBinding(WidthRequestProperty, NextKeyboardButton, nameof(Width));
                button.SetBinding(HeightRequestProperty, NextKeyboardButton, nameof(Height));

#if !(ANDROID && DEBUG)
                VariableLayout.Children.Add(button);
#endif
            }

            this.AddContinuousTouchListener<TouchEventArgs>(Gesture.Pinch, (sender, e) =>
            {
                if (e.State == TouchState.Up)
                {
                    App.ShowFullKeyboard.Value = !App.ShowFullKeyboard.Value;
                }
            });

            Scroll.SetBounces(false);

            //Keypad.SetBinding<double, double>(WidthRequestProperty, Keypad, "Height", value => PaddedButtonsWidth(Keys[0].Length, ButtonWidth(value, Keys.Length)));
            void UpdateKeypadWidth() => Keypad.WidthRequest = (Scroll.Width - (GetColumnSpan(Scroll) - 1) * Keypad.ColumnSpacing) / GetColumnSpan(Scroll) * Columns + (Columns - 1) * Keypad.ColumnSpacing;
            Scroll.Bind<double>(WidthProperty, value => UpdateKeypadWidth());
            Scroll.WhenPropertyChanged(ColumnSpanProperty, (sender, e) => UpdateKeypadWidth());

            Keypad.Children[0].Bind<double>(WidthProperty, value => Scroll.SetPagingInterval(value + Keypad.ColumnSpacing));

            LayoutChanged += HandleLayoutChanged;

            this.WhenPropertyChanged(OrientationProperty, (sender, e) =>
            {
                StackOrientation value = Orientation;

                ChangeModeButton.IsEnabled = value == StackOrientation.Horizontal;
            });
            //VariableLayout.SetBinding<StackOrientation, ScrollOrientation>(StackLayout.OrientationProperty, VariableLayout.Parent, "Orientation", value => value == ScrollOrientation.Vertical ? StackOrientation.Vertical : StackOrientation.Horizontal);

            DockButton.Remove();
            ChangeModeButton.Remove();

            DescendantAdded -= InitialButtonSetup;

            OnscreenSizeChanged += (sender, e) => this.SizeRequest(Size.Width - Padding.HorizontalThickness - SafePadding.HorizontalThickness, Size.Height - Padding.VerticalThickness - SafePadding.VerticalThickness);
        }

        public void ChangeModeButtonClicked(object sender, EventArgs e)
        {
            App.BasicMode.Value = !App.BasicMode.Value;
        }

        public void NextKeyboardButtonClicked(object sender, EventArgs e)
        {
            SoftKeyboardManager.NextKeyboard();
            this.Parent<MainPage>().FocusedMathField.Test();
        }

        public void DismissButtonClicked(object sender, EventArgs e)
        {
            SoftKeyboardManager.OnDismissed();
        }

        protected virtual void OnOnscreenSizeChanged(Size size)
        {
            Size = size;
            OnscreenSizeChanged?.Invoke(this, new EventArgs());
        }

        private Size ScreenSize;
        private Thickness SafePadding;

        public void ScreenSizeChanged(Size screenSize, Thickness safePadding)
        {
            ScreenSize = screenSize;
            SafePadding = safePadding;

            OnPropertyChanged(nameof(VariablesSize));
            HandleLayoutChanged(null, null);
        }

        //public double VariablesSize => Math.Max(40, (Math.Min(MAX_BUTTON_SIZE * (4 + 1.25) + (5 - 1) * ColumnSpacing, Width) - (7 + 1) * ColumnSpacing) / (7 + 1.25));

        public double VariablesSize => Math.Max(40, (Math.Min(MAX_BUTTON_SIZE * (4 + 1.25) + (5 - 1) * ColumnSpacing, Orientation == StackOrientation.Vertical && SoftKeyboardManager.Current == this ? ScreenSize.Height - SafePadding.VerticalThickness : ScreenSize.Width - SafePadding.HorizontalThickness) - (7 + 1) * ColumnSpacing) / (7 + 1.25));

        private bool AutoSize = true;

        public bool Collapsed { get; private set; }

        private void HandleLayoutChanged(object sender, EventArgs e)
        {
            ResetScroll();

            if (AutoSize)
            {
                OnOnscreenSizeChanged(Measure());
            }

            bool collapsed = Size.Width == ScreenSize.Width || Size.Height == ScreenSize.Height;

            Orientation = collapsed && ScreenSize.Height < ScreenSize.Width ? StackOrientation.Vertical : StackOrientation.Horizontal;
            BottomRight.Content = collapsed ? ChangeModeButton : DockButton;

            if (collapsed != Collapsed)
            {
                Collapsed = collapsed;
                OnPropertyChanged(nameof(Collapsed));
            }
        }

        private Size Measure(string state = null)
        {
            Grid grid;

            if (state == null)
            {
                state = this.CurrentState()?.Name;
                grid = this;
            }
            else
            {
                grid = new Grid
                {
                    ColumnDefinitions = (ColumnDefinitionCollection)this.GetValue(ColumnDefinitionsProperty, state, this),
                    ColumnSpacing = ColumnSpacing,
                    RowDefinitions = (RowDefinitionCollection)this.GetValue(RowDefinitionsProperty, state, this),
                    RowSpacing = RowSpacing,
                    Padding = Padding,
                    Margin = Margin
                };
            }

            Size constraint = ScreenSize;
            GridExtensions.AutoSize autoSizeDirection = constraint.Height > constraint.Width ? GridExtensions.AutoSize.Height : GridExtensions.AutoSize.Width;

            if (state != MainPage.BASIC_MODE)
            {
                if (constraint.Height > constraint.Width)
                {
                    constraint.Height /= 2;
                }
                else
                {
                    constraint.Width /= 2;
                }
            }

            constraint -= new Size(SafePadding.HorizontalThickness, SafePadding.VerticalThickness);

            Size size = grid.Measure(constraint.Width, constraint.Height, autoSizeDirection).Request;
            Size fullSize = FullSize(Columns + 1, Rows + 1);

            if (fullSize.Area() < size.Area() && fullSize.Width < App.Current.MainPage.Width && fullSize.Height < App.Current.MainPage.Height)
            {
                size = FullSize(ColumnDefinitions.Count, RowDefinitions.Count);
            }

            return size + new Size(SafePadding.HorizontalThickness, SafePadding.VerticalThickness);
        }

        void IStateTransitionManager.StateWillChange(string oldState, string newState, Animation animation)
        {
            AutoSize = false;

            if (oldState == MainPage.BASIC_MODE)
            {
                foreach (View view in new View[] { BackspaceButton, ArrowKeys, BottomRight })
                {
                    view.GoToState(newState == EXPANDED_MODE ? MainPage.PORTRAIT_MODE : newState, this);
                    Children.Add(view);
                }
            }

            if (newState == MainPage.BASIC_MODE)
            {
                View[] keys = Keypad.Children.Where(view => GetColumn(view) == Keypad.ColumnDefinitions.Count - 1).OrderBy(view => GetRow(view)).ToArray();
                for (int i = 0; i < Rows; i++)
                {
                    Right.Children.Add(keys[i], 0, 1, i * 2 + 1, i * 2 + 3);
                }
            }

            if (animation == null)
            {
                return;
            }

            bool portraitToBasic = (oldState == MainPage.PORTRAIT_MODE || oldState == EXPANDED_MODE) && newState == MainPage.BASIC_MODE;
            bool basicToPortrait = oldState == MainPage.BASIC_MODE && (newState == MainPage.PORTRAIT_MODE || newState == EXPANDED_MODE);

            if (portraitToBasic || basicToPortrait)
            {
                AnimationFactory animationFactory = (callback, start, end, easing, finished) =>
                {
                    if (basicToPortrait)
                    {
                        System.Misc.Swap(ref start, ref end);
                    }

                    return new Animation(callback, start, end, easing, finished);
                };
                double finalHeight = Math.Max(0, Measure(newState).Height);

                animation.WithConcurrent(new Animation(value => OnOnscreenSizeChanged(new Size(Size.Width, value)), Size.Height, finalHeight));
                foreach (Animation temp in CombineLastTwoColumns(animationFactory).OfType<Animation>())
                {
                    animation.WithConcurrent(temp);
                    temp.GetCallback()(0);
                }
                foreach (Animation temp in MorphGridStructure(basicToPortrait, finalHeight, animationFactory))
                {
                    animation.WithConcurrent(temp);
                }
            }
        }

        void IStateTransitionManager.StateDidChange(string oldState, string newState)
        {
            // Leaving basic mode
            if (oldState == MainPage.BASIC_MODE)
            {
                for (int i = 0; i < Rows; i++)
                {
                    Keypad.Children.Add(Right.Children[0], Columns - 1, i);
                }
            }

            // Entering basic mode
            if (newState == MainPage.BASIC_MODE)
            {
                Right.Children.Add(BottomRight, 0, 9);
                Right.Children.Add(BackspaceButton, 0, 0);
                ArrowKeys.Remove();
            }

            (VariableLayout.Parent as ScrollView).Orientation = newState == MainPage.LANDSCAPE_MODE ? ScrollOrientation.Vertical : ScrollOrientation.Horizontal;
            VariableLayout.Orientation = newState == MainPage.LANDSCAPE_MODE ? StackOrientation.Vertical : StackOrientation.Horizontal;

            Scroll.SetIsScrollEnabled(newState != MainPage.BASIC_MODE);

            AutoSize = true;
            HandleLayoutChanged(null, null);
        }

        private IEnumerable<Animation> MorphGridStructure(bool regular, double finalHeight, AnimationFactory animationFactory = null)
        {
            ColumnDefinitionCollection colsCopy = new ColumnDefinitionCollection();
            RowDefinitionCollection rowsCopy = new RowDefinitionCollection();
            for (int i = 0; i < 5; i++)
            {
                colsCopy.Add(new ColumnDefinition { Width = i >= ColumnDefinitions.Count ? new GridLength(0, GridUnitType.Star) : ColumnDefinitions[i].Width });
                rowsCopy.Add(new RowDefinition { Height = RowDefinitions[i].Height });
            }
            ColumnDefinitions = colsCopy;
            RowDefinitions = rowsCopy;

            yield return PropertyAnimation.Create(colsCopy.Last(), ColumnDefinition.WidthProperty, new GridLength(regular ? 1.25 : 0, GridUnitType.Star));

            //double finalVariables = regular ? VariablesSize / (finalHeight - Padding.VerticalThickness - (RegularGridDefinition.RowDefinitions.Count - 1) * RowSpacing) : (Width - Padding.HorizontalThickness - (BasicGridDefinition.ColumnDefintions.Count - 1) * ColumnSpacing) / BasicGridDefinition.ColumnDefintions.Count;
            double absolute;
            double proportional;
            ((RowDefinitionCollection)this.GetValue(RowDefinitionsProperty, MainPage.BASIC_MODE)).DeconstructRows(RowSpacing, out absolute, out proportional);
            double basicVariablesHeight = Math.Max(0, ((regular ? Height : (finalHeight - SafePadding.VerticalThickness)) - absolute) / proportional);

            yield return PropertyAnimation.Create(rowsCopy[0], RowDefinition.HeightProperty, basicVariablesHeight, VariablesSize, factory: animationFactory);
        }

        private Animation CombineLastTwoColumns(AnimationFactory animationFactory = null)
        {
            Animation result = new Animation();

            result.WithConcurrent(PropertyAnimation.Create(Right, MarginProperty, new Thickness(0), new Thickness(0, VariablesSize, 0, -RowSpacing), factory: animationFactory));

            for (int i = 0; i < 2; i++)
            {
                RowDefinition row = Right.RowDefinitions[i * (Right.RowDefinitions.Count - 1)];
                result.WithConcurrent(PropertyAnimation.Create(row, RowDefinition.HeightProperty, new GridLength(1, GridUnitType.Star), new GridLength(0, GridUnitType.Star), factory: animationFactory));
            }

            Thickness unit = new Thickness((Width - Padding.HorizontalThickness - ColumnSpacing * 4) / 4 + RowSpacing);
            Thickness Multiply(Thickness value, Thickness factor) => new Thickness(value.Left * factor.Left, value.Top * factor.Top, value.Right * factor.Right, value.Bottom * factor.Bottom);

            result.WithConcurrent(PropertyAnimation.Create(BackspaceButton, MarginProperty, Multiply(unit, new Thickness(-1, -1, 0, 1.5)), new Thickness(0), finished: () => BackspaceButton.ClearValue(MarginProperty), factory: animationFactory));
            result.WithConcurrent(PropertyAnimation.Create(BottomRight, MarginProperty, Multiply(unit, new Thickness(-1, 0.5, 0, 0)), new Thickness(0), finished: () => BottomRight.ClearValue(MarginProperty), factory: animationFactory));
            result.WithConcurrent(new Animation(value => ArrowKeys.Margin = new Thickness(0, -BackspaceButton.Margin.Bottom, 0, -BottomRight.Margin.Top), finished: () => ArrowKeys.ClearValue(MarginProperty)));

            return result;
        }

        private void ResetScroll() => Scroll.ScrollToAsync(Keypad, ScrollToPosition.End, false);

        public void Enable(bool animated = false) => IsVisible = true;// SetEnabled(true, animated);

        public void Disable(bool animated = false) => IsVisible = false;// SetEnabled(false, animated);

        private void SetEnabled(bool value, bool animated = false)
        {
            if (animated)
            {
                this.Animate("Enable", PropertyAnimation.Create(this, VisualElementAdditions.VisibilityProperty, value.ToInt()), length: 500);
            }
            else
            {
                this.SetVisibility(value.ToInt());
            }
        }

        public IEnumerator<Key> GetEnumerator()
        {
            foreach (Key key in this.GetDescendants<Key>())
            {
                yield return key;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}