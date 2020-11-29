using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Extensions;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.MathDisplay;

namespace Calculator
{
    public class KeyboardEntry : Entry, ISoftKeyboard
    {
        //public event KeystrokeEventHandler Typed;
        public event EventHandler OnscreenSizeChanged;
        public Size Size { get; private set; }

        public bool Showing = false;

        public static KeyboardEntry Instance;

        public MathEntryViewModel ViewModel => BindingContext as MathEntryViewModel;

        public KeyboardEntry()
        {
            Instance = this;
            //Keyboard = Keyboard.Plain;
            
            TextChanged += (sender, e) =>
            {
                if (Text == "  ")
                {
                    return;
                }

                if (Text.Length < 2)
                {
                    ViewModel?.BackspaceCommand?.Execute(null);
                }
                else
                {
                    ViewModel?.InputCommand?.Execute(Text.Trim());
                }
                //string text =  ? KeyboardManager.BACKSPACE.ToString() : ;
                //Typed?.Invoke(text);
                Reset();
            };
        }

        public void Disable(bool animated = false)
        {
            if (!Showing)
            {
                return;
            }

            Showing = false;
            Unfocus();
        }

        public void Enable(bool animated = false)
        {
            if (Showing)
            {
                return;
            }

            Showing = true;
            Focus();
            Reset();
        }

        public void DismissedBySystem()
        {
            if (!Showing)
            {
                return;
            }

            SoftKeyboardManager.OnDismissed();
            //SoftKeyboardManager.NextKeyboard();
        }

        public void OnOnscreenSizeChanged(Size size)
        {
            Size = size;
            OnscreenSizeChanged?.Invoke(this, new EventArgs());
        }

        protected override void OnParentSet()
        {
            if (this.Parent<Page>() is Page oldParent)
            {
                oldParent.Appearing -= Unhide;
            }

            base.OnParentSet();

            if (this.Parent<Page>() is Page newParent)
            {
                newParent.Appearing += Unhide;
            }
        }

        private void Reset()
        {
            Text = "  ";
            CursorPosition = 1;
        }

        private void Unhide(object sender, EventArgs e)
        {
            if (Showing)
            {
                Focus();
            }
        }
    }
}
