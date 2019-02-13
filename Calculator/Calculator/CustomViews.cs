using System;
using System.Collections.Generic;
using System.Text;

namespace Xamarin.Forms
{
    public delegate void TouchEventHandler(Point point, TouchState state);
    public delegate void LongClickEventHandler();

    public enum TouchState { Down, Up, Moving };

    public interface ITouchable { void OnTouch(Point point, TouchState state); }

    public class TouchScreen : StackLayout, ITouchable
    {
        public static Point LastDownEvent;

        public event TouchEventHandler Touch;
        public void OnTouch(Point point, TouchState state)
        {
            if (state == TouchState.Up)
            {
                Drag.End();
            }

            Touch?.Invoke(point, state);
        }
    }

    public class Canvas : AbsoluteLayout, ITouchable
    {
        public event TouchEventHandler Touch;
        public void OnTouch(Point point, TouchState state) => Touch?.Invoke(point, state);
    }

    public class LongClickableButton : Button
    {
        public event LongClickEventHandler LongClick;
        public void OnLongClick() => LongClick?.Invoke();
    }

    public class DockButton : Button, ITouchable
    {
        public event TouchEventHandler Touch;
        public void OnTouch(Point point, TouchState state) => Touch?.Invoke(point, state);
    }

    public class BannerAd : View { }
}
