using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

using Xamarin.Forms;
using Xamarin.Forms.Extensions;

namespace Calculator
{
    public static class SystemKeyboard
    {
        public static ISoftKeyboard Instance => RawInstance;
        //public static bool IsShowing => KeyboardManager.Current == Instance;

        private static KeyboardEntry RawInstance = null;

        public static void Setup(AbsoluteLayout layout)
        {
            if (RawInstance != null)
            {
                return;
            }

            layout.Children.Add(RawInstance = new KeyboardEntry(), new Point(-1000, -1000));
        }

        public class KeyboardEntry : Entry, ISoftKeyboard
        {
            public event KeystrokeEventHandler Typed;
            public event EventHandler OnscreenSizeChanged;
            public Size Size { get; private set; }

            public bool Showing = false;

            public KeyboardEntry()
            {
                //Keyboard = Keyboard.Plain;
                
                TextChanged += (sender, e) =>
                {
                    if (Text == "  ")
                    {
                        return;
                    }
                    
                    Typed?.Invoke(Text.Length < 2 ? KeyboardManager.BACKSPACE.ToString() : Text.Trim());
                    Reset();
                };
            }

            public void Disable()
            {
                Showing = false;
                Unfocus();
            }

            public void Enable()
            {
                Showing = true;
                Focus();
                Reset();
            }

            public void DismissedBySystem()
            {
                SoftKeyboardManager.NextKeyboard();
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
}
