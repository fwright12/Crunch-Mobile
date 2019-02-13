using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculator
{
    /// <summary>
    /// Please include the following method calls where appropriate: 
    /// </summary>
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
