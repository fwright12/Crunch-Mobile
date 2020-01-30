using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using System.Extensions;
using Xamarin.Forms;
using Xamarin.Forms.Extensions;

namespace Calculator
{
    public class EditCell : EditListView.EditCell { }

    public class EditListView : StackLayout
    {
        public static BindableProperty EditingProperty = BindableProperty.Create("Editing", typeof(bool), typeof(EditListView), false);

        public bool Editing
        {
            get => (bool)GetValue(EditingProperty);
            set => SetValue(EditingProperty, value);
        }

        private event SimpleEventHandler DeleteSelected;

        public readonly ListView ListView;
        public readonly Layout<View> EditingToolbar;
        public readonly Button Add;
        public readonly Button Edit;

        private readonly StackLayout Header;
        private readonly ContentView HeaderView;

        public EditListView() : this(new ActionableListView
        {
            ContextActions =
            {
                new MenuItem
                {
                    Text = "Delete",
                    IsDestructive = true
                }
            }
        })
        { }

        private EditListView(ListView listView)
        {
            Orientation = StackOrientation.Vertical;
            Spacing = 0;
            ListView = listView;

            Header = new StackLayout
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

            Button delete;

            Children.Add(ListView);
            Children.Add(EditingToolbar = new StackLayout
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
            });

            EditingToolbar.SetBinding(IsVisibleProperty, this, "Editing");

            Edit.Clicked += (sender, e) => Editing = !Editing;
            Edit.SetBinding<string, bool>(Button.TextProperty, this, "Editing", value => value ? "Cancel" : "Edit");
            Edit.SetBinding<bool, int>(IsEnabledProperty, listView.ItemsSource, "Count", value => value > 0);

            delete.Clicked += (sender, e) => OnDeleteSelected();

            //ListView.SetBinding(Xamarin.Forms.ListView.HeaderProperty, this, "Header", mode: BindingMode.TwoWay);
            //ListView.WhenPropertyChanged(Xamarin.Forms.ListView.HeaderProperty, HeaderChanged);
            HeaderChanged(null, null);

            ListView.ItemTemplate.SetBinding(EditCell.EditingProperty, new Binding("Editing", source: this));
            if (ListView is ActionableListView actionableListView)
            {
                actionableListView.ContextActionClicked += (sender, e) =>
                {
                    if (e.Value.IsDestructive)
                    {
                        OnDelete(sender);
                    }
                };
            }
            ListView.GetSwipeListener().Drawer = this;
        }

        public static implicit operator EditListView(ListView listView) => new EditListView(listView);

        private void OnDelete(object sender)
        {
            try
            {
                ((dynamic)ListView.ItemsSource).Remove((dynamic)sender);
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("Failed to remove item from the ListView. To support item removal, ItemsSource must be set to a type that has a 'Remove(T item)' function (where T is the type of the Enumeration) and implements INotifyCollectionChanged. Consider using an ObservableCollection");
            }
        }

        private void HeaderChanged(object sender, EventArgs e)
        {
            if (ListView.Header == HeaderView.Content)
            {
                return;
            }

            HeaderView.Content = ListView.Header as View;
            ListView.Header = Header;
        }

        private void OnDeleteSelected()
        {
            /*for (int i = 0; i < ListView.ItemsSource.Count; i++)
            {
                if (ListView.ItemsSource[i].IsSelected)
                {
                    ListView.ItemsSource.RemoveAt(i--);
                }
            }*/

            DeleteSelected?.Invoke();
            Editing = false;
        }

        public class EditCell : ViewCell
        {
            public static BindableProperty EditingProperty = BindableProperty.Create("Editing", typeof(bool), typeof(EditListView), false, propertyChanged: OnEditingPropertyChanged);

            public bool Editing
            {
                get => (bool)GetValue(EditingProperty);
                set => SetValue(EditingProperty, value);
            }

            public static BindableProperty ViewProperty = BindableProperty.Create("View", typeof(View), typeof(EditCell), propertyChanged: OnViewPropertyChanged);

            new public View View
            {
                get => (View)ViewHolder.GetValue(ContentView.ContentProperty);
                set => ViewHolder.SetValue(ContentView.ContentProperty, value);
            }

            private readonly CheckBox Selected;

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
                Selected.SetBinding<double, bool>(WidthRequestProperty, this, "Editing", value => value ? -1 : 0);
                //Selected.SetBinding(CheckBox.IsCheckedProperty, "IsSelected", BindingMode.OneWayToSource);
                //Selected.SetBinding(IsVisibleProperty, this, "Editing");

                Selected.WhenPropertyChanged(CheckBox.IsCheckedProperty, (sender, e) =>
                {
                    if (!(this.Parent<EditListView>() is EditListView parent))
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
                });
            }

            private void DeleteMe()
            {
                Selected.IsChecked = false;
                this.Parent<EditListView>()?.OnDelete(BindingContext);
            }

            private static void OnEditingPropertyChanged(BindableObject bindable, object oldValue, object newValue)
            {
                EditCell cell = (EditCell)bindable;

                if (!(bool)newValue)
                {
                    cell.Selected.IsChecked = false;
                }
            }

            private static void OnViewPropertyChanged(BindableObject bindable, object oldValue, object newValue) => ((EditCell)bindable).View = (View)newValue;
        }
    }

    /*public class EditListView1 : ListView<EditListView.ContextActions>
    {
        new public Items ItemsSource
        {
            get => _ItemsSource;
            set => base.ItemsSource = _ItemsSource = value;
        }
        private Items _ItemsSource;

        public class ViewModel
        {
            public object Value { get; set; }

            public bool IsSelected { get; set; }

            public bool Test { get; set; }

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

        protected override void SetupContent(Cell content, int index)
        {
            base.SetupContent(content, index);

            if (!(content is EditCell editCell))
            {
                throw new Exception("The cell template for EditListView must be of or derived from EditCell");
            }
            return;
            EditListView parent = this.Parent<EditListView>();

            //editCell.Selected.SetBinding<double, bool>(WidthRequestProperty, parent, "Editing", value => value ? -1 : 0);
            parent.WhenPropertyChanged(EditListView.EditingProperty, (sender, e) =>
            {
                if (!parent.Editing)
                {
                    //editCell.Selected.IsChecked = false;
                }
            });
        }
    }*/
}
