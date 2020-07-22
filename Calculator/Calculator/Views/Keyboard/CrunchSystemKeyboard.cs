using System;
using System.Collections.Generic;
using System.Text;

using Xamarin.Forms;

namespace Calculator
{
    public class CrunchSystemKeyboard : ISoftKeyboard
    {
        public static CrunchSystemKeyboard Instance;// = new CrunchSystemKeyboard();

        public event EventHandler OnscreenSizeChanged;
        //public event KeystrokeEventHandler Typed;

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
                        VariablesHeight = variablesHeight + App.PAGE_PADDING * 2;
                        OnOnscreenSizeChanged();
                    });
                }
            }
        }
        public Size Size { get; private set; }

        private double VariablesHeight;
        private Thickness SafeArea;

        public void SetSafeArea(Thickness safeArea)
        {
            SafeArea = safeArea;
            OnOnscreenSizeChanged();
        }

        public CrunchSystemKeyboard()
        {
            if (Instance != null)
            {
                return;
            }
            Instance = this;
            //SystemKeyboard.Instance.Typed += (sender) => Typed?.Invoke(sender);
            KeyboardEntry.Instance.OnscreenSizeChanged += (sender, e) => OnOnscreenSizeChanged();
        }

        public void Disable(bool animated = false) => KeyboardEntry.Instance.Disable(animated);

        public void Enable(bool animated = false) => KeyboardEntry.Instance.Enable(animated);

        protected void OnOnscreenSizeChanged(Size onscreenSize)
        {
            Size = onscreenSize;
            OnscreenSizeChanged?.Invoke(this, new EventArgs());
        }

        private void OnOnscreenSizeChanged() => OnOnscreenSizeChanged(new Size(KeyboardEntry.Instance.Size.Width, KeyboardEntry.Instance.Size.Height + VariablesHeight));
    }
}
