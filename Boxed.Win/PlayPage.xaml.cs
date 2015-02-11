using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Boxed.Common;
using Boxed.DataModel;
using Boxed.ViewModels;
using Boxed.Win.Common;
using Brain.Animate;
using Brain.Utils;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using GoogleAnalytics;
using NotificationsExtensions.BadgeContent;
using Windows.UI.Notifications;
using NotificationsExtensions.TileContent;

// The Item Detail Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234232

namespace Boxed.Win
{
    /// <summary>
    /// A page that displays details for a single item within a group while allowing gestures to
    /// flip through other items belonging to the same group.
    /// </summary>
    public sealed partial class PlayPage : IRunnerView
    {
        private NavigationHelper navigationHelper;
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        public GameRunner Runner { get; set; }
        public bool IsBusy { get; set; }

        private DispatcherTimer _timer;
        private SquareControl _lastSelected;
        private bool _started;
        private bool _gridBuilt;


        public PlayPage()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += NavigationHelperOnSaveState;

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;

            Runner = new GameRunner(this);

            gameOverGrid.IsHitTestVisible = false;
            gameOverGrid.Opacity = 0;

            IsBusy = true;
        }

        private void PlayRandomNoise()
        {
            if (GameData.Current.MuteSounds) return;

            string url = "";
            switch (RandomManager.Next(3))
            {
                case 0: url = "ms-appx:///Resources/popHi.mp3"; break;
                case 1: url = "ms-appx:///Resources/popMed.mp3"; break;
                case 2: url = "ms-appx:///Resources/popLo.mp3"; break;
                default:
                    return;
            }

            Media.Source = new Uri(url, UriKind.RelativeOrAbsolute);
            Media.Play();
        }

        private void PlayPopNoise()
        {
            if (GameData.Current.MuteSounds) return;

            Media.Source = new Uri("ms-appx:///Resources/pop.mp3", UriKind.RelativeOrAbsolute);
            Media.Play();
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            GridImage.AnimateAsync(new FadeInLeftAnimation { Delay = 0.2});

            DataTransferManager.GetForCurrentView().DataRequested += OnDataRequested;
        }

        private void OnDataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            var data = args.Request.Data;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<p>Boxed is a totally awesome puzzle game in the Windows Store</p>");
            if (Runner.IsHighScore)
                sb.AppendFormat("<p>I just got a high score of {0} in {1}</p>",
                    Runner.PlayTime,
                    GameDefinition.GameSet.Name);

            sb.AppendLine("<br /><p><b>You should try it out</b></p>");
            sb.AppendLine("<p><a href='http://apps.microsoft.com/windows/app/boxed/032ce91d-899a-495e-914a-6c01d9e72915'>Boxed</a></p>");

            var html = HtmlFormatHelper.CreateHtmlFormat(sb.ToString());
            //data.SetText( sb.ToString());
            data.SetHtmlFormat(html);

