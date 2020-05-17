#if false
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using Xamarin.Forms.Extensions;

[assembly: ExportRenderer(typeof(MasterDetailPage), typeof(TabletDrawerPageRenderer), UIUserInterfaceIdiom.Pad)]

namespace Xamarin.Forms.Extensions
{
    public class TabletDrawerPageRenderer : TabletMasterDetailRenderer
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Delegate = new TabletMasterDetailDelegate(ViewControllers);
        }

        private class TabletMasterDetailDelegate : UISplitViewControllerDelegate
        {
            private readonly UIViewController Master;
            private readonly UIViewController Detail;

            public TabletMasterDetailDelegate(UIViewController master, UIViewController detail)
            {
                Master = master;
                Detail = detail;
            }

            public TabletMasterDetailDelegate(UIViewController[] viewControllers) : this(viewControllers[0], viewControllers.Length > 1 ? viewControllers[1] : null) { }

            public override UIViewController GetPrimaryViewControllerForCollapsingSplitViewController(UISplitViewController splitViewController) => Detail;

            public override UIViewController GetPrimaryViewControllerForExpandingSplitViewController(UISplitViewController splitViewController) => Master;
        }

        private void UpdatePreferredDisplayMode()
        {
            if (!IsSplit)
            {
                PreferredDisplayMode = !MasterDetailPage.IsPresented ? UISplitViewControllerDisplayMode.PrimaryOverlay : UISplitViewControllerDisplayMode.PrimaryHidden;
            }

            PreferredDisplayMode = UISplitViewControllerDisplayMode.AllVisible;
        }


        private bool IsSplit =>
            MasterDetailPage.MasterBehavior == MasterBehavior.Split ||
            (MasterDetailPage.MasterBehavior == MasterBehavior.SplitOnLandscape && true) ||
            (MasterDetailPage.MasterBehavior == MasterBehavior.SplitOnPortrait && true);
    }
}
#endif