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

        private readonly ContentView HeaderView;
        private readonly Layout<View> EditingToolbar;

        public EditListView()
        {
            IsPullToRefreshEnabled = false;
            SelectionMode = ListViewSelectionMode.None;
            ItemTemplate = new DataTemplate(typeof(EditCell));
            HasUnevenRows = true;

            Button edit, delete;

            //base.Header = new AbsoluteLayout { Children = { new StackLayout
            base.Header = new StackLayout
            {
                Orientation = StackOrientation.Vertical,
                Spacing = 0,
                Children =
                {
                    (HeaderView = new ContentView()),
                    new ToolbarView
                    {
                        (edit = new Button
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
                            FontSize = NamedSize.Title.On<Button>(),
                        })
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
                if (EditingToolbar.IsVisible)
                {
                    Footer = new StackLayout
                    {
                        HeightRequest = EditingToolbar.Height,
                    };
                }
                else
                {
                    ClearValue(FooterProperty);
                }
                
                return;

                Thickness margin = Margin;
                margin.Bottom += EditingToolbar.Height * (EditingToolbar.IsVisible.ToInt() * 2 - 1);
                Margin = margin;
            });
            delete.Clicked += (sender, e) => DeleteSelected();

            edit.Clicked += (sender, e) => Editing = !Editing;
            edit.SetBinding(Button.TextProperty, this, "Editing", converter: new ValueConverter<bool, string>(value => value ? "Cancel" : "Edit", null));

            this.WhenPropertyChanged(EditingProperty, EditingChanged);
        }

        private void EditingChanged(object sender, EventArgs e)
        {
            this.Root<AbsoluteLayout>().Children.Add(EditingToolbar, new Rectangle(0.5, 1, 1, -1), AbsoluteLayoutFlags.PositionProportional | AbsoluteLayoutFlags.WidthProportional);
            EditingToolbar.IsVisible = Editing;

            if (!Editing)
            {
                foreach (object item in ItemsSource)
                {
                    if ((item as View)?.Parent<EditCell>() is EditCell editCell)
                    {
                        editCell.Selected.IsChecked = false;
                    }
                }
            }
        }

        private void DeleteSelected()
        {
            List<object> delete = new List<object>();
            foreach(object item in ItemsSource)
            {
                if ((item as View)?.Parent<EditCell>() is EditCell editCell && editCell.Selected.IsChecked)
                {
                    delete.Add(item);
                }
            }

            foreach(object item in delete)
            {
                OnDelete(item);
            }

            Editing = false;
        }

        private void OnDelete(object sender)
        {
            try
            {
                ((dynamic)ItemsSource).Remove((dynamic)sender);
            }
            catch { }

            ContextAction?.Invoke(sender, new EventArgs<ContextActions>(ContextActions.Delete));
        }

        private class EditCell : ViewCell
        {
            public readonly CheckBox Selected;

            private readonly ContentView ViewHolder;

            public EditCell()
            {
                View = new StackLayout
                {
                    Orientation = StackOrientation.Horizontal,
                    Padding = new Thickness(25, 0, 25, 0),
                    Children =
                    {
                        (Selected = new CheckBox
                        {
                            Color = CrunchStyle.CRUNCH_PURPLE,
                            IsVisible = false
                        }),
                        (ViewHolder = new ContentView{ }),
                    }
                };
                
                ViewHolder.SetBinding(ContentView.ContentProperty, ".");

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

            protected override void OnParentSet()
            {
                base.OnParentSet();

                if (!(Parent is EditListView))
                {
                    return;
                }

                Selected.SetBinding(IsVisibleProperty, Parent, "Editing");
            }
        }
    }
}
