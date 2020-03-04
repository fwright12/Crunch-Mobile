using System;
using System.Collections.Generic;
using System.Text;

using System.Extensions;

namespace Calculator
{
    public delegate void KeystrokeEventHandler(IEnumerable<char> keystrokes);
    public delegate void OutputEventHandler(char keystroke);
    public delegate void SpecialKeyEventHandler<T>(T key);

    public interface IKeyboard
    {
        event KeystrokeEventHandler Typed;
    }

    public static class KeyboardManager
    {
        public enum CursorKey { Left, Right, Up, Down, End, Home };

        public static readonly char BACKSPACE = (char)8;
        public static readonly char ENTER = (char)13;
        public static readonly char ESCAPE = (char)27;
        public static readonly char DELETE = (char)127;

        public static event OutputEventHandler Typed;
        public static event SpecialKeyEventHandler<CursorKey> CursorMoved;

        public static void Type(params char[] keystrokes) => Type(keystrokes as IEnumerable<char>);

        public static void Type(IEnumerable<char> keystrokes)
        {
            foreach (char c in keystrokes)
            {
                Typed?.Invoke(c);
            }
        }

        public static void MoveCursor(CursorKey key) => CursorMoved?.Invoke(key);
    }
}
