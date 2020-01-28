using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

using Xamarin.Forms;
using Xamarin.Forms.Extensions;

namespace Calculator.iOS
{
    public static class Gestures
    {
        public static void Test(View shared, UIView native)
        {
            var test = (ObservableCollection<IGestureRecognizer>)shared.GetValue(Xamarin.Forms.Extensions.Gestures.GestureRecognizersProperty);

            test.CollectionChanged += (sender, e) =>
            {
                foreach(IGestureRecognizer gestureRecognizer in e.OldItems)
                {

                }

                foreach (IGestureRecognizer gestureRecognizer in e.NewItems)
                {
                    
                }
            };
        }

        private static void AddLongClick(this UIView native, LongClickGestureRecognizer longClickGestureRecognizer)
        {
            UILongPressGestureRecognizer longPress = new UILongPressGestureRecognizer();
            longPress.AddTarget(() => longClickGestureRecognizer.Invoke(null, new TouchEventArgs(longPress.LocationInView(native).Convert(), longPress.State.Convert())));
            native.AddGestureRecognizer(longPress);
        }

        private static void AddGestureRecognizer<T>(this UIView native, Xamarin.Forms.Extensions.GestureRecognizer gestureRecognizer)
            where T : UIGestureRecognizer, new()
        {
            T t = new T();
            t.AddTarget(() => gestureRecognizer.Invoke(null, new TouchEventArgs(t.LocationInView(native).Convert(), t.State.Convert())));
            native.AddGestureRecognizer(t);
        }
    }
}