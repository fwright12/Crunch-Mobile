using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

using Xamarin.Forms;
using Xamarin.Forms.Extensions;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(Calculator.FunctionsDrawer.ListView), typeof(Calculator.iOS.DrawerListViewRenderer))]

namespace Calculator.iOS
{
    public class DrawerListViewRenderer : ListViewRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<ListView> e)
        {
            base.OnElementChanged(e);
            
            AddGestureRecognizer(new UIDrawerGestureRecognizer(Control, Element as ListView)
            {
                CancelsTouchesInView = false
            });

            /*if (e.OldElement != e.NewElement)
            {
                e.NewElement?.WhenPropertyChanged(EditListView.EditingProperty, (sender, e1) =>
                {
                    Control.AllowsMultipleSelection = sender is EditListView editListView && editListView.Editing;
                });
            }*/
            //Control.InsetsContentViewsToSafeArea = false;
            e.NewElement.SetNativeImplementation(Scrollable.NativeScrollImplementationProperty, ScrollToRequest);
            Control.InsetsContentViewsToSafeArea = false;
            Control.InsetsLayoutMarginsFromSafeArea = false;
            //Scrollable.ScrollRequestProperty.ListenFor(ScrollToRequest, e.OldElement, e.NewElement);

            /*if (e.OldElement is FunctionsDrawer.ListView oldElement)
            {
                oldElement.ScrollToRequest -= ScrollToRequest;
            }
            if (e.NewElement is FunctionsDrawer.ListView newElement)
            {
                newElement.ScrollToRequest += ScrollToRequest;
                /*newElement.WhenPropertyChanged(EditListView.EditingProperty, (sender, e1) =>
                {
                    //Control.Editing = newElement.Editing;
                });*/
            //}

            //Control.AllowsMultipleSelectionDuringEditing = true;
        }

        public override void SafeAreaInsetsDidChange()
        {
            base.SafeAreaInsetsDidChange();
            Control.ContentInset = SafeAreaInsets;
            //Control.InsetsContentViewsToSafeArea = true;
            //Print.Log("safe insets changed", Control.ContentInset);
            //Print.Log("safe insets changed", SafeAreaInsets, SafeAreaInsets.Bottom);
            //Print.Log("\t" + Control.InsetsContentViewsToSafeArea, Control.AdjustedContentInset);
            //Control.ContentInset = SafeAreaInsets;
        }

        private void ScrollToRequest(ScrollToPositionRequestEventArgs e) => Control.SetContentOffset(new CoreGraphics.CGPoint(e.X, e.Y), e.Animated);

        public class UIDrawerGestureRecognizer : UIGestureRecognizer
        {
            private UIScrollView NativeScrollView;
            private ListView SharedScrollView;
            private Token Target;

            public UIDrawerGestureRecognizer(UIScrollView nativeScrollView, ListView sharedScrollView)
            {
                NativeScrollView = nativeScrollView;
                SharedScrollView = sharedScrollView;

                ShouldRecognizeSimultaneously = (a, b) =>
                {
                    //Print.Log("should recognize", b.State);
                    return NativeScrollView.GestureRecognizers.Contains(b);

                    //return b != Control.PanGestureRecognizer;
                    //return !(a == touch && b == Control.PanGestureRecognizer);
                    //Print.Log("should recognize", Element, ((FunctionsDrawer.ListView)Element).Scrolling);
                    //return !(Element as FunctionsDrawer.ListView)?.Scrolling ?? true;
                };
                ShouldReceiveTouch = (r, t) =>
                {
                    //Print.Log("should receive touch", (Element as FunctionsDrawer.ListView).Scrolling);
                    //return (Element as FunctionsDrawer.ListView)?.Scrolling ?? false;
                    return true;
                };
                
                Target = AddTarget(ShouldScroll);
                //NativeScrollView.PanGestureRecognizer.AddTarget(ShouldScroll);

                return;

                NativeScrollView.PanGestureRecognizer.AddTarget(() =>
                {
                    if (NativeScrollView.ContentOffset.Y < 0)
                    {
                        //ScrollView.PanGestureRecognizer.State = UIGestureRecognizerState.Cancelled;
                    }
                    //Print.Log("panning", ScrollView.PanGestureRecognizer.State);
                });
            }

            public void ShouldScroll()
            {
                bool temp = SharedScrollView?.GetSwipeListener().OnSwipeEvent(LocationInView(null).Convert(), State.Convert()) ?? false;
                //bool temp = SharedScrollView?.OnScrollEvent(LocationInView(null).Convert(), State.Convert()) ?? true;
                //Print.Log("touch", Control.ContentOffset.Y, temp, touch.LocationInView(null));
                //Print.Log("touch", temp, NativeScrollView.PanGestureRecognizer.State, State);
                //if (Control.ContentOffset.Y < 0)
                if (temp)
                {
                    
                }
                return;
                if (temp && State == UIGestureRecognizerState.Changed)
                {
                    //Control.SetContentOffset(new CoreGraphics.CGPoint(Control.ContentOffset.X, last), false);
                    State = UIGestureRecognizerState.Cancelled;
                    RemoveTarget(Target);
                    Target = NativeScrollView.PanGestureRecognizer.AddTarget(ShouldScroll);
                }
                else if (!temp && NativeScrollView.PanGestureRecognizer.State == UIGestureRecognizerState.Changed)
                {
                    NativeScrollView.PanGestureRecognizer.State = UIGestureRecognizerState.Cancelled;
                    NativeScrollView.PanGestureRecognizer.RemoveTarget(Target);
                    Target = AddTarget(ShouldScroll);
                }
                /*else if (!temp)
                {
                    Control.ContentOffset = new CoreGraphics.CGPoint(Control.ContentOffset.X, 0);
                    Control.PanGestureRecognizer.State = UIGestureRecognizerState.Cancelled;
                }*/
            }

            //public override bool ShouldRequireFailureOfGestureRecognizer(UIGestureRecognizer otherGestureRecognizer) => otherGestureRecognizer == NativeScrollView.PanGestureRecognizer;

            //public override bool ShouldBeRequiredToFailByGestureRecognizer(UIGestureRecognizer otherGestureRecognizer) => otherGestureRecognizer == NativeScrollView.PanGestureRecognizer;

            private void ShouldStart()
            {
                if (Xamarin.Forms.Extensions.TouchScreen.Active)
                {
                    State = UIGestureRecognizerState.Cancelled;
                    return;
                }

                if (State == UIGestureRecognizerState.Possible)
                {
                    if (NativeScrollView.PanGestureRecognizer.State == UIGestureRecognizerState.Began || NativeScrollView.PanGestureRecognizer.State == UIGestureRecognizerState.Failed)
                    {
                        State = NativeScrollView.PanGestureRecognizer.State;
                    }
                }
            }

            public override void TouchesBegan(NSSet touches, UIEvent evt)
            {
                base.TouchesBegan(touches, evt);
                ShouldStart();
            }

            public override void TouchesMoved(NSSet touches, UIEvent evt)
            {
                base.TouchesMoved(touches, evt);
                //Print.Log("touches moved", test, State);
                if (State != UIGestureRecognizerState.Possible)
                {
                    State = UIGestureRecognizerState.Changed;
                }
                ShouldStart();
            }

            public override void TouchesEnded(NSSet touches, UIEvent evt)
            {
                base.TouchesEnded(touches, evt);
                State = State == UIGestureRecognizerState.Possible ? UIGestureRecognizerState.Failed : UIGestureRecognizerState.Ended;
            }

            public override void TouchesCancelled(NSSet touches, UIEvent evt)
            {
                base.TouchesCancelled(touches, evt);
                State = UIGestureRecognizerState.Cancelled;
            }
        }
    }
}