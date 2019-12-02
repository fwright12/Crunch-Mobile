using System;
using System.Collections.Generic;
using System.Text;

namespace Calculator
{
    public delegate void KeystrokeEventHandler(IEnumerable<char> keystrokes);
    public delegate void OutputEventHandler(char keystroke);
    public delegate void SpecialKeyEventHandler<T>(T key);

    public interface IKeyboard
    {
        KeystrokeEventHandler Typed { get; set; }

        void Enable();
        void Disable();
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
        public static event System.Extensions.StaticEventHandler<IKeyboard> KeyboardChanged;
        public static IKeyboard Current { get; private set; }

        private static List<IKeyboard> Keyboards = new List<IKeyboard>();


        public static IEnumerable<IKeyboard> Connected() => Keyboards;
        public static void ClearKeyboards()
        {
            SwitchTo(null);
            Keyboards = new List<IKeyboard>();
        }

        //public static void AddSource(KeystrokeEventHandler eventHandler) => eventHandler += (keystroke) => Type(keystroke);

        public static void Type(params char[] keystrokes) => Type(keystrokes as IEnumerable<char>);

        public static void Type(IEnumerable<char> keystrokes)
        {
            foreach (char c in keystrokes)
            {
                Typed?.Invoke(c);
            }
        }

        public static void MoveCursor(CursorKey key) => CursorMoved?.Invoke(key);

        public static void AddKeyboard(params IKeyboard[] keyboards) => Keyboards.AddRange(keyboards);

        public static void SwitchTo(int index) => SwitchTo(Keyboards[index]);

        public static void SwitchTo(IKeyboard keyboard)
        {
            if (keyboard == Current)
            {
                return;
            }

            if (Current != null)
            {
                Current.Disable();
                Current.Typed -= Type;
            }

            Current = keyboard;

            if (Current != null)
            {
                Current.Enable();
                Current.Typed += Type;
            }

            KeyboardChanged?.Invoke(Current);
        }

        public static void NextKeyboard() => SelectRelative(1);
        public static void PrevKeyboard() => SelectRelative(-1);

        private static void SelectRelative(int diff) => SwitchTo((Keyboards.IndexOf(Current) + diff) % Keyboards.Count);
    }
}
