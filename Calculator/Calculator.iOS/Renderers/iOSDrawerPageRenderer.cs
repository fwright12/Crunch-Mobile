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
        /*protected override void OnElementChanged(VisualElementChangedEventArgs e)
        {
            base.OnElementChanged(e);

            /*Print.Log("view controllers");
            foreach(UIViewController view in ViewControllers)
            {
                Print.Log(view);
            }

            WillHideViewController += (sender, e1) =>
            {
                Print.Log("will hide", e1.AViewController);
            };
            WillShowViewController += (sender, e1) =>
            {
                Print.Log("will show", e1.AViewController);
            };
            //GetPrimaryViewControllerForExpandingSplitViewController = (a) => ViewControllers[0];
            //GetPrimaryViewControllerForCollapsingSplitViewController = (a) => ViewControllers[1];
            //PreferredDisplayMode = UISplitViewControllerDisplayMode.PrimaryOverlay;
        }

        public override void WillRotate(UIInterfaceOrientation toInterfaceOrientation, double duration)
        {
            base.WillRotate(toInterfaceOrientation, duration);
            
            //PreferredDisplayMode = UISplitViewControllerDisplayMode.PrimaryOverlay;
        }

        //public override UITraitCollection TraitCollection => UITraitCollection.FromHorizontalSizeClass(UIUserInterfaceSizeClass.Regular);

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            //Delegate = new Test();
        }

        private class Test : UISplitViewControllerDelegate
        {
            public override bool CollapseSecondViewController(UISplitViewController splitViewController, UIViewController secondaryViewController, UIViewController primaryViewController)
            {
                return false;
            }
        }*/

        //public override bool Collapsed => false;// MasterDetailPage.IsPresented;

        private void PrintViewControllers()
        {
            Print.Log("view controllers");
            foreach(UIViewController view in ViewControllers)
            {
                Print.Log(view);
            }
        }

        private UIViewController[] LoadedViewControllers;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            //LoadedViewControllers = ViewControllers;
            PrintViewControllers();
            Delegate = new TabletMasterDetailDelegate(MasterDetailPage, ViewControllers);
        }

        //public override UISplitViewControllerDisplayMode DisplayMode => Collapsed ? UISplitViewControllerDisplayMode.PrimaryHidden : base.DisplayMode;

        private bool IsPresented => DisplayMode == UISplitViewControllerDisplayMode.AllVisible || DisplayMode == UISplitViewControllerDisplayMode.PrimaryOverlay;

        protected override void OnElementChanged(VisualElementChangedEventArgs e)
        {
            base.OnElementChanged(e);
            
            if (MasterDetailPage == null)
            {
                return;
            }
            
            MasterDetailPage.PropertyChanged += (sender, e1) =>
            {
                if (!e1.IsProperty(MasterDetailPage.IsPresentedProperty))
                {
                    return;
                }

                Print.Log("root is presented changed", MasterDetailPage.IsPresented, Collapsed, DisplayMode, IsPresented);
                if (Collapsed)
                {
                    //MasterDetailPage.IsPresented = false;
                }
                //MasterDetailPage.IsPresented = !Collapsed && IsPresented;
            };

            /*WillShowViewController += (sender, e1) =>
            {
                Print.Log("will show");
                PrintViewControllers();
                //ViewControllers = LoadedViewControllers;
            };
            WillHideViewController += (sender, e1) =>
            {
                Print.Log("will hide", DisplayMode);
                //AddChildViewController(LoadedViewControllers[1]);
                //ShowDetailViewController(LoadedViewControllers[1], this);
                PrintViewControllers();
                //ViewControllers = LoadedViewControllers;
            };*/
            /*SeparateSecondaryViewController = (a, b) =>
            {
                Print.Log("separating", LoadedViewControllers[1]);
                PrintViewControllers();
                return LoadedViewControllers[0];
            };*/

            //GetPrimaryViewControllerForCollapsingSplitViewController = (a) => LoadedViewControllers[1];

            //GetPrimaryViewControllerForExpandingSplitViewController = (a) => LoadedViewControllers[0];

            //ShouldHideViewController = (a, b, c) => true;
            //CollapseSecondViewController = (a, b, c) => false;
            //PreferredDisplayMode = UISplitViewControllerDisplayMode.AllVisible;
            return;
            MasterDetailPage.PropertyChanged += (sender, e1) =>
            {
                if (!e1.IsProperty(MasterDetailPage.MasterBehaviorProperty) && !e1.IsProperty(MasterDetailPage.IsPresentedProperty))
                {
                    return;
                }
                Print.Log("ispresented changed", MasterDetailPage.IsPresented);
                PrintViewControllers();

                UpdatePreferredDisplayMode();
            };

            UpdatePreferredDisplayMode();
        }

        private class TabletMasterDetailDelegate : UISplitViewControllerDelegate
        {
            private readonly MasterDetailPage Element;
            private readonly UIViewController Master;
            private readonly UIViewController Detail;

            public TabletMasterDetailDelegate(MasterDetailPage element, UIViewController master, UIViewController detail)
            {
                Element = element;
                Master = master;
                Detail = detail;
            }

            public TabletMasterDetailDelegate(MasterDetailPage element, UIViewController[] viewControllers) : this(element, viewControllers[0], viewControllers.Length > 1 ? viewControllers[1] : null) { }

            public override UIViewController GetPrimaryViewControllerForCollapsingSplitViewController(UISplitViewController splitViewController)
            {
                Print.Log("collapsing", Master, Detail);
                //splitViewController.PreferredDisplayMode = UISplitViewControllerDisplayMode.PrimaryHidden;
                //Element.IsPresented = false;
                return Detail;
            }

            /*public override bool CollapseSecondViewController(UISplitViewController splitViewController, UIViewController secondaryViewController, UIViewController primaryViewController)
            {
                Print.Log("collapse secondary", splitViewController.DisplayMode);
                //splitViewController.PreferredDisplayMode = UISplitViewControllerDisplayMode.PrimaryHidden;
                return true;
            }*/

            public override UIViewController GetPrimaryViewControllerForExpandingSplitViewController(UISplitViewController splitViewController)
            {
                Print.Log("expanding", Master, Detail);
                //splitViewController.PreferredDisplayMode = UISplitViewControllerDisplayMode.AllVisible;
                return Master;
            }

            public override void WillChangeDisplayMode(UISplitViewController svc, UISplitViewControllerDisplayMode displayMode)
            {
                //base.WillChangeDisplayMode(svc, displayMode);
                Print.Log("display mode changing to " + displayMode);
            }

            /*public override UIViewController SeparateSecondaryViewController(UISplitViewController splitViewController, UIViewController primaryViewController)
            {
                //Print.Log("separating");
                return Detail;
            }*/
        }

        private void UpdatePreferredDisplayMode()
        {
            if (!IsSplit)
            {
                PreferredDisplayMode = !MasterDetailPage.IsPresented ? UISplitViewControllerDisplayMode.PrimaryOverlay : UISplitViewControllerDisplayMode.PrimaryHidden;
            }

            //PreferredDisplayMode = UISplitViewControllerDisplayMode.AllVisible;
        }


        private bool IsSplit =>
            MasterDetailPage.MasterBehavior == MasterBehavior.Split ||
            (MasterDetailPage.MasterBehavior == MasterBehavior.SplitOnLandscape && true) ||
            (MasterDetailPage.MasterBehavior == MasterBehavior.SplitOnPortrait && true);
    }
}