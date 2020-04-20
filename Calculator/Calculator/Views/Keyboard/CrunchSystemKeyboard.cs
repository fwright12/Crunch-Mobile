using System;
using System.Collections.Generic;
using System.Text;

using Xamarin.Forms;

namespace Calculator
{
    public class CrunchSystemKeyboard : ISoftKeyboard
    {
        public static readonly CrunchSystemKeyboard Instance = new CrunchSystemKeyboard();

        public event EventHandler OnscreenSizeChanged;
        public event KeystrokeEventHandler Typed;

        public VisualElement Variables
        {
            set
            {
                if (value == null)
                {
                    VariablesHeight = 0;
                }
                else
                {
                    value.Bind<double>(VisualElement.HeightProperty, variablesHeight =>
                    {
                        VariablesHeight = variablesHeight + App.PAGE_PADDING;
                        OnOnscreenSizeChanged();
                    });
                }
            }
        }
        public Size Size { get; private set; }

        private double VariablesHeight;

        public CrunchSystemKeyboard()
        {
            if (Instance != null)
            {
                return;
            }

            SystemKeyboard.Instance.Typed += (sender) => Typed?.Invoke(sender);
            SystemKeyboard.Instance.OnscreenSizeChanged += (sender, e) => OnOnscreenSizeChanged();
        }

        public void Disable() => SystemKeyboard.Instance.Disable();

        public void Enable() => SystemKeyboard.Instance.Enable();

        protected void OnOnscreenSizeChanged(Size onscreenSize)
        {
            Size = onscreenSize;
            OnscreenSizeChanged?.Invoke(this, new EventArgs());
        }

        private void OnOnscreenSizeChanged() => OnOnscreenSizeChanged(new Size(SystemKeyboard.Instance.Size.Width, SystemKeyboard.Instance.Size.Height + VariablesHeight));
    }
}
