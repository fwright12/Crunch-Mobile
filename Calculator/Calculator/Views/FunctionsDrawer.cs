using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using System.Extensions;
using System.IO;
using System.Threading;
using Xamarin.Forms;
using Xamarin.Forms.Extensions;
using Xamarin.Forms.Internals;
using Xamarin.Forms.MathDisplay;
using Xamarin.Forms.Xaml;

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
                                FontSize = NamedSize.Body.On<Button>(),
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
                                FontSize = NamedSize.Body.On<Button>(),
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
            /*Content.Resources.Add(new StyleSetter<Button>
            {
                //new Setter { Property = Button.TextColorProperty, Value = Color.Default },
                //new Setter { Property = BackgroundColorProperty, Value = Color.Transparent },
                new Setter { Property = Button.FontSizeProperty, Value = NamedSize.Body.On<Button>() },
                //new Setter { Property = PaddingProperty, Value = new Thickness(0, 10, 0, 0) }
            });*/

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

            public ListView() : base()
            {
                /*this.Bind<object>(HeaderProperty, value =>
                {
                    if (value is View view)
                    {
                        view.MeasureInvalidated += (sender, e) => base.InvalidateMeasure();
                    }
                });*/
            }

            public ListView(ListViewCachingStrategy strategy) : base(strategy) { }

            /*protected override void InvalidateMeasure()
            {
                if (!(Header is View))
                {
                    base.InvalidateMeasure();
                }
            }

            protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
            {
                SizeRequest sr = base.OnMeasure(widthConstraint, heightConstraint);
                
                if (Header is View header)
                {
                    Size request = sr.Request;
                    request.Width = header.Measure(widthConstraint, heightConstraint).Request.Width;
                    sr.Request = request;
                }
                
                return sr;
            }*/
        }

        public class FunctionViewModel
        {
            public string String { get; set; }

            public static implicit operator FunctionViewModel(string value) => new FunctionViewModel { String = value };
            public static implicit operator string(FunctionViewModel model) => model?.String;
        }

        public AbsoluteLayout DropArea { get; set; }
        public double TransitionSpeed = 0.75;

        public readonly EditListView FunctionsList;
        private readonly View Keyboard;
        private readonly AddFunction AddFunctionLayout;
        private readonly View Drawer;

        //private readonly AnyVisualState ClosedState;
        //private readonly List<double> OpenValues;
        private readonly string SAVED_FUNCTIONS_FILE_NAME = "functions.txt";

        private ObservableCollection<FunctionViewModel> Functions;
        private string Filename => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), SAVED_FUNCTIONS_FILE_NAME);
        private int IndexForFunction;

        public FunctionsDrawer() { }

        public FunctionsDrawer(View keyboard, AddFunction addFunctionLayout)
        {
            Heights = new Dictionary<bool, double>
            {
                { true, Height },
                { false, -1 }
            };
            Drawer = this;
            Keyboard = keyboard;
            AddFunctionLayout = addFunctionLayout;
            CornerRadius = 0;
            
            ActionableListView listView;
            Label noFunctions;
            BoxView cover = null;

            /*ClosedState = new AnyVisualState
            {
                Setters =
                {
                    new AnySetter { Action = value => CornerRadius = (float)(dynamic)value, Value = 0 },
                    new AnySetter<double> { Action = value => BackgroundColor = new Color(255, 255, 255, value), Value = 0 },
                    new AnySetter<double> { Action = value => cover.Opacity = value, Value = 1 },
                    new AnySetter<double> { Action = value => Keyboard.Margin = new Thickness(0, value, 0, 0), Value = 0 },
                }
            };
            OpenValues = new List<double> { 20, 1, 0, 20 };*/

            BackgroundColor = Color.Transparent;
            //Padding = new Thickness(0);
            HasShadow = false;
            IsClippedToBounds = true;

            Functions = new ObservableCollection<FunctionViewModel>();
            if (File.Exists(Filename))
            {
                string text = File.ReadAllText(Filename).Trim('\n');
                Print.Log("read from file", text);
#if DEBUG
                text = string.Join('\n', new string[]
                {
                    "√(a^2+b^2)",
                    "a+b+c+d+e+f+g+h+i+j+k+l+m+n+o+p+q+r+s+t+u+v+w+y+z",
                    "(-b+√(b^2-4ac))/(2a)",
                    "p(1+r/n)^(n/t)",
                    "√(b^2+c^2-2bccos\u03B8)",
                    "1/2mv^2",
                    "ut+1/2at^2",
                    "(GMm)/(r^2)",
                });
#endif

                foreach (string function in text.Split('\n'))
                {
                    if (function.Length == 0)
                    {
                        continue;
                    }

                    //Print.Log("found function |" + function + "|", function.Length, function.Length > 0 ? function[0] : -1);
                    Functions.Add(function);
                }
            }

            Content = new AbsoluteLayout
            {
                Children =
                {
                    {
                        (FunctionsList = listView = new ListView
                        {
                            HeightRequest = 0,
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
                                new MenuItemTemplate(() => new MenuItem
                                {
                                    Text = "Delete",
                                    IsDestructive = true,
                                }),
                                new MenuItemTemplate(() => new MenuItem
                                {
                                    Text = "Edit",
                                })
                            },
                            SelectionMode = ListViewSelectionMode.None,
                            IsPullToRefreshEnabled = false,
                            HasUnevenRows = true,
                            Header = new AbsoluteLayout
                            {
                                Children =
                                {
                                    {
                                        Keyboard,
                                        new Rectangle(0.5, 0.5, -1, -1),
                                        AbsoluteLayoutFlags.PositionProportional
                                    }
                                }
                            },
                        }),
                        new Rectangle(0, 0, 1, 1),
                        AbsoluteLayoutFlags.SizeProportional
                    },
                    {
                        (cover = new BoxView
                        {
                            IsVisible = false,
                            //BackgroundColor = App.BACKGROUND_COLOR,
                            InputTransparent = true,
                        }),
                        new Rectangle(0.5, 1, 1, -1),
                        AbsoluteLayoutFlags.PositionProportional | AbsoluteLayoutFlags.WidthProportional
                    }
                }
            };
            //BackgroundColor = Color.Black;
            //Content.BackgroundColor = new Color(245, 245, 245, 0.95);
            //Content.SetDynamicResource(BackgroundColorProperty, "KeyboardBackgroundColor");

            //Color backgroundColor = Color.White;// (Color)App.Current.Resources["SecondaryColor"];
            //BackgroundColor = backgroundColor.WithAlpha(0.5);
            BoxView test = new BoxView();

            VisualStateManager.GetVisualStateGroups(this).Add(new VisualStates("Open", "Closed")
            {
                new Setters { Property = CornerRadiusProperty, Values = { 20, 10 } },
                //{ BackgroundColorProperty, ("Open", new DynamicResource("SecondaryColor")) },
                /*new Setters
                {
                    Property = BackgroundColorProperty,
                    Values =
                    {
                        Color.White,
                        Color.White.WithAlpha(0),
                    }
                },*/
                new TargetedSetters
                {
                    Targets = test,
                    Setters =
                    {
                        new Setters
                        {
                            Property = BackgroundColorProperty,
                            Values = { Color.Black.WithAlpha(0.25), Color.Black.WithAlpha(0) }
                        }
                    }
                },
                new TargetedSetters
                {
                    Targets = (VisualElement)Keyboard.Parent,
                    Setters =
                    {
                        new Setters 
                        {
                            Property = MarginProperty,
                            Values =
                            {
                                new Thunk<Thickness>(() => new Thickness(0, 20 - ((Keyboard as Layout)?.Margin.Top ?? 0), 0, 0)),
                                new Thickness(0),
                            }
                        }
                    }
                },
                /*new TargetedSetters
                {
                    Targets = cover,
                    Setters =
                    {
                        new Setters { Property = OpacityProperty, Values = { 0, 1 } }
                    }
                }*/
            });

            FunctionsList.ListView.GetSwipeListener().Drawer = Drawer;
            FunctionsList.EditingToolbar.SetDynamicResource(BackgroundColorProperty, "PrimaryBackgroundColor");

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
            FunctionsList.ListView.SetBinding<View, int>(Xamarin.Forms.ListView.FooterProperty, Functions, "Count", value => value == 0 ? noFunctions : null);

            Functions.CollectionChanged += (sender, e) =>
            {
                SaveToken?.Cancel();
                SaveToken = new CancellationTokenSource();
                System.Threading.Tasks.Task.Run(SaveFunctions, SaveToken.Token);
            };

            FunctionsList.Add.Clicked += (sender, e) => EditFunctionAt();
            AddFunctionLayout.WhenPropertyChanged(IsVisibleProperty, (sender, e) =>
            {
                ChangeStatus(true);
            });

            FunctionsList.ListView.ItemTapped += (sender, e) =>
            {
                if (FunctionsList.Editing)
                {
                    return;
                }

                ChangeStatus(true);
                DropFunction((FunctionViewModel)e.Item);
            };
            listView.ContextActionClicked += (sender, e) =>
            {
                if (e.Value.Text.ToLower().Trim() == "edit")
                {
                    EditFunctionAt(Functions.IndexOf((FunctionViewModel)sender));
                }
            };

            Action<double> Transition = this.AnimationToState("Closed", "Open").GetCallback();
            Drawer.Bind<double>(HeightProperty, value =>
            {
                if (Parent == null)
                {
                    return;
                }
                
                double start = MinDrawerHeight;
                double end = MaxDrawerHeight;
                double percent = start == end ? 0 : (value - start) / Math.Abs(end - start);
                //Print.Log("height changed", Height, FunctionsList.Height, Keyboard.Height, percent);
                percent = percent.Bound(0, 1);

                Transition(percent);
                this.Parent<MainPage>()?.DrawerShadow?.SetValue(BackgroundColorProperty, test.BackgroundColor);

                if (percent != 1)
                {
                    FunctionsList.Editing = false;
                }

                if (value == start)
                {
                    SetStatus(Closed = true);
                }
                else if (value == end)
                {
                    SetStatus(Closed = false);
                }
            });

            void SetCoverHeight() => cover.HeightRequest = Content.Height - Keyboard.Height - Keyboard.Margin.VerticalThickness;
            LayoutChanged += (sender, e) => SetCoverHeight();
            Keyboard.SizeChanged += (sender, e) => SetCoverHeight();

            SetStatus(true);
        }

        private bool Closed = true;

        public void ChangeStatus(bool animated) => SetStatus(!Closed, animated);

        public void SetStatus(bool closed, bool animated = false)
        {
            if (closed)
            {
                FunctionsList.ListView.ScrollToPosition(0, 0, true);
            }

            //Drawer.HeightRequest = Heights[closed];
            //Drawer.AnimateVisualState("test", closed ? "Closed" : "Open", 16, 1000);
            Drawer.SnapTo(Heights[closed], animated ? (double?)TransitionSpeed : null);
        }

        public double MinDrawerHeight => Heights[true];
        public double MaxDrawerHeight => Heights[false];

        private Dictionary<bool, double> Heights;

        public void SetDrawerHeight(bool closed, double value)
        {
            Heights[closed] = value;

            if (Closed == closed)
            {
                SetStatus(Closed);
            }
        }

        private void EditFunctionAt(int index = -1)
        {
            IndexForFunction = index;
            AddFunctionLayout.AddEquation(index == -1 ? null : Functions[index]);
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
            ChangeStatus(true);

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
    }
}
 