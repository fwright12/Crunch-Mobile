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
            MainPage.EquationChanged += (e) =>
            {
                //e.NewValue.Parent<Calculation>()?.Focus(e.NewValue);
            };
        }

        public Calculation(Equation main)
        {
            Orientation = StackOrientation.Vertical;
            Spacing = 0;
            Variables = new Dictionary<char, Operand>();
            //CursorPositions = new List<Tuple<Expression, int>>();

            //Children.Add(Main = main);
            Add(main);

            //Main.LHS.InputChanged += () => SetAnswer();
            
            //(Main.Children[1] as Text)
            Touch += (point, state) =>
            {
                if (state == TouchState.Moving)
                {
                    double backup = TouchScreen.FatFinger;
                    TouchScreen.FatFinger = 0;
                    TouchScreen.BeginDrag(this, MainPage.VisiblePage.Canvas);
                    TouchScreen.FatFinger = backup;
                }
            };
        }

        public void Add(Equation e) => Insert(Children.Count, e);
        
        public void Insert(int index, Equation equation)
        {
            Children.Insert(index, equation);
            
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
                if (e.NewValue != null)
                {
                    //First answer - make sure its using the subsitutions
                    if (e.OldValue == null)
                    {
                        //e.Update(Variables);
                    }
                    else
                    {
                        foreach (char c in e.OldValue.Unknowns)
                        {
                            //Was removed when the answer changed
                            if (!e.NewValue.Unknowns.Contains(c))
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
                    va.UpdateDependecies();
                }
            };

            //CursorPositions.Insert(index, new Tuple<Expression, int>(equation.LHS, 0));
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

            if (selected + direction < 0 || selected + direction == Children.Count)
            {
                Insert(selected + (direction + 1) / 2, new Equation());
                selected -= (direction - 1) / 2;
            }

            View view = Children[selected + direction];
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

        /*public void SetAnswer()
        {
            Dictionary<char, Operand> substitutions = new Dictionary<char, Operand>();
            
            //Evaluate everything that might need to be substituted
            foreach (KeyValuePair<char, Equation> pair in Substitutions)
            {
                string s = pair.Value.LHS.ToString();

                if (s != "")
                {
                    Operand temp = Crunch.Reader.Evaluate(s);
                    if (temp != null && !s.Contains(pair.Key))
                    {
                        substitutions.Add(pair.Key, temp);
                    }
                }
            }

            Main.Update(substitutions);
            Dictionary<char, Operand> unknowns = Main.RHS.answer.Unknowns;

            if (RecognizeVariables)
            {
                foreach (char c in Substitutions.Keys)
                {
                    if (!unknowns.ContainsKey(c))
                    {
                        (Substitutions[c].Parent as View).IsVisible = false;
                    }
                }

                foreach (char c in unknowns.Keys)
                {
                    if (!Substitutions.ContainsKey(c))
                    {
                        Substitutions.Add(c, AddUnknown(c));
                    }
                    else
                    {
                        (Substitutions[c].Parent as View).IsVisible = true;
                    }
                }
            }

            print.log("*************************");
        }*/

        /*private Equation AddUnknown(char c)
        {
            Equation e = new Equation();
            e.Changed += () =>
            {

            };

            Substitutions.Add(c, e);

            //e.Children[1].IsVisible = false;
            //e.Children[2].IsVisible = false;

            //Expression e = new Expression() { Editable = true };

            Children.Add(new Expression(new Text(c + " = "), e));// { HorizontalOptions = LayoutOptions.Start });

            return e;

            //Expression e = new Expression(new Text(c + " = "), new Expression() { Editable = true });
            //e.HorizontalOptions = LayoutOptions.Start;
            //Children.Add(e);
            //return e.Children[1] as Expression;
        }*/

        /*private void Dependencies()
        {
            List<char>[] dependencies = new List<char>[Children.Count];
            Dictionary<char, Operand> lookup = new Dictionary<char, Operand>();

            for (int i = 0; i < Children.Count; i++)
            {
                Equation e = Children[i] as Equation;

                if (e == null)
                {
                    continue;
                }

                string s = e.LHS.ToString();

                char variable = s[0];

                //Remove any unknown variables that we resolved on the last pass
                if (dependencies[i] != null)
                {
                    for (int j = 0; j < dependencies[i].Count; j++)
                    {
                        if (lookup.ContainsKey(dependencies[i][j]))
                        {
                            dependencies[i].RemoveAt(j--);
                        }
                    }
                }

                //If this equation is not dependent on any unknowns, we can evaluate it
                //If dependencies[i] is null, we haven't tried to evaluate it, so we need to
                if ((dependencies[i] = dependencies[i] ?? new List<char>()).Count == 0)
                {
                    e.Update(lookup);

                    foreach (char c in e.RHS.answer.Unknowns.Keys)
                    {
                        dependencies[i].Add(c);
                    }

                    if (dependencies[i].Count == 0)
                    {
                        lookup.Add(variable, e.RHS.answer);
                    }
                }
            }
        }*/
    }
}
