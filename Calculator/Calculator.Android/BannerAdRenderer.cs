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
using Android.Gms.Ads;

using Xamarin.Forms.Platform.Android;
using Xamarin.Forms;
using Calculator;

[assembly: ExportRenderer(typeof(BannerAd), typeof(Calculator.Droid.BannerAdRenderer))]

namespace Calculator.Droid
{
    public class BannerAdRenderer : ViewRenderer<BannerAd, AdView>
    {
        public BannerAdRenderer(Context context) : base(context) { }

        //Note you may want to adjust this, see further down.
        private AdView adView;
        private AdView CreateNativeAdControl()
        {
            if (adView != null)
                return adView;

            adView = new AdView(Context);
            //adView.AdSize = AdSize.Banner;
            adView.AdSize = new AdSize(MainPage.MaxBannerWidth, (int)Math.Ceiling(MainPage.MaxBannerWidth * 50.0 / 320.0));
            adView.AdUnitId = "ca-app-pub-1795523054003202/4422905833";

            adView.LoadAd(new AdRequest.Builder().Build());

            return adView;
        }

        protected override void OnElementChanged(ElementChangedEventArgs<BannerAd> e)
        {
            base.OnElementChanged(e);
            if (Control == null)
            {
                //MainPage.LoadAd = CreateNativeAdControl;
                CreateNativeAdControl();
                SetNativeControl(adView);
            }
        }
    }
}