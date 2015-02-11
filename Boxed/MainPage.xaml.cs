using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Store;
using Windows.Foundation;
using Windows.Phone.UI.Input;
using Windows.System;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Boxed.Common;
using Boxed.Controls;
using Boxed.DataModel;
using Boxed.ViewModels;
using Brain.Animate;
using Brain.Animate.Extensions;
using PropertyChanged;


namespace Boxed
{
    [ImplementPropertyChanged]
    public sealed partial class MainPage
    {
        public List<GamePack> Packs
        {
            get { return GameManager.Current.GamePacks; }
        }

        public List<GameSet> AllGameSets
        {
            get { return GameManager.Current.AllGameSets; }
        }

        public GameSetVM SelectedGameSet { get; set; }
        public GameVM SelectedGame { get; set; }
        private bool initialised;

        public bool IsMusicEnabled
        {
            get { return !GameData.Current.MuteMusic; }
            set
            {
                GameData.Current.MuteMusic = !value;
                GameData.Current.SaveData();
                if (value)
                    StartMusic();
                else
                    StopMusic();
            }
        }

        private void StartMusic()
        {
            if (App.RootFrame.Media != null)
            {
                App.RootFrame.Media.Source = new Uri("ms-appx:///Resources/Carefree.mp3", UriKind.RelativeOrAbsolute);
                App.RootFrame.Media.MediaEnded += (sender, args) =>
                {
                    if (IsMusicEnabled)
                        StartMusic();
                };
                App.RootFrame.Media.Play();
            }
        }

        private void StopMusic()
        {
            if (App.RootFrame.Media != null)
                App.RootFrame.Media.Stop();
        }

        public MainPage()
        {
            InitializeComponent();

            NavigationCacheMode = NavigationCacheMode.Required;

            //AnimationHelper.AnimateForegroundLightRainbow(Pivot1Header);
            //AnimationHelper.AnimateForegroundLightRainbow(Pivot2Header);
            //AnimationHelper.AnimateForegroundLightRainbow(Pivot3Header);

            HardwareButtons.BackPressed += HardwareButtons_BackPressed;


            Loaded += MainPage_Loaded;
            Unloaded += (sender, args) =>
            {
                Window.Current.VisibilityChanged -= CurrentOnVisibilityChanged; 
            };
        }

        async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (initialised) return;
            initialised = true;

            int totalDelay = 0;
            while (totalDelay < 1800)
            {
                await Task.Delay(100);
                if (quickStart) break;
                totalDelay += 100;
            }

            if (IsMusicEnabled)
                StartMusic();

            //AnimationHelper.AnimateBackgroundRainbow(pivotGrid);
            AnimationHelper.AnimateBackgroundRainbow(gameSetGrid);
            await splashImage.AnimateAsync(new BounceOutAnimation { Delay = 0.0, Duration = 0.2, ToDirection = ZDirection.Away });
            await pivot.AnimateAsync(new FadeInAnimation());
            splashImage.IsHitTestVisible = false;

            Window.Current.VisibilityChanged += CurrentOnVisibilityChanged; 
        }

        private void CurrentOnVisibilityChanged(object sender, VisibilityChangedEventArgs args)
        {
            if (args.Visible)
                AnimationHelper.AnimateBackgroundRainbow(gameSetGrid);
        }

        private async void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            if (e.Handled) return;

            if (SelectedGameSet != null)
            {
                e.Handled = true;
                await pivotGrid.MoveToAsync(0.4, new Point(0, 0), new CircleEase {EasingMode = EasingMode.EaseOut});
                SelectedGameSet = null;

                gameSetList.Opacity = 1;
                await gameSetList.AnimateItems(new FadeInAnimation { DistanceX = 0, DistanceY = -50 });
                return;
            }

            if (pivot.SelectedIndex != 1)
            {
                pivot.SelectedIndex = 1;
                e.Handled = true;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.New)
                pivot.SelectedIndex = 1;

            if (e.NavigationMode == NavigationMode.Back)
            {
                if (SelectedGameSet != null)
                {
                    foreach (var game in SelectedGameSet.Games)
                        game.UpdateHighScore();
                }
            }

