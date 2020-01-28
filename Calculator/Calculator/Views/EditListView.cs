using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using System.Extensions;
using Xamarin.Forms;
using Xamarin.Forms.Extensions;
using System.Collections.ObjectModel;

namespace Calculator
{
    public delegate void ContextActionEventHandler<T>(object sender, EventArgs<T> e) where T : Enum;

    public class EditCell : EditListView.EditCell { }

    public class EditListView : ListView
    {
        public enum ContextActions { Delete, Edit };

        public event ContextActionEventHandler<ContextActions> ContextAction;

        public static BindableProperty EditingProperty = BindableProperty.Create("Editing", typeof(bool), typeof(EditListView), false);

        public bool Editing
        {
            get => (bool)GetValue(EditingProperty);
            set => SetValue(EditingProperty, value);
        }
        
        new public View Header
        {
            get => HeaderView.Content;
            set => HeaderView.Content = value;
        }

        public readonly Button Add;
        public readonly Button Edit;

        private readonly ContentView HeaderView;
        private readonly Layout<View> EditingToolbar;

        new public Items ItemsSource
        {
            get => _ItemsSource;
            set => base.ItemsSource = _ItemsSource = value;
        }
        private Items _ItemsSource;

        /*public class Test<T> : ICollection<T>, System.Collections.Specialized.INotifyCollectionChanged, System.ComponentModel.INotifyPropertyChanged
        {
            public event System.Collections.Specialized.NotifyCollectionChangedEventHandler CollectionChanged;
            public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

            private ObservableCollection<ViewModel<T>> Collection;

            public int Count => Collection.Count;

            public bool IsReadOnly => false;

            public Test()
            {
                Collection = new ObservableCollection<ViewModel<T>>();
                Collection.CollectionChanged += (sender, e) =>
                {
                    CollectionChanged?.Invoke(this, e);
                };
            }

            public void Add(T item) => Collection.Add(item);

            public void Clear() => Collection.Clear();

            public bool Contains(T item) => Collection.Contains(item);

            public void CopyTo(T[] array, int arrayIndex)
            {
                ViewModel<T>[] temp = new ViewModel<T>[4];
            }

            public bool Remove(T item) => Collection.Remove(item);

            public IEnumerator<T> GetEnumerator()
            {
                foreach(ViewModel<T> vm in Collection)
                {
                    yield return vm;
                }
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }*/

        public class ViewModel
        {
            public object Value { get; set; }

            public bool IsSelected { get; set; }

            public ViewModel(object value)
            {
                Value = value;
            }
        }

        public class Items : ObservableCollection<ViewModel>
        {
            public void Add(object item) => base.Add(new ViewModel(item));
            public void Insert(int index, object item) => base.Insert(index, new ViewModel(item));
        }

        public EditListView()
        {
            Button delete;
            
            base.Header = new StackLayout
            {
                Orientation = StackOrientation.Vertical,
                Spacing = 0,
                Children =
                {
                    (HeaderView = new ContentView()),
                    new ToolbarView
                    {
                        Padding = new Thickness(10),
                        Children =
                        {
                            (Edit = new Button
                            {
                                BackgroundColor = Color.Transparent,
                                TextColor = Color.Default,
                                FontSize = NamedSize.Body.On<Button>()
                            }),
                            (Add = new Button
                            {
                                HorizontalOptions = LayoutOptions.EndAndExpand,
                                BackgroundColor = Color.Transparent,
                                Text = "+",
                                TextColor = Color.Default,
                                FontSize = NamedSize.Title.On<Button>(),
                            })
                        }
                    }
                }
            };
            EditingToolbar = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                BackgroundColor = Color.LightGray,
                IsVisible = false,
                Children =
                {
                    (delete = new Button
                    {
                        HorizontalOptions = LayoutOptions.EndAndExpand,
                        Text = "Delete"
                    })
                }
            };
            
            EditingToolbar.WhenPropertyChanged(IsVisibleProperty, (sender, e) =>
            {
                Print.Log("edit toolbar visibility changed", EditingToolbar.Height);
                return;
                Footer = !EditingToolbar.IsVisible ? null : new StackLayout
                {
                    HeightRequest = EditingToolbar.Height,
                };
                
                return;

                Thickness margin = Margin;
                margin.Bottom += EditingToolbar.Height * (EditingToolbar.IsVisible.ToInt() * 2 - 1);
                Margin = margin;
            });
            delete.Clicked += (sender, e) => OnDeleteSelected();

