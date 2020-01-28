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
        public class ListView : EditListView
        {
            public class ScrollToRequestEventArgs : EventArgs
            {
                public int X;
                public int Y;
                public bool Animated;

                public ScrollToRequestEventArgs(int x, int y, bool animated = false)
                {
                    X = x;
                    Y = y;
                    Animated = animated;
                }
            }

            public event EventHandler<ScrollToRequestEventArgs> ScrollToRequest;

            public ListView()
            {
                Scrolled += (sender, e) =>
                {
                    if (!ShouldScroll)
                    {
                        ScrollTo(0, 0);
                    }
                    LastScrollY = e.ScrollY;
                    //Print.Log("scrolled", e.ScrollY);
                };
                /*Scrolled += (sender, e) =>
                {
                    //Print.Log("scrolled", e.ScrollY);
                    if (!ShouldScroll)
                    {
                        //LastScrollY = e.ScrollY;
                        //ScrollTo(0, (int)LastScrollY);
                        FunctionsDrawer parent = this.Parent<FunctionsDrawer>();
                        //bool locked = this.IsLocked();
                        ShouldScroll = Height < parent.MaxDrawerHeight || e.ScrollY <= 0;
                        //Print.Log("set scrolling", ShouldScroll);
                        //Scrolling = this.IsLocked();
                    }
                };*/
            }

            public bool ShouldScroll = false;
            //public bool Scrolling { get; private set; } = false;
            private double LastTouch;
            private DateTime LastTime;
            private double LastScrollY;
            private double LastMovingSpeed;

            public bool OnScrollEvent(Point point, TouchState state)
            {
                FunctionsDrawer parent = this.Parent<FunctionsDrawer>();

                double time = DateTime.Now.Subtract(LastTime).TotalMilliseconds;
                double distance = state == TouchState.Down ? 0 : LastTouch - point.Y;
                //Print.Log("touch", LastScrollY, ShouldScroll, state, LastTouch, point.Y, delta);
                //Print.Log("touch", state, distance / time, distance, time);
                LastTouch = point.Y;
                LastTime = DateTime.Now;
                if (state == TouchState.Moving)
                {
                    LastMovingSpeed = Math.Min(parent.TransitionSpeed * 2, Math.Abs(distance) / time);
                }

                /*if (state != TouchState.Moving && state != TouchState.Up)
                {
                    ShouldScroll = false;
                }*/

                if (!ShouldScroll)
                {
                    //SortedSet<double> snapPoints = this.GetSnapPoints();
                    //HeightRequest = (HeightRequest + delta).Bound(snapPoints.Min, snapPoints.Max);
                    HeightRequest = Math.Min(parent.MaxDrawerHeight, HeightRequest + distance);

                    if (state == TouchState.Up)
                    {
                        //FunctionsDrawer parent = this.Parent<FunctionsDrawer>();
                        //double height = dy > 0 ? parent.MaxDrawerHeight : parent.Keyboard.Height;
                        double speed = parent.TransitionSpeed;// * (distance == 0 ? 1 : 2);// LastMovingSpeed;// parent.TransitionSpeed;// distance / time;
                        if (distance > 0 || (distance == 0 && Math.Abs(Height - parent.MaxDrawerHeight) < Math.Abs(Height - parent.Keyboard.Height)))
                        {
                            this.SnapTo(parent.MaxDrawerHeight, speed);
                        }
                        else
                        {
                            this.SnapTo(parent.Keyboard, speed);
                        }
                    }
                }

                return ShouldScroll = Height == parent.MaxDrawerHeight && LastScrollY >= 0;
                return ShouldScroll;
            }

            public void ScrollTo(int x, int y, bool animated = false) => OnScrollToRequest(this, new ScrollToRequestEventArgs(x, y, animated));

            private void OnScrollToRequest(object sender, ScrollToRequestEventArgs e) => ScrollToRequest?.Invoke(sender, e);
        }

        public AbsoluteLayout DropArea;
        public double TransitionSpeed = 0.75;// 2 / 3.0;

        private readonly StackLayout AddStackLayout;
        private readonly Button ConfirmAdd;

        private readonly ListView FunctionsList;
        private readonly View Keyboard;

        private readonly AnyVisualState ClosedState;
        private readonly List<double> OpenValues;
        private readonly string SAVED_FUNCTIONS_FILE_NAME = "functions.txt";

        private Equation FunctionToAdd;
        private EditListView.Items Functions;
        private string Filename => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), SAVED_FUNCTIONS_FILE_NAME);

        private double MaxDrawerHeight => App.Current.Collapsed ? 0.9 * this.Parent<View>().Height : Keyboard.Height * 2;

        public FunctionsDrawer(View keyboard)
        {
            ClosedState = new AnyVisualState
            {
                Setters =
                {
                    //new Setter { Property = CornerRadiusProperty, Value = 0 },
                    new AnySetter { Action = value => CornerRadius = (float)(dynamic)value, Value = 0 },
                    new AnySetter<double> { Action = value => BackgroundColor = new Color(255, 255, 255, value), Value = 0 },
                    //new AnySetter<double> { Action = value => Padding = new Thickness(0, value, 0, 0), Value = 0 },
                }
            };
            OpenValues = new List<double> { 20, 1 };

            Padding = new Thickness(0);
            Keyboard = keyboard;
            HasShadow = false;
            IsClippedToBounds = true;

            Label noVariables;
            Button cancel;

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
            Functions = new EditListView.Items();
            if (File.Exists(Filename))
            {
                string text = File.ReadAllText(Filename).Trim('\n');
                Print.Log("read from file", text);
                //text = string.Join('\n', dummyData);

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
            
            Content = new StackLayout
            {
                Orientation = StackOrientation.Vertical,
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
                    (FunctionsList = new ListView
                    {
                        BackgroundColor = Color.Transparent,
                        ItemsSource = Functions,
                        ItemTemplate = new DataTemplate(() =>
                        {
                            EditCell cell = new EditCell();
                            cell.SetBinding<View, string>(EditCell.ViewProperty, "Value", MakeExpression);
                            return cell;
                        }),
                        SelectionMode = ListViewSelectionMode.None,
                        IsPullToRefreshEnabled = false,
                        HasUnevenRows = true,
                        Header = keyboard,
                    })
                }
            };

            Label noFunctions = new Label
            {
                Text = "\nNo functions yet!\n\nTap the \"+\" button to add one",
                HorizontalTextAlignment = TextAlignment.Center,
                FontSize = NamedSize.Large.On<Label>(),
            };

            FunctionsList.SetBinding<object, int>(ListView.FooterProperty, Functions, "Count", value => value == 0 ? noFunctions : null);
            FunctionsList.Edit.SetBinding<bool, int>(IsEnabledProperty, Functions, "Count", value => value > 0);

            Functions.CollectionChanged += (sender, e) =>
            {
                SaveToken?.Cancel();
                SaveToken = new CancellationTokenSource();
                System.Threading.Tasks.Task.Run(SaveFunctions, SaveToken.Token);
            };

            FunctionsList.ItemTapped += (sender, e) =>
            {
                ChangeStatus();

                MainPage mainPage = this.Parent<MainPage>();
                DropFunction((string)((EditListView.ViewModel)e.Item).Value);
            };
            FunctionsList.ContextAction += (sender, e) =>
            {
                if (e.Value == EditListView.ContextActions.Edit)
                {
                    AddEquation(((EditListView.ViewModel)sender).Value as string);
                }
            };
            FunctionsList.AddSnapPoint(Keyboard);

            foreach (View child in FunctionToAdd.Children)
            {
                child.IsVisible = child == FunctionToAdd.LHS;
            }
            FunctionToAdd.AnswerChanged += (sender, e) =>
            {
                /*if (e.NewValue == null)
                {
                    if (FunctionToAdd.LHS.Children.Count == 1 && FunctionToAdd.LHS.Children[0] == SoftKeyboard.Cursor)
                    {
                        ConfirmAdd.IsEnabled = false;
                    }

                    return;
                }*/

                ConfirmAdd.IsEnabled = e.NewValue == null ? false : e.NewValue.Unknowns.Count > 0;
                // If there's no answer but there's already an error message, leave the message
                if (!(e.NewValue == null && noVariables.Text.Length > 0))
                {
                    noVariables.Text = ErrorText(e.NewValue);
                }
            };
            noVariables.Text = ErrorText(null);
            //noVariables.SetBinding<string, bool>(Label.TextProperty, ConfirmAdd, "IsEnabled", value => );
            //noVariables.SetBinding<int, bool>(OpacityProperty, ConfirmAdd, "IsEnabled", value => 1 - value.ToInt());
            
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

            this.WhenPropertyChanged(CornerRadiusProperty, (sender, e) =>
            {
                (FunctionsList.Header as View).Margin = new Thickness(0, CornerRadius, 0, 0);
                //AddStackLayout.Padding = 
            });
            FunctionsList.WhenPropertyChanged(HeightProperty, (sender, e) =>
            {
                double start = Keyboard.Height;
                double end = MaxDrawerHeight;
                double percent = (FunctionsList.Height - start) / Math.Abs(end - start);
                //Print.Log("height changed", Height, FunctionsList.Height, Keyboard.Height, percent);
                percent = AddStackLayout.IsVisible ? 1 : percent.Bound(0, 1);

                Transition(percent);
            });

            FunctionsList.SnapTo(keyboard);
        }

        public void ChangeStatus()
        {
            FunctionsList.Editing = false;

            if (FunctionsList.HeightRequest == MaxDrawerHeight)
            {
                FunctionsList.ScrollTo(0, 0, true);
                FunctionsList.SnapTo(Keyboard, TransitionSpeed);
            }
            else
            {
                FunctionsList.SnapTo(MaxDrawerHeight, TransitionSpeed);
            }
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
                (FunctionsList.Header as View).Margin = new Thickness(0);
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

            foreach (EditListView.ViewModel vm in Functions)
            {
                SaveToken.Token.ThrowIfCancellationRequested();
                text += vm.Value.ToString() + '\n';
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
            if (point.HasValue)
            {
                this.Parent<MainPage>().AddCalculation(point.Value, TouchState.Up);
            }
            else
            {
                this.Parent<MainPage>().AddCalculation(TouchState.Up);
            }

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
 