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

using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(Calculator.Canvas), typeof(Calculator.Droid.CanvasRenderer))]

namespace Calculator.Droid
{
    public class CanvasRenderer : VisualElementRenderer<Xamarin.Forms.AbsoluteLayout>
    {
        public CanvasRenderer(Context context) : base(context)
        {
            Touch += (sender, e) => this.RelayTouch(e.Event);// Element.RelayTouch(sender as Android.Views.View, e.Event);
            //Drag += dropEquation;
        }

        private void dropEquation(object sender, DragEventArgs e)
        {
            var evt = e.Event;

            switch (evt.Action)
            {
                case DragAction.Started:
                    /* To register your view as a potential drop zone for the current view being dragged
                     * you need to set the event as handled
                     */
                    e.Handled = true;

                    /* An important thing to know is that drop zones need to be visible (i.e. their Visibility)
                     * property set to something other than Gone or Invisible) in order to be considered. A nice workaround
                     * if you need them hidden initially is to have their layout_height set to 1.
                     */

                    break;
                case DragAction.Entered:
                case DragAction.Exited:
                    /* These two states allows you to know when the dragged view is contained atop your drop zone.
                     * Traditionally you will use that tip to display a focus ring or any other similar mechanism
                     * to advertise your view as a drop zone to the user.
                     */

                    break;
                case DragAction.Drop:
                    /* This state is used when the user drops the view on your drop zone. If you want to accept the drop,
                     *  set the Handled value to true like before.
                     */
                    e.Handled = true;
                    /* It's also probably time to get a bit of the data associated with the drag to know what
                     * you want to do with the information.
                     */

                    Print.Log("dropped");

                    break;
                case DragAction.Ended:
                    /* This is the final state, where you still have possibility to cancel the drop happened.
                     * You will generally want to set Handled to true.
                     */
                    e.Handled = true;
                    break;
            }
        }
    }
}