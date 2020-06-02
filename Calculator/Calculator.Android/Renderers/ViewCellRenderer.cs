using Android.Content;
using Android.Views;

using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(ViewCell), typeof(Calculator.Droid.ViewCellRenderer))]

namespace Calculator.Droid
{
    public class ViewCellRenderer : Xamarin.Forms.Platform.Android.ViewCellRenderer
    {
        private static readonly double DefaultCellPadding = 8;

        private static Thickness LayoutPadding(Context context) => new Thickness((int)context.FromPixels(8));
        private static Thickness TextPadding(Context context)
        {
            var padding = (int)context.FromPixels(8);
            return new Thickness((int)context.FromPixels((int)context.ToPixels(15)), padding, padding, padding);
        }

        protected override Android.Views.View GetCellCore(Cell item, Android.Views.View convertView, ViewGroup parent, Context context)
        {
            var cell = base.GetCellCore(item, convertView, parent, context);

            //cell = new Android.Widget.LinearLayout(context) { Orientation = Android.Widget.Orientation.Horizontal };
            //var padding = (int)context.FromPixels(8);
            //cell.SetPadding(padding, padding, padding, padding);
            /*//(cell as ViewGroup).GetChildAt(0).SetBackgroundColor(Android.Graphics.Color.Green);

            //(cell as ViewGroup).RemoveAllViews();
            var _mainText = new Android.Widget.TextView(context) { Text = "test" };
            _mainText.SetBackgroundColor(Android.Graphics.Color.Green);
            _mainText.SetSingleLine(true);
            _mainText.SetPadding((int)context.ToPixels(15), padding, padding, padding);
            Android.Support.V4.Widget.TextViewCompat.SetTextAppearance(_mainText, global::Android.Resource.Style.TextAppearanceSmall);

            using (var lp = new Android.Widget.LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent))
                (cell as ViewGroup).AddView(_mainText, lp);*/

            //(cell as ViewGroup).GetChildAt(0).SetPadding((int)context.ToPixels(15), padding, padding, padding);
            /*item.Tapped += (sender, e) =>
            {
                //(sender as ViewCell).View = new Label { Text = "hi" };//.Margin = new Thickness(20);// (cell as ViewGroup).GetChildAt(0).Context.ToPixels((int)context.FromPixels(15)) + cell.Context.ToPixels((int)context.FromPixels(8)), 8, 16, 8);
            };*/
            
            if (item is ViewCell viewCell && !viewCell.View.IsSet(Xamarin.Forms.View.MarginProperty))
            {
                Thickness layoutPadding = LayoutPadding(context);
                Thickness textPadding = TextPadding(context);

                //layoutPadding.Left += textPadding.Left;
                //viewCell.View.Margin = layoutPadding;
                viewCell.View.Margin = new Thickness(layoutPadding.Left + textPadding.Left, System.Math.Max(DefaultCellPadding, layoutPadding.Top + textPadding.Top), layoutPadding.Right + textPadding.Right, System.Math.Max(DefaultCellPadding, layoutPadding.Bottom + textPadding.Bottom));

                if (viewCell is LabeledCell labeledCell)
                {
                    //labeledCell.LabelView.Padding = new Thickness(0, textPadding.Top, 0, textPadding.Bottom);
                }
                /*viewCell.View.Margin = new Thickness(
                    layoutPadding.Left + textPadding.Left,
                    layoutPadding.Top + textPadding.Top,
                    layoutPadding.Right + textPadding.Right,
                    layoutPadding.Bottom + textPadding.Bottom);*/

                //int p = (int)context.FromPixels(8);
                //viewCell.View.Margin = new Thickness(p + (int)context.FromPixels((int)context.ToPixels(15)), p * 2, p * 2, p * 2);
                //viewCell.View.Margin = new Thickness(0);
                //viewCell.View.BackgroundColor = Color.Green;
                //cell.SetPadding((int)context.FromPixels(23), 8, 8, 8);
            }

            return cell;
        }
    }
}