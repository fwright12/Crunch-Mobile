using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculator
{
    public interface IUserInterface
    {
        //void CanvasTouched();
        //void KeyboardLongClicked();

        object PhantomCursor();

        void KeyboardSetup();
        void KeyboardCursorMode();
        void RevertFromCursorMode();
        void ShowKeyboard();
        void HideKeyboard();

        void FunctionsMenuSetup();
        void ShowFunctionsMenu();
        void HideFunctionsMenu();
    }
}
