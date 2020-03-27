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
            
            if (answer == null)
            {
                Text = LastAnswer ?? "Error";
            }
            else
            {
                Text += keystroke;
            }

            LastAnswer = answer;
        }
    }
}
