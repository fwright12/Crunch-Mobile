using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using UIKit;
using Google.MobileAds;

namespace Calculator.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
#if DEBUG
            Xamarin.Forms.Internals.Log.Listeners.Add(new Xamarin.Forms.Internals.DelegateLogListener((c, m) => System.Diagnostics.Debug.WriteLine(m, c)));
#endif
            MobileAds.Configure("ca-app-pub-1795523054003202~2467617560");
            global::Xamarin.Forms.Forms.Init();
            LoadApplication(new App());

            FFImageLoading.Forms.Platform.CachedImageRenderer.Init();
            FFImageLoading.Forms.Platform.CachedImageRenderer.InitImageSourceHandler();

#if DEBUG
            Xamarin.Forms.Extensions.BindableObjectExtensions.WhenPropertyChanged(App.Current, Screenshots.InSampleModeProperty, (sender, e) => UIApplication.SharedApplication.StatusBarHidden = App.Current.GetInSampleMode());
#endif
            
            return base.FinishedLaunching(app, options);
        }

#if DEBUG
        private static ShowTouchesWindow theWindow;

        public override UIWindow Window
        {
            get
            {
                if (theWindow == null)
                {
                    theWindow = new ShowTouchesWindow(UIScreen.MainScreen.Bounds);
                    theWindow.AlwaysShowTouches = true;
                }
                return theWindow;

            }
            set { }
        }
#endif
    }
}
