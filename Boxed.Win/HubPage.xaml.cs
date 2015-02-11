using System.Collections.ObjectModel;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using Boxed.DataModel;
using Boxed.Win.Common;
using Boxed.Win.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
using Boxed.Controls;

// The Hub Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=321224
using Brain.Animate;
using Brain.Utils;
using PropertyChanged;

namespace Boxed.Win
{
    [ImplementPropertyChanged]
    public sealed partial class HubPage : Page
    {
        private NavigationHelper navigationHelper;
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        public HubPage()
        {
            this.InitializeComponent();

            AllPacks = new ObservableCollection<GameSet>();

            AllPacks.Clear();
            foreach (var gameSet in GameManager.Current.AllGameSets)
                AllPacks.Add(gameSet);

            Loaded += OnLoaded;

            SizeChanged += OnSizeChanged;

            navigationHelper = new NavigationHelper(this);
            navigationHelper.LoadState += navigationHelper_LoadState;
            navigationHelper.SaveState += vavigationHelper_SaveState;
        }

        public bool IsTall { get; set; }

        private void OnSizeChanged(object sender, SizeChangedEventArgs args)
        {
            IsTall = (args.NewSize.Height > args.NewSize.Width) && (args.NewSize.Width <= 800);

            TitleGrid.HorizontalAlignment = IsTall
                ? Windows.UI.Xaml.HorizontalAlignment.Center
                : Windows.UI.Xaml.HorizontalAlignment.Left;
            TitleGrid.Margin = IsTall ? new Thickness(0, 0, 0, 0) : new Thickness(120, 0, 0, 0);
        }

        private bool animatingBrian;

        private async void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            if (!GameData.Current.MuteMusic)
                App.StartMusic();

            packGridView.ItemsSource = groupedItemsViewSource.View.CollectionGroups;

            GridImage.AnimateAsync(new FadeInLeftAnimation { Delay = 0.2 });

            animatingBrian = true;
            brian.Hide();
            await Task.Delay(800);
            await brian.Do(BrianAction.Entrance);

            if (animatingBrian) await Task.Delay(200);
            if (!animatingBrian) return;
            brian.Do(BrianAction.RotateRight);

        }

        private async void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        private void vavigationHelper_SaveState(object sender, SaveStateEventArgs saveStateEventArgs)
        {
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        private void AppBarButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            TopAppBar.IsOpen = false;
            new HowToPage().Show();
        }

        private bool animating = false;
        private async void Game_OnClicked(object sender, RoutedEventArgs e)
        {
            if (animating) return;
            animating = true;

            var frameworkElement = sender as Button;
            if (frameworkElement == null) return;

            var gameDefinition = frameworkElement.DataContext as GameDefinition;
            if (gameDefinition == null) return;

            Search.QueryText = string.Empty;

            brian.Do(BrianAction.ExitQuick);

            var tasks = new List<Task>();
            tasks.Add(frameworkElement.AnimateAsync(new JumpAnimation { Duration = 0.5 }));
            //tasks.Add(frameworkElement.AnimateAsync(new BounceOutAnimation()));
            tasks.Add(GridImage.AnimateAsync(new FadeOutRightAnimation()));
            if (IsTall)
                tasks.Add(TitleGrid.AnimateAsync(new BounceOutAnimation()));
            else
                tasks.Add(TitleGrid.AnimateAsync(new BounceOutLeftAnimation()));
            tasks.Add(pageRoot.AnimateAsync(new FadeOutAnimation {Delay = 0.4, Duration = 0.2}));
            await Task.WhenAll(tasks);

            var key = gameDefinition.GetKey();
            Frame.Navigate(typeof (PlayPage), key);

            animating = false;
        }

        public ObservableCollection<GameSet> AllPacks { get; set; }

        private void SearchQuerySubmitted(SearchBox sender, SearchBoxQuerySubmittedEventArgs args)
        {
            string queryText = args.QueryText;
            if (string.IsNullOrWhiteSpace(queryText)) return;

            var gameSet = GameManager.Current.AllGameSets.FirstOrDefault(gs => gs.Name == queryText);
            if (gameSet == null) return;

            itemGridView.ScrollIntoView(gameSet);
            tallItemGridView.ScrollIntoView(gameSet);
        }

        private void SearchSuggestionsRequested(SearchBox sender, SearchBoxSuggestionsRequestedEventArgs args)
        {
            string queryText = args.QueryText;
            if (!string.IsNullOrEmpty(queryText))
            {
                queryText = queryText.ToLower();

                var suggestionCollection = args.Request.SearchSuggestionCollection;
                foreach (var gameSet in GameManager.Current.AllGameSets)
                {
                    if (gameSet.Name.ToLower().Contains(queryText))
                        suggestionCollection.AppendQuerySuggestion(gameSet.Name);
                }
            }
        }
    }
}
