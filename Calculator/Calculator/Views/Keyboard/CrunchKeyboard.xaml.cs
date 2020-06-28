using System;
using System.Collections;
using System.Collections.Generic;
using System.Extensions;
using System.Globalization;
using System.Linq;
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

    public abstract class RelativeGridPositionExtension : IMarkupExtension<BindingBase>
    {
        public int Offset { get; set; }

        public object Source { get; set; }

        public BindingBase ProvideValue(IServiceProvider serviceProvider)
        {
            var valueProvider = serviceProvider?.GetService<IProvideValueTarget>() ?? throw new ArgumentException();

            BindableProperty property = (valueProvider.TargetObject as Setter)?.Property ?? valueProvider.TargetProperty as BindableProperty;

            return new Binding
            {
                Source = Source,
                Converter = CrunchKeyboard.IndexFromEnd,
                ConverterParameter = Offset
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
        public event KeystrokeEventHandler Typed;
        public event EventHandler OnscreenSizeChanged;

        public double Spacing { get; set; } = 6;

        public static readonly BindableProperty OrientationProperty = BindableProperty.Create(nameof(Orientation), typeof(StackOrientation), typeof(CrunchKeyboard), StackOrientation.Horizontal, propertyChanged: (bindable, oldvalue, newvalue) => ((CrunchKeyboard)bindable).InvalidateLayout());

        //public static readonly BindableProperty IsCondensedProperty = BindableProperty.Create(nameof(IsCondensed), typeof(bool), typeof(CrunchKeyboard), false, propertyChanged: (bindable, oldvalue, newvalue) => ((CrunchKeyboard)bindable).InvalidateLayout());

        public StackOrientation Orientation
        {
            get => (StackOrientation)GetValue(OrientationProperty);
            protected set => SetValue(OrientationProperty, value);
        }

        /*public bool IsCondensed
        {
            get => (bool)GetValue(IsCondensedProperty);
            set => SetValue(IsCondensedProperty, value);
        }*/

        public static readonly IndexFromEndConverter IndexFromEnd = new IndexFromEndConverter();
        public static readonly string EXPANDED_MODE = "Expanded";

        private readonly Key EXP = new Key { Text = "x\u207F", Basic = "^" };
        private readonly Key SQRD = new Key { Text = "x\u00B2", Basic = "^2" };
        private readonly Key DIV = new Key { Text = "\u00F7", Basic = "/" };
        private readonly Key LOG = new Key { Text = "log", Basic = "log_" };
        private readonly Key SQRT = new Key { Text = "\u221A", Basic = "√" };// { FontFamily = CrunchStyle.SYMBOLA_FONT };
        private readonly Key MULT = new Key { Text = "\u00D7", Basic = "*" };
        private readonly Key TEN = new Key { Text = "10\u207F", Basic = "10^" };
        private readonly Key PI = new Key { Text = "\u03C0" };

        private readonly View[][] Keys;

        //private RowDefinition VariablesRow;// = new RowDefinition { Height = new GridLength(0.5, GridUnitType.Star) };
        //private ColumnDefinition PermanentKeysColumn;// = new ColumnDefinition { Width = new GridLength(1.25, GridUnitType.Star) };

        private readonly int MAX_BUTTON_SIZE = 75;
        private double PERMANENT_KEYS_INCREASE = 1.25;
        private double MIN_COLUMNS = 4;
        private readonly int RecentlyUsed = 10;

        //public int Rows => Keys.Length + this.Orient(0, 1);
        //public double MaxColumns => Keys[0].Length + this.Orient(PERMANENT_KEYS_INCREASE, 0);
        //public double Columns => this.Orient((IsCondensed ? MIN_COLUMNS : Keys[0].Length) + PERMANENT_KEYS_INCREASE, MIN_COLUMNS);
        //public bool ShowingFullKeyboard => !App.IsCondensed && Settings.ShouldShowFullKeyboard;

        public Size FullSize(double columns, double rows) => new Size((columns - 1 + PERMANENT_KEYS_INCREASE) * MAX_BUTTON_SIZE + (columns - 1) * ColumnSpacing, (rows - 1) * MAX_BUTTON_SIZE + RowDefinitions[0].Height.Value + (rows - 1) * RowSpacing);
        /*new Size(
        PaddedButtonsWidth(Keys[0].Length + PERMANENT_KEYS_INCREASE, MAX_BUTTON_SIZE),
        PaddedButtonsWidth(Keys.Length, MAX_BUTTON_SIZE)
        );*/

        public Size Size { get; private set; }
        public double ButtonSize { get; set; }

        public CrunchKeyboard()
        {
            //Resources = new ResourceDictionary();
            // Make spacing consistent between all buttons
            /*this.WhenDescendantAdded<Grid>((grid) =>
            {
                grid.BindingContext = this;
                grid.SetBinding(Grid.RowSpacingProperty, "RowSpacing");
                grid.SetBinding(Grid.ColumnSpacingProperty, "ColumnSpacing");
            });*/

            void InitialButtonSetup(object sender, ElementEventArgs e)
            {
                //this.WhenDescendantAdded<Button>((button) =>
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
                if (e.Element is CursorKey cursorKey)
                {
                    cursorKey.Clicked += (sender1, e1) => KeyboardManager.MoveCursor(cursorKey.Key);
                }
                //this.WhenExactDescendantAdded<Key>((key) =>
                else if (e.Element is Key key)
                {
                    key.Clicked += (sender1, e1) => Typed?.Invoke(key.Output);
                }
                //this.WhenExactDescendantAdded<CursorKey>((cursorKey) =>
            }
            DescendantAdded += InitialButtonSetup;
            RowSpacing = 2;
            ColumnSpacing = 2;
            InitializeComponent();
            
            Grid parentheses = new Grid
            {
                Children =
                {
                    { new Key { Text = "(" }, 0, 0 },
                    { new Key { Text = ")" }, 1, 0 }
                }
            };
            foreach (View view in parentheses.Children)
            {
                view.SetDynamicResource(Button.BorderColorProperty, "PrimaryKeyboardKeyColor");
            }
            
            //double InverseOpacity(double value) => 1 - value;
            //parentheses.SetBinding<double, double>(OpacityProperty, EqualsKey, "Opacity", InverseOpacity, InverseOpacity, BindingMode.TwoWay);

            //equals.SetBinding<bool, double>(IsVisibleProperty, equals, "Opacity", value => value > 0);
            Keys = new View[][]
            {
                //new Key[] { null,  null,    null,   null,   null,   null,   null },
                new Key[] { "sin",  TEN,    EXP,    "7",    "8",    "9",    DIV },
                new Key[] { "cos",  LOG,    SQRD,   "4",    "5",    "6",    MULT },
                new Key[] { "tan",  "ln",   SQRT,    "1",    "2",    "3",    "-" },
                new List<View>(new Key[] { PI,     "e",    "x",    "0",    ".",    "()",   "+" }).ToArray()
            };
            Keys[3][5] = parentheses;

            // Rotate the arrow text for the up and down keys
            foreach (View view in new View[] { ArrowKeys.Children[0], ArrowKeys.Children[3] })
            {
                if (!(view is LabelButton labelButton))
                {
                    continue;
                }

                labelButton.Label.Rotation = 90;
                labelButton.Label.SetBinding<Rectangle, double>(AbsoluteLayout.LayoutBoundsProperty, labelButton.Button, "Width", value => new Rectangle(0.5, 0.5, value, value));
                //AbsoluteLayout.SetLayoutBounds(labelButton.Label, new Rectangle(0.5, 0.5, 100, 100));
            }

            /*Print.Log(this.CurrentState()?.Name);
            this.GoToState(MainPage.PORTRAIT_MODE);
            Print.Log(this.CurrentState()?.Name);
            this.GoToState(MainPage.BASIC_MODE);
            Print.Log(this.CurrentState()?.Name);
            this.GoToState(MainPage.PORTRAIT_MODE);
            Print.Log(this.CurrentState()?.Name);*/

            BackspaceButton.Basic = KeyboardManager.BACKSPACE.ToString();
            ExpandButton.Clicked += (sender, e) => SoftKeyboardManager.OnDismissed();
            NextKeyboardButton.Clicked += (sender, e) => SoftKeyboardManager.NextKeyboard();

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

                    KeyboardManager.Type(c.ToString());
                };
                button.SetBinding(WidthRequestProperty, ExpandButton, "Width");
                button.SetBinding(HeightRequestProperty, ExpandButton, "Height");

#if !(ANDROID && DEBUG)
                VariableLayout.Children.Add(button);
#endif
            }
            //Variables.SetBinding<bool, StackOrientation>(GridExtensions.IsTransposedProperty, VariableLayout, "Orientation", convertBack: value => value ? StackOrientation.Vertical : StackOrientation.Horizontal, mode: BindingMode.OneWayToSource);
            //VariableLayout.SetBinding(StackLayout.OrientationProperty, Variables, "Orientation");

            for (int i = 0; i < Keys.Length; i++)
            {
                Keypad.RowDefinitions.Add(new RowDefinition());
            }
            for (int i = 0; i < Keys[0].Length; i++)
            {
                Keypad.ColumnDefinitions.Add(new ColumnDefinition());
            }

            for (int i = 0; i < Keys.Length; i++)
            {
                for (int j = 0; j < Keys[i].Length; j++)
                {
                    View view = Keys[i][j];

                    if (view == null)
                    {
                        continue;
                    }

                    /*if (Keys[i][j].Text == "()")
                    {
                        
                    }
                    else
                    {
                        if (Keys[i].Length - j > MIN_COLUMNS)
                        {
                            //Keys[i][j].Clicked += (sender, e) => Scroll.ScrollToAsync(Keypad, ScrollToPosition.End, false);
                        }
                    }*/

                    if (view is Button button)
                    {
                        if (j == Keys[i].Length - 1)
                        {
                            button.BorderColor = Color.DarkOrange;
                        }
                        else
                        {
                            button.SetDynamicResource(Button.BorderColorProperty, "PrimaryKeyboardKeyColor");
                        }
                        //button.BorderColor = j == Keys[i].Length - 1 ? Color.DarkOrange : Color.Black;// Color.FromHex("#C4C3D0");
                    }

                    Keypad.Children.Add(view, j, i);
                }
            }

            this.AddVisualStateValues(parentheses, VisualElementExtensions.VisibilityProperty, new States.CrunchKeyboard { Default = 1, Basic = 0 });

            this.AddContinuousTouchListener<TouchEventArgs>(Gesture.Pinch, (sender, e) =>
            {
                if (e.State == TouchState.Up)
                {
                    App.ShowFullKeyboard.Value = !App.ShowFullKeyboard.Value;
                }
            });

            Scroll.SetBounces(false);

            //Keypad.SetBinding<double, double>(WidthRequestProperty, Keypad, "Height", value => PaddedButtonsWidth(Keys[0].Length, ButtonWidth(value, Keys.Length)));
            void UpdateKeypadWidth() => Keypad.WidthRequest = (Scroll.Width - (GetColumnSpan(Scroll) - 1) * Keypad.ColumnSpacing) / GetColumnSpan(Scroll) * Keys[0].Length + (Keys[0].Length - 1) * Keypad.ColumnSpacing;
            Scroll.Bind<double>(WidthProperty, value => UpdateKeypadWidth());
            Scroll.WhenPropertyChanged(ColumnSpanProperty, (sender, e) => UpdateKeypadWidth());

            /*Keypad.WhenPropertyChanged(WidthProperty, (sender, e) =>
            {
                ResetScroll();
            });*/
            Keypad.Children[0].Bind<double>(WidthProperty, value => Scroll.SetPagingInterval(value + Keypad.ColumnSpacing));

            LayoutChanged += HandleLayoutChanged;

            //App.Current.MainPage.SizeChanged += (sender, e) => ScreenSizeChanged();
            //App.ShowFullKeyboard.Bind<bool>(App.ShowFullKeyboard.ValueProperty, value => IsCondensed = value);

            ChangeModeButton.Clicked += (sender, e) =>
            {
                App.BasicMode.Value = !App.BasicMode.Value;
                OnPropertyChanged(nameof(App.BasicMode));
            };

            //VisualStateManagerExtensions.SetStateWillChangeListener(this, StateWillChangeListener);
            //VisualStateManagerExtensions.SetStateDidChangeListener(this, StateDidChangeListener);

            this.WhenPropertyChanged(OrientationProperty, (sender, e) =>
            {
                StackOrientation value = Orientation;

                VariableLayout.Orientation = value;
                (VariableLayout.Parent as ScrollView).Orientation = value == StackOrientation.Vertical ? ScrollOrientation.Vertical : ScrollOrientation.Horizontal;

                ChangeModeButton.IsEnabled = value == StackOrientation.Horizontal;

                //OnVariablesSizeChanged();
            });
            
            DockButton.Remove();
            ChangeModeButton.Remove();

            DescendantAdded -= InitialButtonSetup;

            /*PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == ColumnDefinitionsProperty.PropertyName || e.PropertyName == RowDefinitionsProperty.PropertyName)
                {
                    //Print.Log("columns changed", ColumnDefinitions.Count);
                    InvalidateLayout();
                }
            };*/

            OnscreenSizeChanged += (sender, e) => this.SizeRequest(Size.Width - Padding.HorizontalThickness - SafePadding.HorizontalThickness, Size.Height - Padding.VerticalThickness - SafePadding.VerticalThickness);
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

            Orientation = ScreenSize.Height > ScreenSize.Width ? StackOrientation.Horizontal : StackOrientation.Vertical;
            OnPropertyChanged(nameof(VariablesSize));
            HandleLayoutChanged(null, null);
        }

        //public double VariablesSize => Math.Max(40, (Math.Min(MAX_BUTTON_SIZE * (4 + 1.25) + (5 - 1) * ColumnSpacing, Width) - (7 + 1) * ColumnSpacing) / (7 + 1.25));

        public double VariablesSize => Math.Max(40, (Math.Min(MAX_BUTTON_SIZE * (4 + 1.25) + (5 - 1) * ColumnSpacing, Orientation == StackOrientation.Vertical && SoftKeyboardManager.Current == this ? ScreenSize.Height - SafePadding.VerticalThickness : ScreenSize.Width - SafePadding.HorizontalThickness) - (7 + 1) * ColumnSpacing) / (7 + 1.25));

        private bool AutoSize = true;

        private void HandleLayoutChanged(object sender, EventArgs e)
        {
            LayoutChanged -= HandleLayoutChanged;

            ResetScroll();

            if (AutoSize)
            {
                OnOnscreenSizeChanged(Measure());
            }

            bool collapsed = Size.Width == ScreenSize.Width || Size.Height == ScreenSize.Height;
            BottomRight.Content = collapsed ? ChangeModeButton : DockButton;

            LayoutChanged += HandleLayoutChanged;
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

            Size constraint = ScreenSize - new Size(SafePadding.HorizontalThickness, SafePadding.VerticalThickness);
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

            Size size = grid.Measure(constraint.Width, constraint.Height, autoSizeDirection).Request;
            size += new Size(SafePadding.HorizontalThickness, SafePadding.VerticalThickness);
            
            Size fullSize = FullSize(Keys[0].Length + 1, Keys.Length + 1);
            return fullSize.Area() < size.Area() && fullSize.Width < App.Current.MainPage.Width && fullSize.Height < App.Current.MainPage.Height ? FullSize(ColumnDefinitions.Count, RowDefinitions.Count) : size;
        }

        void IStateTransitionManager.StateWillChange(string oldState, string newState, Animation animation)
        {
            if (oldState == MainPage.BASIC_MODE)
            {
                foreach (View view in new View[] { BackspaceButton, ArrowKeys, BottomRight })
                {
                    Children.Add(view);
                    view.GoToState(newState == EXPANDED_MODE ? MainPage.PORTRAIT_MODE : newState, this);
                }
            }

            if (newState == MainPage.BASIC_MODE)
            {
                Right.IsVisible = true;
                for (int i = 0; i < Keys.Length; i++)
                {
                    Right.Children.Add(Keys[i].Last(), 0, 1, i * 2 + 1, i * 2 + 3);
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
                double finalHeight = Measure(newState).Height;

                AutoSize = false;
                animation.WithConcurrent(new Animation(value =>
                {
                    AbsoluteLayoutExtensions.SetLayout(Parent, Size - new Size(SafePadding.HorizontalThickness, 0));
                    OnOnscreenSizeChanged(new Size(Size.Width, value));
                }, Size.Height, finalHeight, finished: () =>
                {
                    AbsoluteLayoutExtensions.SetLayout(Parent, new Size(-1, -1));
                    AutoSize = true;
                }));
                animation.WithConcurrent(CombineLastTwoColumns(animationFactory));
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
                for (int i = 0; i < Keys.Length; i++)
                {
                    Keypad.Children.Add(Keys[i].Last(), Keys[i].Length - 1, i);
                }
                Right.IsVisible = false;
            }

            // Entering basic mode
            if (newState == MainPage.BASIC_MODE)
            {
                Right.Children.Add(BottomRight, 0, 9);
                Right.Children.Add(BackspaceButton, 0, 0);
                ArrowKeys.Remove();
            }
        }

        private IEnumerable<Animation> MorphGridStructure(bool regular, double finalHeight, AnimationFactory animationFactory = null)
        {
            GridDefinition intermediate = new GridDefinition();
            for (int i = 0; i < 5; i++)
            {
                intermediate.ColumnDefinitions.Add(new ColumnDefinition { Width = i >= ColumnDefinitions.Count ? new GridLength(0, GridUnitType.Star) : ColumnDefinitions[i].Width });
                intermediate.RowDefinitions.Add(new RowDefinition { Height = RowDefinitions[i].Height });
            }
            ColumnDefinitions = intermediate.ColumnDefinitions;
            RowDefinitions = intermediate.RowDefinitions;

            yield return PropertyAnimation.Create(intermediate.ColumnDefinitions.Last(), ColumnDefinition.WidthProperty, new GridLength(regular ? 1.25 : 0, GridUnitType.Star));

            //double finalVariables = regular ? VariablesSize / (finalHeight - Padding.VerticalThickness - (RegularGridDefinition.RowDefinitions.Count - 1) * RowSpacing) : (Width - Padding.HorizontalThickness - (BasicGridDefinition.ColumnDefintions.Count - 1) * ColumnSpacing) / BasicGridDefinition.ColumnDefintions.Count;
            double absolute;
            double proportional;
            ((RowDefinitionCollection)this.GetValue(RowDefinitionsProperty, MainPage.BASIC_MODE)).DeconstructRows(RowSpacing, out absolute, out proportional);
            double basicVariablesHeight = ((regular ? Height : (finalHeight - SafePadding.VerticalThickness)) - absolute) / proportional;

            yield return PropertyAnimation.Create(intermediate.RowDefinitions[0], RowDefinition.HeightProperty, basicVariablesHeight, VariablesSize, factory: animationFactory);
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

        public bool Collapsed { get; private set; }

        private void ResetScroll() => Scroll.ScrollToAsync(Keypad, ScrollToPosition.End, false);

        public void Enable(bool animated = false) => IsVisible = true;// SetEnabled(true, animated);

        public void Disable(bool animated = false) => IsVisible = false;// SetEnabled(false, animated);

        private void SetEnabled(bool value, bool animated = false)
        {
            if (animated)
            {
                this.Animate("Enable", PropertyAnimation.Create(this, Xamarin.Forms.VisualElementExtensions.VisibilityProperty, value.ToInt()), length: 500);
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