using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Boxed.Common;
using Boxed.DataModel;
using Boxed.ViewModels;
using Brain.Animate;
using Brain.Utils;
using GoogleAnalytics;
using PropertyChanged;


namespace Boxed
{
    [ImplementPropertyChanged]
    public sealed partial class PlayPage : IRunnerView
    {
        public GameRunner Runner { get; set; }
        public bool IsBusy { get; set; }

        private DispatcherTimer _timer;
        private SquareControl _lastSelected;
        private bool _started;
        private bool _gridBuilt;

        public PlayPage()
        {
            InitializeComponent();
            //NavigationCacheMode = NavigationCacheMode.Required;

            Runner = new GameRunner(this);

            gameOverGrid.IsHitTestVisible = false;
            gameOverGrid.Opacity = 0;

            IsBusy = true;

            AnimationHelper.AnimateBackgroundRainbow(titleGrid);

            Loaded += (sender, args) =>
            {
                Window.Current.VisibilityChanged += CurrentOnVisibilityChanged;
            };
            Unloaded += (sender, args) =>
            {
                Window.Current.VisibilityChanged -= CurrentOnVisibilityChanged;
            };
        }

        private void CurrentOnVisibilityChanged(object sender, VisibilityChangedEventArgs args)
        {
            if (args.Visible)
                AnimationHelper.AnimateBackgroundRainbow(titleGrid);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var gameDefinition = e.Parameter as GameDefinition;

            Runner.SetGameDefinition(gameDefinition);

            EasyTracker.GetTracker().SendView("Play");

            //BuildGrid();

            StartTimer();
        }

        private void StartTimer()
        {
            StopTimer();

            _timer = new DispatcherTimer {Interval = TimeSpan.FromSeconds(0.25)};
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
                    //if (!cleared)
                    // TODO: TAP sound
                }
                if (cleared)
                {
                    // TODO: Pop sound
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
                    //if (!cleared)
                    // TODO: TAP sound
                }
                if (cleared)
                {
                    // TODO: Pop sound
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

        public void OnGameOver()
        {
            StopTimer();

            // TODO: Win sound

            gameOverGrid.IsHitTestVisible = true;
            gameOverGrid.AnimateAsync(new FadeInDownAnimation());
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
                var delay = (multiplier*x) + (multiplier*y);

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
    }
}
