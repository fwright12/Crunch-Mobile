using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using CoreGraphics;

using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using Calculator;
using Google.MobileAds;

[assembly: ExportRenderer(typeof(BannerAd), typeof(Calculator.iOS.BannerAdRenderer))]

namespace Calculator.iOS
{
    //ca-app-pub-1795523054003202/6023719192

    //test
    //ca-app-pub-3940256099942544/2934735716

    public class BannerAdRenderer : ViewRenderer<BannerAd, BannerView>
    {
        string AdUnitId =
#if DEBUG
            //Test
            "ca-app-pub-3940256099942544/2934735716";
#else
            //Real
            "ca-app-pub-1795523054003202/6023719192";
#endif

        protected override void OnElementChanged(ElementChangedEventArgs<BannerAd> e)
        {
            base.OnElementChanged(e);

            if (Control == null)
            {
                SetNativeControl(CreateBannerView());
            }
            if (e.OldElement == null)
            {
                //e.NewElement.WidthRequest = MainPage.MaxBannerWidth;
                //e.NewElement.HeightRequest = (int)Math.Ceiling(MainPage.MaxBannerWidth * 50.0 / 320.0);
            }
        }

        private BannerView CreateBannerView()
        {
            /*var bannerView = new BannerView(AdSizeCons.GetFromCGSize(new CGSize(
                MainPage.MaxBannerWidth,
                (int)Math.Ceiling(MainPage.MaxBannerWidth * 50.0 / 320.0)
                )))*/
            var bannerView = new BannerView(AdSizeCons.GetFromCGSize(new CGSize(MainPage.MaxBannerWidth, 50)))
            {
                AdUnitID = AdUnitId,
                RootViewController = GetVisibleViewController()
            };

            bannerView.LoadRequest(GetRequest());

            Request GetRequest()
            {
                var request = Request.GetDefaultRequest();
                return request;
            }

            return bannerView;
        }

        private UIViewController GetVisibleViewController()
        {
            var windows = UIApplication.SharedApplication.Windows;
            foreach (var window in windows)
            {
                if (window.RootViewController != null)
                {
                    return window.RootViewController;
                }
            }
            return null;
        }
    }
}