using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Content.Res;

namespace Calculator.Droid
{
    [Activity(Label = "Crunch", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new App());
            SetTheme(Resources.Configuration);

            FFImageLoading.Forms.Platform.CachedImageRenderer.Init(true);
            FFImageLoading.Forms.Platform.CachedImageRenderer.InitImageViewHandler();

            Window.SetSoftInputMode(SoftInput.AdjustResize);

            Android.Gms.Ads.MobileAds.Initialize(ApplicationContext, "ca-app-pub-1795523054003202~5967496762");
        }

        public override void OnConfigurationChanged(Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);
            SetTheme(newConfig);
        }

        private void SetTheme(Configuration config) => App.Current.SystemDarkModeEnabled = config.UiMode.HasFlag(UiMode.NightYes);

        //public static event EventHandler<EventArgs<Android.Support.V7.View.ActionMode>> ContextMenuAppeared;

        /*public override Android.Support.V7.View.ActionMode StartSupportActionMode(global::Android.Support.V7.View.ActionMode.ICallback callback)
        {
            var LastActionMode = base.StartSupportActionMode(callback);
            ContextMenuAppeared?.Invoke(this, new EventArgs<Android.Support.V7.View.ActionMode>(LastActionMode));
            return LastActionMode;
        }*/
    }
}