            Edit.Clicked += (sender, e) => Editing = !Editing;
            Edit.SetBinding<string, bool>(Button.TextProperty, this, "Editing", value => value ? "Cancel" : "Edit");

            this.WhenPropertyChanged(EditingProperty, EditingChanged);
        }

        private void EditingChanged(object sender, EventArgs e)
        {
            this.Root<AbsoluteLayout>().Children.Add(EditingToolbar, new Rectangle(0.5, 1, 1, -1), AbsoluteLayoutFlags.PositionProportional | AbsoluteLayoutFlags.WidthProportional);
            EditingToolbar.IsVisible = Editing;
        }

        private void OnDeleteSelected()
        {
            for (int i = 0; i < ItemsSource.Count; i++)
            {
                if (ItemsSource[i].IsSelected)
                {
                    ItemsSource.RemoveAt(i--);
                }
            }

            Editing = false;
        }

        private void OnDelete(object sender)
        {
            ItemsSource.Remove((dynamic)sender);
            ContextAction?.Invoke(sender, new EventArgs<ContextActions>(ContextActions.Delete));
        }

        public class EditCell : ViewCell
        {
            public static BindableProperty ViewProperty = BindableProperty.Create("View", typeof(View), typeof(EditCell), propertyChanged: OnViewPropertyChanged);

            new public View View
            {
                get => (View)ViewHolder.GetValue(ContentView.ContentProperty);
                set => ViewHolder.SetValue(ContentView.ContentProperty, value);
            }

            public readonly CheckBox Selected;

            private readonly ContentView ViewHolder = new ContentView();

            public EditCell()
            {
                base.View = new StackLayout
                {
                    Orientation = StackOrientation.Horizontal,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    Padding = new Thickness(25, 0, 25, 0),
                    Children =
                    {
                        (Selected = new CheckBox
                        {
                            Color = CrunchStyle.CRUNCH_PURPLE,
                        }),
                        ViewHolder,
                    },
                };

                ViewHolder.HorizontalOptions = LayoutOptions.FillAndExpand;
                Selected.SetBinding(CheckBox.IsCheckedProperty, "IsSelected", BindingMode.OneWayToSource);

                /*Selected.WhenPropertyChanged(CheckBox.IsCheckedProperty, (sender, e) =>
                {
                    if (!(Parent is EditListView parent))
                    {
                        return;
                    }

                    if (Selected.IsChecked)
                    {
                        parent.DeleteSelected += DeleteMe;
                    }
                    else
                    {
                        parent.DeleteSelected -= DeleteMe;
                    }
                });*/

                AddContextAction(new MenuItem
                {
                    Text = "Delete",
                    IsDestructive = true,
                }, new EventHandler((sender, e) => (Parent as EditListView)?.OnDelete((sender as MenuItem)?.CommandParameter)));
                
                AddContextAction(new MenuItem
                {
                    Text = "Edit",
                }, EditListView.ContextActions.Edit);
            }

            private void AddContextAction(MenuItem item, ContextActions action) => AddContextAction(item, (sender, e) => (Parent as EditListView)?.ContextAction?.Invoke((sender as MenuItem)?.CommandParameter, new EventArgs<ContextActions>(action)));

            private void AddContextAction(MenuItem item, EventHandler onClicked)
            {
                item.SetBinding(MenuItem.CommandParameterProperty, ".");
                item.Clicked += onClicked;

                ContextActions.Add(item);
            }

            private static void OnViewPropertyChanged(BindableObject bindable, object oldValue, object newValue) => ((EditCell)bindable).View = (View)newValue;

            protected override void OnParentSet()
            {
                base.OnParentSet();
                
                if (!(Parent is EditListView parent))
                {
                    return;
                }
                
                Selected.SetBinding<double, bool>(WidthRequestProperty, Parent, "Editing", value => value ? -1 : 0);
                Parent.WhenPropertyChanged(EditingProperty, (sender, e) =>
                {
                    if (!parent.Editing)
                    {
                        Selected.IsChecked = false;
                    }
                });
            }
        }
    }
}
