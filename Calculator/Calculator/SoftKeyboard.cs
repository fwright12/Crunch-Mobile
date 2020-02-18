using System;
using System.Collections.Generic;
using System.Text;

using Xamarin.Forms;

namespace Crunch.Mobile
{
    public interface ISoftKeyboard
    {
        Size MeasureOnscreenSize();// double widthConstraint, double heightConstraint);
    }

    public static class SoftKeyboard
    {
        public static EventHandler<EventArgs<Size>> SizeChanged;

        static SoftKeyboard()
        {
            Calculator.KeyboardManager.KeyboardChanged += (sender) =>
            {
                SizeChanged?.Invoke(sender, new EventArgs<Size>((sender as ISoftKeyboard)?.MeasureOnscreenSize() ?? Size.Zero));
            };
        }

        public static void OnSizeChanged(object sender, EventArgs<Size> e)
        {
            if (sender == Calculator.KeyboardManager.Current)
            {
                SizeChanged?.Invoke(sender, e);
            }
        }
    }
}
