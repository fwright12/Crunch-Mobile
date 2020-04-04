using System;
using System.Collections.Generic;
using System.Text;

using Xamarin.Forms;

namespace Calculator
{
    public class BasicModeAnswerLabel : Label
    {
        private string LastAnswer;

        public void Update(string answer, char keystroke)
        {
            Print.Log("typed " + keystroke.ToString(), "answer is " + answer);

            if (LastAnswer == null)
            {
                Text = "";
            }
            
            //if (answer == null)
            if (keystroke == '+' || keystroke == '-' || keystroke == '*' || keystroke == '/')
            {
                Text = LastAnswer;// ?? "Error";
            }
            else if (keystroke == '=')
            {
                Text = answer ?? "Error";
            }
            else if (keystroke == KeyboardManager.BACKSPACE)
            {
                Text = Text.Length <= 1 ? "0" : Text.Substring(0, Text.Length - 1);
            }
            else
            {
                Text += keystroke;
            }

            LastAnswer = answer;
        }

        public void Clear()
        {
            Text = "0";
            LastAnswer = null;
        }
    }
}
