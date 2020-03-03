using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using Xamarin.Forms.Extensions;

[assembly: ExportRenderer(typeof(Calculator.SystemKeyboard.KeyboardEntry), typeof(Calculator.iOS.KeyboardEntryRenderer))]

namespace Calculator.iOS
{
    public class KeyboardEntryRenderer : EntryRenderer
    {
        private bool HiddenBySystem = false;

        public KeyboardEntryRenderer()
        {
            UIKeyboard.Notifications.ObserveWillHide((sender, e) =>
            {
                Print.Log("keyboard will hide");
                HiddenBySystem = true;
                //AdjustResize(0);
            });
            UIKeyboard.Notifications.ObserveWillShow((sender, e) =>
            {
                HiddenBySystem = false;

                if (Element is SystemKeyboard.KeyboardEntry keyboard && !keyboard.Showing)
                {
                    Control.EndEditing(true);
                }
            });
            UIKeyboard.Notifications.ObserveDidChangeFrame((sender, e) =>
            {
                CoreGraphics.CGRect rect = UIKeyboard.FrameEndFromNotification(e.Notification);
                //Print.Log("keyboard size changed", rect);
                if (rect.Width != Xamarin.Forms.Application.Current.MainPage.Width)
                {
                    rect = new CoreGraphics.CGRect(0, 0, Xamarin.Forms.Application.Current.MainPage.Width, 0);
                }
                (Element as SystemKeyboard.KeyboardEntry)?.OnOnscreenSizeChanged(new Size(rect.Width, rect.Height));
            });
            UIKeyboard.Notifications.ObserveDidShow((sender, e) =>
            {
                //AdjustResize(UIKeyboard.FrameEndFromNotification(e.Notification).Height);
            });
        }

        private void AdjustResize(double newKeyboardHeight)
        {
            Layout<View> Parent = Element.Parent<Layout<View>>();
            Thickness margin = Parent.Margin;
            margin.Bottom = newKeyboardHeight;
            Parent.Margin = margin;

            return;
            AbsoluteLayout layout = Element.Parent<AbsoluteLayout>();
            layout.HeightRequest = layout.Height - newKeyboardHeight;
            //AbsoluteLayout.SetLayoutBounds(Placeholder, new Rectangle(0, 1, -1, newKeyboardHeight));

            return;
            Page root = Element.Parent<Page>();
            Thickness padding = root.Padding;
            Print.Log(root, padding.Bottom, padding.Top);
            //padding.Bottom -= LastKeyboardHeight;
            //padding.Bottom += LastKeyboardHeight = newKeyboardHeight;
            //padding.Bottom += newKeyboardHeight;

            root.Padding = padding;
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            if (Control == null)
            {
                return;
            }

            Control.ShouldEndEditing = (sender) =>
            {
                Print.Log("should end editing", Hidden, HiddenBySystem);
                return Element is SystemKeyboard.KeyboardEntry keyboard && (!keyboard.Showing || HiddenBySystem);
            };
        }

        public override UIKeyCommand[] KeyCommands => new UIKeyCommand[]
        {
            UIKeyCommand.Create(UIKeyCommand.LeftArrow, 0, new ObjCRuntime.Selector("LeftArrow")),
            UIKeyCommand.Create(UIKeyCommand.RightArrow, 0, new ObjCRuntime.Selector("RightArrow")),
            UIKeyCommand.Create(UIKeyCommand.UpArrow, 0, new ObjCRuntime.Selector("UpArrow")),
            UIKeyCommand.Create(UIKeyCommand.DownArrow, 0, new ObjCRuntime.Selector("DownArrow"))
        };

        [Export("LeftArrow")]
        private void LeftArrow() => KeyboardManager.MoveCursor(KeyboardManager.CursorKey.Left);

        [Export("RightArrow")]
        private void RightArrow() => KeyboardManager.MoveCursor(KeyboardManager.CursorKey.Right);

        [Export("UpArrow")]
        private void UpArrow() => KeyboardManager.MoveCursor(KeyboardManager.CursorKey.Up);

        [Export("DownArrow")]
        private void DownArrow() => KeyboardManager.MoveCursor(KeyboardManager.CursorKey.Down);
    }
}