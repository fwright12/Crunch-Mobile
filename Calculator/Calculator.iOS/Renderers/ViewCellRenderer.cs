using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

using Foundation;
using MapKit;
using UIKit;

using Xamarin.Forms;

[assembly: ExportRenderer(typeof(ViewCell), typeof(Calculator.iOS.ViewCellRenderer))]

namespace Calculator.iOS
{
    public class ViewCellRenderer : Xamarin.Forms.Platform.iOS.ViewCellRenderer
    {
        /*internal class NativeiOSCell : UITableViewCell, INativeElementView
        {
            public UILabel HeadingLabel { get; set; }
            public UILabel SubheadingLabel { get; set; }
            public UIImageView CellImageView { get; set; }

            public ViewCell NativeCell { get; private set; }
            public Element Element => NativeCell;

            public NativeiOSCell(string cellId, ViewCell cell) : base(UITableViewCellStyle.Default, cellId)
            {
                NativeCell = cell;

                //TextLabel.Text = "textlabel";
                //DetailTextLabel.Text = "detailtextlabel";

                SelectionStyle = UITableViewCellSelectionStyle.Gray;
                ContentView.BackgroundColor = UIColor.FromRGB(255, 255, 224);
                CellImageView = new UIImageView();

                HeadingLabel = new UILabel()
                {
                    Font = UIFont.FromName("Cochin-BoldItalic", 22f),
                    TextColor = UIColor.FromRGB(127, 51, 0),
                    BackgroundColor = UIColor.Clear,
                    Text = "heading"
                };

                SubheadingLabel = new UILabel()
                {
                    Font = UIFont.FromName("AmericanTypewriter", 12f),
                    TextColor = UIColor.FromRGB(38, 127, 0),
                    TextAlignment = UITextAlignment.Center,
                    BackgroundColor = UIColor.Clear,
                    Text = "subheading"
                };

                ContentView.Add(HeadingLabel);
                ContentView.Add(SubheadingLabel);
                ContentView.Add(CellImageView);
            }

            public void UpdateCell(ViewCell cell)
            {
                
            }

            public UIImage GetImage(string filename)
            {
                return (!string.IsNullOrWhiteSpace(filename)) ? UIImage.FromFile("Images/" + filename + ".jpg") : null;
            }

            public override void LayoutSubviews()
            {
                base.LayoutSubviews();

                Print.Log("updated", TextLabel.LayoutMargins, TextLabel.Frame);
                HeadingLabel.Frame = TextLabel.Frame;// new CoreGraphics.CGRect(8, 8, ContentView.Bounds.Width - 63, 25);
                //SubheadingLabel.Frame = new CoreGraphics.CGRect(100, 18, 100, 20);
                //CellImageView.Frame = new CoreGraphics.CGRect(ContentView.Bounds.Width - 63, 5, 33, 33);
            }
        }*/

        private static readonly double DefaultCellPadding = 6;
        private static BindableProperty NativeCellProperty = BindableProperty.CreateAttached("NativeView", typeof(UIView), typeof(Cell), null);

        public override UITableViewCell GetCell(Cell item, UITableViewCell reusableCell, UITableView tv)
        {
            if (reusableCell is INativeElementView nativeElementView && nativeElementView.Element is Cell oldSharedCell)
            {
                
            }
            
            var cell = base.GetCell(item, reusableCell, tv);
            item.SetValue(NativeCellProperty, cell);

            //cell.LayoutSubviews();
            //Print.Log("getting view cell", cell.TextLabel.Frame, item == null, reusableCell == null);
            /*foreach (UIView view in cell.ContentView.Subviews)
            {
                //Print.Log("\t" + view.GetType(), view.Frame, view.InsetsLayoutMarginsFromSafeArea);
            }*/
            /*item.Tapped += (sender, e) =>
            {
                cell.ContentView.LeadingAnchor.ConstraintEqualTo(cell.ContentView.LayoutMarginsGuide.LeadingAnchor).Active = true;

                cell.PreservesSuperviewLayoutMargins = true;
                cell.ContentView.PreservesSuperviewLayoutMargins = true;
                foreach (UIView view in cell.ContentView.Subviews)
                {
                    Print.Log("\t" + view.GetType(), view.LayoutMargins, view.LayoutGuides, view.PreservesSuperviewLayoutMargins);
                    view.PreservesSuperviewLayoutMargins = true;
                }
                Print.Log("here");
            };*/

            if (item is ViewCell viewCell && !viewCell.View.IsSet(Xamarin.Forms.View.MarginProperty))
            {
                viewCell.View.SizeChanged -= UpdateMargin;
                viewCell.View.SizeChanged += UpdateMargin;
            }

            return cell;// new NativeiOSCell(item.GetType().FullName, (ViewCell)item);
        }

        private static void UpdateMargin(object sender, EventArgs e)
        {
            ViewCell viewCell = (ViewCell)((View)sender).Parent;
            UITableViewCell nativeCell = (UITableViewCell)viewCell.GetValue(NativeCellProperty);

            var view = nativeCell.ContentView.Frame;
            var label = nativeCell.TextLabel.Frame;
            //Print.Log("tapped", view, label, nativeCell.ContentView.Frame, nativeCell.SeparatorInset, nativeCell.IndentationWidth, nativeCell.IndentationLevel);
            //Print.Log("\t" + NativeView.TextLabel.LayoutMargins, NativeView.ContentView.Subviews[0].LayoutMargins, NativeView.TextLabel);

            //cell.ContentView.Subviews[0].Frame = cell.TextLabel.Frame;

            Thickness margin = new Thickness(label.Left - view.Left, label.Top - view.Top, view.Right - label.Right, view.Bottom - label.Bottom);

            viewCell.View.Margin = new Thickness(margin.Left, Math.Max(DefaultCellPadding, margin.Top), margin.Right, Math.Max(DefaultCellPadding, margin.Bottom));

            if (viewCell is LabeledCell labeledCell)
            {
                labeledCell.LabelView.FontSize = Math.Ceiling(nativeCell.TextLabel.Font.PointSize);
                //labeledCell.LabelView.Margin = new Thickness(0, margin.Top, 0, margin.Bottom);
            }
        }
    }
}