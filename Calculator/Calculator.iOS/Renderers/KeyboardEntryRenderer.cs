using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using Xamarin.Forms.Extensions;
using Calculator;

[assembly: ExportRenderer(typeof(KeyboardEntry), typeof(Calculator.iOS.KeyboardEntryRenderer))]

namespace Calculator.iOS
{
    public class KeyboardEntryRenderer : EntryRenderer
    {
        private bool HiddenBySystem = false;

        public KeyboardEntryRenderer()
        {
            UIKeyboard.Notifications.ObserveWillHide((sender, e) =>
            {
                HiddenBySystem = true;
            });
            UIKeyboard.Notifications.ObserveWillShow((sender, e) =>
            {
                HiddenBySystem = false;

                if (Element is KeyboardEntry keyboard && !keyboard.Showing)
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

                (Element as KeyboardEntry)?.OnOnscreenSizeChanged(new Size(rect.Width, rect.Height));
            });
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
                //Print.Log("should end editing", Hidden, HiddenBySystem);
                return Element is KeyboardEntry keyboard && (!keyboard.Showing || HiddenBySystem);
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
        private void LeftArrow() => MoveCursor(MathFieldViewModel.CursorKey.Left);

        [Export("RightArrow")]
        private void RightArrow() => MoveCursor(MathFieldViewModel.CursorKey.Right);

        [Export("UpArrow")]
        private void UpArrow() => MoveCursor(MathFieldViewModel.CursorKey.Up);

        [Export("DownArrow")]
        private void DownArrow() => MoveCursor(MathFieldViewModel.CursorKey.Down);

        private void MoveCursor(MathFieldViewModel.CursorKey key) => (Element as KeyboardEntry)?.ViewModel.MoveCursorCommand?.Execute(key);
    }
}