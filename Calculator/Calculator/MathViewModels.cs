using System;
using System.Collections.ObjectModel;
using System.Extensions;
using Xamarin.Forms;
using Xamarin.Forms.MathDisplay;

namespace Calculator
{
    public class MathField : MathExpression
    {
        public static readonly BindableProperty CursorPositionProperty = BindableProperty.Create(nameof(CursorPosition), typeof(int), typeof(MathFieldViewModel), 0, propertyChanged: CursorPositionChanged);//, coerceValue: (bindable, value) => ((int)value).Bound(0, ((MathField)bindable).Children.Count));

        public event EventHandler<ChangedEventArgs<int>> CursorMoved;

        public int CursorPosition
        {
            get => (int)GetValue(CursorPositionProperty);
            set => SetValue(CursorPositionProperty, value);
        }

        public MathField()
        {
            Children.CollectionChanged += (sender, e) =>
            {
                CursorPosition += -(e.OldItems?.Count ?? 0) + (e.NewItems?.Count ?? 0);
            };
        }

        public void Input(string input, int? index = null)
        {
            index = index ?? CursorPosition;

            for (int i = input.Length - 1; i >= 0; i--)
            {
                Children.Insert(index.Value, input[i]);
            }
        }

        public void Delete(int? index = null)
        {
            index = index ?? CursorPosition;

            if (index.Value.IsBetween(1, Children.Count))
            {
                Children.RemoveAt(index.Value - 1);
            }
        }

        protected void OnCursorMoved(ChangedEventArgs<int> e)
        {
            CursorMoved?.Invoke(this, e);
        }

        private static void CursorPositionChanged(BindableObject bindable, object oldValue, object newValue)
        {
            MathField mathField = (MathField)bindable;
            mathField.OnCursorMoved(new ChangedEventArgs<int>((int)oldValue, (int)newValue));
        }
    }

    public class MathExpression : BindableObject
    {
        public ObservableCollection<char> Children { get; private set; } = new ObservableCollection<char>();
    }

    public class MathFieldLayout : AbsoluteLayout
    {
        public Entry Entry { get; private set; }
        public ExpressionLayout Expression { get; set; }

        public MathFieldLayout() : base()
        {
            BindingContext = new MathFieldViewModel();
            
            Expression = new ExpressionLayout();
            Expression.SetBinding(BindableLayout.ItemsSourceProperty, "Children");
            Entry = new Entry();

            Children.Add(Entry, new Rectangle(0, 0, 1, 1), AbsoluteLayoutFlags.All);
            Children.Add(Expression, new Rectangle(0.5, 0.5, -1, -1), AbsoluteLayoutFlags.PositionProportional);
            //Expression.MathField.CursorMoved += CursorMoved;
        }

        private void CursorMoved(object sender, ChangedEventArgs<int> e)
        {
            Action move = e.NewValue > e.OldValue ? SoftKeyboard.Right : (Action)SoftKeyboard.Left;

            for (int i = 0; i < Math.Abs(e.NewValue - e.OldValue); i++)
            {
                move();
            }
        }
    }

    public class ExpressionLayout : StackLayout
    {
        public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(ExpressionLayout));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public ExpressionLayout()
        {
            Orientation = StackOrientation.Horizontal;

            this.SetBinding(BindableLayout.ItemsSourceProperty, this, "Text");

            BindableLayout.SetItemTemplateSelector(this, new MathElementTemplateSelector
            {
                TextTemplate = new DataTemplate(() =>
                {
                    Label label = new Label();
                    label.SetBinding(Label.TextProperty, ".");
                    return label;
                }),
                FractionTemplate = new DataTemplate(() =>
                {
                    return new StackLayout
                    {
                        Orientation = StackOrientation.Vertical
                    };
                })
            });
        }
    }

    public class MathElementTemplateSelector : DataTemplateSelector
    {
        public DataTemplate TextTemplate { get; set; }
        public DataTemplate FractionTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (item is Fraction)
            {
                return FractionTemplate;
            }
            else
            {
                return TextTemplate;
            }
        }
    }

#if false
    public class MathStackLayout : StackLayout
    {
        public static readonly BindableProperty MembersProperty = BindableProperty.Create(nameof(Members), typeof(ObservableCollection<char>), typeof(MathStackLayout), propertyChanged: MembersPropertyChanged);

        public IList<char> Members
        {
            get => (ObservableCollection<char>)GetValue(MembersProperty);
            set => SetValue(MembersProperty, value);
        }

        public View ViewFromModel(object model)
        {
            return new Label { Text = model.ToString() };
        }

        private static void MembersPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            MathStackLayout layout = (MathStackLayout)bindable;

            layout.Children.Clear();

            foreach (char c in (IList<char>)newValue)
            {
                layout.Children.Add(layout.ViewFromModel(c));
            }
        }
        /*private void ChildrenChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                string input = string.Empty;

                foreach (var item in e.NewItems)
                {
                    if (item is char c)
                    {
                        input += c;
                    }
                }
                SoftKeyboard.Type(input);
            }

            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    SoftKeyboard.Delete();
                }
            }
        }*/
    }
#endif
}