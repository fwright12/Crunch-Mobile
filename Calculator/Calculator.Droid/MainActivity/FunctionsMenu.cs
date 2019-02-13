using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Calculator.Droid
{
    public partial class MainActivity
    {
        private LinearLayout functionsMenu;

        private void FunctionMenuSetup()
        {
            functionsMenu = FindViewById<LinearLayout>(Resource.Id.FunctionalityLayout);
            functionsMenu.Visibility = ViewStates.Gone;

            FindViewById<Button>(Resource.Id.FunctionalityButton).Click += ShowFunctionsMenu;

            //Formatting for formulas in menu
            foreach (string s in GraphicsEngine.supportedFunctions.Keys)
            {
                TextView temp = new TextView(this);
                temp.Text = s;
                temp.TextSize = 25;

                temp.SetPadding(0, 10, 0, 10);
                temp.Gravity = GravityFlags.Center;

                temp.Touch += FunctionTouch;

                FindViewById<LinearLayout>(Resource.Id.scrollingLinearLayout).AddView(temp);
            }
        }

        private void FunctionTouch(object sender, View.TouchEventArgs e)
        {
            TextView temp = sender as TextView;

            var data = ClipData.NewPlainText("canvas", temp.Text);
            temp.StartDrag(data, new View.DragShadowBuilder(temp), null, 0);
        }

        private void ShowFunctionsMenu(object sender, EventArgs e)
        {
            functionsMenu.Visibility = ViewStates.Visible;
        }

        private void HideFunctionsMenu()
        {
            functionsMenu.Visibility = ViewStates.Gone;
        }
    }
}