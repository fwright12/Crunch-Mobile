using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;

using Xamarin.Forms;
using Xamarin.Forms.Extensions;
using Xamarin.Forms.MathDisplay;

namespace Calculator
{
    public class test : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => new Expression(Reader.Render(((string)value)));

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => (value as Expression).ToString();
    }

    public class FunctionsDrawer : ContentView
	{
        public FunctionsDrawer(AbsoluteLayout dropArea)
        {
            ListView listView = new ListView
            {
                HasUnevenRows = true,
                ItemTemplate = new DataTemplate(() =>
                {
                    ContentView content = new ContentView { };
                    content.SetBinding(ContentProperty, ".");

                    ViewCell viewCell = new ViewCell
                    {
                        View = new ScrollView
                        {
                            Orientation = ScrollOrientation.Both,
                            Content = content,
                        }
                    };

                    return viewCell;
                }),
            };

            List<string> text = new List<string>
            {
                "(-b+sqrt(b^2-4ac))/(2a)",
                "sqrt(x^2+y^2)"
            };

            List<Expression> equations = new List<Expression>();
            listView.ItemsSource = equations;

            foreach (string s in text)
            {
                equations.Add(new Expression(Reader.Render(s))
                {
                    HorizontalOptions = LayoutOptions.StartAndExpand,
                    FontSize = 20,
                });

                equations.Last().Touch += (sender, e) =>
                {
                    if (e.State != TouchState.Moving)
                    {
                        return;
                    }

                    Expression copy = new Expression(Reader.Render((sender as Expression).ToString()));
                    TouchScreen.BeginDrag(copy, this.Parent<AbsoluteLayout>(), sender as Expression);
                    ChangeStatus();

                    TouchScreen.Dragging += (state) =>
                    {
                        if (state != DragState.Ended)
                        {
                            return;
                        }

                        copy.Remove();

                        this.Parent<MainPage>().AddCalculation(TouchScreen.LastTouch.Subtract(dropArea.PositionOn()), TouchState.Up);
                        SoftKeyboard.Type((sender as Expression).ToString());
                    };
                };
            }

            Button done = new Button
            {
                Text = "done",
                HorizontalOptions = LayoutOptions.End,
                FontSize = 12,
                Padding = new Thickness(0),
                Margin = new Thickness(0),
            };
            done.Clicked += (sender, e) =>
            {
                ChangeStatus();
            };

            StackLayout layout = new StackLayout
            {
                Orientation = StackOrientation.Vertical,
                Padding = new Thickness(10),
            };
            layout.Children.Add(done);
            layout.Children.Add(listView);

            Content = layout;

            Content.WidthRequest = 300;
            Content.BackgroundColor = Color.LightGray;

            IsVisible = false;
        }

        public void ChangeStatus()
        {
            double left = (Parent as View).Width - Width;
            double right = (Parent as View).Width;

            uint length = 1000;
            Animation animation;

            if (IsVisible)
            {
                animation = new Animation(x => this.MoveTo(x, 0), left, right);
            }
            else
            {
                IsVisible = true;
                animation = new Animation(x => this.MoveTo(x, 0), right, left);
            }

            animation.Commit(this, "Move", 1, length, Easing.SinOut, finished: (a, b) => IsVisible = X == left);
        }

        /*public FunctionsDrawer(AbsoluteLayout dropArea)
        {
            TableView tableView = new TableView()
            {
                Intent = TableIntent.Settings,
                HasUnevenRows = true,
            };

            TableSection main = new TableSection { };

            foreach (string s in new string[] { "sqrt(x)" })
            {
                ViewCell temp = new ViewCell
                {
                    View = new Expression(Reader.Render(s))
                    {
                        HorizontalOptions = LayoutOptions.StartAndExpand,
                        FontSize = 20,
                    }
                };

                (temp.View as Expression).Touch += (sender, e) =>
                {
                    if (e.State != TouchState.Moving)
                    {
                        return;
                    }

                    Expression copy = new Expression(Reader.Render((sender as Expression).ToString()));
                    TouchScreen.BeginDrag(copy, this.Parent<AbsoluteLayout>(), (sender as Expression).PositionOn());
                    ChangeStatus();

                    TouchScreen.Dragging += (state) =>
                    {
                        if (state != DragState.Ended)
                        {
                            return;
                        }

                        copy.Remove();

                        this.Parent<MainPage>().AddCalculation(TouchScreen.LastTouch.Subtract(dropArea.PositionOn()), TouchState.Up);
                        SoftKeyboard.Type((sender as Expression).ToString());
                    };
                };
                main.Add(temp);
            }

            TableRoot root = new TableRoot();
            root.Add(main);

            tableView.Root = root;

            Button done = new Button
            {
                Text = "done",
                HorizontalOptions = LayoutOptions.End,
                FontSize = 12,
                Padding = new Thickness(0),
                Margin = new Thickness(0),
            };
            done.Clicked += (sender, e) =>
            {
                ChangeStatus();
            };

            StackLayout layout = new StackLayout
            {
                Orientation = StackOrientation.Vertical,
                Padding = new Thickness(10),
            };
            layout.Children.Add(done);
            layout.Children.Add(tableView);

            Content = layout;

            Content.WidthRequest = 300;
            Content.BackgroundColor = Color.LightGray;

            IsVisible = false;
        }*/
    }
}