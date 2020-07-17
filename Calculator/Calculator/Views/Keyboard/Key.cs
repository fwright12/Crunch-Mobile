using System;
using System.Collections.Generic;
using System.Text;

using Xamarin.Forms;
using Xamarin.Forms.Extensions;

namespace Calculator
{
    public delegate void KeyPressEventHandler(string output);

    public class Key : LongClickableButton
    {
        //public static readonly string DELETE = "delete";
        //public static readonly string DOCK = "dock";
        public static int RepeatedKeyPressSpeed = 100;

        // Steal clicked listeners so we have control over when they fire
        new public event EventHandler Clicked
        {
            add => _Clicked += value;
            remove => _Clicked -= value;
        }
        private event EventHandler _Clicked;

        new public event EventHandler<EventArgs> LongClick
        {
            add
            {
                if (LongClickListenerCount++ == 0)
                {
                    base.LongClick -= OnLongClick;
                }
                base.LongClick += value;
            }
            remove
            {
                base.LongClick -= value;
                if (--LongClickListenerCount == 0)
                {
                    base.LongClick += OnLongClick;
                }
            }
        }
        private int LongClickListenerCount = 0;

        public Key()
        {
            Padding = new Thickness(0);

            base.Clicked += (sender, e) => _Clicked?.Invoke(sender, e);
            base.LongClick += OnLongClick;
        }

        public string Output => CommandParameter as string ?? Text;// Basic == "" ? Text : Basic;

        public static implicit operator Key(string s) => new Key { Text = s };

        private async void OnLongClick(object sender, EventArgs e)
        {
            while (IsPressed)
            {
                _Clicked?.Invoke(sender, e);
                await System.Threading.Tasks.Task.Delay(RepeatedKeyPressSpeed);
            }
        }
    }
}
