using System;
using System.Collections.Generic;
using System.Text;

using System.Extensions;
using Xamarin.Forms;
using Xamarin.Forms.MathDisplay;
using MathDisplay = Xamarin.Forms.MathDisplay;
using Xamarin.Forms.Extensions;
using Crunch;

namespace Calculator
{
    public class VariableAssignment : Equation
    {
        public char Name { get; private set; }
        public Equation Value { get; private set; }
        public int DependencyCount => Dependencies.Count;

        public new Equation RHS
        {
            get { return (Equation)base.RHS; }
            set { base.RHS = value; }
        }

        private List<Equation> Dependencies;

        public VariableAssignment(char name) : base(new Expression(MathDisplay.Reader.Render(name.ToString())), new Equation(new Expression()))
        {
            Name = name;
            Value = RHS as Equation;
            Dependencies = new List<Equation>();

            /*Value.Changed += (o, n) =>
            {
                OnChanged(o, n);
            };*/

            //RHS = Value.RHS;
        }

        private bool UpdatingDependencies = false;

        private void UpdateDependencies(object sender, EventArgs e) => UpdateDependencies();

        public void UpdateDependencies()
        {
            if (UpdatingDependencies)
            {
                return;
            }

            UpdatingDependencies = true;

            foreach(Equation e in Dependencies)
            {
                e.Update(true);
            }

            UpdatingDependencies = false;
        }

        public void AddDependency(Equation e)
        {
            if (e != this && e != Value && !Dependencies.Contains(e))
            {
                Dependencies.Add(e);
            }
            else
            {
                //throw new Exception("Cannot add VariableAssignment to its own dependencies");
            }
        }

        public bool RemoveDependency(Equation e) => Dependencies.Remove(e);

        public IEnumerable<Equation> GetDependencies() => Dependencies;
    }

    public class Equation : MathLayout
    {
        public event EventHandler<ChangedEventArgs<Operand>> AnswerChanged;

        public Expression LHS { get; protected set; }
        public MathLayout RHS { get; protected set; }
        public Tuple<Expression, int> LastCursor { get; private set; }

        public override Expression InputContinuation => LHS;
        public override double MinimumHeight => System.Math.Max(LHS.MinimumHeight, RHS.MinimumHeight);

        static Equation()
        {
            SoftKeyboard.CursorMoved += (e) =>
            {
                if (e.OldValue?.Parent != e.NewValue.Parent)
                {
                    Equation root = e.OldValue?.Parent.Parent<Equation>();
                    if (root != null)
                    {
                        root.LastCursor = new Tuple<Expression, int>((Expression)e.OldValue.Parent, e.OldValue.Index);
                    }
                }
            };
        }

        public Equation() : this(new Expression()) { }

        public Equation(string lhs) : this(new Expression(MathDisplay.Reader.Render(lhs))) { }

        public Equation(Expression lhs) : this(lhs, new Answer())
        {
            LHS.Editable = true;

            LHS.DescendantAdded += Update;
            LHS.DescendantRemoved += Update;
            
            //LHS.InputChanged += () => Update();
        }

        public Equation(string lhs, string rhs) : this(new Expression(MathDisplay.Reader.Render(lhs)), new Expression(MathDisplay.Reader.Render(rhs))) { }

        public Equation(Expression lhs, MathLayout rhs) //: this()
        {
            Orientation = StackOrientation.Horizontal;
            Spacing = 0;
            VerticalOptions = LayoutOptions.Center;
            LHS = lhs;
            RHS = rhs;

            //LHS = lhs;// text == "" ? new Expression() : new Expression(Xamarin.Forms.MathDisplay.Reader.Render(text));
            Text equals = new Text(" = ") { FontSize = Text.MaxFontSize };

            Children.Add(LHS, equals, RHS);

            //Changed += (newAnswer, oldAnswer) => CheckAnswerNecessary();
            AnswerChanged += (o, n) => CheckAnswerNecessary();

            Update();
        }

        private Operand LastAnswer;
        private string LastUpdate;

        private void Update(object sender, ElementEventArgs e) => Update();

        public void Update(bool forceUpdate = false)
        {
            string text = LHS.ToString();

            if (!forceUpdate && LastUpdate == text)
            {
                return;
            }

            Print.Log("Entered: " + text);
            
            if (RHS is Answer)
            {
                Answer ans = RHS as Answer;

                Operand old = ans.answer;
                Operand updated;

                Print.Log("evaluating " + text);
                //Crunch.Math.Evaluate("((8.0)/(1))");
                //Print.Quiet = true;
                try
                {
                    updated = Crunch.Math.Evaluate(text);
                }
                catch (Exception e)
                {
                    Print.Log("error evaluating", e.Message);
                    updated = null;
                }
                Print.Log("answer is", updated);
                ans.Update(updated);
                Print.Quiet = false;

                //OnChanged(old, updated);
                OnChanged(LastAnswer, updated);
                LastAnswer = updated;
            }

            //Operand.Simplifier os = new Operand.Simplifier(Polynomials.Factored, Numbers.Exact, Trigonometry.Degrees, substitutions);
            LastUpdate = text;

            Print.Log("*************************");
        }

        protected void OnChanged(Operand oldAnswer, Operand newAnswer) => AnswerChanged?.Invoke(this, new ChangedEventArgs<Operand>(oldAnswer, newAnswer));

        protected override void OnParentSet()
        {
            base.OnParentSet();
            CheckAnswerNecessary();
        }

        private void DescendantChanged(object sender, ElementEventArgs e)
        {
            Print.Log("descendant changed", e.Element, e.Element.GetType());
            Element parent = e.Element;

            while (!(parent is IMathView))
            {
                parent = parent.Parent;

                if (parent == null || parent is Expression)
                {
                    return;
                }
            }

            Update();
        }

        private void CheckAnswerNecessary()
        {
            if (Parent is Equation)
            {
                //bool noAnswer = LHS.ToString() == RHS.ToString();
                bool noAnswer = LHS.ToString() == ((RHS as Answer)?.BetterToString() ?? RHS.ToString());

                Children[1].IsVisible = !noAnswer;
                Children[2].IsVisible = !noAnswer;
            }
        }

        /*public HashSet<char> SetAnswer() => SetAnswer(new Dictionary<char, Operand>());

        public HashSet<char> SetAnswer(Dictionary<char, Operand> substitutions)
        {
            print.log("Entered: " + LHS.ToString());

            Operand answer = Crunch.Reader.Evaluate(LHS.ToString());

            /*if (answer != null)
            {
                answer.Knowns = substitutions;
            }
            RHS.Update(answer, substitutions);

            print.log("*************************");

            return answer == null ? new HashSet<char>() : answer.Unknowns;
        }*/

        public override void Lyse() => Lyse(LHS);

        public override string ToLatex()
        {
            return LHS.ToLatex() + "=";// + RHS.ToLatex();
        }

        public override string ToString()
        {
            return LHS.ToString() + "=" + RHS.ToString();
        }
    }
}