            data.Properties.Title = "Boxed is Awesome";

        }

        private void OnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            DataTransferManager.GetForCurrentView().DataRequested -= OnDataRequested;
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            //AnimationHelper.AnimateBackgroundRainbow(App.RootFrame);

            base.OnNavigatingFrom(e);
        }

        private void NavigationHelperOnSaveState(object sender, SaveStateEventArgs args)
        {
            if (Runner.IsGameOver) return;

            Runner.SaveState(args.PageState);
        }

        private void navigationHelper_LoadState(object sender, LoadStateEventArgs args)
        {
            var key = (string)args.NavigationParameter as string;
            if (string.IsNullOrWhiteSpace(key)) return;

            var items = key.Split(',').ToArray();
            if (items.Length != 3) return;

            GameDefinition = GameManager.GetGameDefinition(
                new GameStartData
                {
                    PackName = items[0],
                    SetName = items[1],
                    Index = Convert.ToInt32(items[2])
                });

            if (GameDefinition == null) return;

            Runner.SetGameDefinition(GameDefinition);

            //AnimationHelper.AnimateBackgroundDark(App.RootFrame);

            if (args.PageState != null)
            {
                Runner.LoadState(args.PageState);
            }
            else
                EasyTracker.GetTracker().SendView("Play");


            StartTimer();
        }

        private void StartTimer()
        {
            StopTimer();

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(0.25) };
            _timer.Tick += async (sender, o) =>
            {
                if (!_gridBuilt)
                {
                    _gridBuilt = true;
                    GameBorder.Visibility = Visibility.Visible;
                    await BuildGrid();

                    IsBusy = false;

                    Runner.StartGame();
                }
                else
                {
                    if (IsBusy) return;

                    Runner.Tick();
                }
            };
            _timer.Start();
        }

        private void StopTimer()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer = null;
            }

            Runner.Tick();      // Last update
        }


        protected override void OnPointerPressed(PointerRoutedEventArgs e)
        {
            SquareControl squareControl = FindSquareControl(e.GetCurrentPoint(this).Position);
            if (squareControl != null)
            {
                _lastSelected = squareControl;
                var x = Grid.GetColumn(squareControl);
                var y = Grid.GetRow(squareControl);
                _started = Runner.TouchStart(x, y);

                GameBorder.CapturePointer(e.Pointer);
            }

            base.OnPointerPressed(e);
        }

        protected override void OnPointerReleased(PointerRoutedEventArgs e)
        {
            if (!_started) return;
            _started = false;

            SquareControl squareControl = FindSquareControl(e.GetCurrentPoint(this).Position);
            if (squareControl == null)
                squareControl = _lastSelected;

            if (squareControl != null)
            {
                var x = Grid.GetColumn(squareControl);
                var y = Grid.GetRow(squareControl);
                bool cleared;
                if (Runner.TouchFinish(x, y, out cleared))
                {
                    if (!cleared)
                        PlayRandomNoise();
                }
                if (cleared)
                {
                    PlayPopNoise();
                }
            }
            _lastSelected = null;

            base.OnPointerReleased(e);
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);

            if (!_started) return;
            _started = false;

            var squareControl = _lastSelected;
            if (squareControl != null)
            {
                var x = Grid.GetColumn(squareControl);
                var y = Grid.GetRow(squareControl);
                bool cleared;
                if (Runner.TouchFinish(x, y, out cleared))
                {
                    if (!cleared)
                        PlayRandomNoise();
                }
                if (cleared)
                {
                    PlayPopNoise();
                }
            }
            _lastSelected = null;
        }

        protected override void OnPointerMoved(PointerRoutedEventArgs e)
        {
            if (!_started) return;

            var squareControl = FindSquareControl(e.GetCurrentPoint(this).Position);
            if (squareControl != null)
            {
                var x = Grid.GetColumn(squareControl);
                var y = Grid.GetRow(squareControl);
                if (Runner.TouchCell(x, y))
                    _lastSelected = squareControl;
            }

            base.OnPointerMoved(e);
        }


        /*****************************************************************************/


        private SquareControl FindSquareControl(Point position)
        {
            var elements = VisualTreeHelper.FindElementsInHostCoordinates(position, this);
            return elements.OfType<SquareControl>().FirstOrDefault();
        }

        /*****************************************************************************/

        private void UpdateLiveTiles()
        {
            var squareTile = TileContentFactory.CreateTileSquare150x150PeekImageAndText01();
            squareTile.Image.Src = "ms-appx:///Assets/Logo150.png";
            squareTile.TextHeading.Text = Runner.PlayTime;
            squareTile.TextBody1.Text = GameDefinition.GameSet.Name;
            squareTile.TextBody2.Text = Runner.Level;
            squareTile.Branding = TileBranding.None;

            var wideTile = TileContentFactory.CreateTileWide310x150PeekImage02();
            wideTile.Image.Src = "ms-appx:///Assets/WideLogo310.png";
            wideTile.TextHeading.Text = Runner.PlayTime;
            wideTile.TextBody1.Text = GameDefinition.GameSet.Name;
            wideTile.TextBody2.Text = Runner.Level;
            wideTile.Branding = TileBranding.None;

            wideTile.Square150x150Content = squareTile;

            var tileUpdater = TileUpdateManager.CreateTileUpdaterForApplication();
            if (tileUpdater != null)
                tileUpdater.Update(wideTile.CreateNotification());
        }

        public void OnGameOver()
        {
            StopTimer();

            if (Runner.IsHighScore)
            {
                //TODO: Play HiScore sound
                PlayPopNoise();
            }
            else
            {
                // TODO: Play Winning Sound
                PlayPopNoise();
            }
            UpdateLiveTiles();

            gameOverGrid.IsHitTestVisible = true;
            gameOverGrid.AnimateAsync(new FadeInDownAnimation());

            AnimationHelper.AnimateForegroundLightRainbow(HighScoreText);
        }

        private async void CloseBoard(Action completedAction)
        {
            if (Runner.IsGameOver)
            {
                await gameOverGrid.AnimateAsync(new FadeOutUpAnimation());
                AnimationManager.ClearAnimationProperties(gameOverGrid);
                gameOverGrid.Opacity = 0;
                gameOverGrid.IsHitTestVisible = false;
            }

            int option = RandomManager.Next(10);
            GameBorder.BorderThickness = new Thickness(0);

            var tasks = new List<Task>();

            foreach (var squareControl in ShikakuGrid.Children.OfType<SquareControl>())
            {
                var x = Grid.GetColumn(squareControl);
                var y = Grid.GetRow(squareControl);
                var multiplier = 0.1;
                if (Runner.Definition.Width > 7)
                    multiplier = 0.04;
                else if (Runner.Definition.Width > 5)
                    multiplier = 0.06;
                var delay = (multiplier * x) + (multiplier * y);

                AnimationDefinition animation = null;
                switch (option)
                {
                    case 0: animation = new BounceOutUpAnimation { Delay = delay }; break;
                    case 1: animation = new BounceOutDownAnimation { Delay = delay }; break;
                    case 2: animation = new BounceOutLeftAnimation { Delay = delay }; break;
                    case 3: animation = new BounceOutRightAnimation { Delay = delay }; break;
                    case 4: animation = new FlipOutXAnimation { Delay = delay }; break;
                    case 5: animation = new FlipOutYAnimation { Delay = delay }; break;
                    case 6: animation = new FadeOutUpAnimation { Delay = delay }; break;
                    case 7: animation = new FadeOutDownAnimation { Delay = delay }; break;
                    case 8: animation = new FadeOutLeftAnimation { Delay = delay }; break;
                    case 9: animation = new FadeOutRightAnimation { Delay = delay }; break;
                }

                tasks.Add(squareControl.AnimateAsync(animation));

            }

            await Task.WhenAll(tasks);

            completedAction();
        }

        private async void NextGame()
        {
            GameBorder.BorderThickness = new Thickness(3);
            Runner.Next();
            await BuildGrid();

            StartTimer();
        }

        private async void ResetGame()
        {
            GameBorder.BorderThickness = new Thickness(3);
            Runner.Reset();
            await BuildGrid();
        }

        private async Task BuildGrid()
        {
            var tasks = new List<Task>();

            ShikakuGrid.Children.Clear();
            ShikakuGrid.ColumnDefinitions.Clear();
            ShikakuGrid.RowDefinitions.Clear();

            var width = Runner.Definition.Width;
            var height = Runner.Definition.Height;

            for (int i = 0; i < width; i++)
                ShikakuGrid.ColumnDefinitions.Add(new ColumnDefinition());
            for (int i = 0; i < height; i++)
                ShikakuGrid.RowDefinitions.Add(new RowDefinition());

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var squareControl = new SquareControl { DataContext = Runner.Grid[x, y] };
                    Runner.Grid[x, y].View = squareControl;

                    Grid.SetColumn(squareControl, x);
                    Grid.SetRow(squareControl, y);

                    var binding = new Binding();
                    binding.ElementName = "titleGrid";
                    binding.Path = new PropertyPath("Background");
                    squareControl.SetBinding(Control.BackgroundProperty, binding);

                    ShikakuGrid.Children.Add(squareControl);

                    //tasks.Add(squareControl.AnimateIn());
                }
            }

            ShikakuGrid.SizeChanged += ShikakuGridOnSizeChanged;

            await Task.WhenAll(tasks);
        }

        private void ShikakuGridOnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if ((ShikakuGrid.RowDefinitions.Count > 0) &&
                (ShikakuGrid.ColumnDefinitions.Count > 0))
            {
                var rowHeight = ShikakuGrid.RowDefinitions[0].ActualHeight;
                var colWidth = ShikakuGrid.ColumnDefinitions[0].ActualWidth;

                var len = Math.Min(rowHeight, colWidth);

                foreach (var rowDefinition in ShikakuGrid.RowDefinitions)
                {
                    if (Math.Abs(rowDefinition.Height.Value - len) > Double.Epsilon)
                        rowDefinition.Height = new GridLength(len);
                }
            }
        }

        private void NextButton_OnClick(object sender, RoutedEventArgs e)
        {
            CloseBoard(NextGame);
        }

        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            CloseBoard(() => Frame.GoBack());
        }

        private void RefreshButton_OnClick(object sender, RoutedEventArgs e)
        {
            CloseBoard(ResetGame);
        }


        private void AppBarButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            TopAppBar.IsOpen = false;
            new HowToPage().Show();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        private bool navigatingBack;
        private async void GoBack_OnClick(object sender, RoutedEventArgs e)
        {
            if (navigatingBack) return;
            navigatingBack = true;

            var tasks = new List<Task>();
            tasks.Add(backButton.AnimateAsync(new BounceOutLeftAnimation()));
            tasks.Add(titleGrid.AnimateAsync(new FadeOutAnimation()));
            tasks.Add(GridImage.AnimateAsync(new FadeOutRightAnimation()));

            await Task.WhenAll(tasks);

            navigatingBack = false;
            this.Frame.GoBack();
        }

        public GameDefinition GameDefinition { get; set; }

        private void ShareButton_OnClick(object sender, RoutedEventArgs e)
        {
            DataTransferManager.ShowShareUI();
        }
    }
}
