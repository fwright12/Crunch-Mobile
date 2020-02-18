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
    public class AddFunction : Frame
    {
        public readonly Button ConfirmAdd;
        public readonly Button CancelButton;

        //public double AnimationSpeed { get; set; } = 0.125;

        private readonly Label ErrorMessageLabel;

        public readonly Equation FunctionToAdd;

        public AddFunction()
        {
            Content = new StackLayout
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
                Children =
                {
                    new StackLayout
                    {
                        Orientation = StackOrientation.Horizontal,
                        Padding = new Thickness(5, 5, 5, 0),
                        Children =
                        {
                            (CancelButton = new Button
                            {
                                VerticalOptions = LayoutOptions.Start,
                                Text = "Cancel",
                            }),
                            (ErrorMessageLabel = new Label
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
            };

            foreach (View child in FunctionToAdd.Children)
            {
                child.IsVisible = child == FunctionToAdd.LHS;
            }
            FunctionToAdd.AnswerChanged += (sender, e) =>
            {
                ConfirmAdd.IsEnabled = e.NewValue == null ? false : e.NewValue.Unknowns.Count > 0;

                // If there's no answer but there's already an error message, leave the message
                if (!(e.NewValue == null && ErrorMessageLabel.Text.Length > 0))
                {
                    ErrorMessageLabel.Text = ErrorText(e.NewValue);
                }
            };

            CancelButton.Clicked += (sender, e) => Cancel();
            ConfirmAdd.Clicked += (sender, e) =>
            {
                Close();
            };
        }

        public void Cancel()
        {
            Close();
        }

        private void Close()
        {
            IsVisible = false;
            SoftKeyboard.Cursor.Remove();
            //this.AnimateAtSpeed("Close", value => this.MoveTo(X, value), Y, -Height, 16, AnimationSpeed, Easing.BounceOut, (final, canceled) => IsVisible = false);
        }

        public void AddEquation(string existing = null)
        {
            FunctionToAdd.LHS.Children.Clear();
            SoftKeyboard.MoveCursor(FunctionToAdd.LHS);
            if (existing != null)
            {
                SoftKeyboard.Type(existing);
            }

            ConfirmAdd.Text = existing == null ? "Add" : "Update";
            ErrorMessageLabel.Text = ErrorText(null);
            IsVisible = true;
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
    }

    public class FunctionsDrawer : Frame
	{
        public class ListView : ActionableListView
        {
            public bool ContextActionsShowing = false;
        }

        public class FunctionViewModel
        {
            public string String { get; set; }

            public static implicit operator FunctionViewModel(string value) => new FunctionViewModel { String = value };
            public static implicit operator string(FunctionViewModel model) => model?.String;
        }

        public AbsoluteLayout DropArea { get; set; }
        public double TransitionSpeed = 0.75;// 2 / 3.0;

        public readonly EditListView FunctionsList;
        public readonly View Keyboard;
        private readonly AddFunction AddFunctionLayout;

        private readonly AnyVisualState ClosedState;
        private readonly List<double> OpenValues;
        private readonly string SAVED_FUNCTIONS_FILE_NAME = "functions.txt";

        private ObservableCollection<FunctionViewModel> Functions;
        private string Filename => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), SAVED_FUNCTIONS_FILE_NAME);
        private int IndexForFunction;

        public double MaxDrawerHeight
        {
            get
            {
                double availableHeight = this.Parent<View>().Height;
                
                if (availableHeight <= Keyboard.Height)
                {
                    return availableHeight;
                }
                else
                {
                    if (AddFunctionLayout.IsVisible)
                    {
                        availableHeight -= Math.Max(0, AddFunctionLayout.Height);
                    }
                    availableHeight *= 0.9;
                    return App.Current.Home.Collapsed ? availableHeight : Math.Min(availableHeight, Keyboard.Height * 2);
                }
            }
        }

        public FunctionsDrawer(View keyboard, AddFunction addFunctionLayout)
        {
            Keyboard = keyboard;
            AddFunctionLayout = addFunctionLayout;

            ClosedState = new AnyVisualState
            {
                Setters =
                {
                    new AnySetter { Action = value => CornerRadius = (float)(dynamic)value, Value = 0 },
                    new AnySetter<double> { Action = value => BackgroundColor = new Color(255, 255, 255, value), Value = 0 },
                    //new AnySetter<double> { Action = value => (FunctionsList.ListView.Header as Layout<View>).Padding = new Thickness(0, value, 0, 0), Value = 0 },
                }
            };
            OpenValues = new List<double> { 20, 1, 20 };

            BackgroundColor = Color.Transparent;
            Padding = new Thickness(0);
            HasShadow = false;
            IsClippedToBounds = true;

            Functions = new ObservableCollection<FunctionViewModel>();
            if (File.Exists(Filename))
            {
                string text = File.ReadAllText(Filename).Trim('\n');
                Print.Log("read from file", text);
#if DEBUG
                text = string.Join('\n', new List<string>
                {
                    "√(a^2+b^2)",
                    "(-b+√(b^2-4ac))/(2a)",
                    "p(1+r/n)^(n/t)",
                    "√(b^2+c^2-2bccos\u03B8)",
                    "1/2mv^2",
                    "ut+1/2at^2",
                    "(GMm)/(r^2)",
                    //"a+b+c+d+e+f+g+h+i+j+k+l+m+n+o+p+q+r+s+t+u+v+w+y+z",
                });
#endif

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
            Label noFunctions;

            Content = new StackLayout
            {
                Orientation = StackOrientation.Vertical,
                Children =
                {
                    (FunctionsList = listView = new ListView
                    {
                        BackgroundColor = Color.Transparent,
                        ItemsSource = Functions,
                        ItemTemplate = new DataTemplate(() =>
                        {
                            EditCell cell = new EditCell();
                            cell.SetBinding<View, string>(EditCell.ViewProperty, "String", MakeExpression);
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
                        Header = Keyboard,
                    })
                }
            };
            //(listView.Header as View).SizeChanged += (sender, e) => Print.Log("header size changed", ((View)sender).Bounds.Size);

            AddFunctionLayout.ConfirmAdd.Clicked += (sender, e) =>
            {
                string function = AddFunctionLayout.FunctionToAdd.LHS.ToString();

                if (IndexForFunction != -1)
                {
                    Functions[IndexForFunction] = function;
                }
                else
                {
                    Functions.Insert(0, function);
                }
            };

            noFunctions = new Label
            {
                Text = "\nNo functions yet!\n\nTap the \"+\" button to add one",
                HorizontalTextAlignment = TextAlignment.Center,
                FontSize = NamedSize.Large.On<Label>(),
            };
            Functions.CollectionChanged += (sender, e) =>
            {
                FunctionsList.ListView.Footer = Functions.Count == 0 ? noFunctions : null;
            };
            FunctionsList.ListView.Footer = Functions.Count == 0 ? noFunctions : null;
            //noFunctions.SetBinding<bool, int>(IsVisibleProperty, Functions, "Count", value => value == 0);

            Functions.CollectionChanged += (sender, e) =>
            {
                SaveToken?.Cancel();
                SaveToken = new CancellationTokenSource();
                System.Threading.Tasks.Task.Run(SaveFunctions, SaveToken.Token);
            };

            FunctionsList.Add.Clicked += (sender, e) => EditFunctionAt();
            AddFunctionLayout.WhenPropertyChanged(IsVisibleProperty, (sender, e) =>
            {
                ChangeStatus();
            });

            /*FunctionsList.Add.Clicked += (sender, e) =>
            {
                AddFunctionLayout.AddEquation();
                ChangeStatus();

                /*bool willHit = Bounds.Contains(new Rectangle(new Point(AddFunctionLayout.X, Y), new Size(AddFunctionLayout.Width, 0)));
                double y1, y2;
                y1 = y2 = -AddFunctionLayout.Height;
                
                AddFunctionLayout.AnimateAtSpeed("Open", value =>
                {
                    if (willHit)
                    {
                        if (y2 - y1 < 0 && y1 - value > 0)
                        {
                            //Print.Log("positive inflection point");
                            //ChangeStatus();
                            this.SnapTo(FunctionsList.ListView.Header as View, AddFunctionLayout.AnimationSpeed);
                        }
                        else if (y2 - y1 > 0 && y1 - value < 0)
                        {
                            AddFunctionLayout.AbortAnimation("Open");
                            AddFunctionLayout.AnimateAtSpeed("Open", value1 => AddFunctionLayout.MoveTo(AddFunctionLayout.X, value1), AddFunctionLayout.Y, 0, 16, AddFunctionLayout.AnimationSpeed, Easing.BounceOut);
                        }

                        y2 = y1;
                        y1 = value;
                    }

                    AddFunctionLayout.MoveTo(AddFunctionLayout.X, value);
                }, y1, Math.Min(0, willHit ? Y - AddFunctionLayout.Height : 1), 16, AddFunctionLayout.AnimationSpeed, Easing.BounceOut);
            };*/
            FunctionsList.ListView.ItemTapped += (sender, e) =>
            {
                if (FunctionsList.Editing)
                {
                    return;
                }

                ChangeStatus();

                MainPage mainPage = this.Parent<MainPage>();
                DropFunction((FunctionViewModel)e.Item);
            };
            listView.ContextActionClicked += (sender, e) =>
            {
                if (e.Value.Text.ToLower().Trim() == "edit")
                {
                    EditFunctionAt(Functions.IndexOf((FunctionViewModel)sender));
                }
            };
            FunctionsList.AddSnapPoint(Keyboard);

            FunctionsList.WhenPropertyChanged(HeightProperty, (sender, e) =>
            {
                double start = Keyboard.Height;
                double end = MaxDrawerHeight;
                double percent = start == end ? 0 : (FunctionsList.Height - start) / Math.Abs(end - start);
                //Print.Log("height changed", Height, FunctionsList.Height, Keyboard.Height, percent);
                percent = percent.Bound(0, 1);

                Transition(percent);
            });

            //Content.SetBinding<Color, bool>(BackgroundColorProperty, AddFunctionLayout, "IsVisible", value => value ? Color.White : Color.Transparent);

            FunctionsList.SnapTo(keyboard);
        }

        public void ChangeStatus()
        {
            if (FunctionsList.HeightRequest >= MaxDrawerHeight)
            {
                FunctionsList.ListView.ScrollToPosition(0, 0, true);
                FunctionsList.SnapTo(Keyboard, TransitionSpeed);
            }
            else
            {
                FunctionsList.SnapTo(MaxDrawerHeight, TransitionSpeed);
            }
        }
            
        private void EditFunctionAt(int index = -1)
        {
            IndexForFunction = index;
            AddFunctionLayout.AddEquation(index == -1 ? null : Functions[index]);
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

            /*if (AddStackLayout.IsVisible)
            {
                (FunctionsList.ListView.Header as View).Margin = new Thickness(0);
            }*/

            if (percent != 1)
            {
                FunctionsList.Editing = false;
            }
        }

        private View MakeExpression(string text)
        {
            if (text == null)
            {
                return null;
            }

            Expression expression = new Expression(Reader.Render(text))
            {
                FontSize = 33,
            };

            StackLayout content = new TouchableStackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Children = { expression },
                HorizontalOptions = LayoutOptions.FillAndExpand,
            };
            
            /*content.AddContinuousTouchListener<TouchEventArgs>(Gesture.LongClick, (sender, e) =>
            {
                Print.Log("continuous long click", e.State, e.Point);
            });*/
            content.AddLongClickListener(DragFunction);
            //LongClickGestureRecognizer lgr = new LongClickGestureRecognizer();
            //lgr.Listener += DragFunction;
            //expression.AddGestureRecognizer(lgr);

            /*#if ANDROID
                        expression.Touch += (sender, e) =>
                        {
                            Print.Log("touch", e.State, (FunctionsList.ListView as ListView)?.ContextActionsShowing);
                            if (e.State == TouchState.Moving && ((FunctionsList.ListView as ListView)?.ContextActionsShowing ?? false))
                            {
                                DragFunction(sender, e);
                            }
                        };

            #else
                        LongClickGestureRecognizer lgr = new LongClickGestureRecognizer();
                        lgr.LongClick += DragFunction;
                        expression.AddGestureRecognizer(lgr);
            #endif*/

            return content;
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

        private void DragFunction(object sender, DiscreteTouchEventArgs e)
        {
            if (FunctionsList.Editing)
            {
                return;
            }

            Expression expression = (Expression)((StackLayout)sender).Children[0];
            Expression copy = new Expression(Reader.Render(expression.ToString()));
            Print.Log("dragging", expression, sender.GetHashCode());
            TouchScreen.BeginDrag(copy, this.Root<AbsoluteLayout>(), TouchScreen.LastTouch.Subtract(new Point (TouchScreen.Instance.Padding.Left + expression.Width / 2, TouchScreen.Instance.Padding.Top + expression.Height / 2)));//.Subtract(new Point(expression.Width / 2 + TouchScreen.Instance.Padding.Left, expression.Height / 2 + TouchScreen.Instance.Padding.Top)));
            ChangeStatus();

            TouchScreen.Dragging += (sender1, e1) =>
            {
                if (e1.Value != DragState.Ended)
                {
                    return;
                }

                DropFunction(copy.ToString(), copy.Bounds.Location.Subtract(DropArea.PositionOn(copy.Parent<VisualElement>())));
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
 