            //AnimationHelper.AnimateBackgroundRainbow(gameSetGrid);
        }

        private async void OnTapToPlayClick(object sender, RoutedEventArgs e)
        {
            await brian.Do(BrianAction.Exit);

            if (!GameData.Current.SeenHowToPlay)
            {
                GameData.Current.SeenHowToPlay = true;
                await GameData.Current.SaveData();

                Frame.Navigate(typeof (HowToPlayPage));
                //return;
            }

            pivot.SelectedIndex = 2;
        }

        private bool animatingBrian;
        private int lastSelectedIndex = -1;
        private int thisSelectedIndex = -1;
        private async void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            lastSelectedIndex = thisSelectedIndex;
            thisSelectedIndex = pivot.SelectedIndex;
            if (pivot.SelectedIndex == 1)
            {
                animatingBrian = true;

                brian.Hide();
                await Task.Delay(200);
                await brian.Do(BrianAction.Entrance);

                if (animatingBrian) await Task.Delay(200);
                if (!animatingBrian) return;
                brian.Do(BrianAction.RotateRight);
                /*
                while (animatingBrian)
                {
                    await brian.Do(BrianAction.RandomWait);
                    await Task.Delay(50);
                }
                */
            }
            else
                animatingBrian = false;

            if (pivot.SelectedIndex == 2)
            {
                if ((gameSetList.Items == null) || (gameSetList.Items.Count == 0))
                {
                    gameSetList.ItemsSource = AllGameSets;

                    gameSetList.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        gameSetList.Opacity = 1;
                        if (lastSelectedIndex == 1)
                            gameSetList.AnimateItems(new FadeInAnimation {DistanceX = 50, DistanceY = 25, Delay = 0.2});
                        else
                            gameSetList.AnimateItems(new FadeInAnimation {DistanceX = -50, DistanceY = 25, Delay = 0.2});
                    });
                }
                else
                {
                    if (lastSelectedIndex == 1)
                        gameSetList.AnimateItems(new FadeInAnimation { DistanceX = 50, DistanceY = 25, Delay = 0.2 });
                    else
                        gameSetList.AnimateItems(new FadeInAnimation { DistanceX = -50, DistanceY = 25, Delay = 0.2 });
                    
                }
            }
            else
            {
                gameSetList.AnimateItems(new FadeOutAnimation { DistanceX = 0, DistanceY = -25, Delay = 0.01 });  // Hide them all
            }

            if (pivot.SelectedIndex == 0)
            {
                pivot1List.Opacity = 1;
                if (lastSelectedIndex == 1)
                    pivot1List.AnimateItems(new FadeInAnimation { DistanceX = -50, DistanceY = 25, Delay = 0.2 });
                else
                    pivot1List.AnimateItems(new FadeInAnimation { DistanceX = 50, DistanceY = 25, Delay = 0.2 });
            }
            else
            {
                pivot1List.AnimateItems(new FadeOutAnimation { DistanceX = 0, DistanceY = -25, Delay = 0.01 });  // Hide them all
                pivot1List.Opacity = 0;
            }
        }

        private void OnAboutButtonClick(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AboutPage));
        }

        private void OnRateUsButtonClick(object sender, RoutedEventArgs e)
        {
            Launcher.LaunchUriAsync(new Uri("ms-windows-store:reviewapp?appid=" + CurrentApp.AppId));
        }

        private async void OnOurAppsButtonClick(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("ms-windows-store:SEARCH?query=BrainOffline"));
        }

        private bool animatingGameSet;
        private async void GameSetButton_Clicked(object sender, RoutedEventArgs e)
        {
            if (animatingGameSet) return;
            animatingGameSet = true;

            var button = sender as Button;
            if (button == null) return;

            var gameSet = button.DataContext as GameSet;
            if (gameSet == null) return;

            gameSetList.AnimateItems(
                new BounceOutUpAnimation(), 0.05D, 
                gameSet, 
                new ScaleAnimation { EndScale = 1.0, Easing = new CircleEase { EasingMode = EasingMode.EaseIn }});
            await Task.Delay(100);
            animatingGameSet = false;

            SelectedGameSet = new GameSetVM(gameSet);

            var tasks = new List<Task>
            {
                gameSelectionGrid.AnimateItems(new BounceInUpAnimation(), 0.02),
                pivotGrid.MoveToAsync(1.4, new Point(0, -(pivotGrid.ActualHeight *1.2)), new BackEase {Amplitude = 0.4})
            };

            await Task.WhenAll(tasks);
            gameSetList.Opacity = 0;
            gameSetList.ClearItemAnimations();
        }

        private void Game_Click(object sender, ItemClickEventArgs e)
        {
            SelectedGame = e.ClickedItem as GameVM;
            if (SelectedGame == null) return;

            var gridView = sender as GridView;
            if (gridView != null)
            {
                /*
                var element = gridView.ItemContainerGenerator.ContainerFromItem(gameVm) as GridViewItem;
                if (element != null)
                    await element.AnimateAsync(new RotateAnimation());
                */
            }

            Frame.Navigate(typeof(PlayPage), SelectedGame.Definition);
        }

        private bool quickStart = false;
        //private Mutex startMutex = new Mutex();
        private void SplashImage_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            quickStart = true;
            //startMutex.ReleaseMutex();
        }

        private void OnHowToPlayClick(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof (HowToPlayPage));
        }

        private void ToggleMusicClick(object sender, RoutedEventArgs e)
        {
            IsMusicEnabled = !IsMusicEnabled;
        }
    }
}
