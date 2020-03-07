using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Calculator
{
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