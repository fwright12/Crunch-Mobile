using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace Calculator.Droid
{
    [Activity(Label = "Crunch", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            FFImageLoading.Forms.Platform.CachedImageRenderer.Init(true);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new App());

            Window.SetSoftInputMode(SoftInput.AdjustResize);

            Android.Gms.Ads.MobileAds.Initialize(ApplicationContext, "ca-app-pub-1795523054003202~5967496762");
        }

        public static event EventHandler<EventArgs<Android.Support.V7.View.ActionMode>> ContextMenuAppeared;

        /*public override Android.Support.V7.View.ActionMode StartSupportActionMode(global::Android.Support.V7.View.ActionMode.ICallback callback)
        {
            var LastActionMode = base.StartSupportActionMode(callback);
            ContextMenuAppeared?.Invoke(this, new EventArgs<Android.Support.V7.View.ActionMode>(LastActionMode));
            return LastActionMode;
        }*/
    }
}

