using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Boxed.DataModel;
using Brain.Animate;
using Brain.Animate.NavigationAnimations;
using Brain.Extensions;

namespace Boxed
{
    public sealed partial class App : Application
    {
        private TransitionCollection transitions;

        public static AnimationFrame RootFrame { get; set; }
        public static Brush LineBrush = "#FFFFFF".ToColorBrush();


        public App()
        {
            InitializeComponent();
            Suspending += OnSuspending;
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            
#if DEBUG2
            if (System.Diagnostics.Debugger.IsAttached)
            {
                DebugSettings.EnableFrameRateCounter = true;
                //DebugSettings.EnableRedrawRegions = true;
            }
#endif

            RootFrame = Window.Current.Content as AnimationFrame;

            e.SplashScreen.Dismissed += (sender, args) =>
            {
                //RootFrame.Background = new SolidColorBrush(Colors.Aqua);
            };

            if (RootFrame == null)
            {
                RootFrame = new AnimationFrame
                {
                    NavigationAnimation = new RotateScaleUpNavigationAnimation(), 
                    CacheSize = 10,
                    Background = new  SolidColorBrush(Colors.Black),
                    Transitions = null
                };
                //RootFrame.Background = Current.Resources["BackGroundBrush"] as Brush;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                }

                await GameManager.Current.LoadKnownGamePacks();
                await GameData.Current.LoadData();

                Window.Current.Content = RootFrame;

                var statusBar = StatusBar.GetForCurrentView();
                statusBar.HideAsync();
            }

            if (RootFrame.Content == null)
                RootFrame.Navigate(typeof (MainPage), e.Arguments);
        
            Window.Current.Activate();
        }

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            deferral.Complete();
        }
    }
}