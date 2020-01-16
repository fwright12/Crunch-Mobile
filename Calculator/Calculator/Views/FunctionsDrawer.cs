using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;

using System.Extensions;
using Xamarin.Forms;
using Xamarin.Forms.Extensions;
using Xamarin.Forms.MathDisplay;

namespace Calculator
{
    public class FunctionsDrawer : Frame
	{
        public class ListView : EditListView { }

        public AbsoluteLayout DropArea;
        public double TransitionSpeed = 1 / 3.0;

        private readonly StackLayout AddStackLayout;
        private readonly Button ConfirmAdd;

        private readonly ListView FunctionsList;
        private readonly View Keyboard;

        private readonly AnyVisualState ClosedState;
        private readonly List<double> OpenValues;

        private Equation FunctionToAdd;
        private ObservableCollection<Expression> Functions;

        private double MaxDrawerHeight => Math.Min(0.9 * this.Parent<View>().Height, Keyboard.Height * 2);

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

            Label noVariables;
            Button cancel;

            List<string> text = new List<string>
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
            Functions = new ObservableCollection<Expression>();
            foreach (string s in text)
            {
                Functions.Add(MakeExpression(s));
            }

            Content = new StackLayout
            {
                Orientation = StackOrientation.Vertical,
                Children =
                {
                    (AddStackLayout = new StackLayout
                    {
                        Orientation = StackOrientation.Vertical,
                        IsVisible = false,
                        Children =
                        {
                            new StackLayout
                            {
                                Orientation = StackOrientation.Horizontal,
                                Children =
                                {
                                    (cancel = new Button
                                    {
                                        Text = "Cancel"
                                    }),
                                    (noVariables = new Label
                                    {
                                        HorizontalOptions = LayoutOptions.CenterAndExpand,
                                        Text = "Invalid Function!\n(functions must contain variables)",
                                        HorizontalTextAlignment = TextAlignment.Center,
                                        FontSize = NamedSize.Medium.On<Label>(),
                                    }),
                                    (ConfirmAdd = new Button
                                    {
                                        Text = "Add",
                                        IsEnabled = false
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
                        ItemsSource = Functions,
                        Header = keyboard
                    })
                }
            };

            FunctionsList.ContextAction += (sender, e) =>
            {
                if (e.Data == EditListView.ContextActions.Edit)
                {
                    AddEquation((Expression)sender);
                }
            };

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
            };
            noVariables.SetBinding(OpacityProperty, ConfirmAdd, "IsEnabled", converter: new ValueConverter<bool, int>(value => 1 - value.ToInt(), null));
            
            FunctionsList.Add.Clicked += (sender, e) => AddEquation();
            cancel.Clicked += (sender, e) =>
            {
                AddStackLayout.IsVisible = false;
                FunctionsList.SnapTo(MaxDrawerHeight, TransitionSpeed);
            };
            ConfirmAdd.Clicked += (sender, e) =>
            {
                Functions.Insert(0, MakeExpression(FunctionToAdd.LHS.ToString()));

                AddStackLayout.IsVisible = false;
                FunctionsList.SnapTo(MaxDrawerHeight, TransitionSpeed);
            };

            this.WhenPropertyChanged(CornerRadiusProperty, (sender, e) =>
            {
                AddStackLayout.Padding = (FunctionsList.Header as View).Margin = new Thickness(0, CornerRadius, 0, 0);
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

            FunctionsList.SnapTo(Keyboard);
        }

        public void ChangeStatus()
        {
            FunctionsList.Editing = false;

            if (FunctionsList.IsLocked())
            {
                FunctionsList.SnapTo(MaxDrawerHeight, TransitionSpeed);
            }
            else
            {
                FunctionsList.SnapTo(Keyboard, TransitionSpeed);
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
            if (percent == 0)
            {
                BackgroundColor = CrunchStyle.BACKGROUND_COLOR;
            }
        }

        private void AddEquation(Expression existing = null)
        {
            FunctionToAdd.LHS.Children.Clear();
            SoftKeyboard.MoveCursor(FunctionToAdd.LHS);
            if (existing != null)
            {
                SoftKeyboard.Type(existing.ToString());
            }

            ConfirmAdd.Text = existing == null ? "Add" : "Update";
            AddStackLayout.IsVisible = true;

            ChangeStatus();
        }

        private Expression MakeExpression(string text)
        {
            Expression expression = new Expression(Reader.Render(text))
            {
                FontSize = 33,
            };
            expression.Touch += DropEquation;

            return expression;
        }

        private void DropEquation(object sender, TouchEventArgs e)
        {
            if (e.State != TouchState.Moving)
            {
                return;
            }

            Expression copy = new Expression(Reader.Render((sender as Expression).ToString()));
            TouchScreen.BeginDrag(copy, this.Root<AbsoluteLayout>(), TouchScreen.LastTouch.Subtract(e.Point));
            ChangeStatus();

            TouchScreen.Dragging += (state) =>
            {
                if (state != DragState.Ended)
                {
                    return;
                }

                this.Parent<MainPage>().AddCalculation(copy.Bounds.Location.Subtract(DropArea.PositionOn(copy.Parent<VisualElement>())), TouchState.Up);
                copy.Remove();

                SoftKeyboard.Type((sender as Expression).ToString());
            };
        }
    }
}
 