using Xamarin.Forms.Platform.Android;
using Xamarin.Forms;
using Calculator;
using Calculator.Droid;
using Android.Views;
using Android.Views.InputMethods;
using System;

[assembly: ExportRenderer(typeof(GestureRelativeLayout), typeof(AndroidRelativeLayoutRenderer))]
[assembly: ExportRenderer(typeof(SoftKeyboardDisabledEntry), typeof(SoftkeyboardDisabledEntryRenderer))]
[assembly: ExportRenderer(typeof(LiveButton), typeof(LiveButtonRenderer))]

namespace Calculator.Droid
{
    public class LiveButtonRenderer : ButtonRenderer
    {
        private Point startPosition;
        private DateTime startTime;
        private static int minDistance = 100;

        protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                Control.LongClick += (sender, args) => Input.LongClickDown(true);
                Control.Touch += (sender, args) =>
                {
                    switch (args.Event.Action)
                    {
                        case MotionEventActions.Down:
                            startPosition = new Point(args.Event.GetX(), args.Event.GetY());
                            startTime = DateTime.Now;

                            break;
                        case MotionEventActions.Up:
                            Input.LongClickDown(false);

                            Point endPosition = new Point(args.Event.GetX(), args.Event.GetY());

                            if ((DateTime.Now - startTime).TotalMilliseconds < 500)
                            {
                                //Check for swipe to dismiss keyboard
                                if (Math.Abs(endPosition.y - startPosition.y) > minDistance && endPosition.y > startPosition.y)
                                {
                                    print.log("swipe down");
                                }
                                else if (Math.Abs(endPosition.x - startPosition.x) > minDistance)
                                {
                                    if (endPosition.x < startPosition.x)
                                    {
                                        print.log("swipe left");
                                        Input.KeyboardSwipe(-1);
                                    }
                                    else if (endPosition.x > startPosition.x)
                                    {
                                        print.log("swipe right");
                                        Input.KeyboardSwipe(1);
                                    }
                                }
                            }

                            break;
                        case MotionEventActions.Move:
                            Input.CursorMoved(new Point(args.Event.RawX, args.Event.RawY));

                            break;
                    }

                    args.Handled = false;
                };
            }
        }
    }

    public class AndroidRelativeLayoutRenderer : VisualElementRenderer<Xamarin.Forms.RelativeLayout>
    {
        public AndroidRelativeLayoutRenderer()
        {
            Touch += (sender, e) =>
            {
                if (e.Event.Action == MotionEventActions.Up)
                {
                    print.log(e.Event.GetX(), e.Event.GetY(), e.Event.RawX, e.Event.RawY);
                    Input.CanvasTouch(new Point(e.Event.GetX() / (sender as Android.Views.View).Width, e.Event.GetY() / (sender as Android.Views.View).Height));
                    //Input.CanvasTouch(new Point(e.Event.GetX(), e.Event.GetY()));
                }
            };
        }
    }

    public class SoftkeyboardDisabledEntryRenderer : EntryRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement != null)
            {
                ((SoftKeyboardDisabledEntry)e.NewElement).PropertyChanging += OnPropertyChanging;
            }

            if (e.OldElement != null)
            {
                ((SoftKeyboardDisabledEntry)e.OldElement).PropertyChanging -= OnPropertyChanging;
            }

            if (Control != null)
            {
                //Control.SetPadding(1, 0, 0, 0);
                //Control.Background = null;
                //Control.SetBackgroundColor(Android.Graphics.Color.Transparent);
                //Control.SetCursorVisible(true);
            }

            // Disable the Keyboard on Focus
            Control.ShowSoftInputOnFocus = false;
        }

        private void OnPropertyChanging(object sender, PropertyChangingEventArgs propertyChangingEventArgs)
        {
            // Check if the view is about to get Focus
            if (propertyChangingEventArgs.PropertyName == VisualElement.IsFocusedProperty.PropertyName)
            {
                // in case if the focus was moved from another Entry
                // Forcefully dismiss the Keyboard 
                //InputMethodManager imm = (InputMethodManager)this.Context.GetSystemService(Android.Content.Context.InputMethodService);
                //imm.HideSoftInputFromWindow(this.Control.WindowToken, 0);
            }
        }
    }
}