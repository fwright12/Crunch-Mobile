using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using System.Extensions;
using System.IO;
using System.Threading;
using Xamarin.Forms;
using Xamarin.Forms.Extensions;
using Xamarin.Forms.MathDisplay;

namespace Calculator
{
    public class FunctionsDrawer : Frame
	{
        public AbsoluteLayout DropArea { get; set; }
        public double TransitionSpeed = 0.75;// 2 / 3.0;

        public readonly EditListView FunctionsList;
        private readonly StackLayout RootLayout;
        private readonly StackLayout AddStackLayout;
        private readonly Button ConfirmAdd;

        public readonly View Keyboard;

        private readonly AnyVisualState ClosedState;
        private readonly List<double> OpenValues;
        private readonly string SAVED_FUNCTIONS_FILE_NAME = "functions.txt";

        private Equation FunctionToAdd;
        private ObservableCollection<string> Functions;
        private string Filename => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), SAVED_FUNCTIONS_FILE_NAME);

        public double MaxDrawerHeight => RootLayout.Orientation == StackOrientation.Horizontal ? Keyboard.Height : ((App.Current.Collapsed ? 0.9 * this.Parent<View>().Height : Keyboard.Height * 2) - (AddStackLayout.IsVisible ? AddStackLayout.Height : 0));

        public FunctionsDrawer(View keyboard)
        {
            ClosedState = new AnyVisualState
            {
                Setters =
                {
                    //new Setter { Property = CornerRadiusProperty, Value = 0 },
                    new AnySetter { Action = value => CornerRadius = (float)(dynamic)value, Value = 0 },
                    new AnySetter<double> { Action = value => FunctionsList.BackgroundColor = new Color(255, 255, 255, value), Value = 0 },
                    //new AnySetter<double> { Action = value => Padding = new Thickness(0, value, 0, 0), Value = 0 },
                }
            };
            OpenValues = new List<double> { 20, 1 };

            BackgroundColor = Color.Transparent;
            Padding = new Thickness(0);
            Keyboard = keyboard;
            HasShadow = false;
            IsClippedToBounds = true;

            List<string> dummyData = new List<string>
            {
                "(-b+√(b^2-4ac))/(2a)",
                "(-b+√(b^2-4ac))/(2a)",
                "(-b+√(b^2-4ac))/(2a)",
                "(-b+√(b^2-4ac))/(2a)",
                "(-b+√(b^2-4ac))/(2a)",
                "(-b+√(b^2-4ac))/(2a)",
                "a+b+c+d+e+f+g+h+i+j+k+l+m+n+o+p+q+r+s+t+u+v+w+y+z",
                "√(x^2+y^2)"
            };
            Functions = new ObservableCollection<string>();
            if (File.Exists(Filename))
            {
                string text = File.ReadAllText(Filename).Trim('\n');
                Print.Log("read from file", text);
                text = string.Join('\n', dummyData);

                //foreach (string function in dummyData)
                foreach (string function in text.Split('\n'))
                {
                    if (function.Length == 0)
                    {
                        continue;
                    }

                    Print.Log("found function |" + function + "|", function.Length, function.Length > 0 ? function[0] : -1);
                    Functions.Add(function);
                }
            }

            ActionableListView listView;
            Label noVariables;
            Button cancel;
            
            Content = RootLayout = new StackLayout
            {
                Children =
                {
                    (AddStackLayout = new StackLayout
                    {
                        Resources = new ResourceDictionary
                        {
                            new StyleSetter<Button>
                            {
                                new Setter { Property = Button.TextColorProperty, Value = Color.Default },
                                new Setter { Property = BackgroundColorProperty, Value = Color.Transparent },
                                new Setter { Property = Button.FontSizeProperty, Value = NamedSize.Body.On<Button>() },
                                //new Setter { Property = PaddingProperty, Value = new Thickness(0, 10, 0, 0) }
                            }
                        },
                        Orientation = StackOrientation.Vertical,
                        IsVisible = false,
                        Children =
                        {
                            new StackLayout
                            {
                                Orientation = StackOrientation.Horizontal,
                                Padding = new Thickness(5, 5, 5, 0),
                                Children =
                                {
                                    (cancel = new Button
                                    {
                                        VerticalOptions = LayoutOptions.Start,
                                        Text = "Cancel",
                                    }),
                                    (noVariables = new Label
                                    {
                                        HorizontalOptions = LayoutOptions.CenterAndExpand,
                                        VerticalOptions = LayoutOptions.Center,
                                        HorizontalTextAlignment = TextAlignment.Center,
                                        FontSize = NamedSize.Medium.On<Label>(),
                                    }),
                                    (ConfirmAdd = new Button
                                    {
                                        VerticalOptions = LayoutOptions.Start,
                                        Text = "Add",
                                        IsEnabled = false,
                                    })
                                }
                            },
                            (FunctionToAdd = new Equation
                            {
                                HorizontalOptions = LayoutOptions.Center
                            })
                        }
                    }),
                    (FunctionsList = listView = new ActionableListView
                    {
                        ItemsSource = Functions,
                        ItemTemplate = new DataTemplate(() =>
                        {
                            EditCell cell = new EditCell();
                            cell.SetBinding<View, string>(EditCell.ViewProperty, ".", MakeExpression);
                            return cell;
                        }),
                        ContextActions =
                        {
                            new MenuItem
                            {
                                Text = "Delete",
                                IsDestructive = true,
                            },
                            new MenuItem
                            {
                                Text = "Edit",
                            }
                        },
                        SelectionMode = ListViewSelectionMode.None,
                        IsPullToRefreshEnabled = false,
                        HasUnevenRows = true,
                        Header = keyboard,
                    })
                }
            };

            AddStackLayout.WhenPropertyChanged(StackLayout.OrientationProperty, OnAddStackLayoutIsVisibleChanged);
            AddStackLayout.SetBinding(BackgroundColorProperty, this, "BackgroundColor");

            Label noFunctions = new Label
            {
                Text = "\nNo functions yet!\n\nTap the \"+\" button to add one",
                HorizontalTextAlignment = TextAlignment.Center,
                FontSize = NamedSize.Large.On<Label>(),
            };

            FunctionsList.ListView.SetBinding<object, int>(ListView.FooterProperty, Functions, "Count", value => value == 0 ? noFunctions : null);

            Functions.CollectionChanged += (sender, e) =>
            {
                SaveToken?.Cancel();
                SaveToken = new CancellationTokenSource();
                System.Threading.Tasks.Task.Run(SaveFunctions, SaveToken.Token);
            };

            FunctionsList.ListView.ItemTapped += (sender, e) =>
            {
                ChangeStatus();

                MainPage mainPage = this.Parent<MainPage>();
                DropFunction((string)e.Item);
            };
            listView.ContextActionClicked += (sender, e) =>
            {
                if (e.Value.Text.ToLower().Trim() == "edit")
                {
                    AddEquation((string)sender);
                }
            };
            FunctionsList.AddSnapPoint(Keyboard);

            foreach (View child in FunctionToAdd.Children)
            {
                child.IsVisible = child == FunctionToAdd.LHS;
            }
            FunctionToAdd.AnswerChanged += (sender, e) =>
            {
                ConfirmAdd.IsEnabled = e.NewValue == null ? false : e.NewValue.Unknowns.Count > 0;

                // If there's no answer but there's already an error message, leave the message
                if (!(e.NewValue == null && noVariables.Text.Length > 0))
                {
                    noVariables.Text = ErrorText(e.NewValue);
                }
            };
            noVariables.Text = ErrorText(null);
            
            FunctionsList.Add.Clicked += (sender, e) => AddEquation();
            cancel.Clicked += (sender, e) =>
            {
                AddStackLayout.IsVisible = false;
                ChangeStatus();
            };
            ConfirmAdd.Clicked += (sender, e) =>
            {
                Functions.Insert(0, FunctionToAdd.LHS.ToString());

                AddStackLayout.IsVisible = false;
                ChangeStatus();
            };

            (this).WhenPropertyChanged(CornerRadiusProperty, (sender, e) =>
            {
                (FunctionsList.ListView.Header as View).Margin = new Thickness(0, RootLayout.Orientation == StackOrientation.Vertical ? CornerRadius : 0, 0, 0);
            });
            FunctionsList.WhenPropertyChanged(HeightProperty, (sender, e) =>
            {
                if (AddStackLayout.IsVisible)
                {
                    return;
                }

                double start = Keyboard.Height;
                double end = MaxDrawerHeight;
                double percent = start == end ? 0 : (FunctionsList.Height - start) / Math.Abs(end - start);
                //Print.Log("height changed", Height, FunctionsList.Height, Keyboard.Height, percent);
                percent = percent.Bound(0, 1);

                Transition(percent);
            });

            Keyboard.SetBinding<Color, StackOrientation>(BackgroundColorProperty, RootLayout, "Orientation", value => value == StackOrientation.Vertical ? Color.Transparent : CrunchStyle.BACKGROUND_COLOR);
            FunctionsList.SetBinding<Color, StackOrientation>(BackgroundColorProperty, RootLayout, "Orientation", value => value == StackOrientation.Vertical ? Color.Transparent : Color.White);

            FunctionsList.SnapTo(keyboard);
        }

        public void ChangeStatus()
        {
            if (FunctionsList.HeightRequest == MaxDrawerHeight)
            {
                FunctionsList.ListView.ScrollToPosition(0, 0, true);
                FunctionsList.SnapTo(Keyboard, TransitionSpeed);
            }
            else
            {
                FunctionsList.SnapTo(MaxDrawerHeight, TransitionSpeed);
            }
        }

        private void OnAddStackLayoutIsVisibleChanged(object sender, EventArgs e)
        {
            AddStackLayout.WidthRequest = AddStackLayout.IsVisible && RootLayout.Orientation == StackOrientation.Horizontal ? this.Parent<View>().Width * 0.9 - FunctionsList.Width : -1;

            Transition(AddStackLayout.IsVisible.ToInt());
        }

        private void Transition(double percent)
        {
            int i = 0;
            foreach (AnySetter setter in ClosedState.AnySetters(this))
            {
                double value1 = OpenValues[i++];
                double value2 = (int)setter.Value;

                setter.Action(Math.Min(value1, value2) + percent * Math.Abs(value1 - value2));
            }

            if (AddStackLayout.IsVisible)
            {
                (FunctionsList.ListView.Header as View).Margin = new Thickness(0);
            }

            if (percent != 1)
            {
                FunctionsList.Editing = false;
            }
        }

        private void AddEquation(string existing = null)
        {
            FunctionToAdd.LHS.Children.Clear();
            SoftKeyboard.MoveCursor(FunctionToAdd.LHS);
            if (existing != null)
            {
                SoftKeyboard.Type(existing);
            }

            ConfirmAdd.Text = existing == null ? "Add" : "Update";
            AddStackLayout.IsVisible = true;

            ChangeStatus();
        }

        private Expression MakeExpression(string text)
        {
            if (text == null)
            {
                return null;
            }

            Expression expression = new Expression(Reader.Render(text))
            {
                FontSize = 33,
                HorizontalOptions = LayoutOptions.FillAndExpand
            };
            //expression.Touch += DragFunction;
            expression.OnRequestAddLongClick(DragFunction);
            
            return expression;
        }

        private string ErrorText(Crunch.Operand answer)
        {
            string error = "";

            if (!ConfirmAdd.IsEnabled)
            {
                if (answer?.Unknowns.Count == 0)
                {
                    error = "Function must contain variables!";
                }
                else
                {
                    error = "Invalid Function!";
                }
            }

            return error;
        }

        private CancellationTokenSource SaveToken = null;

        private void SaveFunctions()
        {
            string text = "";

            foreach (string function in Functions)
            {
                SaveToken.Token.ThrowIfCancellationRequested();
                text += function + '\n';
            }

            File.WriteAllText(Filename, text);
        }

        private void DragFunction(object sender, TouchEventArgs e)
        {
            /*if (e.State != TouchState.Moving)
            {
                return;
            }*/

            Expression copy = new Expression(Reader.Render((sender as Expression).ToString()));
            TouchScreen.BeginDrag(copy, this.Root<AbsoluteLayout>(), TouchScreen.LastTouch.Subtract(e.Point));
            ChangeStatus();

            TouchScreen.Dragging += (sender1, e1) =>
            {
                if (e1.Value != DragState.Ended)
                {
                    return;
                }

                DropFunction(sender.ToString(), copy.Bounds.Location.Subtract(DropArea.PositionOn(copy.Parent<VisualElement>())));
                copy.Remove();
            };
        }

        private void DropFunction(string function, Point? point = null)
        {
            this.Parent<MainPage>().AddCalculation(point);

            Answer answer = SoftKeyboard.Cursor.Parent<Equation>().RHS as Answer;
            answer.NumberChoice = Crunch.Numbers.Exact;
            SoftKeyboard.Type(function);
            KeyboardManager.MoveCursor(KeyboardManager.CursorKey.Down);
        }

        protected override void OnParentSet()
        {
            base.OnParentSet();
            FunctionsList.AddSnapPoint(MaxDrawerHeight);
        }
    }
}
 