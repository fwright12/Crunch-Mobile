using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Extensions;
using Xamarin.Forms;
using Xamarin.Forms.Extensions;
using Xamarin.Forms.MathDisplay;
using Crunch;

namespace Calculator
{
    public class Calculation : TouchableStackLayout
    {
        //public Equation Main { get; private set; }
        public bool RecognizeVariables = false;

        //private Dictionary<char, Equation> Substitutions = new Dictionary<char, Equation>();
        private Dictionary<char, Operand> Variables;
        //private List<Tuple<Expression, int>> CursorPositions;
        //private Equation Selected;

        static Calculation()
        {
            /*SoftKeyboard.CursorMoved += (e) =>
            {
                View last = e.OldValue.Parent;
                while (!(last.Parent is Calculation))
                {
                    last = last.Parent as View;

                    if (last == null)
                    {
                        return;
                    }
                }

                //(last.Parent as Calculation).CursorPositions[last.Index()] = new Tuple<Expression, int>((Expression)e.OldValue.Parent, e.OldValue.Index);
            };*/
            /*MainPage.EquationChanged += (e) =>
            {
                //e.NewValue.Parent<Calculation>()?.Focus(e.NewValue);
            };*/
        }

        public Calculation()//Equation main)
        {
            Orientation = StackOrientation.Vertical;
            Spacing = 0;
            Variables = new Dictionary<char, Operand>();
            //CursorPositions = new List<Tuple<Expression, int>>();

            //Children.Add(Main = main);
            //Add(main);

            //Main.LHS.InputChanged += () => SetAnswer();
            
            //(Main.Children[1] as Text)
        }

        public void Add(Equation e) => Insert(Children.Count, e);
        
        public void Insert(int index, Equation equation)
        {
            Children.Insert(index, equation);
            equation.HorizontalOptions = LayoutOptions.Start;

            if (equation is VariableAssignment)
            {
                equation = (equation as VariableAssignment).Value;
            }

            if (equation.RHS is Answer)
            {
                (equation.RHS as Answer).Knowns = Variables;
            }

            equation.AnswerChanged += (sender, e) =>
            {
                if (e.OldValue != null)// && (equation.LHS.ChildCount() == 0 || e.NewValue != null))
                {
                    foreach (char c in e.OldValue.Unknowns)
                    {
                        //Was removed when the answer changed
                        if (e.NewValue == null || !e.NewValue.Unknowns.Contains(c))
                        {
                            VariableAssignment va;
                            if (HasVariableAssignment(c, out va))
                            {
                                //va.Dependencies.Remove(e);
                                if (va.RemoveDependency(equation))
                                {
                                    va.IsVisible = va.DependencyCount > 0;
                                }
                            }
                        }
                    }
                }

                if (e.NewValue != null)
                {
                    foreach (char c in e.NewValue.Unknowns)
                    {
                        //Was added when the answer changed
                        if (e.OldValue == null || !e.OldValue.Unknowns.Contains(c))
                        {
                            VariableAssignment va;
                            if (HasVariableAssignment(c, out va))
                            {
                                va.IsVisible = true;
                            }
                            else
                            {
                                va = AddVariableAssignment(c);
                            }

                            Print.Log("alkjsdfl;kajs;dkf", e.OldValue?.ToString() ?? "Null", e.NewValue, e);
                            va.AddDependency(equation);
                            /*if (!va.Dependencies.Contains(e))
                            {
                                va.Dependencies.Add(e);
                                //va.Update(Variables);
                            }*/
                        }
                    }
                }

                if (equation.Parent is VariableAssignment)
                {
                    VariableAssignment va = equation.Parent as VariableAssignment;

                    Variables[va.Name] = e.NewValue;
                    va.UpdateDependencies();
                }
            };

            //CursorPositions.Insert(index, new Tuple<Expression, int>(equation.LHS, 0));
        }

        protected override void OnChildRemoved(Element child)
        {
            base.OnChildRemoved(child);

            if (Children.Count == 0)
            {
                this.Remove();
            }
        }

        public void Up() => Move(-1);
        public void Down() => Move(1);

        private void Move(int direction)
        {
            View last = SoftKeyboard.Cursor;
            while (last.Parent != this)
            {
                last = last.Parent as View;

                if (last == null)
                {
#if DEBUG
                    throw new Exception("This calculation does not have focus");
#else
                    return;
#endif
                }
            }
            int selected = Children.IndexOf(last);

            do
            {
                if (selected + direction < 0 || selected + direction == Children.Count)
                {
                    Insert(selected + (direction + 1) / 2, new Equation());
                    selected -= (direction - 1) / 2;
                }

                selected += direction;
            }
            while (!Children[selected].IsVisible);

            View view = Children[selected];
            Focus((view as VariableAssignment)?.RHS ?? (Equation)view);
        }

        private void Focus(Equation equation)
        {
            if (equation.LastCursor == null)
            {
                SoftKeyboard.MoveCursor(equation.LHS, 0);
            }
            else
            {
                SoftKeyboard.MoveCursor(equation.LastCursor.Item1, equation.LastCursor.Item2);
            }
        }

        private VariableAssignment AddVariableAssignment(char name)
        {
            VariableAssignment va = new VariableAssignment(name);
            /*va.Value.Changed += (oldAnswer, newAnswer) =>
            {
                Variables[name] = newAnswer;
                va.UpdateDependecies();
            };*/

            Add(va);
            return va;
        }

        private bool HasVariableAssignment(char c, out VariableAssignment va)
        {
            foreach(Equation e in Children)
            {
                if (e is VariableAssignment && (e as VariableAssignment).Name == c)
                {
                    va = e as VariableAssignment;
                    return true;
                }
            }

            va = null;
            return false;
        }
    }
}
