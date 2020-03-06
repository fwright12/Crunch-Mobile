using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Calculator
{
    public static class ImageLoading
    {
        private static LoadView LoadSpace = new LoadView { IsVisible = false };

        public static void Preload(this Layout<View> context, params ImageSource[] sources) => LoadSpace.ForceDraw(context, sources);

        private class LoadView : Layout<View>
        {
            public void ForceDraw(Layout<View> context, params ImageSource[] sources)
            {
                context?.Children.Add(this);

                foreach (ImageSource source in sources)
                {
                    Image image = new Image { Source = source };
                    Children.Add(image);
                    LayoutChildIntoBoundingRegion(image, new Rectangle(0, 0, double.PositiveInfinity, double.PositiveInfinity));
                }

                this.Remove();
            }

            protected override void InvalidateLayout() { }

            protected override void InvalidateMeasure() { }

            protected override void OnChildMeasureInvalidated() { }

            protected override bool ShouldInvalidateOnChildAdded(View child) => false;

            protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint) => new SizeRequest(Size.Zero);

            protected override void LayoutChildren(double x, double y, double width, double height) { }
        }
    }

#if DEBUG
    public static class ImageExtensions
    {
        private class FakeLayout : Layout
        {
            public FakeLayout(View content)
            {
                Content = content;
                Parent = App.Current.MainPage;
                OnParentSet();
                
                InvalidateMeasure();
                InvalidateLayout();

                OnMeasure(double.PositiveInfinity, double.PositiveInfinity);
                OnSizeAllocated(1000, 1000);
                ForceLayout();
                Layout(new Rectangle(0, 0, 1000, 1000));
                LayoutChildren(0, 0, 1000, 1000);
                LayoutChildIntoBoundingRegion(content, new Rectangle(Point.Zero, content.Measure(double.PositiveInfinity, double.PositiveInfinity).Request));

                ForceLayout();
            }

            private View Content;

            protected override void LayoutChildren(double x, double y, double width, double height)
            {
                LayoutChildIntoBoundingRegion(Content, new Rectangle(Point.Zero, Content.Measure(double.PositiveInfinity, double.PositiveInfinity).Request));
            }
        }

        private class FakeImage : Image
        {
            public FakeImage(ImageSource source)
            {
                Source = source;
                Parent = App.Current.MainPage;
                SetIsLoading(true);
                InvalidateMeasure();
                OnMeasure(double.PositiveInfinity, double.PositiveInfinity);
                OnSizeAllocated(1000, 1000);
                Layout(new Rectangle(0, 0, 1000, 1000));
            }
        }

        public static Image Preload(this Image image)
        {
            new FakeLayout(new Image { Source = image.Source });
            //(App.Current.Home.Content as AbsoluteLayout).Children.Add(new FakeLayout(new Image { Source = image.Source }), new Rectangle(-1000, -1000, 100, 10));
            return image;
        }

        public static ImageSource Preload(this ImageSource source)
        {
            new FakeImage(source);
            return source;
        }
    }
#endif

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TutorialDialog : ModalView
    {
        public event SimpleEventHandler Completed;

        private readonly List<string[]> info = new List<string[]>()
        {
            new string[] { "canvas.gif", "Tap anywhere on the canvas to add an equation" },
            new string[] { "answerForms.gif", "Tap answers to see different forms, or the deg/rad label to toggle degrees and radians" },
            new string[] { "moveEquations.gif", "Drag the equals sign to move an equation on the canvas" },
            new string[] { "dragDropAnswers.gif", "Drag and drop answers to create live links between calculations" },
        };

        private View[] Screens;
        private int Step = -1;

        public TutorialDialog(bool explainKeyboardScroll = false)
        {
            InitializeComponent();

            Next.Clicked += (sender, e) =>
            {
                if (Step + 1 == info.Count + 1)
                {
                    Completed?.Invoke();
                }
                else
                {
                    Set(Step + 1);
                }
            };
            Back.Clicked += (sender, e) =>
            {
                Set(Step - 1);
            };

            if (explainKeyboardScroll)
            {
                info.Add(new string[] { "scrollKeyboard.gif", "Scroll the keyboard for more operations" });
            }

            Screens = new View[info.Count + 1];
            Screens[0] = Showing.Content;
            for (int i = 0; i < info.Count; i++)
            {
                //FFImageLoading.TaskParameterExtensions.Preload(FFImageLoading.ImageService.Instance.LoadFileFromApplicationBundle(info[i][0])).RunAsync();
                Screens[i + 1] = (WebImage)new Image
                {
                    Source = info[i][0],
                    IsAnimationPlaying = true,
                };
            }

            Set(0);

#if ANDROID
            async void PreloadImages(object sender, EventArgs e)
            {
                SizeChanged -= PreloadImages;

                await System.Threading.Tasks.Task.Delay(1000);

                ImageSource[] sources = new ImageSource[info.Count];
                for (int i = 0; i < info.Count; i++)
                {
                    sources[i] = info[i][0];
                }
                (Content as Layout<View>)?.Preload(sources);

                ImagesPreloaded = true;
            }

            if (!ImagesPreloaded)
            {
                SizeChanged += PreloadImages;
            }
#endif
        }

        private bool ImagesPreloaded = false;

        /*private class Image : FFImageLoading.Forms.CachedImage, IImage
        {
            public event EventHandler<EventArgs<bool>> Loaded;

            public View View => this;

            public Image()
            {
                void InvokeLoadedOnSizeChange(object sender, EventArgs e)
                {
                    Loaded?.Invoke(this, new EventArgs<bool>(true));
                    SizeChanged -= InvokeLoadedOnSizeChange;
                }

                Success += (sender, e) => SizeChanged += InvokeLoadedOnSizeChange;
                Error += (sender, e) => Loaded?.Invoke(this, new EventArgs<bool>(false));
            }
        }*/

        private void Set(int step)
        {
            if (step < 0 || step >= info.Count + 1)
            {
                return;
            }

            Back.IsEnabled = step > 0;
            Next.Text = step + 1 == info.Count + 1 ? "Done" : "Next";
            Description.Text = step > 0 ? info[step - 1][1] : "";

            Step = step;

            Showing.Content = Screens[step];
        }
    }
}