using System.Text;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI;
using Windows.UI.ApplicationSettings;
using Boxed.DataModel;
using Boxed.Win.Common;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Boxed.Common;

// The Hub App template is documented at http://go.microsoft.com/fwlink/?LinkId=321221
using Brain.Animate;
using Brain.Animate.NavigationAnimations;

namespace Boxed.Win
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {

        public static AnimationFrame RootFrame;

        /// <summary>
        /// Initializes the singleton Application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        protected override void OnWindowCreated(WindowCreatedEventArgs args)
        {
            SettingsPane.GetForCurrentView().CommandsRequested += (sender, eventArgs) =>
            {
                var aboutSettings = new SettingsCommand("About", "About", h => new AboutSettings().Show());
                var howToPlaySettings = new SettingsCommand("HowToPlay", "How to play", h => new HowToPage().Show());
                var optionsSettings = new SettingsCommand("Options", "Game options", h => new OptionsSettings().Show());
                var newsSettings = new SettingsCommand("News", "What's New", h => new NewsPage().Show());

                eventArgs.Request.ApplicationCommands.Add(aboutSettings);
                eventArgs.Request.ApplicationCommands.Add(howToPlaySettings);
                eventArgs.Request.ApplicationCommands.Add(optionsSettings);
                eventArgs.Request.ApplicationCommands.Add(newsSettings);
            };
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            await GameManager.Current.LoadKnownGamePacks();
            await GameManager.Current.LoadInternetGamePacks();
            await GameData.Current.LoadData();

            DataTransferManager.GetForCurrentView().DataRequested += OnDataRequested;

            RootFrame = Window.Current.Content as AnimationFrame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active

            if (RootFrame == null)
            {
                RootFrame = new AnimationFrame
                {
                    NavigationAnimation = new SuperScaleNavigationAnimation()
                };
                SuspensionManager.RegisterFrame(RootFrame, "AppFrame");

                RootFrame.Language = Windows.Globalization.ApplicationLanguages.Languages[0];

                RootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    try
                    {
                        await SuspensionManager.RestoreAsync();
                    }
                    catch (SuspensionManagerException)
                    {
                    }
                }

                Window.Current.Content = RootFrame;

                RootFrame.Background = new SolidColorBrush(Colors.Black);

                //AnimationHelper.AnimateBackgroundRainbow(RootFrame);
                AnimationHelper.AnimateBackgroundDark(App.RootFrame);
            }
            if (RootFrame.Content == null)
            {
                RootFrame.Navigate(typeof(HubPage), e.Arguments);
            }

            Window.Current.Activate();
        }

        private void OnDataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            var data = args.Request.Data;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<p>Boxed is a totally awesome puzzle game in the Windows Store</p><br /><br />");
            sb.AppendLine("<b>You should try it out</b>");
            sb.AppendLine();
            sb.AppendLine("<p><a href='http://apps.microsoft.com/windows/app/boxed/032ce91d-899a-495e-914a-6c01d9e72915'>Boxed</a></p>");

            var html = HtmlFormatHelper.CreateHtmlFormat(sb.ToString());
            //data.SetText( sb.ToString());
            data.SetHtmlFormat(html);

            data.Properties.Title = "Boxed is Awesome";
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            await SuspensionManager.SaveAsync();
            deferral.Complete();
        }


        public static void StartMusic()
        {
            var media = App.RootFrame.Media;
            if (media != null)
            {
                if (media.CurrentState == MediaElementState.Playing) 
                    return;

                media.Source = new Uri("ms-appx:///Resources/Carefree.mp3", UriKind.RelativeOrAbsolute);
                media.MediaEnded += MediaEnded;
                media.Play();
            }
        }

        private static void MediaEnded(object sender, RoutedEventArgs routedEventArgs)
        {
            if (!GameData.Current.MuteMusic)
                App.RootFrame.Media.Play();
        }

        public static void StopMusic()
        {
            if (App.RootFrame.Media != null)
            {
                App.RootFrame.Media.MediaEnded -= MediaEnded;
                App.RootFrame.Media.Stop();
            }
        }
    }
}
