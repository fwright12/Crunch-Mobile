using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Views.InputMethods;

using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(Calculator.SystemKeyboard.KeyboardEntry), typeof(Calculator.Droid.KeyboardEntryRenderer))]

namespace Calculator.Droid
{
    public class KeyboardEntryRenderer : EntryRenderer
    {
        private bool ImplicitDismissal = false;

        public KeyboardEntryRenderer(Context context) : base(context) { }

        public override void ClearChildFocus(Android.Views.View child)
        {
            Print.Log("clearing child focus");
            base.ClearChildFocus(child);

            if (!ImplicitDismissal && Element is SystemKeyboard.KeyboardEntry keyboard)
            {
                keyboard.DismissedBySystem();
            }
        }

        public override bool IsFocused => Element is SystemKeyboard.KeyboardEntry keyboard && keyboard.Showing;

        public override void ClearFocus()
        {
            Print.Log("clearing focus");
            ImplicitDismissal = true;
            base.ClearFocus();
            ImplicitDismissal = false;
        }

        protected override void OnFocusChangeRequested(object sender, VisualElement.FocusRequestArgs e)
        {
            base.OnFocusChangeRequested(sender, e);
            Print.Log("focus change requested", e.Focus, e.Result);

            if (e.Focus && Element is SystemKeyboard.KeyboardEntry keyboard && keyboard.Showing)
            {
                RequestFocus();

                InputMethodManager inputMethodManager = Context.GetSystemService(Android.Content.Context.InputMethodService) as InputMethodManager;
                inputMethodManager.ShowSoftInput(Control, ShowFlags.Forced);
                inputMethodManager.ToggleSoftInput(ShowFlags.Forced, HideSoftInputFlags.ImplicitOnly);
            }
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            if (Control == null)
            {
                return;
            }

            Control.ImeOptions = (ImeAction)ImeFlags.NoExtractUi;
        }

        public override bool OnKeyDown([GeneratedEnum] Keycode keyCode, KeyEvent e)
        {
            if (keyCode == Keycode.DpadUp)
            {
                KeyboardManager.MoveCursor(KeyboardManager.CursorKey.Up);
            }
            else if (keyCode == Keycode.DpadDown)
            {
                KeyboardManager.MoveCursor(KeyboardManager.CursorKey.Down);
            }
            else if (keyCode == Keycode.DpadLeft)
            {
                KeyboardManager.MoveCursor(KeyboardManager.CursorKey.Left);
            }
            else if (keyCode == Keycode.DpadRight)
            {
                KeyboardManager.MoveCursor(KeyboardManager.CursorKey.Right);
            }

            return base.OnKeyDown(keyCode, e);
        }
    }
}