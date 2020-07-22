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

[assembly: ExportRenderer(typeof(Calculator.KeyboardEntry), typeof(Calculator.Droid.KeyboardEntryRenderer))]

namespace Calculator.Droid
{
    public class KeyboardEntryRenderer : EntryRenderer
    {
        private bool ImplicitDismissal = false;

        public KeyboardEntryRenderer(Context context) : base(context)
        {
            Xamarin.Forms.Application.Current.MainPage.SizeChanged += (sender, e) => (Element as KeyboardEntry)?.OnOnscreenSizeChanged(new Size(Xamarin.Forms.Application.Current.MainPage.Width, 0));
        }

        public override void ClearChildFocus(Android.Views.View child)
        {
            Print.Log("clearing child focus");
            base.ClearChildFocus(child);

            if (!ImplicitDismissal && Element is KeyboardEntry keyboard)
            {
                keyboard.DismissedBySystem();
            }
        }

        public override bool IsFocused => Element is KeyboardEntry keyboard && keyboard.Showing;

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

            if (e.Focus && Element is KeyboardEntry keyboard && keyboard.Showing)
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
            KeyboardEntry view = Element as KeyboardEntry;

            if (keyCode == Keycode.DpadUp)
            {
                view.ViewModel.MoveCursorCommand?.Execute(MathFieldViewModel.CursorKey.Up);
            }
            else if (keyCode == Keycode.DpadDown)
            {
                view.ViewModel.MoveCursorCommand?.Execute(MathFieldViewModel.CursorKey.Down);
            }
            else if (keyCode == Keycode.DpadLeft)
            {
                view.ViewModel.MoveCursorCommand?.Execute(MathFieldViewModel.CursorKey.Left);
            }
            else if (keyCode == Keycode.DpadRight)
            {
                view.ViewModel.MoveCursorCommand?.Execute(MathFieldViewModel.CursorKey.Right);
            }

            return base.OnKeyDown(keyCode, e);
        }
    }
}